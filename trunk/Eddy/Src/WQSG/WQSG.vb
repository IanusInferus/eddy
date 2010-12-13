'==========================================================================
'
'  File:        WQSG.vb
'  Location:    Eddy.WQSG <Visual Basic .Net>
'  Description: 文本本地化工具WQSG文本插件
'  Version:     2010.12.14.
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
    Implements ITextLocalizerToolStripButtonPlugin

    Public Sub New()
    End Sub

    Private Sub ToolStripButton_Delete_Click()
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

    Public Function GetTextListFactories() As IEnumerable(Of ILocalizationTextListFactory) Implements ITextLocalizerFormatPlugin.GetTextListFactories
        Return New ILocalizationTextListFactory() {Me}
    End Function

    Public Function GetToolStripButtonDescriptors() As IEnumerable(Of ToolStripButtonDescriptor) Implements ITextLocalizerToolStripButtonPlugin.GetToolStripButtonDescriptors
        WQSGTextList.Controller = Controller

        If (From c In Columns Where c.Type = "WQSGText").Count = 0 Then
            Return New ToolStripButtonDescriptor() {}
        Else
            Return New ToolStripButtonDescriptor() {New ToolStripButtonDescriptor With {.Image = My.Resources.Delete, .Text = "删除选中文本", .Click = AddressOf ToolStripButton_Delete_Click}}
        End If
    End Function
End Class
