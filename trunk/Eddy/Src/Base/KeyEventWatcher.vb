'==========================================================================
'
'  File:        KeyEventWatcher.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 按键事件监视器
'  Version:     2010.12.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Eddy.Interfaces

Public Class KeyEventWatcher
    Private KeyDownDict As New Dictionary(Of HashSet(Of VirtualKeys), Action)(HashSet(Of VirtualKeys).CreateSetComparer)
    Private KeyUpDict As New Dictionary(Of HashSet(Of VirtualKeys), Action)(HashSet(Of VirtualKeys).CreateSetComparer)
    Private CurrentKeySet As New HashSet(Of VirtualKeys)

    Public Sub New()
    End Sub

    Public Sub Register(ByVal KeyCombination As VirtualKeys(), ByVal EventType As KeyEventType, ByVal Handler As Action)
        Dim KeySet = New HashSet(Of VirtualKeys)(KeyCombination)

        Select Case EventType
            Case KeyEventType.Down
                If KeyDownDict.ContainsKey(KeySet) Then
                    KeyDownDict(KeySet) = Handler
                Else
                    KeyDownDict.Add(KeySet, Handler)
                End If
            Case KeyEventType.Up
                If KeyUpDict.ContainsKey(KeySet) Then
                    KeyUpDict(KeySet) = Handler
                Else
                    KeyUpDict.Add(KeySet, Handler)
                End If
            Case Else
                Throw New ArgumentException
        End Select
    End Sub
    Public Sub Unregister(ByVal KeyCombination As VirtualKeys(), ByVal EventType As KeyEventType, ByVal Handler As Action)
        Dim KeySet = New HashSet(Of VirtualKeys)(KeyCombination)

        Select Case EventType
            Case KeyEventType.Down
                If KeyDownDict.ContainsKey(KeySet) Then
                    If KeyDownDict(KeySet) = Handler Then
                        KeyDownDict.Remove(KeySet)
                    End If
                End If
            Case KeyEventType.Up
                If KeyUpDict.ContainsKey(KeySet) Then
                    If KeyUpDict(KeySet) = Handler Then
                        KeyUpDict.Remove(KeySet)
                    End If
                End If
            Case Else
                Throw New ArgumentException
        End Select
    End Sub

    Public Sub KeyDown(ByVal Key As VirtualKeys)
        If Not CurrentKeySet.Contains(Key) Then
            CurrentKeySet.Add(Key)
        End If
        If KeyDownDict.ContainsKey(CurrentKeySet) Then
            KeyDownDict(CurrentKeySet)()
        End If
    End Sub

    Public Sub KeyUp(ByVal Key As VirtualKeys)
        If CurrentKeySet.Contains(Key) Then
            Try
                If KeyUpDict.ContainsKey(CurrentKeySet) Then
                    KeyUpDict(CurrentKeySet)()
                End If
            Finally
                CurrentKeySet.Remove(Key)
            End Try
        End If
    End Sub

    Public Sub KeyClear()
        CurrentKeySet.Clear()
    End Sub
End Class
