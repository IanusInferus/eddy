'==========================================================================

'  File:        ExtendedRichTextBox.vb
'  Description: 扩展富文本框
'               此代码是由以下两篇文章中的代码混合而成
'               David Bennett http://blogs.technet.com/david_bennett/archive/2005/04/06/403402.aspx
'               John Fisher http://www.codeproject.com/KB/edit/richtextboxplus.aspx?df=100&forumid=16179&exp=0&select=562462
'  Version:     2010.07.25.

'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.TextEncoding

''' <summary> 
''' Specifies the style of underline that should be 
''' applied to the text. 
''' </summary> 
Public Enum UnderlineStyle
    ''' <summary> 
    ''' No underlining. 
    ''' </summary> 
    None = 0

    ''' <summary> 
    ''' Standard underlining across all words. 
    ''' </summary> 
    Normal = 1

    ''' <summary> 
    ''' Standard underlining broken between words. 
    ''' </summary> 
    Word = 2

    ''' <summary> 
    ''' Double line underlining. 
    ''' </summary> 
    [Double] = 3

    ''' <summary> 
    ''' Dotted underlining. 
    ''' </summary> 
    Dotted = 4

    ''' <summary> 
    ''' Dashed underlining. 
    ''' </summary> 
    Dash = 5

    ''' <summary> 
    ''' Dash-dot ("-.-.") underlining. 
    ''' </summary> 
    DashDot = 6

    ''' <summary> 
    ''' Dash-dot-dot ("-..-..") underlining. 
    ''' </summary> 
    DashDotDot = 7

    ''' <summary> 
    ''' Wave underlining (like spelling mistakes in MS Word). 
    ''' </summary> 
    Wave = 8

    ''' <summary> 
    ''' Extra thick standard underlining. 
    ''' </summary> 
    Thick = 9

    ''' <summary> 
    ''' Extra thin standard underlining. 
    ''' </summary> 
    HairLine = 10

    ''' <summary> 
    ''' Double thickness wave underlining. 
    ''' </summary> 
    DoubleWave = 11

    ''' <summary> 
    ''' Thick wave underlining. 
    ''' </summary> 
    HeavyWave = 12

    ''' <summary> 
    ''' Extra long dash underlining. 
    ''' </summary> 
    LongDash = 13
End Enum

''' <summary> 
''' Specifies the color of underline that should be 
''' applied to the text. 
''' </summary> 
''' <remarks> 
''' I named these colors by their appearance, so some 
''' of them might not be what you expect. Please email 
''' me if you feel one should be changed. 
''' </remarks> 
Public Enum UnderlineColor
    ''' <summary>Black.</summary> 
    Black = &H0

    ''' <summary>None.</summary> 
    None = &H0

    ''' <summary>Blue.</summary> 
    Blue = &H10

    ''' <summary>Cyan.</summary> 
    Cyan = &H20

    ''' <summary>Lime green.</summary> 
    LimeGreen = &H30

    ''' <summary>Magenta.</summary> 
    Magenta = &H40

    ''' <summary>Red.</summary> 
    Red = &H50

    ''' <summary>Yellow.</summary> 
    Yellow = &H60

    ''' <summary>White.</summary> 
    White = &H70

    ''' <summary>DarkBlue.</summary> 
    DarkBlue = &H80

    ''' <summary>DarkCyan.</summary> 
    DarkCyan = &H90

    ''' <summary>Green.</summary> 
    Green = &HA0

    ''' <summary>Dark magenta.</summary> 
    DarkMagenta = &HB0

    ''' <summary>Brown.</summary> 
    Brown = &HC0

    ''' <summary>Olive green.</summary> 
    OliveGreen = &HD0

    ''' <summary>Dark gray.</summary> 
    DarkGray = &HE0

    ''' <summary>Gray.</summary> 
    Gray = &HF0
End Enum

Public Enum TextMode
    TM_PLAINTEXT = 1
    TM_RICHTEXT = 2        ' Default behavior 
    TM_SINGLELEVELUNDO = 4
    TM_MULTILEVELUNDO = 8        ' Default behavior 
    TM_SINGLECODEPAGE = 16
    TM_MULTICODEPAGE = 32        ' Default behavior 
End Enum

