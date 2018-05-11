using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents the throw statement.
    /// A throw statement transfers control to a surrounding catch clause and
    /// gives it an exception object.
    /// "throw" Type ObjectCreation ';' ;
    /// </summary>
    public class ThrowStatement : Statement
    {
        public static readonly Role<ObjectCreationExpression> ObjectCreationRole =
            new Role<ObjectCreationExpression>("ObjectCreation", ObjectCreationExpression.Null);

        /// <summary>
        /// オブジェクトを生成するクラスの定義を参照する式。
        /// The target object to be thrown.
        /// </summary>
        public ObjectCreationExpression CreationExpression{
            get => GetChildByRole(ObjectCreationRole);
            set => SetChildByRole(ObjectCreationRole, value);
        }

        public ThrowStatement(ObjectCreationExpression expr, TextLocation loc)
            : base(loc, expr.EndLocation)
        {
            CreationExpression = expr;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitThrowStatement(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitThrowStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitThrowStatement(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ThrowStatement;
            return o != null && CreationExpression.DoMatch(o.CreationExpression, match);
        }
    }
}
