'==========================================================================
'
'  File:        WQSGTextListFactory.vb
'  Location:    Eddy.WQSG <Visual Basic .Net>
'  Description: 本地化文本列表工厂接口与默认实现的WQSG文本支持
'  Version:     2010.12.14.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Eddy.Interfaces

Partial Public Class WQSGPlugin
    Implements ILocalizationTextListFactory

    Private SupportedTypesValue As String() = {"WQSGText", "WQSGIndex"}
    Public ReadOnly Property SupportedTypes() As IEnumerable(Of String) Implements ILocalizationTextListFactory.SupportedTypes
        Get
            Return SupportedTypesValue.AsEnumerable()
        End Get
    End Property
    Public Function List(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String) As IEnumerable(Of String) Implements ILocalizationTextListFactory.List
        If Type <> "WQSGIndex" Then
            If Not IO.Directory.Exists(Directory) Then Return New String() {}
            Return (From f In IO.Directory.GetFiles(Directory, "*", SearchOption.AllDirectories) Select FileName = GetRelativePath(f, Directory) Where IsMatchFileMask(FileName, "*." & Extension)).OrderBy(Function(s) s, StringComparer.CurrentCultureIgnoreCase).ToArray
        Else
            Dim Column = From c In Columns Where c.Name.Equals(Directory, StringComparison.OrdinalIgnoreCase)
            If Column.Count <> 1 Then Throw New InvalidOperationException
            Dim BaseColumn = Column.First
            If ProviderName = BaseColumn.Name Then Throw New InvalidOperationException
            If BaseColumn.Type <> "WQSGText" Then Throw New InvalidOperationException
            Return List(BaseColumn.Name, BaseColumn.Type, BaseColumn.Directory, BaseColumn.Extension)
        End If
    End Function
    Public Function Load(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding) As ILocalizationTextList Implements ILocalizationTextListFactory.Load
        If Type.Equals("WQSGText", StringComparison.OrdinalIgnoreCase) Then
            Dim Path = GetPath(Directory, TextName & "." & Extension)
            If IsReadOnly OrElse ProviderName = Columns(MainColumnIndex).Name OrElse Columns(MainColumnIndex).Type = "WQSGIndex" Then Return New WQSGTextList(Path, IsReadOnly, Encoding)
            Dim Template = Columns(MainColumnIndex).Item(TextName)
            Dim TemplateWQSG = TryCast(Template, WQSGTextList)
            If TemplateWQSG Is Nothing Then Return New WQSGTextList(Path, IsReadOnly, Encoding)
            Return WQSGTextList.LoadAndRepair(TemplateWQSG, Path, Encoding)
        End If
        If Type.Equals("WQSGIndex", StringComparison.OrdinalIgnoreCase) Then
            Dim Column = From c In Columns Where c.Name.Equals(Directory, StringComparison.OrdinalIgnoreCase)
            If Column.Count <> 1 Then Throw New InvalidOperationException
            Dim BaseColumn = Column.First
            If ProviderName = BaseColumn.Name Then Throw New InvalidOperationException
            If BaseColumn.Type <> "WQSGText" Then Throw New InvalidOperationException
            Return New WQSGIndexList(BaseColumn.Item(TextName))
        End If
        Throw New NotSupportedException
    End Function
    Public Function LoadOrCreate(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding, ByVal Template As ILocalizationTextList, ByVal TranslateText As Func(Of String, String)) As ILocalizationTextList Implements ILocalizationTextListFactory.LoadOrCreate
        If Type.Equals("WQSGText", StringComparison.OrdinalIgnoreCase) Then
            Dim Path = GetPath(Directory, TextName & "." & Extension)
            If Encoding Is Nothing Then Encoding = UTF16
            If IO.File.Exists(Path) Then Return Load(ProviderName, Type, Directory, Extension, TextName, IsReadOnly, Encoding)
            If Not IO.Directory.Exists(GetFileDirectory(Path)) Then IO.Directory.CreateDirectory(GetFileDirectory(Path))
            If IsReadOnly Then Throw New InvalidOperationException
            Dim TemplateWQSG = TryCast(Template, WQSGTextList)
            If TemplateWQSG Is Nothing Then Throw New NotSupportedException
            WQSG.WriteFile(Path, Encoding, From t In Enumerable.Range(0, Template.Count) Select New WQSG.Triple With {.Offset = TemplateWQSG.Item(t).Offset, .Length = TemplateWQSG.Item(t).Length, .Text = TranslateText(TemplateWQSG.Item(t).Text)})
            Return New WQSGTextList(Path, False, Encoding)
        End If
        If Type.Equals("WQSGIndex", StringComparison.OrdinalIgnoreCase) Then
            Dim Column = From c In Columns Where c.Name.Equals(Directory, StringComparison.OrdinalIgnoreCase)
            If Column.Count <> 1 Then Throw New InvalidOperationException
            Dim BaseColumn = Column.First
            If ProviderName = BaseColumn.Name Then Throw New InvalidOperationException
            If BaseColumn.Type <> "WQSGText" Then Throw New InvalidOperationException
            Return New WQSGIndexList(BaseColumn.LoadOrCreateItem(TextName, Template, TranslateText))
        End If
        Throw New NotSupportedException
    End Function
