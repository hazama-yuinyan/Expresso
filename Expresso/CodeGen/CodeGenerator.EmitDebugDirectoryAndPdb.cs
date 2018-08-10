using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Expresso.Ast;

namespace Expresso.CodeGen
{
    public partial class CodeGenerator : IAstWalker<CSharpEmitterContext, Type>
	{
        int param_index, field_index, method_index;
        int[] method_offsets;
        MetadataReader metadata_reader;
        MetadataBuilder metadata_builder;
        const ulong DefaultExeBaseAddress32bit = 0x00400000;
        const ulong DefaultDllBaseAddress32bit = 0x10000000;

        #region Rewriting PE
        PEHeaderBuilder CreatePEHeaderBuilder(PEReader reader)
        {
            var pe_header = reader.PEHeaders.PEHeader;
            var dll_characteristics = DllCharacteristics.DynamicBase | DllCharacteristics.NxCompatible | DllCharacteristics.NoSeh | DllCharacteristics.TerminalServerAware;
            return new PEHeaderBuilder(Machine.Unknown, pe_header.SectionAlignment, 0x200, DefaultExeBaseAddress32bit,
                                       pe_header.MajorLinkerVersion, pe_header.MinorLinkerVersion, pe_header.MajorOperatingSystemVersion, pe_header.MinorOperatingSystemVersion,
                                       pe_header.MajorImageVersion, pe_header.MinorImageVersion, pe_header.MajorSubsystemVersion, pe_header.MinorSubsystemVersion,
                                       pe_header.Subsystem, dll_characteristics, Characteristics.ExecutableImage,
                                       0x00100000, 0x1000, 0x00100000, 0x1000);
        }

        void CreateMetadataBuilder(MetadataReader reader, PortablePDBGenerator pdbGenerator)
        {
            method_index = 1;
            param_index = 1;
            field_index = 1;

            AddAssemblyTable(reader, metadata_builder);
            AddAssemblyRefTable(reader, metadata_builder);
            AddModuleTable(reader, metadata_builder);
            AddTypeDefTable(reader, metadata_builder);
            AddTypeRefTable(reader, metadata_builder);
            AddCustomAttributeTable(reader, metadata_builder);

            if(pdbGenerator != null){
                foreach(var row_id in Enumerable.Range(1, reader.MethodDefinitions.Count)){
                    var method_def_handle = MetadataTokens.MethodDefinitionHandle(row_id);
                    var method_def = reader.GetMethodDefinition(method_def_handle);
                    var name = reader.GetString(method_def.Name);
                    if(!name.EndsWith("_Impl", StringComparison.Ordinal))
                        pdbGenerator.AddDummySequencePoints(row_id - 1);
                    else
                        pdbGenerator.AddMethodDefinition(name.Substring(0, name.IndexOf("_Impl", StringComparison.Ordinal)), method_def_handle);
                }
            }

            AddMemberRefTable(reader, metadata_builder);
            AddStandAloneSigTable(reader, metadata_builder);
            AddTypeSpecTable(reader, metadata_builder);
            AddMethodSpecTable(reader, metadata_builder);
        }

        void AddAssemblyTable(MetadataReader reader, MetadataBuilder builder)
        {
            var asm_def = reader.GetAssemblyDefinition();
            builder.AddAssembly(RetrieveString(builder, reader, asm_def.Name), asm_def.Version, RetrieveString(builder, reader, asm_def.Culture),
                                RetrieveBlob(builder, reader, asm_def.PublicKey), asm_def.Flags, asm_def.HashAlgorithm);
        }

        void AddAssemblyRefTable(MetadataReader reader, MetadataBuilder builder)
        {
            foreach(var asm_ref_handle in reader.AssemblyReferences){
                var asm_ref = reader.GetAssemblyReference(asm_ref_handle);
                builder.AddAssemblyReference(RetrieveString(builder, reader, asm_ref.Name), asm_ref.Version, RetrieveString(builder, reader, asm_ref.Culture),
                                             RetrieveBlob(builder, reader, asm_ref.PublicKeyOrToken), asm_ref.Flags,
                                             RetrieveBlob(builder, reader, asm_ref.HashValue));
            }
        }

