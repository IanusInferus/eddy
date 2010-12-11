'==========================================================================
'
'  File:        ILocalizationTextListFactory.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 本地化文本列表工厂接口
'  Version:     2010.12.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic

''' <summary>本地化文本列表工厂接口。</summary>
Public Interface ILocalizationTextListFactory
    ''' <summary>支持的格式。</summary>
    ReadOnly Property SupportedTypes() As IEnumerable(Of String)
    Function List(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String) As IEnumerable(Of String)
    Function Load(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding) As ILocalizationTextList
    Function LoadOrCreate(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding, ByVal Template As ILocalizationTextList, ByVal TranslateText As Func(Of String, String)) As ILocalizationTextList
End Interface
