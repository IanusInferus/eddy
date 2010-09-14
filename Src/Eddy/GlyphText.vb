'==========================================================================
'
'  File:        LOC1.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: LOC文件格式类(版本1)(图形文本)
'  Version:     2010.09.14.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Text
Imports Firefly
Imports Firefly.Imaging
Imports Firefly.TextEncoding
Imports Firefly.Glyphing

''' <summary>图形文本类</summary>
Public Class GlyphText

    Private FontLib As New Dictionary(Of StringCode, IGlyph)
    Private Text As IEnumerable(Of StringCode)()

    ''' <summary>已重载。从字库、默认字形大小和文本码点创建实例。</summary>
    Public Sub New(ByVal CharGlyph As IEnumerable(Of IGlyph), ByVal GlyphWidth As Integer, ByVal GlyphHeight As Integer, ByVal Text As IEnumerable(Of StringCode)())
        If GlyphWidth <= 0 Then Throw New ArgumentOutOfRangeException
        If GlyphHeight <= 0 Then Throw New ArgumentOutOfRangeException
        If Text Is Nothing Then Throw New ArgumentNullException

        If CharGlyph Is Nothing Then Throw New ArgumentNullException
        For Each g In CharGlyph
            Dim c = g.c
            If FontLib.ContainsKey(c) Then Continue For
            FontLib.Add(c, g)
        Next

        Me.Text = Text
    End Sub

    ''' <summary>指示指定字符码点是否存在字形。</summary>
    Private ReadOnly Property HasGlyph(ByVal StringCode As StringCode) As Boolean
        Get
            Return FontLib.ContainsKey(StringCode) AndAlso FontLib(StringCode) IsNot Nothing
        End Get
    End Property

    ''' <summary>绘制CharInfo表示的文本。</summary>
    Public Overridable Function GetBitmap(ByVal GlyphWidth As Integer, ByVal GlyphHeight As Integer, ByVal TextIndex As Integer, Optional ByVal Space As Integer = 0, Optional ByVal PhoneticDictionary As Dictionary(Of String, String) = Nothing) As Bitmap
        Dim EnablePhonetic As Boolean = PhoneticDictionary IsNot Nothing

        Dim GlyphText As IEnumerable(Of StringCode) = Text(TextIndex)
        If GlyphText Is Nothing OrElse GlyphText.Count = 0 Then Return Nothing

        Dim Size As Integer = GlyphHeight
        Dim GetWidth = Function(Width As Integer) Width + Space
        If PhoneticDictionary IsNot Nothing Then
            GetWidth = Function(Width As Integer) CInt(Round((Width + Space) * 1.4))
        End If

        Dim Lines As New List(Of StringCode())
        Dim Line As New List(Of StringCode)
        Dim MaxLineWidth As Integer
        Dim LineWidth As Integer
        For Each c In GlyphText
            If c.UnicodeString = Lf Then
                Lines.Add(Line.ToArray)
                Line.Clear()
                If MaxLineWidth < LineWidth Then MaxLineWidth = LineWidth
                LineWidth = 0
            ElseIf HasGlyph(c) Then
                Line.Add(c)
                LineWidth += GetWidth(FontLib(c).VirtualBox.Width)
            Else
                Line.Add(c)
                LineWidth += GetWidth(GlyphWidth)
            End If
        Next
        Lines.Add(Line.ToArray)
        Line.Clear()
        If MaxLineWidth < LineWidth Then MaxLineWidth = LineWidth
        LineWidth = 0


        Using ZHFont As New Drawing.Font("宋体", Size, FontStyle.Regular, GraphicsUnit.Pixel)
            Using PFont As New Drawing.Font("宋体", (Size * 2) \ 3, FontStyle.Regular, GraphicsUnit.Pixel)
                Using JPFont As New Drawing.Font("MingLiU", Size, FontStyle.Regular, GraphicsUnit.Pixel)

                    Dim PadX As Integer = 5
                    Dim PadY As Integer = 5
                    Dim Bitmap As Bitmap
                    If EnablePhonetic Then
                        Bitmap = New Bitmap(MaxLineWidth + PadX * 2, GetWidth(Size) * 2 * Lines.Count + PadY * 2)
                    Else
                        Bitmap = New Bitmap(MaxLineWidth + PadX * 2, GetWidth(Size) * Lines.Count + PadY * 2)
                    End If
                    Using g As Graphics = Graphics.FromImage(Bitmap)
                        g.Clear(Color.White)

                        Dim x As Integer = PadX
                        Dim y As Integer = PadY

                        For Each GlyphLine In Lines
                            If EnablePhonetic Then y += GetWidth(Size)

                            Dim ControlCode As New List(Of String)
                            Dim ControlCodeMode = False

                            For Each c In GlyphLine
                                If c.HasUnicodes Then
                                    Dim ch As String = c.UnicodeString
                                    Select Case ch
                                        Case "<", "{"
                                            If Not ControlCodeMode Then
                                                ControlCodeMode = True
                                                ControlCode.Add(ch)
                                            End If
                                            Continue For
                                        Case ">", "}"
                                            If ControlCodeMode Then
                                                ControlCodeMode = False
                                                ControlCode.Add(ch)

                                                Dim s As String = String.Join("", ControlCode.ToArray)
                                                g.DrawString(s, ZHFont, Brushes.Black, x, y, StringFormat.GenericTypographic)
                                                x += g.MeasureStringWidth(s, ZHFont) + 1

                                                ControlCode.Clear()
                                            End If
                                            Continue For
                                        Case Else
                                            If ControlCodeMode Then
                                                ControlCode.Add(ch)
                                                Continue For
                                            Else
                                            End If
                                    End Select
                                End If

                                If c.IsControlChar OrElse Not ((HasGlyph(c) OrElse c.HasUnicodes OrElse c.HasCodes)) Then
                                    g.FillRectangle(Brushes.Gray, New Rectangle(x, y, GlyphWidth, GlyphHeight))
                                    x += GetWidth(GlyphWidth)
                                ElseIf HasGlyph(c) Then
                                    Dim Width = FontLib(c).VirtualBox.Width
                                    Dim Glyph = FontLib(c)
                                    If Glyph.VirtualBox.Width <= 0 OrElse Glyph.VirtualBox.Height <= 0 Then

                                    End If
                                    Using SrcImage As New Bitmap(Glyph.VirtualBox.Width, Glyph.VirtualBox.Height)
                                        SrcImage.SetRectangle(0, 0, Glyph.Block)
                                        Dim SrcRect As New Rectangle(0, 0, Glyph.VirtualBox.Width, Glyph.VirtualBox.Height)
                                        Dim DestRect As New Rectangle(x, y, Glyph.VirtualBox.Width, Glyph.VirtualBox.Height)
                                        g.DrawImage(SrcImage, DestRect, SrcRect, GraphicsUnit.Pixel)
                                        '下面这句因为.Net Framework 2.0内部错误源矩形会向右偏移1像素
                                        'g.DrawImage(SrcImage, x, y, SrcRect, GraphicsUnit.Pixel)
                                    End Using

                                    If c.HasUnicodes Then
                                        Dim ch As String = c.UnicodeString
                                        If EnablePhonetic AndAlso PhoneticDictionary.ContainsKey(ch) Then
                                            Dim s = PhoneticDictionary(ch)
                                            Dim OffsetX As Integer = (Width - g.MeasureStringWidth(s, PFont)) \ 2
                                            If g IsNot Nothing Then g.DrawString(s, PFont, Brushes.DimGray, x + OffsetX, y - Size, StringFormat.GenericTypographic)
                                        End If
                                    End If
                                    x += GetWidth(Width)
                                Else
                                    Dim ch As String = c.UnicodeString
                                    If (c.UnicodeString >= ChrQ(&H3040).ToString) AndAlso (c.UnicodeString < ChrQ(&H3100).ToString) Then
                                        Dim Width = g.MeasureStringWidth(ch, JPFont)
                                        g.DrawString(ch, JPFont, Brushes.Black, x, y, StringFormat.GenericTypographic)
                                        If EnablePhonetic AndAlso PhoneticDictionary.ContainsKey(ch) Then
                                            Dim s = PhoneticDictionary(ch)
                                            Dim OffsetX As Integer = (Width - g.MeasureStringWidth(s, PFont)) \ 2
                                            If g IsNot Nothing Then g.DrawString(s, PFont, Brushes.DimGray, x + OffsetX, y - Size, StringFormat.GenericTypographic)
                                        End If
                                        x += GetWidth(Width)
                                    Else
                                        Dim Width = g.MeasureStringWidth(ch, ZHFont)
                                        g.DrawString(ch, ZHFont, Brushes.Black, x, y, StringFormat.GenericTypographic)
                                        If EnablePhonetic AndAlso PhoneticDictionary.ContainsKey(ch) Then
                                            Dim s = PhoneticDictionary(ch)
                                            Dim OffsetX As Integer = (Width - g.MeasureStringWidth(s, PFont)) \ 2
                                            If g IsNot Nothing Then g.DrawString(s, PFont, Brushes.DimGray, x + OffsetX, y - Size, StringFormat.GenericTypographic)
                                        End If
                                        x += GetWidth(Width)
                                    End If
                                End If
                            Next

                            y += GetWidth(Size)
                            x = PadX
                        Next
                    End Using

                    Return Bitmap
                End Using
            End Using
        End Using
    End Function
End Class
