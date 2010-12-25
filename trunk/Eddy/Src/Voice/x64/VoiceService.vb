'==========================================================================
'
'  File:        IVoiceService.vb
'  Location:    Eddy.Voice <Visual Basic .Net>
'  Description: 文本本地化工具朗读插件服务实现
'  Version:     2010.12.18.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Speech
Imports System.Speech.Synthesis
Imports Eddy.Voice

Public NotInheritable Class VoiceService
    Implements IVoiceService

    Public Sub New()
    End Sub

    Private WithEvents SynthValue As SpeechSynthesizer
    Private ReadOnly Property Synth As SpeechSynthesizer
        Get
            If SynthValue Is Nothing Then
                SynthValue = New SpeechSynthesizer()
            End If
            Return SynthValue
        End Get
    End Property
    Private InstalledVoicesValue As Dictionary(Of String, VoiceInfo)
    Private ReadOnly Property InstalledVoices As Dictionary(Of String, VoiceInfo)
        Get
            If InstalledVoicesValue Is Nothing Then
                InstalledVoicesValue = (From v In Synth.GetInstalledVoices Select v.VoiceInfo).ToDictionary(Function(v) v.Name, Function(v) v)
                If InstalledVoicesValue.Count = 0 Then Throw New InvalidOperationException("没有找到已安装的TTS引擎")
            End If
            Return InstalledVoicesValue
        End Get
    End Property

    Public Function IsVoiceInstalled(ByVal VoiceName As String) As Boolean Implements IVoiceService.IsVoiceInstalled
        Return InstalledVoices.ContainsKey(VoiceName)
    End Function

    Public Sub SpeakAsync(ByVal VoiceName As String, ByVal Text As String) Implements IVoiceService.SpeakAsync
        If IsVoiceInstalled(VoiceName) Then
            Dim Voice = InstalledVoices(VoiceName)
            Dim p As New PromptBuilder
            p.StartVoice(Voice)
            p.AppendText(Text)
            p.EndVoice()
            Synth.SpeakAsync(p)
        Else
            Synth.SpeakAsync(Text)
        End If
    End Sub

    Public Sub SpeakAsync(ByVal Text As String) Implements IVoiceService.SpeakAsync
        Synth.SpeakAsync(Text)
    End Sub

    Public Sub SpeakAsyncCancelAll() Implements IVoiceService.SpeakAsyncCancelAll
        Synth.SpeakAsyncCancelAll()
    End Sub

    Public Event SpeakStarted() Implements IVoiceService.SpeakStarted
    Public Event SpeakCompleted() Implements IVoiceService.SpeakCompleted
    Private Sub SynthValue_SpeakStarted(ByVal sender As Object, ByVal e As SpeakStartedEventArgs) Handles SynthValue.SpeakStarted
        RaiseEvent SpeakStarted()
    End Sub
    Private Sub SynthValue_SpeakCompleted(ByVal sender As Object, ByVal e As SpeakCompletedEventArgs) Handles SynthValue.SpeakCompleted
        RaiseEvent SpeakCompleted()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If SynthValue IsNot Nothing Then
            SynthValue.Dispose()
            SynthValue = Nothing
        End If
        If InstalledVoicesValue IsNot Nothing Then
            InstalledVoicesValue = Nothing
        End If
    End Sub
End Class
