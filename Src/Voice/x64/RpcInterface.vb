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


    RequestPrimitiveTypeBinding = &H3000000 '(TypeId FriendlyTypeName) ->
    ResponsePrimitiveTypeBinding = &H3000001 '()

    RequestCollectionTypeBinding = &H3000010 '(TypeId ElementTypeId:TypeId) ->
    ResponseCollectionTypeBinding = &H3000011 '()

    RequestRecordTypeBinding = &H3000020 '(TypeId FriendlyTypeName NumFieldOrProperty (Name TypeId){NumFieldOrProperty}) ->
    ResponseRecordTypeBinding = &H3000021 '()

    RequestMethodBinding = &H3000030 '(MethodId MethodName NumTypeParamter TypeId{NumTypeParamter} NumParameter TypeId{NumParameter} NumReturnValue TypeId{NumReturnValue}) ->
    ResponseMethodBinding = &H3000031 '()
End Enum

Public Module RpcVerbFunctions
    <Extension()> Public Function Kind(ByVal This As RpcVerb) As RpcVerbKind
        Return This And RpcVerbKind.KindMask
    End Function
End Module

Public Class PrimitiveTypeBinding
    Public TypeId As Int32
    Public FriendlyTypeName As String
End Class

Public Class CollectionTypeBinding
    Public TypeId As Int32
    Public ElementTypeId As String
End Class

Public Class RecordTypeFieldBinding
    Public Name As String
    Public TypeId As Int32
End Class

Public Class RecordTypeBinding
    Public TypeId As Int32
    Public FriendlyTypeName As String
    Public FieldOrProperties As RecordTypeFieldBinding()
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

Public Interface IPipe
    Inherits IDisposable

    Sub Send(ByVal p As Packet)
    Function Receive() As Packet
End Interface
