'==========================================================================
'
'  File:        ITextLocalizerControlPlugin.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 文本本地化工具控件插件接口
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
    ToolStrip = 4
End Enum

''' <summary>控件描述</summary>
Public Class ControlDescriptor
    Public Control As Object
    Public Target As ControlId
End Class

''' <summary>TextLocalizer的控件插件接口</summary>
Public Interface ITextLocalizerControlPlugin
    Inherits ITextLocalizerControllerPlugin

    Function GetControlDescriptors() As IEnumerable(Of ControlDescriptor)
End Interface

''' <summary>控件容器</summary>
Public Enum TextLocalizerAction
    None = 0
    RefreshGrid = 1
End Enum
