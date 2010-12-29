'==========================================================================
'
'  File:        TextLocalizerData.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 数据
'  Version:     2010.12.29.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Eddy.Interfaces

Public Class TextLocalizerData
    Implements ITextLocalizerData

    Public ApplicationName As String = "漩涡文本本地化工具(Firefly.Eddy)"

    Public CurrentProject As LocalizationProject
    Public CurrentProjectFilePath As String
    Public CurrentUserFilePath As String

    Public Columns As New List(Of LocalizationTextProvider)
    Public NameToColumn As New Dictionary(Of String, Integer)
    Public TextNames As New List(Of String)
    Public TextNameDict As New Dictionary(Of String, Integer)

    Public Plugins As New List(Of ITextLocalizerPlugin)
    Public TextHighlighters As New List(Of ITextLocalizerTextHighlighter)
    Public GridTextFormatters As New List(Of ITextLocalizerGridTextFormatter)
    Public ToolStripButtonPlugins As New List(Of ITextLocalizerToolStripButtonPlugin)
    Public FormatPlugins As New List(Of ITextLocalizerFormatPlugin)
    Public TranslatorPlugins As New List(Of ITextLocalizerTranslatorPlugin)
    Public KeyListenerPlugins As New List(Of ITextLocalizerKeyListenerPlugin)
    Public ConfigurationPlugins As New List(Of ITextLocalizerConfigurationPlugin)
    Public UserInterfacePlugins As New List(Of ITextLocalizerUserInterfacePlugin)

    Public Factory As ILocalizationTextListFactory

    Public Sub New()
    End Sub

    Private ReadOnly Property TextNamesInterface As IEnumerable(Of String) Implements ITextLocalizerData.TextNames
        Get
            Return TextNames
        End Get
    End Property
    Private ReadOnly Property ColumnsInterface As IEnumerable(Of LocalizationTextProvider) Implements ITextLocalizerData.Columns
        Get
            Return Columns
        End Get
    End Property
    Public ReadOnly Property MainColumnIndex As Integer Implements ITextLocalizerData.MainColumnIndex
        Get
            If NameToColumn.ContainsKey(CurrentProject.MainLocalizationTextBox) Then
                Return NameToColumn(CurrentProject.MainLocalizationTextBox)
            End If
            Dim i As Integer
            If Not Integer.TryParse(CurrentProject.MainLocalizationTextBox, i) Then Throw New InvalidDataException("MainLocalizationTextBox")
            If i < 0 OrElse i >= Columns.Count Then Throw New InvalidDataException("MainLocalizationTextBox")
            Return i
        End Get
    End Property
End Class
