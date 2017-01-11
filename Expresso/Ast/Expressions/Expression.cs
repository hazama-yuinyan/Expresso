﻿using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory;
using System;
using System.Collections.Generic;
using System.Linq;

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
                walker.VisitPatternPlaceholder(this, child);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitPatternPlaceholder(this, child);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitPatternPlaceholder(this, child, data);
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

        protected Expression()
        {
        }

        protected Expression(TextLocation start, TextLocation end)
            : base(start, end)
        {
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
        /// <summary>
        /// Makes a simple assignment expression.
        /// </summary>
        /// <returns>An assignment expression.</returns>
        /// <param name="lhs">The left-hand-side expression(single item)</param>
        /// <param name="rhs">The right-hand-side expression(single item)</param>
        public static AssignmentExpression MakeSingleAssignment(Expression lhs, Expression rhs)
        {
            return new AssignmentExpression(Expression.MakeSequenceExpression(lhs), Expression.MakeSequenceExpression(rhs), OperatorType.Assign);
        }

        /// <summary>
        /// Makes a simple augmented assignment.
        /// </summary>
        /// <returns>An assignment expression.</returns>
        /// <param name="opType">The type of the operator.</param>
        /// <param name="lhs">The left-hand-side expression(single item).</param>
        /// <param name="rhs">The right-hand-side expression(single item).</param>
        public static AssignmentExpression MakeSingleAugmentedAssignment(OperatorType opType, Expression lhs, Expression rhs)
        {
            return new AssignmentExpression(Expression.MakeSequenceExpression(lhs), Expression.MakeSequenceExpression(rhs), opType);
        }

        /// <summary>
        /// Makes a simuletanous assignment expression.
        /// </summary>
        /// <returns>An assignment.</returns>
        /// <param name="lhs">The left-hand-side expressions(multiple items).</param>
        /// <param name="rhs">The right-hand-side expressions(multiple items).</param>
        public static AssignmentExpression MakeAssignment(SequenceExpression lhs, SequenceExpression rhs)
        {
            return new AssignmentExpression(lhs, rhs, OperatorType.Assign);
        }

        /// <summary>
        /// Makes a simuletanous augumented assignment
        /// </summary>
        /// <param name="opType">The type of the operator.</param>
        /// <param name="lhs">The left-hand-side expressions(multiple items).</param>
        /// <param name="rhs">The right-hand-side expressions(multiple items).</param>
        public static AssignmentExpression MakeAugumentedAssignment(OperatorType opType, SequenceExpression lhs, SequenceExpression rhs)
        {
            return new AssignmentExpression(lhs, rhs, opType);
        }

        /// <summary>
        /// Makes a complex assignment expression.
        /// </summary>
        /// <returns>An assignment.</returns>
        /// <param name="lhs">The left hand side expression(can be an assignment expression).</param>
        /// <param name="rhs">The right hand side expression(single item).</param>
        public static AssignmentExpression MakeMultipleAssignment(AssignmentExpression lhs, SequenceExpression rhs)
        {
            return new AssignmentExpression(lhs, rhs, OperatorType.Assign);
        }

        /// <summary>
        /// Makes a call expression.
        /// </summary>
        /// <returns>A call expression.</returns>
        /// <param name="target">The target expression to call. It has to be a callable object.</param>
        /// <param name="args">Arguments.</param>
        public static CallExpression MakeCallExpr(Expression target, IEnumerable<Expression> args, TextLocation loc = default(TextLocation))
        {
            return new CallExpression(target, args, loc);
        }

        /// <summary>
        /// Makes a call expression.(handy params version)
        /// </summary>
        /// <returns>A call expression.</returns>
        /// <param name="target">The target expression to call. It has to be a callable object.</param>
        /// <param name="args">Arguments.</param>
        public static CallExpression MakeCallExpr(Expression target, TextLocation loc, params Expression[] args)
        {
            return new CallExpression(target, args, loc);
        }

        /// <summary>
        /// Makes a sequence expression.
        /// </summary>
        /// <returns>A sequence expression.</returns>
        public static SequenceExpression MakeSequenceExpression(IEnumerable<Expression> items)
        {
            return new SequenceExpression(items);
        }

        /// <summary>
        /// Makes a sequence expression from a sequence of arguments.
        /// </summary>
        /// <returns>A sequence expression.</returns>
        public static SequenceExpression MakeSequenceExpression(params Expression[] items)
        {
            return new SequenceExpression(items);
        }

        /// <summary>
        /// Makes a unary expression.
        /// </summary>
        /// <returns>An expression.</returns>
        /// <param name="op">The operator type.</param>
        /// <param name="operand">The operand.</param>
        public static UnaryExpression MakeUnaryExpr(OperatorType op, Expression operand, TextLocation loc = default(TextLocation))
        {
            return new UnaryExpression(op, operand, loc);
        }

        /// <summary>
        /// Makes a binary expression.
        /// </summary>
        /// <returns>The binary expr.</returns>
        /// <param name="op">The operator type.</param>
        /// <param name="lhs">The left hand side expression.</param>
        /// <param name="rhs">The right hand side expression.</param>
        public static BinaryExpression MakeBinaryExpr(OperatorType op, Expression lhs, Expression rhs)
        {
            return new BinaryExpression(lhs, rhs, op);
        }

        public static SequenceInitializer MakeSequenceInitializer(SimpleType type, IEnumerable<Expression> initializers)
        {
            return new SequenceInitializer(type, initializers);
        }

        public static SequenceInitializer MakeSequenceInitializer(SimpleType type, params Expression[] initializers)
        {
            return new SequenceInitializer(type, initializers);
        }

        public static ObjectCreationExpression MakeObjectCreation(AstType path, IEnumerable<Identifier> names,
            IEnumerable<Expression> values, TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new ObjectCreationExpression(path, names.Select(ident => new PathExpression(ident)), values, start, end);
        }

        public static KeyValueLikeExpression MakeKeyValuePair(Expression key, Expression value)
        {
            return new KeyValueLikeExpression(key, value);
        }

        public static CastExpression MakeCastExpr(Expression target, AstType toType)
        {
            return new CastExpression(toType, target);
        }

        public static ConditionalExpression MakeCondExpr(Expression test, Expression trueExpr, Expression falseExpr)
        {
            return new ConditionalExpression(test, trueExpr, falseExpr);
        }

        public static ComprehensionExpression MakeComp(Expression yieldExpr, ComprehensionForClause body,
            SimpleType objType)
        {
            return new ComprehensionExpression(yieldExpr, body, objType);
        }

        public static ComprehensionForClause MakeCompFor(PatternConstruct left, Expression target, ComprehensionIter body)
        {
            return new ComprehensionForClause(left, target, body);
        }

        public static ComprehensionIfClause MakeCompIf(Expression condition, ComprehensionIter body)
        {
            return new ComprehensionIfClause(condition, body);
        }

        public static LiteralExpression MakeConstant(string typeName, object val,
            TextLocation loc = default(TextLocation))
        {
            return new LiteralExpression(val, new PrimitiveType(typeName, TextLocation.Empty), loc);
        }

        public static SelfReferenceExpression MakeSelfRef(TextLocation start = default(TextLocation))
        {
            return new SelfReferenceExpression(start);
        }

        public static SuperReferenceExpression MakeSuperRef(TextLocation start = default(TextLocation))
        {
            return new SuperReferenceExpression(start);
        }

        public static MemberReferenceExpression MakeMemRef(Expression target, Identifier subscript)
        {
            return new MemberReferenceExpression(target, subscript);
        }

        public static IntegerSequenceExpression MakeIntSeq(Expression start, Expression end, Expression step, bool upperInclusive)
        {
            return new IntegerSequenceExpression(start, end, step, upperInclusive);
        }

        public static NewExpression MakeNewExpr(ObjectCreationExpression creationExpr, TextLocation loc = default(TextLocation))
        {
            return new NewExpression(creationExpr, loc);
        }

        /// <summary>
        /// Makes a path expression from a sequence of identifiers.
        /// </summary>
        /// <returns>A path expression.</returns>
        /// <param name="paths">Path items.</param>
        public static PathExpression MakePath(IEnumerable<Identifier> paths)
        {
            return new PathExpression(paths);
        }

        /// <summary>
        /// Makes a path expression from a sequence of identifiers.(handy params version)
        /// </summary>
        /// <returns>A path expression.</returns>
        /// <param name="paths">Path items.</param>
        public static PathExpression MakePath(params Identifier[] paths)
        {
            return new PathExpression(paths);
        }

        public static ParenthesizedExpression MakeParen(Expression expr)
        {
            return new ParenthesizedExpression(expr);
        }

        public static IndexerExpression MakeIndexer(Expression target, IEnumerable<Expression> args, TextLocation loc = default(TextLocation))
        {
            return new IndexerExpression(target, args, loc);
        }

        public static IndexerExpression MakeIndexer(Expression target, TextLocation loc, params Expression[] args)
        {
            return new IndexerExpression(target, args, loc);
        }
        #endregion
    }
}
