'==========================================================================
'
'  File:        ITextLocalizerControllerPlugin.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 文本本地化工具使用Controller的插件接口
'  Version:     2010.05.17.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms

''' <summary>控件编号</summary>
Public Enum ControlId
    None = 0
    MainWindow = 1
    MainPanel = 2
    Grid = 3
End Enum

''' <summary>TextLocalizer的使用Controller的插件接口</summary>
Public Interface ITextLocalizerControllerPlugin
    Inherits ITextLocalizerPlugin

    Sub InitializeController(ByVal Receiver As ITextLocalizerApplicationController)
End Interface

''' <summary>TextLocalizer控制器</summary>
Public Interface ITextLocalizerApplicationController
    Event TextNameChanged(ByVal e As EventArgs)
    Event TextIndexChanged(ByVal e As EventArgs)
    Event ColumnSelectionChanged(ByVal e As EventArgs)
    Event KeyDown(ByVal ControlId As ControlId, ByVal e As KeyEventArgs)
    Event KeyPress(ByVal ControlId As ControlId, ByVal e As KeyPressEventArgs)
    Event KeyUp(ByVal ControlId As ControlId, ByVal e As KeyEventArgs)
    Sub RefreshGrid()
    Sub RefreshColumn(ByVal ColumnIndex As Integer)
    Sub RefreshMainPanel()
    Sub FlushLocalizedText()

    ReadOnly Property Form() As Form
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
