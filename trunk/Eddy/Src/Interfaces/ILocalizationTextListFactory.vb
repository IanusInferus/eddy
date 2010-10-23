'==========================================================================
'
'  File:        ILocalizationTextListFactory.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 本地化文本列表工厂接口
'  Version:     2010.10.05.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq

''' <summary>本地化文本列表工厂接口。</summary>
Public Interface ILocalizationTextListFactory
    ''' <summary>支持的格式。</summary>
    ReadOnly Property SupportedTypes() As IEnumerable(Of String)
    Function List(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String) As IEnumerable(Of String)
    Function Load(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding) As ILocalizationTextList
    Function LoadOrCreate(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding, ByVal Template As ILocalizationTextList, ByVal TranslateText As Func(Of String, String)) As ILocalizationTextList
End Interface

''' <summary>本地化文本列表工厂聚合器</summary>
Public Class LocalizationTextListFactoryAggregation
    Implements ILocalizationTextListFactory

    Private TypeToFactory As New Dictionary(Of String, List(Of ILocalizationTextListFactory))

    Public Sub New(ByVal Factories As IEnumerable(Of ILocalizationTextListFactory))
        AddFactories(Factories)
    End Sub

    Public Sub AddFactories(ByVal Factories As IEnumerable(Of ILocalizationTextListFactory))
        For Each f In Factories
            For Each t In f.SupportedTypes
                Dim l As List(Of ILocalizationTextListFactory)
                If TypeToFactory.ContainsKey(t) Then
                    l = TypeToFactory(t)
                Else
                    l = New List(Of ILocalizationTextListFactory)
                    TypeToFactory.Add(t, l)
                End If
                l.Add(f)
            Next
        Next
    End Sub

    Public ReadOnly Property SupportedTypes() As System.Collections.Generic.IEnumerable(Of String) Implements ILocalizationTextListFactory.SupportedTypes
        Get
            Return TypeToFactory.Keys
        End Get
    End Property

    Public Function List(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String) As IEnumerable(Of String) Implements ILocalizationTextListFactory.List
        Dim TextNamesAll As New List(Of String)
        If TypeToFactory.ContainsKey(Type) Then
            For Each f In TypeToFactory(Type).AsEnumerable.Reverse
                Dim TextNames = f.List(ProviderName, Type, Directory, Extension)
                TextNamesAll.AddRange(TextNames)
            Next
        End If
        Return TextNamesAll.ToArray
    End Function

    Public Function Load(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding) As ILocalizationTextList Implements ILocalizationTextListFactory.Load
        If TypeToFactory.ContainsKey(Type) Then
            Dim FirstNormalException As Exception = Nothing
            For Each f In TypeToFactory(Type).AsEnumerable.Reverse
                Try
                    Return f.Load(ProviderName, Type, Directory, Extension, TextName, IsReadOnly, Encoding)
                Catch ex As NotSupportedException
                Catch ex As Exception
                    If FirstNormalException Is Nothing Then
                        FirstNormalException = ex
                    End If
                End Try
            Next
            If FirstNormalException IsNot Nothing Then
                Throw New Exception(Nothing, FirstNormalException)
            End If
        End If
        Throw New NotSupportedException
    End Function

    Public Function LoadOrCreate(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding, ByVal Template As ILocalizationTextList, ByVal TranslateText As Func(Of String, String)) As ILocalizationTextList Implements ILocalizationTextListFactory.LoadOrCreate
        If TypeToFactory.ContainsKey(Type) Then
            Dim FirstNormalException As Exception = Nothing
            For Each f In TypeToFactory(Type).AsEnumerable.Reverse
                Try
                    Return f.LoadOrCreate(ProviderName, Type, Directory, Extension, TextName, IsReadOnly, Encoding, Template, TranslateText)
                Catch ex As NotSupportedException
                Catch ex As Exception
                    If FirstNormalException Is Nothing Then
                        FirstNormalException = ex
                    End If
                End Try
            Next
            If FirstNormalException IsNot Nothing Then
                Throw FirstNormalException
            End If
        End If
        Throw New NotSupportedException
    End Function
End Class
