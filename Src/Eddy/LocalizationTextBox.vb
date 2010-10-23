'==========================================================================
'
'  File:        LocalizationTextBox.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 本地化文本框
'  Version:     2010.10.24.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.ComponentModel
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Glyphing
Imports Firefly.Texting
Imports Eddy.Interfaces

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class LocalizationTextBox
    Inherits Windows.Forms.UserControl

    'UserControl 重写 Dispose，以清理组件列表。
    <Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.PictureBox = New Firefly.GUI.ScrollablePictureBox
        Me.TextBox = New ExtendedRichTextBox
        Me.SuspendLayout()
        '
        'PictureBox
        '
        Me.PictureBox.AutoScroll = True
        Me.PictureBox.BackColor = Drawing.Color.White
        Me.PictureBox.Dock = Windows.Forms.DockStyle.Fill
        Me.PictureBox.Image = Nothing
        Me.PictureBox.Location = New Drawing.Point(0, 0)
        Me.PictureBox.Name = "PictureBox"
        Me.PictureBox.Size = New Drawing.Size(500, 150)
        Me.PictureBox.TabIndex = 2
        Me.PictureBox.Visible = False
        '
        'TextBox
        '
        Me.TextBox.BackColor = Drawing.Color.White
        Me.TextBox.Dock = Windows.Forms.DockStyle.Fill
        Me.TextBox.HideSelection = False
        Me.TextBox.Location = New Drawing.Point(0, 0)
        Me.TextBox.Multiline = True
        Me.TextBox.Name = "TextBox"
        Me.TextBox.ReadOnly = True
        Me.TextBox.ScrollBars = Windows.Forms.ScrollBars.Vertical
        Me.TextBox.Size = New Drawing.Size(500, 150)
        Me.TextBox.BorderStyle = Windows.Forms.BorderStyle.None
        Me.TextBox.EnableAutoDragDrop = True
        Me.TextBox.AcceptsTab = True
        Me.TextBox.AutoWordSelection = False
        Me.TextBox.TabIndex = 1
        '
        'LocalizationTextBox
        '
        Me.AutoScaleDimensions = New Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.TextBox)
        Me.Controls.Add(Me.PictureBox)
        Me.Name = "LocalizationTextBox"
        Me.Size = New Drawing.Size(500, 150)
        Me.BorderStyle = Windows.Forms.BorderStyle.None
        Me.SetStyle(Windows.Forms.ControlStyles.Selectable, True)
        Me.SetStyle(Windows.Forms.ControlStyles.EnableNotifyMessage, True)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub
    Friend WithEvents PictureBox As Firefly.GUI.ScrollablePictureBox
    Friend WithEvents TextBox As ExtendedRichTextBox

    Private TextProviderValue As LocalizationTextProvider
    Private IsGlyphTextValue As Boolean = False

    Private TextNameValue As String
    Private TextIndexValue As Integer
    Private SpaceValue As Integer

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public Property TextProvider() As LocalizationTextProvider
        Get
            Return TextProviderValue
        End Get
        Private Set(ByVal Value As LocalizationTextProvider)
            TextProviderValue = Value
            IsGlyphTextValue = TextProvider.Type.Equals("LOC")
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public ReadOnly Property IsGlyphText() As Boolean
        Get
            Return IsGlyphTextValue
        End Get
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public ReadOnly Property IsReadOnly() As Boolean
        Get
            Return TextProviderValue.IsReadOnly
        End Get
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public ReadOnly Property TextName() As String
        Get
            Return TextNameValue
        End Get
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public Shadows Property Font() As Drawing.Font
        Get
            If TextBox Is Nothing Then Return MyBase.Font
            Return TextBox.Font
        End Get
        Set(ByVal Value As Drawing.Font)
            MyBase.Font = Value
            TextBox.Font = Value
        End Set
    End Property

    Private FontPixelValue As Integer = 16
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public Shadows Property FontPixel() As Integer
        Get
            Return FontPixelValue
        End Get
        Set(ByVal Value As Integer)
            If Value <= 0 Then Throw New ArgumentException
            FontPixelValue = Value
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public ReadOnly Property TextModified() As Boolean
        Get
            If Not IsLoaded Then Return False
            UpdateSource()
            Return TextProviderValue.IsModified(TextNameValue)
        End Get
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public Property Space() As Integer
        Get
            Return SpaceValue
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Throw New ArgumentOutOfRangeException
            SpaceValue = Value
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public ReadOnly Property IsLoaded() As Boolean
        Get
            Return TextNameValue <> ""
        End Get
    End Property

    Private Initialized As Boolean = False
    ''' <summary>初始化</summary>
    Public Sub Init(ByVal TextProvider As LocalizationTextProvider)
        If Initialized Then Throw New InvalidOperationException
        TextProviderValue = TextProvider
        IsGlyphTextValue = TextProvider.Type.Equals("LOC", StringComparison.OrdinalIgnoreCase)
        TextBox.ReadOnly = IsReadOnly
        If IsGlyphTextValue Then
            PictureBox.Visible = True
            TextBox.Visible = False
        Else
            PictureBox.Visible = False
            TextBox.Visible = True
        End If
        Initialized = True
    End Sub

    Private TextList As ILocalizationTextList
    Private TextListLOC As LOCList

    ''' <summary>装载文本</summary>
    Public Sub LoadText(ByVal TextName As String)
        If Not Initialized Then Throw New InvalidOperationException
        UnloadText()
        If Not TextProviderValue.ContainsKey(TextName) Then
            TextBox.ReadOnly = True
            Return
        Else
            TextBox.ReadOnly = TextProviderValue.IsReadOnly
        End If
        TextNameValue = TextName
        TextList = TextProviderValue.Item(TextNameValue)
        TextListLOC = Nothing
        If IsGlyphTextValue Then
            TextListLOC = CType(TextList, LOCList)
        End If
        TextIndexValue = -1
    End Sub

    ''' <summary>装载或创建文本</summary>
    Public Sub LoadOrCreateText(ByVal TextName As String, ByVal Template As ILocalizationTextList, ByVal TranslateText As Func(Of String, String))
        If Not Initialized Then Throw New InvalidOperationException
        UnloadText()
        If Not TextProviderValue.ContainsKey(TextName) Then
            TextProviderValue.LoadOrCreateItem(TextName, Template, TranslateText)
            TextBox.ReadOnly = TextProviderValue.IsReadOnly
        End If
        LoadText(TextName)
    End Sub

    ''' <summary>卸载文本</summary>
    Public Sub UnloadText()
        If Not Initialized Then Throw New InvalidOperationException
        If Not IsLoaded Then Return

        TextList.Flush()
        TextList = Nothing
        TextListLOC = Nothing
        TextProvider.ForceUnloadText(TextName)
        TextBox.Text = Nothing
        If IsGlyphTextValue Then
            PictureBox.Image = Nothing
        End If
        TextIndexValue = -1
        TextNameValue = ""
    End Sub

    ''' <summary>保存文本</summary>
    Public Sub SaveText()
        If Not Initialized Then Throw New InvalidOperationException
        If IsReadOnly Then Throw New InvalidOperationException
        UpdateSource()
        TextList.Flush()
    End Sub

    ''' <summary>获取或设置文本索引，可改变当前显示的文本</summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public Property TextIndex() As Integer
        Get
            Return TextIndexValue
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 OrElse ((Not IsReadOnly) AndAlso Value >= TextCount) Then Throw New ArgumentOutOfRangeException
            If Not IsReadOnly Then UpdateSource()
            TextIndexValue = Value
            UpdateDisplay()
        End Set
    End Property

    ''' <summary>获取或设置文本编号，可改变当前显示的文本</summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public Property TextNumber() As Integer
        Get
            Return TextIndex + 1
        End Get
        Set(ByVal Value As Integer)
            TextIndex = Value - 1
        End Set
    End Property

    ''' <summary>总文本数</summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public ReadOnly Property TextCount() As Integer
        Get
            If TextList Is Nothing Then Return 0
            Return TextList.Count
        End Get
    End Property

    ''' <summary>获取或设置当前文本</summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public Overrides Property Text() As String
        Get
            If Not IsReadOnly Then UpdateSource()
            If IsReadOnly AndAlso TextIndexValue >= TextCount Then Return Nothing
            Return TextList.Text(TextIndexValue)
        End Get
        Set(ByVal Value As String)
            If IsReadOnly Then Throw New InvalidOperationException
            TextList.Text(TextIndexValue) = Value
            UpdateDisplay()
        End Set
    End Property

    Public Sub UpdateDisplay()
        If TextIndexValue < TextCount Then
            TextBox.Text = TextList.Text(TextIndexValue)
            If IsGlyphTextValue Then
                PictureBox.Image = TextListLOC.GetBitmap(FontPixel, FontPixel, TextIndexValue, SpaceValue)
            End If
        Else
            TextBox.Text = Nothing
            If IsGlyphTextValue Then
                PictureBox.Image = Nothing
            End If
        End If
    End Sub
    Public Sub UpdateSource()
        If TextIndexValue < 0 Then Return
        If IsReadOnly Then Return
        If TextList.Text(TextIndexValue) <> TextBox.Text Then TextList.Text(TextIndexValue) = TextBox.Text
    End Sub

    Public Sub SwitchBox()
        If IsGlyphTextValue Then
            PictureBox.Visible = Not PictureBox.Visible
            TextBox.Visible = Not TextBox.Visible
        End If
    End Sub

    Private Sub LocalizationTextBox_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        AddHandler PictureBox.PictureBox.MouseUp, AddressOf LocalizationTextBox_MouseUp
    End Sub

    Private Sub LocalizationTextBox_MouseUp(ByVal sender As Object, ByVal e As Windows.Forms.MouseEventArgs) Handles Me.MouseUp, TextBox.MouseUp, PictureBox.MouseUp
        Select Case e.Button
            Case Windows.Forms.MouseButtons.Left
                If PictureBox.Visible Then PictureBox.Focus()
                If TextBox.Visible Then TextBox.Focus()
            Case Windows.Forms.MouseButtons.Middle
                SwitchBox()
        End Select
    End Sub

    Private FocusedValue As Boolean = False
    Public Overrides ReadOnly Property Focused() As Boolean
        Get
            Return FocusedValue
        End Get
    End Property

    Public Shadows Event GotFocus As EventHandler
    Public Shadows Event LostFocus As EventHandler
    Protected Overrides Sub OnEnter(ByVal e As EventArgs)
        FocusedValue = True
        RaiseEvent GotFocus(Me, e)
        MyBase.OnEnter(e)
    End Sub
    Protected Overrides Sub OnLeave(ByVal e As EventArgs)
        MyBase.OnLeave(e)
        RaiseEvent LostFocus(Me, e)
        FocusedValue = False
    End Sub
End Class
