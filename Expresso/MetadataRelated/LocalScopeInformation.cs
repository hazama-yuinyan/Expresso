using System.Collections.Generic;
using System.Reflection.Metadata;
using Expresso.Ast;

namespace Expresso.CodeGen
{
    /// <summary>
    /// Stores local scope informations.
    /// </summary>
    public struct LocalScopeInformation
    {
        public FunctionDeclaration Func{
            get; set;
        }

        public ImportScopeHandle ImportScope{
            get; set;
        }

        public List<LocalVariableInformation> LocalVariables{
            get; set;
        }

        public List<LocalConstantHandle> LocalConstants{
            get; set;
        }

        public int StartOffset{
            get; set;
        }

        public int Length{
            get; set;
        }

        public LocalScopeInformation(FunctionDeclaration func, ImportScopeHandle importScope, int startOffset)
        {
            Func = func;
            ImportScope = importScope;
            LocalVariables = new List<LocalVariableInformation>();
            LocalConstants = new List<LocalConstantHandle>();
            StartOffset = startOffset;
            Length = -1;
        }
    }
}
