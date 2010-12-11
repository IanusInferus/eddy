'==========================================================================
'
'  File:        ITextLocalizerPlugin.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 数据使用插件接口
'  Version:     2010.12.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Drawing

''' <summary>数据接口</summary>
Public Interface ITextLocalizerData
    ReadOnly Property TextNames As IEnumerable(Of String)
    ReadOnly Property Columns As IEnumerable(Of LocalizationTextProvider)
    ReadOnly Property MainColumnIndex As Integer
End Interface

''' <summary>数据使用插件接口</summary>
Public Interface ITextLocalizerDataPlugin
    Inherits ITextLocalizerPlugin

    Sub InitializeData(ByVal TextLocalizerData As ITextLocalizerData)
End Interface
