'==========================================================================
'
'  File:        FormMain.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 文本本地化工具主窗体
'  Version:     2010.10.24.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.GUI
Imports Eddy.Interfaces

Public Class FormMain
    Implements ITextLocalizerApplicationController

    Private ApplicationData As New TextLocalizerData

    Private LocalizationTextBoxes As New List(Of LocalizationTextBox)

    Public Sub Initialize(ByVal ApplicationData As TextLocalizerData)
        Me.ApplicationData = ApplicationData
        Me.Text = ApplicationData.ApplicationName

        Application.AddMessageFilter(New Intercepter)

        DataGridView_Multiview.SuspendDraw()
        Try
            Me.CenterToScreen()

            Me.Width = ApplicationData.CurrentProject.WindowWidth
            Me.Height = ApplicationData.CurrentProject.WindowHeight

            Me.Panel_LocalizationBoxes.SuspendLayout()
            Me.SuspendLayout()

            For Each c As Control In Me.Panel_LocalizationBoxes.Controls
                c.Dispose()
            Next
            Me.Panel_LocalizationBoxes.Controls.Clear()

            Dim Splitters As New List(Of Splitter)
            Dim Height As Integer = 0
            Dim Des As LocalizationTextBoxDescriptor
            For n As Integer = 0 To ApplicationData.CurrentProject.LocalizationTextBoxDescriptors.Length - 2
                Des = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(n)
                Dim L As New LocalizationTextBox
                With L
                    .Dock = System.Windows.Forms.DockStyle.Top
                    .Location = New System.Drawing.Point(0, Height)
                    .Name = String.Format("LocalizationTextBox{0}", n + 1)
                    Height += Des.HeightRatio * Panel_LocalizationBoxes.Height
                    .TabIndex = n

                    .Space = Des.Space
                    If Des.FontName <> "" Then .Font = New Font(Des.FontName, Des.FontPixel, FontStyle.Regular, GraphicsUnit.Pixel)

                    .Size = New System.Drawing.Size(Me.Panel_LocalizationBoxes.Width, Des.HeightRatio * Panel_LocalizationBoxes.Height)
                End With
                LocalizationTextBoxes.Add(L)

                Dim S As New Splitter
                With S
                    .Dock = System.Windows.Forms.DockStyle.Top
                    .Location = New System.Drawing.Point(0, Height)
                    .Name = String.Format("Splitter{0}", n + 1)
                    .Size = New System.Drawing.Size(Me.Panel_LocalizationBoxes.Width, 3)
                    Height += S.Height
                    .TabStop = False
                    .BackColor = System.Drawing.SystemColors.ScrollBar
                    .BorderStyle = BorderStyle.None
                End With
                Splitters.Add(S)
            Next
            Des = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(ApplicationData.CurrentProject.LocalizationTextBoxDescriptors.Length - 1)
            Dim LL As New LocalizationTextBox
            With LL
                .Dock = System.Windows.Forms.DockStyle.Fill
                .Location = New System.Drawing.Point(0, Height)
                .Name = String.Format("LocalizationTextBox{0}", ApplicationData.CurrentProject.LocalizationTextBoxDescriptors.Length)
                .TabIndex = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors.Length - 1

                .Space = Des.Space
                If Des.FontName <> "" Then .Font = New Font(Des.FontName, Des.FontPixel, FontStyle.Regular, GraphicsUnit.Pixel)

                .Size = New System.Drawing.Size(Me.Panel_LocalizationBoxes.Width, Me.Panel_LocalizationBoxes.Height - Height)
            End With
            LocalizationTextBoxes.Add(LL)

            Me.Panel_LocalizationBoxes.Controls.Add(LL)
            For n = LocalizationTextBoxes.Count - 2 To 0 Step -1
                Me.Panel_LocalizationBoxes.Controls.Add(Splitters(n))
                Me.Panel_LocalizationBoxes.Controls.Add(LocalizationTextBoxes(n))
            Next

            For n As Integer = 0 To ApplicationData.CurrentProject.LocalizationTextBoxDescriptors.Length - 1
                Dim L = LocalizationTextBoxes(n)
                LocalizationTextBoxes(n).Init(ApplicationData.Columns(n))
            Next

            For Each L In LocalizationTextBoxes
                AddHandler L.TextBox.SelectionChanged, AddressOf BoxScrolled
                AddHandler L.TextBox.TextChanged, AddressOf Box_TextChanged
                AddHandler L.GotFocus, AddressOf Box_GotFocus
            Next

            For Each L In LocalizationTextBoxes
                Dim tp = L.TextProvider
                If ApplicationData.CurrentProject.EnableLocalizationGrid Then
                    DataGridView_Multiview.RowTemplate = New DataGridViewRow With {.HeaderCell = New DataGridViewRowIndexHeaderCell}
                    If L.IsGlyphText Then
                        DataGridView_Multiview.Columns.Add(New DataGridViewImageColumnEx With {.Name = tp.Name, .HeaderText = tp.DisplayName, .CellTemplate = New DataGridViewImageCellEx With {.ValueIsIcon = False, .ImageLayout = DataGridViewImageCellLayout.Zoom}})
                    Else
                        DataGridView_Multiview.Columns.Add(New DataGridViewRichTextBoxColumn With {.Name = tp.Name, .HeaderText = tp.DisplayName})
                    End If
                End If
            Next

            If ApplicationData.CurrentProject.EnableLocalizationGrid Then
                DataGridView_Multiview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
                SplitContainer_Main.SplitterDistance = ApplicationData.CurrentProject.LocalizationGridWidthRatio * Me.Width
                Dim RowHeadersWidth As Integer = ApplicationData.CurrentProject.LocalizationRowHeaderWidthRatio * SplitContainer_Main.SplitterDistance
                If RowHeadersWidth < 4 Then RowHeadersWidth = 4
                DataGridView_Multiview.RowHeadersWidth = RowHeadersWidth
                For n = 0 To ApplicationData.Columns.Count - 1
                    Dim c = DataGridView_Multiview.Columns(n)
                    c.Width = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(n).ColumnWidthRatio * SplitContainer_Main.SplitterDistance
                Next
                If ApplicationData.CurrentProject.LocalizationGridAutoResizeWidth Then
                    SplitContainer_Main.FixedPanel = FixedPanel.None
                    DataGridView_Multiview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                    Dim Sum = DataGridView_Multiview.RowHeadersWidth + (From c As DataGridViewColumn In DataGridView_Multiview.Columns Select c.Width).Sum
                    RowHeadersWidth = ApplicationData.CurrentProject.LocalizationRowHeaderWidthRatio * Sum
                    If RowHeadersWidth < 4 Then RowHeadersWidth = 4
                    DataGridView_Multiview.RowHeadersWidth = RowHeadersWidth
                    For n = 0 To ApplicationData.Columns.Count - 1
                        DataGridView_Multiview.Columns(n).Width = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(n).ColumnWidthRatio * Sum
                    Next
                Else
                    SplitContainer_Main.FixedPanel = FixedPanel.Panel2
                    DataGridView_Multiview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
                End If
            Else
                SplitContainer_Main.FixedPanel = FixedPanel.Panel1
                DataGridView_Multiview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
                SplitContainer_Main.SplitterDistance = 0
                SplitContainer_Main.IsSplitterFixed = True
                Me.Width = ApplicationData.CurrentProject.WindowWidth
                Me.Height = ApplicationData.CurrentProject.WindowHeight
            End If

            For Each t In ApplicationData.TextNames
                ComboBox_TextName.Items.Add(t)
            Next

            For Each Plugin In ApplicationData.Plugins
                Dim ControllerPlugin = TryCast(Plugin, ITextLocalizerControllerPlugin)
                If ControllerPlugin IsNot Nothing Then
                    ControllerPlugin.InitializeController(Me)
                End If
            Next

            For Each ControlPlugin In ApplicationData.ControlPlugins
                Dim ControlDescriptors = ControlPlugin.GetControlDescriptors()
                If ControlDescriptors IsNot Nothing Then
                    For Each d In ControlDescriptors
                        Select Case d.Target
                            Case ControlId.MainWindow
                                Me.SuspendLayout()
                                Me.Controls.Add(d.Control)
                                d.Control.BringToFront()
                                Me.ResumeLayout(False)
                            Case ControlId.MainPanel
                                Me.Panel_Work.SuspendLayout()
                                Me.Panel_Work.Controls.Add(d.Control)
                                d.Control.BringToFront()
                                Me.Panel_Work.ResumeLayout(False)
                            Case ControlId.Grid
                                Me.SplitContainer_Main.Panel1.SuspendLayout()
                                Me.SplitContainer_Main.Panel1.Controls.Add(d.Control)
                                d.Control.BringToFront()
                                Me.SplitContainer_Main.Panel1.ResumeLayout(False)
                            Case ControlId.ToolStrip
                                Me.ToolStrip_Tools.Items.Add(d.Control)
                        End Select
                    Next
                End If
            Next

            Dim DisplayLOCBoxTip = False
            For Each L In LocalizationTextBoxes
                If L.IsGlyphText Then
                    DisplayLOCBoxTip = True
                    Exit For
                End If
            Next
            Label_LOCBoxTip.Visible = DisplayLOCBoxTip

            Me.Panel_LocalizationBoxes.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

            MeWidth = Me.Width
            MeHeight = Me.Height
            MainPanelHeight = Panel_LocalizationBoxes.Height
            LocalizationTextBoxHeights = GetLocalizationTextBoxHeights()

            If ApplicationData.CurrentProject.Maximized Then
                Me.WindowState = FormWindowState.Maximized
                Panel_LocalizationBoxes_Resize(Nothing, Nothing)
            End If

            LocalizerEnable = False

            UpdateToTextName(ApplicationData.CurrentProject.TextName, ApplicationData.CurrentProject.TextNumber - 1)
            VScrollBar_Bar.Select()

        Finally
            DataGridView_Multiview.ResumeDraw()
            DataGridView_Multiview.Invalidate()
        End Try
    End Sub

    Private Sub UpdateViewStateToConfig()
        If Panel_LocalizationBoxes.Height > 0 Then
            For n As Integer = 0 To ApplicationData.Columns.Count - 1
                Dim Des = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(n)
                Des.HeightRatio = LocalizationTextBoxHeights(n) / Panel_LocalizationBoxes.Height
            Next
        End If
        If ApplicationData.CurrentProject.EnableLocalizationGrid Then
            If FormWindowState.Maximized AndAlso Not ApplicationData.CurrentProject.LocalizationGridAutoResizeWidth Then
                Dim SplitContainer_Main_SplitterDistance = ApplicationData.CurrentProject.WindowWidth - (MeWidth - SplitContainer_Main.SplitterDistance)
                If ApplicationData.CurrentProject.WindowWidth > 0 Then ApplicationData.CurrentProject.LocalizationGridWidthRatio = SplitContainer_Main_SplitterDistance / ApplicationData.CurrentProject.WindowWidth
                If SplitContainer_Main_SplitterDistance > 0 Then
                    ApplicationData.CurrentProject.LocalizationRowHeaderWidthRatio = DataGridView_Multiview.RowHeadersWidth / SplitContainer_Main_SplitterDistance
                    For n = 0 To ApplicationData.Columns.Count - 1
                        ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(n).ColumnWidthRatio = DataGridView_Multiview.Columns(n).Width / SplitContainer_Main_SplitterDistance
                    Next
                End If
            Else
                If MeWidth > 0 Then ApplicationData.CurrentProject.LocalizationGridWidthRatio = SplitContainer_Main.SplitterDistance / MeWidth
                Dim Sum = DataGridView_Multiview.RowHeadersWidth + (From c As DataGridViewColumn In DataGridView_Multiview.Columns Select c.Width).Sum
                If Sum > 0 Then
                    ApplicationData.CurrentProject.LocalizationRowHeaderWidthRatio = DataGridView_Multiview.RowHeadersWidth / Sum
                    For n = 0 To ApplicationData.Columns.Count - 1
                        ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(n).ColumnWidthRatio = DataGridView_Multiview.Columns(n).Width / Sum
                    Next
                End If
            End If
        End If
    End Sub

