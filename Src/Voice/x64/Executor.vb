﻿Imports System
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

    Public Sub SendException(ByVal Message As String)
        Pipe.Send(New Packet With {.Verb = RpcVerb.Excetpion, .Content = UTF16.GetBytes(Message)})
    End Sub

    Public Function SendRequestExecute(ByVal MethodId As Integer, ByVal Content As Byte()) As Byte()
        Pipe.Send(New Packet With {.Verb = RpcVerb.RequestExecute, .Content = Content})
        Dim p = ReceiveResponseExecute()
    End Function

    Public Function ReceiveResponseExecute() As Byte()
        Dim p = Pipe.Receive()
        Select Case p.Verb
            Case RpcVerb.ResponseExecute
                Return p.Content
            Case RpcVerb.Excetpion
                Throw New Exception(UTF16.GetString(p.Content))
            Case RpcVerb.RequestExecute

        End Select
    End Function

    Public Sub SendMetaData()

    End Sub

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
