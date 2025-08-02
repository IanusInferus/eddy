'==========================================================================
'
'  File:        LocalizationProject.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 本地化项目项目文件
'  Version:     2025.08.03.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports Firefly
Imports Firefly.Setting
Imports Eddy.Interfaces

Public Class LocalizationProject
    Public TextName As String
    Public TextNumber As Integer
    Public Maximized As Boolean = False
    Public WindowWidth As Integer = 800
    Public WindowHeight As Integer = 600
    Public LocalizationTextBoxDescriptors As LocalizationTextBoxDescriptor()
    Public MainLocalizationTextBox As String

    Public EnableLocalizationGrid As Boolean = True
    Public LocalizationGridAutoResizeWidth As Boolean = True
    Public LocalizationGridWidthRatio As Double = 300 / 800
    Public LocalizationRowHeaderWidthRatio As Double = 50 / 300

    Public Plugins As PluginDescriptor()
    Public UIPlugin As String
End Class

Public Class LocalizationTextBoxDescriptor
    Public Name As String
    Public DisplayName As String

    Public HeightRatio As Double = 1 / 3
    Public ColumnWidthRatio As Double = 120 / 300

    Public Directory As String
    Public Extension As String
    Public Type As String
    Public Editable As Boolean
    Public Encoding As String

    Public FontName As String
    Public FontPixel As Integer
    Public Space As Integer

    Public LocFontScale As Double = 1
End Class

Public Class PluginDescriptor
    Public AssemblyName As String
    Public Enable As Boolean = True
End Class

Public Class LocalizationProjectUser
    Public TextName As String
    Public TextNumber As Integer
    Public Maximized As Boolean = False
    Public WindowWidth As Integer = 800
    Public WindowHeight As Integer = 600
    Public LocalizationTextBoxDescriptors As LocalizationTextBoxDescriptorUser()

    Public EnableLocalizationGrid As Boolean = True
    Public LocalizationGridAutoResizeWidth As Boolean = True
    Public LocalizationGridWidthRatio As Double = 300 / 800
    Public LocalizationRowHeaderWidthRatio As Double = 50 / 300

    Public Plugins As PluginDescriptor()
    Public UIPlugin As String
End Class

Public Class LocalizationTextBoxDescriptorUser
    Public HeightRatio As Double = 1 / 3
    Public ColumnWidthRatio As Double = 120 / 300

    Public FontName As String
    Public FontPixel As Integer
    Public Space As Integer

    Public LocFontScale As Double = 1
End Class

Public NotInheritable Class LocalizationProjectGlobalToUserMapper
    Private Sub New()
    End Sub

    Public Shared Function GetMappedObject(ByVal Project As LocalizationProject) As LocalizationProjectUser
        Dim ProjectUser As New LocalizationProjectUser

        ProjectUser.TextName = Project.TextName
        ProjectUser.TextNumber = Project.TextNumber
        ProjectUser.Maximized = Project.Maximized
        ProjectUser.WindowWidth = Project.WindowWidth
        ProjectUser.WindowHeight = Project.WindowHeight
        ProjectUser.LocalizationTextBoxDescriptors = Project.LocalizationTextBoxDescriptors.Select(Function(d) GetMappedObject(d)).ToArray
        ProjectUser.EnableLocalizationGrid = Project.EnableLocalizationGrid
        ProjectUser.LocalizationGridAutoResizeWidth = Project.LocalizationGridAutoResizeWidth
        ProjectUser.LocalizationGridWidthRatio = Project.LocalizationGridWidthRatio
        ProjectUser.LocalizationRowHeaderWidthRatio = Project.LocalizationRowHeaderWidthRatio
        ProjectUser.Plugins = Project.Plugins
        ProjectUser.UIPlugin = Project.UIPlugin

        Return ProjectUser
    End Function

    Public Shared Function GetMappedObject(ByVal Descriptor As LocalizationTextBoxDescriptor) As LocalizationTextBoxDescriptorUser
        Dim DescriptorUser As New LocalizationTextBoxDescriptorUser

        DescriptorUser.HeightRatio = Descriptor.HeightRatio
        DescriptorUser.ColumnWidthRatio = Descriptor.ColumnWidthRatio
        DescriptorUser.FontName = Descriptor.FontName
        DescriptorUser.FontPixel = Descriptor.FontPixel
        DescriptorUser.Space = Descriptor.Space
        DescriptorUser.LocFontScale = Descriptor.LocFontScale

        Return DescriptorUser
    End Function

    Public Shared Function GetInverseMappedObject(ByVal ProjectUser As LocalizationProjectUser, ByVal InitialProject As LocalizationProject) As LocalizationProject
        If ProjectUser.LocalizationTextBoxDescriptors.Length <> InitialProject.LocalizationTextBoxDescriptors.Length Then Throw New InvalidDataException

        Dim Project As New LocalizationProject
        Project.MainLocalizationTextBox = InitialProject.MainLocalizationTextBox

        Project.TextName = ProjectUser.TextName
        Project.TextNumber = ProjectUser.TextNumber
        Project.Maximized = ProjectUser.Maximized
        Project.WindowWidth = ProjectUser.WindowWidth
        Project.WindowHeight = ProjectUser.WindowHeight
        Project.LocalizationTextBoxDescriptors = ProjectUser.LocalizationTextBoxDescriptors.Zip(InitialProject.LocalizationTextBoxDescriptors, Function(d, id) GetInverseMappedObject(d, id)).ToArray
        Project.EnableLocalizationGrid = ProjectUser.EnableLocalizationGrid
        Project.LocalizationGridAutoResizeWidth = ProjectUser.LocalizationGridAutoResizeWidth
        Project.LocalizationGridWidthRatio = ProjectUser.LocalizationGridWidthRatio
        Project.LocalizationRowHeaderWidthRatio = ProjectUser.LocalizationRowHeaderWidthRatio
        Project.Plugins = ProjectUser.Plugins
        Project.UIPlugin = ProjectUser.UIPlugin

        Return Project
    End Function

    Public Shared Function GetInverseMappedObject(ByVal DescriptorUser As LocalizationTextBoxDescriptorUser, ByVal InitialDescriptor As LocalizationTextBoxDescriptor) As LocalizationTextBoxDescriptor
        Dim Descriptor As New LocalizationTextBoxDescriptor

        Descriptor.Name = InitialDescriptor.Name
        Descriptor.DisplayName = InitialDescriptor.DisplayName
        Descriptor.Directory = InitialDescriptor.Directory
        Descriptor.Extension = InitialDescriptor.Extension
        Descriptor.Type = InitialDescriptor.Type
        Descriptor.Editable = InitialDescriptor.Editable
        Descriptor.Encoding = InitialDescriptor.Encoding

        Descriptor.HeightRatio = DescriptorUser.HeightRatio
        Descriptor.ColumnWidthRatio = DescriptorUser.ColumnWidthRatio
        Descriptor.FontName = DescriptorUser.FontName
        Descriptor.FontPixel = DescriptorUser.FontPixel
        Descriptor.Space = DescriptorUser.Space
        Descriptor.LocFontScale = DescriptorUser.LocFontScale

        Return Descriptor
    End Function
End Class
