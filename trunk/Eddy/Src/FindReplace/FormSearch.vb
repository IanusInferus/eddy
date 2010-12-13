'==========================================================================
'
'  File:        FormSearch.vb
'  Location:    Eddy.FindReplace <Visual Basic .Net>
'  Description: 文本本地化工具查找替换插件窗体
'  Version:     2010.12.14.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Setting
Imports Eddy.Interfaces

Public Class FormSearch
    Public WithEvents Controller As ITextLocalizerApplicationController
    Public TextNames As IEnumerable(Of String)
    Public Columns As IEnumerable(Of LocalizationTextProvider)
    Public MainColumnIndex As Integer

    Public Event FindOrReplacePerformed(ByVal TextFind As String, ByVal Regex As Regex)

    Public Shadows Sub Hide()
        Me.Owner = Nothing
        MyBase.Hide()
    End Sub

    Private Sub LocalizationTextBoxes_GotFocus() Handles Controller.ColumnSelectionChanged
        Dim ColumnIndex = Controller.ColumnIndex
        NumericUpDown_Column.Text = Columns(ColumnIndex).DisplayName
        If Not CBool(Mode And FindReplaceMode.MultiColumn) AndAlso Columns(ColumnIndex).IsReadOnly Then
            Button_Replace.Enabled = False
            Button_ReplaceAll.Enabled = False
        Else
            Button_Replace.Enabled = True
            Button_ReplaceAll.Enabled = True
        End If
    End Sub

    Private Sub FormSearch_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ComboBox_Scope.Text = ComboBox_Scope.Items(0)
        Dim ColumnIndex = Controller.ColumnIndex
        NumericUpDown_Column.Text = Columns(ColumnIndex).DisplayName
        If Not CBool(Mode And FindReplaceMode.MultiColumn) AndAlso Columns(ColumnIndex).IsReadOnly Then
            Button_Replace.Enabled = False
            Button_ReplaceAll.Enabled = False
        Else
            Button_Replace.Enabled = True
            Button_ReplaceAll.Enabled = True
        End If
    End Sub

    Private Sub FormSearch_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            DialogResult = Windows.Forms.DialogResult.Cancel
            Me.Hide()
        End If
    End Sub

    Public Mode As FindReplaceMode

    Private Sub ComboBox_Scope_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox_Scope.SelectedIndexChanged
        Mode = ComboBox_Scope.SelectedIndex
        Dim ColumnIndex = Controller.ColumnIndex
        If Not CBool(Mode And FindReplaceMode.MultiColumn) AndAlso Columns(ColumnIndex).IsReadOnly Then
            Button_Replace.Enabled = False
            Button_ReplaceAll.Enabled = False
        Else
            Button_Replace.Enabled = True
            Button_ReplaceAll.Enabled = True
        End If
    End Sub

    Private Sub CheckBox_Regex_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox_Regex.CheckedChanged
        CheckBox_SmartHanzi.Enabled = Not CheckBox_Regex.Checked
    End Sub

    Private Sub FormSearch_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyUp
        If e.KeyData = System.Windows.Forms.Keys.Escape Then
            Me.Hide()
            e.Handled = True
        End If
    End Sub

    Private TextFindHistory As New ListBindingConverter
    Private Sub Button_Find_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Find.Click
        If ComboBox_TextFind.Text = "" Then Return

        Dim TextFind = ComboBox_TextFind.Text
        If ComboBox_TextFind.Items.Contains(TextFind) Then
            ComboBox_TextFind.Items.Remove(TextFind)
        End If
        ComboBox_TextFind.Items.Insert(0, TextFind)
        ComboBox_TextFind.Text = TextFind

        Controller.FlushLocalizedText()

        Dim FindReplace As New FindReplace(Mode, CheckBox_Case.Checked, CheckBox_Up.Checked, CheckBox_Regex.Checked, CheckBox_SmartHanzi.Checked)
        Dim tps = Columns

        Dim ColumnIndex = Controller.ColumnIndex
        Dim Input As New FindIndex With {.ColumnIndex = ColumnIndex, .TextName = Controller.TextName, .TextIndex = Controller.TextIndex, .Start = Controller.SelectionStart, .Length = Controller.SelectionLength}
        Dim Result = FindReplace.Find(tps, MainColumnIndex, Input, TextFind)
        If Result IsNot Nothing Then
            With Controller
                .TextName = Result.TextName
                .TextIndex = Result.TextIndex
            End With
            Controller.ColumnIndex = Result.ColumnIndex
            Controller.SelectionStart = Result.Start
            Controller.SelectionLength = Result.Length
            Controller.ScrollToCaret(Result.ColumnIndex)
            RaiseEvent FindOrReplacePerformed(TextFind, FindReplace.GetFindRegex(TextFind))
        Else
            Controller.ShowInfo("未找到下一条匹配文本。")
        End If
    End Sub

    Private Sub Button_Replace_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Replace.Click
        If ComboBox_TextFind.Text = "" Then Return

        Dim TextFind = ComboBox_TextFind.Text
        If ComboBox_TextFind.Items.Contains(TextFind) Then
            ComboBox_TextFind.Items.Remove(TextFind)
        End If
        ComboBox_TextFind.Items.Insert(0, TextFind)
        ComboBox_TextFind.Text = TextFind

        Dim TextReplace = ComboBox_TextReplace.Text
        If ComboBox_TextReplace.Items.Contains(TextReplace) Then
            ComboBox_TextReplace.Items.Remove(TextReplace)
        End If
        ComboBox_TextReplace.Items.Insert(0, TextReplace)
        ComboBox_TextReplace.Text = TextReplace

        Controller.FlushLocalizedText()

        Dim FindReplace As New FindReplace(Mode, CheckBox_Case.Checked, CheckBox_Up.Checked, CheckBox_Regex.Checked, CheckBox_SmartHanzi.Checked)
        Dim tps = Columns

        Dim ColumnIndex = Controller.ColumnIndex
        Dim Input As New FindIndex With {.ColumnIndex = ColumnIndex, .TextName = Controller.TextName, .TextIndex = Controller.TextIndex, .Start = Controller.SelectionStart, .Length = Controller.SelectionLength}
        Dim ReplaceResult = FindReplace.Replace(tps, MainColumnIndex, Input, TextFind, TextReplace)
        If ReplaceResult IsNot Nothing Then
            Controller.RefreshColumn(ReplaceResult.ColumnIndex)
            Input = ReplaceResult
        End If
        Dim Result = FindReplace.Find(tps, MainColumnIndex, Input, TextFind, True)
        If Result IsNot Nothing Then
            With Controller
                .TextName = Result.TextName
                .TextIndex = Result.TextIndex
            End With
            Controller.ColumnIndex = Result.ColumnIndex
            Controller.SelectionStart = Result.Start
            Controller.SelectionLength = Result.Length
            Controller.ScrollToCaret(Result.ColumnIndex)
            RaiseEvent FindOrReplacePerformed(TextFind, FindReplace.GetFindRegex(TextFind))
        Else
            Controller.ShowInfo("未找到下一条匹配文本。")
        End If
    End Sub

    Private Sub Button_ReplaceAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_ReplaceAll.Click
        If ComboBox_TextFind.Text = "" Then Return

        Dim TextFind = ComboBox_TextFind.Text
        If ComboBox_TextFind.Items.Contains(TextFind) Then
            ComboBox_TextFind.Items.Remove(TextFind)
        End If
        ComboBox_TextFind.Items.Insert(0, TextFind)
        ComboBox_TextFind.Text = TextFind

        Dim TextReplace = ComboBox_TextReplace.Text
        If ComboBox_TextReplace.Items.Contains(TextReplace) Then
            ComboBox_TextReplace.Items.Remove(TextReplace)
        End If
        ComboBox_TextReplace.Items.Insert(0, TextReplace)
        ComboBox_TextReplace.Text = TextReplace

        Controller.FlushLocalizedText()

        Dim FindReplace As New FindReplace(Mode, CheckBox_Case.Checked, CheckBox_Up.Checked, CheckBox_Regex.Checked, CheckBox_SmartHanzi.Checked)
        Dim tps = Columns

        Dim ColumnIndex = Controller.ColumnIndex
        Dim Input As New FindIndex With {.ColumnIndex = ColumnIndex, .TextName = Controller.TextName, .TextIndex = Controller.TextIndex, .Start = Controller.SelectionStart, .Length = Controller.SelectionLength}
        Dim Count = FindReplace.ReplaceAll(tps, MainColumnIndex, Input, TextFind, TextReplace)
        Controller.RefreshMainPanel()
        Controller.RefreshGrid()
        Controller.ShowInfo("替换了{0}处搜索项。".Formats(Count))
    End Sub
End Class
