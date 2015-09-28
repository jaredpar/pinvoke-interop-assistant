/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * **********************************************************************************/

using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SignatureGenerator
{
    [Serializable]
    public class StructureNativeType : DefinedNativeType
    {
        #region Fields

        private bool byDefaultInOnly;

        #endregion

        #region Properties

        public override bool MarshalsAsPointerWithKnownDirection
        {
            get
            {
                // will return true for byval classes as well
                return (base.MarshalsAsPointerWithKnownDirection || byDefaultInOnly);
            }
        }

        public override bool MarshalsOut
        {
            get
            {
                if (descMarshalsOut) return true;
                return (!byDefaultInOnly && !descMarshalsIn);
            }
        }

        #endregion

        #region Construction

        public StructureNativeType(NativeTypeDesc desc)
            : base(desc)
        {
            if (desc.Type.IsValueType || desc.IsStructField)
            {
                // the only allowed UnmanagedType is Struct
                ValidateUnmanagedType(desc, MarshalType.Struct);
            }
            else
            {
                // the only allowed UnmanagedType is LPStruct
                ValidateUnmanagedType(desc, MarshalType.Class);

                // if it's not byref, then it goes in-only by default
                if (++this.indirections == 1) byDefaultInOnly = true;
            }

            if (desc.MarshalAs != null)
            {
                // warn against explicit MarshalAsAttribute on a normalized structure
                Type marshal_type = Utility.GetNormalizedType(desc.Type);
                if (marshal_type != desc.Type)
                {
                    Log.Add(Errors.WARN_NormalizedStructure, desc.Type.FullName, marshal_type.FullName);
                }
            }

            if (!Utility.HasLayout(desc.Type))
            {
                Log.Add(Errors.ERROR_TypeHasNoLayout, desc.Type.FullName);
            }

            StructureDefinition struct_def = StructureDefinition.Get(desc);

            if (this.indirections == 1 && desc.PointerIndirections == 0)
            {
                // either byval class or byref struct
                ExplainPinningOrCopying(desc, struct_def.IsBlittable);
            }
            else if (this.indirections == 2 && desc.PointerIndirections == 0)
            {
                // byref class - double indirected
                ExplainMemoryManagement(desc,
                    struct_def.IsUnion ? Resources._Union : Resources._Structure);
            }

            this.typeDefinition = struct_def;
            this.nameModifier = (struct_def.IsUnion ? UnionModifier : StructModifier);
        }

        #endregion

        #region Properties



        #endregion

        #region Hint logging

        private void ExplainPinningOrCopying(NativeTypeDesc desc, bool isBlittable)
        {
            if (!desc.IsCallbackParam)
            {
                if (isBlittable)
                {
                    // blittable parameter will be pinned
                    Log.Add(Errors.INFO_LayoutBlittableParameter, desc.Type.FullName);

                    WarnBlittableInOutMismatch(desc);
                }
                else
                {
                    // non-blittable parameter will be copied
                    Log.Add(Errors.INFO_LayoutNonBlittableParameter, desc.Type.FullName);

                    if ((desc.MarshalsIn && !desc.MarshalsOut) ||
                        (byDefaultInOnly && !desc.MarshalsIn && !desc.MarshalsOut))
                    {
                        // managed -> native
                        Log.Add(Errors.INFO_LayoutDirectionManagedToNative, desc.Type.FullName);
                        isConstPointer = true;
                    }
                    else if (!desc.MarshalsIn && desc.MarshalsOut)
                    {
                        // managed <- native
                        Log.Add(Errors.INFO_LayoutDirectionNativeToManaged, desc.Type.FullName);
                    }
                    else
                    {
                        // managed <-> native
                        Log.Add(Errors.INFO_LayoutDirectionBoth, desc.Type.FullName);
                    }
                }
            }
            else
            {
                // this is a callback so everything is reversed
                if (isBlittable)
                {
                    // blittable parameter means managed directly accesses unmanaged
                    Log.Add(Errors.INFO_LayoutBlittableCallbackParameter, desc.Type.FullName);

                    WarnBlittableInOutMismatch(desc);
                }
                else
                {
                    // non-blittable parameter will be copied
                    Log.Add(Errors.INFO_LayoutNonBlittableCallbackParameter, desc.Type.FullName);

                    if ((desc.MarshalsIn && !desc.MarshalsOut) ||
                        (byDefaultInOnly && !desc.MarshalsIn && !desc.MarshalsOut))
                    {
                        // managed <- native
                        Log.Add(Errors.INFO_LayoutDirectionNativeToManaged, desc.Type.FullName);
                        isConstPointer = true;
                    }
                    else if (!desc.MarshalsIn && desc.MarshalsOut)
                    {
                        // managed -> native
                        Log.Add(Errors.INFO_LayoutDirectionManagedToNative, desc.Type.FullName);
                    }
                    else
                    {
                        // managed <-> native
                        Log.Add(Errors.INFO_LayoutDirectionBoth, desc.Type.FullName);
                    }
                }
            }
        }

        private void WarnBlittableInOutMismatch(NativeTypeDesc desc)
        {
            // the argument will always be passed in-out
            if (desc.MarshalsIn && !desc.MarshalsOut)
            {
                Log.Add(Errors.WARN_LayoutBlittableMarkedIn, desc.Type.FullName);
            }
            else if (!desc.MarshalsIn && desc.MarshalsOut)
            {
                Log.Add(Errors.WARN_LayoutBlittableMarkedOut, desc.Type.FullName);
            }
        }

        #endregion
    }

    [Serializable]
    public class StructureDefinition : NativeTypeDefinition
    {
        #region PrintContext

        private sealed class PrintContext
        {
            #region Fields

            private readonly ICodePrinter printer;
            private readonly ILogPrinter logPrinter;
            private readonly PrintFlags flags;
            private int paddingId;
            private int currentPack;
            private bool usedNonDefaultPack;

            #endregion

            #region Properties

            public ICodePrinter Printer
            {
                get
                { return printer; }
            }

            public ILogPrinter LogPrinter
            {
                get
                { return logPrinter; }
            }

            public PrintFlags Flags
            {
                get
                { return flags; }
            }

            #endregion

            #region Construction

            public PrintContext(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
            {
                Debug.Assert(printer != null && logPrinter != null);

                this.printer = printer;
                this.logPrinter = logPrinter;
                this.flags = flags;
            }

            #endregion

            #region PrintPadding

            public void PrintPadding(int paddingSize, bool avoidLargerTypes)
            {
                Debug.Assert(paddingSize >= 0);

                if (paddingSize > 0)
                {
                    TypeName padding_type;

                    printer.PrintLn();

                    if (avoidLargerTypes)
                    {
                        padding_type = TypeName.I1;
                    }
                    else
                    {
                        switch (paddingSize)
                        {
                            case 2: padding_type = TypeName.I2; paddingSize = 1; break;
                            case 4: padding_type = TypeName.I4; paddingSize = 1; break;
                            case 8: padding_type = TypeName.I8; paddingSize = 1; break;
                            default:
                            {
                                padding_type = TypeName.I1;
                                break;
                            }
                        }
                    }

                    // {padding_type} _unused{paddingId}[{paddingSize}];
                    new PrimitiveNativeType(padding_type, false).PrintTo(printer, logPrinter, flags);

                    printer.Print(OutputType.Other, " ");
                    printer.Print(OutputType.Identifier, String.Format("_unused{0}", paddingId));

                    if (paddingSize > 1)
                    {
                        printer.Print(OutputType.Operator, "[");
                        printer.Print(OutputType.Literal, paddingSize.ToString());
                        printer.Print(OutputType.Operator, "]");
                    }

                    printer.Print(OutputType.Operator, ";");

                    paddingId++;
                }
            }

            public void PrintPadding(int paddingSize)
            {
                PrintPadding(paddingSize, false);
            }

            #endregion

            #region UsedNonDefaultPack, SetPack, SetDefaultPack

            public bool UsedNonDefaultPack
            {
                get
                { return usedNonDefaultPack; }
            }

            public bool SetPack(int newPack, bool noNewLine)
            {
                Debug.Assert(newPack > 0);

                if (newPack != currentPack)
                {
                    if (newPack != DefaultPack) usedNonDefaultPack = true;

                    if (!noNewLine) printer.PrintLn();
                    if (currentPack == 0)
                    {
                        // need to push current pack to be restored in the end
                        printer.PrintLn(OutputType.Keyword, "#pragma pack (push)");
                    }
                    printer.Print(OutputType.Keyword, String.Format("#pragma pack ({0})", newPack));
                    currentPack = newPack;

                    return true;
                }

                return false;
            }

            public bool SetPack(int newPack)
            {
                return SetPack(newPack, false);
            }

            public bool SetDefaultPack()
            {
                if (currentPack != 0)
                {
                    printer.PrintLn();
                    printer.Print(OutputType.Keyword, "#pragma pack (pop)");

                    currentPack = 0;
                    return true;
                }
                return false;
            }

            #endregion
        }

        #endregion

        #region Comparers

        private sealed class ExplicitFieldComparer : IComparer<KeyValuePair<int, NativeField>>
        {
            public static readonly ExplicitFieldComparer Instance = new ExplicitFieldComparer();

            #region IComparer<KeyValuePair<int,NativeField>> Members

            public int Compare(KeyValuePair<int, NativeField> p1, KeyValuePair<int, NativeField> p2)
            {
                int offset1 = p1.Value.Offset ?? 0;
                int offset2 = p2.Value.Offset ?? 0;

                if (offset1 != offset2) return (offset1 - offset2);
                else
                {
                    // let's be deterministic
                    return p1.Key.CompareTo(p2.Key);
                }
            }

            #endregion
        }

        private sealed class SequentialFieldComparer : IComparer<KeyValuePair<int, NativeField>>
        {
            public static readonly SequentialFieldComparer Instance = new SequentialFieldComparer();

            #region IComparer<KeyValuePair<int,NativeField>> Members

            public int Compare(KeyValuePair<int, NativeField> p1, KeyValuePair<int, NativeField> p2)
            {
                return p1.Key.CompareTo(p2.Key);
            }

            #endregion
        }

        #endregion

        #region ForwardDeclaration

        [Serializable]
        class ForwardDeclaration : NativeTypeDefinition
        {
            private StructureDefinition definition;

            public ForwardDeclaration(StructureDefinition definition)
            {
                Debug.Assert(definition != null);
                this.definition = definition;
            }

            protected override string MessageLogPrefix
            {
                get { return definition.MessageLogPrefix; }
            }

            public override string Name
            {
                get { return definition.Name; }
            }

            public override int Size
            {
                get { return definition.Size; }
            }

            public override void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
            {
                // empty
            }

            public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
            {
                base.PrintTo(printer, logPrinter, flags);

                printer.Print(OutputType.Keyword, (definition.isUnion ? "union" : "struct"));
                printer.Print(OutputType.Other, " ");
                printer.Print(OutputType.Identifier, Name);
                printer.Print(OutputType.Operator, ";");
            }
        }

        #endregion

        #region Fields

        private string name;
        private bool isUnion;
        private bool isInvalid;
        private bool isBlittable;
        private bool isExplicitLayout;

        private int pack;
        private int size;
        private int maxFieldAlignmentReq;
        private bool unalignedSizeOrOffsets;

        private const int DefaultPack = 8; // true for all platforms?
        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private NativeField[] fields;

        #endregion

        #region Properties

        protected override string MessageLogPrefix
        {
            get
            {
                if (isUnion)
                    return String.Format(Resources.Union, name);
                else
                    return String.Format(Resources.Structure, name);
            }
        }

        public bool IsBlittable
        {
            get { return isBlittable; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override int Size
        {
            get
            { return size; }
        }

        public override int Alignment
        {
	        get { return maxFieldAlignmentReq; }
        }

        public int FieldCount
        {
            get
            { return fields.Length; }
        }

        public bool IsUnion
        {
            get
            { return isUnion; }
        }

        #endregion

        #region Construction

        public static StructureDefinition Get(NativeTypeDesc desc)
        {
            return NativeTypeDefinition.Get<StructureDefinition>(
                new TypeDefKey(desc.Type, (desc.Flags & MarshalFlags.TypeDefKeyFlags)));
        }

        public StructureDefinition()
        { }

        protected override void Initialize(TypeDefKey key)
        {
            Type type = key.Type;

            Debug.Assert(type.IsValueType || Utility.HasLayout(type));

            this.name = Utility.GetNameOfType(type);
            this.isBlittable = Utility.IsStructBlittable(type, 
                (key.Flags & MarshalFlags.AnsiPlatform) == MarshalFlags.AnsiPlatform);

            // reflect the structure
            FieldInfo[] fis = type.GetFields(bindingFlags);

            this.fields = new NativeField[fis.Length];
            KeyValuePair<int, NativeField>[] fields_with_tokens = new KeyValuePair<int, NativeField>[fis.Length];

            MarshalFlags flags = key.Flags & ~(MarshalFlags.AnsiStrings | MarshalFlags.UnicodeStrings);
            flags |= Utility.GetCharSetMarshalFlag(type);

            for (int i = 0; i < fis.Length; i++)
            {
                NativeField nf = NativeField.FromClrField(fis[i], flags);
                this.fields[i] = nf;

                // check for misaligned reference type fields
                // (can only be misaligned if layout was specified explicitly)
                if (nf.Offset.HasValue && nf.ContainsManagedReference)
                {
                    int ptr_size = TypeName.GetPointerSize((key.Flags & MarshalFlags.Platform64Bit) == MarshalFlags.Platform64Bit);
                    if (nf.Offset.Value % ptr_size != 0)
                    {
                        Log.Add(Errors.ERROR_MisalignedReferenceTypeField, nf.Name);
                    }
                }

                if (nf.Type.TypeSize == 0)
                {
                    // this means that the field type is another structure whose size has not been set up
                    // yet -> there are circular dependencies among structures
                    this.isInvalid = true;

                    Log.Add(Errors.ERROR_RecursiveStructureDeclaration, nf.Name);
                }
                
                if (type.IsExplicitLayout && !nf.Offset.HasValue)
                {
                    Log.Add(Errors.ERROR_NoFieldOffsetInSequentialLayout, nf.Name);
                }

                fields_with_tokens[i] = new KeyValuePair<int, NativeField>(fis[i].MetadataToken, nf);
            }

            // sort fields to reflect the layout
            if (type.IsExplicitLayout)
            {
                this.isExplicitLayout = true;

                // explicit layout - sort according to offsets
                Array.Sort<KeyValuePair<int, NativeField>, NativeField>(
                    fields_with_tokens,
                    this.fields,
                    ExplicitFieldComparer.Instance);

                // managed references overlapping with other fields are not checked here - such errors are
                // reported by the loader and assemblies containing these types are never loaded
            }
            else
            {
                // sequential layout - sort according to metadata tokens
                Array.Sort<KeyValuePair<int, NativeField>, NativeField>(
                    fields_with_tokens,
                    this.fields,
                    SequentialFieldComparer.Instance);
            }

            SetupSizeAndAlignment(type);
            SetupIsUnionFlag();
        }

        /// <summary>
        /// Sets up the <see cref="size"/> and <see cref="pack"/> fields.
        /// </summary>
        private void SetupSizeAndAlignment(Type type)
        {
            // setup alignment
            if (type.StructLayoutAttribute != null)
            {
                this.pack = type.StructLayoutAttribute.Pack;

                switch (this.pack)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 4:
                    case 8:
                    case 16:
                    case 32:
                    case 64:
                    case 128: break;

                    default:
                    {
                        Log.Add(Errors.ERROR_UnsupportedAlignment, this.pack);
                        this.pack = 0;
                        break;
                    }
                }
            }
            if (this.pack == 0)
            {
                this.pack = DefaultPack;
            }
            else
            {
                if (this.pack != DefaultPack &&
                    type.StructLayoutAttribute.Value != LayoutKind.Sequential)
                {
                    Log.Add(Errors.WARN_NoPackEffectOnExplicitLayout);
                }
            }

            int minimum_size;

            // determine the size of the structure based on sizes/offsets of the fields and the given alignment
            if (fields.Length > 0)
            {
                this.size = 0;

                for (int i = 0; i < fields.Length; i++)
                {
                    NativeField nf = fields[i];

                    if (isExplicitLayout)
                    {
                        AlignField(nf, ref this.size, false);

                        int end = (nf.Offset ?? this.size) + nf.Type.TypeSize;
                        if (end > this.size) this.size = end;
                    }
                    else
                    {
                        AlignField(nf, ref this.size, true);
                        nf.SetOffset(this.size);

                        this.size += nf.Type.TypeSize;
                    }
                }

                minimum_size = this.size;
                AlignSelf(ref this.size);

                // may only happen when structures are recursively nested
                if (this.size == 0) this.size = 1;
            }
            else
            {
                this.size = 1;
                minimum_size = this.size;
            }

            // add extra padding if size is specified explicitly
            if (type.StructLayoutAttribute != null)
            {
                int explicit_size = type.StructLayoutAttribute.Size;
                if (explicit_size < 0)
                {
                    Log.Add(Errors.ERROR_InvalidUnmanagedSize, explicit_size);
                }
                else if (explicit_size > 0)
                {
                    if (explicit_size < minimum_size)
                    {
                        Log.Add(Errors.WARN_InsufficientUnmanagedSize, explicit_size, minimum_size);
                        explicit_size = minimum_size;
                    }

                    this.size = explicit_size;

                    AlignSelf(ref explicit_size);
                    if (explicit_size != this.size)
                        this.unalignedSizeOrOffsets = true;
                }
            }
        }

        /// <summary>
        /// Sets up the <see cref="isUnion"/> and <see cref="unalignedSizeOrOffsets"/>
        /// </summary>
        private void SetupIsUnionFlag()
        {
            if (isExplicitLayout && fields.Length > 0)
            {
                NativeField nf = fields[0];
                if (!nf.Offset.HasValue || nf.Offset.Value > 0)
                {
                    // not having the first field at offset 0 disqualifies us from being a union
                    isUnion = false;
                    return;
                }

                int union_end;
                int end_index = ComputeLastUnitedField(0, out union_end);
                if (end_index != fields.Length - 1)
                {
                    // not grouping all fields into a union disqualifies us from being a union
                    isUnion = false;

                    // go through all fields now to set the unalignedSizeOrOffsets flag
                    do
                    {
                        end_index = ComputeLastUnitedField(end_index + 1, out union_end);
                    }
                    while (end_index != fields.Length - 1);

                    return;
                }

                if (union_end != size)
                {
                    // not having the union as big as the desired size may be a problem
                    isUnion = false;
                    return;
                }

                isUnion = true;
            }
            else
                isUnion = false;
        }

        private void AlignField(NativeField field, ref int offset, bool updateOffset)
        {
            // - alignment-requirement of a struct field is the smaller of the declared packsize and the
            //   largest of the alignment-requirement of its fields
            // - alignment-requirement of a scalar field is the smaller of its size and the declared packsize
            int pack_requirement = Math.Min(field.Type.AlignmentRequirement, this.pack);

            // update the max field alignment requirement of this structure
            if (pack_requirement > this.maxFieldAlignmentReq)
            {
                this.maxFieldAlignmentReq = pack_requirement;
            }

            // advance the offset to satisfy the pack requirement
            if (updateOffset && pack_requirement > 0)
            {
                int mod = (offset % pack_requirement);
                if (mod > 0)
                    offset += (pack_requirement - mod);

                Debug.Assert(offset % pack_requirement == 0);
            }
        }

        private void AlignSelf(ref int offset)
        {
            if (fields.Length == 0)
            {
                // danger, this is not idempotent
                offset++;
            }
            else
            {
                // alignment-requirement of a struct field is the smaller of the declared packsize and the
                // largest of the alignment-requirement of its fields
                int pack_requirement = Math.Min(this.maxFieldAlignmentReq, this.pack);

                // advance the offset to satisfy the pack requirement
                if (pack_requirement > 0)
                {
                    int mod = (offset % pack_requirement);
                    if (mod > 0)
                        offset += (pack_requirement - mod);

                    Debug.Assert(offset % pack_requirement == 0);
                }
            }
        }

        #endregion

        #region ICodePrintable Members

        public override void PrintTo(ICodePrinter printer, ILogPrinter logPrinter, PrintFlags flags)
        {
            Debug.Assert(printer != null && logPrinter != null);

            base.PrintTo(printer, logPrinter, flags);

            PrintContext context = new PrintContext(printer, logPrinter, flags);

            if (unalignedSizeOrOffsets)
            {
                // the size of the structure or field offsets is "odd" -> need to pragma pack (1) it
                context.SetPack(1, true);
            }
            else
                context.SetPack(DefaultPack, true);

            if (!isUnion)
            {
                printer.PrintLn();

                printer.Print(OutputType.Keyword, "struct");
                PrintIdentifierAndSize(context);

                printer.Print(OutputType.Operator, "{");

                printer.Indent();
            }

            try
            {
                if (!isInvalid)
                {
                    int current_offset = 0;

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (isExplicitLayout)
                        {
                            // this may in fact print more fields in a union, so i is passed byref
                            PrintExplicitlyLaidOutField(context, ref i, ref current_offset);
                        }
                        else
                        {
                            PrintSequentiallyLaidOutField(context, i, ref current_offset);
                        }
                    }

                    int tmp_offset = current_offset;
                    if (!context.UsedNonDefaultPack)
                    {
                        // if we never used a pack different from the default (8), we are sure
                        // about the implicit padding at the end of the structure
                        AlignSelf(ref tmp_offset);
                    }

                    if (size != tmp_offset)
                    {
                        // add final padding to the end of the structure to make its size exactly as requested
                        context.PrintPadding(size - current_offset, context.UsedNonDefaultPack);
                    }
                }
            }
            finally
            {
                if (!isUnion)
                {
                    printer.Unindent();
                    printer.PrintLn();

                    printer.Print(OutputType.Operator, "};");
                }

                context.SetDefaultPack();
            }
        }

        private void PrintExplicitlyLaidOutField(PrintContext context, ref int index, ref int currentOffset)
        {
            Debug.Assert(isExplicitLayout);

            NativeField nf = fields[index];

            int start = (nf.Offset ?? currentOffset);
            Debug.Assert(start >= currentOffset); // otherwise we would have printed it before

            bool pack1 = false;

            if (start > currentOffset)
            {
                if (IsPragmaPack1Needed(start))
                {
                    context.SetPack(1);
                    pack1 = true;
                }

                // there is a gap between the end of last field and start of this field
                context.PrintPadding(start - currentOffset);

                currentOffset = start;
            }

            if (!pack1 && !unalignedSizeOrOffsets)
                context.SetPack(DefaultPack);

            // determine the fields that will have to be grouped in a union
            int union_end;
            int last_united_index = ComputeLastUnitedField(index, out union_end);

            if (last_united_index > index)
            {
                context.Printer.PrintLn();
                context.Printer.Print(OutputType.Keyword, "union");

                if (isUnion) PrintIdentifierAndSize(context);
                else context.Printer.PrintLn();

                context.Printer.Print(OutputType.Operator, "{");

                context.Printer.Indent();
                try
                {
                    for (int j = index; j <= last_united_index; j++)
                    {
                        // the union field will be the field itself or an anonymous
                        // structure if a padding is needed to reach the field offset
                        PrintUnionField(context, j, currentOffset);
                    }
                }
                finally
                {
                    context.Printer.Unindent();

                    context.Printer.PrintLn();
                    context.Printer.Print(OutputType.Operator, "};");
                }
            }
            else
            {
                // no union is needed
                context.Printer.PrintLn();
                nf.PrintTo(context.Printer, context.LogPrinter, context.Flags);
            }

            currentOffset = union_end;
            index = last_united_index;
        }

        private void PrintSequentiallyLaidOutField(PrintContext context, int index, ref int currentOffset)
        {
            Debug.Assert(!isExplicitLayout);

            // sequential layout - no unions should be needed; however if the size
            // is "odd", we are in pragma pack (1) and the packing will be simulated
            // by our artificial padding fields
            if (!unalignedSizeOrOffsets) context.SetPack(this.pack);

            NativeField nf = fields[index];

            int previous_offset = currentOffset;
            AlignField(nf, ref currentOffset, true);

            if (unalignedSizeOrOffsets)
            {
                // inject artificial padding
                context.PrintPadding(currentOffset - previous_offset);
            }

            context.Printer.PrintLn();
            nf.PrintTo(context.Printer, context.LogPrinter, context.Flags);

            currentOffset += nf.Type.TypeSize;
        }

        private void PrintIdentifierAndSize(PrintContext context)
        {
            context.Printer.Print(OutputType.Other, " ");
            context.Printer.Print(OutputType.Identifier, name);

            context.Printer.Print(OutputType.Other, " ");
            context.Printer.PrintLn(OutputType.Comment, String.Format("// size = {0} bytes", size));
        }

        /// <summary>
        /// Determines whether we need to print pragma pack when we're padding up to the given offset.
        /// </summary>
        private bool IsPragmaPack1Needed(int offset, int @base)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                NativeField nf = fields[i];
                if (nf.Offset.HasValue && nf.Offset.Value == offset)
                {
                    int tmp_offset = offset - @base;
                    AlignField(nf, ref tmp_offset, true);

                    if (tmp_offset != (offset - @base))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether we need to print pragma pack when we're padding up to the given offset.
        /// </summary>
        private bool IsPragmaPack1Needed(int offset)
        {
            return IsPragmaPack1Needed(offset, 0);
        }

        private void PrintUnionField(PrintContext context, int fieldIndex, int unionOffset)
        {
            NativeField nf = fields[fieldIndex];

            if (nf.Offset.Value == unionOffset)
            {
                // no padding necessary - print the field
                context.Printer.PrintLn();
                nf.PrintTo(context.Printer, context.LogPrinter, context.Flags);
            }
            else
            {
                // this field will be wrapped in a struct with a padding before it
                if (IsPragmaPack1Needed(nf.Offset.Value, unionOffset))
                    context.SetPack(1);

                context.Printer.PrintLn();

                context.Printer.PrintLn(OutputType.Keyword, "struct");
                context.Printer.Print(OutputType.Operator, "{");

                context.Printer.Indent();
                try
                {
                    // add padding
                    context.PrintPadding(nf.Offset.Value - unionOffset);
                    context.Printer.PrintLn();

                    // add the field itself
                    nf.PrintTo(context.Printer, context.LogPrinter, context.Flags);
                }
                finally
                {
                    context.Printer.Unindent();

                    context.Printer.PrintLn();
                    context.Printer.Print(OutputType.Operator, "};");
                }
            }
        }

        private int ComputeLastUnitedField(int firstIndex, out int unionEnd)
        {
            Debug.Assert(isExplicitLayout && firstIndex >= 0 && firstIndex < fields.Length);

            NativeField nf = fields[firstIndex];

            // are there more fields?
            if (firstIndex == fields.Length - 1 || !nf.Offset.HasValue)
            {
                unionEnd = (nf.Offset ?? 0) + nf.Type.TypeSize;
                return firstIndex;
            }

            int union_start = nf.Offset.Value;
            unionEnd = union_start + nf.Type.TypeSize;

            int last_index = firstIndex;
            int previous_last_index, previous_union_end;

            do
            {
                previous_last_index = last_index;
                previous_union_end = unionEnd;

                for (int i = firstIndex + 1; i < fields.Length; i++)
                {
                    nf = fields[i];
                    
                    // we need offsets of all fields
                    if (!nf.Offset.HasValue) return last_index;

                    if (nf.Offset.Value >= unionEnd)
                    {
                        if (!unalignedSizeOrOffsets)
                        {
                            // this fields looks like it should be placed after the union
                            // however, if it turns out that under standard alignment rules
                            // the actual offset would be higher, we will switch to pack=1
                            int temp_union_end = unionEnd;
                            AlignField(nf, ref temp_union_end, true);

                            if (nf.Offset.Value < temp_union_end)
                            {
                                // the standard alignment results in too much padding so we
                                // fall back to pack=1 and space it manually
                                unalignedSizeOrOffsets = true;
                            }
                            else if (nf.Offset.Value == temp_union_end && i == last_index + 1)
                            {
                                // let's inflate the union without including the next field
                                // the padding will come implicitly as we are under certain pack
                                // and will eliminate superfluous "noop" artificial padding
                                // 
                                // (the purpose of this is to detect cases when the layout is
                                // explicit but in fact results in the same offsets that the
                                // sequential layout would generate)
                                unionEnd = temp_union_end;
                            }
                        }
                    }
                    else
                    {
                        // this field clearly belongs to the union
                        last_index = i;
                        unionEnd = Math.Max(unionEnd, nf.Offset.Value + nf.Type.TypeSize);
                    }
                }
            }
            while (last_index != previous_last_index || unionEnd != previous_union_end);

            // we found a fixed point: last_index denotes the last field that should be
            // included in the union and unionEnd is the offset just after the union
            return last_index;
        }

        #endregion

        #region Definition Enumeration

        public override void GetDefinitionsRecursive(NativeTypeDefinitionSet set, NativeTypeDefinition parentDef)
        {
            foreach (NativeField nf in fields)
            {
                DefinedNativeType def_type = nf.Type as DefinedNativeType;
                if (def_type != null)
                {
                    if (!set.Contains(def_type.Definition))
                    {
                        set.Add(def_type.Definition);

                        def_type.Definition.GetDefinitionsRecursive(set, def_type.Definition);
                    }
                    if (parentDef != null) set.AddDependency(parentDef, def_type.Definition);
                }
            }
        }

        public override NativeTypeDefinition GetForwardDeclaration()
        {
            return new ForwardDeclaration(this);
        }

        public NativeField GetField(int fieldIndex)
        {
            return fields[fieldIndex];
        }

        #endregion
    }
}
