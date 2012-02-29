'==========================================================================
'
'  File:        RpcVoice.vb
'  Location:    Eddy.Voice <Visual Basic .Net>
'  Description: 远程过程调用代理(具体)
'  Version:     2012.02.29.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Reflection
Imports System.Threading
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Streaming
Imports Firefly.Mapping
Imports Firefly.Mapping.Binary
Imports Eddy
Imports Eddy.Base
Imports System.Linq.Expressions
Imports System.Collections.Generic
Imports System.Linq

Public Class RpcVoice
    Private Sub New()
    End Sub

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

    Private Class ServiceProxy
        Implements IDisposable

        Public Pipe As PipeMaster
        Public Master As RpcExecutorMaster

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
    End Class

    Private Class VoiceServiceProxy
        Inherits ServiceProxy
        Implements IVoiceService

        Public m_IsVoiceInstalled_String As Func(Of String, Boolean)
        Public Function IsVoiceInstalled(ByVal VoiceName As String) As Boolean Implements IVoiceService.IsVoiceInstalled
            Return m_IsVoiceInstalled_String(VoiceName)
        End Function

        Public m_SpeakAsync_String_String As Action(Of String, String)
        Public Sub SpeakAsync(ByVal VoiceName As String, ByVal Text As String) Implements IVoiceService.SpeakAsync
            m_SpeakAsync_String_String(VoiceName, Text)
        End Sub

        Public m_SpeakAsync_String As Action(Of String)
        Public Sub SpeakAsync(ByVal Text As String) Implements IVoiceService.SpeakAsync
            m_SpeakAsync_String(Text)
        End Sub

        Public m_SpeakAsyncCancelAll As Action
        Public Sub SpeakAsyncCancelAll() Implements IVoiceService.SpeakAsyncCancelAll
            m_SpeakAsyncCancelAll()
        End Sub

        Public Sub OnSpeakStarted()
            RaiseEvent SpeakStarted()
        End Sub
        Public Sub OnSpeakCompleted()
            RaiseEvent SpeakCompleted()
        End Sub

        Public Event SpeakStarted() Implements IVoiceService.SpeakStarted
        Public Event SpeakCompleted() Implements IVoiceService.SpeakCompleted
    End Class

    Public Shared Function CreateMaster(Of T As IDisposable)(ByVal SlavePath As String, ByVal MainThreadAsyncInvoker As Action(Of Action)) As T
        Dim s As New BinarySerializer
        s.PutReaderTranslator(New StringTranslator)
        s.PutWriterTranslator(New StringTranslator)

        Dim Proxy As New VoiceServiceProxy

        Dim Pipe As PipeMaster = Nothing
        Dim Master As RpcExecutorMaster = Nothing
        Dim Success = False
        Try
            Pipe = New PipeMaster(SlavePath)

            Dim Dict As New Dictionary(Of EventInfo, MethodInfo)
            Dict.Add(GetType(IVoiceService).GetEvent("SpeakStarted"), GetType(VoiceServiceProxy).GetMethod("OnSpeakStarted"))
            Dict.Add(GetType(IVoiceService).GetEvent("SpeakCompleted"), GetType(VoiceServiceProxy).GetMethod("OnSpeakCompleted"))

            Dim EventParameterReceiverResolver =
                Function(ei As EventInfo) As Action(Of Integer, IParameterReader)
                    If ei.Name = "SpeakStarted" Then
                        Return Sub(NumParameter, r)
                                   If NumParameter <> 0 Then Throw New InvalidOperationException
                                   Proxy.OnSpeakStarted()
                               End Sub
                    End If
                    If ei.Name = "SpeakCompleted" Then
                        Return Sub(NumParameter, r)
                                   If NumParameter <> 0 Then Throw New InvalidOperationException
                                   Proxy.OnSpeakCompleted()
                               End Sub
                    End If
                    Throw New NotImplementedException
                End Function

            Master = New RpcExecutorMaster(Pipe, New SerializerAdapter(s), GetType(IVoiceService), EventParameterReceiverResolver, MainThreadAsyncInvoker)
            Success = True
        Finally
            If Not Success Then
                If Pipe IsNot Nothing Then
                    Pipe.Dispose()
                    Pipe = Nothing
                End If
                If Master IsNot Nothing Then
                    Master.Dispose()
                    Master = Nothing
                End If
            End If
        End Try

        Proxy.Pipe = Pipe
        Proxy.Master = Master

        Proxy.m_IsVoiceInstalled_String =
            Function(VoiceName As String) As Boolean
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

        Proxy.m_SpeakAsync_String_String =
            Sub(VoiceName As String, Text As String)
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

        Proxy.m_SpeakAsync_String =
            Sub(Text As String)
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

        Proxy.m_SpeakAsyncCancelAll =
            Sub()
                Master.SendRequestExecute(GetType(IVoiceService).GetMethod("SpeakAsyncCancelAll"),
                    Sub(NumParameter, w)
                        If NumParameter <> 0 Then Throw New InvalidOperationException
                    End Sub,
                    Sub(NumReturnValue, r)
                        If NumReturnValue <> 0 Then Throw New InvalidOperationException
                    End Sub
                )
            End Sub

        Return DirectCast(DirectCast(Proxy, Object), T)
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
