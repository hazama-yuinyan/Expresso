using System;
namespace Expresso.CodeGen
{
    /// <summary>
    /// Holds information about a Sequence Point.
    /// </summary>
    public struct SequencePoint
    {
        public int ILOffset{
            get; set;
        }

        public int StartLine{
            get; set;
        }

        public int StartColumn{
            get; set;
        }

        public int EndLine{
            get; set;
        }

        public int EndColumn{
            get; set;
        }

        public SequencePoint(int ilOffset, int startLine, int startColumn, int endLine, int endColumn)
        {
            ILOffset = ilOffset;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }
    }
}
