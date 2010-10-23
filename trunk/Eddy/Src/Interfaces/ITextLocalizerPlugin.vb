'==========================================================================
'
'  File:        ITextLocalizerPlugin.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 文本本地化工具插件接口
'  Version:     2010.05.17.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Drawing

''' <summary>TextLocalizer的所有插件的接口</summary>
Public Interface ITextLocalizerPlugin
    Inherits IDisposable

End Interface

''' <summary>文本风格</summary>
Public Class TextStyle
    Public Index As Integer
    Public Length As Integer
    Public ForeColor As Color
    Public BackColor As Color
End Class

''' <summary>TextLocalizer的高亮插件接口</summary>
Public Interface ITextLocalizerTextHighlighter
    Inherits ITextLocalizerPlugin

    Function GetTextStyles(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of TextStyle())
End Interface

''' <summary>TextLocalizer的预览框文本格式化插件接口</summary>
Public Interface ITextLocalizerGridTextFormatter
    Inherits ITextLocalizerPlugin

    Function Format(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of String)
End Interface

''' <summary>TextLocalizer的格式插件接口</summary>
Public Interface ITextLocalizerFormatPlugin
    Inherits ITextLocalizerPlugin

    Function GetTextListFactories() As IEnumerable(Of ILocalizationTextListFactory)
End Interface

''' <summary>TextLocalizer的文本默认值翻译插件接口</summary>
Public Interface ITextLocalizerTranslatorPlugin
    Inherits ITextLocalizerPlugin

    Function TranslateText(ByVal SourceColumn As Integer, ByVal TargeColumn As Integer, ByVal Text As String) As String
End Interface
