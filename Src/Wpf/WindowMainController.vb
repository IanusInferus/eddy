'==========================================================================
'
'  File:        WindowMainController.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 主窗体控制器实现
'  Version:     2010.12.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports Eddy
Imports Eddy.Interfaces

Partial Public NotInheritable Class WindowMain
    Implements ITextLocalizerApplicationController

    Public Event TextNameChanged() Implements ITextLocalizerApplicationController.TextNameChanged
    Public Event TextIndexChanged() Implements ITextLocalizerApplicationController.TextIndexChanged
    Public Event ColumnSelectionChanged() Implements ITextLocalizerApplicationController.ColumnSelectionChanged
    Public Sub RefreshGrid() Implements ITextLocalizerApplicationController.RefreshGrid

    End Sub
    Public Sub RefreshColumn(ByVal ColumnIndex As Integer) Implements ITextLocalizerApplicationController.RefreshColumn

    End Sub
    Public Sub RefreshMainPanel() Implements ITextLocalizerApplicationController.RefreshMainPanel

    End Sub
    Public Sub FlushLocalizedText() Implements ITextLocalizerApplicationController.FlushLocalizedText

    End Sub
    Public Sub Unload() Implements ITextLocalizerApplicationController.Unload

    End Sub
    Public Sub Reload() Implements ITextLocalizerApplicationController.Reload

    End Sub

    Public Property TextName As String Implements ITextLocalizerApplicationController.TextName
        Get

        End Get
        Set(ByVal Value As String)

        End Set
    End Property
    Public Property TextIndex As Integer Implements ITextLocalizerApplicationController.TextIndex
        Get

        End Get
        Set(ByVal Value As Integer)

        End Set
    End Property
    Public Property TextIndices As IEnumerable(Of Integer) Implements ITextLocalizerApplicationController.TextIndices
        Get

        End Get
        Set(ByVal Value As IEnumerable(Of Integer))

        End Set
    End Property
    Public Property ColumnIndex As Integer Implements ITextLocalizerApplicationController.ColumnIndex
        Get

        End Get
        Set(ByVal Value As Integer)

        End Set
    End Property
    Public Property SelectionStart As Integer Implements ITextLocalizerApplicationController.SelectionStart
        Get

        End Get
        Set(ByVal Value As Integer)

        End Set
    End Property
    Public Property SelectionLength As Integer Implements ITextLocalizerApplicationController.SelectionLength
        Get

        End Get
        Set(ByVal Value As Integer)

        End Set
    End Property
    Public Property Text(ByVal ColumnIndex As Integer) As String Implements ITextLocalizerApplicationController.Text
        Get

        End Get
        Set(ByVal Value As String)

        End Set
    End Property
    Public Sub ScrollToCaret(ByVal ColumnIndex As Integer) Implements ITextLocalizerApplicationController.ScrollToCaret

    End Sub

    Public ReadOnly Property MainWindow As WindowReference Implements ITextLocalizerApplicationController.MainWindow
        Get

        End Get
    End Property
    Public ReadOnly Property UIThreadAsyncInvoker As Action(Of Action) Implements ITextLocalizerApplicationController.UIThreadAsyncInvoker
        Get

        End Get
    End Property

    Public Sub ShowError(ByVal Message As String) Implements ITextLocalizerApplicationController.ShowError

    End Sub
    Public Sub ShowError(ByVal Message As String, ByVal Information As String) Implements ITextLocalizerApplicationController.ShowError

    End Sub
    Public Sub ShowInfo(ByVal Message As String) Implements ITextLocalizerApplicationController.ShowInfo

    End Sub
    Public Sub ShowInfo(ByVal Message As String, ByVal Information As String) Implements ITextLocalizerApplicationController.ShowInfo

    End Sub
    Public Function ShowYesNoQuestion(ByVal Message As String) As Boolean Implements ITextLocalizerApplicationController.ShowYesNoQuestion

    End Function
    Public Function ShowYesNoQuestion(ByVal Message As String, ByVal Information As String) As Boolean Implements ITextLocalizerApplicationController.ShowYesNoQuestion

    End Function
End Class
