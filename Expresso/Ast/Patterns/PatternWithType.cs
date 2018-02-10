using System;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a pattern with a type annotation.
    /// Pattern [ "(-" Type ] ;
    /// </summary>
    public class PatternWithType : PatternConstruct
    {
        public PatternConstruct Pattern{
            get{return GetChildByRole(Roles.Pattern);}
            set{SetChildByRole(Roles.Pattern, value);}
        }

        public AstType Type{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        public PatternWithType(PatternConstruct pattern, AstType type)
            : base(pattern.StartLocation, type.EndLocation)
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
