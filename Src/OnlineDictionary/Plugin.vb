'==========================================================================
'
'  File:        Plugin.vb
'  Location:    Eddy.Voice <Visual Basic .Net>
'  Description: 文本本地化工具在线词典插件
'  Version:     2010.12.13.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Diagnostics
Imports System.Threading
Imports System.Threading.Tasks
Imports System.ComponentModel
Imports System.Net
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Setting
Imports Eddy.Interfaces

Public Class Config
    Public Dictionaries As OnlineDictionaryDescriptor()
End Class

Public Class OnlineDictionaryDescriptor
    Public Name As String
    Public UrlTemplate As String
    Public Encoding As String
    Public IconUrl As String
End Class

Public Class Plugin
    Inherits TextLocalizerBase
    Implements ITextLocalizerToolStripButtonPlugin

    Private SettingPath As String = "OnlineDictionary.locplugin"
    Private Config As Config

    Public Sub New()
        If File.Exists(SettingPath) Then
            Config = Xml.ReadFile(Of Config)(SettingPath)
        Else
            Config = New Config With {.Dictionaries = New OnlineDictionaryDescriptor() {
                New OnlineDictionaryDescriptor With {.Name = "金山词霸..", .UrlTemplate = "http://www.iciba.com/index.php?s=%s", .Encoding = "UTF-8"},
                New OnlineDictionaryDescriptor With {.Name = "Yahoo!辞書..", .UrlTemplate = "http://dic.search.yahoo.co.jp/search?p=%s&ei=UTF-8", .Encoding = "UTF-8"},
                New OnlineDictionaryDescriptor With {.Name = "沪江小D..", .UrlTemplate = "http://dict.hjenglish.com/app/jp/jc/%s", .Encoding = "UTF-8"},
                New OnlineDictionaryDescriptor With {.Name = "Babylon..", .UrlTemplate = "http://info.babylon.com/cgi-bin/info.cgi?word=%s&lang=0&type=undefined", .Encoding = "UTF-8", .IconUrl = "http://www.babylon.com"}
            }}
        End If
    End Sub
    Protected Overrides Sub DisposeManagedResource()
        Try
            Xml.WriteFile(SettingPath, UTF16, Config)
        Catch
        End Try
        MyBase.DisposeManagedResource()
    End Sub

    Private Function GetEncoding(ByVal NameOrCodePage As String) As Encoding
        Dim CodePage As Integer
        If Integer.TryParse(NameOrCodePage, CodePage) Then
            Return Encoding.GetEncoding(CodePage)
        Else
            Return Encoding.GetEncoding(NameOrCodePage)
        End If
    End Function
    Public Function GetToolStripButtonDescriptors() As IEnumerable(Of ToolStripButtonDescriptor) Implements ITextLocalizerToolStripButtonPlugin.GetToolStripButtonDescriptors
        Dim l As New List(Of ToolStripButtonDescriptor)

        For Each d In Config.Dictionaries
            Dim t = d.UrlTemplate
            Dim i = t
            If d.IconUrl <> "" Then i = d.IconUrl
            Dim e = GetEncoding(d.Encoding)
            Dim bd = New ToolStripButtonDescriptor With {.Image = My.Resources.Dictionary, .Text = d.Name, .Click =
                Sub() ToolStripButton_Click(t, e)
            }
            GetIconAsync(i, bd, Controller.UIThreadInvoker)
            l.Add(bd)
        Next

        Return l
    End Function

    Private Sub ToolStripButton_Click(ByVal UrlTemplate As String, ByVal Encoding As Encoding)
        Dim SelectionStart = Controller.SelectionStart
        Dim SelectionLength = Controller.SelectionLength
        If SelectionLength = 0 Then Return
        Dim ColumnIndex = Controller.ColumnIndex
        Dim tp = Columns(ColumnIndex)
        Dim Text = Controller.Text(ColumnIndex).Substring(SelectionStart, SelectionLength)
        Dim TextBytes = Encoding.GetBytes(Text)
        Dim Url = UrlTemplate.Replace("%s", String.Join("", TextBytes.Select(Function(b) "%" & b.ToString("X2"))))
        Process.Start(Url)
    End Sub

    Private Shared rHost As New Regex("^http://(?<host>.*?)(/.*)?$", RegexOptions.ExplicitCapture)
    Private Sub GetIconAsync(ByVal IconUrl As String, ByVal bd As ToolStripButtonDescriptor, ByVal UIThreadInvoker As Action(Of Action))
        Dim m = rHost.Match(IconUrl)
        If Not m.Success Then Return
        Dim Host = m.Result("${host}")
        Dim Url = "http://www.google.com/s2/favicons?domain={0}".Formats(Host)

        Dim i As Image = Nothing
        Dim GetIcon =
            Sub()
                Try
                    Dim Request = HttpWebRequest.Create(Url)
                    Request.Timeout = 10000
                    Dim Response = Request.GetResponse()
                    If String.Equals(Response.ContentType, "image/png", StringComparison.OrdinalIgnoreCase) Then
                        Using s = Response.GetResponseStream
                            Dim Bytes = New Byte(Response.ContentLength - 1) {}
                            s.Read(Bytes, 0, Bytes.Length)
                            Using ms As New MemoryStream(Bytes)
                                i = Image.FromStream(ms)
                            End Using
                        End Using
                    End If
                Catch
                End Try

                Dim F =
                    Sub()
                        bd.ImageChanged.Raise(i)
                    End Sub

                UIThreadInvoker(F)
            End Sub

        Dim Task As New Task(GetIcon)
        Task.Start()
    End Sub
End Class
