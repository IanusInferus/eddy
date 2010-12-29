'==========================================================================
'
'  File:        Plugin.vb
'  Location:    Eddy.Voice <Visual Basic .Net>
'  Description: 文本本地化工具版本管理插件
'  Version:     2010.12.29.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Xml.Linq
Imports System.Diagnostics
Imports Firefly
Imports Firefly.Mapping
Imports Eddy
Imports Eddy.Interfaces

Public Class Config
    Public CheckPathTemplate As String = "%s\.svn"
    Public CheckoutCommandTemplate As String = "TortoiseProc /command:checkout /path:""%s"" /url:""http://%n"" /closeonend:0"
    Public UpdateCommandTemplate As String = "TortoiseProc /command:update /path:""%s"" /closeonend:0"
    Public CommitCommandTemplate As String = "TortoiseProc /command:commit /path:""%s"" /closeonend:0"
End Class

Public Class Voice
    Inherits TextLocalizerBase
    Implements ITextLocalizerToolStripButtonPlugin
    Implements ITextLocalizerConfigurationPlugin

    Private Config As Config
    Public Sub SetConfiguration(ByVal Config As XElement) Implements ITextLocalizerConfigurationPlugin.SetConfiguration
        If Config Is Nothing Then
            Me.Config = New Config
        Else
            Me.Config = (New XmlSerializer).Read(Of Config)(Config)
        End If
    End Sub
    Public Function GetConfiguration() As XElement Implements ITextLocalizerConfigurationPlugin.GetConfiguration
        Return (New XmlSerializer).Write(Me.Config)
    End Function

    Public Function GetToolStripButtonDescriptors() As IEnumerable(Of ToolStripButtonDescriptor) Implements ITextLocalizerToolStripButtonPlugin.GetToolStripButtonDescriptors
        Return New ToolStripButtonDescriptor() {
            New ToolStripButtonDescriptor With {.Image = My.Resources.Update, .Text = "更新...", .Click = AddressOf Update},
            New ToolStripButtonDescriptor With {.Image = My.Resources.Commit, .Text = "提交...", .Click = AddressOf Commit}
        }
    End Function

    Private Sub Update()
        Dim Column = Columns(Controller.ColumnIndex)
        Dim DirectoryPath = GetAbsolutePath(Column.Directory, Environment.CurrentDirectory)
        Dim DirectoryName = GetFileName(DirectoryPath)

        Dim CheckPath = Config.CheckPathTemplate.Replace("%s", DirectoryPath).Replace("%n", DirectoryName)

        If Not Directory.Exists(CheckPath) Then
            Dim CheckoutCommand = Config.CheckoutCommandTemplate.Replace("%s", DirectoryPath).Replace("%n", DirectoryName)
            Execute(CheckoutCommand)
        Else
            Dim UpdateCommand = Config.UpdateCommandTemplate.Replace("%s", DirectoryPath).Replace("%n", DirectoryName)
            Execute(UpdateCommand)
        End If

    End Sub

    Private Sub Commit()
        Dim Column = Columns(Controller.ColumnIndex)
        Dim DirectoryPath = GetAbsolutePath(Column.Directory, Environment.CurrentDirectory)
        Dim DirectoryName = GetFileName(DirectoryPath)

        Dim CommitCommand = Config.CommitCommandTemplate.Replace("%s", DirectoryPath).Replace("%n", DirectoryName)
        Execute(CommitCommand)
    End Sub

    Private Sub Execute(ByVal Command As String)
        Microsoft.VisualBasic.Interaction.Shell(Command)
    End Sub
End Class
