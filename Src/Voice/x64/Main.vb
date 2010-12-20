'==========================================================================
'
'  File:        Main.vb
'  Location:    Eddy.Voice <Visual Basic .Net>
'  Description: 文本本地化工具朗读插件服务
'  Version:     2010.12.18.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Firefly

Public Module Main
    Public Function Main() As Integer
        If System.Diagnostics.Debugger.IsAttached Then
            Return MainInner()
        Else
            Try
                Return MainInner()
            Catch ex As Exception
                Console.WriteLine(ExceptionInfo.GetExceptionInfo(ex))
                Return -1
            End Try
        End If
    End Function

    Public Function MainInner() As Integer
        Dim CmdLine = CommandLine.GetCmdLine()
        Dim argv = CmdLine.Arguments

        If argv.Length = 2 Then
            Listen(argv(0), argv(1))
        Else
            DisplayInfo()
            Return -1
        End If
        Return 0
    End Function

    Public Sub DisplayInfo()
        Console.WriteLine("文本本地化工具朗读插件服务器")
        Console.WriteLine("由Eddy.Voice.dll加载。无法从命令行加载。")
    End Sub

    Public Sub Listen(ByVal PipeIn As String, ByVal PipeOut As String)

    End Sub
End Module
