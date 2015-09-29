' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports CodeParam = System.CodeDom.CodeParameterDeclarationExpression
Imports CodeParamPair = System.Collections.Generic.KeyValuePair(Of System.CodeDom.CodeParameterDeclarationExpression, System.CodeDom.CodeParameterDeclarationExpression)

Namespace Transform

    ''' <summary>
    ''' Used to make several marshalling decisions about.  
    ''' 
    ''' Make sure to resolve typedefs and namedtypes before calling this function.  This
    ''' code will not attempt to descend typedefs and named types  
    ''' </summary>
    ''' <remarks></remarks>

    Friend Class MarshalTransform

        Private _kind As TransformKindFlags
        Private _trans As CodeTransform
        Private _list As New List(Of TransformPlugin)

        Friend Property Kind() As TransformKindFlags
            Get
                Return _kind
            End Get
            Set(ByVal value As TransformKindFlags)
                _kind = value
            End Set
        End Property

        Friend Sub New(ByVal lang As LanguageType, ByVal kind As TransformKindFlags)
            _trans = New CodeTransform(lang)
            _kind = kind

            ' Method Parameters
            _list.Add(New BooleanTypesTransformPlugin)

            ' Process BSTR types before any other string.  BSTR can techinally be used as other String types
            ' such as LPWSTR and the other string matching code will flag them as such.  Therefore we will 
            ' process them first since the reverse is not true
            _list.Add(New BstrTransformPlugin)
            _list.Add(New MutableStringBufferTransformPlugin)
            _list.Add(New ConstantStringTransformPlugin)
            _list.Add(New ArrayParameterTransformPlugin(_trans))
            _list.Add(New BetterManagedTypesTransformPlugin)
            _list.Add(New PointerToKnownTypeTransformPlugin(_trans))
            _list.Add(New SystemIntTransformPlugin)
            _list.Add(New RawStringTransformPlugin)

            ' Very low on the list as it's a last ditch effort
            _list.Add(New DoublePointerOutTransformPlugin)
            _list.Add(New PointerPointerTransformPlugin)
            _list.Add(New DirectionalModifiersTransformPlugin)

            ' Struct Member
            _list.Add(New StringBufferStructMemberTransformPlugin)
            _list.Add(New StringPointerStructMemberTransformPlugin)
            _list.Add(New BoolStructMemberTransformPlugin)

            ' Union Members
            _list.Add(New BoolUnionMemberTransformPlugin)

            ' Mainly wrapper generators
            _list.Add(New OneWayStringBufferTransformPlugin)
            _list.Add(New TwoWayStringBufferTransformPlugin)
            _list.Add(New TwoWayViaReturnStringBufferTransformPlugin)
            _list.Add(New PInvokePointerTransformPlugin)

            For Each cur As TransformPlugin In _list
                cur.LanguageType = lang
            Next
        End Sub

        ''' <summary>
        ''' Run all of the marshaling hueristiscs on the type and it's members
        ''' </summary>
        ''' <param name="ctd"></param>
        ''' <remarks></remarks>
        Friend Sub Process(ByVal ctd As CodeTypeDeclaration)

            ' First check and see if it is a delegate type, if so run the delegate hueristics
            Dim ctdDel As CodeTypeDelegate = TryCast(ctd, CodeTypeDelegate)
            If ctdDel IsNot Nothing AndAlso ctdDel.UserData.Contains(TransformConstants.DefinedType) Then
                ProcessDelegate(ctdDel)
                Return
            End If

            ' Now run the hueristics over the actual members of the type
            If ctd.UserData.Contains(TransformConstants.DefinedType) Then
                Dim nt As NativeDefinedType = TryCast(ctd.UserData(TransformConstants.DefinedType), NativeDefinedType)
                If nt IsNot Nothing Then
                    Select Case nt.Kind
                        Case NativeSymbolKind.StructType
                            ProcessStruct(ctd)
                        Case NativeSymbolKind.UnionType
                            RunPluginUnionMembers(ctd)
                        Case NativeSymbolKind.EnumType
                            RunPluginEnumMembers(ctd)
                    End Select
                End If
            End If

            ' Now process the methods on the type.  First step is to convert all of them into 
            ' best PInvoke signature.  Then create wrapper methods for them
            Dim col As New CodeTypeMemberCollection(ctd.Members)
            Dim list As New List(Of CodeMemberMethod)
            For Each mem As CodeTypeMember In col

                ' Look at procedures
                Dim codeProc As CodeMemberMethod = TryCast(mem, CodeMemberMethod)
                If codeProc IsNot Nothing AndAlso codeProc.UserData.Contains(TransformConstants.Procedure) Then
                    list.Add(codeProc)
                End If
            Next

            For Each codeProc As CodeMemberMethod In list
                ProcessParameters(codeProc)
                ProcessReturnType(codeProc)
            Next

            For Each codeProc As CodeMemberMethod In list
                ProcessWrapperMethods(ctd, codeProc)
            Next
        End Sub

        Private Sub ProcessDelegate(ByVal del As CodeTypeDelegate)
            For Each plugin As TransformPlugin In _list
                If 0 <> (plugin.TransformKind And TransformKindFlags.Signature) Then
                    plugin.ProcessParameters(del)
                End If
            Next
        End Sub

        Private Sub ProcessStruct(ByVal ctd As CodeTypeDeclaration)
            ProcessStructMembers(ctd, TransformKindFlags.StructMembers)
        End Sub

        Private Sub RunPluginUnionMembers(ByVal ctd As CodeTypeDeclaration)
            If TransformKindFlags.UnionMembers <> (_kind And TransformKindFlags.UnionMembers) Then
                Return
            End If

            ' Union fields are not processed as often.  After the initial conversion all union fields are
            ' left in the raw form of IntPtr, Int and such.  Essentially all value types.  It's possible
            ' to create an alignment issue if we try and refactor these out to better types.  For instance
            ' we could create a string for an IntPtr and create an alignment issue
            For Each plugin As TransformPlugin In _list
                If 0 <> (plugin.TransformKind And TransformKindFlags.UnionMembers) Then
                    plugin.ProcessUnionMembers(ctd)
                End If
            Next
        End Sub

        Private Sub RunPluginEnumMembers(ByVal ctd As CodeTypeDeclaration)
            If TransformKindFlags.EnumMembers <> (_kind And TransformKindFlags.EnumMembers) Then
                Return
            End If

            ' Enum fields are not processed
        End Sub

        Private Sub ProcessParameters(ByVal codeProc As CodeMemberMethod)
            If TransformKindFlags.Signature <> (_kind And TransformKindFlags.Signature) Then
                Return
            End If

            For Each plugin As TransformPlugin In _list
                If 0 <> (plugin.TransformKind And TransformKindFlags.Signature) Then
                    plugin.ProcessParameters(codeProc)
                End If
            Next
        End Sub

        Private Sub ProcessReturnType(ByVal codeMethod As CodeMemberMethod)
            If TransformKindFlags.Signature <> (_kind And TransformKindFlags.Signature) Then
                Return
            End If

            For Each plugin As TransformPlugin In _list
                If 0 <> (plugin.TransformKind And TransformKindFlags.Signature) Then
                    plugin.ProcessReturnType(codeMethod)
                End If
            Next
        End Sub

        Private Sub ProcessStructMembers(ByVal ctd As CodeTypeDeclaration, ByVal kind As TransformKindFlags)
            If TransformKindFlags.StructMembers <> (_kind And TransformKindFlags.StructMembers) Then
                Return
            End If

            For Each plugin As TransformPlugin In _list
                If 0 <> (plugin.TransformKind And kind) Then
                    plugin.ProcessStructMembers(ctd)
                End If
            Next
        End Sub

        Private Sub ProcessWrapperMethods(ByVal ctd As CodeTypeDeclaration, ByVal codeMethod As CodeMemberMethod)
            If TransformKindFlags.WrapperMethods <> (_kind And TransformKindFlags.WrapperMethods) Then
                Return
            End If

            Dim list As New List(Of CodeMemberMethod)
            For Each plugin As TransformPlugin In _list
                If 0 <> (plugin.TransformKind And TransformKindFlags.WrapperMethods) Then
                    list.AddRange(plugin.ProcessWrapperMethods(codeMethod))
                End If
            Next

            ctd.Members.AddRange(list.ToArray())
        End Sub

    End Class

End Namespace
