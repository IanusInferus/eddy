Imports System
Imports Firefly
Imports Firefly.Streaming

'Packet:
'Verb:Int32 Length:Int32 Content:Byte{Length}
Public Class Packet
    Public Verb As IpcVerb
    Public Content As Byte()
End Class

Public Enum IpcVerb As Int32
    Excetpion = &H100 'Exception:String

    RequestExecute = &H200 '(MethodId NumParameter Parameter*) ->
    ResponseExecute = &H201 'Return?

    RequestPrimitiveTypeBinding = &H300 '(TypeId FriendlyTypeName) ->
    ResponsePrimitiveTypeBinding = &H301 '()

    RequestCollectionTypeBinding = &H310 '(TypeId FriendlyTypeName) ->
    ResponseCollectionTypeBinding = &H311 '()

    RequestRecordTypeBinding = &H320 '(TypeId FriendlyTypeName NumFieldOrProperty (Name TypeId){NumFieldOrProperty}) ->
    ResponseRecordTypeBinding = &H321 '()

    RequestMethodBinding = &H330 '(MethodId MethodName NumTypeParamter TypeId{NumTypeParamter} NumParameter TypeId{NumParameter}) ->
    ResponseMethodBinding = &H331 '()
End Enum

Public Interface ISerializer
    Function Read(Of T)(ByVal s As IReadableStream) As T
    Sub Write(Of T)(ByVal Value As T, ByVal s As IWritableStream)
End Interface

Public Interface IPipe
    Inherits IDisposable

    Sub Send(ByVal p As Packet)
    Function Receive() As Packet
End Interface