        void AddModuleTable(MetadataReader reader, MetadataBuilder builder)
        {
            var module_def = reader.GetModuleDefinition();
            builder.AddModule(module_def.Generation, RetrieveString(builder, reader, module_def.Name), RetrieveGuid(builder, reader, module_def.Mvid), 
                              RetrieveGuid(builder, reader, module_def.GenerationId), RetrieveGuid(builder, reader, module_def.BaseGenerationId));
        }

        void AddMethodDef(MetadataReader reader, MetadataBuilder builder, MethodDefinitionHandle handle, int methodOffset)
        {
            var method_def = reader.GetMethodDefinition(handle);
            builder.AddMethodDefinition(method_def.Attributes, method_def.ImplAttributes, RetrieveString(builder, reader, method_def.Name),
                                        RetrieveBlob(builder, reader, method_def.Signature), methodOffset, MetadataTokens.ParameterHandle(param_index));

            foreach(var param_handle in method_def.GetParameters()){
                var parameter = reader.GetParameter(param_handle);
                builder.AddParameter(parameter.Attributes, RetrieveString(builder, reader, parameter.Name), parameter.SequenceNumber);
                ++param_index;
            }
        }

        void AddFieldDef(MetadataReader reader, MetadataBuilder builder, FieldDefinitionHandle handle)
        {
            var field_def = reader.GetFieldDefinition(handle);
            builder.AddFieldDefinition(field_def.Attributes, RetrieveString(builder, reader, field_def.Name), RetrieveBlob(builder, reader, field_def.Signature));
        }

        void AddTypeDef(MetadataReader reader, MetadataBuilder builder, TypeDefinition typeDef)
        {
            var first_field_handle = typeDef.GetFields().FirstOrDefault();
            builder.AddTypeDefinition(typeDef.Attributes, RetrieveString(builder, reader, typeDef.Namespace), RetrieveString(builder, reader, typeDef.Name),
                                      typeDef.BaseType, MetadataTokens.FieldDefinitionHandle(field_index), MetadataTokens.MethodDefinitionHandle(method_index));

            foreach(var field_def_handle in typeDef.GetFields()){
                AddFieldDef(reader, builder, field_def_handle);
                ++field_index;
            }

            foreach(var method_def_handle in typeDef.GetMethods()){
                AddMethodDef(reader, builder, method_def_handle, method_offsets[method_index - 1]);
                ++method_index;
            }
        }

        void AddTypeDefTable(MetadataReader reader, MetadataBuilder builder)
        {
            foreach(var type_def_handle in reader.TypeDefinitions){
                var type_def = reader.GetTypeDefinition(type_def_handle);
                AddTypeDef(reader, builder, type_def);
            }

            foreach(var type_def_handle in reader.TypeDefinitions){
                var type_def = reader.GetTypeDefinition(type_def_handle);
                foreach(var nested_type_handle in type_def.GetNestedTypes())
                    builder.AddNestedType(nested_type_handle, type_def_handle);
            }
        }

        void AddTypeRefTable(MetadataReader reader, MetadataBuilder builder)
        {
            foreach(var type_ref_handle in reader.TypeReferences){
                var type_ref = reader.GetTypeReference(type_ref_handle);
                builder.AddTypeReference(type_ref.ResolutionScope, RetrieveString(builder, reader, type_ref.Namespace), RetrieveString(builder, reader, type_ref.Name));
            }
        }

        void AddCustomAttributeTable(MetadataReader reader, MetadataBuilder builder)
        {
            foreach(var custom_attribute_handle in reader.CustomAttributes){
                var custom_attribute = reader.GetCustomAttribute(custom_attribute_handle);
                builder.AddCustomAttribute(custom_attribute.Parent, custom_attribute.Constructor, RetrieveBlob(builder, reader, custom_attribute.Value));
            }
        }

