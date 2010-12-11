'==========================================================================
'
'  File:        ApplicationController.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 文本本地化工具主控制器
'  Version:     2010.12.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Windows.Forms
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Setting
Imports Firefly.GUI
Imports Eddy.Interfaces

Public Class TextLocalizerData
    Implements ITextLocalizerData

    Public ApplicationName As String = "漩涡文本本地化工具(Firefly.Eddy)"

    Public CurrentProject As LocalizationProject
    Public CurrentProjectFilePath As String
    Public CurrentUserFilePath As String

    Public Columns As New List(Of LocalizationTextProvider)
    Public NameToColumn As New Dictionary(Of String, Integer)
    Public TextNames As New List(Of String)
    Public TextNameDict As New Dictionary(Of String, Integer)

    Public Plugins As New List(Of ITextLocalizerPlugin)
    Public TextHighlighters As New List(Of ITextLocalizerTextHighlighter)
    Public GridTextFormatters As New List(Of ITextLocalizerGridTextFormatter)
    Public ToolStripButtonPlugins As New List(Of ITextLocalizerToolStripButtonPlugin)
    Public FormatPlugins As New List(Of ITextLocalizerFormatPlugin)
    Public TranslatorPlugins As New List(Of ITextLocalizerTranslatorPlugin)

    Public Factory As New LocalizationTextListFactoryAggregation(New ILocalizationTextListFactory() {New LocalizationTextListFactory()})

    Public Sub New()
    End Sub

    Private ReadOnly Property TextNamesInterface As IEnumerable(Of String) Implements ITextLocalizerData.TextNames
        Get
            Return TextNames
        End Get
    End Property
    Private ReadOnly Property ColumnsInterface As IEnumerable(Of LocalizationTextProvider) Implements ITextLocalizerData.Columns
        Get
            Return Columns
        End Get
    End Property
    Public ReadOnly Property MainColumnIndex As Integer Implements ITextLocalizerData.MainColumnIndex
        Get
            If NameToColumn.ContainsKey(CurrentProject.MainLocalizationTextBox) Then
                Return NameToColumn(CurrentProject.MainLocalizationTextBox)
            End If
            Dim i As Integer
            If Not Integer.TryParse(CurrentProject.MainLocalizationTextBox, i) Then Throw New InvalidDataException("MainLocalizationTextBox")
            If i < 0 OrElse i >= Columns.Count Then Throw New InvalidDataException("MainLocalizationTextBox")
            Return i
        End Get
    End Property
End Class

Public Class ApplicationController
    Implements IDisposable

    Public ApplicationData As New TextLocalizerData

    Private Sub Load()
        Try
            ApplicationData.CurrentProject = Xml.ReadFile(Of LocalizationProject)(ApplicationData.CurrentProjectFilePath)
            If File.Exists(ApplicationData.CurrentUserFilePath) Then
                Dim CurrentProjectUser = Xml.ReadFile(Of LocalizationProjectUser)(ApplicationData.CurrentUserFilePath)
                ApplicationData.CurrentProject = LocalizationProjectGlobalToUserMapper.GetInverseMappedObject(CurrentProjectUser, ApplicationData.CurrentProject)
            End If
        Catch ex As Exception
            Throw New InvalidDataException("读取配置文件错误", ex)
        End Try
    End Sub

    Private Sub Save()
        Try
            System.Environment.CurrentDirectory = GetFileDirectory(ApplicationData.CurrentProjectFilePath)

            Dim CurrentProjectUser = LocalizationProjectGlobalToUserMapper.GetMappedObject(ApplicationData.CurrentProject)

            Xml.WriteFile(ApplicationData.CurrentUserFilePath, UTF16, CurrentProjectUser)
        Catch
        End Try
    End Sub

    Public Sub New(ByVal CurrentProjectFilePath As String)
        ApplicationData.CurrentProjectFilePath = CurrentProjectFilePath
        ApplicationData.CurrentUserFilePath = ApplicationData.CurrentProjectFilePath & ".user"

        Dim CurrentDirectory = System.Environment.CurrentDirectory
        System.Environment.CurrentDirectory = GetFileDirectory(ApplicationData.CurrentProjectFilePath)

        Load()

        If ApplicationData.CurrentProject.LocalizationTextBoxDescriptors Is Nothing OrElse ApplicationData.CurrentProject.LocalizationTextBoxDescriptors.Length < 1 Then
            Throw New InvalidDataException("一栏文本框也没有")
        End If

        For n = 0 To ApplicationData.CurrentProject.LocalizationTextBoxDescriptors.Length - 1
            Dim Des = ApplicationData.CurrentProject.LocalizationTextBoxDescriptors(n)
            Dim Encoding As Encoding
            If Des.Encoding <> "" Then
                Dim PureDigits As Boolean = True
                For Each c In Des.Encoding
                    If Not Char.IsDigit(c) Then
                        PureDigits = False
                        Exit For
                    End If
                Next
                If PureDigits Then
                    Encoding = Encoding.GetEncoding(CInt(Des.Encoding))
                Else
                    Encoding = Encoding.GetEncoding(Des.Encoding)
                End If
            Else
                Encoding = TextEncoding.Default
            End If
            Dim tp As New LocalizationTextProvider(ApplicationData.Factory, Des.Name, Des.DisplayName, Des.Directory, Des.Extension, Des.Type, Not Des.Editable, Encoding)
            ApplicationData.Columns.Add(tp)
            ApplicationData.NameToColumn.Add(tp.Name, n)
        Next

        AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf RedirectBinding

        If ApplicationData.CurrentProject.Plugins Is Nothing Then ApplicationData.CurrentProject.Plugins = New PluginDescriptor() {}
        Dim PluginDescriptors As New List(Of PluginDescriptor)
        Dim PluginDescriptorSet As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each p In ApplicationData.CurrentProject.Plugins
