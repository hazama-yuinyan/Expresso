using System;
using ICSharpCode.NRefactory;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a constructor declaration.
    /// </summary>
    public class ConstructorDeclaration : EntityDeclaration
    {
        public static readonly Role<ConstructorInitializer> InitializerRole = new Role<ConstructorInitializer>("Initializer", ConstructorInitializer.Null);

        public override ICSharpCode.NRefactory.TypeSystem.SymbolKind SymbolKind{
            get{
                return SymbolKind.Constructor;
            }
        }

        public ExpressoTokenNode LParenthesis{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

        public AstNodeCollection<ParameterDeclaration> Parameters{
            get{return GetChildrenByRole(Roles.Parameter);}
        }

        public ExpressoTokenNode RParenthesis{
            get{return GetChildByRole(Roles.RParenthesisToken);}
        }

        public ExpressoTokenNode ColonToken{
            get{return GetChildByRole(Roles.ColonToken);}
        }

        public ConstructorInitializer Initializer{
            get{return GetChildByRole(InitializerRole);}
        }

        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
        }

        public ConstructorDeclaration(IEnumerable<ParameterDeclaration> parameters,
            BlockStatement body, ConstructorInitializer initializer)
        {
            foreach(var param in parameters)
                AddChild(param, Roles.Parameter);

            SetChildByRole(Roles.Body, body);
            SetChildByRole(Initializer, initializer ?? ConstructorInitializer.Null);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitConstructorDeclaration(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitConstructorDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitConstructorDeclaration(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ConstructorDeclaration;
            return o != null && Parameters.DoMatch(o.Parameters, match)
                && Initializer.DoMatch(o.Initializer, match) && Body.DoMatch(o.Body, match);
        }
    }

    public enum ConstructorInitializerType
    {
        Any,
        Base,
        This
    }

    public class ConstructorInitializer : AstNode
    {
        public static readonly TokenRole BaseKeywordRole = new TokenRole("base");
        public static readonly TokenRole ThisKeywordRole = new TokenRole("this");

        #region Null
        public static readonly new ConstructorInitializer Null = new NullConstructorInitializer();
        sealed class NullConstructorInitializer : ConstructorInitializer
        {
            public override NodeType NodeType{
                get{
                    return NodeType.Unknown;
                }
            }

            public override bool IsNull{
                get{
                    return true;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitNullNode(this, data);
            }

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        public override NodeType NodeType{
            get{
                return NodeType.Unknown;
            }
        }

        public ConstructorInitializerType InitializerType{
            get; private set;
        }

        public ExpressoTokenNode Keyword{
            get{
                if(InitializerType == ConstructorInitializerType.Base)
                    return GetChildByRole(BaseKeywordRole);
                else
                    return GetChildByRole(ThisKeywordRole);
            }
        }

        public ExpressoTokenNode LParenthesis{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

        public AstNodeCollection<Expression> Arguments{
            get{return GetChildrenByRole(Roles.Expression);}
        }

        public ExpressoTokenNode RParenthesis{
            get{return GetChildByRole(Roles.RParenthesisToken);}
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitConstructorInitializer(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitConstructorInitializer(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitConstructorInitializer(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ConstructorInitializer;
            return o != null && (InitializerType == ConstructorInitializerType.Any || InitializerType == o.InitializerType)
                && Arguments.DoMatch(o.Arguments, match);
        }
    }
}

