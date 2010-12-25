Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Diagnostics
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Threading
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Streaming
Imports Firefly.Mapping
Imports Eddy
Imports Eddy.Base

Public Class Rpc
    Private Sub New()
    End Sub

    Private Shared Function CreateMasterType(Of T As IDisposable)(ByVal SlavePath As String) As Type
        Throw New InvalidOperationException

        Dim ServiceInterfaceType = GetType(T)
        If Not ServiceInterfaceType.IsInterface Then Throw New ArgumentException("服务类型必须为接口")
        If ServiceInterfaceType.IsGenericTypeDefinition Then Throw New ArgumentException("服务类型必须是具体类型，泛型类型应先具体化")

        Dim an As New AssemblyName("IpcDynamicAssembly")
        Dim ab = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect)
        Dim mob = ab.DefineDynamicModule(an.Name)
        Dim tb = mob.DefineType("IpcServiceProxy", TypeAttributes.Public Or TypeAttributes.Class, GetType(Object), {GetType(T)})

        Dim fbIpcMaster = tb.DefineField("IpcMaster", GetType(PipeMaster), FieldAttributes.Private)

        Dim cb = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, {GetType(PipeMaster)})
        Dim cig = cb.GetILGenerator
        cig.Emit(OpCodes.Ldarg_0)
        cig.Emit(OpCodes.Ldarg_1)
        cig.Emit(OpCodes.Stfld, fbIpcMaster)


        For Each m In ServiceInterfaceType.GetMethods
            Dim Parameters = m.GetParameters()
            Dim mb = tb.DefineMethod(m.Name, m.Attributes, m.CallingConvention, m.ReturnType, Parameters.Select(Function(p) p.ParameterType).ToArray)
            If m.IsGenericMethodDefinition Then
                Dim GenericParameters = m.GetGenericArguments()
                Dim gpbs = mb.DefineGenericParameters(GenericParameters.Select(Function(gp) gp.Name).ToArray())
                For Each Pair In gpbs.Zip(GenericParameters, Function(b, gp) New With {.GenericParameter = gp, .Builder = b})
                    Dim Constraints = Pair.GenericParameter.GetGenericParameterConstraints()
                    Pair.Builder.SetGenericParameterAttributes(Pair.GenericParameter.GenericParameterAttributes)
                    Dim InterfaceConstraints = Constraints.Where(Function(c) c.IsInterface).ToArray()
                    Dim BaseConstraints = Constraints.Except(InterfaceConstraints).ToArray()
                    If BaseConstraints.Length > 0 Then
                        Pair.Builder.SetBaseTypeConstraint(BaseConstraints.Single)
                    End If
                    If InterfaceConstraints.Length > 0 Then
                        Pair.Builder.SetInterfaceConstraints(InterfaceConstraints)
                    End If
                Next
            End If


        Next
    End Function

    Private Class StringTranslator
        Implements IProjectorToProjectorDomainTranslator(Of String, Byte())
        Implements IProjectorToProjectorRangeTranslator(Of String, Byte())

        Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of Byte(), R)) As Func(Of String, R) Implements IProjectorToProjectorDomainTranslator(Of String, Byte()).TranslateProjectorToProjectorDomain
            Return Function(s) Projector(UTF8.GetBytes(s))
        End Function
        Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, Byte())) As Func(Of D, String) Implements IProjectorToProjectorRangeTranslator(Of String, Byte()).TranslateProjectorToProjectorRange
            Return Function(Domain) UTF8.GetString(Projector(Domain))
        End Function
    End Class

    Private Class SerializerAdapter
        Implements ISerializer

        Private bs As BinarySerializer
        Public Sub New(ByVal bs As BinarySerializer)
            Me.bs = bs
        End Sub

        Public Function Read(Of T)(ByVal s As IReadableStream) As T Implements ISerializer.Read
            Return bs.Read(Of T)(s)
        End Function

        Public Sub Write(Of T)(ByVal Value As T, ByVal s As IWritableStream) Implements ISerializer.Write
            bs.Write(Value, s)
        End Sub
    End Class

    Private NotInheritable Class VoiceServiceProxy
        Implements IVoiceService

        Private Pipe As PipeMaster
        Private Master As RpcExecutorMaster
        Public Sub New(ByVal SlavePath As String, ByVal s As ISerializer, ByVal MainThreadAsyncInvoker As Action(Of Action))
            Dim Success = False
            Try
                Me.Pipe = New PipeMaster(SlavePath)

                Dim EventParameterReceiverResolver =
                    Function(ei As EventInfo) As Action(Of Integer, IParameterReader)
                        If ei.Name = "SpeakStarted" Then
                            Return Sub(NumParameter, r)
                                       If NumParameter <> 0 Then Throw New InvalidOperationException
                                       RaiseEvent SpeakStarted()
                                   End Sub
                        End If
                        If ei.Name = "SpeakCompleted" Then
                            Return Sub(NumParameter, r)
                                       If NumParameter <> 0 Then Throw New InvalidOperationException
                                       RaiseEvent SpeakCompleted()
                                   End Sub
                        End If
                        Throw New NotImplementedException
                    End Function

                Me.Master = New RpcExecutorMaster(Pipe, s, GetType(IVoiceService), EventParameterReceiverResolver, MainThreadAsyncInvoker)
                Success = True
            Finally
                If Not Success Then
                    Dispose()
                End If
            End Try
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If Pipe IsNot Nothing Then
                Pipe.Dispose()
                Pipe = Nothing
            End If
            If Master IsNot Nothing Then
                Master.Dispose()
                Master = Nothing
            End If
        End Sub

        Public Function IsVoiceInstalled(ByVal VoiceName As String) As Boolean Implements IVoiceService.IsVoiceInstalled
            Dim b As Boolean = False
            Master.SendRequestExecute(GetType(IVoiceService).GetMethod("IsVoiceInstalled"),
                Sub(NumParameter, w)
                    If NumParameter <> 1 Then Throw New InvalidOperationException
                    w.WriteParameter(VoiceName)
                End Sub,
                Sub(NumReturnValue, r)
                    If NumReturnValue <> 1 Then Throw New InvalidOperationException
                    b = r.ReadParameter(Of Boolean)()
                End Sub
            )
            Return b
        End Function

        Public Sub SpeakAsync(ByVal VoiceName As String, ByVal Text As String) Implements IVoiceService.SpeakAsync
            Master.SendRequestExecute(GetType(IVoiceService).GetMethod("SpeakAsync", {GetType(String), GetType(String)}),
                Sub(NumParameter, w)
                    If NumParameter <> 2 Then Throw New InvalidOperationException
                    w.WriteParameter(VoiceName)
                    w.WriteParameter(Text)
                End Sub,
                Sub(NumReturnValue, r)
                    If NumReturnValue <> 0 Then Throw New InvalidOperationException
                End Sub
            )
        End Sub

        Public Sub SpeakAsync(ByVal Text As String) Implements IVoiceService.SpeakAsync
            Master.SendRequestExecute(GetType(IVoiceService).GetMethod("SpeakAsync", {GetType(String)}),
                Sub(NumParameter, w)
                    If NumParameter <> 1 Then Throw New InvalidOperationException
                    w.WriteParameter(Text)
                End Sub,
                Sub(NumReturnValue, r)
                    If NumReturnValue <> 0 Then Throw New InvalidOperationException
                End Sub
            )
        End Sub

        Public Sub SpeakAsyncCancelAll() Implements IVoiceService.SpeakAsyncCancelAll
            Master.SendRequestExecute(GetType(IVoiceService).GetMethod("SpeakAsyncCancelAll"),
                Sub(NumParameter, w)
                    If NumParameter <> 0 Then Throw New InvalidOperationException
                End Sub,
                Sub(NumReturnValue, r)
                    If NumReturnValue <> 0 Then Throw New InvalidOperationException
                End Sub
            )
        End Sub

        Public Event SpeakCompleted() Implements IVoiceService.SpeakCompleted

        Public Event SpeakStarted() Implements IVoiceService.SpeakStarted
    End Class

    Public Shared Function CreateMaster(Of T As IDisposable)(ByVal SlavePath As String, ByVal MainThreadAsyncInvoker As Action(Of Action)) As T
        Dim s As New BinarySerializer
        s.PutReaderTranslator(New StringTranslator)
        s.PutWriterTranslator(New StringTranslator)

        Return DirectCast(DirectCast(New VoiceServiceProxy(SlavePath, New SerializerAdapter(s), MainThreadAsyncInvoker), Object), T)
    End Function

    Public Shared Sub ListenOnSlave(Of T As IDisposable, C As T)(ByVal PipeIn As String, ByVal PipeOut As String, ByVal ConcreteService As C)
        Dim s As New BinarySerializer
        s.PutReaderTranslator(New StringTranslator)
        s.PutWriterTranslator(New StringTranslator)

        Using EventPump As New ManualResetEvent(False)
            Dim MainThreadEventLoop =
                Sub()
                    EventPump.WaitOne(100)
                    EventPump.Reset()
                End Sub

            Using Pipe As New PipeSlave(PipeIn, PipeOut)
                Dim Service = DirectCast(ConcreteService, IVoiceService)

                Dim MethodParameterReceiverResolver =
                    Function(mi As MethodInfo) As Action(Of Integer, IParameterReader, Integer, IParameterWriter)
                        If mi.Name = "IsVoiceInstalled" Then
                            Return Sub(NumParameter, r, NumReturnValue, w)
                                       If NumParameter <> 1 Then Throw New InvalidOperationException
                                       If NumReturnValue <> 1 Then Throw New InvalidOperationException
                                       Dim VoiceName = r.ReadParameter(Of String)()
                                       Dim b = Service.IsVoiceInstalled(VoiceName)
                                       w.WriteParameter(b)
                                   End Sub
                        End If
                        If mi.Name = "SpeakAsync" AndAlso mi.GetParameters().Length = 2 Then
                            Return Sub(NumParameter, r, NumReturnValue, w)
                                       If NumParameter <> 2 Then Throw New InvalidOperationException
                                       If NumReturnValue <> 0 Then Throw New InvalidOperationException
                                       Dim VoiceName = r.ReadParameter(Of String)()
                                       Dim Text = r.ReadParameter(Of String)()
                                       Service.SpeakAsync(VoiceName, Text)
                                   End Sub
                        End If
                        If mi.Name = "SpeakAsync" AndAlso mi.GetParameters().Length = 1 Then
                            Return Sub(NumParameter, r, NumReturnValue, w)
                                       If NumParameter <> 1 Then Throw New InvalidOperationException
                                       If NumReturnValue <> 0 Then Throw New InvalidOperationException
                                       Dim Text = r.ReadParameter(Of String)()
                                       Service.SpeakAsync(Text)
                                   End Sub
                        End If
                        If mi.Name = "SpeakAsyncCancelAll" Then
                            Return Sub(NumParameter, r, NumReturnValue, w)
                                       If NumParameter <> 0 Then Throw New InvalidOperationException
                                       If NumReturnValue <> 0 Then Throw New InvalidOperationException
                                       Service.SpeakAsyncCancelAll()
                                   End Sub
                        End If
                        Throw New NotImplementedException
                    End Function

                Dim RpcSlave As New RpcExecutorSlave(Pipe, New SerializerAdapter(s), GetType(T), MethodParameterReceiverResolver, MainThreadEventLoop)

                AddHandler Service.SpeakStarted,
                    Sub()
                        RpcSlave.SendRequestExecuteAsync(GetType(IVoiceService).GetEvent("SpeakStarted"),
                            Sub(NumParameter, w)
                                If NumParameter <> 0 Then Throw New InvalidOperationException
                            End Sub
                        )
                    End Sub

                AddHandler Service.SpeakCompleted,
                    Sub()
                        RpcSlave.SendRequestExecuteAsync(GetType(IVoiceService).GetEvent("SpeakCompleted"),
                            Sub(NumParameter, w)
                                If NumParameter <> 0 Then Throw New InvalidOperationException
                            End Sub
                        )
                    End Sub

                RpcSlave.Listen()
            End Using
        End Using
    End Sub
End Class
