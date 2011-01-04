'==========================================================================
'
'  File:        LocalizationTextBox.xaml.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 本地化文本框
'  Version:     2011.01.04.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.IO
Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Glyphing
Imports Firefly.Texting
Imports Eddy.Interfaces
Imports Eddy.Base

Public Class LocalizationTextBox
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
    Public Shadows Property FontName() As String
        Get
            Return TextBox.FontFamily.Source
        End Get
        Set(ByVal Value As String)
            TextBox.FontFamily = New FontFamily(WpfFontNameBugPreventer(Value))
        End Set
    End Property
    Private Function WpfFontNameBugPreventer(ByVal Name As String) As String
        '注意WPF不接受中文的字体名称，因此有此hack
        Dim f As New System.Drawing.FontFamily(Name)
        Dim familyName As String = f.GetName(1033) 'en-US
        Return familyName
    End Function

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)> _
    Public Shadows Property FontPixel() As Integer
        Get
            If TextBox Is Nothing Then Return MyBase.FontSize
            Return TextBox.FontSize
        End Get
        Set(ByVal Value As Integer)
            If Value <= 0 Then Throw New ArgumentException
            TextBox.FontSize = Value
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
    Public Shadows ReadOnly Property IsLoaded() As Boolean
        Get
            Return TextNameValue <> ""
        End Get
    End Property

    Private Shadows Initialized As Boolean = False
    ''' <summary>初始化</summary>
    Public Sub Init(ByVal TextProvider As LocalizationTextProvider)
        If Initialized Then Throw New InvalidOperationException
        TextProviderValue = TextProvider
        IsGlyphTextValue = TextProvider.Type.Equals("LOC", StringComparison.OrdinalIgnoreCase)
        TextBox.IsReadOnly = IsReadOnly
        If IsGlyphTextValue Then
            PictureBox_Visible = True
            TextBox_Visible = False
        Else
            PictureBox_Visible = False
            TextBox_Visible = True
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
            TextBox.IsReadOnly = True
            Return
        Else
            TextBox.IsReadOnly = TextProviderValue.IsReadOnly
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
            TextBox.IsReadOnly = TextProviderValue.IsReadOnly
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
        TextBox_Text = Nothing
        If IsGlyphTextValue Then
            PictureBox_Image = Nothing
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
    Public Property Text() As String
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
            TextBox_Text = TextList.Text(TextIndexValue)
            If IsGlyphTextValue Then
                PictureBox_Image = TextListLOC.GetBitmap(FontPixel, FontPixel, TextIndexValue, SpaceValue)
            End If
        Else
            TextBox_Text = Nothing
            If IsGlyphTextValue Then
                PictureBox_Image = Nothing
            End If
        End If
    End Sub
    Public Sub UpdateSource()
        If TextIndexValue < 0 Then Return
        If IsReadOnly Then Return
        If TextList.Text(TextIndexValue) <> TextBox_Text Then TextList.Text(TextIndexValue) = TextBox_Text
    End Sub

    Public Sub SwitchBox()
        If IsGlyphTextValue Then
            PictureBox_Visible = Not PictureBox_Visible
            TextBox_Visible = Not TextBox_Visible
        End If
    End Sub

    Private Sub LocalizationTextBox_MouseUp(ByVal sender As Object, ByVal e As Input.MouseButtonEventArgs) Handles Me.MouseUp, TextBox.MouseUp, PictureBox.MouseUp
        Select Case e.ChangedButton
            Case Input.MouseButton.Left
                If PictureBox_Visible Then PictureBox.Focus()
                If TextBox_Visible Then TextBox.Focus()
            Case Input.MouseButton.Middle
                SwitchBox()
        End Select
    End Sub

    Public ReadOnly Property Focused() As Boolean
        Get
            Return PictureBox.IsFocused OrElse TextBox.IsFocused
        End Get
    End Property

    Private Property PictureBox_Visible As Boolean
        Get
            Return PictureBox.Visibility = Windows.Visibility.Visible
        End Get
        Set(ByVal Value As Boolean)
            If Value Then
                PictureBox.Visibility = Windows.Visibility.Visible
            Else
                PictureBox.Visibility = Windows.Visibility.Hidden
            End If
        End Set
    End Property
    Private Property TextBox_Visible As Boolean
        Get
            Return TextBox.Visibility = Windows.Visibility.Visible
        End Get
        Set(ByVal Value As Boolean)
            If Value Then
                TextBox.Visibility = Windows.Visibility.Visible
            Else
                TextBox.Visibility = Windows.Visibility.Hidden
            End If
        End Set
    End Property
    Private Property TextBox_Text As String
        Get
            Return TextBox.Text
            'Dim Lines = TextBox.Document.Blocks.OfType(Of Documents.Paragraph).Select(Function(p) String.Join("", p.Inlines.OfType(Of Documents.Run).Select(Function(r) r.Text))).ToArray()
            'Return String.Join(CrLf, Lines)
        End Get
        Set(ByVal Value As String)
            TextBox.Text = Value
            'Dim Lines = Value.UnifyNewLineToLf.Split(Lf)
            'TextBox.Document.Blocks.Clear()
            'TextBox.Document.Blocks.AddRange(Lines.Select(Function(l) New Documents.Paragraph(New Documents.Run(l))))
        End Set
    End Property
    Private WriteOnly Property PictureBox_Image As System.Drawing.Bitmap
        Set(ByVal Value As System.Drawing.Bitmap)
            PictureBox.Source = Interop.Imaging.CreateBitmapSourceFromHBitmap(Value.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
        End Set
    End Property
End Class
