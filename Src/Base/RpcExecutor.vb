'==========================================================================
'
'  File:        RpcExecutor.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 远程调用执行器
'  Version:     2010.12.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Collections.Concurrent
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Streaming
Imports Firefly.Mapping

Friend Module RpcExecutorUtility
    Private Function GetTypeFriendlyName(ByVal Type As Type) As String
        If Type.IsArray Then
            Dim n = Type.GetArrayRank
            Dim ElementTypeName = GetTypeFriendlyName(Type.GetElementType)
            If n = 1 Then
                Return "ArrayOf" & ElementTypeName
            End If
            Return "Array" & n & "Of" & ElementTypeName
        End If
        If Type.IsGenericType Then
            Dim Name = Regex.Match(Type.Name, "^(?<Name>.*?)`.*$", RegexOptions.ExplicitCapture).Result("${Name}")
            Return Name & "Of" & String.Join("And", (From t In Type.GetGenericArguments() Select GetTypeFriendlyName(t)).ToArray)
        End If
        Return Type.Name
    End Function

    <Extension()> Public Function FromBytes(Of T)(ByVal This As ISerializer, ByVal Bytes As Byte()) As T
        Using s = StreamEx.Create()
            s.Write(Bytes)
            s.Position = 0
            Return This.Read(Of T)(s)
        End Using
    End Function
    <Extension()> Public Function ToBytes(Of T)(ByVal This As ISerializer, ByVal Value As T) As Byte()
        Using s = StreamEx.Create()
            This.Write(Value, s)
            s.Position = 0
            Return s.Read(s.Length)
        End Using
    End Function

    Private Sub PushPrimitiveTypeHash(ByVal t As Type, ByVal f As Action(Of Byte))
        For Each b In UTF8.GetBytes(GetTypeFriendlyName(t))
            f(b)
        Next
    End Sub
    Private Sub PushCollectionTypeHash(ByVal t As Type, ByVal f As Action(Of Byte))
        For Each b In UTF8.GetBytes(GetTypeFriendlyName(t))
            f(b)
        Next
    End Sub
    Private Sub PushRecordTypeHash(ByVal t As Type, ByVal m As FieldOrPropertyInfo(), ByVal f As Action(Of Byte))
        For Each b In UTF8.GetBytes(GetTypeFriendlyName(t))
            f(b)
        Next
        For Each fp In m
            For Each b In UTF8.GetBytes(fp.Member.Name)
                f(b)
            Next
            PushTypeHash(fp.Type, f)
        Next
    End Sub
    Private TypeHashGuard As New HashSet(Of Type)
    Private Sub PushTypeHash(ByVal t As Type, ByVal f As Action(Of Byte))
        If TypeHashGuard.Contains(t) Then Throw New InvalidOperationException("CircularReference: {0}".Formats(t.FullName))
        TypeHashGuard.Add(t)
        Try
            If t.IsProperCollectionType Then
                PushCollectionTypeHash(t, f)
            Else
                Dim iri = t.TryGetImmutableRecordInfo
                If iri IsNot Nothing Then
                    PushRecordTypeHash(t, iri.Members, f)
                Else
                    Dim mri = t.TryGetMutableRecordInfo
                    If mri IsNot Nothing Then
                        PushRecordTypeHash(t, mri.Members, f)
                    Else
                        PushPrimitiveTypeHash(t, f)
                    End If
                End If
            End If
        Finally
            TypeHashGuard.Remove(t)
        End Try
    End Sub
    Public Function GetTypeHash(ByVal t As Type) As Int32
        Dim c As New CRC32
        PushTypeHash(t, AddressOf c.PushData)
        Return c.GetCRC32()
    End Function

    Public Function GetEventParameters(ByVal ei As EventInfo) As ParameterInfo()
        Dim ht = ei.EventHandlerType
        Dim m = ht.GetMethod("Invoke")
        Return m.GetParameters()
    End Function
End Module