Public Class ExtendedRichTextBox
    Inherits RichTextBox

    Public Shadows Sub Find()
        Throw New NotSupportedException
    End Sub

    Private TextValue As String
    Public Overrides Property [Text]() As String
        Get
            Return TextValue
        End Get
        Set(ByVal Value As String)
            MyBase.Text = ""
            MyBase.Text = Value
        End Set
    End Property

    Public Overrides ReadOnly Property TextLength() As Integer
        Get
            If TextValue Is Nothing Then Return 0
            Return TextValue.Length
        End Get
    End Property

    Private Structure AbsoluteIndex
        Public Start As Integer
        Public Length As Integer
        Public ForeColor As Color
        Public BackColor As Color
    End Structure

    Private Structure LinedIndex
        Public StartLine As Integer
        Public StartChar As Integer
        Public EndLine As Integer
        Public EndChar As Integer
        Public ForeColor As Color
        Public BackColor As Color
    End Structure

    Private Function IndexAbsToLined(ByVal ai As AbsoluteIndex) As LinedIndex
        Dim Chars = Text.ToCharArray
        Dim StartLine As Integer = 0
        Dim StartLineStart As Integer = 0
        For n = 0 To ai.Start - 1
            Dim c = Chars(n)
            If AscW(c) = Lf Then
                StartLine += 1
                StartLineStart = n + 1
            End If
        Next
        Dim EndLine As Integer = 0
        Dim EndLineStart As Integer = 0
        For n = 0 To ai.Start + ai.Length - 1
            Dim c = Chars(n)
            If AscW(c) = Lf Then
                EndLine += 1
                EndLineStart = n + 1
            End If
        Next
        Return New LinedIndex With {.StartLine = StartLine, .StartChar = ai.Start - StartLineStart, .EndLine = EndLine, .EndChar = ai.Start + ai.Length - EndLineStart, .ForeColor = ai.ForeColor, .BackColor = ai.BackColor}
    End Function
    Private Function IndexLinedToAbs(ByVal li As LinedIndex) As AbsoluteIndex
        Dim Chars = Text.ToCharArray
        Dim StartLine As Integer = 0
        Dim StartLineStart As Integer = 0
        For n = 0 To Chars.Length - 1
            Dim c = Chars(n)
            If AscW(c) = Lf Then
                StartLine += 1
                StartLineStart = n + 1
                If StartLine = li.StartLine Then
                    Exit For
                End If
            End If
        Next
        Dim EndLine As Integer = 0
        Dim EndLineStart As Integer = 0
        For n = 0 To Chars.Length - 1
            Dim c = Chars(n)
            If AscW(c) = Lf Then
                EndLine += 1
                EndLineStart = n + 1
                If EndLine = li.EndLine Then
                    Exit For
                End If
            End If
        Next
        Dim Start = StartLineStart + li.StartChar
        Dim Length = EndLineStart + li.EndChar - Start
        Return New AbsoluteIndex With {.Start = Start, .Length = Length, .ForeColor = li.ForeColor, .BackColor = li.BackColor}
    End Function

    Protected Overrides Sub OnTextChanged(ByVal e As System.EventArgs)
        TextValue = MyBase.Text.Replace(CrLf, Lf).Replace(Cr, Lf).Replace(Lf, CrLf)
        MyBase.OnTextChanged(e)
    End Sub

    Public Shadows Sub [Select](ByVal Start As Integer, ByVal Length As Integer)
        If Text Is Nothing Then
            MyBase.Select(Start, Length)
        Else
            Dim FakeStart = Start - (From c In Text.Substring(0, Start) Where AscW(c) = Cr).Count
            Dim FakeLength = Length - (From c In Text.Substring(Start, Min(Length, Text.Length - Start)) Where AscW(c) = Cr).Count
            MyBase.Select(FakeStart, FakeLength)
        End If
    End Sub

    Private Const EM_SETSEL As Integer = &HB1
    Public Sub SetTextColor(ByVal Start As Integer, ByVal Length As Integer, ByVal ForeColor As Color, ByVal BackColor As Color)
        If Text Is Nothing Then
            SendMessage(New HandleRef(Me, Handle), EM_SETSEL, Start, Start + Length)
        Else
            Dim FakeStart = Start - (From c In Text.Substring(0, Start) Where AscW(c) = Cr).Count
            Dim FakeLength = Length - (From c In Text.Substring(Start, Min(Length, Text.Length - Start)) Where AscW(c) = Cr).Count
            SendMessage(New HandleRef(Me, Handle), EM_SETSEL, FakeStart, FakeStart + FakeLength)
        End If
        SelectionColor = ForeColor
        SelectionBackColor = BackColor
    End Sub

    Public Shadows ReadOnly Property SelectionStart() As Integer
        Get
            Dim FakeStart = MyBase.SelectionStart
            Dim RealStart = 0
            While FakeStart > 0
                If RealStart >= TextValue.Length Then Return TextValue.Length
                If AscW(TextValue(RealStart)) <> Cr Then
                    FakeStart -= 1
                End If
                RealStart += 1
            End While
            Return RealStart
        End Get
    End Property

    Public Shadows ReadOnly Property SelectionLength() As Integer
        Get
            Dim Start = SelectionStart
            Dim FakeLength = MyBase.SelectionLength
            Dim RealLength = 0
            While FakeLength > 0
                If Start + RealLength >= TextValue.Length Then Return Max(0, TextValue.Length - Start)
                If AscW(TextValue(Start + RealLength)) <> Cr Then
                    FakeLength -= 1
                End If
                RealLength += 1
            End While
            Return RealLength
        End Get
    End Property

    Public Overrides Property Font() As System.Drawing.Font
        Get
            Return MyBase.Font
        End Get
        Set(ByVal value As System.Drawing.Font)
            MyBase.Font = value
        End Set
    End Property

    Public Shadows Property SelectionFont() As System.Drawing.Font
        Get
            Return MyBase.SelectionFont
        End Get
        Set(ByVal value As System.Drawing.Font)
            MyBase.SelectionFont = value
        End Set
    End Property

    Private _Updating As Integer = 0
    Private _OldEventMask As IntPtr = IntPtr.Zero

    Public Sub New()
        Dim tb = Me
        tb.LanguageOption = tb.LanguageOption And Not RichTextBoxLanguageOptions.AutoFont
        tb.LanguageOption = tb.LanguageOption And Not RichTextBoxLanguageOptions.AutoFontSizeAdjust
        tb.LanguageOption = tb.LanguageOption And Not RichTextBoxLanguageOptions.AutoKeyboard
        tb.LanguageOption = tb.LanguageOption And Not RichTextBoxLanguageOptions.DualFont
        tb.LanguageOption = tb.LanguageOption And Not RichTextBoxLanguageOptions.ImeAlwaysSendNotify
        tb.LanguageOption = tb.LanguageOption Or RichTextBoxLanguageOptions.ImeCancelComplete
        tb.LanguageOption = tb.LanguageOption And Not RichTextBoxLanguageOptions.UIFonts

        ' Get the horizontal and vertical resolutions at which the object is 
        ' being displayed 
        Using _graphics = Me.CreateGraphics()
            xDpi = _graphics.DpiX
            yDpi = _graphics.DpiY
        End Using

        HideCaret(New HandleRef(Me, Handle))
        SendMessage(New HandleRef(Me, Handle), EM_SETTEXTMODE, Convert.ToInt32(TextMode.TM_RICHTEXT Or TextMode.TM_MULTILEVELUNDO Or TextMode.TM_MULTICODEPAGE), 0)
        SendMessage(New HandleRef(Me, Handle), EM_SETUNDOLIMIT, 1024, 0)
    End Sub

    Private Const WM_VSCROLL As Integer = &H115
    Private Const WM_HSCROLL As Integer = &H114
    Private Const SB_LINEUP As Integer = 0
    Private Const SB_LINEDOWN As Integer = 1
    Private Const SB_PAGEUP As Integer = 2
    Private Const SB_PAGEDOWN As Integer = 3
    Private Const SB_THUMBPOSITION As Integer = 4
    Private Const SB_THUMBTRACK As Integer = 5
    Private Const SB_TOP As Integer = 6
    Private Const SB_BOTTOM As Integer = 7
    Private Const SB_ENDSCROLL As Integer = 8

    Private Const WM_SETREDRAW As Integer = &HB
    Private Const EM_SETEVENTMASK As Integer = &H431
    Private Const EM_SETCHARFORMAT As Integer = &H444
    Private Const EM_GETCHARFORMAT As Integer = &H43A
    Private Const EM_GETPARAFORMAT As Integer = &H43D
    Private Const EM_SETPARAFORMAT As Integer = &H447
    Private Const EM_SETUNDOLIMIT As Integer = &H452
    Private Const EM_SETTEXTMODE As Integer = &H459
    Private Const EM_GETTEXTMODE As Integer = &H460
    Private Const EM_SETTYPOGRAPHYOPTIONS As Integer = &H4CA
    Private Const CFM_UNDERLINETYPE As Integer = &H800000
    Private Const CFM_BACKCOLOR As Integer = &H4000000
    Private Const CFE_AUTOBACKCOLOR As Integer = &H4000000
    Private Const SCF_SELECTION As Integer = &H1
    Private Const PFM_ALIGNMENT As Integer = &H8
    Private Const TO_ADVANCEDTYPOGRAPHY As Integer = &H1

    ' These are the scroll bar constants. 
    Private Const SBS_HORIZ As Integer = 0
    Private Const SBS_VERT As Integer = 1
    ' Get which bits. 
    Private Const SIF_RANGE As Integer = &H1
    Private Const SIF_PAGE As Integer = &H2
    Private Const SIF_POS As Integer = &H4
    Private Const SIF_DISABLENOSCROLL As Integer = &H8
    Private Const SIF_TRACKPOS As Integer = &H10
    Private Const SIF_ALL As Integer = (SIF_RANGE Or SIF_PAGE Or SIF_POS Or SIF_TRACKPOS)

    <DllImport("user32", CharSet:=CharSet.Auto)> _
    Private Shared Function SendMessage(ByVal hWnd As HandleRef, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function
    Private Shared Function SendMessage(ByVal hWnd As HandleRef, ByVal msg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As IntPtr
        Return SendMessage(hWnd, msg, New IntPtr(wParam), New IntPtr(lParam))
    End Function

    <DllImport("user32", CharSet:=CharSet.Auto)> _
    Private Shared Function SendMessage(ByVal hWnd As HandleRef, ByVal msg As Integer, ByVal wParam As IntPtr, ByRef lParam As CHARFORMAT2) As IntPtr
    End Function
    Private Shared Function SendMessage(ByVal hWnd As HandleRef, ByVal msg As Integer, ByVal wParam As Integer, ByRef lParam As CHARFORMAT2) As IntPtr
        Return SendMessage(hWnd, msg, New IntPtr(wParam), lParam)
    End Function

    <DllImport("user32", CharSet:=CharSet.Auto)> _
    Private Shared Function SendMessage(ByVal hWnd As HandleRef, ByVal msg As Integer, ByVal wParam As IntPtr, ByRef lParam As PARAFORMAT2) As IntPtr
    End Function
    Private Shared Function SendMessage(ByVal hWnd As HandleRef, ByVal msg As Integer, ByVal wParam As Integer, ByRef lParam As PARAFORMAT2) As IntPtr
        Return SendMessage(hWnd, msg, New IntPtr(wParam), lParam)
    End Function

    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)> _
    Private Shared Function SetWindowTheme(ByVal hWnd As HandleRef, <MarshalAs(UnmanagedType.LPWStr)> ByVal pszSubAppName As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal pszSubIdList As String) As Integer
    End Function

    ''' <summary> 
    ''' The HideCaret function removes the caret from the screen. 
    ''' </summary> 
    <DllImport("user32.dll")> _
    Protected Shared Function HideCaret(ByVal hWnd As HandleRef) As Boolean
    End Function

    ''' <summary> 
    ''' This will find the scroll position of the specified window. 
    ''' </summary> 
    ''' <param name="hWnd">the window to send the message to</param> 
    ''' <param name="nBar">the number of the sroll bar to look at</param> 
    ''' <returns></returns> 
    <DllImport("user32", CharSet:=CharSet.Auto)> _
    Private Shared Function GetScrollInfo(ByVal hWnd As HandleRef, ByVal nBar As Integer, ByRef info As SCROLLINFO) As Integer
    End Function

    ''' <summary> 
    ''' Contains information about character formatting in a rich edit control. 
    ''' </summary> 
    ''' <remarks><see cref="CHARFORMAT2"/> requires Rich Edit 2.0.</remarks> 
    <StructLayout(LayoutKind.Sequential)> _
    Private Structure CHARFORMAT2
        Public cbSize As Integer
        Public dwMask As UInteger
        Public dwEffects As UInteger
        Public yHeight As Integer
        Public yOffset As Integer
        Public crTextColor As Integer
        Public bCharSet As Byte
        Public bPitchAndFamily As Byte
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)> _
        Public szFaceName As Char()
        Public wWeight As Short
        Public sSpacing As Short
        Public crBackColor As Integer
        Public LCID As Integer
        Public dwReserved As UInteger
        Public sStyle As Short
        Public wKerning As Short
        Public bUnderlineType As Byte
        Public bAnimation As Byte
        Public bRevAuthor As Byte
    End Structure

    ''' <summary> 
    ''' Contains information about paragraph formatting in a rich edit control. 
    ''' </summary> 
    ''' <remarks><see cref="PARAFORMAT2"/> requires Rich Edit 2.0.</remarks> 
    <StructLayout(LayoutKind.Sequential)> _
    Private Structure PARAFORMAT2
        Public cbSize As Integer
        Public dwMask As UInteger
        Public wNumbering As Short
        Public wReserved As Short
        Public dxStartIndent As Integer
        Public dxRightIndent As Integer
        Public dxOffset As Integer
        Public wAlignment As Short
        Public cTabCount As Short
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)> _
        Public rgxTabs As Integer()
        Public dySpaceBefore As Integer
        Public dySpaceAfter As Integer
        Public dyLineSpacing As Integer
        Public sStyle As Short
        Public bLineSpacingRule As Byte
        Public bOutlineLevel As Byte
        Public wShadingWeight As Short
        Public wShadingStyle As Short
        Public wNumberingStart As Short
        Public wNumberingStyle As Short
        Public wNumberingTab As Short
        Public wBorderSpace As Short
        Public wBorderWidth As Short
        Public wBorders As Short
    End Structure

    ''' <summary> 
    ''' Contains information the scroll bar positions. 
    ''' </summary> 
    <StructLayout(LayoutKind.Sequential)> _
    Private Structure SCROLLINFO
        Public cbSize As Integer
        Public fMask As Integer
        Public nMin As Integer
        Public nMax As Integer
        Public nPage As Integer
        Public nPos As Integer
        Public nTrackPos As Integer
    End Structure

    ''' <summary> 
    ''' Gets or sets the underline style to apply to the current selection or insertion point. 
    ''' </summary> 
    ''' <value>A <see cref="UnderlineStyle"/> that represents the underline style to 
    ''' apply to the current text selection or to text entered after the insertion point.</value> 
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property SelectionUnderlineStyle() As UnderlineStyle
        Get
            Dim fmt As New CHARFORMAT2()
            fmt.cbSize = Marshal.SizeOf(fmt)

            ' Get the underline style 
            SendMessage(New HandleRef(Me, Handle), EM_GETCHARFORMAT, SCF_SELECTION, fmt)
            If (fmt.dwMask And CFM_UNDERLINETYPE) = 0 Then
                Return UnderlineStyle.None
            Else
                Dim style As Byte = CByte((fmt.bUnderlineType And &HF))
                Return CType(style, UnderlineStyle)
            End If
        End Get
        Set(ByVal value As UnderlineStyle)
            ' Ensure we don't alter the color 
            Dim color As UnderlineColor = SelectionUnderlineColor

            ' Ensure we don't show it if it shouldn't be shown 
            If value = UnderlineStyle.None Then
                color = UnderlineColor.Black
            End If

            ' Set the underline type 
            Dim fmt As New CHARFORMAT2()
            fmt.cbSize = Marshal.SizeOf(fmt)
            fmt.dwMask = CFM_UNDERLINETYPE
            fmt.bUnderlineType = CByte((CByte(value) Or CByte(color)))
            SendMessage(New HandleRef(Me, Handle), EM_SETCHARFORMAT, SCF_SELECTION, fmt)
        End Set
    End Property


    ''' <summary> 
    ''' Gets or sets the underline color to apply to the current selection or insertion point. 
    ''' </summary> 
    ''' <value>A <see cref="UnderlineColor"/> that represents the underline color to 
    ''' apply to the current text selection or to text entered after the insertion point.</value> 
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property SelectionUnderlineColor() As UnderlineColor
        Get
            Dim fmt As New CHARFORMAT2()
            fmt.cbSize = Marshal.SizeOf(fmt)

            ' Get the underline color 
            SendMessage(New HandleRef(Me, Handle), EM_GETCHARFORMAT, SCF_SELECTION, fmt)
            If (fmt.dwMask And CFM_UNDERLINETYPE) = 0 Then
                Return UnderlineColor.None
            Else
                Dim style As Byte = CByte((fmt.bUnderlineType And &HF0))
                Return CType(style, UnderlineColor)
            End If
        End Get
        Set(ByVal value As UnderlineColor)
            ' If the an underline color of "None" is specified, remove underline effect 
            If value = UnderlineColor.None Then
                SelectionUnderlineStyle = UnderlineStyle.None
            Else
                ' Ensure we don't alter the style 
                Dim style As UnderlineStyle = SelectionUnderlineStyle

                ' Ensure we don't show it if it shouldn't be shown 
                If style = UnderlineStyle.None Then
                    value = UnderlineColor.Black
                End If

                ' Set the underline color 
                Dim fmt As New CHARFORMAT2()
                fmt.cbSize = Marshal.SizeOf(fmt)
                fmt.dwMask = CFM_UNDERLINETYPE
                fmt.bUnderlineType = CByte((CByte(style) Or CByte(value)))
                SendMessage(New HandleRef(Me, Handle), EM_SETCHARFORMAT, SCF_SELECTION, fmt)
            End If
        End Set
    End Property

    ''' <summary> 
    ''' Maintains performance while updating. 
    ''' </summary> 
    ''' <remarks> 
    ''' <para> 
    ''' It is recommended to call this method before doing 
    ''' any major updates that you do not wish the user to 
    ''' see. Remember to call EndUpdate when you are finished 
    ''' with the update. Nested calls are supported. 
    ''' </para> 
    ''' <para> 
    ''' Calling this method will prevent redrawing. It will 
    ''' also setup the event mask of the underlying richedit 
    ''' control so that no events are sent. 
    ''' </para> 
    ''' </remarks> 
    Public Sub SuspendDraw()
        ' Deal with nested calls 
        _Updating += 1
        If _Updating > 1 Then
            Return
        End If

        ' Prevent the control from raising any events 
        _OldEventMask = SendMessage(New HandleRef(Me, Handle), EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero)

        If Not Visible Then Return

        ' Prevent the control from redrawing itself 
        SendMessage(New HandleRef(Me, Handle), WM_SETREDRAW, 0, 0)
    End Sub

    ''' <summary> 
    ''' Resumes drawing and event handling. 
    ''' </summary> 
    ''' <remarks> 
    ''' This method should be called every time a call is made 
    ''' made to BeginUpdate. It resets the event mask to it's 
    ''' original value and enables redrawing of the control. 
    ''' </remarks> 
    Public Sub ResumeDraw()
        ' Deal with nested calls 
        _Updating -= 1
        If _Updating > 0 Then
            Return
        End If

        If Visible Then
            ' Allow the control to redraw itself 
            SendMessage(New HandleRef(Me, Handle), WM_SETREDRAW, 1, 0)
        End If

        ' Allow the control to raise event messages 
        SendMessage(New HandleRef(Me, Handle), EM_SETEVENTMASK, IntPtr.Zero, _OldEventMask)
    End Sub

    Protected Overrides Sub OnSelectionChanged(ByVal e As System.EventArgs)
        If _Updating > 0 Then Return
        MyBase.OnSelectionChanged(e)
    End Sub

    ''' <summary> 
    ''' This scrolls the scroll bar down to the bottom of the window. 
    ''' </summary> 
    Public Sub ScrollToBottom()
        SendMessage(New HandleRef(Me, Handle), WM_VSCROLL, SB_BOTTOM, 0)
    End Sub

    ''' <summary> 
    ''' Scrolls the data up one page. 
    ''' </summary> 
    Public Sub ScrollPageUp()
        SendMessage(New HandleRef(Me, Handle), WM_VSCROLL, SB_PAGEUP, 0)
    End Sub

    ''' <summary> 
    ''' Scrolls the data down one page. 
    ''' </summary> 
    Public Sub ScrollPageDown()
        SendMessage(New HandleRef(Me, Handle), WM_VSCROLL, SB_PAGEDOWN, 0)
    End Sub

    ''' <summary> 
    ''' Scrolls the data up.
    ''' </summary> 
    Public Sub ScrollLineUp(ByVal num As Integer)
        For i As Integer = 0 To num - 1
            SendMessage(New HandleRef(Me, Handle), WM_VSCROLL, SB_LINEUP, 0)
        Next
    End Sub

    ''' <summary> 
    ''' Scrolls the data down.
    ''' </summary> 
    Public Sub ScrollLineDown(ByVal num As Integer)
        For i As Integer = 0 To num - 1
            SendMessage(New HandleRef(Me, Handle), WM_VSCROLL, SB_LINEDOWN, 0)
        Next
    End Sub

    ''' <summary> 
    ''' This is the information associated with the scroll bar, showing it's position 
    ''' and other details. 
    ''' </summary> 
    ''' <value>the scroll bar information</value> 
    Public ReadOnly Property VerticalScrollInformation() As ScrollBarInformation
        Get
            Dim info As New SCROLLINFO()
            info.cbSize = Marshal.SizeOf(info)
            info.fMask = SIF_ALL
            Dim ret As Integer = GetScrollInfo(New HandleRef(Me, Handle), SBS_VERT, info)
            If ret = 0 Then
                Return Nothing
            Else
                Return New ScrollBarInformation(info.nMin, info.nMax, info.nPage, info.nPos, info.nTrackPos)
            End If
        End Get
    End Property

    Public Property ScrollPosition() As Integer
        Get
            Dim info As New SCROLLINFO()
            info.cbSize = Marshal.SizeOf(info)
            info.fMask = SIF_ALL
            Dim ret As Integer = GetScrollInfo(New HandleRef(Me, Handle), SBS_VERT, info)
            Return info.nPos
        End Get
        Set(ByVal value As Integer)
            If value < VerticalScrollInformation.Position Then
                While True
                    If value >= VerticalScrollInformation.Position Then Exit While
                    Dim PreviousPosition = VerticalScrollInformation.Position
                    ScrollLineUp(1)
                    If PreviousPosition = VerticalScrollInformation.Position Then Exit While
                End While
            ElseIf value > VerticalScrollInformation.Position Then
                While True
                    If value <= VerticalScrollInformation.Position Then Exit While
                    Dim PreviousPosition = VerticalScrollInformation.Position
                    ScrollLineDown(1)
                    If PreviousPosition = VerticalScrollInformation.Position Then Exit While
                End While
            End If
        End Set
    End Property

    ' Not used in this application. Descriptions can be found with documentation 
    ' of Windows GDI function SetMapMode 
    Private Const MM_TEXT As Integer = 1
    Private Const MM_LOMETRIC As Integer = 2
    Private Const MM_HIMETRIC As Integer = 3
    Private Const MM_LOENGLISH As Integer = 4
    Private Const MM_HIENGLISH As Integer = 5
    Private Const MM_TWIPS As Integer = 6

    ' Ensures that the metafile maintains a 1:1 aspect ratio 
    Private Const MM_ISOTROPIC As Integer = 7

    ' Allows the x-coordinates and y-coordinates of the metafile to be adjusted 
    ' independently 
    Private Const MM_ANISOTROPIC As Integer = 8

    ' The number of hundredths of millimeters (0.01 mm) in an inch 
    ' For more information, see GetImagePrefix() method. 
    Private Const HMM_PER_INCH As Integer = 2540

    ' The number of twips in an inch 
    ' For more information, see GetImagePrefix() method. 
    Private Const TWIPS_PER_INCH As Integer = 1440

    ' Dictionary that mapas Framework font families to RTF font families 
    Private rtfFontFamily As Dictionary(Of String, String)

    ' The horizontal resolution at which the control is being displayed 
    Private xDpi As Single

    ' The vertical resolution at which the control is being displayed 
    Private yDpi As Single

    Private Const WM_USER As Integer = &H400
    Private Const WM_REFLECT As Integer = WM_USER + &H1C00
    Private Const WM_NOTIFY As Integer = &H4E
    Protected Overloads Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case WM_VSCROLL
                MyBase.WndProc(m)
                If (m.WParam.ToInt32() And &HFFFF) = SB_THUMBTRACK Then
                    OnVScroll(EventArgs.Empty)
                End If
                If (m.WParam.ToInt32() And &HFFFF) = SB_THUMBPOSITION Then
                    OnVScroll(EventArgs.Empty)
                End If
            Case WM_HSCROLL
                MyBase.WndProc(m)
                If (m.WParam.ToInt32() And &HFFFF) = SB_THUMBTRACK Then
                    OnHScroll(EventArgs.Empty)
                End If
                If (m.WParam.ToInt32() And &HFFFF) = SB_THUMBPOSITION Then
                    OnHScroll(EventArgs.Empty)
                End If
            Case Else
                MyBase.WndProc(m)
        End Select
    End Sub

    Private ITextDocumentValue As ITextDocument = Nothing
    Private ITextDocumentPtr As IntPtr = IntPtr.Zero

    Protected ReadOnly Property TextDocument() As ITextDocument
        Get
            If Me.ITextDocumentValue Is Nothing Then
                LoadInterfaces()
            End If
            Return Me.ITextDocumentValue
        End Get
    End Property

    Private Const tomFalse As Integer = 0
    Private Const tomTrue As Integer = -1
    Private Const tomSuspend As Integer = -9999995
    Private Const tomResume As Integer = -9999994
    Public Sub ClearUndoHistory()
        Dim td As ITextDocument = Me.TextDocument
        Dim zero As Integer = 0
        td.Undo(tomFalse, zero)
        td.Undo(tomTrue, zero)
    End Sub
    Public Sub SuspendUndoHistory()
        Dim td As ITextDocument = Me.TextDocument
        Dim zero As Integer = 0
        td.Undo(tomSuspend, zero)
    End Sub
    Public Sub ResumeUndoHistory()
        Dim td As ITextDocument = Me.TextDocument
        Dim zero As Integer = 0
        td.Undo(tomResume, zero)
    End Sub

    Private Const EM_GETOLEINTERFACE As Integer = WM_USER + 60
    Private Sub LoadInterfaces()
        If Me.ITextDocumentValue Is Nothing Then
            ' Allocate the ptr that EM_GETOLEINTERFACE will fill in. 
            Dim ptr As IntPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(GetType(IntPtr)))
            ' Alloc the ptr. 
            Marshal.WriteIntPtr(ptr, IntPtr.Zero)
            ' Clear it. 
            Try
                If IntPtr.Zero <> SendMessage(New HandleRef(Me, Handle), EM_GETOLEINTERFACE, IntPtr.Zero, ptr) Then
                    ' Read the returned pointer. 
                    Dim pRichEdit As IntPtr = Marshal.ReadIntPtr(ptr)
                    Try
                        If pRichEdit <> IntPtr.Zero Then
                            ' Query for the IRichEditOle interface. 
                            Dim guid As New Guid("8CC497C0-A1DF-11ce-8098-00AA0047BE5D")
                            Marshal.QueryInterface(pRichEdit, guid, Me.ITextDocumentPtr)

                            ' Wrap it in the C# interface for IRichEditOle. 
                            Me.ITextDocumentValue = DirectCast(Marshal.GetTypedObjectForIUnknown(Me.ITextDocumentPtr, GetType(ITextDocument)), ITextDocument)
                            If Me.ITextDocumentValue Is Nothing Then
                                Throw New Exception("Failed to get the object wrapper for the interface.")
                            End If
                        Else
                            Throw New Exception("Failed to get the pointer.")
                        End If
                    Finally
                        Marshal.Release(pRichEdit)
                    End Try
                Else
                    Throw New Exception("EM_GETOLEINTERFACE failed.")
                End If
            Catch generatedExceptionName As Exception
                Me.ReleaseInterfaces()
            Finally
                ' Free the ptr memory. 
                Marshal.FreeCoTaskMem(ptr)
            End Try
        End If
    End Sub

    Public Sub ReleaseInterfaces()
        If Me.ITextDocumentPtr <> IntPtr.Zero Then
            Marshal.Release(Me.ITextDocumentPtr)
        End If

        Me.ITextDocumentPtr = IntPtr.Zero
        Me.ITextDocumentValue = Nothing
    End Sub

    Shared static_Dispose_DisposedValue As Boolean = False
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If static_Dispose_DisposedValue Then
            Me.ReleaseInterfaces()
            static_Dispose_DisposedValue = True
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private Sub ExtendedRichTextBox_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        If e.Handled Then Return
        Select Case e.KeyData
            Case Keys.Control Or Keys.X, Keys.Shift Or Keys.Delete
                If Me.ReadOnly Then Return
                If Me.SelectedText <> "" Then
                    Try
                        Clipboard.SetText(Me.SelectedText)
                    Catch ex As ExternalException
                    End Try
                    Me.SelectedText = ""
                End If
            Case Keys.Control Or Keys.C, Keys.Control Or Keys.Insert
                If Me.SelectedText <> "" Then
                    Try
                        Clipboard.SetText(Me.SelectedText)
                    Catch ex As ExternalException
                    End Try
                End If
            Case Keys.Control Or Keys.V, Keys.Shift Or Keys.Insert
                If Me.ReadOnly Then Return
                If Clipboard.ContainsText Then
                    Try
                        Me.SelectedText = Clipboard.GetText()
                    Catch ex As ExternalException
                    End Try
                End If
            Case Else
                Return
        End Select
        e.Handled = True
    End Sub
