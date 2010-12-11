'==========================================================================
'
'  File:        ITextLocalizerKeyListenerPlugin.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 键盘监听插件接口
'  Version:     2010.12.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic

''' <summary>控件编号</summary>
Public Enum ControlId
    None = 0
    MainWindow = 1
End Enum

Public Enum KeyEventType
    Down
    Up
End Enum

''' <summary>键盘监听器</summary>
Public Class KeyListener
    Public Source As ControlId
    Public KeyCombination As VirtualKeys()
    Public EventType As KeyEventType
    Public Handler As Action
End Class

''' <summary>键盘监听插件接口</summary>
Public Interface ITextLocalizerKeyListenerPlugin
    Inherits ITextLocalizerPlugin

    Function GetKeyListeners() As IEnumerable(Of KeyListener)
End Interface
