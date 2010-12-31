'==========================================================================
'
'  File:        Main.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 文本本地化工具入口函数
'  Version:     2010.12.31.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Diagnostics
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.GUI
Imports Eddy.Base

Public Module Main
    Public Sub Application_ThreadException(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)
        ExceptionHandler.PopupException(e.Exception, New StackTrace(4, True))
    End Sub

    Public Function Main() As Integer
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        If Debugger.IsAttached Then
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException)
            Return MainInner()
        Else
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
            Try
                AddHandler Application.ThreadException, AddressOf Application_ThreadException
                Return MainInner()
            Catch ex As Exception
                ExceptionHandler.PopupException(ex)
                Return -1
            Finally
                RemoveHandler Application.ThreadException, AddressOf Application_ThreadException
            End Try
        End If
    End Function

    Public Function MainInner() As Integer
        Dim CurrentProjectFilePath As String

        Dim Args = CommandLine.GetCmdLine.Arguments
        Select Case Args.Length
            Case 0
                Dim Files As String() = Directory.GetFiles(System.Environment.CurrentDirectory, "*.locproj")
                If Files.Length = 0 Then
                    MessageDialog.Show("当前文件夹下无法找到.locproj项目文件。", ExceptionInfo.AssemblyDescriptionOrTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop)
                    Return -1
                ElseIf Files.Length > 1 Then
                    MessageDialog.Show("当前文件夹下找到多个.locproj项目文件。", ExceptionInfo.AssemblyDescriptionOrTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop)
                    Return -1
                Else
                    CurrentProjectFilePath = Files(0)
                End If
            Case 1
                CurrentProjectFilePath = Args(0)
            Case Else
                MessageDialog.Show("参数无法识别。", ExceptionInfo.AssemblyDescriptionOrTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop)
                Return -1
        End Select

        Using Controller As New ApplicationController(CurrentProjectFilePath)
            Dim UserInterfacePlugins = Controller.ApplicationData.UserInterfacePlugins
            If UserInterfacePlugins.Count = 0 Then
                MessageDialog.Show("没有用户界面插件。", ExceptionInfo.AssemblyDescriptionOrTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop)
                Return -1
            End If
            Dim UserInterface As ITextLocalizerUserInterfacePlugin = Nothing
            For Each UserInterfacePlugin In UserInterfacePlugins.Where(Function(p) String.Equals(p.GetType().Assembly.GetName().Name, Controller.ApplicationData.CurrentProject.UIPlugin, StringComparison.OrdinalIgnoreCase))
                UserInterface = UserInterfacePlugin
            Next
            If UserInterface Is Nothing Then
                UserInterface = UserInterfacePlugins.First
            End If

            UserInterface.Initialize(Controller.ApplicationData)
            Return UserInterface.Run()
        End Using
    End Function
End Module
