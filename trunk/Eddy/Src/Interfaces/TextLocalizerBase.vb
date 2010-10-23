'==========================================================================
'
'  File:        TextLocalizerBase.vb
'  Location:    Eddy.Interfaces <Visual Basic .Net>
'  Description: 文本本地化工具插件默认基类实现
'  Version:     2010.05.17.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms

''' <summary>TextLocalizer的插件的默认基类实现</summary>
Public MustInherit Class TextLocalizerBase
    Implements ITextLocalizerDataPlugin
    Implements ITextLocalizerControllerPlugin

    Protected TextLocalizerData As ITextLocalizerData
    Protected ReadOnly Property TextNames As IEnumerable(Of String)
        Get
            Return TextLocalizerData.TextNames
        End Get
    End Property
    Protected ReadOnly Property Columns As IEnumerable(Of LocalizationTextProvider)
        Get
            Return TextLocalizerData.Columns
        End Get
    End Property
    Protected ReadOnly Property MainColumnIndex As Integer
        Get
            Return TextLocalizerData.MainColumnIndex
        End Get
    End Property
    Protected ReadOnly Property NameToColumn As Dictionary(Of String, Integer)
        Get
            Static d As Dictionary(Of String, Integer)
            If d Is Nothing Then
                d = New Dictionary(Of String, Integer)
                Dim k = 0
                For Each c In Columns
                    d.Add(c.Name, k)
                    k += 1
                Next
            End If
            Return d
        End Get
    End Property

    Public Sub InitializeData(ByVal TextLocalizerData As ITextLocalizerData) Implements ITextLocalizerDataPlugin.InitializeData
        Me.TextLocalizerData = TextLocalizerData
    End Sub

    Protected WithEvents Controller As ITextLocalizerApplicationController
    Public Sub InitializeController(ByVal Controller As ITextLocalizerApplicationController) Implements ITextLocalizerControllerPlugin.InitializeController
        Me.Controller = Controller
    End Sub

    ''' <summary>释放托管对象或间接非托管对象(Stream等)。</summary>
    Protected Overridable Sub DisposeManagedResource()
    End Sub

    ''' <summary>释放直接非托管对象(Handle等)。</summary>
    Protected Overridable Sub DisposeUnmanagedResource()
    End Sub

    ''' <summary>将大型字段设置为 null。</summary>
    Protected Overridable Sub DisposeNullify()
    End Sub

    '检测冗余的调用
    Private DisposedValue As Boolean = False
    ''' <summary>释放流的资源。请优先覆盖DisposeManagedResource、DisposeUnmanagedResource、DisposeNullify方法。如果你直接保存非托管对象(Handle等)，请覆盖Finalize方法，并在其中调用Dispose(False)。</summary>
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If DisposedValue Then Return
        DisposedValue = True
        If disposing Then
            DisposeManagedResource()
        End If
        DisposeUnmanagedResource()
        DisposeNullify()
    End Sub

    ''' <summary>释放流的资源。</summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。请将清理代码放入上面的 Dispose(ByVal disposing As Boolean) 中。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
