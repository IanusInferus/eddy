'==========================================================================
'
'  File:        Voice.vb
'  Location:    Eddy.Voice <Visual Basic .Net>
'  Description: 文本本地化工具朗读插件
'  Version:     2012.02.29.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Xml.Linq
Imports System.Reflection
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Mapping
Imports Firefly.Mapping.XmlText
Imports Eddy
Imports Eddy.Interfaces
Imports Eddy.Base

Public Class Config
    Public IgnoreSequence As String = "\{.*?\}"
    Public Voices As VoiceDescriptor()
    Public Platform As Platform = Platform.x86
End Class

Public Enum Platform
    x86
    x64
End Enum

Public Class VoiceDescriptor
    Public LocalizationBoxName As String
    Public TTSName As String
End Class

Public Class Voice
    Inherits TextLocalizerBase
    Implements ITextLocalizerToolStripButtonPlugin
    Implements ITextLocalizerKeyListenerPlugin
    Implements ITextLocalizerConfigurationPlugin

    Private Config As Config
    Public Sub SetConfiguration(ByVal Config As XElement) Implements ITextLocalizerConfigurationPlugin.SetConfiguration
        If Config Is Nothing Then
            Me.Config = New Config With {.Voices = New VoiceDescriptor() {}}
        Else
            Me.Config = (New XmlSerializer).Read(Of Config)(Config)
        End If
        If Me.Config.IgnoreSequence <> "" Then Regex = New Regex(Me.Config.IgnoreSequence, RegexOptions.ExplicitCapture Or RegexOptions.Compiled)
        NameToVoiceName = Me.Config.Voices.ToDictionary(Function(d) d.LocalizationBoxName, Function(d) d.TTSName, StringComparer.OrdinalIgnoreCase)
        Installed = New Dictionary(Of String, Boolean)
    End Sub
    Public Function GetConfiguration() As XElement Implements ITextLocalizerConfigurationPlugin.GetConfiguration
        Return (New XmlSerializer).Write(Me.Config)
    End Function

    Private Regex As Regex
    Private NameToVoiceName As Dictionary(Of String, String)
    Private Installed As Dictionary(Of String, Boolean)

    Protected Overrides Sub DisposeManagedResource()
        If VoiceService IsNot Nothing Then VoiceService.Dispose()
        MyBase.DisposeManagedResource()
    End Sub

    Private ButtonDescriptor As ToolStripButtonDescriptor
    Public Function GetToolStripButtonDescriptors() As IEnumerable(Of ToolStripButtonDescriptor) Implements ITextLocalizerToolStripButtonPlugin.GetToolStripButtonDescriptors
        ButtonDescriptor = New ToolStripButtonDescriptor With {.Image = My.Resources.VoiceStart, .Text = "朗读(F1)", .Click = AddressOf ToolStripButton_Click}
        Return New ToolStripButtonDescriptor() {ButtonDescriptor}
    End Function

    Private LockObject As New Object
    Private Started As Boolean = False
    Private WithEvents VoiceService As IVoiceService
    Private Sub ToolStripButton_Click()
        Dim IsStarted As Boolean
        SyncLock LockObject
            IsStarted = Started
        End SyncLock
        If IsStarted Then
            StopVoice()
        Else
            StartVoice()
        End If
    End Sub
    Private Sub StartVoice()
        If VoiceService Is Nothing Then
            Dim StartDir = GetFileDirectory(Assembly.GetExecutingAssembly().Location)
            Select Case Config.Platform
                Case Platform.x86
                    VoiceService = Rpc.CreateMaster(Of IVoiceService)(GetPath(StartDir, "Eddy.Voice.x86.exe"), Controller.UIThreadAsyncInvoker)
                Case Platform.x64
                    VoiceService = Rpc.CreateMaster(Of IVoiceService)(GetPath(StartDir, "Eddy.Voice.x64.exe"), Controller.UIThreadAsyncInvoker)
                Case Else
                    Throw New InvalidDataException
            End Select
        End If
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
        Dim VoiceName As String = Nothing
        If NameToVoiceName.ContainsKey(tp.Name) Then
            VoiceName = NameToVoiceName(tp.Name)
        End If
        If VoiceName Is Nothing Then
            VoiceService.SpeakAsync(Text)
            Return
        End If
        If Not Installed.ContainsKey(VoiceName) Then
            If VoiceService.IsVoiceInstalled(VoiceName) Then
                Installed.Add(VoiceName, True)
            Else
                Installed.Add(VoiceName, False)
                Controller.ShowInfo("无法找到TTS引擎:", NameToVoiceName(tp.Name))
            End If
        End If
        If Installed(VoiceName) Then
            VoiceService.SpeakAsync(VoiceName, Text)
        Else
            VoiceService.SpeakAsync(Text)
        End If
    End Sub
    Private Sub StopVoice()
        If VoiceService IsNot Nothing Then
            VoiceService.SpeakAsyncCancelAll()
        End If
    End Sub
    Private Sub Synth_SpeakStarted() Handles VoiceService.SpeakStarted
        ButtonDescriptor.ImageChanged.Raise(My.Resources.VoiceStop)
        ButtonDescriptor.TextChanged.Raise("停止朗读(Esc)")
        SyncLock LockObject
            Started = True
        End SyncLock
    End Sub
    Private Sub Synth_SpeakCompleted() Handles VoiceService.SpeakCompleted
        ButtonDescriptor.ImageChanged.Raise(My.Resources.VoiceStart)
        ButtonDescriptor.TextChanged.Raise("朗读(F1)")
        SyncLock LockObject
            Started = False
        End SyncLock
    End Sub

    Public Function GetKeyListeners() As IEnumerable(Of KeyListener) Implements ITextLocalizerKeyListenerPlugin.GetKeyListeners
        Return New KeyListener() {
            New KeyListener With {.Source = ControlId.MainWindow, .KeyCombination = {VirtualKeys.F1}, .EventType = KeyEventType.Up, .Handler = AddressOf StartVoice},
            New KeyListener With {.Source = ControlId.MainWindow, .KeyCombination = {VirtualKeys.Escape}, .EventType = KeyEventType.Up, .Handler = AddressOf StopVoice}
        }
    End Function
End Class
