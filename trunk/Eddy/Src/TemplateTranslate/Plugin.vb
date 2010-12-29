'==========================================================================
'
'  File:        Plugin.vb
'  Location:    Eddy.TemplateTranslate <Visual Basic .Net>
'  Description: 文本本地化工具模板翻译插件
'  Version:     2010.12.29.
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

Public Class WindowReferenceAdapter
    Implements IWin32Window
    Private Reference As WindowReference
    Public Sub New(ByVal Reference As WindowReference)
        Me.Reference = Reference
    End Sub
    Private ReadOnly Property HandleInterface As IntPtr Implements IWin32Window.Handle
        Get
            Return Reference.Handle
        End Get
    End Property
End Class

Public Class Plugin
    Inherits TextLocalizerBase
    Implements ITextLocalizerToolStripButtonPlugin

    Private WithEvents FormTemplateTranslate As FormTemplateTranslate
    Private WithEvents ToolStripButton_TemplateTranslate As System.Windows.Forms.ToolStripButton

    Public Sub New()
        FormTemplateTranslate = New FormTemplateTranslate
    End Sub

    Public Function GetToolStripButtonDescriptors() As IEnumerable(Of ToolStripButtonDescriptor) Implements ITextLocalizerToolStripButtonPlugin.GetToolStripButtonDescriptors
        FormTemplateTranslate.Controller = Controller
        FormTemplateTranslate.TextNames = TextNames
        FormTemplateTranslate.Columns = Columns
        FormTemplateTranslate.MainColumnIndex = MainColumnIndex

        Return New ToolStripButtonDescriptor() {New ToolStripButtonDescriptor With {.Image = My.Resources.TemplateTranslate, .Text = "按模板翻译...", .Click = AddressOf ToolStripButton_Click}}
    End Function

    Private Sub ToolStripButton_Click()
        If FormTemplateTranslate.Visible Then
            FormTemplateTranslate.Focus()
        Else
            With FormTemplateTranslate
                .Show(New WindowReferenceAdapter(Controller.MainWindow))
            End With
        End If
    End Sub
End Class
