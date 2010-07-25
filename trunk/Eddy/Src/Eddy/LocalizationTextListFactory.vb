'==========================================================================
'
'  File:        LocalizationTextListFactory.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 本地化文本列表工厂默认实现
'  Version:     2010.07.25.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.IO
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Firefly.Glyphing
Imports Firefly.Project

''' <summary>本地化文本列表工厂接口的默认实现，支持PlainText、AgemoText、LOC这五种格式。</summary>
Public Class LocalizationTextListFactory
    Implements ILocalizationTextListFactory

    Public LOCTexter As ITexter(Of LOC) = New LOCTexter

    Private SupportedTypesValue As String() = {"RawText", "PlainText", "AgemoText", "LOC"}
    Public ReadOnly Property SupportedTypes() As IEnumerable(Of String) Implements ILocalizationTextListFactory.SupportedTypes
        Get
            Return SupportedTypesValue.AsEnumerable()
        End Get
    End Property
    Public Function List(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String) As IEnumerable(Of String) Implements ILocalizationTextListFactory.List
        Return (From f In IO.Directory.GetFiles(Directory, "*", SearchOption.AllDirectories) Select FileName = GetRelativePath(f, Directory) Where IsMatchFileMask(FileName, "*." & Extension)).OrderBy(Function(s) s, StringComparer.CurrentCultureIgnoreCase).ToArray
    End Function
    Public Function Load(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding) As ILocalizationTextList Implements ILocalizationTextListFactory.Load
        Dim Path = GetPath(Directory, TextName & "." & Extension)
        If Type.Equals("RawText", StringComparison.OrdinalIgnoreCase) Then
            Return New RawTextList(Path, IsReadOnly, Encoding)
        End If
        If Type.Equals("PlainText", StringComparison.OrdinalIgnoreCase) Then
            Return New PlainTextList(Path, IsReadOnly, Encoding)
        End If
        If Type.Equals("AgemoText", StringComparison.OrdinalIgnoreCase) Then
            Return New AgemoTextList(Path, IsReadOnly, Encoding)
        End If
        If Type.Equals("LOC", StringComparison.OrdinalIgnoreCase) Then
            Return New LOCList(Path, IsReadOnly, Encoding, LOCTexter)
        End If
        Throw New NotSupportedException
    End Function
    Public Function LoadOrCreate(ByVal ProviderName As String, ByVal Type As String, ByVal Directory As String, ByVal Extension As String, ByVal TextName As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding, ByVal Template As ILocalizationTextList, ByVal TranslateText As Func(Of String, String)) As ILocalizationTextList Implements ILocalizationTextListFactory.LoadOrCreate
        Dim Path = GetPath(Directory, TextName & "." & Extension)
        If Encoding Is Nothing Then Encoding = UTF16
        If IO.File.Exists(Path) Then Return Load(ProviderName, Type, Directory, Extension, TextName, IsReadOnly, Encoding)
        If Not IO.Directory.Exists(Directory) Then IO.Directory.CreateDirectory(Directory)
        If Type.Equals("RawText", StringComparison.OrdinalIgnoreCase) Then
            If IsReadOnly Then Throw New InvalidOperationException
            If Template.Count <> 1 Then Throw New InvalidOperationException
            Txt.WriteFile(Path, Encoding, (From t In Enumerable.Range(0, Template.Count) Select TranslateText(Template.Text(t))).Single)
            Return New RawTextList(Path, False, Encoding)
        End If
        If Type.Equals("PlainText", StringComparison.OrdinalIgnoreCase) Then
            If IsReadOnly Then Throw New InvalidOperationException
            Plain.WriteFile(Path, Encoding, From t In Enumerable.Range(0, Template.Count) Select TranslateText(Template.Text(t)))
            Return New PlainTextList(Path, False, Encoding)
        End If
        If Type.Equals("AgemoText", StringComparison.OrdinalIgnoreCase) Then
            If IsReadOnly Then Throw New InvalidOperationException
            Agemo.WriteFile(Path, Encoding, From t In Enumerable.Range(0, Template.Count) Select TranslateText(Template.Text(t)))
            Return New AgemoTextList(Path, False, Encoding)
        End If
        If Type.Equals("LOC", StringComparison.OrdinalIgnoreCase) Then
            Return New LOCList(IsReadOnly, Template.Count, LOCTexter)
        End If
        Throw New NotSupportedException
    End Function
End Class

Public Class RawTextList
    Implements ILocalizationTextList

    Private Path As String
    Private IsReadOnlyValue As Boolean
    Private Encoding As System.Text.Encoding
    Private IsModifiedValue As Boolean
    Private Value As String

    Public Sub New(ByVal Path As String, ByVal IsReadOnly As Boolean, Optional ByVal Encoding As System.Text.Encoding = Nothing)
        Me.Path = Path
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        If Encoding Is Nothing Then
            Me.Encoding = Txt.GetEncoding(Path)
        Else
            Me.Encoding = Encoding
        End If
        Value = Txt.ReadFile(Path, Me.Encoding)
    End Sub
    Public Sub New(ByVal IsReadOnly As Boolean, ByVal Count As Integer)
        Me.Path = ""
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        Value = ""
    End Sub

    Public ReadOnly Property Count() As Integer Implements ILocalizationTextList.Count
        Get
            Return 1
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

    Public Property Item(ByVal Index As Integer) As String
        Get
            If Index <> 0 Then Throw New ArgumentOutOfRangeException
            Return Value
        End Get
        Set(ByVal Value As String)
            If IsReadOnlyValue Then Throw New InvalidOperationException
            If Index <> 0 Then Throw New ArgumentOutOfRangeException
            IsModifiedValue = True
            Me.Value = Value
        End Set
    End Property

    Public Property Text(ByVal Index As Integer) As String Implements ILocalizationTextList.Text
        Get
            Return Item(Index)
        End Get
        Set(ByVal Value As String)
            Item(Index) = Value
        End Set
    End Property

    Public Sub Flush() Implements ILocalizationTextList.Flush
        If Path = "" Then Return
        If IsModifiedValue Then
            Txt.WriteFile(Path, Encoding, Value)
            IsModifiedValue = False
        End If
    End Sub
End Class

Public Class PlainTextList
    Implements ILocalizationTextList

    Private Path As String
    Private IsReadOnlyValue As Boolean
    Private Encoding As System.Text.Encoding
    Private IsModifiedValue As Boolean
    Private Values As List(Of String)

    Public Sub New(ByVal Path As String, ByVal IsReadOnly As Boolean, Optional ByVal Encoding As System.Text.Encoding = Nothing)
        Me.Path = Path
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        If Encoding Is Nothing Then
            Me.Encoding = Txt.GetEncoding(Path)
        Else
            Me.Encoding = Encoding
        End If
        Values = Plain.ReadFile(Path, Me.Encoding).ToList
    End Sub
    Public Sub New(ByVal IsReadOnly As Boolean, ByVal Count As Integer)
        Me.Path = ""
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        Values = (From i In Enumerable.Range(0, Count) Select "").ToList
    End Sub

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

    Public Property Item(ByVal Index As Integer) As String
        Get
            Return Values(Index)
        End Get
        Set(ByVal Value As String)
            If IsReadOnlyValue Then Throw New InvalidOperationException
            If Index < 0 OrElse Index >= Values.Count Then Throw New ArgumentOutOfRangeException
            IsModifiedValue = True
            Values(Index) = Value
        End Set
    End Property

    Public Property Text(ByVal Index As Integer) As String Implements ILocalizationTextList.Text
        Get
            Return Item(Index)
        End Get
        Set(ByVal Value As String)
            Item(Index) = Value
        End Set
    End Property

    Public Sub Flush() Implements ILocalizationTextList.Flush
        If Path = "" Then Return
        If IsModifiedValue Then
            Plain.WriteFile(Path, Encoding, Values)
            IsModifiedValue = False
        End If
    End Sub
End Class

Public Class AgemoTextList
    Implements ILocalizationTextList

    Private Path As String
    Private IsReadOnlyValue As Boolean
    Private Encoding As System.Text.Encoding
    Private IsModifiedValue As Boolean
    Private Values As List(Of String)

    Public Sub New(ByVal Path As String, ByVal IsReadOnly As Boolean, Optional ByVal Encoding As System.Text.Encoding = Nothing)
        Me.Path = Path
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        If Encoding Is Nothing Then
            Me.Encoding = Txt.GetEncoding(Path)
        Else
            Me.Encoding = Encoding
        End If
        Values = Agemo.ReadFile(Path, Me.Encoding).ToList
    End Sub
    Public Sub New(ByVal IsReadOnly As Boolean, ByVal Count As Integer)
        Me.Path = ""
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        Values = (From i In Enumerable.Range(0, Count) Select "").ToList
    End Sub

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

    Public Property Item(ByVal Index As Integer) As String
        Get
            Return Values(Index)
        End Get
        Set(ByVal Value As String)
            If IsReadOnlyValue Then Throw New InvalidOperationException
            If Index < 0 OrElse Index >= Values.Count Then Throw New ArgumentOutOfRangeException
            IsModifiedValue = True
            Values(Index) = Value
        End Set
    End Property

    Public Property Text(ByVal Index As Integer) As String Implements ILocalizationTextList.Text
        Get
            Return Item(Index)
        End Get
        Set(ByVal Value As String)
            Item(Index) = Value
        End Set
    End Property

    Public Sub Flush() Implements ILocalizationTextList.Flush
        If Path = "" Then Return
        If IsModifiedValue Then
            Agemo.WriteFile(Path, Encoding, Values)
            IsModifiedValue = False
        End If
    End Sub
End Class

Public Class LOCList
    Implements ILocalizationTextList

    Private Path As String
    Private IsReadOnlyValue As Boolean
    Private Encoding As System.Text.Encoding
    Private IsModifiedValue As Boolean
    Private Values As LOC

    Private LOCTexter As ITexter(Of LOC)

    Public Sub New(ByVal Path As String, ByVal IsReadOnly As Boolean, ByVal Encoding As System.Text.Encoding, ByVal LOCTexter As ITexter(Of LOC))
        Me.Path = Path
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        Me.Encoding = Encoding
        Using s As New StreamEx(Path, FileMode.Open, FileAccess.Read)
            Values = New LOC(s)
        End Using
        Me.LOCTexter = LOCTexter
    End Sub
    Public Sub New(ByVal IsReadOnly As Boolean, ByVal Count As Integer, ByVal LOCTexter As ITexter(Of LOC))
        Me.Path = ""
        Me.IsReadOnlyValue = IsReadOnly
        Me.IsModifiedValue = False
        Values = LOC.GenerateLOC(New FontLib, (From i In Enumerable.Range(0, Count) Select New CharCode() {}).ToArray)
        Me.LOCTexter = LOCTexter
    End Sub

    Public ReadOnly Property Count() As Integer Implements ILocalizationTextList.Count
        Get
            Return Values.Text.Count
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

    Public Property Item(ByVal Index As Integer) As CharCode()
        Get
            Return Values.Text(Index)
        End Get
        Set(ByVal Value As CharCode())
            If IsReadOnlyValue Then Throw New InvalidOperationException
            If Index < 0 OrElse Index >= Count Then Throw New ArgumentOutOfRangeException
            IsModifiedValue = True
            Values.Text(Index) = Value
        End Set
    End Property

    Public Property Text(ByVal Index As Integer) As String Implements ILocalizationTextList.Text
        Get
            Return LOCTexter.Text(Values, Index)
        End Get
        Set(ByVal Value As String)
            LOCTexter.Text(Values, Index) = Value
        End Set
    End Property

    Public ReadOnly Property LOC() As LOC
        Get
            Return Values
        End Get
    End Property

    Public Sub Flush() Implements ILocalizationTextList.Flush
        If Path = "" Then Return
        If IsModifiedValue Then
            Agemo.WriteFile(Path, Encoding, Values)
            IsModifiedValue = False
        End If
    End Sub
End Class

Public Interface ITexter(Of T)
    Property Text(ByVal Texts As T, ByVal Index As Integer) As String
End Interface

Public Class LOCTexter
    Implements ITexter(Of LOC)

    Private Function CharCodeToString(ByVal c As CharCode) As String
        If c.HasUnicode Then Return c.Character
        Return "{" & c.Code.ToString("X4") & "}"
    End Function

    Private Function StringToCharCode(ByVal m As Match) As CharCode
        Dim r = m.Result("${Code}")
        If r <> "" Then
            Return CharCode.FromCode(Integer.Parse(r, Globalization.NumberStyles.AllowHexSpecifier))
        Else
            Return CharCode.FromUniChar(Char32.FromString(m.Value))
        End If
    End Function

    Public Property Text(ByVal Texts As LOC, ByVal Index As Integer) As String Implements ITexter(Of LOC).Text
        Get
            Return String.Join("", (From c In Texts.Text(Index) Select CharCodeToString(c)).ToArray).Replace(CrLf, Lf).Replace(Cr, Lf).Replace(Lf, CrLf)
        End Get
        Set(ByVal Value As String)
            Static r As New Regex("\{(?<Code>[0-9A-Fa-f]{4,6})\}|.|\r|\n", RegexOptions.ExplicitCapture)
            Texts.Text(Index) = (From m As Match In r.Matches(Value) Select StringToCharCode(m)).ToArray
        End Set
    End Property
End Class
