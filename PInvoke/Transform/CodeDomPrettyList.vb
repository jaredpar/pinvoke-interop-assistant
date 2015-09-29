' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.Text.RegularExpressions
Imports PInvoke

Namespace Transform

    ''' <summary>
    ''' Used to pretty list entries in the code dom tree
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CodeDomPrettyList
        Private _bag As NativeSymbolBag
        Private _resolvedTypeDefList As List(Of NativeTypeDef)

        Public Sub New(ByVal bag As NativeSymbolBag)
            _bag = bag
        End Sub

        Public Sub PerformRename(ByVal col As CodeTypeDeclarationCollection)
            If col Is Nothing Then : Throw New ArgumentNullException("col") : End If

            Dim map As New Dictionary(Of String, String)(StringComparer.Ordinal)
            map.Add("tagPOINT", "Point")

            Dim it As New CodeDomIterator()
            Dim list As List(Of Object) = it.Iterate(col)

            ' Use the iterator so we make sure to reach nested types
            For Each ctd As CodeTypeDeclaration In FindUnmodifiedTypes(list)
                Dim definedNt As NativeDefinedType = GetDefined(ctd)
                If IsBadName(ctd.Name) Then
                    For Each possible As NativeTypeDef In FindTypedefsTargeting(definedNt)
                        If Not IsBadName(possible.Name) Then
                            map(ctd.Name) = possible.Name
                        End If
                    Next
                End If
            Next

            SmartTypeRename(map, list)
            ResetCustomExpressions(list)
        End Sub

        Private Function FindUnmodifiedTypes(ByVal col As List(Of Object)) As List(Of CodeTypeDeclaration)

            Dim list As New List(Of CodeTypeDeclaration)

            ' Use the iterator so we make sure to reach nested types
            For Each obj As Object In col
                Dim ctd As CodeTypeDeclaration = TryCast(obj, CodeTypeDeclaration)
                If ctd IsNot Nothing Then
                    Dim definedNt As NativeDefinedType = GetDefined(ctd)
                    If definedNt IsNot Nothing AndAlso 0 = String.CompareOrdinal(definedNt.Name, ctd.Name) Then
                        list.Add(ctd)
                    End If
                End If
            Next

            Return list
        End Function

        Private Function IsBadName(ByVal name As String) As Boolean
            If Regex.IsMatch(name, "^_\w+$") Then
                Return True
            End If

            If NativeSymbolBag.IsAnonymousName(name) Then
                Return True
            End If

            Return False
        End Function

        Private Function GetDefined(ByVal ctd As CodeTypeDeclaration) As NativeDefinedType
            ThrowIfNull(ctd)

            If ctd.UserData.Contains(TransformConstants.Type) Then
                Return DirectCast(ctd.UserData(TransformConstants.Type), NativeDefinedType)
            End If

            Return Nothing
        End Function

        Private Sub SmartTypeRename(ByVal map As Dictionary(Of String, String), ByVal col As List(Of Object))
            ThrowIfNull(map)
            ThrowIfNull(col)

            For Each obj As Object In col
                Dim ctd As CodeTypeDeclaration = TryCast(obj, CodeTypeDeclaration)
                If ctd IsNot Nothing Then
                    Dim newName As String = Nothing
                    If map.TryGetValue(ctd.Name, newName) Then
                        ctd.Name = newName
                    End If
                    Continue For
                End If

                Dim typeRef As CodeTypeReference = TryCast(obj, CodeTypeReference)
                If typeRef IsNot Nothing Then
                    Dim newName As String = Nothing
                    If map.TryGetValue(typeRef.BaseType, newName) Then
                        typeRef.BaseType = newName
                    End If
                    Continue For
                End If
            Next

        End Sub

        ''' <summary>
        ''' Instances of CodeCustomExpression are represented as CodeSnippetExpression instances in the tree.  Because
        ''' their is no good way to virtually update the Value property we are forced to recalculate it whenever there 
        ''' is a change to one of the expressions contained within the custom node. 
        ''' 
        ''' This method will force all of the custom expression instances in the tree to be updated 
        ''' </summary>
        ''' <param name="col"></param>
        ''' <remarks></remarks>
        Private Sub ResetCustomExpressions(ByVal col As List(Of Object))
            ThrowIfNull(col)

            For Each obj As Object In col
                Dim custom As CodeCustomExpression = TryCast(obj, CodeCustomExpression)
                If custom IsNot Nothing Then
                    custom.ResetValue()
                End If
            Next
        End Sub

        ''' <summary>
        ''' Find any typedegs where the passed in value is the real type
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function FindTypedefsTargeting(ByVal target As NativeType) As List(Of NativeTypeDef)
            ' Build the cache
            If _resolvedTypeDefList Is Nothing Then
                _resolvedTypeDefList = New List(Of NativeTypeDef)(_bag.FindResolvedTypedefs())
            End If

            Dim list As New List(Of NativeTypeDef)

            ' First look in the symbol bag
            For Each td As NativeTypeDef In _resolvedTypeDefList
                If Object.ReferenceEquals(td.RealTypeDigged, target) Then
                    list.Add(td)
                End If
            Next

            ' Next look in the native storage for more types  
            Dim ns As NativeStorage = _bag.NativeStorageLookup
            Dim typeRef As NativeStorage.TypeReference = ns.CreateTypeReference(target)
            If typeRef IsNot Nothing Then
                For Each trow As NativeStorage.TypedefTypeRow In ns.TypedefType.FindByTarget(typeRef)
                    Dim found As NativeTypeDef = Nothing
                    If _bag.TryFindOrLoadTypedef(trow.Name, found) Then
                        list.Add(found)
                    End If
                Next
            End If

            Return list
        End Function


    End Class

End Namespace

