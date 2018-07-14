using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        static List<FunctionDeclaration> seen_funcs = new List<FunctionDeclaration>();

        int num_documents = 0, num_methods = 0;
        MetadataBuilder metadata_builder = new MetadataBuilder();
        List<SequencePoint> sequence_points = new List<SequencePoint>();
        DocumentHandle current_doc_handle;

        public void MarkFunction(FunctionDeclaration funcDecl)
        {
            if(seen_funcs.IndexOf(funcDecl) != -1)
                throw new InvalidOperationException("The function {0} is already peeked.");

            seen_funcs.Add(funcDecl);
            ++num_methods;
            metadata_builder.SetCapacity(TableIndex.MethodDebugInformation, num_methods);
            seen_funcs.Add(funcDecl);

            if(sequence_points.Count > 0){
                var sequence_point_blob = SerializeSequencePoints();
                metadata_builder.AddMethodDebugInformation(current_doc_handle, sequence_point_blob);
                sequence_points.Clear();
            }
        }

        public void MarkSequencePoint(int ilOffset, TextLocation startLoc, TextLocation endLoc)
        {
            sequence_points.Add(new SequencePoint(ilOffset, startLoc.Line, startLoc.Column, endLoc.Line, endLoc.Column));
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

        public void AddDocument(string filePath, Guid languageGuid)
        {
            ++num_documents;
            metadata_builder.SetCapacity(TableIndex.Document, num_documents);

            current_doc_handle = metadata_builder.AddDocument(metadata_builder.GetOrAddDocumentName(filePath), default, default, metadata_builder.GetOrAddGuid(languageGuid));
        }

        BlobHandle SerializeSequencePoints()
        {
            var writer = new BlobBuilder();

            // header: LocalSignature
            writer.WriteCompressedInteger(0);

            var first_seq_point = sequence_points.First();
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

            foreach(var seq_point in sequence_points.Skip(1)){
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

            return metadata_builder.GetOrAddBlob(writer);
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

        public static PortablePDBGenerator CreatePortablePDBGenerator()
        {
            return new PortablePDBGenerator();
        }
    }
}
