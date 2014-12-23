using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a parameter(which appear in function signatures).
    /// </summary>
    public class ParameterDeclaration : AstNode
    {
        public override NodeType NodeType{
            get{
                return NodeType.Unknown;
            }
        }

        /// <summary>
        /// この引数の型。
        /// The type of the argument.
        /// </summary>
        public AstType Type{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        /// <summary>
        /// この引数の名前。
        /// The name of the argument.
        /// </summary>
        public string Name{
            get{return GetChildByRole(Roles.Identifier).Name;}
        }

        /// <summary>
        /// この引数の名前。
        /// The name of the parameter.
        /// </summary>
        /// <value>The name token.</value>
        public Identifier NameToken{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public ExpressoTokenNode AssignToken{
            get{return GetChildByRole(Roles.AssignToken);}
        }

        /// <summary>
        /// この引数のデフォルト値。
        /// The optional value for this argument. It would be null if none is specified.
        /// </summary>
        public Expression Option{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        public ParameterDeclaration(string name, AstType type, Expression option = null)
        {
            NameToken = AstNode.MakeIdentifier(name);
            ParamType = type;
            Option = option ?? Expression.Null;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitParameterDeclaration(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitParameterDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitParameterDeclaration(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ParameterDeclaration;
            return o != null && Type.DoMatch(o.Type, match) && Name == o.Name
                && Option.DoMatch(o.Option, match);
        }
    }
}

