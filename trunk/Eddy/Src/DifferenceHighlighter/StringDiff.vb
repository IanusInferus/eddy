'==========================================================================
'
'  File:        StringDiff.vb
'  Location:    Eddy.DifferenceHighlighter <Visual Basic .Net>
'  Description: 序列比较
'  Version:     2011.02.22.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.Linq

Public Class TranslatePart
    Public SourceIndex As Integer
    Public SourceLength As Integer
    Public TargetIndex As Integer
    Public TargetLength As Integer
End Class

Public NotInheritable Class StringDiff
    Private Sub New()
    End Sub

    Private Class ComparerDynamicProgramming(Of T As IEquatable(Of T))
        Private Enum DifferencePattern
            None = 0
            Del
            Add
            Same
            Replace
        End Enum

        Private Class StringDifference
            Public Pattern As DifferencePattern
            Public SourceIndex As Integer
            Public SourceLength As Integer
            Public TargetIndex As Integer
            Public TargetLength As Integer
        End Class

        Private Source As T()
        Private Target As T()
        Private n As Integer
        Private m As Integer
        Private C As Integer(,)
        Private D As DifferencePattern(,)

        Public Sub New(ByVal Source As T(), ByVal Target As T())
            Me.Source = Source
            Me.Target = Target
            n = Source.Length
            m = Target.Length
            C = New Integer(n, m) {}
            D = New Integer(n, m) {}

            For i = 0 To n
                C(i, 0) = i
            Next
            For j = 0 To m
                C(0, j) = j
            Next
        End Sub

        Private Sub CalcDifference()
            For i = 1 To n
                For j = 1 To m
                    Dim s = C(i - 1, j) + 1
                    Dim dv = DifferencePattern.Del
                    If D(i - 1, j) <> DifferencePattern.Del AndAlso D(i - 1, j) <> DifferencePattern.Replace Then
                        s += 1
                    End If

                    Dim s2 = C(i, j - 1) + 1
                    If D(i, j - 1) <> DifferencePattern.Add AndAlso D(i, j - 1) <> DifferencePattern.Replace Then
                        s2 += 1
                    End If
                    If s2 < s Then
                        s = s2
                        dv = DifferencePattern.Add
                    End If

                    If Source(i - 1).Equals(Target(j - 1)) Then
                        Dim s3 = C(i - 1, j - 1)
                        If D(i - 1, j - 1) <> DifferencePattern.Same Then
                            s3 += 1
                        End If
                        If s3 < s Then
                            s = s3
                            dv = DifferencePattern.Same
                        End If
                    Else
                        Dim s4 = C(i - 1, j - 1) + 2
                        If D(i - 1, j - 1) = DifferencePattern.Same Then
                            s4 += 1
                        End If
                        If s4 < s Then
                            s = s4
                            dv = DifferencePattern.Replace
                        End If
                    End If

                    C(i, j) = s
                    D(i, j) = dv
                Next
            Next
        End Sub

        Public Function GetDifference() As TranslatePart()
            CalcDifference()

            Dim s As New Stack(Of StringDifference)
            Dim i = n
            Dim j = m
            While True
                If i <= 0 Then
                    If j <= 0 Then
                    Else
                        s.Push(New StringDifference With {.Pattern = DifferencePattern.Add, .SourceIndex = 0, .SourceLength = 0, .TargetIndex = 0, .TargetLength = j})
                    End If
                    Exit While
                ElseIf j <= 0 Then
                    s.Push(New StringDifference With {.Pattern = DifferencePattern.Del, .SourceIndex = 0, .SourceLength = i, .TargetIndex = 0, .TargetLength = 0})
                    Exit While
                Else
                    Dim Pattern = D(i, j)
                    Select Case Pattern
                        Case DifferencePattern.Del
                            i -= 1
                            s.Push(New StringDifference With {.Pattern = DifferencePattern.Del, .SourceIndex = i, .SourceLength = 1, .TargetIndex = j, .TargetLength = 0})
                        Case DifferencePattern.Add
                            j -= 1
                            s.Push(New StringDifference With {.Pattern = DifferencePattern.Add, .SourceIndex = i, .SourceLength = 0, .TargetIndex = j, .TargetLength = 1})
                        Case DifferencePattern.Replace, DifferencePattern.Same
                            i -= 1
                            j -= 1
                            s.Push(New StringDifference With {.Pattern = Pattern, .SourceIndex = i, .SourceLength = 1, .TargetIndex = j, .TargetLength = 1})
                    End Select
                End If
            End While

            If s.Count = 0 Then Return New TranslatePart() {}

            Dim l As New List(Of StringDifference)
            Dim c0 As StringDifference = Nothing
            Dim c As StringDifference = s.Pop
            If c.Pattern = DifferencePattern.Replace Then
                l.Add(New StringDifference With {.Pattern = DifferencePattern.Del, .SourceIndex = c.SourceIndex, .SourceLength = c.SourceLength, .TargetIndex = c.TargetIndex, .TargetLength = 0})
                c = New StringDifference With {.Pattern = DifferencePattern.Add, .SourceIndex = c.SourceIndex + c.SourceLength, .SourceLength = 0, .TargetIndex = c.TargetIndex, .TargetLength = c.TargetLength}
            End If
            While True
                If c0 IsNot Nothing Then
                    If c.Pattern = c0.Pattern Then
                        c0.SourceLength += c.SourceLength
                        c0.TargetLength += c.TargetLength
                        If s.Count > 0 Then
                            c = s.Pop
                        Else
                            Exit While
                        End If
                    ElseIf c.Pattern = DifferencePattern.Replace Then
                        If c0.Pattern = DifferencePattern.Del Then
                            c0.SourceLength += c.SourceLength
                            c = New StringDifference With {.Pattern = DifferencePattern.Add, .SourceIndex = c.SourceIndex + c.SourceLength, .SourceLength = 0, .TargetIndex = c.TargetIndex, .TargetLength = c.TargetLength}
                        ElseIf c0.Pattern = DifferencePattern.Add Then
                            c0.TargetLength += c.TargetLength
                            c = New StringDifference With {.Pattern = DifferencePattern.Del, .SourceIndex = c.SourceIndex, .SourceLength = c.SourceLength, .TargetIndex = c.TargetIndex + c.TargetLength, .TargetLength = 0}
                        Else
                            l.Add(c0)
                            c0 = New StringDifference With {.Pattern = DifferencePattern.Del, .SourceIndex = c.SourceIndex, .SourceLength = c.SourceLength, .TargetIndex = c.TargetIndex, .TargetLength = 0}
                            c = New StringDifference With {.Pattern = DifferencePattern.Add, .SourceIndex = c.SourceIndex + c.SourceLength, .SourceLength = 0, .TargetIndex = c.TargetIndex, .TargetLength = c.TargetLength}
                        End If
                        l.Add(c0)
                        c0 = c
                        If s.Count > 0 Then
                            c = s.Pop
                        Else
                            Exit While
                        End If
                    Else
                        l.Add(c0)
                        c0 = c
                        If s.Count > 0 Then
                            c = s.Pop
                        Else
                            Exit While
                        End If
                    End If
                Else
                    c0 = c
                    If s.Count > 0 Then
                        c = s.Pop
                    Else
                        Exit While
                    End If
                End If
            End While
            l.Add(c0)

            Return (From p In l Select New TranslatePart With {.SourceIndex = p.SourceIndex, .SourceLength = p.SourceLength, .TargetIndex = p.TargetIndex, .TargetLength = p.TargetLength}).ToArray
        End Function
    End Class

    Private Class ComparerBreadthFirst(Of T As IEquatable(Of T))
        Private Source As T()
        Private Target As T()
        Private N As Integer
        Private M As Integer

        Public Sub New(ByVal Source As T(), ByVal Target As T())
            Me.Source = Source
            Me.Target = Target
            N = Source.Length
            M = Target.Length
        End Sub

        Private Class ListNode
            Public x As Integer
            Public y As Integer
            Public Previous As ListNode
        End Class

        Public Function GetDifference() As TranslatePart()
            Dim Success As Boolean = False
            Dim Route As ListNode = Nothing
            Dim Even As Boolean = True
            Dim Comparer = EqualityComparer(Of T).Default

            Dim xRoot As Integer = 0
            Dim yRoot As Integer = 0
            Dim hMaxRoot As Integer = 0
            While xRoot < N AndAlso yRoot < M
                If Comparer.Equals(Source(xRoot), Target(yRoot)) Then
                    xRoot += 1
                    yRoot += 1
                Else
                    Exit While
                End If
            End While

            Dim MinDeterminedSolutionCost = (N - xRoot) + (M - yRoot)
            Dim kMinDeterminedSolution = 0
            Dim k2Lx As New Dictionary(Of Integer, ListNode)
            k2Lx.Add(0, New ListNode With {.x = xRoot, .y = yRoot, .Previous = Nothing})

            If xRoot = N AndAlso yRoot = M Then
                Success = True
                Route = k2Lx(0)
            End If

            For D = 1 To N + M
                If Success Then Exit For
                Even = (D Mod 2 = 0)
                For k = -D To D Step 2
                    Dim IsRemoveReachable = k2Lx.ContainsKey(k - 1)
                    Dim IsAddReachable = k2Lx.ContainsKey(k + 1)

                    Dim Previous As ListNode
                    Dim x As Integer
                    Dim y As Integer
                    Dim hMax As Integer
                    If IsRemoveReachable AndAlso IsAddReachable Then
                        Dim RemoveReachedPrevious = k2Lx(k - 1)
                        Dim xRemove = RemoveReachedPrevious.x + 1
                        Dim yRemove = xRemove - k
                        Dim hMaxRemove = (N - xRemove) + (M - yRemove)
                        Dim AddReachedPrevious = k2Lx(k + 1)
                        Dim xAdd = AddReachedPrevious.x
                        Dim yAdd = xAdd - k
                        Dim hMaxAdd = (N - xAdd) + (M - yAdd)
                        If hMaxRemove < hMaxAdd Then
                            Previous = RemoveReachedPrevious
                            x = xRemove
                            y = yRemove
                            hMax = hMaxRemove
                        Else
                            Previous = AddReachedPrevious
                            x = xAdd
                            y = yAdd
                            hMax = hMaxAdd
                        End If
                    ElseIf IsRemoveReachable Then
                        Previous = k2Lx(k - 1)
                        x = Previous.x + 1
                        y = x - k
                        hMax = (N - x) + (M - y)
                    ElseIf IsAddReachable Then
                        Previous = k2Lx(k + 1)
                        x = Previous.x
                        y = x - k
                        hMax = (N - x) + (M - y)
                    Else
                        Continue For
                    End If

                    If x > N OrElse y > M Then
                        If k2Lx.ContainsKey(k) Then k2Lx.Remove(k)
                        Continue For
                    End If

                    Dim hMin = 1
                    If D + hMin > MinDeterminedSolutionCost AndAlso Abs(k - kMinDeterminedSolution) > 1 Then
                        If k2Lx.ContainsKey(k) Then k2Lx.Remove(k)
                        Continue For
                    End If

                    While x < N AndAlso y < M
                        If Comparer.Equals(Source(x), Target(y)) Then
                            x += 1
                            y += 1
                        Else
                            Exit While
                        End If
                    End While

                    hMax = (N - x) + (M - y)
                    If D + hMax <= MinDeterminedSolutionCost Then
                        MinDeterminedSolutionCost = D + hMax
                        kMinDeterminedSolution = k
                    End If

                    If k2Lx.ContainsKey(k) Then
                        k2Lx(k) = New ListNode With {.x = x, .y = y, .Previous = Previous}
                    Else
                        k2Lx.Add(k, New ListNode With {.x = x, .y = y, .Previous = Previous})
                    End If

                    If x = N AndAlso y = M Then
                        Success = True
                        Route = k2Lx(k)
                        Exit For
                    End If
                Next
            Next
            If Not Success Then Throw New InvalidOperationException

            Dim CurrentRouteReversed As New List(Of TranslatePart)
            Dim CurrentPart As New TranslatePart With {.SourceIndex = N, .SourceLength = 0, .TargetIndex = M, .TargetLength = 0}
            Dim CurrentNode = Route
            While CurrentNode IsNot Nothing
                Dim xDifference = CLng(CurrentPart.SourceIndex - CurrentNode.x)
                Dim yDifference = CLng(CurrentPart.TargetIndex - CurrentNode.y)
                Dim SnakeDifference = Min(xDifference, yDifference)
                If SnakeDifference > 0 Then
                    If CurrentPart.SourceLength <> 0 OrElse CurrentPart.TargetLength <> 0 Then
                        CurrentRouteReversed.Add(CurrentPart)
                    End If
                    Dim xSnake = CurrentPart.SourceIndex - SnakeDifference
                    Dim ySnake = CurrentPart.TargetIndex - SnakeDifference
                    CurrentRouteReversed.Add(New TranslatePart With {.SourceIndex = xSnake, .SourceLength = SnakeDifference, .TargetIndex = ySnake, .TargetLength = SnakeDifference})
                    CurrentPart = New TranslatePart With {.SourceIndex = CurrentNode.x, .SourceLength = xSnake - CurrentNode.x, .TargetIndex = CurrentNode.y, .TargetLength = ySnake - CurrentNode.y}
                Else
                    Dim DotProduct = xDifference * CurrentPart.TargetLength - yDifference * CurrentPart.SourceLength
                    If DotProduct <> 0 Then
                        CurrentRouteReversed.Add(CurrentPart)
                        CurrentPart = New TranslatePart With {.SourceIndex = CurrentNode.x, .SourceLength = xDifference, .TargetIndex = CurrentNode.y, .TargetLength = yDifference}
                    Else
                        CurrentPart = New TranslatePart With {.SourceIndex = CurrentNode.x, .SourceLength = CurrentPart.SourceLength + xDifference, .TargetIndex = CurrentNode.y, .TargetLength = CurrentPart.TargetLength + yDifference}
                    End If
                End If
                CurrentNode = CurrentNode.Previous
            End While
            If CurrentPart.SourceLength <> 0 OrElse CurrentPart.TargetLength <> 0 Then
                CurrentRouteReversed.Add(CurrentPart)
            End If

            If CurrentPart.SourceIndex <> 0 OrElse CurrentPart.TargetIndex <> 0 Then
                CurrentRouteReversed.Add(New TranslatePart With {.SourceIndex = 0, .SourceLength = CurrentPart.SourceIndex, .TargetIndex = 0, .TargetLength = CurrentPart.TargetIndex})
            End If

            Return Enumerable.Range(0, CurrentRouteReversed.Count).Select(Function(i) CurrentRouteReversed(CurrentRouteReversed.Count - 1 - i)).ToArray
        End Function
    End Class

    Public Shared Function Compare(Of T As IEquatable(Of T))(ByVal Source As T(), ByVal Target As T()) As TranslatePart()
        'Return (New ComparerDynamicProgramming(Of T)(Source, Target)).GetDifference()
        Return (New ComparerBreadthFirst(Of T)(Source, Target)).GetDifference()
    End Function
End Class
