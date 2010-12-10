'==========================================================================
'
'  File:        Voice.vb
'  Location:    Eddy.Voice <Visual Basic .Net>
'  Description: 文本本地化工具控制符高亮插件
'  Version:     2010.12.10.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports System.Speech
Imports System.Speech.Synthesis
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Setting
Imports Eddy.Interfaces

Public Class Config
    Public IgnoreSequence As String = "\{.*?\}"
    Public Voices As VoiceDescriptor()
End Class

Public Class VoiceDescriptor
    Public LocalizationBoxName As String
    Public TTSName As String
End Class

Public Class Voice
    Inherits TextLocalizerBase
    Implements ITextLocalizerToolStripButtonPlugin

    Private SettingPath As String = "Voice.locplugin"
    Private Config As Config
    Private Regex As Regex
    Private NameToVoiceName As Dictionary(Of String, String)

    Public Sub New()
        If File.Exists(SettingPath) Then
            Config = Xml.ReadFile(Of Config)(SettingPath)
        Else
            Config = New Config With {.Voices = New VoiceDescriptor() {}}
        End If
        If Config.IgnoreSequence <> "" Then Regex = New Regex(Config.IgnoreSequence, RegexOptions.ExplicitCapture Or RegexOptions.Compiled)
        NameToVoiceName = Config.Voices.ToDictionary(Function(d) d.LocalizationBoxName, Function(d) d.TTSName, StringComparer.OrdinalIgnoreCase)
    End Sub
    Protected Overrides Sub DisposeManagedResource()
        Try
            Xml.WriteFile(SettingPath, UTF16, Config)
        Catch
        End Try
        If Synth IsNot Nothing Then Synth.Dispose()
        MyBase.DisposeManagedResource()
    End Sub

    Public Function GetToolStripButtonDescriptors() As IEnumerable(Of ToolStripButtonDescriptor) Implements ITextLocalizerToolStripButtonPlugin.GetToolStripButtonDescriptors
        Return New ToolStripButtonDescriptor() {New ToolStripButtonDescriptor With {.Image = Nothing, .Text = "朗读(F1)", .Click = AddressOf ToolStripButton_Click}}
    End Function

    Private Synth As SpeechSynthesizer
    Private InstalledVoices As Dictionary(Of String, VoiceInfo)
    Private Sub ToolStripButton_Click()
        If Synth Is Nothing Then
            Try
                Synth = New SpeechSynthesizer()
            Catch
                Return
            End Try
            InstalledVoices = (From v In Synth.GetInstalledVoices Select v.VoiceInfo).ToDictionary(Function(v) v.Name, Function(v) v)
        End If
        If InstalledVoices.Count = 0 Then Return
        Dim ColumnIndex = Controller.ColumnIndex
        Dim tp = Columns(ColumnIndex)
        Dim Text As String
        Dim SelectionStart = Controller.SelectionStart
        Dim SelectionLength = Controller.SelectionLength
        If SelectionLength = 0 Then
            Text = Controller.Text(ColumnIndex)
        Else
            Text = Controller.Text(ColumnIndex).Substring(SelectionStart, SelectionLength)
        End If
        If Regex IsNot Nothing Then
            Text = Regex.Replace(Text, "")
        End If
        Dim Voice As VoiceInfo = Nothing
        If NameToVoiceName.ContainsKey(tp.Name) AndAlso InstalledVoices.ContainsKey(NameToVoiceName(tp.Name)) Then
            Voice = InstalledVoices(NameToVoiceName(tp.Name))
        End If
        If Voice IsNot Nothing Then
            Dim p As New PromptBuilder
            p.StartVoice(Voice)
            p.AppendText(Text)
            p.EndVoice()
            Synth.SpeakAsync(p)
        Else
            Synth.SpeakAsync(Text)
        End If
    End Sub

    Private Sub Application_KeyDown(ByVal ControlId As ControlId, ByVal e As KeyEventArgs) Handles Controller.KeyDown
        Select Case e.KeyData
            Case Keys.F1
                ToolStripButton_Click()
            Case Keys.Escape
                If Synth IsNot Nothing Then
                    Synth.SpeakAsyncCancelAll()
                End If
            Case Else
                Return
        End Select
        e.Handled = True
    End Sub
End Class
