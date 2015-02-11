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
        /// Gets or sets the stack.
        /// </summary>
        /// <value>The stack.</value>
        public Stack<AstNode> Stack{
            get; private set;
        }

        /// <summary>
        /// A symbol table that maps names to symbols located in external files.
        /// </summary>
        /// <value>The symbol map.</value>
        public Dictionary<string, SymbolTable> SymbolMap{
            get; set;
        }

        public TypeInferenceContext()
        {
            Stack = new Stack<AstNode>();
            SymbolMap = new Dictionary<string, SymbolTable>();
        }
    }
}

