'==========================================================================
'
'  File:        EscapeSequenceHighlighter.vb
'  Location:    Eddy.EscapeSequenceHighlighter <Visual Basic .Net>
'  Description: 文本本地化工具控制符高亮插件
'  Version:     2012.02.29.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Xml.Linq
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Mapping
Imports Firefly.Mapping.XmlText
Imports Eddy.Interfaces

Public Class Config
    Public Regex As String = "\{.*?\}"
    Public ForeColor As String = "FF0000FF"
    Public BackColor As String = "FFBFBFFF"
    Public HideInGrid As Boolean = False
End Class

Public Class EscapeSequenceHighlighter
    Inherits TextLocalizerBase
    Implements ITextLocalizerTextHighlighter
    Implements ITextLocalizerGridTextFormatter
    Implements ITextLocalizerToolStripButtonPlugin
    Implements ITextLocalizerConfigurationPlugin

    Private Config As Config
    Public Sub SetConfiguration(ByVal Config As XElement) Implements ITextLocalizerConfigurationPlugin.SetConfiguration
        If Config Is Nothing Then
            Me.Config = New Config
        Else
            Me.Config = (New XmlSerializer).Read(Of Config)(Config)
        End If
        EscapeSequenceRegex = New Regex(Me.Config.Regex, RegexOptions.ExplicitCapture Or RegexOptions.Compiled)
        ForeColor = Color.FromArgb(Integer.Parse(Me.Config.ForeColor, Globalization.NumberStyles.HexNumber))
        BackColor = Color.FromArgb(Integer.Parse(Me.Config.BackColor, Globalization.NumberStyles.HexNumber))
    End Sub
    Public Function GetConfiguration() As XElement Implements ITextLocalizerConfigurationPlugin.GetConfiguration
        Return (New XmlSerializer).Write(Me.Config)
    End Function

    Private EscapeSequenceRegex As Regex
    Private ForeColor As Color
    Private BackColor As Color

    Private Function GetTextStylesForText(ByVal Text As String) As TextStyle()
        Return (From m As Match In EscapeSequenceRegex.Matches(Text) Select (New TextStyle With {.Index = m.Index, .Length = m.Length, .ForeColor = ForeColor, .BackColor = BackColor})).ToArray
    End Function

    Public Function GetTextStyles(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of TextStyle()) Implements ITextLocalizerTextHighlighter.GetTextStyles
        Return (From i In Enumerable.Range(0, Columns.Count) Select GetTextStylesForText(FormatedTexts(i))).ToArray
    End Function

    Public Function Format(ByVal TextName As String, ByVal TextIndex As Integer, ByVal FormatedTexts As IEnumerable(Of String)) As IEnumerable(Of String) Implements ITextLocalizerGridTextFormatter.Format
        If Config.HideInGrid Then
            Return From t In FormatedTexts Select EscapeSequenceRegex.Replace(t, "")
        Else
            Return FormatedTexts
        End If
    End Function

    Private HideInGridDescriptor As ToolStripButtonDescriptor
    Public Function GetToolStripButtonDescriptors() As IEnumerable(Of ToolStripButtonDescriptor) Implements ITextLocalizerToolStripButtonPlugin.GetToolStripButtonDescriptors
        If Config.HideInGrid Then
            HideInGridDescriptor = New ToolStripButtonDescriptor With {.Image = My.Resources.Show, .Text = "显示预览框中转义序列", .Click = AddressOf HideInGridChanged}
        Else
            HideInGridDescriptor = New ToolStripButtonDescriptor With {.Image = My.Resources.Hide, .Text = "隐藏预览框中转义序列", .Click = AddressOf HideInGridChanged}
        End If
        Return New ToolStripButtonDescriptor() {HideInGridDescriptor}
    End Function

    Private Sub HideInGridChanged()
        Config.HideInGrid = Not Config.HideInGrid
        Try
            Controller.RefreshGrid()
        Finally
            If Config.HideInGrid Then
                HideInGridDescriptor.ImageChanged.Raise(My.Resources.Show)
                HideInGridDescriptor.TextChanged.Raise("显示预览框中转义序列")
            Else
                HideInGridDescriptor.ImageChanged.Raise(My.Resources.Hide)
                HideInGridDescriptor.TextChanged.Raise("隐藏预览框中转义序列")
            End If
        End Try
    End Sub
End Class
