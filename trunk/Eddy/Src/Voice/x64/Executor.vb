Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Streaming
Imports Firefly.Mapping

Public Class ExecutorMaster
    Implements IDisposable

    Private Pipe As IPipe
    Private s As ISerializer
    Private MainThreadInvoker As Action(Of Action)
    Private InterfaceType As Type
    Private Task As Task

    Public Sub New(ByVal Pipe As IPipe, ByVal s As ISerializer, ByVal MainThreadInvoker As Action(Of Action), ByVal InterfaceType As Type)
        If Not InterfaceType.IsInterface Then Throw New ArgumentException
        Me.Pipe = Pipe
        Me.s = s
        Me.MainThreadInvoker = MainThreadInvoker
        Me.InterfaceType = InterfaceType
        Me.Task = New Task(AddressOf Listen)
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        If Pipe IsNot Nothing Then
            Pipe.Dispose()
            Pipe = Nothing
        End If
    End Sub

    Private Function ReceivePacket(ByVal Verb As RpcVerb) As Packet
        Dim p = Pipe.Receive()
        If p.Verb = Verb Then Return p
        While True
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
        End While
        Throw New InvalidOperationException
    End Function

    Private Sub SendException(ByVal Message As String)
        Pipe.Send(New Packet With {.Verb = RpcVerb.Excetpion, .Content = UTF16.GetBytes(Message)})
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

    Public Sub SendRequestExecute(ByVal mi As MethodInfo, ByVal ParameterWrites As IEnumerable(Of Action(Of IParameterWriter)), ByVal ReturnValueReaders As IEnumerable(Of Action(Of IParameterReader)))
        Dim mid = GetMethodId(mi)
        Dim pws = ParameterWrites.ToArray()
        Dim rrs = ReturnValueReaders.ToArray()

        Dim cp As New ContentPacker(s)
        cp.WriteParameter(mid)
        If pws.Length <> mi.GetParameters().Length Then Throw New ArgumentException
        cp.WriteParameter(Of Int32)(pws.Length)
        For Each pw In pws
            pw(cp)
        Next

        Pipe.Send(New Packet With {.Verb = RpcVerb.RequestExecute, .Content = cp.Build()})
        Dim p = ReceivePacket(RpcVerb.ResponseExecute)

        Dim cu As New ContentUnpacker(p.Content, s)
        Dim NumReturnValue = cu.ReadParameter(Of Int32)()
        If NumReturnValue <> rrs.Length Then Throw New InvalidDataException
        For Each rr In rrs
            rr(cu)
        Next
    End Sub
    Private Sub ProcessRequestExecute(ByVal p As Packet)
        Try
            Select Case p.Verb
                Case RpcVerb.RequestExecute
                    Dim cu As New ContentUnpacker(p.Content, s)
                    Dim eid = cu.ReadParameter(Of Int32)()
                    Dim ei = EventDict(eid)
                    Dim eh = EventHandlers(ei)

                    Dim NumParameters = cu.ReadParameter(Of Int32)()
                    If NumParameters <> ei.GetRaiseMethod().GetParameters().Length Then Throw New InvalidDataException

                    eh(cu)

                    Dim NumReturnValues = cu.ReadParameter(Of Int32)()
                    If NumReturnValues <> 0 Then Throw New InvalidDataException

                    Dim cp As New ContentPacker(s)
                    cp.WriteParameter(0)

                    Pipe.Send(New Packet With {.Verb = RpcVerb.ResponseExecute, .Content = cp.Build()})
                Case Else
                    Throw New NotSupportedException
            End Select
        Catch ex As Exception
            SendException(ExceptionInfo.GetExceptionInfo(ex))
        End Try
    End Sub

    Private Shared Function GetTypeFriendlyName(ByVal Type As Type) As String
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

    Private Function FromBytes(Of T)(ByVal Bytes As Byte()) As T
        Using Stream = StreamEx.Create()
            Stream.Write(Bytes)
            Stream.Position = 0
            Return s.Read(Of T)(Stream)
        End Using
    End Function
    Private Function ToBytes(Of T)(ByVal Value As T) As Byte()
        Using Stream = StreamEx.Create()
            s.Write(Value, Stream)
            Stream.Position = 0
            Return Stream.Read(Stream.Length)
        End Using
    End Function

    Private Sub SendPrimitiveType(ByVal tid As Int32, ByVal t As Type)
        Dim tb As New PrimitiveTypeBinding With {
            .TypeId = tid,
            .FriendlyTypeName = GetTypeFriendlyName(t)
        }
        Pipe.Send(New Packet With {.Verb = RpcVerb.RequestPrimitiveTypeBinding, .Content = ToBytes(tb)})
        ReceivePacket(RpcVerb.ResponsePrimitiveTypeBinding)
    End Sub
    Private Sub SendCollectionType(ByVal tid As Int32, ByVal t As Type)
        Dim tb As New CollectionTypeBinding With {
            .TypeId = tid,
            .ElementTypeId = GetTypeId(t)
        }
        Pipe.Send(New Packet With {.Verb = RpcVerb.RequestCollectionTypeBinding, .Content = ToBytes(tb)})
        ReceivePacket(RpcVerb.ResponseCollectionTypeBinding)
    End Sub
    Private Sub SendRecordType(ByVal tid As Int32, ByVal t As Type, ByVal m As FieldOrPropertyInfo())
        Dim tb As New RecordTypeBinding With {
            .TypeId = tid,
            .FriendlyTypeName = GetTypeFriendlyName(t),
            .FieldOrProperties = m.Select(Function(f) New RecordTypeFieldBinding With {.Name = f.Member.Name, .TypeId = GetTypeId(f.Type)}).ToArray()
        }
        Pipe.Send(New Packet With {.Verb = RpcVerb.RequestRecordTypeBinding, .Content = ToBytes(tb)})
        ReceivePacket(RpcVerb.ResponseRecordTypeBinding)
    End Sub
    Private Sub SendMethod(ByVal mid As Int32, ByVal mi As MethodInfo)
        Dim mb As New MethodBinding With {
            .MethodId = mid,
            .MethodName = mi.Name,
            .TypeParamters = mi.GetGenericArguments().Select(Function(ga) GetTypeId(ga)).ToArray(),
            .Parameters = mi.GetParameters().Select(Function(param) GetTypeId(param.ParameterType)).ToArray(),
            .ReturnValues = (Function(rv) IIf(Of Int32())(rv Is GetType(Void), {}, {GetTypeId(rv)}))(mi.ReturnType)
        }
        Pipe.Send(New Packet With {.Verb = RpcVerb.RequestMethodBinding, .Content = ToBytes(mb)})
        ReceivePacket(RpcVerb.ResponseMethodBinding)
    End Sub
    Private Sub ProcessType(ByVal tid As Int32, ByVal t As Type)
        If TypeDict.ContainsKey(tid) Then
            If TypeDict(tid) Is t Then Return
            Throw New InvalidDataException
        ElseIf PrimitiveTypeBindings.ContainsKey(tid) Then
            Dim tb = PrimitiveTypeBindings(tid)
            Dim Name = GetTypeFriendlyName(t)
            If tb.FriendlyTypeName <> Name Then Throw New InvalidDataException
            TypeDict.Add(tid, t)
            TypeInvDict.Add(t, tid)
            PrimitiveTypeBindings.Remove(tid)
        ElseIf CollectionTypeBindings.ContainsKey(tid) Then
            Dim tb = CollectionTypeBindings(tid)
            ProcessType(tb.ElementTypeId, t.GetCollectionElementType)
            TypeDict.Add(tid, t)
            TypeInvDict.Add(t, tid)
            CollectionTypeBindings.Remove(tid)
        ElseIf RecordTypeBindings.ContainsKey(tid) Then
            Dim tb = RecordTypeBindings(tid)
            Dim Name = GetTypeFriendlyName(t)
            If tb.FriendlyTypeName <> Name Then Throw New InvalidDataException

            Dim Members As FieldOrPropertyInfo() = Nothing
            Dim iri = t.TryGetImmutableRecordInfo
            If iri IsNot Nothing Then
                Members = iri.Members
            Else
                Dim mri = t.TryGetMutableRecordInfo
                If mri IsNot Nothing Then
                    Members = mri.Members
                End If
            End If

            If Members Is Nothing Then Throw New InvalidDataException
            If tb.FieldOrProperties.Length <> Members.Length Then Throw New InvalidDataException

            For Each Pair In tb.FieldOrProperties.Zip(Members, Function(tbf, m) New With {.f = tbf, .m = m})
                If Pair.f.Name <> Pair.m.Member.Name Then Throw New InvalidDataException
                ProcessType(Pair.f.TypeId, Pair.m.Type)
            Next

            TypeDict.Add(tid, t)
            TypeInvDict.Add(t, tid)
            RecordTypeBindings.Remove(tid)
        Else
            Throw New InvalidDataException
        End If
    End Sub
    Private Sub ProcessMetaData(ByVal p As Packet)
        Try
            Select Case p.Verb
                Case RpcVerb.RequestPrimitiveTypeBinding
                    Dim tb = FromBytes(Of PrimitiveTypeBinding)(p.Content)
                    If tb.TypeId <> NumType Then Throw New InvalidDataException
                    PrimitiveTypeBindings.Add(tb.TypeId, tb)
                    NumType += 1
                Case RpcVerb.RequestCollectionTypeBinding
                    Dim tb = FromBytes(Of CollectionTypeBinding)(p.Content)
                    If tb.TypeId <> NumType Then Throw New InvalidDataException
                    CollectionTypeBindings.Add(tb.TypeId, tb)
                    NumType += 1
                Case RpcVerb.RequestRecordTypeBinding
                    Dim tb = FromBytes(Of RecordTypeBinding)(p.Content)
                    If tb.TypeId <> NumType Then Throw New InvalidDataException
                    RecordTypeBindings.Add(tb.TypeId, tb)
                    NumType += 1
                Case RpcVerb.RequestMethodBinding
                    Dim mb = FromBytes(Of MethodBinding)(p.Content)
                    If mb.MethodId <> NumMethod Then Throw New InvalidDataException

                    Dim ei = InterfaceType.GetEvent(mb.MethodName)
                    If mb.TypeParamters.Length <> 0 Then Throw New InvalidDataException

                    Dim Parameters = ei.GetRaiseMethod().GetParameters()
                    If mb.Parameters.Length <> Parameters.Length Then Throw New InvalidDataException
                    If mb.ReturnValues.Length <> 0 Then Throw New InvalidDataException

                    For Each Pair In mb.Parameters.Zip(Parameters, Function(mbp, mp) New With {.tid = mbp, .t = mp.ParameterType})
                        ProcessType(Pair.tid, Pair.t)
                    Next

                    EventDict.Add(mb.MethodId, ei)

                    NumMethod += 1
                Case Else
                    Throw New NotSupportedException
            End Select
        Catch ex As Exception
            SendException(ExceptionInfo.GetExceptionInfo(ex))
        End Try
    End Sub

    Private NumType As Int32
    Private TypeDict As New Dictionary(Of Int32, Type)
    Private TypeInvDict As New Dictionary(Of Type, Int32)
    Private PrimitiveTypeBindings As New Dictionary(Of Int32, PrimitiveTypeBinding)
    Private CollectionTypeBindings As New Dictionary(Of Int32, CollectionTypeBinding)
    Private RecordTypeBindings As New Dictionary(Of Int32, RecordTypeBinding)

    Private NumMethod As Int32
    Private MethodDict As New Dictionary(Of Int32, MethodInfo)
    Private MethodInvDict As New Dictionary(Of MethodInfo, Int32)
    Private EventDict As New Dictionary(Of Int32, EventInfo)
    Private EventHandlers As New Dictionary(Of EventInfo, Action(Of IParameterWriter))

    Private Function GetTypeId(ByVal t As Type) As Int32
        If Not TypeInvDict.ContainsKey(t) Then
            Dim tid = NumType
            Dim Success = False
            Try
                TypeInvDict.Add(t, tid)
                TypeDict.Add(tid, t)
                NumType += 1
                If t.IsProperCollectionType Then
                    SendCollectionType(tid, t)
                Else
                    Dim iri = t.TryGetImmutableRecordInfo
                    If iri IsNot Nothing Then
                        SendRecordType(tid, t, iri.Members)
                    Else
                        Dim mri = t.TryGetMutableRecordInfo
                        If mri IsNot Nothing Then
                            SendRecordType(tid, t, mri.Members)
                        Else
                            SendPrimitiveType(tid, t)
                        End If
                    End If
                End If
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
            Dim mid = MethodDict.Count
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

    End Sub
End Class

Public Class ExecutorSlave
    Implements IDisposable

    Private Pipe As IPipe

    Public Sub New(ByVal Pipe As IPipe)
        Me.Pipe = Pipe
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        If Pipe IsNot Nothing Then
            Pipe.Dispose()
            Pipe = Nothing
        End If
    End Sub


End Class