#Region " 设置 "
    Private Sub TextLocalizer_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
#If CONFIG <> "Debug" Then
        Try
#End If
        '不要在这里写代码，由于Windows x64的一个bug，导致在调试时在这里抛出的异常无法捕捉
#If CONFIG <> "Debug" Then
        Catch ex As Exception
            ExceptionHandler.PopupException(ex)
            End
        End Try
#End If
    End Sub
    Private Sub TextLocalizer_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
#If CONFIG <> "Debug" Then
        Try
#End If
        '不要在这里写代码，由于Windows x64的一个bug，导致在调试时在这里抛出的异常无法捕捉
#If CONFIG <> "Debug" Then
        Catch ex As Exception
            ExceptionHandler.PopupException(ex)
            End
        End Try
#End If
    End Sub

    Public Sub FlushLocalizedText()
        For Each L In LocalizationTextBoxes
            If L.TextModified Then L.SaveText()
        Next
    End Sub
    Private Sub TextLocalizer_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
#If CONFIG <> "Debug" Then
        Try
#End If
        '不要在这里多写代码，由于Windows x64的一个bug，导致在调试时在这里抛出的异常无法捕捉
        FlushLocalizedText()
        UpdateViewStateToConfig()
#If CONFIG <> "Debug" Then
        Catch ex As Exception
            ExceptionHandler.PopupException(ex)
            End
        End Try
