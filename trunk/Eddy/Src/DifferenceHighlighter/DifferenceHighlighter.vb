'==========================================================================
'
'  File:        DifferenceHighlighter.vb
'  Location:    Eddy.DifferenceHighlighter <Visual Basic .Net>
'  Description: 文本本地化工具差异比较高亮插件
'  Version:     2010.09.27.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.IO
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Setting
Imports Firefly.Project

Public Class Config
    Public ComparePairs As ComparePair()
End Class

Public Class ComparePair
    Public Source As String
    Public Target As String
    Public RemoveLineBackColor As String = "FFFFDFDF"
    Public AddLineBackColor As String = "FFDFFFDF"
    Public RemoveBackColor As String = "FFFFBFBF"
    Public AddBackColor As String = "FFBFFFBF"
End Class

Public Class DifferenceHighlighter
    Inherits TextLocalizerBase
    Implements ITextLocalizerTextHighlighter

    Private SettingPath As String = "DifferenceHighlighter.locplugin"
    Private Config As Config

    Public Sub New()
        If File.Exists(SettingPath) Then
            Config = Xml.ReadFile(Of Config)(SettingPath)
        Else
            Config = New Config With {.ComparePairs = New ComparePair() {}}
        End If
    End Sub
    Protected Overrides Sub DisposeManagedResource()
        Try
            Xml.WriteFile(SettingPath, UTF16, Config)
        Catch
        End Try
        MyBase.DisposeManagedResource()
    End Sub

    Private Function SplitToLines(ByVal Text As String) As String()
        Dim Lines As New List(Of String)
        Dim Index As Integer = 0
        While True
            Dim i = Text.IndexOf(Lf, Index)
            If i < 0 Then
                Lines.Add(Text.Substring(Index))
                Exit While
            End If
            Lines.Add(Text.Substring(Index, i + 1 - Index))
            Index = i + 1
        End While
        Return Lines.ToArray
    End Function

    Private Function GetCharIndexFromLineIndex(ByVal LineStart As Integer(), ByVal TextLength As Integer, ByVal LineIndex As Integer, ByVal ColumnIndex As Integer) As Integer
        If LineIndex < 0 Then Return 0
        If LineIndex >= LineStart.Count Then Return TextLength
        Return LineStart(LineIndex) + ColumnIndex
    End Function

    Public Function GetTextStyles(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of TextStyle()) Implements Firefly.Project.ITextLocalizerTextHighlighter.GetTextStyles
        Dim TextStyles = (From i In Enumerable.Range(0, Columns.Count) Select New List(Of TextStyle)).ToArray

        If Config.ComparePairs IsNot Nothing Then
            For Each p In Config.ComparePairs
                If p Is Nothing Then Continue For
                If Not NameToColumn.ContainsKey(p.Source) Then Continue For
                If Not NameToColumn.ContainsKey(p.Target) Then Continue For
                Dim SourceText = FormatedTexts(NameToColumn(p.Source))
                Dim TargetText = FormatedTexts(NameToColumn(p.Target))
                If SourceText Is Nothing Then SourceText = ""
                If TargetText Is Nothing Then TargetText = ""
                Dim RemoveLineBackColor = Color.FromArgb(Integer.Parse(p.RemoveLineBackColor, Globalization.NumberStyles.HexNumber))
                Dim AddLineBackColor = Color.FromArgb(Integer.Parse(p.AddLineBackColor, Globalization.NumberStyles.HexNumber))
                Dim RemoveBackColor = Color.FromArgb(Integer.Parse(p.RemoveBackColor, Globalization.NumberStyles.HexNumber))
                Dim AddBackColor = Color.FromArgb(Integer.Parse(p.AddBackColor, Globalization.NumberStyles.HexNumber))
                Dim SourceLines = SplitToLines(SourceText)
                Dim TargetLines = SplitToLines(TargetText)
                Dim Diff = StringDiff.Compare(SourceLines, TargetLines)

                Dim DiffPairs As New List(Of KeyValuePair(Of TranslatePart, TranslatePart))
                Dim DiffQueue As New Queue(Of TranslatePart)(Diff)
                While DiffQueue.Count > 0
                    Dim a = DiffQueue.Dequeue
                    If a.SourceLength = a.TargetLength Then Continue While
                    If (a.SourceLength <> 0) = (a.TargetLength <> 0) Then Throw New InvalidOperationException
                    If DiffQueue.Count = 0 Then
                        If a.SourceLength <> 0 Then
                            DiffPairs.Add(New KeyValuePair(Of TranslatePart, TranslatePart)(a, Nothing))
                        Else
                            DiffPairs.Add(New KeyValuePair(Of TranslatePart, TranslatePart)(Nothing, a))
                        End If
                        Exit While
                    End If
                    Dim b = DiffQueue.Peek
                    If b.SourceLength = b.TargetLength OrElse ((a.SourceLength <> 0) = (b.SourceLength <> 0)) Then
                        If a.SourceLength <> 0 Then
                            DiffPairs.Add(New KeyValuePair(Of TranslatePart, TranslatePart)(a, Nothing))
                        Else
                            DiffPairs.Add(New KeyValuePair(Of TranslatePart, TranslatePart)(Nothing, a))
                        End If
                        Continue While
                    End If
                    DiffQueue.Dequeue()
                    If a.SourceLength <> 0 Then
                        DiffPairs.Add(New KeyValuePair(Of TranslatePart, TranslatePart)(a, b))
                    Else
                        DiffPairs.Add(New KeyValuePair(Of TranslatePart, TranslatePart)(b, a))
                    End If
                End While

                Dim SourceLineStart = GetSummation(0, SourceLines.Select(Function(s) s.Length).ToArray)
                Dim TargetLineStart = GetSummation(0, TargetLines.Select(Function(s) s.Length).ToArray)

                Dim Source = TextStyles(NameToColumn(p.Source))
                Dim Target = TextStyles(NameToColumn(p.Target))
                For Each Pair In DiffPairs
                    Dim SourceBlockIndex As Integer = 0
                    Dim SourceBlockLength As Integer = 0
                    Dim TargetBlockIndex As Integer = 0
                    Dim TargetBlockLength As Integer = 0
                    If Pair.Key IsNot Nothing Then
                        Dim d = Pair.Key
                        SourceBlockIndex = GetCharIndexFromLineIndex(SourceLineStart, SourceText.Length, d.SourceIndex, 0)
                        SourceBlockLength = GetCharIndexFromLineIndex(SourceLineStart, SourceText.Length, d.SourceIndex + d.SourceLength, 0) - SourceBlockIndex
                        Source.Add(New TextStyle With {.Index = SourceBlockIndex, .Length = SourceBlockLength, .ForeColor = Color.Black, .BackColor = RemoveLineBackColor})
                    End If
                    If Pair.Value IsNot Nothing Then
                        Dim d = Pair.Value
                        TargetBlockIndex = GetCharIndexFromLineIndex(TargetLineStart, TargetText.Length, d.TargetIndex, 0)
                        TargetBlockLength = GetCharIndexFromLineIndex(TargetLineStart, TargetText.Length, d.TargetIndex + d.TargetLength, 0) - TargetBlockIndex
                        Target.Add(New TextStyle With {.Index = TargetBlockIndex, .Length = TargetBlockLength, .ForeColor = Color.Black, .BackColor = AddLineBackColor})
                    End If
                    If Pair.Key Is Nothing OrElse Pair.Value Is Nothing Then
                        Continue For
                    End If
                    Dim SourceBlock = SourceText.Substring(SourceBlockIndex, SourceBlockLength).ToCharArray
                    Dim TargetBlock = TargetText.Substring(TargetBlockIndex, TargetBlockLength).ToCharArray
                    Dim InBlockDiff = StringDiff.Compare(SourceBlock, TargetBlock)
                    For Each d In InBlockDiff
                        If d.SourceLength = d.TargetLength Then
                            'Source.Add(New TextStyle With {.Index = SourceBlockIndex + d.SourceIndex, .Length = d.SourceLength, .ForeColor = Color.Black, .BackColor = RemoveLineBackColor})
                            'Target.Add(New TextStyle With {.Index = TargetBlockIndex + d.TargetIndex, .Length = d.TargetLength, .ForeColor = Color.Black, .BackColor = AddLineBackColor})
                        Else
                            If d.SourceLength <> 0 Then
                                Source.Add(New TextStyle With {.Index = SourceBlockIndex + d.SourceIndex, .Length = d.SourceLength, .ForeColor = Color.Black, .BackColor = RemoveBackColor})
                            End If
                            If d.TargetLength <> 0 Then
                                Target.Add(New TextStyle With {.Index = TargetBlockIndex + d.TargetIndex, .Length = d.TargetLength, .ForeColor = Color.Black, .BackColor = AddBackColor})
                            End If
                        End If
                    Next
                Next
            Next
        End If

        Return (From l In TextStyles Select l.ToArray).ToArray
    End Function
End Class
