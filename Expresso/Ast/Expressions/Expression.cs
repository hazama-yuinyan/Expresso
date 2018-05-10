using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory;
using System;
using System.Collections.Generic;
using System.Linq;
using Expresso.Ast.Analysis;

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
            public override bool IsNull => true;

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

            public override NodeType NodeType => NodeType.Pattern;

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

        public override NodeType NodeType => NodeType.Expression;

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
                throw new ArgumentNullException(nameof(replaceFunction));

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
            return new AssignmentExpression(MakeSequenceExpression(lhs), MakeSequenceExpression(rhs), OperatorType.Assign);
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
            return new AssignmentExpression(MakeSequenceExpression(lhs), MakeSequenceExpression(rhs), opType);
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
        public static CallExpression MakeCallExpr(Expression target, IEnumerable<Expression> args, TextLocation loc = default)
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
        public static UnaryExpression MakeUnaryExpr(OperatorType op, Expression operand, TextLocation loc = default)
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

        public static SequenceInitializer MakeSequenceInitializer(SimpleType type, IEnumerable<Expression> initializers, TextLocation start = default,
                                                                  TextLocation end = default)
        {
            return new SequenceInitializer(type, initializers, start, end);
        }

        public static SequenceInitializer MakeSequenceInitializer(SimpleType type, TextLocation start, TextLocation end, params Expression[] initializers)
        {
            return new SequenceInitializer(type, initializers, start, end);
        }

        public static ObjectCreationExpression MakeObjectCreation(AstType path, IEnumerable<Identifier> names,
            IEnumerable<Expression> values, TextLocation start = default, TextLocation end = default)
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
            TextLocation loc = default)
        {
            return new LiteralExpression(val, new PrimitiveType(typeName, TextLocation.Empty), loc);
        }

        public static SelfReferenceExpression MakeSelfRef(TextLocation start = default)
        {
            return new SelfReferenceExpression(start);
        }

        public static SuperReferenceExpression MakeSuperRef(TextLocation start = default)
        {
            return new SuperReferenceExpression(start);
        }

        public static NullReferenceExpression MakeNullRef(TextLocation loc = default)
        {
            return new NullReferenceExpression(loc);
        }

        public static MemberReferenceExpression MakeMemRef(Expression target, Identifier subscript)
        {
            return new MemberReferenceExpression(target, subscript);
        }

        public static IntegerSequenceExpression MakeIntSeq(Expression start, Expression end, Expression step, bool upperInclusive,
                                                           TextLocation startLoc = default, TextLocation endLoc = default)
        {
            if(start is LiteralExpression literal1 && literal1.Value is double){
                throw new ParserException(
                    "An intseq expression can't handle numbers outside of the int range.",
                    "ES4002",
                    start
                ){
                    HelpObject = literal1.Value
                };
            }
            if(end is LiteralExpression literal2 && literal2.Value is double){
                throw new ParserException(
                    "An intseq expression can't handle numbers outside of the int range.",
                    "ES4002",
                    end
                ){
                    HelpObject = literal2.Value
                };
            }
            if(step is LiteralExpression literal3 && literal3.Value is double){
                throw new ParserException(
                    "An intseq expression can't handle numbers outside of the int range.",
                    "ES4002",
                    step
                ){
                    HelpObject = literal3.Value
                };
            }

            return new IntegerSequenceExpression(start, end, step, upperInclusive, startLoc, endLoc);
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

        public static ParenthesizedExpression MakeParen(Expression expr, TextLocation start = default, TextLocation end = default)
        {
            return new ParenthesizedExpression(expr, start, end);
        }

        public static IndexerExpression MakeIndexer(Expression target, IEnumerable<Expression> args, TextLocation loc = default)
        {
            return new IndexerExpression(target, args, loc);
        }

        public static IndexerExpression MakeIndexer(Expression target, TextLocation loc, params Expression[] args)
        {
            return new IndexerExpression(target, args, loc);
        }

        public static ClosureLiteralExpression MakeClosureExpression(IEnumerable<ParameterDeclaration> parameters, AstType returnType,
                                                                     BlockStatement body, TextLocation loc = default, List<Identifier> liftedIdentifiers = null)
        {
            return new ClosureLiteralExpression(parameters, returnType, body, loc, liftedIdentifiers);
        }

        public static ClosureLiteralExpression MakeClosureExpression(AstType returnType, BlockStatement body,
                                                                     TextLocation loc = default, List<Identifier> liftedIdentifiers = null,
                                                                     params ParameterDeclaration[] parameters)
        {
            return new ClosureLiteralExpression(parameters, returnType, body, loc, liftedIdentifiers);
        }

        public static bool IsNullNode(Expression expr)
        {
            return expr == null || expr.IsNull;
        }
        #endregion
    }
}