Public Class RpcExecutorMaster
    Implements IDisposable

    Private NumType As Int32
    Private TypeDict As New Dictionary(Of Int32, Type)
    Private TypeInvDict As New Dictionary(Of Type, Int32)
    Private TypeBindings As New Dictionary(Of Int32, TypeBinding)

    Private NumMethod As Int32
    Private MethodDict As New Dictionary(Of Int32, MethodInfo)
    Private MethodInvDict As New Dictionary(Of MethodInfo, Int32)
    Private EventDict As New Dictionary(Of Int32, EventInfo)

    Private Pipe As IMasterPipe
    Private s As ISerializer
    Private InterfaceType As Type
    Private EventParameterReceiverResolver As Func(Of EventInfo, Action(Of Integer, IParameterReader))

    Private MainThreadAsyncInvoker As Action(Of Action)
    Private CancellationTokenSource As New CancellationTokenSource
    Private CancellationToken As CancellationToken
    Private Task As Task
    Private ClientRequestExecute As Packet

    Public Sub New(ByVal Pipe As IMasterPipe, ByVal s As ISerializer, ByVal InterfaceType As Type, ByVal EventParameterReceiverResolver As Func(Of EventInfo, Action(Of Integer, IParameterReader)), ByVal MainThreadAsyncInvoker As Action(Of Action))
        If Not InterfaceType.IsInterface Then Throw New ArgumentException
        Me.Pipe = Pipe
        Me.s = s
        Me.InterfaceType = InterfaceType
        Me.EventParameterReceiverResolver = EventParameterReceiverResolver
        Me.MainThreadAsyncInvoker = MainThreadAsyncInvoker

        Me.CancellationTokenSource = New CancellationTokenSource
        Me.CancellationToken = CancellationTokenSource.Token
        Me.Task = New Task(AddressOf Listen)
        Me.Task.Start()
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        If Task IsNot Nothing Then
            CancellationTokenSource.Cancel()
            Try
                Task.Wait(1000)
            Catch
            End Try
            Task.Dispose()
            CancellationTokenSource.Dispose()
            CancellationTokenSource = Nothing
            CancellationToken = Nothing
            Task = Nothing
        End If
    End Sub

    Private Sub ProcessException(ByVal p As Packet)
        Dim RemoteException As Exception = Nothing
        Try
            Select Case p.Verb
                Case RpcVerb.Excetpion
                    RemoteException = New Exception(UTF16.GetString(p.Content))
                Case Else
                    Throw New NotSupportedException
            End Select
        Catch ex As Exception
            SendException(ExceptionInfo.GetExceptionInfo(ex))
        End Try
        If RemoteException IsNot Nothing Then
            Throw RemoteException
        End If
    End Sub
    Private Sub ProcessRequestExecute(ByVal p As Packet)
        Try
            Select Case p.Verb
                Case RpcVerb.RequestExecute
                    Dim cu As New ContentUnpacker(p.Content, s)
                    Dim eid = cu.ReadParameter(Of Int32)()
                    Dim ei = EventDict(eid)

                    Dim NumParameter = cu.ReadParameter(Of Int32)()
                    If NumParameter <> GetEventParameters(ei).Length Then Throw New InvalidDataException

                    EventParameterReceiverResolver(ei)(NumParameter, cu)

                    Dim cp As New ContentPacker(s)
                    cp.WriteParameter(0)

                    Pipe.Send(New Packet With {.Verb = RpcVerb.ResponseExecute, .Content = cp.Build()})
                Case RpcVerb.ResponseEvent
                Case Else
                    Throw New NotSupportedException
            End Select
        Catch ex As Exception
            SendException(ExceptionInfo.GetExceptionInfo(ex))
        End Try
    End Sub
    Private Sub ProcessMetaData(ByVal p As Packet)
        Try
            Select Case p.Verb
                Case RpcVerb.RequestTypeBinding
                    Dim tb = s.FromBytes(Of TypeBinding)(p.Content)
                    If tb.TypeId <> NumType Then Throw New InvalidDataException
                    TypeBindings.Add(tb.TypeId, tb)
                    NumType += 1
                    Dim cp As New ContentPacker(s)
                    Pipe.Send(New Packet With {.Verb = RpcVerb.ResponseTypeBinding, .Content = cp.Build()})
                Case RpcVerb.RequestMethodBinding
                    Dim mb = s.FromBytes(Of MethodBinding)(p.Content)
                    If mb.MethodId <> NumMethod Then Throw New InvalidDataException

                    Dim ei = InterfaceType.GetEvent(mb.MethodName)
                    If mb.TypeParamters.Length <> 0 Then Throw New InvalidDataException

                    Dim Parameters = GetEventParameters(ei)
                    If mb.Parameters.Length <> Parameters.Length Then Throw New InvalidDataException
                    If mb.ReturnValues.Length <> 0 Then Throw New InvalidDataException

                    For Each Pair In mb.Parameters.Zip(Parameters, Function(mbp, mp) New With {.tid = mbp, .t = mp.ParameterType})
                        Dim tid = Pair.tid
                        Dim t = Pair.t
                        If TypeDict.ContainsKey(tid) Then
                            If TypeDict(tid) IsNot t Then Throw New InvalidDataException
                        ElseIf TypeBindings.ContainsKey(tid) Then
                            Dim tb = TypeBindings(tid)
                            Dim h = GetTypeHash(t)
                            If tb.Hash <> h Then Throw New InvalidDataException
                            TypeDict.Add(tid, t)
                            TypeInvDict.Add(t, tid)
                            TypeBindings.Remove(tid)
                        Else
                            Throw New InvalidDataException
                        End If
                    Next

                    EventDict.Add(mb.MethodId, ei)

                    NumMethod += 1
                    Dim cp As New ContentPacker(s)
                    Pipe.Send(New Packet With {.Verb = RpcVerb.ResponseMethodBinding, .Content = cp.Build()})
                Case Else
                    Throw New NotSupportedException
            End Select
        Catch ex As Exception
            SendException(ExceptionInfo.GetExceptionInfo(ex))
        End Try
    End Sub
    Private Sub ProcessPacket(ByVal p As Packet)
        Select Case p.Verb.Kind
            Case RpcVerbKind.KindException
                ProcessException(p)
            Case RpcVerbKind.KindExecute
                ProcessRequestExecute(p)
            Case RpcVerbKind.KindMetaData
                ProcessMetaData(p)
            Case Else
                Throw New NotSupportedException
        End Select
    End Sub
    Private Function ReceivePacket(ByVal Verb As RpcVerb) As Packet
        While True
            Dim p = Pipe.Receive()
            If p.Verb = Verb Then Return p
            ProcessPacket(p)
        End While
        Throw New InvalidOperationException
    End Function

    Private Sub SendException(ByVal Message As String)
        Pipe.Send(New Packet With {.Verb = RpcVerb.Excetpion, .Content = UTF16.GetBytes(Message)})
    End Sub
    Public Sub SendRequestExecute(ByVal mi As MethodInfo, ByVal ParameterWrite As Action(Of Integer, IParameterWriter), ByVal ReturnValueReader As Action(Of Integer, IParameterReader))
        CancellationTokenSource.Cancel()
        Try
            Task.Wait()
        Catch
        End Try
        If ClientRequestExecute IsNot Nothing Then
            ProcessPacket(ClientRequestExecute)
            ClientRequestExecute = Nothing
            ReceivePacket(RpcVerb.ResponseEvent)
        End If

        Try
            Dim mid = GetMethodId(mi)

            Dim cp As New ContentPacker(s)
            cp.WriteParameter(mid)
            Dim NumParameter = mi.GetParameters().Length
            cp.WriteParameter(Of Int32)(NumParameter)
            ParameterWrite(NumParameter, cp)

            Pipe.Send(New Packet With {.Verb = RpcVerb.RequestExecute, .Content = cp.Build()})
            Dim p = ReceivePacket(RpcVerb.ResponseExecute)

            Dim cu As New ContentUnpacker(p.Content, s)
            Dim NumReturnValue = cu.ReadParameter(Of Int32)()
            ReturnValueReader(NumReturnValue, cu)
        Finally
            CancellationTokenSource = New CancellationTokenSource
            CancellationToken = CancellationTokenSource.Token
            Task = New Task(AddressOf Listen)
            Task.Start()
        End Try
    End Sub
    Private Sub SendMethod(ByVal mid As Int32, ByVal mi As MethodInfo)
        Dim ReturnValues As New List(Of Int32)
        Dim rv = mi.ReturnType
        If rv IsNot GetType(Void) Then
            ReturnValues.Add(GetTypeId(rv))
        End If
        Dim mb As New MethodBinding With {
            .MethodId = mid,
            .MethodName = mi.Name,
            .TypeParamters = mi.GetGenericArguments().Select(Function(ga) GetTypeId(ga)).ToArray(),
            .Parameters = mi.GetParameters().Select(Function(param) GetTypeId(param.ParameterType)).ToArray(),
            .ReturnValues = ReturnValues.ToArray()
        }
        Pipe.Send(New Packet With {.Verb = RpcVerb.RequestMethodBinding, .Content = s.ToBytes(mb)})
        ReceivePacket(RpcVerb.ResponseMethodBinding)
    End Sub

    Private Function GetTypeId(ByVal t As Type) As Int32
        If Not TypeInvDict.ContainsKey(t) Then
            Dim tid = NumType
            Dim Success = False
            Try
                TypeInvDict.Add(t, tid)
                TypeDict.Add(tid, t)
                NumType += 1

                Dim h As Int32 = GetTypeHash(t)
                Pipe.Send(New Packet With {.Verb = RpcVerb.RequestTypeBinding, .Content = s.ToBytes(New TypeBinding With {.TypeId = tid, .Hash = h})})
                ReceivePacket(RpcVerb.ResponseTypeBinding)

                Success = True
            Finally
                If Not Success Then
                    TypeDict.Remove(tid)
                    TypeInvDict.Remove(t)
                End If
            End Try
        End If
        Return TypeInvDict(t)
    End Function
    Private Function GetMethodId(ByVal mi As MethodInfo) As Int32
        If Not MethodInvDict.ContainsKey(mi) Then
            Dim mid = NumMethod
            Dim Success = False
            Try
                MethodInvDict.Add(mi, mid)
                MethodDict.Add(mid, mi)
                NumMethod += 1
                SendMethod(mid, mi)
                Success = True
            Finally
                If Not Success Then
                    MethodDict.Remove(mid)
                    MethodInvDict.Remove(mi)
                End If
            End Try
        End If
        Return MethodInvDict(mi)
    End Function

    Private Sub Listen()
        While True
            CancellationToken.ThrowIfCancellationRequested()
            Thread.Sleep(100)
            Pipe.Send(New Packet With {.Verb = RpcVerb.RequestEvent, .Content = New Byte() {}}, 1000)
            Dim p As Packet = Nothing
            Try
                p = Pipe.Receive(2000)
            Catch ex As Exception
                Throw
            End Try
            If p.Verb = RpcVerb.ResponseEvent Then Continue While

            If CancellationToken.IsCancellationRequested Then
                ClientRequestExecute = p
                Throw New OperationCanceledException
            End If

            Dim f =
                Sub()
                    Task.Wait()

                    ProcessPacket(p)

                    CancellationTokenSource = New CancellationTokenSource
                    CancellationToken = CancellationTokenSource.Token
                    Task = New Task(AddressOf Listen)
                    Task.Start()
                End Sub

            MainThreadAsyncInvoker(f)
            Exit While
        End While
    End Sub
