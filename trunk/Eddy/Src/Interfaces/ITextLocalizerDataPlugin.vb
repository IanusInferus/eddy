'==========================================================================
'
'  File:        ITextLocalizerPlugin.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 文本本地化工具插件使用数据接口
'  Version:     2010.05.17.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Drawing

''' <summary>TextLocalizer的插件使用数据的接口</summary>
Public Interface ITextLocalizerData
    ReadOnly Property TextNames As IEnumerable(Of String)
    ReadOnly Property Columns As IEnumerable(Of LocalizationTextProvider)
    ReadOnly Property MainColumnIndex As Integer
End Interface

''' <summary>TextLocalizer的所有使用数据的插件接口</summary>
Public Interface ITextLocalizerDataPlugin
    Inherits ITextLocalizerPlugin

    Sub InitializeData(ByVal TextLocalizerData As ITextLocalizerData)
End Interface
