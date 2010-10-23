'==========================================================================
'
'  File:        LocalizationTextProvider.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 本地化文本数据提供
'  Version:     2010.10.05.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions

Public Interface ILocalizationTextList
    ReadOnly Property Count() As Integer
    ReadOnly Property IsReadOnly() As Boolean
    ReadOnly Property IsModified() As Boolean
    Property Text(ByVal Index As Integer) As String
    Sub Flush()
End Interface

Public Class LocalizationTextProvider
    Implements IDisposable, IDictionary(Of String, ILocalizationTextList)

    Private FactoryValue As ILocalizationTextListFactory
    Private NameValue As String
    Private DisplayNameValue As String
    Private DirectoryValue As String
    Private ExtensionValue As String
    Private TypeValue As String
    Private IsReadOnlyValue As Boolean
    Private EncodingValue As System.Text.Encoding = System.Text.Encoding.Unicode

    Private FileNamesValue As New List(Of String)
    Private ReadOnly Property FileNames() As List(Of String)
        Get
            Initialize()
            Return FileNamesValue
        End Get
    End Property
    Private FileNameDictValue As New Dictionary(Of String, Integer)
    Private ReadOnly Property FileNameDict() As Dictionary(Of String, Integer)
        Get
            Initialize()
            Return FileNameDictValue
        End Get
    End Property
    Private Lists As New Dictionary(Of String, ILocalizationTextList)

    Public ReadOnly Property TextListFactory() As ILocalizationTextListFactory
        Get
            Return FactoryValue
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return NameValue
        End Get
    End Property

    Public ReadOnly Property DisplayName() As String
        Get
            Return DisplayNameValue
        End Get
    End Property

    Public ReadOnly Property Directory() As String
        Get
            Return DirectoryValue
        End Get
    End Property

    Public ReadOnly Property Extension() As String
        Get
            Return ExtensionValue
        End Get
    End Property

    Public ReadOnly Property Type() As String
        Get
            Return TypeValue
        End Get
    End Property

    Public ReadOnly Property IsReadOnly() As Boolean
        Get
            Return IsReadOnlyValue
        End Get
    End Property

    Public ReadOnly Property Encoding() As System.Text.Encoding
        Get
            Return EncodingValue
        End Get
    End Property

    Public ReadOnly Property Count() As Integer Implements ICollection(Of KeyValuePair(Of String, ILocalizationTextList)).Count
        Get
            Return FileNames.Count
        End Get
    End Property

    Public ReadOnly Property Keys() As ICollection(Of String) Implements IDictionary(Of String, ILocalizationTextList).Keys
        Get
            Return FileNames
        End Get
    End Property

    Public ReadOnly Property Values() As ICollection(Of ILocalizationTextList) Implements IDictionary(Of String, ILocalizationTextList).Values
        Get
            For Each TextName In FileNames
                If Lists.ContainsKey(TextName) Then
                    Return Lists(TextName)
                Else
                    Dim v = FactoryValue.Load(NameValue, TypeValue, DirectoryValue, ExtensionValue, TextName, IsReadOnlyValue, EncodingValue)
                    Lists.Add(TextName, v)
                    Return v
                End If
            Next
            Return Lists.Values
        End Get
    End Property

    Public Function ContainsKey(ByVal TextName As String) As Boolean Implements IDictionary(Of String, ILocalizationTextList).ContainsKey
        Return FileNameDict.ContainsKey(TextName)
    End Function

    Public ReadOnly Property IsModified(ByVal TextName As String) As Boolean
        Get
            If Not FileNameDict.ContainsKey(TextName) Then Throw New ArgumentOutOfRangeException
            If Not Lists.ContainsKey(TextName) Then Return False
            Return Lists(TextName).IsModified
        End Get
    End Property

    Public Property Item(ByVal TextName As String) As ILocalizationTextList Implements IDictionary(Of String, ILocalizationTextList).Item
        Get
            If FileNameDict.ContainsKey(TextName) Then
                If Lists.ContainsKey(TextName) Then
                    Return Lists(TextName)
                Else
                    Dim v = FactoryValue.Load(NameValue, TypeValue, DirectoryValue, ExtensionValue, TextName, IsReadOnlyValue, EncodingValue)
                    Lists.Add(TextName, v)
                    Return v
                End If
            Else
                Throw New ArgumentOutOfRangeException
            End If
        End Get
        Set(ByVal Value As ILocalizationTextList)
            Throw New ArgumentOutOfRangeException
        End Set
    End Property

    Public Property Item(ByVal Index As Integer) As ILocalizationTextList
        Get
            Return Item(FileNames(Index))
        End Get
        Set(ByVal Value As ILocalizationTextList)
            Item(FileNames(Index)) = Value
        End Set
    End Property

    Public Function IndexOf(ByVal TextName As String) As String
        Return FileNameDict(TextName)
    End Function

    Public Sub ForceUnloadText(ByVal TextName As String)
        If FileNameDict.ContainsKey(TextName) Then
            If Lists.ContainsKey(TextName) Then
                Dim v = Lists(TextName)
                v.Flush()
                Lists.Remove(TextName)
            End If
        Else
            Throw New ArgumentOutOfRangeException
        End If
    End Sub

    Public Function LoadOrCreateItem(ByVal TextName As String, ByVal Template As ILocalizationTextList, ByVal TranslateText As Func(Of String, String)) As ILocalizationTextList
        If FileNameDict.ContainsKey(TextName) Then Return Item(TextName)
        If IsReadOnly Then Return Nothing
        Dim v = FactoryValue.LoadOrCreate(NameValue, TypeValue, DirectoryValue, ExtensionValue, TextName, IsReadOnlyValue, EncodingValue, Template, TranslateText)
        Lists.Add(TextName, v)
        FileNames.Add(TextName)
        FileNameDict.Add(TextName, FileNameDict.Count)
        Return v
    End Function

    Public Sub New(ByVal TextListFactory As ILocalizationTextListFactory, ByVal Name As String, ByVal DisplayName As String, ByVal Directory As String, ByVal Extension As String, ByVal Type As String, ByVal IsReadOnly As Boolean, Optional ByVal Encoding As System.Text.Encoding = Nothing)
        Me.FactoryValue = TextListFactory
        Me.NameValue = Name
        Me.DisplayNameValue = DisplayName
        Me.DirectoryValue = Directory
        Me.ExtensionValue = Extension
        Me.TypeValue = Type
        Me.IsReadOnlyValue = IsReadOnly
        Me.EncodingValue = Encoding
    End Sub

    Private Shared Function IsMatchFileMask(ByVal FileName As String, ByVal Mask As String) As Boolean
        Dim Pattern = "^" & Regex.Escape(Mask).Replace("\?", ".?").Replace("\*", "*?") & "$"
        Dim r As New Regex(Pattern, RegexOptions.ExplicitCapture Or RegexOptions.IgnoreCase)
        Return r.Match(FileName).Success
    End Function
    Private Initialized As Boolean = False
    Public Sub Initialize()
        If Initialized Then Return
        Dim i = 0
        For Each FileName In TextListFactory.List(NameValue, TypeValue, DirectoryValue, ExtensionValue)
            If IsMatchFileMask(FileName, "*." & Extension) Then
                Dim TextName = FileName.Substring(0, FileName.Length - Extension.Length - 1)
                FileNamesValue.Add(TextName)
                FileNameDictValue.Add(TextName, i)
                i += 1
            End If
        Next
        Initialized = True
    End Sub

