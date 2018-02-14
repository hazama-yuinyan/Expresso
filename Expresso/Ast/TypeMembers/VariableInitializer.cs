using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a variable initialization.
    /// A variable initialization represents a variable and an initialization.
    /// Note that the syntax itself is the same as that of ParameterDeclaration.
    /// Pattern ( [ "(-" Type ] [ '=' Expression ] ) ;
    /// </summary>
    public class VariableInitializer : AstNode
    {
        public override NodeType NodeType{
            get{
                return NodeType.Unknown;
            }
        }

        /// <summary>
        /// Convenient property for retrieving the inner identifier pattern.
        /// </summary>
        /// <value>The name.</value>
        public string Name{
            get{
                var pattern = Pattern;
                if(pattern is IdentifierPattern ident_pat)
                    return ident_pat.Identifier.Name;
                else
                    return null;
            }
        }

        public Identifier NameToken{
            get{
                var pattern = Pattern;
                if(pattern is IdentifierPattern ident_pat)
                    return ident_pat.Identifier;
                else
                    return null;
            }
        }

        /// <summary>
        /// Represents the pattern.
        /// </summary>
        /// <value>The name token.</value>
        public PatternConstruct Pattern{
            get{return GetChildByRole(Roles.Pattern);}
            set{SetChildByRole(Roles.Pattern, value);}
        }

        public ExpressoTokenNode AssignToken{
            get{return GetChildByRole(Roles.AssignToken);}
        }

        /// <summary>
        /// Represents the initialization code.
        /// </summary>
        /// <value>The initializer.</value>
        public Expression Initializer{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        public VariableInitializer(PatternConstruct pattern, Expression initializer = null)
            : base(pattern.StartLocation, initializer == null ? pattern.EndLocation : initializer.EndLocation)
        {
            Pattern = pattern;
            Initializer = initializer ?? Expression.Null;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitVariableInitializer(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitVariableInitializer(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitVariableInitializer(this, data);
        }

        internal protected override bool DoMatch(AstNode other, Match match)
        {
            var o = other as VariableInitializer;
            return o != null && Pattern.DoMatch(o.Pattern, match)
                && Initializer.DoMatch(o.Initializer, match);
        }
    }
}

