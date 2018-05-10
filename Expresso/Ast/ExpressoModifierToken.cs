using System;
using ICSharpCode.NRefactory;
using System.Collections.Generic;
using System.Diagnostics;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents an Expresso's modifier.
    /// </summary>
    public class ExpressoModifierToken : ExpressoTokenNode
    {
        public static readonly TokenRole PublicModifierRole = new TokenRole("public", Null);
        public static readonly TokenRole ProtectedModifierRole = new TokenRole("protected", Null);
        public static readonly TokenRole PrivateModifierRole = new TokenRole("private", Null);
        public static readonly TokenRole AbstractModifierRole = new TokenRole("abstract", Null);
        public static readonly TokenRole VirtualModifierRole = new TokenRole("virtual", Null);
        public static readonly TokenRole StaticModifierRole = new TokenRole("static", Null);
        public static readonly TokenRole OverrideModifierRole = new TokenRole("override", Null);
        public static readonly TokenRole MutatingModifierRole = new TokenRole("mutating", Null);
        public static readonly TokenRole ExportModifierRole = new TokenRole("export", Null);
        public static readonly TokenRole ImmutableModifierRole = new TokenRole("immutable", Null);

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
            bool res = o != null && modifier == o.modifier;
            Debug.WriteIf(!res, modifier);
            return res;
        }

        // Not worth using a dictionary for such few elements.
        // This table is sorted in the order that modifiers should be output when generating code.
        static readonly Modifiers[] allModifiers = {
            Modifiers.Public, Modifiers.Protected, Modifiers.Private,
            Modifiers.Abstract, Modifiers.Virtual, Modifiers.Static, Modifiers.Override,
            Modifiers.Mutating,
            Modifiers.Export,
            Modifiers.Immutable,
            Modifiers.Any
        };

        public static IEnumerable<Modifiers> AllModifiers => allModifiers;

        public ExpressoModifierToken(TextLocation loc, Modifiers modifier)
            : base(loc, GetTokenRole(modifier.ToString().ToLower()))
        {
            this.modifier = modifier;
        }

        static TokenRole GetTokenRole(string modifier)
        {
            switch(modifier){
            case "public":
                return PublicModifierRole;

            case "protected":
                return ProtectedModifierRole;

            case "private":
                return PrivateModifierRole;

            case "abstract":
                return AbstractModifierRole;

            case "virtual":
                return VirtualModifierRole;

            case "static":
                return StaticModifierRole;

            case "override":
                return OverrideModifierRole;

            case "mutating":
                return MutatingModifierRole;

            case "export":
                return ExportModifierRole;

            case "immutable":
                return ImmutableModifierRole;

            default:
                throw new InvalidOperationException("Unknown modifier!");
            }
        }
    }
}

