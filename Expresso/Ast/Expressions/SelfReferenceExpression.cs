﻿using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the keyword "self" as expression.
    /// The self keyword references the class object or the module object that are in scope.
    /// "self" ;
    /// </summary>
    public class SelfReferenceExpression : Expression
    {
        /// <summary>
        /// Gets the identifier that represents the "self" keyword.
        /// </summary>
        public Identifier SelfIdentifier{
            get{return GetChildByRole(Roles.Identifier);}
            private set{SetChildByRole(Roles.Identifier, value);}
        }

        public SelfReferenceExpression(TextLocation loc)
            : base(loc, new TextLocation(loc.Line, loc.Column + "self".Length))
        {
            SelfIdentifier = AstNode.MakeIdentifier("self");
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitSelfReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSelfReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitSelfReferenceExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            return other is SelfReferenceExpression;
        }

        #endregion
    }
}

