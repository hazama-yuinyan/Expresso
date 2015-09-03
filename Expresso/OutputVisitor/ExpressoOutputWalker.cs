using System;
using System.Collections.Generic;
using System.IO;
using Expresso.Formatting;


namespace Expresso.Ast
{
    public class ExpressoOutputWalker : IAstWalker
    {
        readonly TokenWriter writer;
        readonly ExpressoFormattingOptions policy;
        readonly Stack<AstNode> stack = new Stack<AstNode>();

        public ExpressoOutputWalker(TextWriter textWriter, ExpressoFormattingOptions policy)
        {
            if(textWriter == null)
                throw new ArgumentNullException("textWriter");

            if(policy == null)
                throw new ArgumentNullException("policy");

            writer = textWriter;
            this.policy = policy;
        }

        #region IsKeyword test
        static readonly HashSet<string> keywords = new HashSet<string>{
            "abstract", "base", "bool", "break", "byte", "case", "catch",
            "char", "class", "const", "continue", "def", "default", "do",
            "double", "else", "enum", "explicit", "export", "false",
            "finally", "float", "for", "goto", "if", "implicit",
            "in", "int", "intseq", "interface", "internal", "is", "long", "module", "new",
            "null", "object", "operator", "out", "override", "params", "private",
            "protected", "public", "readonly", "ref", "return", "sealed",
            "static", "string", "struct", "switch", "require", "this", "throw", "true",
            "try", "typeof", "virtual", "void", "while"
        };

        public static bool IsKeyword(string identifier, AstNode context)
        {
            return keywords.Contains(identifier);
        }
        #endregion
    }
}

