using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory;
using System;
using System.Collections.Generic;

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

        #region Factory methods
        static internal CallExpression MakeCallExpr(Expression target, IEnumerable<Expression> args)
        {
            return new CallExpression(target, args);
        }

        static internal SequenceExpression MakeSequence(IEnumerable<Expression> items)
        {
            return new SequenceExpression(items);
        }

        static internal UnaryExpression MakeUnaryExpr(OperatorType op, Expression operand)
        {
            return new UnaryExpression(op, operand);
        }

        static internal BinaryExpression MakeBinaryExpr(OperatorType op, Expression lhs, Expression rhs)
        {
            return new BinaryExpression(lhs, rhs, op);
        }

        static internal SequenceInitializer MakeSeqInitializer(AstType type, IEnumerable<Expression> initializeList)
        {
            return new SequenceInitializer(initializeList, type);
        }

        static internal AssignmentExpression MakeAssignment(IEnumerable<Expression> targets, SequenceExpression rhs)
        {
            return new AssignmentExpression(MakeSequence(targets), rhs);
        }

        static internal ObjectCreationExpression MakeObjectCreation(IEnumerable<Identifier> names,
            IEnumerable<Expression> values)
        {
            return new ObjectCreationExpression(names, values);
        }

        static internal AssignmentExpression MakeAugumentedAssignment(SequenceExpression targets, SequenceExpression rhs, OperatorType opType)
        {
            return new AssignmentExpression(targets, rhs, opType);
        }

        static internal CastExpression MakeCastExpr(Expression target, Expression toExpr)
        {
            return new CastExpression(toExpr, target);
        }

        static internal ConditionalExpression MakeCondExpr(Expression test, Expression trueExpr, Expression falseExpr)
        {
            return new ConditionalExpression(test, trueExpr, falseExpr);
        }

        static internal ComprehensionExpression MakeComp(Expression yieldExpr, ComprehensionForClause body, AstType objType)
        {
            return new ComprehensionExpression(yieldExpr, body, objType);
        }

        static internal ComprehensionForClause MakeCompFor(SequenceExpression left, Expression target, ComprehensionIter body)
        {
            return new ComprehensionForClause(left, target, body);
        }

        static internal ComprehensionIfClause MakeCompIf(Expression condition, ComprehensionIter body)
        {
            return new ComprehensionIfClause(condition, body);
        }

        static internal LiteralExpression MakeConstant(AstType type, object val)
        {
            return new LiteralExpression(val, type);
        }

        static internal SelfReferenceExpression MakeSelfRef(TextLocation start)
        {
            return new SelfReferenceExpression(start);
        }

        static internal SuperReferenceExpression MakeSuperRef(TextLocation start)
        {
            return new SuperReferenceExpression(start);
        }

        static internal MemberReference MakeMemRef(Expression parent, Expression child)
        {
            return new MemberReference(parent, child);
        }

        static internal IntegerSequenceExpression MakeIntSeq(Expression start, Expression end, Expression step, bool upperInclusive)
        {
            return new IntegerSequenceExpression(start, end, step, upperInclusive);
        }

        static internal NewExpression MakeNewExpr(ObjectCreationExpression creationExpr)
        {
            return new NewExpression(creationExpr);
        }
        #endregion
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
        As,
        /// <summary>
        /// Any operator(for pattern matching).
        /// </summary>
        Any
    }
}
