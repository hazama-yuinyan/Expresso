using System;
using ICSharpCode.NRefactory;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// Specific role used only for Expresso tokens.
    /// </summary>
    public sealed class TokenRole : Role<ExpressoTokenNode>
    {
        internal static readonly List<string> Tokens = new List<string>();
        internal static readonly List<int> TokenLengths = new List<int>();
        internal readonly uint TokenIndex;

        static TokenRole()
        {
            // null token
            Tokens.Add("");
            TokenLengths.Add(0);
        }

        /// <summary>
        /// Gets the token as string.
        /// </summary>
        public string Token{
            get; private set;
        }

        /// <summary>
        /// Gets the char length of the token.
        /// </summary>
        public int Length{
            get; private set;
        }

        public TokenRole(string token) : base(token, ExpressoTokenNode.Null)
        {
            Token = token;
            Length = token.Length;

            int index = Tokens.FindIndex((input) => input == token);
            if(index < 0){
                TokenIndex = (uint)Tokens.Count;
                Tokens.Add(token);
                TokenLengths.Add(Length);
            }else{
                TokenIndex = (uint)index;
            }
        }
    }
}

