using System;
using System.Collections.Generic;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// Type inference context.
    /// </summary>
    public class TypeInferenceContext
    {
        /// <summary>
        /// A symbol table that maps names to symbols located in external files.
        /// </summary>
        /// <value>The symbol map.</value>
        public SymbolTable Symbols{
            get; set;
        }

        public TypeInferenceContext()
        {
            Symbols = null;
        }
    }
}

