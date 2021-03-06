﻿using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// A KeyValueLikeExpression represents a key-value-pair-like expression.
    /// It is used for items in ObjectCreationExpressions, named arguments or dictionary literals.
    /// Expression ':' Expression ;
    /// </summary>
    public class KeyValueLikeExpression : Expression
    {
        public static readonly Role<Expression> KeyRole = new Role<Expression>("Key", Null);

        /// <summary>
        /// Represents the key.
        /// </summary>
        public Expression KeyExpression{
            get => GetChildByRole(KeyRole);
            set => SetChildByRole(KeyRole, value);
        }

        public AstNodeCollection<ExpressoTokenNode> ColonToken => GetChildrenByRole(Roles.ColonToken);

        /// <summary>
        /// Represents the value.
        /// </summary>
        public Expression ValueExpression{
            get => GetChildByRole(Roles.Expression);
            set => SetChildByRole(Roles.Expression, value);
        }

        public KeyValueLikeExpression(Expression key, Expression value)
            : base(key.StartLocation, value.EndLocation)
        {
            KeyExpression = key;
            ValueExpression = value;
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

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as KeyValueLikeExpression;
            return o != null && KeyExpression.DoMatch(o.KeyExpression, match) && ValueExpression.DoMatch(o.ValueExpression, match);
        }

        #endregion
    }
}

