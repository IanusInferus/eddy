'==========================================================================
'
'  File:        EscapeSequenceHighlighter.vb
'  Location:    Eddy.EscapeSequenceHighlighter <Visual Basic .Net>
'  Description: 文本本地化工具控制符高亮插件
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

Public Class Config
    Public Regex As String = "\{.*?\}"
    Public ForeColor As String = "FF0000FF"
    Public BackColor As String = "FFBFBFFF"
    Public HideInGrid As Boolean = False
End Class

Public Class EscapeSequenceHighlighter
    Inherits TextLocalizerBase
    Implements ITextLocalizerTextHighlighter
    Implements ITextLocalizerGridTextFormatter
    Implements ITextLocalizerControlPlugin

    Private SettingPath As String = "EscapeSequenceHighlighter.locplugin"
    Private Config As Config
    Private EscapeSequenceRegex As Regex
    Private ForeColor As Color
    Private BackColor As Color

    Private WithEvents CheckBox_Multiview_HideEscapeSequence As System.Windows.Forms.CheckBox

    Private Initialized As Boolean = False
    Public Sub New()
        If File.Exists(SettingPath) Then
            Config = Xml.ReadFile(Of Config)(SettingPath)
        Else
            Config = New Config
        End If
        EscapeSequenceRegex = New Regex(Config.Regex, RegexOptions.ExplicitCapture Or RegexOptions.Compiled)
        ForeColor = Color.FromArgb(Integer.Parse(Config.ForeColor, Globalization.NumberStyles.HexNumber))
        BackColor = Color.FromArgb(Integer.Parse(Config.BackColor, Globalization.NumberStyles.HexNumber))

        Me.CheckBox_Multiview_HideEscapeSequence = New System.Windows.Forms.CheckBox
        '
        'CheckBox_Multiview_HideEscapeSequence
        '
        Me.CheckBox_Multiview_HideEscapeSequence.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CheckBox_Multiview_HideEscapeSequence.AutoSize = True
        Me.CheckBox_Multiview_HideEscapeSequence.Location = New System.Drawing.Point(172, 548)
        Me.CheckBox_Multiview_HideEscapeSequence.Name = "CheckBox_Multiview_HideEscapeSequence"
        Me.CheckBox_Multiview_HideEscapeSequence.Size = New System.Drawing.Size(96, 16)
        Me.CheckBox_Multiview_HideEscapeSequence.TabIndex = 1
        Me.CheckBox_Multiview_HideEscapeSequence.Text = "隐藏转义序列"
        Me.CheckBox_Multiview_HideEscapeSequence.UseVisualStyleBackColor = True
        Me.CheckBox_Multiview_HideEscapeSequence.Checked = Config.HideInGrid
        Initialized = True
    End Sub
    Protected Overrides Sub DisposeManagedResource()
        Try
            Xml.WriteFile(SettingPath, UTF16, Config)
        Catch
        End Try
        MyBase.DisposeManagedResource()
    End Sub

    Private Function GetTextStylesForText(ByVal Text As String) As TextStyle()
        Return (From m As Match In EscapeSequenceRegex.Matches(Text) Select (New TextStyle With {.Index = m.Index, .Length = m.Length, .ForeColor = ForeColor, .BackColor = BackColor})).ToArray
    End Function

    Public Function GetTextStyles(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of TextStyle()) Implements Eddy.Interfaces.ITextLocalizerTextHighlighter.GetTextStyles
        Return (From i In Enumerable.Range(0, Columns.Count) Select GetTextStylesForText(FormatedTexts(i))).ToArray
    End Function

    Public Function Format(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of String) Implements ITextLocalizerGridTextFormatter.Format
        If Config.HideInGrid Then
            Return From t In FormatedTexts Select EscapeSequenceRegex.Replace(t, "")
        Else
            Return FormatedTexts
        End If
    End Function

    Public Function GetControlDescriptors() As IEnumerable(Of ControlDescriptor) Implements ITextLocalizerControlPlugin.GetControlDescriptors
        Return New ControlDescriptor() {New ControlDescriptor With {.Control = CheckBox_Multiview_HideEscapeSequence, .Target = ControlId.Grid}}
    End Function

    Private Sub CheckBox_Multiview_HideEscapeSequence_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox_Multiview_HideEscapeSequence.CheckedChanged
        If Not Initialized Then Return
        Config.HideInGrid = CheckBox_Multiview_HideEscapeSequence.Checked
        Controller.RefreshGrid()
    End Sub
End Class
