﻿'==========================================================================
'
'  File:        ITextLocalizerControllerPlugin.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 控制器使用插件接口
'  Version:     2010.12.13.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.ComponentModel

''' <summary>窗口句柄引用</summary>
Public Class WindowReference
    Public Handle As IntPtr
End Class

''' <summary>控制器使用插件接口</summary>
Public Interface ITextLocalizerControllerPlugin
    Inherits ITextLocalizerPlugin

    Sub InitializeController(ByVal Receiver As ITextLocalizerApplicationController)
End Interface

''' <summary>控制器</summary>
Public Interface ITextLocalizerApplicationController
    Event TextNameChanged()
    Event TextIndexChanged()
    Event ColumnSelectionChanged()
    Sub RefreshGrid()
    Sub RefreshColumn(ByVal ColumnIndex As Integer)
    Sub RefreshMainPanel()
    Sub FlushLocalizedText()

    ReadOnly Property MainWindow() As WindowReference
    ReadOnly Property UIThreadInvoker As Action(Of Action)
    ReadOnly Property ApplicationName() As String
    Property TextName() As String
    Property TextIndex() As Integer
    Property TextIndices() As IEnumerable(Of Integer)
    Property ColumnIndex() As Integer
    Property SelectionStart() As Integer
    Property SelectionLength() As Integer
    Property Text(ByVal ColumnIndex As Integer) As String
    Sub ScrollToCaret(ByVal ColumnIndex As Integer)
End Interface