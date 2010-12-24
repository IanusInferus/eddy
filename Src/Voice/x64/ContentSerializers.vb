﻿Imports System
Imports Firefly
Imports Firefly.Streaming

Public NotInheritable Class ContentPacker
    Implements IDisposable
    Implements IParameterWriter

    Private s As ISerializer
    Private BaseStream As IStream

    Public Sub New(ByVal s As ISerializer)
        Me.s = s
        Me.BaseStream = StreamEx.Create()
    End Sub

    Public Sub WriteParameter(Of T)(ByVal Parameter As T) Implements IParameterWriter.WriteParameter
        s.Write(Parameter, s)
    End Sub

    Public Function Build() As Byte()
        BaseStream.Position = 0
        Dim Bytes = BaseStream.Read(BaseStream.Length)
        BaseStream.Dispose()
        BaseStream = Nothing
        s = Nothing
        Return Bytes
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If BaseStream IsNot Nothing Then
            BaseStream.Dispose()
            BaseStream = Nothing
        End If
        s = Nothing
    End Sub
End Class

Public Class ContentUnpacker
    Implements IDisposable
    Implements IParameterReader

    Private s As ISerializer
    Private BaseStream As IStream

    Public Sub New(ByVal Content As Byte(), ByVal s As ISerializer)
        Me.s = s
        Me.BaseStream = StreamEx.Create()
        Me.BaseStream.Write(Content)
        Me.BaseStream.Position = 0
    End Sub

    Public Function ReadParameter(Of T)() As T Implements IParameterReader.ReadParameter
        Return s.Read(Of T)(BaseStream)
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If BaseStream IsNot Nothing Then
            BaseStream.Dispose()
            BaseStream = Nothing
        End If
        s = Nothing
    End Sub
End Class
