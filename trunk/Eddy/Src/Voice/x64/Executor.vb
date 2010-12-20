Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports Firefly
Imports Firefly.TextEncoding

Public Class ExecutorMaster
    Implements IDisposable

    Private Pipe As IPipe
    Private EventRaiser As Action(Of Action)

    Public Sub New(ByVal Pipe As IPipe, ByVal EventRaiser As Action(Of Action))
        Me.Pipe = Pipe
        Me.EventRaiser = EventRaiser
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        If Pipe IsNot Nothing Then
            Pipe.Dispose()
            Pipe = Nothing
        End If
    End Sub

    Public Sub ThrowException(ByVal Message As String)
        Pipe.Send(New Packet With {.Verb = IpcVerb.Excetpion, .Content = UTF16.GetBytes(Message)})
    End Sub

    Public Function Request(ByVal MethodId As Integer, ByVal Content As Byte()) As Byte()
        Pipe.Send(New Packet With {.Verb = IpcVerb.RequestExecute, .Content = Content})

        While True
            Dim p = Pipe.Receive()
            Select Case p.Verb
                Case IpcVerb.ResponseExecute
                    Return p.Content
                Case IpcVerb.Excetpion
                    Throw New Exception(UTF16.GetString(p.Content))
                Case IpcVerb.RequestExecute

            End Select

        End While
    End Function

    Private MethodList As New List(Of MethodInfo)
    Private MethodDict As New Dictionary(Of MethodInfo, Integer)
    Public Function GetMethodId(ByVal mi As MethodInfo) As Integer

    End Function
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
