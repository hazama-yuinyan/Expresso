﻿using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// An <see ref="Expresso.Ast.KeyValueLikeExpression"> represents a key-value-pair-like expression.
    /// It is used for items in ObjectCreationExpressions, named arguments or dictionary literals.
    /// </summary>
    public class KeyValueLikeExpression : Expression
    {
        public static readonly Role<Expression> KeyRole = new Role<Expression>("Key");

        public Expression KeyExpression{
            get{return GetChildByRole(KeyRole);}
            set{SetChildByRole(KeyRole, value);}
        }

        public AstNodeCollection<ExpressoTokenNode> ColonToken{
            get{return GetChildrenByRole(Roles.ColonToken);}
        }

        public Expression Value{
            get{return GetChildByRole(Roles.Expression);}
            set{SetChildByRole(Roles.Expression, value);}
        }

        public KeyValueLikeExpression(Expression key, Expression value)
        {
            KeyExpression = key;
            Value = value;
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitKeyValueLikeExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitKeyValueLikeExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitKeyValueLikeExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as KeyValueLikeExpression;
            return o != null && KeyExpression.DoMatch(o.KeyExpression, match)
                && Value.DoMatch(o.Value, match);
        }

        #endregion
    }
}

