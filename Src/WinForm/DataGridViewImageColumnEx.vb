'==========================================================================
'
'  File:        DataGridViewImageColumnEx.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 数据表扩展图片框列
'  Version:     2009.10.05.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Windows.Forms

Public Class DataGridViewImageColumnEx
    Inherits DataGridViewColumn
    Public Sub New()
        MyBase.New(New DataGridViewImageCellEx())
    End Sub

    Public Overloads Overrides Property CellTemplate() As DataGridViewCell
        Get
            Return MyBase.CellTemplate
        End Get
        Set(ByVal value As DataGridViewCell)
            If Not (TypeOf value Is DataGridViewImageCellEx) Then
                Throw New InvalidCastException("CellTemplate must be a DataGridViewImageCellEx")
            End If

            MyBase.CellTemplate = value
        End Set
    End Property
End Class

Public Class DataGridViewImageCellEx
    Inherits DataGridViewImageCell

    Protected Overloads Overrides Sub Paint(ByVal graphics As Graphics, ByVal clipBounds As Rectangle, ByVal cellBounds As Rectangle, ByVal rowIndex As Integer, ByVal cellState As DataGridViewElementStates, ByVal value As Object, ByVal formattedValue As Object, ByVal errorText As String, ByVal cellStyle As DataGridViewCellStyle, ByVal advancedBorderStyle As DataGridViewAdvancedBorderStyle, ByVal paintParts As DataGridViewPaintParts)
        MyBase.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, Nothing, Nothing, errorText, cellStyle, advancedBorderStyle, paintParts)

        Dim BackColor = Color.LightBlue
        Using b As New Bitmap(cellBounds.Width - 1, cellBounds.Height - 1)
            Using g = System.Drawing.Graphics.FromImage(b)
                If Me.Selected Then
                    g.Clear(BackColor)
                Else
                    g.Clear(Color.White)
                End If

                Dim img = TryCast(formattedValue, Image)
                If img IsNot Nothing Then
                    Dim Width = b.Width
                    Dim Height = b.Height
                    If b.Width < img.Width Then
                        If img.Width > 0 Then Height = b.Width * img.Height / img.Width
                    Else
                        Width = img.Width
                        Height = img.Height
                    End If
                    If Me.Selected Then
                        Dim colorMatrixElements As Single()() = { _
                            New Single() {BackColor.R / 255, 0, 0, 0, 0}, _
                            New Single() {0, BackColor.G / 255, 0, 0, 0}, _
                            New Single() {0, 0, BackColor.B / 255, 0, 0}, _
                            New Single() {0, 0, 0, 1, 0}, _
                            New Single() {0, 0, 0, 0, 1}}

                        Dim colorMatrix As New ColorMatrix(colorMatrixElements)
                        Dim imageAttributes As New ImageAttributes
                        imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap)
                        g.DrawImage(img, New Rectangle(0, 0, Width, Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttributes)
                    Else
                        g.DrawImage(img, New Rectangle(0, 0, Width, Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel)
                    End If
                End If
            End Using
            graphics.DrawImage(b, cellBounds.Left, cellBounds.Top)
        End Using
    End Sub
End Class
