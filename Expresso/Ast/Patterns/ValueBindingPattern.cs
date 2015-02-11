using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a value binding pattern.
    /// ( "let" | "var" ) PatternConstruct ;
    /// </summary>
    public class ValueBindingPattern : PatternConstruct
    {
        public bool IsConst{
            get; set;
        }

        /// <summary>
        /// Represents the inner pattern.
        /// </summary>
        public PatternConstruct Pattern{
            get{return GetChildByRole(Roles.Pattern);}
            set{SetChildByRole(Roles.Pattern, value);}
        }

        public ValueBindingPattern(PatternConstruct inner, bool isConst)
        {
            Pattern = inner;
            IsConst = isConst;
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitValueBindingPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitValueBindingPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitValueBindingPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ValueBindingPattern;
            return o != null && Pattern.DoMatch(o.Pattern, match);
        }

        #endregion
    }
}

