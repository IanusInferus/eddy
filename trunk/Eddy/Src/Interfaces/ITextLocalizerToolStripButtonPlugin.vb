'==========================================================================
'
'  File:        ITextLocalizerToolStripButtonPlugin.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 文本本地化工具工具条按钮插件接口
'  Version:     2010.12.10.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Drawing

''' <summary>工具条按钮描述</summary>
Public Class ToolStripButtonDescriptor
    Public Image As Image
    Public Text As String
    Public Click As Action
End Class

''' <summary>TextLocalizer的工具条按钮接口</summary>
Public Interface ITextLocalizerToolStripButtonPlugin
    Inherits ITextLocalizerPlugin

    Function GetToolStripButtonDescriptors() As IEnumerable(Of ToolStripButtonDescriptor)
End Interface
