'==========================================================================
'
'  File:        Plugin.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: UI插件
'  Version:     2010.12.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports Eddy
Imports Eddy.Interfaces
Imports Eddy.Base

Public NotInheritable Class Plugin
    Implements ITextLocalizerUserInterfacePlugin

    Private App As Application
    Private WindowMain As WindowMain
    Public Sub Initialize(ByVal ApplicationData As TextLocalizerData) Implements ITextLocalizerUserInterfacePlugin.Initialize
        If App IsNot Nothing Then Throw New InvalidOperationException
        If WindowMain IsNot Nothing Then Throw New InvalidOperationException
        App = New Application
        WindowMain = New WindowMain
    End Sub

    Public Function Run() As Integer Implements ITextLocalizerUserInterfacePlugin.Run
        Return App.Run()
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If WindowMain IsNot Nothing Then
            Try
                WindowMain.Close()
            Catch
            End Try
            WindowMain = Nothing
        End If
    End Sub
End Class
