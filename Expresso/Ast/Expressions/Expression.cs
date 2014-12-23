using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory;
using System;

namespace Expresso.Ast
{
    /// <summary>
    /// 式の共通基底。
	/// Base class for all expressions.
    /// </summary>
    public abstract class Expression : AstNode
    {
        #region Null
        public static new readonly Expression Null = new NullExpression();

        sealed class NullExpression : Expression
        {
            public override bool IsNull{
                get{
                    return true;
                }
            }

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

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        #region Pattern Placeholder
        public static implicit operator Expression(Pattern pattern)
        {
            return (pattern != null) ? new PatternPlaceholder(pattern) : null;
        }

        sealed class PatternPlaceholder : Expression, INode
        {
            readonly Pattern child;

            public PatternPlaceholder(Pattern child)
            {
                this.child = child;
            }

            public override NodeType NodeType{
                get{
                    return NodeType.Pattern;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitPatternPlaceholder(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitPatternPlaceholder(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitPatternPlaceholder(this, data);
            }

            internal protected override bool DoMatch(AstNode other, Match match)
            {
                return child.DoMatch(other, match);
            }

            bool INode.DoMatchCollection(Role role, INode pos, Match match, BacktrackingInfo backtrackingInfo)
            {
                return child.DoMatchCollection(role, pos, match, backtrackingInfo);
            }
        }
        #endregion

        public override NodeType NodeType{
            get{
                return NodeType.Expression;
            }
        }

        public new Expression Clone()
        {
            return (Expression)base.Clone();
        }

        public Expression ReplaceWith(Func<Expression, Expression> replaceFunction)
        {
            if(replaceFunction == null)
                throw new ArgumentNullException("replaceFunction");

            return (Expression)base.ReplaceWith(node => replaceFunction((Expression)node));
        }
    }

    /// <summary>
    /// 演算子のタイプ。
    /// </summary>
    public enum OperatorType
    {
		None,
        Assign,
        Plus,
        Minus,
        Times,
        Divide,
		Power,
		Modulus,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Equality,
        InEquality,
		Not,
        ConditionalAnd,
        ConditionalOr,
		BitwiseOr,
		BitwiseAnd,
		ExclusiveOr,
		BitwiseShiftLeft,
		BitwiseShiftRight,
        Is,
        /// <summary>
        /// Any operator(for pattern matching).
        /// </summary>
        Any
    }
}
