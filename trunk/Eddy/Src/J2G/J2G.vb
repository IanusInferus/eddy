'==========================================================================
'
'  File:        J2G.vb
'  Location:    Eddy.J2G <Visual Basic .Net>
'  Description: 文本本地化工具日汉转换插件
'  Version:     2010.12.10.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.IO
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Firefly.Setting
Imports Eddy.Interfaces

Public Class J2GPlugin
    Inherits TextLocalizerBase
    Implements ITextLocalizerToolStripButtonPlugin
    Implements ITextLocalizerTranslatorPlugin

    Public Sub New()
    End Sub

    Private Sub ToolStripButton_Click()
        Dim Text = Controller.Text(Controller.ColumnIndex)
        Dim s = Controller.SelectionStart
        Dim l = Controller.SelectionLength
        If l = 0 Then
            s = 0
            l = Text.Length
        End If
        Dim SelectedText = Text.Substring(s, l)
        Controller.Text(Controller.ColumnIndex) = Text.Substring(0, s) & HanziConverter.J2G(SelectedText) & Text.Substring(s + l, Text.Length - s - l)
        Controller.SelectionStart = s
        Controller.SelectionLength = SelectedText.Length
    End Sub

    Public Function GetToolStripButtonDescriptors() As IEnumerable(Of ToolStripButtonDescriptor) Implements ITextLocalizerToolStripButtonPlugin.GetToolStripButtonDescriptors
        Return New ToolStripButtonDescriptor() {New ToolStripButtonDescriptor With {.Image = My.Resources.Translate, .Text = "日汉转换", .Click = AddressOf ToolStripButton_Click}}
    End Function

    Public Function TranslateText(ByVal SourceColumn As Integer, ByVal TargeColumn As Integer, ByVal Text As String) As String Implements ITextLocalizerTranslatorPlugin.TranslateText
        Return HanziConverter.J2G(Text)
    End Function
End Class
