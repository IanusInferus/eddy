'==========================================================================
'
'  File:        FormTemplateTranslate.vb
'  Location:    Eddy.TemplateTranslate <Visual Basic .Net>
'  Description: 文本本地化工具模板翻译插件窗体
'  Version:     2010.10.24.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports Firefly.GUI
Imports Eddy.Interfaces

Public Class FormTemplateTranslate
    Public WithEvents Controller As ITextLocalizerApplicationController
    Public TextNames As IEnumerable(Of String)
    Public Columns As IEnumerable(Of LocalizationTextProvider)
    Public MainColumnIndex As Integer

    Public Event FindOrReplacePerformed(ByVal TextFind As String, ByVal Regex As Regex)

    Public Shadows Sub Hide()
        Me.Owner = Nothing
        MyBase.Hide()
    End Sub

    Private Sub FormTemplateTranslate_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim TextNames = Columns(MainColumnIndex).Keys
        ListBox_Source.Items.AddRange(TextNames.Select(Function(s) CObj(s)).ToArray)
        ListBox_Target.Items.AddRange(TextNames.Select(Function(s) CObj(s)).ToArray)

        Label_CurrentFileValue.Text = Controller.TextName
        Label_CurrentItemValue.Text = Controller.TextIndex + 1
    End Sub

    Private Sub FormSearch_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            DialogResult = Windows.Forms.DialogResult.Cancel
            Me.Hide()
        End If
    End Sub

    Private Sub Controller_TextNameChanged(ByVal e As System.EventArgs) Handles Controller.TextNameChanged
        Label_CurrentFileValue.Text = Controller.TextName
        Label_CurrentItemValue.Text = Controller.TextIndex + 1
    End Sub

    Private Sub Controller_TextIndexChanged(ByVal e As System.EventArgs) Handles Controller.TextIndexChanged
        Label_CurrentItemValue.Text = Controller.TextIndex + 1
    End Sub

    Private Sub Button_SelectPrevious_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_SelectPrevious.Click
        CheckBox_SourceList.Checked = True

        ListBox_Source.BeginUpdate()
        Try
            Dim CurrentName = Controller.TextName
            Dim Flag = True
            For n = 0 To ListBox_Source.Items.Count - 1
                If Flag Then
                    If DirectCast(ListBox_Source.Items(n), String) = CurrentName Then
                        Flag = False
                    End If
                End If
                ListBox_Source.SetSelected(n, Flag)
            Next
        Finally
            ListBox_Source.EndUpdate()
        End Try
    End Sub

    Private Sub Button_SelectNext_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_SelectNext.Click
        CheckBox_TargetList.Checked = True

        ListBox_Target.BeginUpdate()
        Try
            Dim CurrentName = Controller.TextName
            Dim Flag = False
            For n = 0 To ListBox_Target.Items.Count - 1
                ListBox_Target.SetSelected(n, Flag)
                If Not Flag Then
                    If DirectCast(ListBox_Target.Items(n), String) = CurrentName Then
                        Flag = True
                    End If
                End If
            Next
        Finally
            ListBox_Target.EndUpdate()
        End Try
    End Sub

    Private Sub Button_Translate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Translate.Click
        Dim SourceNames As New List(Of String)
        Dim SourceNameSet As New HashSet(Of String)
        Dim TargetNames As New List(Of String)
        Dim TargetNameSet As New HashSet(Of String)
        Dim CurrentName = Controller.TextName
        Dim CurrentIndex = Controller.TextIndex

        If CheckBox_SourceList.Checked Then
            For Each s In ListBox_Source.SelectedItems.OfType(Of String)()
                SourceNames.Add(s)
                SourceNameSet.Add(s)
            Next
        End If
        If CheckBox_TargetList.Checked Then
            For Each s In ListBox_Target.SelectedItems.OfType(Of String)()
                TargetNames.Add(s)
                TargetNameSet.Add(s)
            Next
        End If
        If (CheckBox_SourceCurrent.Checked OrElse CheckBox_TargetCurrent.Checked) AndAlso (SourceNameSet.Contains(CurrentName) OrElse TargetNameSet.Contains(CurrentName)) Then
            MessageDialog.Show("源和目标中有相同的文件。", Controller.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        If SourceNameSet.Intersect(TargetNameSet).Count > 0 Then
            MessageDialog.Show("源和目标中有相同的文件。", Controller.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Dim Dict As New Dictionary(Of String, KeyValuePair(Of String, Integer))
        Dim MainColumn = Columns(MainColumnIndex)
        For Each t In SourceNames
            Dim l = MainColumn.TryGetValue(t)
            If l Is Nothing Then Continue For
            For n = 0 To l.Count - 1
                Dim Text = l.Text(n)
                If Dict.ContainsKey(Text) Then
                    Dict(Text) = New KeyValuePair(Of String, Integer)(t, n)
                Else
                    Dict.Add(Text, New KeyValuePair(Of String, Integer)(t, n))
                End If
            Next
        Next
        If CheckBox_SourceCurrent.Checked Then
            Dim t = CurrentName
            Dim l = MainColumn.TryGetValue(t)
            If l IsNot Nothing Then
                For n = 0 To CurrentIndex - 1
                    Dim Text = l.Text(n)
                    If Dict.ContainsKey(Text) Then
                        Dict(Text) = New KeyValuePair(Of String, Integer)(t, n)
                    Else
                        Dict.Add(Text, New KeyValuePair(Of String, Integer)(t, n))
                    End If
                Next
            End If
        End If

        If CheckBox_TargetCurrent.Checked Then
            Dim t = CurrentName
            Dim l = MainColumn.TryGetValue(t)
            If l IsNot Nothing Then
                For n = CurrentIndex To l.Count - 1
                    Dim Text = l.Text(n)
                    If Dict.ContainsKey(Text) Then
                        Dim Record = Dict(Text)
                        For Each c In Columns
                            If c.IsReadOnly Then Continue For
                            If c Is MainColumn Then Continue For
                            Dim lt = c.TryGetValue(t)
                            If lt Is Nothing Then Continue For
                            Dim ls = c.TryGetValue(Record.Key)
                            If ls Is Nothing Then Continue For
                            lt.Text(n) = ls.Text(Record.Value)
                        Next
                    End If
                Next
            End If
        End If
        For Each t In TargetNames
            Dim l = MainColumn.TryGetValue(t)
            If l Is Nothing Then Continue For
            For n = 0 To l.Count - 1
                Dim Text = l.Text(n)
                If Dict.ContainsKey(Text) Then
                    Dim Record = Dict(Text)
                    For Each c In Columns
                        If c.IsReadOnly Then Continue For
                        If c Is MainColumn Then Continue For
                        Dim lt = c.TryGetValue(t)
                        If lt Is Nothing Then Continue For
                        Dim ls = c.TryGetValue(Record.Key)
                        If ls Is Nothing Then Continue For
                        lt.Text(n) = ls.Text(Record.Value)
                    Next
                End If
            Next
        Next

        Controller.RefreshMainPanel()
        Controller.RefreshGrid()

        MessageDialog.Show("成功执行按模板翻译。", Controller.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class
