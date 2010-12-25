'==========================================================================
'
'  File:        IVoiceService.vb
'  Location:    Eddy.Voice <Visual Basic .Net>
'  Description: 文本本地化工具朗读插件服务接口
'  Version:     2010.12.18.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System

Public Interface IVoiceService
    Inherits IDisposable

    Function IsVoiceInstalled(ByVal VoiceName As String) As Boolean
    Sub SpeakAsync(ByVal VoiceName As String, ByVal Text As String)
    Sub SpeakAsync(ByVal Text As String)
    Sub SpeakAsyncCancelAll()
    Event SpeakStarted()
    Event SpeakCompleted()
End Interface
