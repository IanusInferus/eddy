Imports System
Imports System.IO
Imports System.IO.Pipes
Imports System.Diagnostics
Imports System.Net
Imports Firefly
Imports Firefly.Streaming

Public NotInheritable Class PipeMaster
    Implements IPipe

    Private PipeOut As AnonymousPipeServerStream
    Private PipeIn As AnonymousPipeServerStream
    Private Slave As Process
    Public Sub New(ByVal SlavePath As String)
        Dim Success As Boolean = False
        Try
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

    Public Sub Send(ByVal p As Packet) Implements IPipe.Send
        Dim Pipe = PipeOut.AsWritable()
        Pipe.WriteInt32(p.Verb)
        Pipe.WriteInt32(p.Content.Length)
        Pipe.Write(p.Content)
        Pipe.Flush()
    End Sub

    Public Function Receive() As Packet Implements IPipe.Receive
        Dim Pipe = PipeIn.AsReadable
        Dim Verb = CType(Pipe.ReadInt32, IpcVerb)
        Dim Length = Pipe.ReadInt32
        Dim Content = New Byte(Length - 1) {}

        Dim Offset As Int32 = 0
        While Offset < Length
            Dim Count = PipeIn.Read(Content, Offset, Length - Offset)
            If Count = 0 Then Throw New ProtocolViolationException
            Offset += Count
        End While
        If Offset <> Length Then Throw New ProtocolViolationException

        Return New Packet With {.Verb = Verb, .Content = Content}
    End Function
End Class

Public NotInheritable Class PipeSlave
    Implements IPipe

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

    Public Sub Send(ByVal p As Packet) Implements IPipe.Send
        Dim Pipe = PipeOut.AsWritable()
        Pipe.WriteInt32(p.Verb)
        Pipe.WriteInt32(p.Content.Length)
        Pipe.Write(p.Content)
        Pipe.Flush()
    End Sub

    Public Function Receive() As Packet Implements IPipe.Receive
        Dim Pipe = PipeIn.AsReadable
        Dim Verb = CType(Pipe.ReadInt32, IpcVerb)
        Dim Length = Pipe.ReadInt32
        Dim Content = New Byte(Length - 1) {}

        Dim Offset As Int32 = 0
        While Offset < Length
            Dim Count = PipeIn.Read(Content, Offset, Length - Offset)
            If Count = 0 Then Throw New ProtocolViolationException
            Offset += Count
        End While
        If Offset <> Length Then Throw New ProtocolViolationException

        Return New Packet With {.Verb = Verb, .Content = Content}
    End Function
End Class
