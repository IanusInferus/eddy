'==========================================================================
'
'  File:        FindReplace.vb
'  Location:    Eddy.FindReplace <Visual Basic .Net>
'  Description: 文本查找替换
'  Version:     2010.12.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Eddy.Interfaces

Public Class FindIndex
    Public ColumnIndex As Integer
    Public TextName As String
    Public TextIndex As Integer
    Public Start As Integer
    Public Length As Integer
End Class

Public Enum FindReplaceMode
    OneFileOneColumn = 0
    MultiFileOneColumn = 1
    OneFileMultiColumn = 2
    MultiFileMultiColumn = 3
    MultiFile = 1
    MultiColumn = 2
End Enum

Public Class FindReplace
    Private Mode As FindReplaceMode
    Private SearchUp As Boolean

    Private CaseSensitive As Boolean
    Private UseRegex As Boolean
    Private SmartHanziMatch As Boolean

    Public Sub New(ByVal Mode As FindReplaceMode, ByVal CaseSensitive As Boolean, ByVal SearchUp As Boolean, ByVal UseRegex As Boolean, ByVal SmartHanziMatch As Boolean)
        Me.Mode = Mode
        Me.SearchUp = SearchUp

        Me.CaseSensitive = CaseSensitive
        Me.UseRegex = UseRegex
        If UseRegex Then
            Me.SmartHanziMatch = False
        Else
            Me.SmartHanziMatch = SmartHanziMatch
        End If
    End Sub

    Private Function TranslateChar(ByVal c As Char32) As String
        Static TSet As New HashSet(Of Char32)(".$^{[(|)*+?\".ToUTF32)
        If TSet.Contains(c) Then Return "\" & c
        If SmartHanziMatch Then
            Static VarientDict As Dictionary(Of Char32, Char32())
            If VarientDict Is Nothing Then
                Dim MappingPoints = HanziConverter.G2T_Set.Concat(HanziConverter.T2G_Set).Concat(HanziConverter.T2J_Set).Concat(HanziConverter.J2T_Set).Concat(HanziConverter.G2J_Set).Concat(HanziConverter.J2G_Set).Distinct
                Dim GroupByKey = From g In (From p In MappingPoints Group By p.Key Into Group) Select Key = g.Key, Values = (New Char32() {g.Key}).Union(From p2 In g.Group Select p2.Value).Distinct
                VarientDict = GroupByKey.ToDictionary(Function(g) g.Key, Function(g) g.Values.ToArray)
            End If
            If VarientDict.ContainsKey(c) Then
                Dim v = VarientDict(c)
                Return "(?:" & String.Join("|", (From s In v Select "(?:" & s & ")").ToArray) & ")"
            End If
        End If
        Return c
    End Function

    Public Function GetFindRegex(ByVal TextFind As String) As Regex
        Dim Pattern As String
        If UseRegex Then
            Pattern = TextFind
        Else
            Pattern = String.Join("", (From c In TextFind.ToUTF32 Select TranslateChar(c)).ToArray)
        End If
        Dim Options = RegexOptions.Multiline
        If SearchUp Then Options = Options Or RegexOptions.RightToLeft
        If Not CaseSensitive Then Options = Options Or RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant
        Return New Regex(Pattern, Options)
    End Function
    Public Function GetReplacePattern(ByVal TextReplace As String) As String
        Dim Pattern As String
        If UseRegex Then
            Pattern = TextReplace.Descape
        Else
            Pattern = TextReplace.Replace("$", "$$")
        End If
        Return Pattern
    End Function

    Private Delegate Function Action(ByVal ColumnIndex As Integer, ByVal TextName As String, ByVal TextIndex As Integer, ByVal t As String, ByVal Start As Integer) As Boolean
    Private Sub ForEach(ByVal Columns As IEnumerable(Of LocalizationTextProvider), ByVal MainColumnIndex As Integer, ByVal Input As FindIndex, ByVal TextFind As String, ByVal FilterOutReadOnly As Boolean, ByVal f As Action)
        Dim MainColumn = Columns(MainColumnIndex)
        Dim tp = Columns(Input.ColumnIndex)
        Dim tl = tp.TryGetValue(Input.TextName)
        If tl IsNot Nothing AndAlso Input.TextIndex < tl.Count Then
            Dim t = tl.Text(Input.TextIndex)
            Dim Start As Integer
            If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                If SearchUp Then
                    Start = Input.Start
                Else
                    Start = Input.Start + Input.Length
                End If
                If f(Input.ColumnIndex, Input.TextName, Input.TextIndex, t, Start) Then Return
            End If
        End If

        Select Case Mode
            Case FindReplaceMode.OneFileOneColumn, FindReplaceMode.MultiFileOneColumn
                If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                    If SearchUp Then
                        If tl Is Nothing Then Exit Select
                        For TextIndex = Input.TextIndex - 1 To 0 Step -1
                            If TextIndex >= tl.Count Then Continue For
                            Dim t = tl.Text(TextIndex)
                            If f(Input.ColumnIndex, Input.TextName, TextIndex, t, 0) Then Return
                        Next
                    Else
                        If tl Is Nothing Then Exit Select
                        For TextIndex = Input.TextIndex + 1 To tl.Count - 1
                            If TextIndex >= tl.Count Then Continue For
                            Dim t = tl.Text(TextIndex)
                            If f(Input.ColumnIndex, Input.TextName, TextIndex, t, 0) Then Return
                        Next
                    End If
                End If
            Case FindReplaceMode.OneFileMultiColumn, FindReplaceMode.MultiFileMultiColumn
                If SearchUp Then
                    For ColumnIndex = Input.ColumnIndex - 1 To 0 Step -1
                        tp = Columns(ColumnIndex)
                        If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                            tl = tp.TryGetValue(Input.TextName)
                            If tl Is Nothing Then Continue For
                            If Input.TextIndex >= tl.Count Then Continue For
                            Dim t = tl.Text(Input.TextIndex)
                            If f(ColumnIndex, Input.TextName, Input.TextIndex, t, 0) Then Return
                        End If
                    Next
                    For TextIndex = Input.TextIndex - 1 To 0 Step -1
                        For ColumnIndex = Columns.Count - 1 To 0 Step -1
                            tp = Columns(ColumnIndex)
                            If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                                tl = tp.TryGetValue(Input.TextName)
                                If tl Is Nothing Then Continue For
                                If TextIndex >= tl.Count Then Continue For
                                Dim t = tl.Text(TextIndex)
                                If f(ColumnIndex, Input.TextName, TextIndex, t, 0) Then Return
                            End If
                        Next
                    Next
                Else
                    For ColumnIndex = Input.ColumnIndex + 1 To Columns.Count - 1
                        tp = Columns(ColumnIndex)
                        If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                            tl = tp.TryGetValue(Input.TextName)
                            If tl Is Nothing Then Continue For
                            If Input.TextIndex >= tl.Count Then Continue For
                            Dim t = tl.Text(Input.TextIndex)
                            If f(ColumnIndex, Input.TextName, Input.TextIndex, t, 0) Then Return
                        End If
                    Next
                    If tl Is Nothing Then Exit Select
                    For TextIndex = Input.TextIndex + 1 To tl.Count - 1
                        For ColumnIndex = 0 To Columns.Count - 1
                            tp = Columns(ColumnIndex)
                            If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                                tl = tp.TryGetValue(Input.TextName)
                                If tl Is Nothing Then Continue For
                                If TextIndex >= tl.Count Then Continue For
                                Dim t = tl.Text(TextIndex)
                                If f(ColumnIndex, Input.TextName, TextIndex, t, 0) Then Return
                            End If
                        Next
                    Next
                End If
        End Select

        Select Case Mode
            Case FindReplaceMode.MultiFileOneColumn
                If SearchUp Then
                    Dim CurrentTextNameIndex = MainColumn.IndexOf(Input.TextName)
                    For TextNameIndex = CurrentTextNameIndex - 1 To 0 Step -1
                        Dim TextName = MainColumn.Keys(TextNameIndex)
                        tp = MainColumn
                        tl = tp.TryGetValue(TextName)
                        If tl Is Nothing Then Continue For
                        For TextIndex = tl.Count - 1 To 0 Step -1
                            Dim ColumnIndex = Input.ColumnIndex
                            tp = Columns(ColumnIndex)
                            If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                                tl = tp.TryGetValue(TextName)
                                If tl Is Nothing Then Continue For
                                If TextIndex >= tl.Count Then Continue For
                                Dim t = tl.Text(TextIndex)
                                If f(ColumnIndex, TextName, TextIndex, t, 0) Then Return
                            End If
                        Next
                    Next
                Else
                    Dim CurrentTextNameIndex = MainColumn.IndexOf(Input.TextName)
                    For TextNameIndex = CurrentTextNameIndex + 1 To MainColumn.Count - 1
                        Dim TextName = MainColumn.Keys(TextNameIndex)
                        tp = MainColumn
                        tl = tp.TryGetValue(TextName)
                        If tl Is Nothing Then Continue For
                        For TextIndex = 0 To tl.Count - 1
                            Dim ColumnIndex = Input.ColumnIndex
                            tp = Columns(ColumnIndex)
                            If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                                tl = tp.TryGetValue(TextName)
                                If tl Is Nothing Then Continue For
                                If TextIndex >= tl.Count Then Continue For
                                Dim t = tl.Text(TextIndex)
                                If f(ColumnIndex, TextName, TextIndex, t, 0) Then Return
                            End If
                        Next
                    Next
                End If
            Case FindReplaceMode.MultiFileMultiColumn
                If SearchUp Then
                    Dim CurrentTextNameIndex = MainColumn.IndexOf(Input.TextName)
                    For TextNameIndex = CurrentTextNameIndex - 1 To 0 Step -1
                        Dim TextName = MainColumn.Keys(TextNameIndex)
                        tp = MainColumn
                        tl = tp.TryGetValue(TextName)
                        If tl Is Nothing Then Continue For
                        For TextIndex = tl.Count - 1 To 0 Step -1
                            For ColumnIndex = Columns.Count - 1 To 0 Step -1
                                tp = Columns(ColumnIndex)
                                If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                                    tl = tp.TryGetValue(TextName)
                                    If tl Is Nothing Then Continue For
                                    If TextIndex >= tl.Count Then Continue For
                                    Dim t = tl.Text(TextIndex)
                                    If f(ColumnIndex, TextName, TextIndex, t, 0) Then Return
                                End If
                            Next
                        Next
                    Next
                Else
                    Dim CurrentTextNameIndex = MainColumn.IndexOf(Input.TextName)
                    For TextNameIndex = CurrentTextNameIndex + 1 To MainColumn.Count - 1
                        Dim TextName = MainColumn.Keys(TextNameIndex)
                        tp = MainColumn
                        tl = tp.TryGetValue(TextName)
                        If tl Is Nothing Then Continue For
                        For TextIndex = 0 To tl.Count - 1
                            For ColumnIndex = 0 To Columns.Count - 1
                                tp = Columns(ColumnIndex)
                                If Not (tp.IsReadOnly AndAlso FilterOutReadOnly) Then
                                    tl = tp.TryGetValue(TextName)
                                    If tl Is Nothing Then Continue For
                                    If TextIndex >= tl.Count Then Continue For
                                    Dim t = tl.Text(TextIndex)
                                    If f(ColumnIndex, TextName, TextIndex, t, 0) Then Return
                                End If
                            Next
                        Next
                    Next
                End If
        End Select
    End Sub

    Private Function TryGetFindIndex(ByVal r As Regex, ByVal ColumnIndex As Integer, ByVal TextName As String, ByVal TextIndex As Integer, ByVal t As String, ByVal Start As Integer, ByRef ret As FindIndex) As Boolean
        Dim m = r.Match(t, Start)
        If m.Success Then
            ret = New FindIndex With {.ColumnIndex = ColumnIndex, .TextName = TextName, .TextIndex = TextIndex, .Start = m.Index, .Length = m.Length}
            Return True
        End If
        Return False
    End Function

    Public Function Find(ByVal Columns As IEnumerable(Of LocalizationTextProvider), ByVal MainColumnIndex As Integer, ByVal Input As FindIndex, ByVal TextFind As String, Optional ByVal FilterOutReadOnly As Boolean = False) As FindIndex
        Dim r = GetFindRegex(TextFind)
        Dim ret As FindIndex = Nothing
        ForEach(Columns, MainColumnIndex, Input, TextFind, FilterOutReadOnly, Function(ColumnIndex, TextName, TextIndex, t, Start) TryGetFindIndex(r, ColumnIndex, TextName, TextIndex, t, Start, ret))
        Return ret
    End Function

    Public Function Replace(ByVal Columns As IEnumerable(Of LocalizationTextProvider), ByVal MainColumnIndex As Integer, ByVal Input As FindIndex, ByVal TextFind As String, ByVal TextReplace As String) As FindIndex
        Dim MainColumn = Columns(MainColumnIndex)
        Dim r = GetFindRegex(TextFind)
        Dim ReplacePattern = GetReplacePattern(TextReplace)
        Dim tp = Columns(Input.ColumnIndex)
        If Not tp.IsReadOnly Then
            Dim tl = tp.TryGetValue(Input.TextName)
            If tl Is Nothing Then Return Nothing
            If Input.TextIndex >= tl.Count Then Return Nothing
            Dim t = tl.Text(Input.TextIndex)
            Dim Start As Integer
            If SearchUp Then
                Start = Input.Start + Input.Length
            Else
                Start = Input.Start
            End If
            Dim m = r.Match(t, Start)
            If m.Success Then
                If m.Index = Input.Start AndAlso m.Length = Input.Length Then
                    t = t.Substring(0, m.Index) & m.Result(ReplacePattern) & t.Substring(m.Index + m.Length)
                    tl.Text(Input.TextIndex) = t
                    Return New FindIndex With {.ColumnIndex = Input.ColumnIndex, .TextName = Input.TextName, .TextIndex = Input.TextIndex, .Start = m.Index, .Length = m.Length}
                End If
            End If
        End If
        Return Nothing
    End Function

    Public Function ReplaceAll(ByVal Columns As IEnumerable(Of LocalizationTextProvider), ByVal MainColumnIndex As Integer, ByVal Input As FindIndex, ByVal TextFind As String, ByVal TextReplace As String) As Integer
        Dim MainColumn = Columns(MainColumnIndex)
        Dim r = GetFindRegex(TextFind)
        Dim ReplacePattern = GetReplacePattern(TextReplace)
        Dim tp = Columns(Input.ColumnIndex)
        Dim tl = tp.TryGetValue(Input.TextName)
        Dim t As String
        Dim cr As New CounterReplacer With {.ReplacePattern = ReplacePattern, .Count = 0}
        Select Case Mode
            Case FindReplaceMode.OneFileOneColumn
                If tl Is Nothing Then Return Nothing
                For TextIndex = 0 To tl.Count - 1
                    If TextIndex >= tl.Count Then Continue For
                    t = tl.Text(TextIndex)
                    tl.Text(TextIndex) = r.Replace(t, AddressOf cr.EvaluateMatch)
                Next
            Case FindReplaceMode.MultiFileOneColumn
                If tp.IsReadOnly Then Return 0
                For TextNameIndex = 0 To MainColumn.Count - 1
                    Dim TextName = MainColumn.Keys(TextNameIndex)
                    tl = tp.TryGetValue(TextName)
                    If tl Is Nothing Then Continue For
                    For TextIndex = 0 To tl.Count - 1
                        If TextIndex >= tl.Count Then Continue For
                        t = tl.Text(TextIndex)
                        tl.Text(TextIndex) = r.Replace(t, AddressOf cr.EvaluateMatch)
                    Next
                    If TextName <> Input.TextName Then tp.ForceUnloadText(TextName)
                Next
            Case FindReplaceMode.OneFileMultiColumn
                For ColumnIndex = 0 To Columns.Count - 1
                    tp = Columns(ColumnIndex)
                    If tp.IsReadOnly Then Continue For
                    tl = tp.TryGetValue(Input.TextName)
                    If tl Is Nothing Then Continue For
                    For TextIndex = 0 To tl.Count - 1
                        If TextIndex >= tl.Count Then Continue For
                        t = tl.Text(TextIndex)
                        tl.Text(TextIndex) = r.Replace(t, AddressOf cr.EvaluateMatch)
                    Next
                Next
            Case FindReplaceMode.MultiFileMultiColumn
                For ColumnIndex = 0 To Columns.Count - 1
                    tp = Columns(ColumnIndex)
                    If tp.IsReadOnly Then Continue For
                    For TextNameIndex = 0 To MainColumn.Count - 1
                        Dim TextName = MainColumn.Keys(TextNameIndex)
                        tl = tp.TryGetValue(TextName)
                        If tl Is Nothing Then Continue For
                        For TextIndex = 0 To tl.Count - 1
                            If TextIndex >= tl.Count Then Continue For
                            t = tl.Text(TextIndex)
                            tl.Text(TextIndex) = r.Replace(t, AddressOf cr.EvaluateMatch)
                        Next
                        If TextName <> Input.TextName Then tp.ForceUnloadText(TextName)
                    Next
                Next
        End Select
        Return cr.Count
    End Function

    Private Class CounterReplacer
        Public ReplacePattern As String
        Public Count As Integer = 0
        Public Function EvaluateMatch(ByVal m As Match) As String
            Dim ret = m.Result(ReplacePattern)
            Count += 1
            Return ret
        End Function
    End Class
End Class
