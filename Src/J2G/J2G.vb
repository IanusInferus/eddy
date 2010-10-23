'==========================================================================
'
'  File:        J2G.vb
'  Location:    Eddy.J2G <Visual Basic .Net>
'  Description: 文本本地化工具日汉转换插件
'  Version:     2010.10.24.
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
    Implements ITextLocalizerControlPlugin
    Implements ITextLocalizerTranslatorPlugin

    Friend WithEvents ToolStripButton_Translate As System.Windows.Forms.ToolStripButton

    Public Sub New()
        Me.ToolStripButton_Translate = New System.Windows.Forms.ToolStripButton
        '
        'ToolStripButton_Translate
        '
        Me.ToolStripButton_Translate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_Translate.Image = My.Resources.Translate
        Me.ToolStripButton_Translate.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_Translate.Name = "ToolStripButton_Translate"
        Me.ToolStripButton_Translate.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_Translate.Text = "日汉转换"
    End Sub

    Private Sub ToolStripButton_Translate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton_Translate.Click
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

    Public Function GetControlDescriptors() As System.Collections.Generic.IEnumerable(Of Eddy.Interfaces.ControlDescriptor) Implements Eddy.Interfaces.ITextLocalizerControlPlugin.GetControlDescriptors
        Return New ControlDescriptor() {New ControlDescriptor With {.Control = ToolStripButton_Translate, .Target = ControlId.ToolStrip}}
    End Function

    Public Function TranslateText(ByVal SourceColumn As Integer, ByVal TargeColumn As Integer, ByVal Text As String) As String Implements Eddy.Interfaces.ITextLocalizerTranslatorPlugin.TranslateText
        Return HanziConverter.J2G(Text)
    End Function
End Class
