'==========================================================================
'
'  File:        DataGridViewRowIndexHeaderCell.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 数据表索引号行头格
'  Version:     2009.10.07.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Windows.Forms

Public Class DataGridViewRowIndexHeaderCell
    Inherits DataGridViewRowHeaderCell

    Protected Overrides Function GetValue(ByVal rowIndex As Integer) As Object
        If rowIndex < 0 Then Return ""
        Dim Count = Me.DataGridView.RowCount
        If rowIndex >= Count Then Return ""
        If Count <= 0 Then Count = 1
        Dim k = Floor(Log(Count, 10)) + 1
        Dim s = (rowIndex + 1).ToString
        Return New String(" ", k - s.Length) & s
    End Function
End Class
