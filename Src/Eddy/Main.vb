'==========================================================================
'
'  File:        Main.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 文本本地化工具入口函数
'  Version:     2010.09.14.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.IO
Imports System.Diagnostics
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.GUI

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
                    MessageBox.Show("当前文件夹下无法找到.locproj项目文件。", My.Application.Info.Description, MessageBoxButtons.OK, MessageBoxIcon.Stop)
                    Return -1
                End If
                CurrentProjectFilePath = Files(0)
            Case 1
                CurrentProjectFilePath = Args(0)
            Case Else
                MessageBox.Show("参数无法识别。", My.Application.Info.Description, MessageBoxButtons.OK, MessageBoxIcon.Stop)
                Return -1
        End Select

        Using Controller As New ApplicationController(CurrentProjectFilePath)
            Dim Form = My.Forms.FormMain

            Form.Initialize(Controller.ApplicationData)

            Application.Run(Form)
        End Using

        Return 0
    End Function
End Module
