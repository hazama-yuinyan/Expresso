using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Expresso.Ast;

namespace Expresso.CodeGen
{
    public partial class CodeGenerator : IAstWalker<CSharpEmitterContext, Type>
	{
        int param_index = 1, method_offset = 0, last_rva = -1;
        #region Rewriting PE
        PEHeaderBuilder CreatePEHeaderBuilder(PEReader reader)
        {
            var pe_header = reader.PEHeaders.PEHeader;
            return new PEHeaderBuilder(reader.PEHeaders.CoffHeader.Machine, pe_header.SectionAlignment, pe_header.SectionAlignment, pe_header.ImageBase,
                                       pe_header.MajorLinkerVersion, pe_header.MinorLinkerVersion, pe_header.MajorOperatingSystemVersion, pe_header.MinorOperatingSystemVersion,
                                       pe_header.MajorImageVersion, pe_header.MinorImageVersion, pe_header.MajorSubsystemVersion, pe_header.MinorSubsystemVersion,
                                       pe_header.Subsystem, pe_header.DllCharacteristics, reader.PEHeaders.CoffHeader.Characteristics, pe_header.SizeOfStackReserve,
                                       pe_header.SizeOfStackCommit, pe_header.SizeOfHeapReserve, pe_header.SizeOfHeapCommit);
        }

        MetadataBuilder CreateMetadataBuilder(MetadataReader reader, PortablePDBGenerator pdbGenerator)
        {
            var builder = new MetadataBuilder();
            AddAssemblyTable(reader, builder);
            AddAssemblyRefTable(reader, builder);
            AddModuleTable(reader, builder);
            AddTypeDefTable(reader, builder);
            AddTypeRefTable(reader, builder);
            AddCustomAttributeTable(reader, builder);

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

            AddMemberRefTable(reader, builder);
            AddStandAloneSigTable(reader, builder);
            AddTypeSpecTable(reader, builder);
            AddMethodSpecTable(reader, builder);

            AddUserStrings(reader, builder);

            return builder;
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

        void AddMethodDef(MetadataReader reader, MetadataBuilder builder, MethodDefinitionHandle handle)
        {
            var method_def = reader.GetMethodDefinition(handle);
            if(last_rva != -1)
                method_offset += method_def.RelativeVirtualAddress - last_rva;

            last_rva = method_def.RelativeVirtualAddress;

            builder.AddMethodDefinition(method_def.Attributes, method_def.ImplAttributes, RetrieveString(builder, reader, method_def.Name),
                                        RetrieveBlob(builder, reader, method_def.Signature), method_offset, MetadataTokens.ParameterHandle(param_index));

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

        void AddTypeDef(MetadataReader reader, MetadataBuilder builder, int typeIndex, TypeDefinitionHandle handle)
        {
            var type_def = reader.GetTypeDefinition(handle);
            foreach(var field_def_handle in type_def.GetFields())
                AddFieldDef(reader, builder, field_def_handle);

            var methods = type_def.GetMethods();
            foreach(var method_index in Enumerable.Range(0, methods.Count)){
                var method_def_handle = methods.ElementAt(method_index);
                AddMethodDef(reader, builder, method_def_handle);
            }

            var first_field_handle = type_def.GetFields().FirstOrDefault();
            builder.AddTypeDefinition(type_def.Attributes, RetrieveString(builder, reader, type_def.Namespace), RetrieveString(builder, reader, type_def.Name),
                                      type_def.BaseType, first_field_handle, type_def.GetMethods().FirstOrDefault());
        }

        void AddTypeDefTable(MetadataReader reader, MetadataBuilder builder)
        {
            foreach(var type_index in Enumerable.Range(0, reader.TypeDefinitions.Count)){
                var type_def_handle = reader.TypeDefinitions.ElementAt(type_index);
                var type_def = reader.GetTypeDefinition(type_def_handle);
                AddTypeDef(reader, builder, type_index, type_def_handle);

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

        void AddUserStrings(MetadataReader reader, MetadataBuilder builder)
        {
            var handle = MetadataTokens.UserStringHandle(0);
            do{
                var user_string = reader.GetUserString(handle);
                builder.GetOrAddUserString(user_string);
                handle = reader.GetNextHandle(handle);
            }while(!handle.IsNil);
        }

        void RetrieveIlStream(PEReader peReader, MetadataReader metadataReader, MethodDefinitionHandle methodDefinitionHandle, MethodBodyStreamEncoder methodBodies)
        {
            var method_def = metadataReader.GetMethodDefinition(methodDefinitionHandle);
            Console.WriteLine("Adding the method body of {0}..., rva = {1}", metadataReader.GetString(method_def.Name), method_def.RelativeVirtualAddress);
            if(metadataReader.GetString(method_def.Name) == "main")
                main_method_def_handle = methodDefinitionHandle;

            var rva = method_def.RelativeVirtualAddress;
            if(rva == 0)
                throw new InvalidOperationException("RelativeVirtualAddress is 0!");

            var old_method_body = peReader.GetMethodBody(rva);
            var contents = old_method_body.GetILContent();
            var method_body_attributes = old_method_body.LocalVariablesInitialized ? MethodBodyAttributes.None : MethodBodyAttributes.InitLocals;
            var method_body = methodBodies.AddMethodBody(contents.Length, old_method_body.MaxStack, old_method_body.ExceptionRegions.Length, localVariablesSignature: old_method_body.LocalSignature, attributes: method_body_attributes);
            var writer = new BlobWriter(method_body.Instructions);
                
            writer.WriteBytes(contents);
            /*writer.Offset = 0;

            int offset;
            while(offset < contents.Length){
                inst
            }*/
        }

        void EmitDebugDirectoryAndPdb(string assemblyFileName, string pdbName, BlobContentId pdbId, PortablePDBGenerator pdbGenerator)
        {
            ManagedPEBuilder builder;
            MetadataBuilder metadata_builder2;
            using(var asm_file = File.OpenRead(assemblyFileName))
            using(var reader = new PEReader(asm_file)){
                var header_builder = CreatePEHeaderBuilder(reader);
                var metadata_reader = reader.GetMetadataReader();
                var metadata_builder = CreateMetadataBuilder(metadata_reader, pdbGenerator);
                metadata_builder2 = CreateMetadataBuilder(metadata_reader, null);
                var metadata_root_builder = new MetadataRootBuilder(metadata_builder, metadata_reader.MetadataVersion);

                var il_builder = new BlobBuilder();
                var method_bodies = new MethodBodyStreamEncoder(il_builder);
                foreach(var method_def_handle in metadata_reader.MethodDefinitions)
                    RetrieveIlStream(reader, metadata_reader, method_def_handle, method_bodies);

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
