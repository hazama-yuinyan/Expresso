using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a pattern with a type annotation.
    /// Pattern [ "(-" Type ] ;
    /// </summary>
    public class PatternWithType : PatternConstruct
    {
        #region Null
        public static readonly new PatternWithType Null = new NullPatternWithType();

        sealed class NullPatternWithType : PatternWithType
        {
            public override bool IsNull => true;

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

            protected internal override bool DoMatch (AstNode other, Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        public PatternConstruct Pattern{
            get{return GetChildByRole(Roles.Pattern);}
            set{SetChildByRole(Roles.Pattern, value);}
        }

        public AstType Type{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        protected PatternWithType()
        {
        }

        public PatternWithType(PatternConstruct pattern, AstType type)
            : base(pattern.StartLocation, (type == null) ? pattern.EndLocation : type.EndLocation)
        {
            Pattern = pattern;
            Type = type;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitPatternWithType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitPatternWithType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitPatternWithType(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as PatternWithType;
            return o != null && Pattern.DoMatch(o.Pattern, match) && Type.DoMatch(o.Type, match);
        }
    }
}
