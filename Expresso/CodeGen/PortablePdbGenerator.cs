using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Expresso.Ast;
using ICSharpCode.NRefactory;

namespace Expresso.CodeGen
{
    /// <summary>
    /// A portable PDB generator.
    /// </summary>
    public class PortablePDBGenerator
    {
        static List<string> seen_func_names = new List<string>();
        static List<List<SequencePoint>> emitted_sps = new List<List<SequencePoint>>();
        static List<LocalScopeInformation> local_scopes = new List<LocalScopeInformation>();

        List<SequencePoint> sequence_points = new List<SequencePoint>();
        List<DebugDocument> documents = new List<DebugDocument>();
        string current_func_name;
        DocumentHandle current_doc_handle;
        LocalScopeInformation current_scope;
        Dictionary<string, MethodDefinitionHandle> impl_method_handles = new Dictionary<string, MethodDefinitionHandle>();

        public MetadataBuilder MetadataBuilder{
            private get; set;
        }

        public static PortablePDBGenerator CreatePortablePDBGenerator()
        {
            return new PortablePDBGenerator();
        }

        public void AddSequencePoints(string funcName)
        {
            if(seen_func_names.Contains(funcName))
                throw new InvalidOperationException("The function {0} is already peeked.");

            seen_func_names.Add(funcName);
            if(current_func_name == null)
                current_func_name = funcName;

            SetSequencePoints(funcName);
        }

        public void AddMethodDefinition(string name, MethodDefinitionHandle methodDefinitionHandle)
        {
            impl_method_handles.Add(name, methodDefinitionHandle);
        }

        public void AddDummySequencePoints(int index)
        {
            SetSequencePoints(null);

            emitted_sps.Insert(index, new List<SequencePoint>());
        }

        public void MarkSequencePoint(int ilOffset, TextLocation startLoc, TextLocation endLoc)
        {
            sequence_points.Add(new SequencePoint(ilOffset, startLoc.Line, startLoc.Column, endLoc.Line, endLoc.Column));
        }

        public void AddLocalScope(int startOffset)
        {
            var new_scope = new LocalScopeInformation(current_func_name, default, startOffset);
            local_scopes.Add(new_scope);
            current_scope = new_scope;
        }

        public void AddLocalVariable(LocalVariableAttributes attributes, int index, string name)
        {
            current_scope.LocalVariables.Add(new LocalVariableInformation(attributes, index, name));
        }

        public void SetLengthOnLocalScope(int length)
        {
            current_scope.Length = length;
        }

        public void WriteToFile(string filePath, BlobContentId pdbId, MethodDefinitionHandle mainMethodHandle)
        {
            var type_system_row_counts = MetadataBuilder.GetRowCounts();

            AddDebugTables();

            var pdb_builder = new PortablePdbBuilder(MetadataBuilder, type_system_row_counts, mainMethodHandle, _ => pdbId);

            var blob_builder = new BlobBuilder();
            pdb_builder.Serialize(blob_builder);
            using(var file_stream = File.Create(filePath)){
                blob_builder.WriteContentTo(file_stream);
            }
        }

        public void AddDocument(string filePath, Guid languageGuid)
        {
            documents.Add(new DebugDocument(filePath, default, default, languageGuid));
        }

        void SetSequencePoints(string nextFuncName)
        {
            if(sequence_points.Count > 0){
                emitted_sps.Add(new List<SequencePoint>(sequence_points));
                current_func_name = nextFuncName;
                sequence_points.Clear();
            }
        }

