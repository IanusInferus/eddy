'==========================================================================
'
'  File:        Plugin.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 文本本地化工具插件
'  Version:     2010.12.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Windows.Forms
Imports Eddy
Imports Eddy.Base

Public NotInheritable Class Plugin
    Implements ITextLocalizerUserInterfacePlugin

    Private FormMain As FormMain
    Public Sub Initialize(ByVal ApplicationData As TextLocalizerData) Implements ITextLocalizerUserInterfacePlugin.Initialize
        If FormMain IsNot Nothing Then Throw New InvalidOperationException
        FormMain = New FormMain
        FormMain.Initialize(ApplicationData)
    End Sub
    Public Function Run() As Integer Implements ITextLocalizerUserInterfacePlugin.Run
        Application.Run(FormMain)
        Return 0
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If FormMain IsNot Nothing Then
            FormMain.Dispose()
            FormMain = Nothing
        End If
    End Sub
End Class
