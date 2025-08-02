'==========================================================================
'
'  File:        WindowMain.xaml.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 主窗体
'  Version:     2025.08.03.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Diagnostics
Imports System.Reflection
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Interop
Imports Firefly
Imports Eddy.Interfaces
Imports Eddy.Base

Public Class WindowMain
    Private ApplicationData As New TextLocalizerData
    Private LocalizationTextBoxes As New List(Of LocalizationTextBox)
    Private KeyEventWatcher As New KeyEventWatcher

    Public Sub Initialize(ByVal ApplicationData As TextLocalizerData)
        Me.ApplicationData = ApplicationData
        Me.Title = ApplicationData.ApplicationName

        Me.Width = ApplicationData.CurrentProject.WindowWidth
        Me.Height = ApplicationData.CurrentProject.WindowHeight
        Me.WindowStartupLocation = WindowStartupLocation.CenterScreen

        Dim NumLocalizationTextBox = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors.Length
        For n = 0 To NumLocalizationTextBox - 1
            Dim Des = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(n)
            Dim L As New LocalizationTextBox
            With L
                .Name = String.Format("LocalizationTextBox{0}", n + 1)
                .TabIndex = n
                .Space = Des.Space
                .LocFontScale = Des.LocFontScale
                If Des.FontName <> "" Then
                    .FontName = Des.FontName
                    .FontPixel = Des.FontPixel
                End If
            End With
            LocalizationTextBoxes.Add(L)
        Next

        Dim MainPanelChildren As New List(Of UIElement)
        Dim RowDefinitions As New List(Of Controls.RowDefinition)
        For k = 0 To NumLocalizationTextBox * 2 - 1 - 1
            If k And 1 Then
                Dim Splitter As New Controls.GridSplitter With {.HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch}
                MainPanelChildren.Add(Splitter)
                RowDefinitions.Add(New Controls.RowDefinition With {.Height = New GridLength(3, GridUnitType.Pixel)})
                Controls.Grid.SetRow(Splitter, k)
            Else
                Dim n = k \ 2
                Dim Des = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(n)
                Dim L = LocalizationTextBoxes(n)
                MainPanelChildren.Add(L)
                RowDefinitions.Add(New Controls.RowDefinition With {.Height = New GridLength(Des.HeightRatio, GridUnitType.Star)})
                Controls.Grid.SetRow(L, k)
            End If
        Next

        Me.MainPanel.RowDefinitions.Clear()
        For Each rd In RowDefinitions
            Me.MainPanel.RowDefinitions.Add(rd)
        Next
        Me.MainPanel.Children.Clear()
        For Each c In MainPanelChildren
            Me.MainPanel.Children.Add(c)
        Next

        For n = 0 To NumLocalizationTextBox - 1
            Dim L = LocalizationTextBoxes(n)
            LocalizationTextBoxes(n).Init(ApplicationData.Columns(n))
        Next

        For Each L In LocalizationTextBoxes
            AddHandler L.TextBox.SelectionChanged, AddressOf Box_Scrolled
            AddHandler L.TextBox.TextChanged, AddressOf Box_TextChanged
            AddHandler L.GotFocus, AddressOf Box_GotFocus
        Next

        AddHandler ComponentDispatcher.ThreadFilterMessage, AddressOf PreFilterMessage

        For Each t In ApplicationData.TextNames
            ComboBox_TextName.Items.Add(t)
        Next

        For Each Plugin In ApplicationData.Plugins
            Dim ControllerPlugin = TryCast(Plugin, ITextLocalizerControllerPlugin)
            If ControllerPlugin IsNot Nothing Then
                ControllerPlugin.InitializeController(Me)
            End If
        Next

        ToolBar_Tools.Items.Clear()
        For Each ToolStripButtonPlugin In ApplicationData.ToolStripButtonPlugins
            Dim ButtonDescriptors = ToolStripButtonPlugin.GetToolStripButtonDescriptors()
            For Each ButtonDescriptor In ButtonDescriptors
                Dim bd = ButtonDescriptor
                Dim b As New Controls.Button
                b.Content = New Controls.Image With {.Source = ButtonDescriptor.Image.ToWpfImageSource, .Stretch = Stretch.None, .SnapsToDevicePixels = True}
                b.ToolTip = ButtonDescriptor.Text
                AddHandler b.Click, Sub(o, e) bd.Click()
                AddHandler bd.ImageChanged.Value, Sub(i) b.Content = New Controls.Image With {.Source = i.ToWpfImageSource, .Stretch = Stretch.None, .SnapsToDevicePixels = True}
                AddHandler bd.TextChanged.Value, Sub(t) b.ToolTip = t
                ToolBar_Tools.Items.Add(b)
            Next
        Next

        For Each KeyListenerPlugin In ApplicationData.KeyListenerPlugins
            Dim KeyListeners = KeyListenerPlugin.GetKeyListeners()
            For Each kl In KeyListeners
                KeyEventWatcher.Register(kl.KeyCombination, kl.EventType, kl.Handler)
            Next
        Next
        KeyEventWatcher.Register(New VirtualKeys() {VirtualKeys.F}, KeyEventType.Down, Sub() System.Diagnostics.Debug.WriteLine("F"))

        Dim DisplayLOCBoxTip = Visibility.Hidden
        For Each L In LocalizationTextBoxes
            If L.IsGlyphText Then
                DisplayLOCBoxTip = Visibility.Visible
                Exit For
            End If
        Next
        TextBlock_LOCBoxTip.Visibility = DisplayLOCBoxTip

        LocalizerEnable = False
        'UpdateToTextName(ApplicationData.CurrentProject.TextName, ApplicationData.CurrentProject.TextNumber - 1)
        VScrollBar_Bar.Focus()
    End Sub

    Private Sub Box_Scrolled(ByVal sender As Object, ByVal e As EventArgs)
        'RePositionBoxScrollBars()
    End Sub

    Private WithEvents Timer As New Forms.Timer
    Friend IMECompositing As Integer = 0
    Private Block As Integer = 0
    Private Sub Box_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles Timer.Tick
        If System.Threading.Interlocked.CompareExchange(IMECompositing, -1, -1) Then Return
        System.Threading.Interlocked.Exchange(Block, -1)
        Timer.Stop()
        'ReHighlight()
        System.Threading.Interlocked.Exchange(Block, 0)
    End Sub
    Private Sub Box_TextChanged(ByVal sender As Object, ByVal e As Controls.TextChangedEventArgs)
        If System.Threading.Interlocked.CompareExchange(Block, -1, -1) Then Return
        Timer.Stop()
        Timer.Interval = 500
        Timer.Start()
    End Sub
    Private MouseWheelHandled As Boolean = True
    Private Function DirectToInt32(ByVal i As IntPtr) As Int32
        If IntPtr.Size = 4 Then Return i.ToInt32
        If IntPtr.Size = 8 Then Return CID(i.ToInt64)
        Return CID(i.ToInt64)
    End Function
    Private Const WM_MOUSEWHEEL = 522
    Private Const WM_IME_STARTCOMPOSITION = &H10D
    Private Const WM_IME_ENDCOMPOSITION = &H10E
    Private Const WM_IME_NOTIFY = &H282
    Private Sub PreFilterMessage(ByRef m As Interop.MSG, ByRef Handled As Boolean)
        'Diagnostics.Debug.WriteLine(m.message.ToString("X8"))
        Select Case m.message
            Case WM_MOUSEWHEEL
                'Me.TextLocalizer_MouseWheel(Me, New MouseEventArgs(DirectToInt32(m.wParam) And &HFFFF, 0, DirectToInt32(m.lParam) And &HFFFF, (DirectToInt32(m.lParam.ToInt64) >> 16) And &HFFFF, CUS(CUShort((DirectToInt32(m.wParam) >> 16) And &HFFFF))))
                Dim h = MouseWheelHandled
                MouseWheelHandled = True
                Handled = h
            Case WM_IME_STARTCOMPOSITION
                System.Threading.Interlocked.Exchange(Me.IMECompositing, -1)
                Handled = False
            Case WM_IME_ENDCOMPOSITION
                System.Threading.Interlocked.Exchange(Me.IMECompositing, 0)
                Handled = False
            Case WM_IME_NOTIFY

                Handled = False
            Case Else
                Handled = False
        End Select
    End Sub

    Private CurrentColumnIndex As Integer = 0
    Private Sub Box_GotFocus(ByVal sender As Object, ByVal e As RoutedEventArgs)
        For n = 0 To ApplicationData.Columns.Count - 1
            If LocalizationTextBoxes(n).Focused Then
                CurrentColumnIndex = n
                RaiseEvent ColumnSelectionChanged()
                Return
            End If
        Next
    End Sub

    Public Property LocalizerEnable() As Boolean
        Get
            Return MainPanel.IsEnabled
        End Get
        Set(ByVal Value As Boolean)
            MainPanel.IsEnabled = Value
            VScrollBar_Bar.IsEnabled = Value
        End Set
    End Property

End Class