#Region " IDisposable 支持 "
    ''' <summary>释放流的资源。</summary>
    ''' <remarks>对继承者的说明：不要调用基类的Dispose()，而应调用Dispose(True)，否则会出现无限递归。</remarks>
    Protected Overridable Sub Dispose(ByVal Disposing As Boolean)
        Static DisposedValue As Boolean = False '检测冗余的调用
        If DisposedValue Then Return
        If Disposing Then
            For Each p In Lists
                p.Value.Flush()
            Next
            '释放其他状态(托管对象)。
        End If

        '释放您自己的状态(非托管对象)。
        '将大型字段设置为 null。
        DisposedValue = True
    End Sub
    ''' <summary>释放流的资源。</summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。请将清理代码放入上面的 Dispose(ByVal disposing As Boolean) 中。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    Private ReadOnly Property IsReadOnlyCollection() As Boolean Implements ICollection(Of KeyValuePair(Of String, ILocalizationTextList)).IsReadOnly
        Get
            Return True
        End Get
    End Property

    Private Sub AddCollection(ByVal item As KeyValuePair(Of String, ILocalizationTextList)) Implements ICollection(Of KeyValuePair(Of String, ILocalizationTextList)).Add
        Throw New NotSupportedException
    End Sub

    Private Sub AddDictionary(ByVal Key As String, ByVal Value As ILocalizationTextList) Implements IDictionary(Of String, ILocalizationTextList).Add
        Throw New NotSupportedException
    End Sub

    Private Function RemoveCollection(ByVal Item As KeyValuePair(Of String, ILocalizationTextList)) As Boolean Implements ICollection(Of KeyValuePair(Of String, ILocalizationTextList)).Remove
        Throw New NotSupportedException
    End Function

    Private Function RemoveDictionary(ByVal Key As String) As Boolean Implements IDictionary(Of String, ILocalizationTextList).Remove
        Throw New NotSupportedException
    End Function

    Private Sub Clear() Implements ICollection(Of KeyValuePair(Of String, ILocalizationTextList)).Clear
        Throw New NotSupportedException
    End Sub

    Private Function Contains(ByVal Item As KeyValuePair(Of String, ILocalizationTextList)) As Boolean Implements ICollection(Of KeyValuePair(Of String, ILocalizationTextList)).Contains
        Return FileNameDict.ContainsKey(Item.Key)
    End Function

    Private Sub CopyTo(ByVal Array As KeyValuePair(Of String, ILocalizationTextList)(), ByVal ArrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, ILocalizationTextList)).CopyTo
        If Array Is Nothing Then Throw New ArgumentNullException("Array")
        If ArrayIndex < 0 Then Throw New ArgumentOutOfRangeException("ArrayIndex")
        Dim n = 0
        For Each TextName In FileNames
            If Not Lists.ContainsKey(TextName) Then
                Array(ArrayIndex + n) = New KeyValuePair(Of String, ILocalizationTextList)(TextName, Lists(TextName))
            Else
                Dim v = FactoryValue.Load(NameValue, TypeValue, DirectoryValue, ExtensionValue, TextName, IsReadOnlyValue, EncodingValue)
                Lists.Add(TextName, v)
                Array(ArrayIndex + n) = New KeyValuePair(Of String, ILocalizationTextList)(TextName, v)
            End If
            n += 1
        Next
    End Sub

    Private Function TryGetValue(ByVal Key As String, ByRef Value As ILocalizationTextList) As Boolean Implements IDictionary(Of String, ILocalizationTextList).TryGetValue
        If ContainsKey(Key) Then
            Value = Item(Key)
            Return True
        Else
            Value = Nothing
            Return False
        End If
    End Function

    Public Function TryGetValue(ByVal Key As String) As ILocalizationTextList
        If ContainsKey(Key) Then
            Return Item(Key)
        Else
            Return Nothing
        End If
    End Function

    Private Function GetEnumeratorEnumerable() As IEnumerator(Of KeyValuePair(Of String, ILocalizationTextList)) Implements IEnumerable(Of KeyValuePair(Of String, ILocalizationTextList)).GetEnumerator
        Return New LocalizationTextProviderEnumerator(Me)
    End Function

    Private Function GetEnumeratorCollection() As IEnumerator Implements IEnumerable.GetEnumerator
        Return New LocalizationTextProviderEnumerator(Me)
    End Function
End Class

Public Class LocalizationTextProviderEnumerator
    Implements IEnumerator(Of KeyValuePair(Of String, ILocalizationTextList))

    Private p As LocalizationTextProvider
    Private e As IEnumerator(Of String)

    Public Sub New(ByVal p As LocalizationTextProvider)
        Me.p = p
        Me.e = p.Keys.GetEnumerator()
    End Sub

    Public ReadOnly Property Current() As KeyValuePair(Of String, ILocalizationTextList) Implements IEnumerator(Of KeyValuePair(Of String, ILocalizationTextList)).Current
        Get
            Return New KeyValuePair(Of String, ILocalizationTextList)(e.Current, p.TryGetValue(e.Current))
        End Get
    End Property

    Private ReadOnly Property CurrentCollection() As Object Implements IEnumerator.Current
        Get
            Return Current
        End Get
    End Property

    Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
        Return e.MoveNext()
    End Function

    Public Sub Reset() Implements IEnumerator.Reset
        Me.e = p.Keys.GetEnumerator()
    End Sub

    Private disposedValue As Boolean = False
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                If e IsNot Nothing Then e.Dispose()
            End If

            p = Nothing
            e = Nothing
        End If
        Me.disposedValue = True
    End Sub

    Public Sub Dispose() Implements System.IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