        void AddMemberRefTable(MetadataReader reader, MetadataBuilder builder)
        {
            var row_count = reader.GetTableRowCount(TableIndex.MemberRef);
            foreach(var row_number in Enumerable.Range(1, row_count)){
                var mem_ref = reader.GetMemberReference(MetadataTokens.MemberReferenceHandle(row_number));
                builder.AddMemberReference(mem_ref.Parent, RetrieveString(builder, reader, mem_ref.Name), RetrieveBlob(builder, reader, mem_ref.Signature));
            }
        }

        void AddStandAloneSigTable(MetadataReader reader, MetadataBuilder builder)
        {
            var row_count = reader.GetTableRowCount(TableIndex.StandAloneSig);
            foreach(var row_number in Enumerable.Range(1, row_count)){
                var stand_alone_sig = reader.GetStandaloneSignature(MetadataTokens.StandaloneSignatureHandle(row_number));
                builder.AddStandaloneSignature(RetrieveBlob(builder, reader, stand_alone_sig.Signature));
            }
        }

        void AddTypeSpecTable(MetadataReader reader, MetadataBuilder builder)
        {
            var row_count = reader.GetTableRowCount(TableIndex.TypeSpec);
            foreach(var row_number in Enumerable.Range(1, row_count)){
                var type_spec = reader.GetTypeSpecification(MetadataTokens.TypeSpecificationHandle(row_number));
                builder.AddTypeSpecification(RetrieveBlob(builder, reader, type_spec.Signature));
            }
        }

        void AddMethodSpecTable(MetadataReader reader, MetadataBuilder builder)
        {
            var row_count = reader.GetTableRowCount(TableIndex.MethodSpec);
            foreach(var row_number in Enumerable.Range(1, row_count)){
                var method_spec = reader.GetMethodSpecification(MetadataTokens.MethodSpecificationHandle(row_number));
                builder.AddMethodSpecification(method_spec.Method, RetrieveBlob(builder, reader, method_spec.Signature));
            }
        }

        int RetrieveIlStream(PEReader peReader, MetadataReader metadataReader, MethodDefinitionHandle methodDefinitionHandle, MethodBodyStreamEncoder methodBodies)
        {
            var method_def = metadataReader.GetMethodDefinition(methodDefinitionHandle);
            if(metadataReader.GetString(method_def.Name) == "main")
                main_method_def_handle = methodDefinitionHandle;

            var rva = method_def.RelativeVirtualAddress;
            if(rva == 0)
                return 0;

            var old_method_body = peReader.GetMethodBody(rva);
            var contents = old_method_body.GetILContent();
            var method_body_attributes = old_method_body.LocalVariablesInitialized ? MethodBodyAttributes.None : MethodBodyAttributes.InitLocals;
            var method_body = methodBodies.AddMethodBody(contents.Length, old_method_body.MaxStack, old_method_body.ExceptionRegions.Length, localVariablesSignature: old_method_body.LocalSignature, attributes: method_body_attributes);

            WriteInstructions(method_body.Instructions, contents);
            return method_body.Offset;
        }

        #region WriteInstrcutions
        void WriteInstructions(Blob finalIL, ImmutableArray<byte> generatedIL)
        {
            var writer = new BlobWriter(finalIL);

            writer.WriteBytes(generatedIL);
            writer.Offset = 0;

            int offset = 0;
            while(offset < generatedIL.Length){
                var operand_type = InstructionOperandTypes.ReadOperandType(generatedIL, ref offset);
                switch(operand_type){
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                    offset += 4;
                    break;

                case OperandType.InlineString:
                    {
                        writer.Offset = offset;

                        int pseudo_token = ReadInt32(generatedIL, offset);
                        var handle = ResolveUserStringHandleFromPseudoToken(pseudo_token);

                        writer.WriteInt32(MetadataTokens.GetToken(handle));

                        offset += 4;
                        break;
                    }

                case OperandType.InlineSig:
                case OperandType.InlineBrTarget:
                case OperandType.InlineI:
                case OperandType.ShortInlineR:
                    offset += 4;
                    break;

                case OperandType.InlineSwitch:
                    int arg_count = ReadInt32(generatedIL, offset);
                    offset += (arg_count + 1) * 4;
                    break;

                case OperandType.InlineI8:
                case OperandType.InlineR:
                    offset += 8;
                    break;

                case OperandType.InlineNone:
                    break;

                case OperandType.InlineVar:
                    offset += 2;
                    break;

                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    offset += 1;
                    break;

                default:
                    throw new Exception(string.Format("Unexpected value: {0}", operand_type));
                }
            }
        }