End Class

''' <summary> 
''' This class contains all the scroll bar information. 
''' </summary> 
Public Class ScrollBarInformation
    Private nMin As Integer = 0
    Private nMax As Integer = 0
    Private nPage As Integer = 0
    Private nPos As Integer = 0
    Private nTrackPos As Integer = 0

    ''' <summary> 
    ''' Sets up an empty scroll bar information class. 
    ''' </summary> 
    Public Sub New()

    End Sub

    ''' <summary> 
    ''' This sets up the scroll bar information with all the basic details. 
    ''' </summary> 
    ''' <param name="min">the minimum size</param> 
    ''' <param name="max">the maximum size</param> 
    ''' <param name="page">the size of the page</param> 
    ''' <param name="pos">the position of the thingy</param> 
    ''' <param name="trackpos">this is updated while the scroll bar is wiggling up and down.</param> 
    Public Sub New(ByVal min As Integer, ByVal max As Integer, ByVal page As Integer, ByVal pos As Integer, ByVal trackpos As Integer)
        Me.nMin = min
        Me.nMax = max
        Me.nPage = page
        Me.nPos = pos
        Me.nTrackPos = trackpos
    End Sub

    ''' <summary> 
    ''' Specifies the minimum scrolling position. 
    ''' </summary> 
    ''' <value>the minimum scrolling position</value> 
    Public Property Minimum() As Integer
        Get
            Return nMin
        End Get
        Set(ByVal value As Integer)
            nMin = value
        End Set
    End Property

    ''' <summary> 
    ''' Specifies the maximum scrolling position. 
    ''' </summary> 
    ''' <value>the maximum scrolling position</value> 
    Public Property Maximum() As Integer
        Get
            Return nMax
        End Get
        Set(ByVal value As Integer)
            nMax = value
        End Set
    End Property

    ''' <summary> 
    ''' Specifies the page size. A scroll bar uses this value to determine the 
    ''' appropriate size of the proportional scroll box. 
    ''' </summary> 
    ''' <value></value> 
    Public Property Page() As Integer
        Get
            Return nPage
        End Get
        Set(ByVal value As Integer)
            nPage = value
        End Set
    End Property

    ''' <summary> 
    ''' The position of the thumb inside the scroll bar. 
    ''' </summary> 
    ''' <value></value> 
    Public Property Position() As Integer
        Get
            Return nPos
        End Get
        Set(ByVal value As Integer)
            nPos = value
        End Set
    End Property

    ''' <summary> 
    ''' Specifies the immediate position of a scroll box that the user is dragging. 
    ''' An application can retrieve this value while processing the SB_THUMBTRACK 
    ''' request code. An application cannot set the immediate scroll position; the 
    ''' SetScrollInfo function ignores this member. 
    ''' </summary> 
    ''' <value>the immediated position of the scroll box</value> 
    Public Property TrackPosition() As Integer
        Get
            Return nTrackPos
        End Get
        Set(ByVal value As Integer)
            nTrackPos = value
        End Set
    End Property
