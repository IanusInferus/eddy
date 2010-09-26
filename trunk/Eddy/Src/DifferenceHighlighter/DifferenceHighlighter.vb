'==========================================================================
'
'  File:        DifferenceHighlighter.vb
'  Location:    Eddy.DifferenceHighlighter <Visual Basic .Net>
'  Description: 文本本地化工具差异比较高亮插件
'  Version:     2009.10.08.
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
    Public BlankLineBackColor As String = "FF7F7F7F"
    Public RemoveLineBackColor As String = "FF7F0000"
    Public AddLineBackColor As String = "FF007F00"
    Public RemoveBackColor As String = "FFFF0000"
    Public AddBackColor As String = "FF00FF00"
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
                Dim BlankLineBackColor = Color.FromArgb(Integer.Parse(p.BlankLineBackColor, Globalization.NumberStyles.HexNumber))
                Dim RemoveLineBackColor = Color.FromArgb(Integer.Parse(p.RemoveLineBackColor, Globalization.NumberStyles.HexNumber))
                Dim AddLineBackColor = Color.FromArgb(Integer.Parse(p.AddLineBackColor, Globalization.NumberStyles.HexNumber))
                Dim AddBackColor = Color.FromArgb(Integer.Parse(p.AddBackColor, Globalization.NumberStyles.HexNumber))
                Dim SourceLines = SplitToLines(SourceText)
                Dim TargetLines = SplitToLines(TargetText)
                Dim Diff = StringDiff.Compare(SourceLines, TargetLines)

                Dim SourceLineStart = GetSummation(0, SourceLines.Select(Function(s) s.Length).ToArray)
                Dim TargetLineStart = GetSummation(0, TargetLines.Select(Function(s) s.Length).ToArray)

                Dim Source = TextStyles(NameToColumn(p.Source))
                Dim Target = TextStyles(NameToColumn(p.Target))
                For Each d In Diff
                    If d.SourceLength <> d.TargetLength Then
                        If d.SourceLength <> 0 Then
                            Dim Index = GetCharIndexFromLineIndex(SourceLineStart, SourceText.Length, d.SourceIndex, 0)
                            Dim Length = GetCharIndexFromLineIndex(SourceLineStart, SourceText.Length, d.SourceIndex + d.SourceLength, 0) - Index
                            Source.Add(New TextStyle With {.Index = Index, .Length = Length, .ForeColor = Color.Black, .BackColor = RemoveLineBackColor})
                        End If
                        If d.TargetLength <> 0 Then
                            Dim Index = GetCharIndexFromLineIndex(TargetLineStart, TargetText.Length, d.TargetIndex, 0)
                            Dim Length = GetCharIndexFromLineIndex(TargetLineStart, TargetText.Length, d.TargetIndex + d.TargetLength, 0) - Index
                            Target.Add(New TextStyle With {.Index = Index, .Length = Length, .ForeColor = Color.Black, .BackColor = RemoveLineBackColor})
                        End If
                    End If
                Next
            Next
        End If

        Return (From l In TextStyles Select l.ToArray).ToArray
    End Function
End Class
