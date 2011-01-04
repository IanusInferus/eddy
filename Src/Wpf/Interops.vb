'==========================================================================
'
'  File:        Interops.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: WinForm与WPF互操作
'  Version:     2011.01.04.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Public Module Interops
    <Extension()> Public Function ToWpfImageSource(ByVal This As Image) As ImageSource
        Using ms As New MemoryStream
            This.Save(ms, ImageFormat.Png)
            ms.Position = 0
            Dim bd = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad)
            Dim writable = New WriteableBitmap(bd.Frames.Single())
            writable.Freeze()
            Return writable
        End Using
    End Function
End Module
