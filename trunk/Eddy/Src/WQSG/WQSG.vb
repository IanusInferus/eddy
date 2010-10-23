'==========================================================================
'
'  File:        WQSG.vb
'  Location:    Eddy.WQSG <Visual Basic .Net>
'  Description: 文本本地化工具WQSG文本插件
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
Imports Firefly.Setting
Imports Eddy.Interfaces

Public Class WQSGPlugin
    Inherits TextLocalizerBase
    Implements ITextLocalizerFormatPlugin
    Implements ITextLocalizerControlPlugin

    Friend WithEvents ToolStripButton_Delete As System.Windows.Forms.ToolStripButton

    Public Sub New()
        Me.ToolStripButton_Delete = New System.Windows.Forms.ToolStripButton
        '
        'ToolStripButton_Delete
        '
        Me.ToolStripButton_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_Delete.Image = My.Resources.Delete
        Me.ToolStripButton_Delete.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_Delete.Name = "ToolStripButton_Delete"
        Me.ToolStripButton_Delete.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_Delete.Text = "删除选中文本"
    End Sub

    Private Sub ToolStripButton_Delete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton_Delete.Click
        Dim TextIndices = Controller.TextIndices.ToArray
        For Each Column In Columns
            If Column.IsReadOnly Then Continue For

            If Column.Type <> "WQSGText" Then
                Dim l = Column.Item(Controller.TextName)
                For Each TextIndex In TextIndices
                    l.Text(TextIndex) = ""
                Next
            Else
                Dim l = DirectCast(Column.Item(Controller.TextName), WQSGTextList)
                For Each TextIndex In TextIndices
                    l.Text(TextIndex) = ""
                    l.Item(TextIndex).Length = 0
                Next
            End If
        Next
        Controller.RefreshMainPanel()
        Controller.RefreshGrid()
    End Sub

    Public Function GetTextListFactories() As System.Collections.Generic.IEnumerable(Of ILocalizationTextListFactory) Implements Eddy.Interfaces.ITextLocalizerFormatPlugin.GetTextListFactories
        Return New ILocalizationTextListFactory() {Me}
    End Function

    Public Function GetControlDescriptors() As System.Collections.Generic.IEnumerable(Of Eddy.Interfaces.ControlDescriptor) Implements Eddy.Interfaces.ITextLocalizerControlPlugin.GetControlDescriptors
        If (From c In Columns Where c.Type = "WQSGText").Count = 0 Then
            Return New ControlDescriptor() {}
        Else
            Return New ControlDescriptor() {New ControlDescriptor With {.Control = ToolStripButton_Delete, .Target = ControlId.ToolStrip}}
        End If
    End Function
End Class
