'==========================================================================
'
'  File:        Rpc.vb
'  Location:    Eddy.Voice <Visual Basic .Net>
'  Description: 远程过程调用代理
'  Version:     2010.12.27.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Diagnostics
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Threading
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Streaming
Imports Firefly.Mapping
Imports Eddy
Imports Eddy.Base

Public Class Rpc
    Private Sub New()
    End Sub

    Public Shared Function CreateMaster(Of T As IDisposable)(ByVal SlavePath As String, ByVal s As ISerializer, ByVal MainThreadAsyncInvoker As Action(Of Action)) As T
        Dim Result = CreateMasterType(Of T)()
        Dim ProxyType = Result.MasterType
        Dim MethodToExecuteField = Result.MethodToExecuteField
        Dim EventToRaiserMethod = Result.EventToRaiserMethod

        Dim Proxy = DirectCast(Activator.CreateInstance(ProxyType), T)
        Dim ServiceProxy = DirectCast(DirectCast(Proxy, Object), _ServiceProxy)

        Dim Pipe As PipeMaster = Nothing
        Dim Master As RpcExecutorMaster = Nothing
        Dim Success = False
        Try
            Pipe = New PipeMaster(SlavePath)

            Dim DummyMethod = DirectCast(AddressOf CreateMasterEventParameterReceiverResolver(Of T, T), Func(Of T, Dictionary(Of EventInfo, MethodInfo), Func(Of EventInfo, Action(Of Integer, IParameterReader)))).Method
            Dim m = DummyMethod.GetGenericMethodDefinition().MakeGenericMethod(GetType(T), ProxyType)
            Dim EventParameterReceiverResolver = DirectCast(m.Invoke(Nothing, New Object() {Proxy, EventToRaiserMethod}), Func(Of EventInfo, Action(Of Integer, IParameterReader)))

            Master = New RpcExecutorMaster(Pipe, New SerializerAdapter(s), GetType(T), EventParameterReceiverResolver, MainThreadAsyncInvoker)
            Success = True
        Finally
            If Not Success Then
                If Pipe IsNot Nothing Then
                    Pipe.Dispose()
                    Pipe = Nothing
                End If
                If Master IsNot Nothing Then
                    Master.Dispose()
                    Master = Nothing
                End If
            End If
        End Try

        ServiceProxy.Pipe = Pipe
        ServiceProxy.Master = Master

        For Each Pair In MethodToExecuteField
            Dim mi = Pair.Key
            Dim fi = Pair.Value

            fi.SetValue(Proxy, CreateMasterMethodHandler(Master, mi))
        Next

        Return Proxy
    End Function
    Public Shared Function CreateMaster(Of T As IDisposable)(ByVal SlavePath As String, ByVal MainThreadAsyncInvoker As Action(Of Action)) As T
        Dim s As New BinarySerializer
        s.PutReaderTranslator(New StringTranslator)
        s.PutWriterTranslator(New StringTranslator)

        Return CreateMaster(Of T)(SlavePath, New SerializerAdapter(s), MainThreadAsyncInvoker)
    End Function

    Public Shared Sub ListenOnSlave(Of T As IDisposable, C As T)(ByVal PipeIn As String, ByVal PipeOut As String, ByVal s As ISerializer, ByVal ConcreteService As C)
        Using EventPump As New ManualResetEvent(False)
            Dim MainThreadEventLoop =
                Sub()
                    EventPump.WaitOne(100)
                    EventPump.Reset()
                End Sub

            Dim MethodParameterReceiverResolver = CreateSlaveMethodParameterReceiverResolver(Of T, C)(ConcreteService)

            Using Pipe As New PipeSlave(PipeIn, PipeOut)
                Dim RpcSlave As New RpcExecutorSlave(Pipe, s, GetType(T), MethodParameterReceiverResolver, MainThreadEventLoop)

                For Each ei In GetType(T).GetEvents()
                    ei.AddEventHandler(ConcreteService, CreateSlaveEventHandler(RpcSlave, ei))
                Next

                RpcSlave.Listen()
            End Using
        End Using
    End Sub
    Public Shared Sub ListenOnSlave(Of T As IDisposable, C As T)(ByVal PipeIn As String, ByVal PipeOut As String, ByVal ConcreteService As C)
        Dim s As New BinarySerializer
        s.PutReaderTranslator(New StringTranslator)
        s.PutWriterTranslator(New StringTranslator)

        ListenOnSlave(Of T, C)(PipeIn, PipeOut, New SerializerAdapter(s), ConcreteService)
    End Sub

    Public Class _ServiceProxy
        Implements IDisposable

        Public Pipe As PipeMaster
        Public Master As RpcExecutorMaster

        Public Sub Dispose() Implements IDisposable.Dispose
            If Pipe IsNot Nothing Then
                Pipe.Dispose()
                Pipe = Nothing
            End If
            If Master IsNot Nothing Then
                Master.Dispose()
                Master = Nothing
            End If
        End Sub
    End Class

    Private Class MasterTypeResult
        Public MasterType As Type
        Public MethodToExecuteField As Dictionary(Of MethodInfo, FieldInfo)
        Public EventToRaiserMethod As Dictionary(Of EventInfo, MethodInfo)
    End Class
    Private Shared Function CreateMasterType(Of T As IDisposable)() As MasterTypeResult
        Dim MethodToExecuteField As New Dictionary(Of MethodInfo, FieldInfo)
        Dim EventToRaiserMethod As New Dictionary(Of EventInfo, MethodInfo)

        Dim ServiceInterfaceType = GetType(T)
        If Not ServiceInterfaceType.IsInterface Then Throw New ArgumentException("服务类型必须为接口")
        If ServiceInterfaceType.IsGenericTypeDefinition Then Throw New ArgumentException("服务类型必须是具体类型，泛型类型应先具体化")

        Dim an As New AssemblyName("IpcDynamicAssembly")
        Dim ab = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect)
        Dim mob = ab.DefineDynamicModule(an.Name)
        Dim tb = mob.DefineType("IpcServiceProxy", TypeAttributes.Public Or TypeAttributes.Class, GetType(_ServiceProxy), {GetType(T)})

        For Each mi In ServiceInterfaceType.GetMethods.Where(Function(m) Not m.IsSpecialName)
            Dim Parameters = mi.GetParameters().Select(Function(p) p.ParameterType).ToArray()
            Dim ReturnValues = (Function(rv) IIf(Of Type())(rv Is GetType(Void), New Type() {}, New Type() {rv}))(mi.ReturnType)

            Dim FieldType As Type

            If ReturnValues.Length > 0 Then
                Dim Num = Parameters.Length + ReturnValues.Length
                FieldType = Type.GetType("System.Func`" & Num).MakeGenericType(Parameters.Concat(ReturnValues).ToArray())
            Else
                Dim Num = Parameters.Length
                If Num > 0 Then
                    FieldType = Type.GetType("System.Action`" & Num).MakeGenericType(Parameters)
                Else
                    FieldType = GetType(Action)
                End If
            End If

            Dim fb = tb.DefineField("<>_" & String.Join("_", {mi.Name}.Concat(Parameters.Select(Function(p) p.Name)).ToArray()), FieldType, FieldAttributes.Public)
            MethodToExecuteField.Add(mi, fb)

            Dim mb = tb.DefineMethod(mi.Name, mi.Attributes And Not MethodAttributes.Abstract, mi.CallingConvention, mi.ReturnType, Parameters)
            If mi.IsGenericMethodDefinition Then Throw New NotSupportedException

            Dim ig = mb.GetILGenerator()
            ig.Emit(OpCodes.Ldarg_0)
            ig.Emit(OpCodes.Ldfld, fb)
            For n = 0 To Parameters.Length - 1
                ig.Emit(OpCodes.Ldarg, n + 1)
            Next
            ig.EmitCall(OpCodes.Callvirt, FieldType.GetMethod("Invoke"), Nothing)
        Next

        For Each ei In ServiceInterfaceType.GetEvents()
            Dim Parameters = ei.GetEventParameters().Select(Function(e) e.ParameterType).ToArray()

            Dim eb = tb.DefineEvent(ei.Name, ei.Attributes, ei.EventHandlerType)

            Dim am = ei.GetAddMethod()
            Dim rm = ei.GetRemoveMethod()

            tb.DefineMethod(am.Name, am.Attributes And Not MethodAttributes.Abstract, am.CallingConvention, am.ReturnType, am.GetParameters().Select(Function(p) p.ParameterType).ToArray())
            tb.DefineMethod(rm.Name, rm.Attributes And Not MethodAttributes.Abstract, rm.CallingConvention, rm.ReturnType, rm.GetParameters().Select(Function(p) p.ParameterType).ToArray())

            'Dim mb = tb.DefineMethod(ei.Name, MethodAttributes.Public, CallingConventions.HasThis, GetType(Void), Parameters)

            'Dim ig = mb.GetILGenerator()
            'ig.Emit(OpCodes.Ldarg_0)
            'ig.Emit(OpCodes.Ldfld, ei)
            'For n = 0 To Parameters.Length - 1
            '    ig.Emit(OpCodes.Ldarg, n + 1)
            'Next
            'ig.EmitCall(OpCodes.Callvirt, FieldType.GetMethod("Invoke"), Nothing)

            'MethodToExecuteField.Add(mi, fb)
        Next

        Return New MasterTypeResult With {.MasterType = tb.CreateType(), .MethodToExecuteField = MethodToExecuteField, .EventToRaiserMethod = EventToRaiserMethod}
    End Function

    Private Shared Function CreateMasterEventParameterReceiverResolver(Of T As IDisposable, C As T)(ByVal ProxyService As C, ByVal EventToRaiseMethod As Dictionary(Of EventInfo, MethodInfo)) As Func(Of EventInfo, Action(Of Integer, IParameterReader))
        Dim Type = GetType(T)
        If Not Type.IsInterface Then Throw New ArgumentException

        Dim Events = Type.GetEvents()
        Dim Dict As New Dictionary(Of EventInfo, Action(Of Integer, IParameterReader))

        For Each ei In Events
            Dim ProxyParameter = Expression.Parameter(GetType(C))

            Dim pNumParameter = Expression.Parameter(GetType(Integer), "NumParameter")
            Dim pr = Expression.Parameter(GetType(IParameterReader), "r")

            Dim Parameters = ei.GetEventParameters()

            Dim Statements As New List(Of Expression)
            Statements.Add(Expression.IfThen(Expression.NotEqual(pNumParameter, Expression.Constant(Parameters.Length)), Expression.Throw(Expression.[New](GetType(InvalidOperationException)))))

            Dim ParameterVariables = Parameters.Select(Function(p) Expression.Variable(p.ParameterType)).ToArray()
            Dim Read = GetType(IParameterReader).GetMethods.Single
            Statements.AddRange(ParameterVariables.Select(Function(pv) Expression.Assign(pv, Expression.Call(pr, Read.MakeGenericMethod(pv.Type)))).ToArray())

            Dim RealCall = Expression.Call(ProxyParameter, EventToRaiseMethod(ei), ParameterVariables)

            Statements.Add(RealCall)

            Dim Inner = Expression.Lambda(Expression.Block(ParameterVariables, Statements), pNumParameter, pr)
            Dim Outer = Expression.Lambda(Inner, ProxyParameter)
            Dim Compiled = DirectCast(Outer.Compile(), Func(Of C, Action(Of Integer, IParameterReader)))

            Dict.Add(ei, Compiled(ProxyService))
        Next

        Dim MethodParameterReceiverResolver =
            Function(ei As EventInfo) As Action(Of Integer, IParameterReader)
                If Dict.ContainsKey(ei) Then Return Dict(ei)
                Throw New NotSupportedException
            End Function

        Return MethodParameterReceiverResolver
    End Function

    Private Shared Function CreateMasterMethodHandler(ByVal Master As RpcExecutorMaster, ByVal mi As MethodInfo) As [Delegate]
        Dim MasterParameter = Expression.Parameter(GetType(RpcExecutorMaster))
        Dim miParameter = Expression.Parameter(GetType(MethodInfo))

        Dim pNumParameter = Expression.Parameter(GetType(Integer), "NumParameter")
        Dim pw = Expression.Parameter(GetType(IParameterWriter), "w")
        Dim pNumReturnValue = Expression.Parameter(GetType(Integer), "NumReturnValue")
        Dim pr = Expression.Parameter(GetType(IParameterReader), "r")

        Dim Parameters = mi.GetParameters()
        Dim ReturnValues = (Function(rv) IIf(Of Type())(rv Is GetType(Void), New Type() {}, New Type() {rv}))(mi.ReturnType)

        Dim ParameterParameters = Parameters.Select(Function(p) Expression.Parameter(p.ParameterType)).ToArray()
        Dim ReturnValueVariables = ReturnValues.Select(Function(v) Expression.Variable(v)).ToArray()

        Dim WriteStatements As New List(Of Expression)
        WriteStatements.Add(Expression.IfThen(Expression.NotEqual(pNumParameter, Expression.Constant(Parameters.Length)), Expression.Throw(Expression.[New](GetType(InvalidOperationException)))))
        Dim Write = GetType(IParameterWriter).GetMethods.Single
        WriteStatements.AddRange(ParameterParameters.Select(Function(pp) Expression.Call(pw, Write.MakeGenericMethod(pp.Type), pp)))
        Dim WriteLambda = Expression.Lambda(GetType(Action(Of Integer, IParameterWriter)), Expression.Block(WriteStatements), pNumParameter, pw)

        Dim ReadStatements As New List(Of Expression)
        ReadStatements.Add(Expression.IfThen(Expression.NotEqual(pNumReturnValue, Expression.Constant(ReturnValues.Length)), Expression.Throw(Expression.[New](GetType(InvalidOperationException)))))
        Dim Read = GetType(IParameterReader).GetMethods.Single
        ReadStatements.AddRange(ReturnValueVariables.Select(Function(pp) Expression.Assign(pp, Expression.Call(pr, Read.MakeGenericMethod(pp.Type)))))
        Dim ReadLambda = Expression.Lambda(GetType(Action(Of Integer, IParameterReader)), Expression.Block(ReadStatements), pNumReturnValue, pr)

        Dim SendRequestExecute = GetType(RpcExecutorMaster).GetMethod("SendRequestExecute")
        Dim Send = Expression.Call(MasterParameter, SendRequestExecute, miParameter, WriteLambda, ReadLambda)
        Dim InnerBody As Expression = Send
        If ReturnValues.Length > 0 Then
            InnerBody = Expression.Block({ReturnValueVariables.Single}, {Send, ReturnValueVariables.Single})
        End If

        Dim Inner = Expression.Lambda(InnerBody, ParameterParameters)
        Dim Outer = Expression.Lambda(GetType(Func(Of RpcExecutorMaster, MethodInfo, [Delegate])), Inner, MasterParameter, miParameter)

        Dim Compiled = DirectCast(Outer.Compile(), Func(Of RpcExecutorMaster, MethodInfo, [Delegate]))
        Return Compiled(Master, mi)
    End Function

    Private Shared Function CreateSlaveMethodParameterReceiverResolver(Of T As IDisposable, C As T)(ByVal ConcreteService As C) As Func(Of MethodInfo, Action(Of Integer, IParameterReader, Integer, IParameterWriter))
        Dim Type = GetType(T)
        If Not Type.IsInterface Then Throw New ArgumentException

        Dim Methods = Type.GetMethods()
        Dim Dict As New Dictionary(Of MethodInfo, Action(Of Integer, IParameterReader, Integer, IParameterWriter))

        For Each mi In Methods
            Dim ConcreteServiceParameter = Expression.Parameter(Type)

            Dim pNumParameter = Expression.Parameter(GetType(Integer), "NumParameter")
            Dim pr = Expression.Parameter(GetType(IParameterReader), "r")
            Dim pNumReturnValue = Expression.Parameter(GetType(Integer), "NumReturnValue")
            Dim pw = Expression.Parameter(GetType(IParameterWriter), "w")

            Dim Parameters = mi.GetParameters()
            Dim ReturnValues = (Function(rv) IIf(Of Type())(rv Is GetType(Void), New Type() {}, New Type() {rv}))(mi.ReturnType)

            Dim Statements As New List(Of Expression)
            Statements.Add(Expression.IfThen(Expression.NotEqual(pNumParameter, Expression.Constant(Parameters.Length)), Expression.Throw(Expression.[New](GetType(InvalidOperationException)))))
            Statements.Add(Expression.IfThen(Expression.NotEqual(pNumReturnValue, Expression.Constant(ReturnValues.Length)), Expression.Throw(Expression.[New](GetType(InvalidOperationException)))))

            Dim ParameterVariables = Parameters.Select(Function(p) Expression.Variable(p.ParameterType)).ToArray()
            Dim Read = GetType(IParameterReader).GetMethods.Single
            Statements.AddRange(ParameterVariables.Select(Function(pv) Expression.Assign(pv, Expression.Call(pr, Read.MakeGenericMethod(pv.Type)))).ToArray())

            Dim RealCall = Expression.Call(ConcreteServiceParameter, mi, ParameterVariables)

            If ReturnValues.Length = 0 Then
                Statements.Add(RealCall)
            Else
                Dim Write = GetType(IParameterWriter).GetMethods.Single
                Statements.Add(Expression.Call(pw, Write.MakeGenericMethod(ReturnValues.Single), RealCall))
            End If

            Dim Inner = Expression.Lambda(Expression.Block(ParameterVariables, Statements), pNumParameter, pr, pNumReturnValue, pw)
            Dim Outer = Expression.Lambda(Inner, ConcreteServiceParameter)
            Dim Compiled = DirectCast(Outer.Compile(), Func(Of T, Action(Of Integer, IParameterReader, Integer, IParameterWriter)))

            Dict.Add(mi, Compiled(ConcreteService))
        Next

        Dim MethodParameterReceiverResolver =
            Function(mi As MethodInfo) As Action(Of Integer, IParameterReader, Integer, IParameterWriter)
                If Dict.ContainsKey(mi) Then Return Dict(mi)
                Throw New NotSupportedException
            End Function

        Return MethodParameterReceiverResolver
    End Function

    Private Shared Function CreateSlaveEventHandler(ByVal Slave As RpcExecutorSlave, ByVal ei As EventInfo) As [Delegate]
        Dim SlaveParameter = Expression.Parameter(GetType(RpcExecutorSlave))
        Dim eiParameter = Expression.Parameter(GetType(EventInfo))

        Dim pNumParameter = Expression.Parameter(GetType(Integer), "NumParameter")
        Dim pw = Expression.Parameter(GetType(IParameterWriter), "w")

        Dim Parameters = ei.GetEventParameters()
        Dim ParameterParameters = Parameters.Select(Function(p) Expression.Parameter(p.ParameterType)).ToArray()

        Dim Statements As New List(Of Expression)
        Statements.Add(Expression.IfThen(Expression.NotEqual(pNumParameter, Expression.Constant(Parameters.Length)), Expression.Throw(Expression.[New](GetType(InvalidOperationException)))))

        Dim Write = GetType(IParameterWriter).GetMethods.Single
        Statements.AddRange(ParameterParameters.Select(Function(pp) Expression.Call(pw, Write.MakeGenericMethod(pp.Type), pp)))

        Dim LambdaParam = Expression.Lambda(Expression.Block(Statements), pNumParameter, pw)

        Dim SendRequestExecuteAsync = GetType(RpcExecutorSlave).GetMethod("SendRequestExecuteAsync")
        Dim Send = Expression.Call(SlaveParameter, SendRequestExecuteAsync, eiParameter, LambdaParam)

        Dim Inner = Expression.Lambda(ei.EventHandlerType, Send, ParameterParameters)
        Dim Outer = Expression.Lambda(GetType(Func(Of RpcExecutorSlave, EventInfo, [Delegate])), Inner, SlaveParameter, eiParameter)

        Dim Compiled = DirectCast(Outer.Compile(), Func(Of RpcExecutorSlave, EventInfo, [Delegate]))
        Return Compiled(Slave, ei)
    End Function

    Private Class StringTranslator
        Implements IProjectorToProjectorDomainTranslator(Of String, Byte())
        Implements IProjectorToProjectorRangeTranslator(Of String, Byte())

        Public Function TranslateProjectorToProjectorDomain(Of R)(ByVal Projector As Func(Of Byte(), R)) As Func(Of String, R) Implements IProjectorToProjectorDomainTranslator(Of String, Byte()).TranslateProjectorToProjectorDomain
            Return Function(s) Projector(UTF8.GetBytes(s))
        End Function
        Public Function TranslateProjectorToProjectorRange(Of D)(ByVal Projector As Func(Of D, Byte())) As Func(Of D, String) Implements IProjectorToProjectorRangeTranslator(Of String, Byte()).TranslateProjectorToProjectorRange
            Return Function(Domain) UTF8.GetString(Projector(Domain))
        End Function
    End Class

    Private Class SerializerAdapter
        Implements ISerializer

        Private bs As BinarySerializer
        Public Sub New(ByVal bs As BinarySerializer)
            Me.bs = bs
        End Sub

        Public Function Read(Of T)(ByVal s As IReadableStream) As T Implements ISerializer.Read
            Return bs.Read(Of T)(s)
        End Function

        Public Sub Write(Of T)(ByVal Value As T, ByVal s As IWritableStream) Implements ISerializer.Write
            bs.Write(Value, s)
        End Sub
    End Class
End Class
