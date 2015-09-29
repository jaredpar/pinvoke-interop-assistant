' Copyright (c) Microsoft Corporation.  All rights reserved.
Imports System.CodeDom
Imports System.Collections.Generic
Imports System.Diagnostics.CodeAnalysis
Imports System.Text.RegularExpressions
Imports PInvoke
Imports PInvoke.Parser
Imports PInvoke.Transform

Namespace Transform

    ''' <summary>
    ''' Wraps a lot of the functionality into a simple few method wrapper
    ''' </summary>
    ''' <remarks></remarks>
    Public Class BasicConverter
        Private _ns As NativeStorage
        Private _type As LanguageType
        Private _transformKind As TransformKindFlags = TransformKindFlags.All

        ''' <summary>
        ''' Native storage to use when resolving types
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property NativeStorage() As NativeStorage
            Get
                Return _ns
            End Get
            Set(ByVal value As NativeStorage)
                _ns = value
            End Set
        End Property

        ''' <summary>
        ''' Language to generate into
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property LanguageType() As LanguageType
            Get
                Return _type
            End Get
            Set(ByVal value As LanguageType)
                _type = value
            End Set
        End Property

        Public Property TransformKindFlags() As TransformKindFlags
            Get
                Return _transformKind
            End Get
            Set(ByVal value As TransformKindFlags)
                _transformKind = value
            End Set
        End Property

        Public Sub New()
            MyClass.New(LanguageType.VisualBasic, NativeStorage.DefaultInstance)
        End Sub

        Public Sub New(ByVal type As LanguageType)
            MyClass.New(type, NativeStorage.DefaultInstance)
        End Sub

        Public Sub New(ByVal type As LanguageType, ByVal ns As NativeStorage)
            _ns = ns
            _type = type
        End Sub

        Public Function ConvertToCodeDom(ByVal c As NativeConstant, ByVal ep As ErrorProvider) As CodeTypeDeclarationCollection
            Dim bag As New NativeSymbolBag(_ns)
            bag.AddConstant(c)
            Return ConvertBagToCodeDom(bag, ep)
        End Function

        Public Function ConvertToPInvokeCode(ByVal c As NativeConstant) As String
            Dim ep As New ErrorProvider()
            Dim col As CodeTypeDeclarationCollection = ConvertToCodeDom(c, ep)
            Return ConvertCodeDomToPInvokeCodeImpl(col, ep)
        End Function

        Public Function ConvertToCodeDom(ByVal typedef As NativeTypeDef, ByVal ep As ErrorProvider) As CodeTypeDeclarationCollection
            Dim bag As New NativeSymbolBag(_ns)
            bag.AddTypedef(typedef)
            Return ConvertBagToCodeDom(bag, ep)
        End Function

        Public Function ConvertToPInvokeCode(ByVal typedef As NativeTypeDef) As String
            Dim ep As New ErrorProvider()
            Dim col As CodeTypeDeclarationCollection = ConvertToCodeDom(typedef, ep)
            Return ConvertCodeDomToPInvokeCodeImpl(col, ep)
        End Function

        Public Function ConvertToCodeDom(ByVal definedNt As NativeDefinedType, ByVal ep As ErrorProvider) As CodeTypeDeclarationCollection
            Dim bag As New NativeSymbolBag(_ns)
            bag.AddDefinedType(definedNt)
            Return ConvertBagToCodeDom(bag, ep)
        End Function

        Public Function ConvertToPInvokeCode(ByVal definedNt As NativeDefinedType) As String
            Dim ep As New ErrorProvider()
            Dim col As CodeTypeDeclarationCollection = ConvertToCodeDom(definedNt, ep)
            Return ConvertCodeDomToPInvokeCodeImpl(col, ep)
        End Function

        Public Function ConvertToCodeDom(ByVal proc As NativeProcedure, ByVal ep As ErrorProvider) As CodeTypeDeclarationCollection
            Dim bag As New NativeSymbolBag(_ns)
            bag.AddProcedure(proc)
            Return ConvertBagToCodeDom(bag, ep)
        End Function

        Public Function ConvertToPInvokeCode(ByVal proc As NativeProcedure) As String
            Dim ep As New ErrorProvider()
            Dim col As CodeTypeDeclarationCollection = ConvertToCodeDom(proc, ep)
            Return ConvertCodeDomToPInvokeCodeImpl(col, ep)
        End Function

        Public Function ConvertToCodeDom(ByVal bag As NativeSymbolBag, ByVal ep As ErrorProvider) As CodeTypeDeclarationCollection
            Return ConvertBagToCodeDom(bag, ep)
        End Function

        Public Function ConvertToPInvokeCode(ByVal bag As NativeSymbolBag) As String
            Dim ep As New ErrorProvider()
            Return ConvertBagToPInvokeCodeImpl(bag, ep)
        End Function

        ''' <summary>
        ''' Convert the block of Native code into PInvoke code
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ConvertNativeCodeToPInvokeCode(ByVal code As String) As String
            If code Is Nothing Then : Throw New ArgumentNullException("code") : End If

            Dim ep As New ErrorProvider()
            Dim col As CodeTypeDeclarationCollection = ConvertNativeCodeToCodeDom(code, ep)
            Return ConvertCodeDomToPInvokeCodeImpl(col, ep)
        End Function

        ''' <summary>
        ''' Convert the block of native code into a CodeDom hierarchy
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="ep"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ConvertNativeCodeToCodeDom(ByVal code As String, ByVal ep As ErrorProvider) As CodeTypeDeclarationCollection
            If code Is Nothing Then : Throw New ArgumentNullException("code") : End If
            If ep Is Nothing Then : Throw New ArgumentNullException("ep") : End If

            Dim analyzer As NativeCodeAnalyzer = NativeCodeAnalyzerFactory.CreateForMiniParse(OsVersion.WindowsVista, _ns.LoadAllMacros())
            Dim bag As NativeSymbolBag
            Using reader As New IO.StringReader(code)
                Dim result As NativeCodeAnalyzerResult = analyzer.Analyze(reader)

                ep.Append(result.ErrorProvider)
                bag = NativeSymbolBag.CreateFrom(result, _ns)
            End Using

            Return ConvertBagToCodeDom(bag, ep)
        End Function

        Private Function ConvertBagToPInvokeCodeImpl(ByVal bag As NativeSymbolBag, ByVal ep As ErrorProvider) As String
            Dim col As CodeTypeDeclarationCollection = ConvertBagToCodeDom(bag, ep)
            Return ConvertCodeDomToPInvokeCodeImpl(col, ep)
        End Function

        Public Function ConvertCodeDomToPInvokeCode(ByVal ctd As CodeTypeDeclaration) As String
            Dim col As New CodeTypeDeclarationCollection()
            col.Add(ctd)
            Return ConvertCodeDomToPInvokeCodeImpl(col, New ErrorProvider())
        End Function

        Public Function ConvertCodeDomToPInvokeCode(ByVal col As CodeTypeDeclarationCollection) As String
            Return ConvertCodeDomToPInvokeCodeImpl(col, New ErrorProvider())
        End Function

        Public Function ConvertCodeDomToPInvokeCode(ByVal col As CodeTypeDeclarationCollection, ByVal ep As ErrorProvider) As String
            Return ConvertCodeDomToPInvokeCodeImpl(col, ep)
        End Function

        <SuppressMessage("Microsoft.Security", "CA2122")> _
        Private Function ConvertCodeDomToPInvokeCodeImpl(ByVal col As CodeTypeDeclarationCollection, ByVal ep As ErrorProvider) As String
            ThrowIfNull(col)
            ThrowIfNull(ep)

            Dim writer As New IO.StringWriter
            Dim provider As CodeDom.Compiler.CodeDomProvider
            Dim commentStart As String

            ' Generate based on the language
            Select Case _type
                Case Transform.LanguageType.VisualBasic
                    commentStart = "'"
                    provider = New Microsoft.VisualBasic.VBCodeProvider()
                Case Transform.LanguageType.CSharp
                    commentStart = "//"
                    provider = New Microsoft.CSharp.CSharpCodeProvider()
                Case Else
                    InvalidEnumValue(_type)
                    Return String.Empty
            End Select

            For Each warning As String In ep.Warnings
                writer.WriteLine("{0} Warning: {1}", commentStart, warning)
            Next

            For Each err As String In ep.Errors
                writer.WriteLine("{0} Error: {1}", commentStart, err)
            Next

            For Each ctd As CodeTypeDeclaration In col
                provider.GenerateCodeFromMember(ctd, writer, New Compiler.CodeGeneratorOptions())
            Next

            If _type = Transform.LanguageType.CSharp Then
                ' CSharp specific fixup
                Return FixupCSharpCode(writer.ToString())
            Else
                Return writer.ToString()
            End If

        End Function

        ''' <summary>
        ''' Core conversion routine.  All code should just go through this 
        ''' </summary>
        ''' <param name="bag"></param>
        ''' <param name="ep"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ConvertBagToCodeDom(ByVal bag As NativeSymbolBag, ByVal ep As ErrorProvider) As CodeTypeDeclarationCollection
            ThrowIfNull(bag)
            ThrowIfNull(ep)

            ' Make sure than all of the referenced NativeDefinedType instances are in the correct
            ' portion of the bag
            ChaseReferencedDefinedTypes(bag)

            ' First step is to resolve the symbols
            bag.TryResolveSymbolsAndValues(ep)

            ' Create the codedom transform
            Dim transform As New CodeTransform(Me._type)
            Dim marshalUtil As New MarshalTransform(Me._type, _transformKind)
            Dim col As New CodeTypeDeclarationCollection()

            ' Only output the constants if there are actually any
            Dim list As New List(Of NativeConstant)(bag.FindResolvedConstants())
            If list.Count > 0 Then
                Dim constCtd As CodeTypeDeclaration = transform.GenerateConstants(list)
                If constCtd.Members.Count > 0 Then
                    col.Add(constCtd)
                End If
            End If

            For Each definedNt As NativeDefinedType In bag.FindResolvedDefinedTypes()
                Dim ctd As CodeTypeDeclaration = transform.GenerateDeclaration(definedNt)
                marshalUtil.Process(ctd)
                col.Add(ctd)
            Next

            Dim procList As New List(Of NativeProcedure)(bag.FindResolvedProcedures())
            If procList.Count > 0 Then
                Dim procType As CodeTypeDeclaration = transform.GenerateProcedures(procList)
                marshalUtil.Process(procType)
                col.Add(procType)
            End If

            ' Add the helper types that we need
            AddHelperTypes(col)

            ' Next step is to run the pretty lister on it
            Dim prettyLister As New CodeDomPrettyList(bag)
            prettyLister.PerformRename(col)

            Return col
        End Function

        ''' <summary>
        ''' Make sure that any NativeDefinedType referenced is in the bag.  That way if we 
        ''' have structures which point to other NativeDefinedType instances, they are automagically
        ''' put into the bag 
        ''' </summary>
        ''' <param name="bag"></param>
        ''' <remarks></remarks>
        Private Sub ChaseReferencedDefinedTypes(ByVal bag As NativeSymbolBag)
            bag.TryResolveSymbolsAndValues()

            For Each sym As NativeSymbol In bag.FindAllReachableNativeSymbols()
                If NativeSymbolCategory.Defined = sym.Category Then
                    Dim defined As NativeDefinedType = Nothing
                    If Not bag.TryFindDefinedType(sym.Name, defined) Then
                        bag.AddDefinedType(DirectCast(sym, NativeDefinedType))
                    End If
                End If
            Next

        End Sub

        ''' <summary>
        ''' The CodeDom cannot directly output CSharp PInvoke code because it does not support the 
        ''' extern keyword which is how CSHarp defines it's PInvoke code headers.  We need to fixup
        ''' the method signatures for the PInvoke methods here 
        ''' 
        ''' We have to be careful though to avoid wrapper methods
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function FixupCSharpCode(ByVal code As String) As String
            Dim builder As New Text.StringBuilder()
            Using reader As New IO.StringReader(code)
                Dim line As String = reader.ReadLine()
                While line IsNot Nothing

                    ' Look for the DLLImport line
                    If Regex.IsMatch(line, "^\s*\[System\.Runtime\.InteropServices\.DllImport.*$") Then
                        builder.AppendLine(line)

                        ' Process the signature line by line
                        Dim sigBuilder As New Text.StringBuilder
                        Do
                            line = reader.ReadLine()
                            If line Is Nothing Then
                                builder.Append(sigBuilder)
                                Exit While
                            End If

                            Dim match As Match = Regex.Match(line, "^\s*public\s+static(.*)$")
                            If match.Success Then
                                line = "public static extern " & match.Groups(1).Value
                            End If

                            match = Regex.Match(line, "(.*){\s*$")
                            If match.Success Then
                                line = match.Groups(1).Value & ";"
                            End If

                            If Regex.IsMatch(line, "\s*}\s*") Then
                                Exit Do
                            End If
                            sigBuilder.AppendLine(line)
                        Loop

                        builder.AppendLine(sigBuilder.ToString())
                    Else
                        builder.AppendLine(line)
                    End If

                    line = reader.ReadLine()
                End While
            End Using

            Return builder.ToString()
        End Function

        ''' <summary>
        ''' Add any of the helper types that we need
        ''' </summary>
        ''' <param name="col"></param>
        ''' <remarks></remarks>
        Private Sub AddHelperTypes(ByVal col As CodeTypeDeclarationCollection)
            Dim addPInvokePointer As Boolean = False
            Dim it As New CodeDomIterator()
            Dim list As List(Of Object) = it.Iterate(col)
            For Each obj As Object In list
                Dim ctdRef As CodeTypeReference = TryCast(obj, CodeTypeReference)
                If ctdRef IsNot Nothing AndAlso 0 = String.CompareOrdinal(ctdRef.BaseType, MarshalTypeFactory.PInvokePointerTypeName) Then
                    addPInvokePointer = True
                End If
            Next

            If addPInvokePointer Then
                col.Add(MarshalTypeFactory.CreatePInvokePointerType())
            End If
        End Sub

    End Class

End Namespace
