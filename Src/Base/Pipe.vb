﻿'==========================================================================
'
'  File:        Pipe.vb
'  Location:    Eddy <Visual Basic .Net>
'  Description: 管道
'  Version:     2010.12.26.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.IO
Imports System.IO.Pipes
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Diagnostics
Imports System.Net
Imports Firefly
Imports Firefly.Streaming

Public NotInheritable Class PipeMaster
    Implements IMasterPipe

    Private SlavePath As String
    Private PipeOut As AnonymousPipeServerStream
    Private PipeIn As AnonymousPipeServerStream
    Private Slave As Process
    Public Sub New(ByVal SlavePath As String)
        Dim Success As Boolean = False
        Try
            Me.SlavePath = SlavePath

            PipeOut = New AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable)
            PipeIn = New AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable)

            Slave = New Process()
            Slave.StartInfo.FileName = SlavePath
            Slave.StartInfo.Arguments = PipeOut.GetClientHandleAsString() + " " + PipeIn.GetClientHandleAsString()
            Slave.StartInfo.UseShellExecute = False
            Slave.StartInfo.CreateNoWindow = True
            Slave.Start()

            PipeOut.DisposeLocalCopyOfClientHandle()
            PipeIn.DisposeLocalCopyOfClientHandle()

            Success = True
        Finally
            If Not Success Then
                Dispose()
            End If
        End Try
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        If Slave IsNot Nothing Then
            If Slave.WaitForExit(1000) Then
            Else
                Slave.Kill()
            End If
            Slave.Dispose()
            Slave = Nothing
        End If
        If PipeIn IsNot Nothing Then
            PipeIn.Dispose()
            PipeIn = Nothing
        End If
        If PipeOut IsNot Nothing Then
            PipeOut.Dispose()
            PipeOut = Nothing
        End If
    End Sub

    Public Sub Send(ByVal p As Packet) Implements IMasterPipe.Send
        'Debug.WriteLine("MasterSend: {0}".Formats(p.Verb))
        If PipeOut Is Nothing Then Throw New InvalidOperationException
        If p Is Nothing OrElse p.Content Is Nothing Then Throw New ArgumentNullException
        Dim Pipe = PipeOut.AsWritable()
        Pipe.WriteInt32(p.Verb)
        Pipe.WriteInt32(p.StackDepth)
        Pipe.WriteInt32(p.Content.Length)
        Pipe.Write(p.Content)
        Pipe.Flush()
    End Sub

    Public Function Receive() As Packet Implements IMasterPipe.Receive
        If PipeIn Is Nothing Then Throw New InvalidOperationException
        Dim Pipe = PipeIn.AsReadable
        Dim Verb = CType(Pipe.ReadInt32(), RpcVerb)
        Dim StackDepth As Int32 = Pipe.ReadInt32()
        Dim Length = Pipe.ReadInt32()
        Dim Content = New Byte(Length - 1) {}

        Dim Offset As Int32 = 0
        While Offset < Length
            Dim Count = PipeIn.Read(Content, Offset, Length - Offset)
            If Count = 0 Then Throw New ProtocolViolationException
            Offset += Count
        End While
        If Offset <> Length Then Throw New ProtocolViolationException

        'Debug.WriteLine("MasterReceived: {0}".Formats(Verb))
        Return New Packet With {.Verb = Verb, .StackDepth = StackDepth, .Content = Content}
    End Function

    Public Sub Send(ByVal p As Packet, ByVal Timeout As Integer) Implements IMasterPipe.Send
        Using Task As New Task(Sub() Send(p))
            Task.Start()

            If Task.Wait(Timeout) Then

            Else
                Slave.Kill()
                Task.Wait()
                Throw New InvalidOperationException
            End If
        End Using
    End Sub

    Public Function Receive(ByVal Timeout As Integer) As Packet Implements IMasterPipe.Receive
        Using Task As New Task(Of Packet)(AddressOf Receive)
            Task.Start()

            If Task.Wait(Timeout) Then
                Return Task.Result
            Else
                Slave.Kill()
                Task.Wait()
                Throw New InvalidOperationException
            End If
        End Using
    End Function
End Class

Public NotInheritable Class PipeSlave
    Implements ISlavePipe

    Private PipeOut As AnonymousPipeClientStream
    Private PipeIn As AnonymousPipeClientStream
    Public Sub New(ByVal PipeInHandleString As String, ByVal PipeOutHandleString As String)
        Dim Success As Boolean = False
        Try
            PipeOut = New AnonymousPipeClientStream(PipeDirection.Out, PipeOutHandleString)
            PipeIn = New AnonymousPipeClientStream(PipeDirection.In, PipeInHandleString)

            Success = True
        Finally
            If Not Success Then
                Dispose()
            End If
        End Try
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        If PipeIn IsNot Nothing Then
            PipeIn.Dispose()
            PipeIn = Nothing
        End If
        If PipeOut IsNot Nothing Then
            PipeOut.Dispose()
            PipeOut = Nothing
        End If
    End Sub

    Public Sub Send(ByVal p As Packet) Implements ISlavePipe.Send
        'Debug.WriteLine("SlaveSend: {0}".Formats(p.Verb))
        If PipeOut Is Nothing Then Throw New InvalidOperationException
        If p Is Nothing OrElse p.Content Is Nothing Then Throw New ArgumentNullException
        Dim Pipe = PipeOut.AsWritable()
        Pipe.WriteInt32(p.Verb)
        Pipe.WriteInt32(p.StackDepth)
        Pipe.WriteInt32(p.Content.Length)
        Pipe.Write(p.Content)
        Pipe.Flush()
    End Sub

    Public Function Receive() As Packet Implements ISlavePipe.Receive
        If PipeIn Is Nothing Then Throw New InvalidOperationException
        Dim Pipe = PipeIn.AsReadable
        Dim Verb = CType(Pipe.ReadInt32, RpcVerb)
        Dim StackDepth As Int32 = Pipe.ReadInt32()
        Dim Length = Pipe.ReadInt32
        Dim Content = New Byte(Length - 1) {}

        Dim Offset As Int32 = 0
        While Offset < Length
            Dim Count = PipeIn.Read(Content, Offset, Length - Offset)
            If Count = 0 Then Throw New ProtocolViolationException
            Offset += Count
        End While
        If Offset <> Length Then Throw New ProtocolViolationException

        'Debug.WriteLine("SlaveReceived: {0}".Formats(Verb))
        Return New Packet With {.Verb = Verb, .StackDepth = StackDepth, .Content = Content}
    End Function
End Class