#End If
    End Sub

#End Region

    Private RichTextBoxForText As New ExtendedRichTextBox
    Private GridRows As Object()()
    Private Function IsGridViewRowValueInitialize() As Boolean
        Return GridRows IsNot Nothing
    End Function
    Private Function IsGridViewRowValueCached(ByVal TextIndex As Integer) As Boolean
        Dim r = GridRows(TextIndex)
        Return r IsNot Nothing
    End Function
    Private Function GetGridViewRowValue(ByVal TextIndex As Integer) As Object()
        Return GridRows(TextIndex)
    End Function
    Private Sub UpdateGridRowValue(ByVal TextIndex As Integer)
        Dim r = GridRows(TextIndex)
        If r Is Nothing Then
            r = New Object(ApplicationData.Columns.Count - 1) {}
            GridRows(TextIndex) = r
        End If
        Dim Texts = New String(ApplicationData.Columns.Count - 1) {}
        For TextColumn = 0 To ApplicationData.Columns.Count - 1
            Dim L = LocalizationTextBoxes(TextColumn)
            If L.IsGlyphText Then
                Texts(TextColumn) = ""
            Else
                If L.TextProvider.ContainsKey(TextName) Then
                    Dim TextList = L.TextProvider.Item(TextName)
                    If TextIndex < TextList.Count Then
                        Texts(TextColumn) = TextList.Text(TextIndex)
                    Else
                        Texts(TextColumn) = ""
                    End If
                Else
                    Texts(TextColumn) = ""
                    Dim k = TextColumn
                    Dim tt = Function(Text) TranslateText(ApplicationData.MainColumnIndex, k, Text)
                    Dim TextList = L.TextProvider.LoadOrCreateItem(TextName, ApplicationData.Columns(ApplicationData.MainColumnIndex).Item(TextName), tt)
                    If TextList IsNot Nothing Then
                        If TextIndex < TextList.Count Then
                            Texts(TextColumn) = TextList.Text(TextIndex)
                        Else
                            Texts(TextColumn) = ""
                        End If
                    End If
                End If
            End If
        Next
        If ApplicationData.GridTextFormatters.Count > 0 Then
            For Each GridTextFormatter In ApplicationData.GridTextFormatters
                Texts = GridTextFormatter.Format(TextName, TextIndex, Texts).ToArray
            Next
        End If
        For TextColumn = 0 To ApplicationData.Columns.Count - 1
            Dim L = LocalizationTextBoxes(TextColumn)
            If L.TextProvider.ContainsKey(TextName) Then
                If L.IsGlyphText Then
                    Dim TextListLOC = CType(L.TextProvider.Item(TextName), LOCList)
                    Dim Image = TextListLOC.GetBitmap(L.FontPixel, L.FontPixel, TextIndex, L.Space)
                    If Image Is Nothing Then
                        Image = New Bitmap(1, 1)
                        Using g = Graphics.FromImage(Image)
                            g.Clear(Color.White)
                        End Using
                    End If
                    r(TextColumn) = Image
                Else
                    Dim Text As String = Texts(TextColumn)
                    RichTextBoxForText.Text = Text
                    r(TextColumn) = RichTextBoxForText.Rtf
                End If
            Else
                If L.IsGlyphText Then
                    Dim Image = New Bitmap(1, 1)
                    Using g = Graphics.FromImage(Image)
                        g.Clear(Color.White)
                    End Using
                    r(TextColumn) = Image
                Else
                    RichTextBoxForText.Text = ""
                    r(TextColumn) = RichTextBoxForText.Rtf
                End If
            End If
        Next
        If ApplicationData.TextHighlighters.Count > 0 Then
            Dim Highlights = (From h In ApplicationData.TextHighlighters Select h.GetTextStyles(TextName, TextIndex, Texts)).ToArray
            For TextColumn = 0 To ApplicationData.Columns.Count - 1
                Dim L = LocalizationTextBoxes(TextColumn)
                If L.TextProvider.ContainsKey(TextName) Then
                    If L.IsGlyphText Then
                    Else
                        RichTextBoxForText.Rtf = r(TextColumn)
                        For Each h In Highlights
                            If h Is Nothing Then Continue For
                            If h(TextColumn) Is Nothing Then Continue For
                            For Each ts In h(TextColumn)
                                If ts.Length < 0 OrElse ts.Index < 0 OrElse ts.Index + ts.Length > RichTextBoxForText.TextLength Then Continue For
                                RichTextBoxForText.SetTextColor(ts.Index, ts.Length, ts.ForeColor, ts.BackColor)
                            Next
                        Next
                        r(TextColumn) = RichTextBoxForText.Rtf
                    End If
                End If
            Next
        End If
    End Sub
    Private Sub UpdateGridTextIndex(ByVal row As DataGridViewRow, ByVal TextIndex As Integer)
        UpdateGridRowValue(TextIndex)
        Dim r = GetGridViewRowValue(TextIndex)
        For TextColumn = 0 To ApplicationData.Columns.Count - 1
            row.Cells(TextColumn).Value = r(TextColumn)
        Next
    End Sub

    Private ReadOnly Property TextCount() As Integer
        Get
            If LocalizationTextBoxes Is Nothing Then Return 0
            Return LocalizationTextBoxes(ApplicationData.MainColumnIndex).TextCount
        End Get
    End Property

    Private Function TranslateText(ByVal SourceColumn As Integer, ByVal TargetColumn As Integer, ByVal s As String) As String
        Dim ret As String = s
        For Each t In ApplicationData.TranslatorPlugins
            ret = t.TranslateText(SourceColumn, TargetColumn, ret)
        Next
        Return ret
    End Function

    Private Property TextNameValue As String
        Get
            Return ApplicationData.CurrentProject.TextName
        End Get
        Set(ByVal Value As String)
            ApplicationData.CurrentProject.TextName = Value
        End Set
    End Property
    Private Sub UpdateToTextName(ByVal TextName As String, ByVal TextIndex As Integer)
        Dim Value = TextName

        TextNameValue = Value
        ComboBox_TextName.Text = Value
        If Value = "" Then
            Me.Text = ApplicationData.ApplicationName
            Return
        Else
            Me.Text = ApplicationData.ApplicationName & " - " & Value
        End If
        FlushLocalizedText()

        For Each L In LocalizationTextBoxes
            L.LoadText(TextName)
        Next
        LocalizerEnable = False
        Dim MaxCount As Integer = TextCount
        If MaxCount <= 0 Then
            VScrollBar_Bar.Minimum = 1
            VScrollBar_Bar.Maximum = 1
            NumericUpDown_TextNumber.Minimum = 1
            NumericUpDown_TextNumber.Maximum = 1
            Return
        End If
        Dim k = 0
        For Each L In LocalizationTextBoxes
            If Not L.IsLoaded Then
                Dim tt = Function(Text) TranslateText(ApplicationData.MainColumnIndex, k, Text)
                L.LoadOrCreateText(TextName, ApplicationData.Columns(ApplicationData.MainColumnIndex).Item(TextName), tt)
            End If
            If Not L.IsReadOnly Then
                If L.TextCount <> MaxCount Then
                    Throw New InvalidDataException("{0}文本条数{1}与主文本条数{2}不一致".Formats(GetPath(L.TextProvider.Directory, TextName & "." & L.TextProvider.Extension), L.TextCount, MaxCount))
                End If
            End If
            k += 1
        Next

        FlushLocalizedText()

        Me.SuspendLayout()
        If ApplicationData.CurrentProject.EnableLocalizationGrid Then
            Dim NumRow = LocalizationTextBoxes(ApplicationData.MainColumnIndex).TextCount
            DataGridView_Multiview.CausesValidation = False
            GridRows = New Object(NumRow - 1)() {}
            DataGridView_Multiview.RowCount = NumRow
        End If

        VScrollBar_Bar.Minimum = 1
        VScrollBar_Bar.Maximum = MaxCount
        NumericUpDown_TextNumber.Minimum = 1
        NumericUpDown_TextNumber.Maximum = MaxCount
        UpdateToTextIndex(TextIndex)
        LocalizerEnable = True
        Me.ResumeLayout(False)

        RaiseEvent TextNameChanged(Nothing)
    End Sub
    Public Property TextName() As String
        Get
            Return TextNameValue
        End Get
        Set(ByVal Value As String)
            If TextNameValue = Value Then Return
            UpdateToTextName(Value, 0)
        End Set
    End Property

    Public Property LocalizerEnable() As Boolean
        Get
            Return Panel_LocalizationBoxes.Enabled
        End Get
        Set(ByVal Value As Boolean)
            Panel_LocalizationBoxes.Enabled = Value
            VScrollBar_Bar.Enabled = Value
        End Set
    End Property

    Private Property TextIndexValue As Integer
        Get
            Return ApplicationData.CurrentProject.TextNumber - 1
        End Get
        Set(ByVal Value As Integer)
            ApplicationData.CurrentProject.TextNumber = Value + 1
        End Set
    End Property
    Public Property TextNumber() As Integer
        Get
            Return TextIndex + 1
        End Get
        Set(ByVal Value As Integer)
            TextIndex = Value - 1
        End Set
    End Property
    Public Property TextIndex() As Integer
        Get
            Return TextIndexValue
        End Get
        Set(ByVal Value As Integer)
            If TextIndexValue = Value Then Return
            UpdateToTextIndex(Value)
        End Set
    End Property
    Private Sub UpdateToTextIndex(ByVal TextIndex As Integer)
        Dim Value = TextIndex
        FlushLocalizedText()
        If ApplicationData.CurrentProject.EnableLocalizationGrid Then
            If TextIndexValue >= 0 AndAlso TextIndexValue < TextCount Then
                UpdateGridTextIndex(DataGridView_Multiview.Rows(TextIndexValue), TextIndexValue)
                DataGridView_Multiview.InvalidateRow(TextIndexValue)
            End If
        End If
        If Value < VScrollBar_Bar.Minimum - 1 Then Return 'Value = VScrollBar_Bar.Minimum - 1
        If Value > VScrollBar_Bar.Maximum - 1 Then Return 'Value = VScrollBar_Bar.Maximum - 1
        TextIndexValue = Value
        VScrollBar_Bar.Value = Value + 1
        For Each L In LocalizationTextBoxes
            L.TextIndex = Value
        Next
        NumericUpDown_TextNumber.Value = VScrollBar_Bar.Value
        ReHighlight()
        If ApplicationData.CurrentProject.EnableLocalizationGrid Then
            If BlockCell Then Return
            BlockCell = True
            Dim First = DataGridView_Multiview.FirstDisplayedScrollingRowIndex
            Dim Count = DataGridView_Multiview.DisplayedRowCount(False)
            Dim Index = Value
            If Index < First OrElse Index >= First + Count Then
                DataGridView_Multiview.FirstDisplayedScrollingRowIndex = Index
            End If
            DataGridView_Multiview.Refresh()
            For Each r As DataGridViewRow In DataGridView_Multiview.Rows
                r.Selected = False
            Next
            DataGridView_Multiview.Rows(Index).Selected = True
            If DataGridView_Multiview.CurrentCell Is Nothing AndAlso DataGridView_Multiview.ColumnCount > 0 Then
                DataGridView_Multiview.CurrentCell = DataGridView_Multiview.Rows(Index).Cells(0)
            ElseIf DataGridView_Multiview.CurrentCell.RowIndex <> Index Then
                DataGridView_Multiview.CurrentCell = DataGridView_Multiview.Rows(Index).Cells(DataGridView_Multiview.CurrentCell.ColumnIndex)
            End If
            BlockCell = False
        End If
        RaiseEvent TextIndexChanged(Nothing)
    End Sub

    Private Sub RePositionBoxScrollBars()
        If Application_ColumnIndex < 0 OrElse Application_ColumnIndex >= LocalizationTextBoxes.Count Then Return
        Dim Foucsed = LocalizationTextBoxes(Application_ColumnIndex)
        Dim si = Foucsed.TextBox.VerticalScrollInformation
        If si Is Nothing Then Return
        If si.Maximum - si.Minimum <= 0 Then Return
        Dim Ratio = si.Position / (si.Maximum - si.Minimum)

        For Each l In LocalizationTextBoxes
            If l Is Foucsed Then Continue For
            Dim lsi = l.TextBox.VerticalScrollInformation
            If lsi Is Nothing Then Continue For
            If lsi.Maximum - lsi.Minimum <= 0 Then Continue For
            Dim n = lsi.Minimum + CInt(Ratio * (lsi.Maximum - lsi.Minimum))
            If n < lsi.Minimum Then n = lsi.Minimum
            If n > lsi.Maximum Then n = lsi.Maximum
            If Abs(n - lsi.Position) < l.Font.Height * 1.5 Then Continue For
            l.TextBox.ScrollPosition = n
        Next
    End Sub

    Private Sub ReHighlight()
        For Each l In LocalizationTextBoxes
            Dim Source = l.TextBox
            Source.SuspendDraw()
            Source.SuspendUndoHistory()
            Dim SourceStart = Source.SelectionStart
            Dim SourceLength = Source.SelectionLength
            Source.SelectAll()
            Source.SelectionColor = System.Drawing.SystemColors.ControlText
            Source.SelectionBackColor = System.Drawing.SystemColors.Window
            Source.Select(SourceStart, SourceLength)
        Next
        If ApplicationData.TextHighlighters.Count > 0 Then
            Dim Texts = New String(ApplicationData.Columns.Count - 1) {}
            For TextColumn = 0 To ApplicationData.Columns.Count - 1
                Dim L = LocalizationTextBoxes(TextColumn)
                Dim SourceText = L.TextBox.Text
                If SourceText Is Nothing Then SourceText = ""
                Texts(TextColumn) = SourceText
            Next
            Dim Highlights = (From h In ApplicationData.TextHighlighters Select h.GetTextStyles(TextName, TextIndex, Texts)).ToArray
            For TextColumn = 0 To ApplicationData.Columns.Count - 1
                Dim L = LocalizationTextBoxes(TextColumn)
                Dim Source = L.TextBox
                Dim SourceStart = Source.SelectionStart
                Dim SourceLength = Source.SelectionLength
                For Each h In Highlights
                    If h Is Nothing Then Continue For
                    If h(TextColumn) Is Nothing Then Continue For
                    For Each ts In h(TextColumn)
                        If ts.Length < 0 OrElse ts.Index < 0 OrElse ts.Index + ts.Length > Source.TextLength Then Continue For
                        Source.SetTextColor(ts.Index, ts.Length, ts.ForeColor, ts.BackColor)
                    Next
                Next
                Source.Select(SourceStart, SourceLength)
            Next
        End If
        For Each l In LocalizationTextBoxes
            Dim Source = l.TextBox
            Source.ResumeDraw()
            Source.ResumeUndoHistory()
            Source.Invalidate()
        Next
    End Sub

    Private Sub BoxScrolled(ByVal sender As Object, ByVal e As EventArgs)
        RePositionBoxScrollBars()
    End Sub

    Private WithEvents Timer As New Timer
    Friend IMECompositing As Integer = 0
    Private Block As Integer = 0
    Private Sub Box_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles Timer.Tick
        If System.Threading.Interlocked.CompareExchange(IMECompositing, -1, -1) Then Return
        System.Threading.Interlocked.Exchange(Block, -1)
        Timer.Stop()
        ReHighlight()
        System.Threading.Interlocked.Exchange(Block, 0)
    End Sub
    Private Sub Box_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        If System.Threading.Interlocked.CompareExchange(Block, -1, -1) Then Return
        Timer.Stop()
        Timer.Interval = 500
        Timer.Start()
    End Sub

    Private Sub ComboBox_TextName_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles ComboBox_TextName.KeyUp
        If e.Handled Then Return
        If e.KeyData = Keys.Enter Then
            Button_Open_Click(sender, Nothing)
            e.Handled = True
        End If
    End Sub

    Private Sub Button_Open_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_Open.Click
        If ComboBox_TextName.Text = "" Then Return
        If Not ApplicationData.TextNameDict.ContainsKey(ComboBox_TextName.Text) Then Return
        TextName = ComboBox_TextName.Text
    End Sub

    Public Class Intercepter
        Implements IMessageFilter

        Public Handled As Boolean = True

        Private Function DirectToInt32(ByVal i As IntPtr) As Int32
            If IntPtr.Size = 4 Then Return i.ToInt32
            If IntPtr.Size = 8 Then Return CID(i.ToInt64)
            Return CID(i.ToInt64)
        End Function

        Private Const WM_MOUSEWHEEL = 522
        Private Const WM_IME_STARTCOMPOSITION = &H10D
        Private Const WM_IME_ENDCOMPOSITION = &H10E
        Private Const WM_IME_NOTIFY = &H282
        Public Function PreFilterMessage(ByRef m As System.Windows.Forms.Message) As Boolean Implements System.Windows.Forms.IMessageFilter.PreFilterMessage
            Select Case m.Msg
                Case WM_MOUSEWHEEL
                    My.Forms.FormMain.TextLocalizer_MouseWheel(Me, New MouseEventArgs(DirectToInt32(m.WParam) And &HFFFF, 0, DirectToInt32(m.LParam) And &HFFFF, (DirectToInt32(m.LParam.ToInt64) >> 16) And &HFFFF, CUS(CUShort((DirectToInt32(m.WParam) >> 16) And &HFFFF))))
                    Dim h = Handled
                    Handled = True
                    Return h
                Case WM_IME_STARTCOMPOSITION
                    System.Threading.Interlocked.Exchange(My.Forms.FormMain.IMECompositing, -1)
                Case WM_IME_ENDCOMPOSITION
                    System.Threading.Interlocked.Exchange(My.Forms.FormMain.IMECompositing, 0)
                Case WM_IME_NOTIFY

            End Select
            Return False
        End Function
    End Class

    Private Sub TextLocalizer_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel
        Dim p = DataGridView_Multiview.PointToClient(MousePosition)
        If p.X >= 0 AndAlso p.Y >= 0 AndAlso p.X < DataGridView_Multiview.Width AndAlso p.Y < DataGridView_Multiview.Height Then
            Dim i = TryCast(sender, Intercepter)
            If i IsNot Nothing Then
                If DataGridView_Multiview.Focused Then
                    i.Handled = False
                Else
                    DataGridView_Multiview.Focus()
                End If
            End If
            Return
        End If
        If ComboBox_TextName.DroppedDown Then
            Dim i = TryCast(sender, Intercepter)
            If i IsNot Nothing Then
                i.Handled = False
            End If
            Return
        End If

        If TextCount = 1 Then
            Dim i = TryCast(sender, Intercepter)
            If i IsNot Nothing Then
                i.Handled = False
            End If
            Return
        End If

        If ActiveForm IsNot Me Then Return
        If Not LocalizerEnable Then Return
        TextNumber = CInt(VScrollBar_Bar.Value - e.Delta / 120)
    End Sub
    Private Sub TextLocalizer_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        RaiseEvent Application_KeyDown(ControlId.MainWindow, e)
        If e.Handled Then Return
        Select Case e.KeyData
            Case Keys.PageUp
                If DataGridView_Multiview.Focused Then Return
                If VScrollBar_Bar.Focused Then Return
                If Not LocalizerEnable Then Return
                TextNumber = TextNumber - 1
            Case Keys.PageDown
                If DataGridView_Multiview.Focused Then Return
                If VScrollBar_Bar.Focused Then Return
                If Not LocalizerEnable Then Return
                TextNumber = TextNumber + 1
            Case Keys.F6
                Button_PreviousFile_Click(sender, e)
            Case Keys.F7
                Button_NextFile_Click(sender, e)
            Case Keys.Control Or Keys.G
                If Not NumericUpDown_TextNumber.Focused Then NumericUpDown_TextNumber.Focus()
                NumericUpDown_TextNumber.Select(0, NumericUpDown_TextNumber.Text.Length)
            Case Else
                Return
        End Select
        e.Handled = True
    End Sub
    Private Sub VScrollBar_Bar_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles VScrollBar_Bar.ValueChanged
        If Not LocalizerEnable Then Return
        TextNumber = VScrollBar_Bar.Value
    End Sub
    Private Sub NumericUpDown_TextNumber_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown_TextNumber.ValueChanged
        If VScrollBar_Bar.Value <> NumericUpDown_TextNumber.Value Then VScrollBar_Bar.Value = NumericUpDown_TextNumber.Value
    End Sub

    Private Sub Button_PreviousFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_PreviousFile.Click
        If TextName = "" OrElse Not ApplicationData.TextNameDict.ContainsKey(TextName) Then
            If ApplicationData.TextNames.Count > 0 Then
                If ApplicationData.TextNames.Count > 0 Then
                    TextName = ApplicationData.TextNames(0)
                Else
                    TextName = ""
                End If
            End If
        End If
        If TextName <> "" Then
            Dim Number = ApplicationData.TextNameDict(TextName)
            If Number < 1 Then Return
            TextName = ApplicationData.TextNames(Number - 1)
            Button_Open_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub Button_NextFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button_NextFile.Click
        If TextName = "" OrElse Not ApplicationData.TextNameDict.ContainsKey(TextName) Then
            If ApplicationData.TextNames.Count > 0 Then
                If ApplicationData.TextNames.Count > 0 Then
                    TextName = ApplicationData.TextNames(0)
                Else
                    TextName = ""
                End If
            End If
        End If
        If TextName <> "" Then
            Dim Number = ApplicationData.TextNameDict(TextName)
            If Number + 1 >= ApplicationData.TextNames.Count Then Return
            TextName = ApplicationData.TextNames(Number + 1)
            Button_Open_Click(Nothing, Nothing)
        End If
    End Sub

    Private MeWidth As Integer
    Private MeHeight As Integer
    Private MainPanelHeight As Integer
    Private LocalizationTextBoxHeights As Integer()
    Private Function GetLocalizationTextBoxHeights() As Integer()
        Dim LocalizationTextBoxHeights = New Integer(ApplicationData.Columns.Count - 1) {}
        For n = 0 To ApplicationData.Columns.Count - 1
            LocalizationTextBoxHeights(n) = LocalizationTextBoxes(n).Height
        Next
        Return LocalizationTextBoxHeights
    End Function
    Private Sub Panel_LocalizationBoxes_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Panel_LocalizationBoxes.Resize
        If Me.WindowState = FormWindowState.Minimized Then Return
        If Panel_LocalizationBoxes.Height = 0 Then Return
        If MainPanelHeight = 0 Then Return
        For n = 0 To ApplicationData.Columns.Count - 1
            LocalizationTextBoxes(n).Height *= Panel_LocalizationBoxes.Height / MainPanelHeight
        Next

        MeWidth = Me.Width
        MeHeight = Me.Height
        If Me.WindowState = FormWindowState.Normal Then
            ApplicationData.CurrentProject.WindowWidth = Me.Width
            ApplicationData.CurrentProject.WindowHeight = Me.Height
            ApplicationData.CurrentProject.Maximized = False
        End If
        MainPanelHeight = Panel_LocalizationBoxes.Height
        LocalizationTextBoxHeights = GetLocalizationTextBoxHeights()
    End Sub

    Private BlockCell As Boolean = False
    Private Sub DataGridView_Multiview_CellEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView_Multiview.CellEnter
        If BlockCell Then Return
        If Not LocalizerEnable Then Return
        BlockCell = True
        Try
            TextNumber = e.RowIndex + 1
        Finally
            BlockCell = False
        End Try
    End Sub

    Private Sub DataGridView_Multiview_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles DataGridView_Multiview.RowHeaderMouseClick
        If BlockCell Then Return
        If Not LocalizerEnable Then Return
        BlockCell = True
        Try
            TextNumber = e.RowIndex + 1
        Finally
            BlockCell = False
        End Try
    End Sub

    Private Sub DataGridView_Multiview_CellValueNeeded(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellValueEventArgs) Handles DataGridView_Multiview.CellValueNeeded
        If Not IsGridViewRowValueInitialize() Then Return
        If Not IsGridViewRowValueCached(e.RowIndex) Then
            UpdateGridRowValue(e.RowIndex)
        End If
        e.Value = GetGridViewRowValue(e.RowIndex)(e.ColumnIndex)
    End Sub

    Private Sub DataGridView_Multiview_RowPrePaint(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewRowPrePaintEventArgs) Handles DataGridView_Multiview.RowPrePaint
        Dim TextIndex = e.RowIndex
        Dim PreferredHeight = DataGridView_Multiview.Rows(TextIndex).GetPreferredHeight(TextIndex, DataGridViewAutoSizeRowMode.AllCells, True)
        If DataGridView_Multiview.Rows(TextIndex).Height <> PreferredHeight Then
            DataGridView_Multiview.Rows(TextIndex).Height = PreferredHeight
        End If
    End Sub

    Private BlockCheck As Boolean = False
    Public Sub Application_RefreshGrid() Implements ITextLocalizerApplicationController.RefreshGrid
        If Not LocalizerEnable Then Return
        If BlockCheck Then Return
        BlockCheck = True
        Application.DoEvents()

        DataGridView_Multiview.SuspendLayout()
        Dim NumRow = LocalizationTextBoxes(ApplicationData.MainColumnIndex).TextCount
        For TextIndex As Integer = 0 To NumRow - 1
            UpdateGridTextIndex(DataGridView_Multiview.Rows(TextIndex), TextIndex)
        Next
        DataGridView_Multiview.ResumeLayout()
        DataGridView_Multiview.Invalidate()

        BlockCheck = False
    End Sub
    Public Sub Application_RefreshColumn(ByVal ColumnIndex As Integer) Implements ITextLocalizerApplicationController.RefreshColumn
        LocalizationTextBoxes(ColumnIndex).UpdateDisplay()
    End Sub
    Public Sub Application_RefreshMainPanel() Implements ITextLocalizerApplicationController.RefreshMainPanel
        For Each lb In LocalizationTextBoxes
            Dim TextLength = lb.TextBox.TextLength
            Dim SelectionStart = lb.TextBox.SelectionStart
            Dim SelectionLength = lb.TextBox.SelectionLength
            lb.UpdateDisplay()
            If lb.TextBox.TextLength = TextLength Then
                If lb.TextBox.SelectionStart <> SelectionStart OrElse lb.TextBox.SelectionLength <> SelectionLength Then
                    lb.TextBox.Select(SelectionStart, SelectionLength)
                End If
            End If
        Next
    End Sub
    Public Sub Application_FlushLocalizedText() Implements ITextLocalizerApplicationController.FlushLocalizedText
        FlushLocalizedText()
    End Sub

    Public Event TextIndexChanged(ByVal e As System.EventArgs) Implements ITextLocalizerApplicationController.TextIndexChanged
    Public Event TextNameChanged(ByVal e As System.EventArgs) Implements ITextLocalizerApplicationController.TextNameChanged
    Public Event Application_ColumnSelectionChanged(ByVal e As System.EventArgs) Implements ITextLocalizerApplicationController.ColumnSelectionChanged
    Public Event Application_KeyDown(ByVal ControlId As ControlId, ByVal e As System.Windows.Forms.KeyEventArgs) Implements ITextLocalizerApplicationController.KeyDown
    Public Event Application_KeyPress(ByVal ControlId As ControlId, ByVal e As System.Windows.Forms.KeyPressEventArgs) Implements ITextLocalizerApplicationController.KeyPress
    Public Event Application_KeyUp(ByVal ControlId As ControlId, ByVal e As System.Windows.Forms.KeyEventArgs) Implements ITextLocalizerApplicationController.KeyUp

    Public ReadOnly Property Form() As System.Windows.Forms.Form Implements ITextLocalizerApplicationController.Form
        Get
            Return Me
        End Get
    End Property
    Public ReadOnly Property Application_ApplicationName() As String Implements ITextLocalizerApplicationController.ApplicationName
        Get
            Return ApplicationData.ApplicationName
        End Get
    End Property
    Public Property Application_TextName() As String Implements ITextLocalizerApplicationController.TextName
        Get
            Return TextName
        End Get
        Set(ByVal Value As String)
            TextName = Value
        End Set
    End Property
    Public Property Application_TextIndex() As Integer Implements ITextLocalizerApplicationController.TextIndex
        Get
            Return TextIndex
        End Get
        Set(ByVal Value As Integer)
            TextIndex = Value
        End Set
    End Property
    Public Property Application_TextIndices() As IEnumerable(Of Integer) Implements ITextLocalizerApplicationController.TextIndices
        Get
            If ApplicationData.CurrentProject.EnableLocalizationGrid Then
                Return From c As DataGridViewCell In DataGridView_Multiview.SelectedCells Select c.RowIndex Distinct
            Else
                Return New Integer() {TextIndex}
            End If
        End Get
        Set(ByVal Value As IEnumerable(Of Integer))
            If ApplicationData.CurrentProject.EnableLocalizationGrid Then
                For Each r As DataGridViewRow In DataGridView_Multiview.SelectedRows
                    r.Selected = False
                Next
                For Each n In Value
                    If n < 0 OrElse n >= DataGridView_Multiview.RowCount Then Continue For
                    Dim r As DataGridViewRow = DataGridView_Multiview.Rows(n)
                    r.Selected = True
                Next
            ElseIf Value.Count = 1 Then
                TextIndex = Value(0)
            End If
        End Set
    End Property
    Private CurrentColumnIndex As Integer = 0
    Private Sub Box_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs)
        For n = 0 To ApplicationData.Columns.Count - 1
            If LocalizationTextBoxes(n).Focused Then
                CurrentColumnIndex = n
                RaiseEvent Application_ColumnSelectionChanged(e)
                Return
            End If
        Next
    End Sub
    Public Property Application_ColumnIndex() As Integer Implements ITextLocalizerApplicationController.ColumnIndex
        Get
            If ApplicationData.Columns.Count = 0 Then Throw New InvalidOperationException
            For n = 0 To ApplicationData.Columns.Count - 1
                If LocalizationTextBoxes(n).Focused Then
                    Return n
                End If
            Next
            Return CurrentColumnIndex
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 OrElse Value >= ApplicationData.Columns.Count Then Return
            LocalizationTextBoxes(Value).Focus()
        End Set
    End Property
    Public Property Application_SelectionStart() As Integer Implements ITextLocalizerApplicationController.SelectionStart
        Get
            If ApplicationData.Columns.Count = 0 Then Throw New InvalidOperationException
            Dim l = LocalizationTextBoxes(CurrentColumnIndex)
            If l.TextBox.Visible Then Return l.TextBox.SelectionStart
            Return 0
        End Get
        Set(ByVal Value As Integer)
            If ApplicationData.Columns.Count = 0 Then Throw New InvalidOperationException
            Dim l = LocalizationTextBoxes(CurrentColumnIndex)
            l.TextBox.Select(Value, l.TextBox.SelectionLength)
        End Set
    End Property
    Public Property Application_SelectionLength() As Integer Implements ITextLocalizerApplicationController.SelectionLength
        Get
            If ApplicationData.Columns.Count = 0 Then Throw New InvalidOperationException
            Dim l = LocalizationTextBoxes(CurrentColumnIndex)
            If l.TextBox.Visible Then Return l.TextBox.SelectionLength
            Return 0
        End Get
        Set(ByVal Value As Integer)
            If ApplicationData.Columns.Count = 0 Then Throw New InvalidOperationException
            Dim l = LocalizationTextBoxes(CurrentColumnIndex)
            l.TextBox.Select(l.TextBox.SelectionStart, Value)
        End Set
    End Property
    Public Property Application_Text(ByVal ColumnIndex As Integer) As String Implements ITextLocalizerApplicationController.Text
        Get
            If ApplicationData.Columns.Count = 0 Then Throw New InvalidOperationException
            Dim l = LocalizationTextBoxes(CurrentColumnIndex)
            Return l.Text
        End Get
        Set(ByVal Value As String)
            If ApplicationData.Columns.Count = 0 Then Throw New InvalidOperationException
            Dim l = LocalizationTextBoxes(CurrentColumnIndex)
            If l.IsReadOnly Then
                MessageBox.Show("无法修改只读文本。", Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                l.Text = Value
            End If
        End Set
    End Property
    Public Sub ScrollToCaret(ByVal ColumnIndex As Integer) Implements ITextLocalizerApplicationController.ScrollToCaret
        Dim lb = LocalizationTextBoxes(ColumnIndex)
        lb.TextBox.ScrollToCaret()
        If Not lb.TextBox.Visible Then lb.SwitchBox()
    End Sub
End Class
