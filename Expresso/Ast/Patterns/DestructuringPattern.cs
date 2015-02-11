using System;
using System.Collections.Generic;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a destructuring pattern.
    /// </summary>
    public class DestructuringPattern : PatternConstruct
    {
        public PathExpression Path{
            get{return GetChildByRole(Roles.Path);}
            set{SetChildByRole(Roles.Path, value);}
        }

        public AstNodeCollection<PatternConstruct> Items{
            get{return GetChildrenByRole(Roles.Pattern);}
        }

        public DestructuringPattern(PathExpression path, IEnumerable<PatternConstruct> patterns)
        {
            Path = path;
            foreach(var item in patterns)
                AddChild(item, Roles.Pattern);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitDestructuringPattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitDestructuringPattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitDestructuringPattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as DestructuringPattern;
            return o != null && Path.DoMatch(o.Path, match) && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}

