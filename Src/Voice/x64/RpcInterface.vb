Imports System
Imports Firefly
Imports Firefly.Streaming

'Packet:
'Verb:Int32 Length:Int32 Content:Byte{Length}
Public Class Packet
    Public Verb As RpcVerb
    Public Content As Byte()
End Class

Public Enum RpcVerb As Int32
    KindMask = &HFF000000


    KindException = &H1000000

    Excetpion = &H1000000 'Exception:String


    KindExecute = &H2000000

    RequestExecute = &H2000000 '(MethodId NumParameter Parameter*) ->
    ResponseExecute = &H2000001 'Return?


    KindMetaData = &H3000000

    RequestPrimitiveTypeBinding = &H3000000 '(TypeId FriendlyTypeName) ->
    ResponsePrimitiveTypeBinding = &H3000001 '()

    RequestCollectionTypeBinding = &H3000010 '(TypeId FriendlyTypeName) ->
    ResponseCollectionTypeBinding = &H3000011 '()

    RequestRecordTypeBinding = &H3000020 '(TypeId FriendlyTypeName NumFieldOrProperty (Name TypeId){NumFieldOrProperty}) ->
    ResponseRecordTypeBinding = &H3000021 '()

    RequestMethodBinding = &H3000030 '(MethodId MethodName NumTypeParamter TypeId{NumTypeParamter} NumParameter TypeId{NumParameter}) ->
    ResponseMethodBinding = &H3000031 '()
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