        void AddDebugTables()
        {
            MetadataBuilder.SetCapacity(TableIndex.Document, documents.Count);
            foreach(var doc in documents){
                current_doc_handle = MetadataBuilder.AddDocument(MetadataBuilder.GetOrAddDocumentName(doc.FilePath), default, default, MetadataBuilder.GetOrAddGuid(doc.LanguageGuid));

                MetadataBuilder.SetCapacity(TableIndex.MethodDebugInformation, emitted_sps.Count);
                foreach(var sequence_point_list in emitted_sps){
                    var sp_blob = SerializeSequencePoints(sequence_point_list);
                    if(sp_blob.IsNil)
                        MetadataBuilder.AddMethodDebugInformation(default, default);
                    else
                        MetadataBuilder.AddMethodDebugInformation(current_doc_handle, sp_blob);
                }
            }

            var first_local_variable = default(LocalVariableHandle);
            foreach(var scope in local_scopes){
                foreach(var lv in scope.LocalVariables){
                    var local_variable = MetadataBuilder.AddLocalVariable(lv.Attributes, lv.Index, MetadataBuilder.GetOrAddString(lv.Name));
                    if(first_local_variable.IsNil)
                        first_local_variable = local_variable;
                }

                var method_handle = impl_method_handles[scope.FuncName];
                MetadataBuilder.AddLocalScope(method_handle, scope.ImportScope, first_local_variable, default, scope.StartOffset, scope.Length);
                first_local_variable = default;
            }
        }

        BlobHandle SerializeSequencePoints(List<SequencePoint> sequencePoints)
        {
            if(!sequencePoints.Any())
                return new BlobHandle();
            
            var writer = new BlobBuilder();

            // header: LocalSignature
            writer.WriteCompressedInteger(0);

            var first_seq_point = sequencePoints.First();
            var prev_non_hidden_start_line = first_seq_point.StartLine;
            var prev_non_hidden_start_column = first_seq_point.StartColumn;
            var prev_offset = first_seq_point.IlOffset;

            // first IL offset
            writer.WriteCompressedInteger(first_seq_point.IlOffset);
            // first ΔLine and ΔColumns
            SerializeDeltaLineColumns(writer, first_seq_point);

            // first δLine and δColumn
            writer.WriteCompressedInteger(first_seq_point.StartLine);
            writer.WriteCompressedInteger(first_seq_point.StartColumn);

            foreach(var seq_point in sequencePoints.Skip(1)){
                // δILOffset
                writer.WriteCompressedInteger(seq_point.IlOffset - prev_offset);

                // ΔLine and ΔColumn
                SerializeDeltaLineColumns(writer, seq_point);

                // δLine and δColumn
                writer.WriteCompressedSignedInteger(seq_point.StartLine - prev_non_hidden_start_line);
                writer.WriteCompressedSignedInteger(seq_point.StartColumn - prev_non_hidden_start_column);

                prev_non_hidden_start_line = seq_point.StartLine;
                prev_non_hidden_start_column = seq_point.StartColumn;
            }

            return MetadataBuilder.GetOrAddBlob(writer);
        }

        static void SerializeDeltaLineColumns(BlobBuilder writer, SequencePoint sequencePoint)
        {
            var delta_line = sequencePoint.EndLine - sequencePoint.StartLine;
            // ΔLine
            writer.WriteCompressedInteger(delta_line);
            // ΔColumn
            if(delta_line == 0)
                writer.WriteCompressedInteger(sequencePoint.EndColumn - sequencePoint.StartColumn);
            else
                writer.WriteCompressedSignedInteger(sequencePoint.EndColumn - sequencePoint.StartColumn);
        }

        static AssemblyHashAlgorithm GetHashAlgorithm(System.Configuration.Assemblies.AssemblyHashAlgorithm algorithm)
        {
            switch(algorithm){
            case System.Configuration.Assemblies.AssemblyHashAlgorithm.MD5:
                return AssemblyHashAlgorithm.MD5;

            case System.Configuration.Assemblies.AssemblyHashAlgorithm.SHA1:
                return AssemblyHashAlgorithm.Sha1;

            case System.Configuration.Assemblies.AssemblyHashAlgorithm.SHA256:
                return AssemblyHashAlgorithm.Sha256;

            case System.Configuration.Assemblies.AssemblyHashAlgorithm.SHA384:
                return AssemblyHashAlgorithm.Sha384;

            case System.Configuration.Assemblies.AssemblyHashAlgorithm.SHA512:
                return AssemblyHashAlgorithm.Sha512;

            case System.Configuration.Assemblies.AssemblyHashAlgorithm.None:
                return AssemblyHashAlgorithm.None;

            default:
                throw new ArgumentOutOfRangeException(nameof(algorithm));
            }
        }
    }
}
