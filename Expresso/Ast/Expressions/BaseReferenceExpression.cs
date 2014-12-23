﻿using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the keyword "base" as expression.
    /// </summary>
    public class BaseReferenceExpression : Expression
    {
        public BaseReferenceExpression(TextLocation start)
        {
            start_loc = start;
            end_loc = new TextLocation(start.Line, start.Column + "base".Length);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            throw new NotImplementedException();
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            throw new NotImplementedException();
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            throw new NotImplementedException();
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            return other is BaseReferenceExpression;
        }

        #endregion
    }
}

