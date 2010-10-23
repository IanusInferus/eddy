'==========================================================================
'
'  File:        Plugin.vb
'  Location:    Eddy.TemplateTranslate <Visual Basic .Net>
'  Description: 文本本地化工具模板翻译插件
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
Imports System.Windows.Forms
Imports Eddy.Interfaces

Public Class Plugin
    Inherits TextLocalizerBase
    Implements ITextLocalizerControlPlugin

    Private WithEvents FormTemplateTranslate As FormTemplateTranslate
    Private WithEvents ToolStripButton_TemplateTranslate As System.Windows.Forms.ToolStripButton

    Public Sub New()
        FormTemplateTranslate = New FormTemplateTranslate

        Me.ToolStripButton_TemplateTranslate = New System.Windows.Forms.ToolStripButton
        '
        'ToolStripButton_TemplateTranslate
        '
        Me.ToolStripButton_TemplateTranslate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_TemplateTranslate.Image = My.Resources.TemplateTranslate
        Me.ToolStripButton_TemplateTranslate.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_TemplateTranslate.Name = "ToolStripButton_TemplateTranslate"
        Me.ToolStripButton_TemplateTranslate.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_TemplateTranslate.Text = "按模板翻译"
    End Sub

    Public Function GetControlDescriptors() As IEnumerable(Of ControlDescriptor) Implements ITextLocalizerControlPlugin.GetControlDescriptors
        FormTemplateTranslate.Controller = Controller
        FormTemplateTranslate.TextNames = TextNames
        FormTemplateTranslate.Columns = Columns
        FormTemplateTranslate.MainColumnIndex = MainColumnIndex

        Return New ControlDescriptor() {New ControlDescriptor With {.Control = ToolStripButton_TemplateTranslate, .Target = ControlId.ToolStrip}}
    End Function

    Private Sub ToolStripButton_TemplateTranslate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton_TemplateTranslate.Click
        If FormTemplateTranslate.Visible Then
            FormTemplateTranslate.Focus()
        Else
            With FormTemplateTranslate
                .Show(Controller.Form)
            End With
        End If
    End Sub
End Class