        static int ReadInt32(ImmutableArray<byte> buffer, int pos)
        {
            return buffer[pos] | buffer[pos + 1] << 8 | buffer[pos + 2] << 16 | buffer[pos + 3] << 24;
        }

        UserStringHandle ResolveUserStringHandleFromPseudoToken(int pseudoStringToken)
        {
            int index = pseudoStringToken;
            var str = metadata_reader.GetUserString(MetadataTokens.UserStringHandle(index));
            return metadata_builder.GetOrAddUserString(str);
        }
        #endregion

        public void EmitDebugDirectoryAndPdb(string assemblyFileName, string pdbName, BlobContentId pdbId, PortablePDBGenerator pdbGenerator)
        {
            ManagedPEBuilder builder;
            MetadataBuilder metadata_builder2;
            using(var asm_file = File.OpenRead(assemblyFileName))
            using(var reader = new PEReader(asm_file)){
                var header_builder = CreatePEHeaderBuilder(reader);
                metadata_reader = reader.GetMetadataReader();
                metadata_builder = new MetadataBuilder();

                var il_builder = new BlobBuilder();
                var method_bodies = new MethodBodyStreamEncoder(il_builder);
                var method_def_count = metadata_reader.MethodDefinitions.Count;
                method_offsets = new int[method_def_count];
                foreach(var pair in Enumerable.Range(0, method_def_count).Zip(metadata_reader.MethodDefinitions, (l, r) => new {Index = l, MethodDef = r})){
                    var method_offset = RetrieveIlStream(reader, metadata_reader, pair.MethodDef, method_bodies);
                    method_offsets[pair.Index] = method_offset;
                }

                CreateMetadataBuilder(metadata_reader, pdbGenerator);
                var metadata_builder_for_pe = metadata_builder;
                metadata_builder = new MetadataBuilder();
                CreateMetadataBuilder(metadata_reader, null);
                metadata_builder2 = metadata_builder;
                var metadata_root_builder = new MetadataRootBuilder(metadata_builder_for_pe, metadata_reader.MetadataVersion);

                var debug_directory_builder = new DebugDirectoryBuilder();
                debug_directory_builder.AddCodeViewEntry(pdbName, pdbId, 0);
                builder = new ManagedPEBuilder(header_builder, metadata_root_builder, il_builder, debugDirectoryBuilder: debug_directory_builder, entryPoint: main_method_def_handle,
                                               flags: reader.PEHeaders.CorHeader.Flags);
            }

            var new_pe_builder = new BlobBuilder();
            builder.Serialize(new_pe_builder);

            using(var write_stream = File.Create(assemblyFileName)){
                new_pe_builder.WriteContentTo(write_stream);
            }

            Console.WriteLine("Emitting a PDB file...");
            var pdb_file_path = Path.Combine(options.OutputPath, options.ExecutableName + ".pdb");
            pdbGenerator.MetadataBuilder = metadata_builder2;
            pdbGenerator.WriteToFile(pdb_file_path, pdbId, main_method_def_handle);
        }

        static StringHandle RetrieveString(MetadataBuilder builder, MetadataReader reader, StringHandle stringHandle)
        {
            return builder.GetOrAddString(reader.GetString(stringHandle));
        }

        static BlobHandle RetrieveBlob(MetadataBuilder builder, MetadataReader reader, BlobHandle blobHandle)
        {
            return builder.GetOrAddBlob(reader.GetBlobContent(blobHandle));
        }

        static GuidHandle RetrieveGuid(MetadataBuilder builder, MetadataReader reader, GuidHandle guidHandle)
        {
            return builder.GetOrAddGuid(reader.GetGuid(guidHandle));
        }
        #endregion
	}
}
