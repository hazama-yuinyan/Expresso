﻿using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace Expresso.CodeGen
{
    /// <summary>
    /// A portable PDB generator.
    /// </summary>
    public class PortablePDBGenerator : DebugInfoGenerator
    {
        static Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter> symbol_writers = new Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter>();
        static List<LambdaExpression> seen_methods = new List<LambdaExpression>();

        int num_documents = 0, num_methods = 0;
        MetadataBuilder metadata_builder = new MetadataBuilder();
        List<Tuple<int, DebugInfoExpression>> sequence_points = new List<Tuple<int, DebugInfoExpression>>();
        DocumentHandle current_doc_handle;

        public override void MarkSequencePoint(LambdaExpression method, int ilOffset, DebugInfoExpression sequencePoint)
        {
            //ilg.MarkSequencePoint(GetSymbolWriter(method_builder, sequencePoint.Document), sequencePoint.StartLine, sequencePoint.StartColumn, sequencePoint.EndLine, sequencePoint.EndColumn);
            if(seen_methods.IndexOf(method) == -1){
                ++num_methods;
                metadata_builder.SetCapacity(TableIndex.MethodDebugInformation, num_methods);
                seen_methods.Add(method);

                if(sequence_points.Count > 0){
                    var sequence_points_blob = SerializeSequencePoints();
                    metadata_builder.AddMethodDebugInformation(current_doc_handle, sequence_points_blob);
                    sequence_points.Clear();
                }

                sequence_points.Add(new Tuple<int, DebugInfoExpression>(ilOffset, sequencePoint));
            }else{
                sequence_points.Add(new Tuple<int, DebugInfoExpression>(ilOffset, sequencePoint));
            }
        }

        public void WriteToFile(string filePath)
        {
            if(sequence_points.Count > 0){
                // In order to mark sequence points on the last method
                var sequence_points_blob = SerializeSequencePoints();
                metadata_builder.AddMethodDebugInformation(current_doc_handle, sequence_points_blob);
                sequence_points.Clear();
            }

            var raw_pdb_id = new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01};
            var pdb_id = new BlobContentId(raw_pdb_id);

            var pdb_builder = new PortablePdbBuilder(metadata_builder, metadata_builder.GetRowCounts(), default, _ => pdb_id);
            var blob_builder = new BlobBuilder();
            pdb_builder.Serialize(blob_builder);
            using(var file_stream = File.Create(filePath)){
                blob_builder.WriteContentTo(file_stream);
            }
        }

        ISymbolDocumentWriter GetSymbolWriter(MethodBuilder method, SymbolDocumentInfo document)
        {
            if(!symbol_writers.TryGetValue(document, out var result)){
                result = ((ModuleBuilder)method.Module).DefineDocument(document.FileName, document.Language, document.LanguageVendor, document.DocumentType);
                symbol_writers.Add(document, result);

                ++num_documents;
                metadata_builder.SetCapacity(TableIndex.Document, num_documents);

                var name = document.FileName;
                //var hash_argorithm = metadata_builder.GetOrAddGuid(null);
                var language = document.Language;
                //var hash = metadata_builder.GetOrAddBlob(null);
                current_doc_handle = metadata_builder.AddDocument(metadata_builder.GetOrAddDocumentName(name), default, default, metadata_builder.GetOrAddGuid(language));
            }

            return result;
        }

        BlobHandle SerializeSequencePoints()
        {
            var writer = new BlobBuilder();

            // header: LocalSignature
            writer.WriteCompressedInteger(0);

            var first_seq_point = sequence_points.First();
            var prev_non_hidden_start_line = first_seq_point.Item2.StartLine;
            var prev_non_hidden_start_column = first_seq_point.Item2.StartColumn;
            var prev_offset = first_seq_point.Item1;

            // first IL offset
            writer.WriteCompressedInteger(first_seq_point.Item1);
            // first ΔLine and ΔColumns
            SerializeDeltaLineColumns(writer, first_seq_point);

            // first δLine and δColumn
            writer.WriteCompressedInteger(first_seq_point.Item2.StartLine);
            writer.WriteCompressedInteger(first_seq_point.Item2.StartColumn);

            foreach(var seq_point in sequence_points.Skip(1)){
                // δILOffset
                writer.WriteCompressedInteger(seq_point.Item1 - prev_offset);

                // ΔLine and ΔColumn
                SerializeDeltaLineColumns(writer, seq_point);

                // δLine and δColumn
                writer.WriteCompressedSignedInteger(seq_point.Item2.StartLine - prev_non_hidden_start_line);
                writer.WriteCompressedSignedInteger(seq_point.Item2.StartColumn - prev_non_hidden_start_column);

                prev_non_hidden_start_line = seq_point.Item2.StartLine;
                prev_non_hidden_start_column = seq_point.Item2.StartColumn;
            }

            return metadata_builder.GetOrAddBlob(writer);
        }

        static void SerializeDeltaLineColumns(BlobBuilder writer, Tuple<int, DebugInfoExpression> sequencePoint)
        {
            var debug_info = sequencePoint.Item2;
            var delta_line = debug_info.EndLine - debug_info.StartLine;
            // ΔLine
            writer.WriteCompressedInteger(delta_line);
            // ΔColumn
            if(delta_line == 0)
                writer.WriteCompressedInteger(debug_info.EndColumn - debug_info.StartColumn);
            else
                writer.WriteCompressedSignedInteger(debug_info.EndColumn - debug_info.StartColumn);
        }

        public static PortablePDBGenerator CreatePortablePDBGenerator()
        {
            return new PortablePDBGenerator();
        }
    }
}
