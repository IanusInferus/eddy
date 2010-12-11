'==========================================================================
'
'  File:        ITextLocalizerUserInterfacePlugin.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 界面插件接口
'  Version:     2010.12.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports Eddy.Interfaces

''' <summary>界面插件接口</summary>
Public Interface ITextLocalizerUserInterfacePlugin
    Inherits ITextLocalizerPlugin

    Sub Initialize(ByVal ApplicationData As TextLocalizerData)
    Sub Run()
End Interface
