Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Diagnostics
Imports System.Reflection
Imports System.Reflection.Emit
Imports Firefly
Imports Firefly.Streaming
Imports Firefly.Mapping

Public Class Ipc
    Private Sub New()
    End Sub

    Private Shared Function CreateMasterType(Of T As IDisposable)(ByVal SlavePath As String) As Type
        Dim ServiceInterfaceType = GetType(T)
        If Not ServiceInterfaceType.IsInterface Then Throw New ArgumentException("服务类型必须为接口")
        If ServiceInterfaceType.IsGenericTypeDefinition Then Throw New ArgumentException("服务类型必须是具体类型，泛型类型应先具体化")

        Dim an As New AssemblyName("IpcDynamicAssembly")
        Dim ab = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect)
        Dim mob = ab.DefineDynamicModule(an.Name)
        Dim tb = mob.DefineType("IpcServiceProxy", TypeAttributes.Public Or TypeAttributes.Class, GetType(Object), {GetType(T)})

        Dim fbIpcMaster = tb.DefineField("IpcMaster", GetType(PipeMaster), FieldAttributes.Private)

        Dim cb = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, {GetType(PipeMaster)})
        Dim cig = cb.GetILGenerator
        cig.Emit(OpCodes.Ldarg_0)
        cig.Emit(OpCodes.Ldarg_1)
        cig.Emit(OpCodes.Stfld, fbIpcMaster)


        For Each m In ServiceInterfaceType.GetMethods
            Dim Parameters = m.GetParameters()
            Dim mb = tb.DefineMethod(m.Name, m.Attributes, m.CallingConvention, m.ReturnType, Parameters.Select(Function(p) p.ParameterType).ToArray)
            If m.IsGenericMethodDefinition Then
                Dim GenericParameters = m.GetGenericArguments()
                Dim gpbs = mb.DefineGenericParameters(GenericParameters.Select(Function(gp) gp.Name).ToArray())
                For Each Pair In gpbs.Zip(GenericParameters, Function(b, gp) New With {.GenericParameter = gp, .Builder = b})
                    Dim Constraints = Pair.GenericParameter.GetGenericParameterConstraints()
                    Pair.Builder.SetGenericParameterAttributes(Pair.GenericParameter.GenericParameterAttributes)
                    Dim InterfaceConstraints = Constraints.Where(Function(c) c.IsInterface).ToArray()
                    Dim BaseConstraints = Constraints.Except(InterfaceConstraints).ToArray()
                    If BaseConstraints.Length > 0 Then
                        Pair.Builder.SetBaseTypeConstraint(BaseConstraints.Single)
                    End If
                    If InterfaceConstraints.Length > 0 Then
                        Pair.Builder.SetInterfaceConstraints(InterfaceConstraints)
                    End If
                Next
            End If


        Next
    End Function

    Public Shared Function CreateMaster(Of T As IDisposable)(ByVal SlavePath As String) As T





    End Function

    Public Shared Sub ListenOnSlave(Of T As IDisposable)(ByVal PipeIn As String, ByVal PipeOut As String, ByVal ConcreteService As T)

    End Sub
End Class
