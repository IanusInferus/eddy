'==========================================================================
'
'  File:        ITextLocalizerPlugin.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 插件接口
'  Version:     2010.12.29.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Xml.Linq

''' <summary>所有插件的接口</summary>
Public Interface ITextLocalizerPlugin
    Inherits IDisposable

End Interface

''' <summary>事件源</summary>
Public Class EventSource
    Public Event Value()
    Public Sub Raise()
        RaiseEvent Value()
    End Sub
End Class

''' <summary>事件源</summary>
Public Class EventSource(Of T)
    Public Event Value(ByVal Parameters As T)
    Public Sub Raise(ByVal Parameters As T)
        RaiseEvent Value(Parameters)
    End Sub
End Class

''' <summary>文本风格</summary>
Public Class TextStyle
    Public Index As Integer
    Public Length As Integer
    Public ForeColor As Color
    Public BackColor As Color
End Class

''' <summary>高亮插件接口</summary>
Public Interface ITextLocalizerTextHighlighter
    Inherits ITextLocalizerPlugin

    Function GetTextStyles(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of TextStyle())
End Interface

''' <summary>预览框文本格式化插件接口</summary>
Public Interface ITextLocalizerGridTextFormatter
    Inherits ITextLocalizerPlugin

    Function Format(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of String)
End Interface

''' <summary>格式插件接口</summary>
Public Interface ITextLocalizerFormatPlugin
    Inherits ITextLocalizerPlugin

    Function GetTextListFactories() As IEnumerable(Of ILocalizationTextListFactory)
End Interface

''' <summary>文本默认值翻译插件接口</summary>
Public Interface ITextLocalizerTranslatorPlugin
    Inherits ITextLocalizerPlugin

    Function TranslateText(ByVal SourceColumn As Integer, ByVal TargeColumn As Integer, ByVal Text As String) As String
End Interface

''' <summary>配置接口</summary>
Public Interface ITextLocalizerConfigurationPlugin
    Inherits ITextLocalizerPlugin

    ''' <summary>如果文件不存在，将传入Nothing</summary>
    Sub SetConfiguration(ByVal Config As XElement)

    ''' <summary>可传出Nothing表示不做修改</summary>
    Function GetConfiguration() As XElement
End Interface