End Class

Public Class RpcExecutorSlave
    Implements IDisposable

    Private NumType As Int32
    Private TypeDict As New Dictionary(Of Int32, Type)
    Private TypeInvDict As New Dictionary(Of Type, Int32)
    Private TypeBindings As New Dictionary(Of Int32, TypeBinding)

    Private NumMethod As Int32
    Private MethodDict As New Dictionary(Of Int32, EventInfo)
    Private MethodInvDict As New Dictionary(Of EventInfo, Int32)
    Private EventDict As New Dictionary(Of Int32, MethodInfo)

    Private Pipe As ISlavePipe
    Private s As ISerializer
    Private InterfaceType As Type
    Private MethodParameterReceiverResolver As Func(Of MethodInfo, Action(Of Integer, IParameterReader, Integer, IParameterWriter))

    Private MainThreadEventLoop As Action

    Public Sub New(ByVal Pipe As ISlavePipe, ByVal s As ISerializer, ByVal InterfaceType As Type, ByVal MethodParameterReceiverResolver As Func(Of MethodInfo, Action(Of Integer, IParameterReader, Integer, IParameterWriter)), ByVal MainThreadEventLoop As Action)
        If Not InterfaceType.IsInterface Then Throw New ArgumentException
        Me.Pipe = Pipe
        Me.s = s
        Me.InterfaceType = InterfaceType
        Me.MethodParameterReceiverResolver = MethodParameterReceiverResolver
        Me.MainThreadEventLoop = MainThreadEventLoop
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub

    Private Sub ProcessException(ByVal p As Packet)
        Dim LocalException As Exception = Nothing
        Try
            Select Case p.Verb
                Case RpcVerb.Excetpion
                    LocalException = New Exception(UTF16.GetString(p.Content))
                Case Else
                    Throw New NotSupportedException
            End Select
        Catch ex As Exception
            SendException(ExceptionInfo.GetExceptionInfo(ex))
        End Try
        If LocalException IsNot Nothing Then
            Throw LocalException
        End If
    End Sub
    Private RequestExecuteQueue As New BlockingCollection(Of KeyValuePair(Of EventInfo, Action(Of Integer, IParameterWriter)))
    Private Sub ProcessRequestExecute(ByVal p As Packet)
        Try
            Select Case p.Verb
                Case RpcVerb.RequestExecute
                    Dim cu As New ContentUnpacker(p.Content, s)
                    Dim mid = cu.ReadParameter(Of Int32)()
                    Dim mi = EventDict(mid)

                    Dim NumParameter = cu.ReadParameter(Of Int32)()
                    Dim NumReturnValue As Int32 = (Function(rv) IIf(rv Is GetType(Void), 0, 1))(mi.ReturnType)

                    Dim cp As New ContentPacker(s)
                    cp.WriteParameter(NumReturnValue)

                    MethodParameterReceiverResolver(mi)(NumParameter, cu, NumReturnValue, cp)

                    Pipe.Send(New Packet With {.Verb = RpcVerb.ResponseExecute, .Content = cp.Build()})
                Case RpcVerb.RequestEvent
                    MainThreadEventLoop()
                    While RequestExecuteQueue.Count > 0
                        Dim Pair = RequestExecuteQueue.Take()
                        SendRequestExecute(Pair.Key, Pair.Value)
                    End While
                    Dim cp As New ContentPacker(s)
                    Pipe.Send(New Packet With {.Verb = RpcVerb.ResponseEvent, .Content = cp.Build()})
                Case Else
                    Throw New NotSupportedException
            End Select
        Catch ex As Exception
            SendException(ExceptionInfo.GetExceptionInfo(ex))
        End Try
    End Sub
    Private Sub ProcessMetaData(ByVal p As Packet)
        Try
            Select Case p.Verb
                Case RpcVerb.RequestTypeBinding
                    Dim tb = s.FromBytes(Of TypeBinding)(p.Content)
                    If tb.TypeId <> NumType Then Throw New InvalidDataException
                    TypeBindings.Add(tb.TypeId, tb)
                    NumType += 1
                    Dim cp As New ContentPacker(s)
                    Pipe.Send(New Packet With {.Verb = RpcVerb.ResponseTypeBinding, .Content = cp.Build()})
                Case RpcVerb.RequestMethodBinding
                    Dim mb = s.FromBytes(Of MethodBinding)(p.Content)
                    If mb.MethodId <> NumMethod Then Throw New InvalidDataException

                    If mb.TypeParamters.Length <> 0 Then
                        '暂时不支持泛型方法
                        Throw New NotSupportedException("暂时不支持泛型方法")
                    End If

                    Dim ResultMethod As MethodInfo = Nothing
                    Dim mis = InterfaceType.GetMethods.Where(Function(m) m.Name = mb.MethodName)
                    For Each mi In mis
                        Dim TypeParameters As Type() = Nothing
                        If mi.IsGenericMethod Then
                            TypeParameters = mi.GetGenericArguments()
                            If TypeParameters.Length <> mb.TypeParamters.Length Then Continue For
                        End If
                        Dim Parameters = mi.GetParameters().Select(Function(param) param.ParameterType).ToArray()
                        If Parameters.Length <> mb.Parameters.Length Then Continue For
                        Dim ReturnValues = (Function(rv) IIf(Of Type())(rv Is GetType(Void), New Type() {}, New Type() {rv}))(mi.ReturnType)
                        If ReturnValues.Length <> mb.ReturnValues.Length Then Continue For

                        Dim Failure As Boolean = False
                        For Each Pair In mb.Parameters.Concat(mb.ReturnValues).Zip(Parameters.Concat(ReturnValues), Function(mbp, mp) New With {.tid = mbp, .t = mp})
                            Dim tid = Pair.tid
                            Dim t = Pair.t
                            If TypeDict.ContainsKey(tid) Then
                                If TypeDict(tid) IsNot t Then
                                    Failure = True
                                    Exit For
                                End If
                            ElseIf TypeBindings.ContainsKey(tid) Then
                                Dim tb = TypeBindings(tid)
                                Dim h = GetTypeHash(t)
                                If tb.Hash <> h Then
                                    Failure = True
                                    Exit For
                                End If
                                TypeDict.Add(tid, t)
                                TypeInvDict.Add(t, tid)
                                TypeBindings.Remove(tid)
                            Else
                                Failure = True
                                Exit For
                            End If
                        Next
                        If Failure Then Continue For
                        ResultMethod = mi
                    Next

                    If ResultMethod Is Nothing Then Throw New InvalidDataException

                    EventDict.Add(mb.MethodId, ResultMethod)

                    NumMethod += 1
                    Dim cp As New ContentPacker(s)
                    Pipe.Send(New Packet With {.Verb = RpcVerb.ResponseMethodBinding, .Content = cp.Build()})
                Case Else
                    Throw New NotSupportedException
            End Select
        Catch ex As Exception
            SendException(ExceptionInfo.GetExceptionInfo(ex))
        End Try
    End Sub
    Private Sub ProcessPacket(ByVal p As Packet)
        Select Case p.Verb.Kind
            Case RpcVerbKind.KindException
                ProcessException(p)
            Case RpcVerbKind.KindExecute
                ProcessRequestExecute(p)
            Case RpcVerbKind.KindMetaData
                ProcessMetaData(p)
            Case Else
                Throw New NotSupportedException
        End Select
    End Sub
    Private Function ReceivePacket(ByVal Verb As RpcVerb) As Packet
        While True
            Dim p = Pipe.Receive()
            If p.Verb = Verb Then Return p
            ProcessPacket(p)
        End While
        Throw New InvalidOperationException
    End Function

    Private Sub SendException(ByVal Message As String)
        Pipe.Send(New Packet With {.Verb = RpcVerb.Excetpion, .Content = UTF16.GetBytes(Message)})
    End Sub
    Private Sub SendRequestExecute(ByVal ei As EventInfo, ByVal ParameterWrite As Action(Of Integer, IParameterWriter))
        Dim mid = GetMethodId(ei)

        Dim cp As New ContentPacker(s)
        cp.WriteParameter(mid)
        Dim NumParameter = GetEventParameters(ei).Length
        cp.WriteParameter(Of Int32)(NumParameter)
        ParameterWrite(NumParameter, cp)

        Pipe.Send(New Packet With {.Verb = RpcVerb.RequestExecute, .Content = cp.Build()})
        Dim p = ReceivePacket(RpcVerb.ResponseExecute)

        Dim cu As New ContentUnpacker(p.Content, s)
        Dim NumReturnValue = cu.ReadParameter(Of Int32)()
        If NumReturnValue <> 0 Then
            SendException("NumReturnValueIsNotZero")
        End If
    End Sub
    Private Sub SendMethod(ByVal mid As Int32, ByVal ei As EventInfo)
        Dim mb As New MethodBinding With {
            .MethodId = mid,
            .MethodName = ei.Name,
            .TypeParamters = New Int32() {},
            .Parameters = GetEventParameters(ei).Select(Function(param) GetTypeId(param.ParameterType)).ToArray(),
            .ReturnValues = New Int32() {}
        }
        Pipe.Send(New Packet With {.Verb = RpcVerb.RequestMethodBinding, .Content = s.ToBytes(mb)})
        ReceivePacket(RpcVerb.ResponseMethodBinding)
    End Sub

    Private Function GetTypeId(ByVal t As Type) As Int32
        If Not TypeInvDict.ContainsKey(t) Then
            Dim tid = NumType
            Dim Success = False
            Try
                TypeInvDict.Add(t, tid)
                TypeDict.Add(tid, t)
                NumType += 1

                Dim h As Int32 = GetTypeHash(t)
                Pipe.Send(New Packet With {.Verb = RpcVerb.RequestTypeBinding, .Content = s.ToBytes(New TypeBinding With {.TypeId = tid, .Hash = h})})
                ReceivePacket(RpcVerb.ResponseTypeBinding)

                Success = True
            Finally
                If Not Success Then
                    TypeDict.Remove(tid)
                    TypeInvDict.Remove(t)
                End If
            End Try
        End If
        Return TypeInvDict(t)
    End Function
    Private Function GetMethodId(ByVal ei As EventInfo) As Int32
        If Not MethodInvDict.ContainsKey(ei) Then
            Dim mid = NumMethod
            Dim Success = False
            Try
                MethodInvDict.Add(ei, mid)
                MethodDict.Add(mid, ei)
                NumMethod += 1
                SendMethod(mid, ei)
                Success = True
            Finally
                If Not Success Then
                    MethodDict.Remove(mid)
                    MethodInvDict.Remove(ei)
                End If
            End Try
        End If
        Return MethodInvDict(ei)
    End Function

    Public Sub Listen()
        While True
            Dim p = Pipe.Receive()
            ProcessPacket(p)
        End While
    End Sub
    Public Sub SendRequestExecuteAsync(ByVal ei As EventInfo, ByVal ParameterWrite As Action(Of Integer, IParameterWriter))
        RequestExecuteQueue.Add(CreatePair(ei, ParameterWrite))
    End Sub
End Class