End Class

Public Class WQSGTextList
    Implements ILocalizationTextList

    Private Path As String
    Private IsReadOnlyValue As Boolean
    Private Encoding As System.Text.Encoding
    Private IsModifiedValue As Boolean
    Private Values As List(Of WQSG.Triple)

    Public Shared Controller As ITextLocalizerApplicationController

    Public Sub New(ByVal Path As String, ByVal IsReadOnly As Boolean, Optional ByVal Encoding As System.Text.Encoding = Nothing)
        Me.Path = Path
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        If Encoding Is Nothing Then
            Me.Encoding = Txt.GetEncoding(Path)
        Else
            Me.Encoding = Encoding
        End If
        Values = WQSG.ReadFile(Path, Me.Encoding).ToList
    End Sub
    Public Sub New(ByVal IsReadOnly As Boolean, ByVal Count As Integer)
        Me.Path = ""
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        Values = (From i In Enumerable.Range(0, Count) Select New WQSG.Triple With {.Offset = 0, .Length = 0, .Text = ""}).ToList
    End Sub
    Public Shared Function LoadAndRepair(ByVal Template As WQSGTextList, ByVal Path As String, Optional ByVal Encoding As System.Text.Encoding = Nothing) As WQSGTextList
        If Encoding Is Nothing Then Encoding = Txt.GetEncoding(Path)
        Dim BaseValues = Template.Values
        Dim Values = WQSG.ReadFile(Path, Encoding)
        If Values.Count = BaseValues.Count Then Return New WQSGTextList(Path, False, Encoding)

        If Controller.ShowYesNoQuestion("文本{0}条数不对，尝试修复吗？".Formats(Path)) Then
        Else
            Throw New InvalidDataException
        End If

        Dim RepeatDict As New Dictionary(Of Integer, Integer)
        For Each v In Values
            If RepeatDict.ContainsKey(v.Offset) Then
                RepeatDict(v.Offset) += 1
            Else
                RepeatDict.Add(v.Offset, 1)
            End If
        Next
        Dim Repeated = RepeatDict.Where(Function(v) v.Value > 1).ToArray()
        If Repeated.Length > 0 Then
            Controller.ShowError("修复失败。下列索引多次出现。", String.Join(CrLf, Repeated.Select(Function(v) v.Key.ToString("X8")).ToArray))
            Throw New InvalidDataException
        End If

        Dim Dict = Values.ToDictionary(Function(v) v.Offset)
        Dim NewValues = New WQSG.Triple(BaseValues.Count - 1) {}
        Dim AddValues As New List(Of Integer)
        For n = 0 To NewValues.Length - 1
            Dim t As New WQSG.Triple With {.Offset = BaseValues(n).Offset}
            If Dict.ContainsKey(t.Offset) Then
                Dim d = Dict(t.Offset)
                Dict.Remove(t.Offset)
                t.Length = d.Length
                t.Text = d.Text
            Else
                t.Length = 0
                t.Text = ""
                AddValues.Add(t.Offset)
            End If
            NewValues(n) = t
        Next
        If Dict.Count > 0 Then
            If Controller.ShowYesNoQuestion("修复可能，需要删除下列索引，修复吗？", String.Join(CrLf, Dict.Keys.Select(Function(v) v.ToString("X8")).ToArray)) Then
            Else
                Throw New InvalidDataException
            End If
        End If
        If AddValues.Count > 0 Then
            If Controller.ShowYesNoQuestion("修复可能。需要增加下列索引，修复吗？", String.Join(CrLf, AddValues.Select(Function(v) v.ToString("X8")).ToArray)) Then
            Else
                Throw New InvalidDataException
            End If
        End If
        WQSG.WriteFile(Path, Encoding, NewValues)
        Return New WQSGTextList(Path, False, Encoding)
    End Function

    Public ReadOnly Property Count() As Integer Implements ILocalizationTextList.Count
        Get
            Return Values.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements ILocalizationTextList.IsReadOnly
        Get
            Return IsReadOnlyValue
        End Get
    End Property

    Public ReadOnly Property IsModified() As Boolean Implements ILocalizationTextList.IsModified
        Get
            Return IsModifiedValue
        End Get
    End Property

    Public Property Item(ByVal Index As Integer) As WQSG.Triple
        Get
            Return Values(Index)
        End Get
        Set(ByVal Value As WQSG.Triple)
            If IsReadOnlyValue Then Throw New InvalidOperationException
            If Index < 0 OrElse Index >= Values.Count Then Throw New ArgumentOutOfRangeException
            IsModifiedValue = True
            Values(Index) = Value
        End Set
    End Property

    Public Property Text(ByVal Index As Integer) As String Implements ILocalizationTextList.Text
        Get
            Return Values(Index).Text
        End Get
        Set(ByVal Value As String)
            If IsReadOnlyValue Then Throw New InvalidOperationException
            If Index < 0 OrElse Index >= Values.Count Then Throw New ArgumentOutOfRangeException
            IsModifiedValue = True
            Values(Index).Text = Value
        End Set
    End Property

    Public Sub Flush() Implements ILocalizationTextList.Flush
        If Path = "" Then Return
        If IsModifiedValue Then
            WQSG.WriteFile(Path, Encoding, Values)
            IsModifiedValue = False
        End If
    End Sub
End Class

Public Class WQSGIndexList
    Implements ILocalizationTextList

    Private BaseTextList As WQSGTextList

    Public Sub New(ByVal BaseTextList As WQSGTextList)
        Me.BaseTextList = BaseTextList
    End Sub

    Public ReadOnly Property Count() As Integer Implements ILocalizationTextList.Count
        Get
            Return BaseTextList.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean Implements ILocalizationTextList.IsReadOnly
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property IsModified() As Boolean Implements ILocalizationTextList.IsModified
        Get
            Return False
        End Get
    End Property

    Public Property Item(ByVal Index As Integer) As WQSG.Triple
        Get
            Return BaseTextList.Item(Index)
        End Get
        Set(ByVal Value As WQSG.Triple)
            Throw New InvalidOperationException
        End Set
    End Property

    Public Property Text(ByVal Index As Integer) As String Implements ILocalizationTextList.Text
        Get
            Dim v = BaseTextList.Item(Index)
            Return "{0:X8},{1}".Formats(v.Offset, v.Length)
        End Get
        Set(ByVal Value As String)
            Throw New InvalidOperationException
        End Set
    End Property

    Public Sub Flush() Implements ILocalizationTextList.Flush
    End Sub
End Class