#If CONFIG <> "Debug" Then
            Try
#End If
            Dim AsmName As New AssemblyName(p.AssemblyName)
            Dim Asm As Assembly
            Try
                Asm = Assembly.Load(AsmName)
            Catch
                If Not p.Enable Then Continue For
                Throw
            End Try
            Dim NewAsmName = Asm.GetName
            Dim IsVersionMatch = (NewAsmName.Version = AsmName.Version)
            Dim OldFullName = p.AssemblyName
            Dim FullName = NewAsmName.FullName
            AsmName.Version = Nothing
            NewAsmName.Version = Nothing
            Dim IsOtherMatch = NewAsmName.FullName.Equals(AsmName.FullName, StringComparison.OrdinalIgnoreCase)
            If Not IsOtherMatch Then
                If MessageDialog.Show("插件签名不匹配。\n原始签名：{0}\n实际签名：{1}\n承认该签名吗？".Descape.Formats(p.AssemblyName, FullName), ApplicationData.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
                    p.AssemblyName = FullName
                    IsVersionMatch = True
                Else
                    PluginDescriptorSet.Add(AsmName.Name)
                    Continue For
                End If
            End If
            If Not IsVersionMatch Then
                MessageDialog.Show("插件版本不匹配。\n原始签名：{0}\n实际签名：{1}\n将采用实际版本。".Descape.Formats(OldFullName, FullName), ApplicationData.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Information)
                p.AssemblyName = FullName
            End If
            If Not p.Enable Then
                If Not IsOtherMatch OrElse Not IsVersionMatch Then
                    If MessageDialog.Show("插件变更但没有启用。\n签名：{0}\n启用插件吗？".Descape.Formats(FullName), ApplicationData.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
                        p.Enable = True
                    End If
                End If
            End If
            If Not p.Enable Then Continue For
            If Not LoadPlugin(Asm) Then
                ExceptionHandler.PopupInfo("{0}中没有任何插件可加载。".Formats(p.AssemblyName))
            End If
            PluginDescriptorSet.Add(AsmName.Name)
#If CONFIG <> "Debug" Then
            Catch ex As Exception
                ExceptionHandler.PopupException(ex)
                If MessageDialog.Show("插件加载出错，程序将关闭。\n签名：{0}\n下次启动是否禁用该插件？".Descape.Formats(p.AssemblyName), ApplicationData.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
                    p.Enable = False
                    Save()
                End If
                Application.Exit()
                Return
            End Try
#End If
        Next
        Dim NewPlugins As New List(Of PluginDescriptor)
        For Each DllPath In Directory.GetFiles(Application.StartupPath, "*.dll", SearchOption.TopDirectoryOnly).OrderBy(Function(f) f, StringComparer.OrdinalIgnoreCase)
            Dim AsmToken As AssemblyName
            Try
                AsmToken = AssemblyName.GetAssemblyName(GetAbsolutePath(DllPath, Application.StartupPath))
            Catch
                '由于可能遇到非.Net程序集，所以需要作此判断
                Continue For
            End Try
            Dim Asm = Assembly.Load(AsmToken)
            Dim AsmName = Asm.GetName
            If AsmName.Name.Equals("Eddy.Interfaces", StringComparison.OrdinalIgnoreCase) Then Continue For
            If AsmName.Name.Equals("Eddy.Base", StringComparison.OrdinalIgnoreCase) Then Continue For
            If PluginDescriptorSet.Contains(AsmName.Name) Then Continue For
            Dim p = New PluginDescriptor With {.AssemblyName = AsmName.FullName, .Enable = True}
#If CONFIG <> "Debug" Then
            Try
#End If
            If LoadPlugin(Asm) Then
                NewPlugins.Add(p)
            End If
#If CONFIG <> "Debug" Then
            Catch ex As Exception
                ExceptionHandler.PopupException(ex)
                If MessageDialog.Show("{0}加载出错，程序将关闭。下次启动是否禁用该插件？".Formats(p.AssemblyName), ApplicationData.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
                    p.Enable = False
                    NewPlugins.Add(p)
                    ApplicationData.CurrentProject.Plugins = ApplicationData.CurrentProject.Plugins.Concat(NewPlugins).ToArray
                    Save()
                End If
                Application.Exit()
                Return
            End Try
#End If
        Next
        ApplicationData.CurrentProject.Plugins = ApplicationData.CurrentProject.Plugins.Concat(NewPlugins).ToArray

        For Each Plugin In ApplicationData.Plugins
            Dim DataPlugin = TryCast(Plugin, ITextLocalizerDataPlugin)
            DataPlugin.InitializeData(ApplicationData)
        Next

        For Each FormatPlugin In ApplicationData.FormatPlugins
            ApplicationData.Factory.AddFactories(FormatPlugin.GetTextListFactories())
        Next

        ApplicationData.TextNames.AddRange(ApplicationData.Columns(ApplicationData.MainColumnIndex).Keys)
        ApplicationData.TextNames.Sort(StringComparer.CurrentCultureIgnoreCase)
        ApplicationData.TextNameDict = ApplicationData.TextNames.Select(Function(s, i) New With {.Index = i, .Name = s}).ToDictionary(Function(p) p.Name, Function(p) p.Index)
    End Sub

    Private Shared BlockRedirectBinding As Boolean = False
    Private Shared Function RedirectBinding(ByVal sender As Object, ByVal e As ResolveEventArgs) As Assembly
        If BlockRedirectBinding Then Return Nothing
        BlockRedirectBinding = True
        Try
            Dim AsmName As New AssemblyName(e.Name)
            AsmName.Version = Nothing
            Dim SimpleName = AsmName.Name
            '不要使用Assembly.Load以外的其他LoadFrom、LoadFile等方法，否则会因为动态生成的一些泛型类型的程序集出现类型不匹配
            Dim Asm As Assembly
            Try
                Asm = Assembly.Load(AssemblyName.GetAssemblyName(GetAbsolutePath(SimpleName & ".dll", Application.StartupPath)))
            Catch
                Try
                    Asm = Assembly.Load(AssemblyName.GetAssemblyName(GetAbsolutePath(GetPath(SimpleName, SimpleName & ".dll"), Application.StartupPath)))
                Catch
                    '动态生成的Assembly会在这里无法解析
                    'ExceptionHandler.PopInfo("{0}无法解析。".Formats(e.Name))
                    Return Nothing
                End Try
            End Try
            Dim LoadedName = New AssemblyName(Asm.FullName)
            If Not (AsmName.GetPublicKeyToken Is Nothing OrElse Enumerable.SequenceEqual(LoadedName.GetPublicKeyToken, AsmName.GetPublicKeyToken)) Then Return Nothing
            Return Asm
        Finally
            BlockRedirectBinding = False
        End Try
    End Function

    Private Function LoadPlugin(ByVal Asm As Assembly) As Boolean
        Dim k = 0
        For Each Type In From t In Asm.GetTypes() Where t.GetInterfaces.Contains(GetType(ITextLocalizerPlugin))
            If Type.IsAbstract Then Continue For
            If Type.IsInterface Then Continue For
            If (From c In Type.GetConstructors Where c.GetParameters.Count = 0).Count = 0 Then Continue For

            Dim Obj As ITextLocalizerPlugin = Activator.CreateInstance(Type)
            ApplicationData.Plugins.Add(Obj)
            k += 1

            Dim TextHighlighter = TryCast(Obj, ITextLocalizerTextHighlighter)
            If TextHighlighter IsNot Nothing Then
                ApplicationData.TextHighlighters.Add(TextHighlighter)
            End If

            Dim GridTextFormatter = TryCast(Obj, ITextLocalizerGridTextFormatter)
            If GridTextFormatter IsNot Nothing Then
                ApplicationData.GridTextFormatters.Add(GridTextFormatter)
            End If

            Dim ToolStripButtonPlugin = TryCast(Obj, ITextLocalizerToolStripButtonPlugin)
            If ToolStripButtonPlugin IsNot Nothing Then
                ApplicationData.ToolStripButtonPlugins.Add(ToolStripButtonPlugin)
            End If

            Dim FormatPlugin = TryCast(Obj, ITextLocalizerFormatPlugin)
            If FormatPlugin IsNot Nothing Then
                ApplicationData.FormatPlugins.Add(FormatPlugin)
            End If

            Dim TranslatorPlugin = TryCast(Obj, ITextLocalizerTranslatorPlugin)
            If TranslatorPlugin IsNot Nothing Then
                ApplicationData.TranslatorPlugins.Add(TranslatorPlugin)
            End If
        Next
        Return k <> 0
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If ApplicationData.CurrentProject IsNot Nothing Then
            Save()
            ApplicationData.CurrentProject = Nothing
        End If

        ApplicationData.TextHighlighters.Clear()
        ApplicationData.GridTextFormatters.Clear()
        ApplicationData.ToolStripButtonPlugins.Clear()
        ApplicationData.FormatPlugins.Clear()
        ApplicationData.TranslatorPlugins.Clear()

        For Each p In ApplicationData.Plugins
            Try
                p.Dispose()
            Catch
            End Try
        Next
        ApplicationData.Plugins.Clear()
    End Sub
End Class
