using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a variable initialization.
    /// </summary>
    public class VariableInitializer : AstNode
    {
        public override NodeType NodeType{
            get{
                return NodeType.Unknown;
            }
        }

        public string Name{
            get{return GetChildByRole(Roles.Identifier).Name;}
        }

        public Identifier NameToken{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public ExpressoTokenNode AssignToken{
            get{return GetChildByRole(Roles.AssignToken);}
        }

        public Expression Initializer{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        public VariableInitializer(Identifier name, Expression initializer = null)
        {
            NameToken = name;
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
            return o != null && MatchString(Name, o.Name) && Initializer.DoMatch(o.Initializer, match);
        }
    }
}

