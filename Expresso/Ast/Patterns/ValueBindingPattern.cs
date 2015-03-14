using System;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a value binding pattern.
    /// ( "let" | "var" ) PatternConstruct ;
    /// </summary>
    public class ValueBindingPattern : PatternConstruct
    {
        /// <summary>
        /// Represents the inner pattern.
        /// </summary>
        public PatternConstruct Pattern{
            get{return GetChildByRole(Roles.Pattern);}
            set{SetChildByRole(Roles.Pattern, value);}
        }

        /// <summary>
        /// Represents the modifiers that describe the properties of inner patterns.
        /// </summary>
        public Modifiers Modifiers{
            get{return EntityDeclaration.GetModifiers(this);}
            set{EntityDeclaration.SetModifiers(this, value);}
        }

        public ValueBindingPattern(PatternConstruct inner, Modifiers modifiers)
        {
            Pattern = inner;
            Modifiers = modifiers;
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
            return o != null && Pattern.DoMatch(o.Pattern, match)
                && Modifiers == o.Modifiers;
        }

        #endregion
    }
}

