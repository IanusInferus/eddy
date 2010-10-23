'==========================================================================
'
'  File:        Plugin.vb
'  Location:    Eddy.FindReplace <Visual Basic .Net>
'  Description: 文本本地化工具查找替换插件
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
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Setting
Imports Eddy.Interfaces

Public Class Config
    Public EnableColor As Boolean = True
    Public ForeColor As String = "FF000000"
    Public BackColor As String = "FFFFFFB7"
End Class

Public Class Plugin
    Inherits TextLocalizerBase
    Implements ITextLocalizerTextHighlighter
    Implements ITextLocalizerControlPlugin

    Private SettingPath As String = "FindReplace.locplugin"
    Private Config As Config
    Private ForeColor As Color
    Private BackColor As Color

    Private WithEvents FormSearch As FormSearch
    Private WithEvents ToolStripButton_FindReplace As System.Windows.Forms.ToolStripButton

    Public Sub New()
        If File.Exists(SettingPath) Then
            Config = Xml.ReadFile(Of Config)(SettingPath)
        Else
            Config = New Config
        End If
        ForeColor = Color.FromArgb(Integer.Parse(Config.ForeColor, Globalization.NumberStyles.HexNumber))
        BackColor = Color.FromArgb(Integer.Parse(Config.BackColor, Globalization.NumberStyles.HexNumber))

        FormSearch = New FormSearch

        Me.ToolStripButton_FindReplace = New System.Windows.Forms.ToolStripButton
        '
        'ToolStripButton_FindReplace
        '
        Me.ToolStripButton_FindReplace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton_FindReplace.Image = My.Resources.FindReplace
        Me.ToolStripButton_FindReplace.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton_FindReplace.Name = "ToolStripButton_FindReplace"
        Me.ToolStripButton_FindReplace.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton_FindReplace.Text = "查找替换(Ctrl+F)"
    End Sub
    Protected Overrides Sub DisposeManagedResource()
        Try
            Xml.WriteFile(SettingPath, UTF16, Config)
        Catch
        End Try
        MyBase.DisposeManagedResource()
    End Sub

    Public Function GetControlDescriptors() As IEnumerable(Of ControlDescriptor) Implements ITextLocalizerControlPlugin.GetControlDescriptors
        FormSearch.Controller = Controller
        FormSearch.TextNames = TextNames
        FormSearch.Columns = Columns
        FormSearch.MainColumnIndex = MainColumnIndex

        Return New ControlDescriptor() {New ControlDescriptor With {.Control = ToolStripButton_FindReplace, .Target = ControlId.ToolStrip}}
    End Function

    Public Sub Application_KeyDown(ByVal ControlId As ControlId, ByVal e As KeyEventArgs) Handles Controller.KeyDown
        Select Case e.KeyData
            Case Keys.Control Or Keys.F, Keys.Control Or Keys.H, Keys.Control Or Keys.R
                ToolStripButton_FindReplace_Click(Nothing, e)
            Case Else
                Return
        End Select
        e.Handled = True
    End Sub

    Private Sub ToolStripButton_FindReplace_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton_FindReplace.Click
        If FormSearch.Visible Then
            FormSearch.Focus()
        Else
            With FormSearch
                .Show(Controller.Form)
            End With
        End If
    End Sub

    Private CurrentFindText As String = ""
    Private CurrentFindTextRegex As Regex
    Private Sub FormSearch_FindOrReplacePerformed(ByVal TextFind As String, ByVal Regex As System.Text.RegularExpressions.Regex) Handles FormSearch.FindOrReplacePerformed
        If Not Config.EnableColor Then Return
        If CurrentFindText <> TextFind Then
            CurrentFindText = TextFind
            If CurrentFindText = "" Then
                CurrentFindTextRegex = Nothing
            Else
                CurrentFindTextRegex = Regex
            End If
            Controller.RefreshMainPanel()
            Controller.RefreshGrid()
        End If
    End Sub
    Private Function GetTextStylesForText(ByVal Text As String) As TextStyle()
        Return (From m As Match In CurrentFindTextRegex.Matches(Text) Select (New TextStyle With {.Index = m.Index, .Length = m.Length, .ForeColor = ForeColor, .BackColor = BackColor})).ToArray
    End Function
    Public Function GetTextStyles(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of TextStyle()) Implements Eddy.Interfaces.ITextLocalizerTextHighlighter.GetTextStyles
        If CurrentFindTextRegex Is Nothing Then Return Nothing
        Return (From i In Enumerable.Range(0, Columns.Count) Select GetTextStylesForText(FormatedTexts(i))).ToArray
    End Function
    Private Sub FormSearch_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles FormSearch.VisibleChanged
        If Not FormSearch.Visible Then FormSearch_FindOrReplacePerformed("", Nothing)
    End Sub
End Class
