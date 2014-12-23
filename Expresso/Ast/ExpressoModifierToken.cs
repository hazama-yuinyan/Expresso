using System;
using ICSharpCode.NRefactory;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents Expresso's modifier.
    /// </summary>
    public class ExpressoModifierToken : ExpressoTokenNode
    {
        Modifiers modifier;

        public Modifiers Modifier{
            get{
                return modifier;
            }

            set{
                ThrowIfFrozen();
                modifier = value;
            }
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            ExpressoModifierToken o = other as ExpressoModifierToken;
            return o != null && modifier == o.modifier;
        }

        // Not worth using a dictionary for such few elements.
        // This table is sorted in the order that modifiers should be output when generating code.
        static readonly Modifiers[] allModifiers = {
            Modifiers.Public, Modifiers.Protected, Modifiers.Private,
            Modifiers.Abstract, Modifiers.Virtual, Modifiers.Static, Modifiers.Override,
            Modifiers.Const,
            Modifiers.Any
        };

        public static IEnumerable<Modifiers> AllModifiers {
            get{return allModifiers;}
        }

        public ExpressoModifierToken(TextLocation location, Modifiers modifier) : base(location, null)
        {
            int token_length = modifier.ToString().Length;
            end_loc = new TextLocation(location.Line, location.Column + token_length);
        }
    }
}