End Class

<ComImport()> _
<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
<Guid("8CC497C0-A1DF-11ce-8098-00AA0047BE5D")> _
Public Interface ITextDocument
    ' IDispath methods (We never use them) 
    Function GetIDsOfNames(ByVal riid As Guid, ByVal rgszNames As IntPtr, ByVal cNames As UInteger, ByVal lcid As UInteger, ByRef rgDispId As Integer) As Integer
    Function GetTypeInfo(ByVal iTInfo As UInteger, ByVal lcid As UInteger, ByVal ppTInfo As IntPtr) As Integer
    Function GetTypeInfoCount(ByRef pctinfo As UInteger) As Integer
    Function Invoke(ByVal dispIdMember As UInteger, ByVal riid As Guid, ByVal lcid As UInteger, ByVal wFlags As UInteger, ByVal pDispParams As IntPtr, ByVal pvarResult As IntPtr, _
    ByVal pExcepInfo As IntPtr, ByRef puArgErr As UInteger) As Integer

    ' ITextDocument methods 
    ' [retval][out] BSTR* 
    Function GetName(<[In](), Out(), MarshalAs(UnmanagedType.BStr)> ByRef pName As String) As Integer

    ' [retval][out] ITextSelection** 
    Function GetSelection(ByVal ppSel As IntPtr) As Integer

    ' [retval][out] 
    Function GetStoryCount(ByRef pCount As Integer) As Integer

    ' [retval][out] ITextStoryRanges** 
    Function GetStoryRanges(ByVal ppStories As IntPtr) As Integer

    ' [retval][out] 
    Function GetSaved(ByRef pValue As Integer) As Integer

    ' [in] 
    Function SetSaved(ByVal Value As Integer) As Integer

    ' [retval][out] 
    Function GetDefaultTabStop(ByRef pValue As Single) As Integer

    ' [in] 
    Function SetDefaultTabStop(ByVal Value As Single) As Integer

    Function [New]() As Integer

    ' [in] VARIANT * 
    ' [in] 
    ' [in] 
    Function Open(ByVal pVar As IntPtr, ByVal Flags As Integer, ByVal CodePage As Integer) As Integer

    ' [in] VARIANT * 
    ' [in] 
    ' [in] 
    Function Save(ByVal pVar As IntPtr, ByVal Flags As Integer, ByVal CodePage As Integer) As Integer

    ' [retval][out] 
    Function Freeze(ByRef pCount As Integer) As Integer

    ' [retval][out] 
    Function Unfreeze(ByRef pCount As Integer) As Integer

    Function BeginEditCollection() As Integer

    Function EndEditCollection() As Integer

    ' [in] 
    ' [retval][out] 
    Function Undo(ByVal Count As Integer, ByRef prop As Integer) As Integer

    ' [in] 
    ' [retval][out] 
    Function Redo(ByVal Count As Integer, ByRef prop As Integer) As Integer

    ' [in] 
    ' [in] 
    ' [retval][out] ITextRange** 
    Function Range(ByVal cp1 As Integer, ByVal cp2 As Integer, ByVal ppRange As IntPtr) As Integer

    ' [in] 
    ' [in] 
    ' [retval][out] ITextRange** 
    Function RangeFromPoint(ByVal x As Integer, ByVal y As Integer, ByVal ppRange As IntPtr) As Integer
End Interface
