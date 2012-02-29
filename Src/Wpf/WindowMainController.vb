'==========================================================================
'
'  File:        WindowMainController.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 主窗体控制器实现
'  Version:     2012.02.29.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Diagnostics
Imports System.Windows.Interop
Imports Eddy
Imports Eddy.Interfaces

Partial Public NotInheritable Class WindowMain
    Implements ITextLocalizerApplicationController

    Public Event TextNameChanged() Implements ITextLocalizerApplicationController.TextNameChanged
    Public Event TextIndexChanged() Implements ITextLocalizerApplicationController.TextIndexChanged
    Public Event ColumnSelectionChanged() Implements ITextLocalizerApplicationController.ColumnSelectionChanged
    Public Sub RefreshGrid() Implements ITextLocalizerApplicationController.RefreshGrid
        Throw New NotImplementedException
    End Sub
    Public Sub RefreshColumn(ByVal ColumnIndex As Integer) Implements ITextLocalizerApplicationController.RefreshColumn
        Throw New NotImplementedException
    End Sub
    Public Sub RefreshMainPanel() Implements ITextLocalizerApplicationController.RefreshMainPanel
        Throw New NotImplementedException
    End Sub
    Public Sub FlushLocalizedText() Implements ITextLocalizerApplicationController.FlushLocalizedText
        Throw New NotImplementedException
    End Sub
    Public Sub Unload() Implements ITextLocalizerApplicationController.Unload
        Throw New NotImplementedException
    End Sub
    Public Sub Reload() Implements ITextLocalizerApplicationController.Reload
        Throw New NotImplementedException
    End Sub

    Public Property TextName As String Implements ITextLocalizerApplicationController.TextName
        Get
            Throw New NotImplementedException
        End Get
        Set(ByVal Value As String)
            Throw New NotImplementedException
        End Set
    End Property
    Public Property TextIndex As Integer Implements ITextLocalizerApplicationController.TextIndex
        Get
            Throw New NotImplementedException
        End Get
        Set(ByVal Value As Integer)
            Throw New NotImplementedException
        End Set
    End Property
    Public Property TextIndices As IEnumerable(Of Integer) Implements ITextLocalizerApplicationController.TextIndices
        Get
            Throw New NotImplementedException
        End Get
        Set(ByVal Value As IEnumerable(Of Integer))
            Throw New NotImplementedException
        End Set
    End Property
    Public Property ColumnIndex As Integer Implements ITextLocalizerApplicationController.ColumnIndex
        Get
            Throw New NotImplementedException
        End Get
        Set(ByVal Value As Integer)
            Throw New NotImplementedException
        End Set
    End Property
    Public Property SelectionStart As Integer Implements ITextLocalizerApplicationController.SelectionStart
        Get
            Throw New NotImplementedException
        End Get
        Set(ByVal Value As Integer)
            Throw New NotImplementedException
        End Set
    End Property
    Public Property SelectionLength As Integer Implements ITextLocalizerApplicationController.SelectionLength
        Get
            Throw New NotImplementedException
        End Get
        Set(ByVal Value As Integer)
            Throw New NotImplementedException
        End Set
    End Property
    Public Property Text(ByVal ColumnIndex As Integer) As String Implements ITextLocalizerApplicationController.Text
        Get
            Throw New NotImplementedException
        End Get
        Set(ByVal Value As String)
            Throw New NotImplementedException
        End Set
    End Property
    Public Sub ScrollToCaret(ByVal ColumnIndex As Integer) Implements ITextLocalizerApplicationController.ScrollToCaret
        Throw New NotImplementedException
    End Sub

    Public ReadOnly Property MainWindow As WindowReference Implements ITextLocalizerApplicationController.MainWindow
        Get
            Dim WindowHandle = New WindowInteropHelper(Me).Handle
            Return New WindowReference With {.Handle = WindowHandle}
        End Get
    End Property
    Public ReadOnly Property UIThreadAsyncInvoker As Action(Of Action) Implements ITextLocalizerApplicationController.UIThreadAsyncInvoker
        Get
            Return Sub(a) Dispatcher.BeginInvoke(a)
        End Get
    End Property

    Private Shared Function GetAssemblyTitle(ByVal a As Assembly) As String
        Dim Attributes = a.GetCustomAttributes(GetType(AssemblyTitleAttribute), True)
        If Attributes.Length >= 1 Then
            Dim Str = DirectCast(Attributes(0), AssemblyTitleAttribute).Title
            If Str <> "" Then Return Str
        End If

        Return ""
    End Function
    Private Shared Function GetAssemblyDescription(ByVal a As Assembly) As String
        Dim Attributes = a.GetCustomAttributes(GetType(AssemblyDescriptionAttribute), True)
        If Attributes.Length >= 1 Then
            Dim Str = DirectCast(Attributes(0), AssemblyDescriptionAttribute).Description
            If Str <> "" Then Return Str
        End If

        Return ""
    End Function
    Private Shared Function GetAssemblyDescriptionOrTitle(ByVal a As Assembly) As String
        Dim Description = GetAssemblyDescription(a)
        If Description <> "" Then Return Description

        Dim Title = GetAssemblyTitle(a)
        If Title <> "" Then Return Title

        Return ""
    End Function
    Public Function GetPluginDescriptionOrTitle() As String
        Dim t As New StackTrace(2, False)
        If t.FrameCount > 0 Then
            Dim f = t.GetFrame(0)
            Dim m = f.GetMethod()
            Dim a = m.Module.Assembly
            Return GetAssemblyDescriptionOrTitle(a)
        Else
            Return ""
        End If
    End Function
    Public Sub ShowError(ByVal Message As String) Implements ITextLocalizerApplicationController.ShowError
        Throw New NotImplementedException
    End Sub
    Public Sub ShowError(ByVal Message As String, ByVal Information As String) Implements ITextLocalizerApplicationController.ShowError
        Throw New NotImplementedException
    End Sub
    Public Sub ShowInfo(ByVal Message As String) Implements ITextLocalizerApplicationController.ShowInfo
        Throw New NotImplementedException
    End Sub
    Public Sub ShowInfo(ByVal Message As String, ByVal Information As String) Implements ITextLocalizerApplicationController.ShowInfo
        Throw New NotImplementedException
    End Sub
    Public Function ShowYesNoQuestion(ByVal Message As String) As Boolean Implements ITextLocalizerApplicationController.ShowYesNoQuestion
        Throw New NotImplementedException
    End Function
    Public Function ShowYesNoQuestion(ByVal Message As String, ByVal Information As String) As Boolean Implements ITextLocalizerApplicationController.ShowYesNoQuestion
        Throw New NotImplementedException
    End Function
End Class
