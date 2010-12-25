'==========================================================================
'
'  File:        RpcInterface.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 远程调用接口
'  Version:     2010.12.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports Firefly
Imports Firefly.Streaming

'Packet:
'Verb:Int32 Length:Int32 Content:Byte{Length}
Public Class Packet
    Public Verb As RpcVerb
    Public Content As Byte()
End Class

Public Enum RpcVerbKind As int32
    KindMask = &HFF000000

    KindException = &H1000000
    KindExecute = &H2000000
    KindMetaData = &H3000000
End Enum

Public Enum RpcVerb As Int32
    Excetpion = &H1000000 'Exception:String

    RequestExecute = &H2000000 '(MethodId NumParameter Parameter*) ->
    ResponseExecute = &H2000001 '(NumReturnValue ReturnValue*)
    RequestEvent = &H2000010 '() ->
    ResponseEvent = &H2000011 '()

    RequestTypeBinding = &H3000000 '(TypeId Hash) ->
    ResponseTypeBinding = &H3000001 '()
    RequestMethodBinding = &H3000030 '(MethodId MethodName TypeId{} TypeId{} TypeId{}) ->
    ResponseMethodBinding = &H3000031 '()
End Enum

Public Module RpcVerbFunctions
    <Extension()> Public Function Kind(ByVal This As RpcVerb) As RpcVerbKind
        Return This And RpcVerbKind.KindMask
    End Function
End Module

Public Class TypeBinding
    Public TypeId As Int32
    Public Hash As Int32
End Class

Public Class MethodBinding
    Public MethodId As Int32
    Public MethodName As String
    Public TypeParamters As Int32()
    Public Parameters As Int32()
    Public ReturnValues As Int32()
End Class

Public Interface ISerializer
    Function Read(Of T)(ByVal s As IReadableStream) As T
    Sub Write(Of T)(ByVal Value As T, ByVal s As IWritableStream)
End Interface

Public Interface IParameterWriter
    Sub WriteParameter(Of T)(ByVal Parameter As T)
End Interface

Public Interface IParameterReader
    Function ReadParameter(Of T)() As T
End Interface

Public Interface IMasterPipe
    Inherits IDisposable

    Sub Send(ByVal p As Packet)
    Sub Send(ByVal p As Packet, ByVal Timeout As Integer)
    Function Receive() As Packet
    Function Receive(ByVal Timeout As Integer) As Packet
End Interface

Public Interface ISlavePipe
    Inherits IDisposable

    Sub Send(ByVal p As Packet)
    Function Receive() As Packet
End Interface