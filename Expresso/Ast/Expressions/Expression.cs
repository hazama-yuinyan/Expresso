using ICSharpCode.NRefactory.PatternMatching;
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
        public static CallExpression MakeCallExpr(Expression target, IEnumerable<Expression> args)
        {
            return new CallExpression(target, args);
        }

        public static CallExpression MakeCallExpr(Expression target, params Expression[] args)
        {
            return new CallExpression(target, args);
        }

        public static SequenceExpression MakeSequence(IEnumerable<Expression> items)
        {
            return new SequenceExpression(items);
        }

        public static SequenceExpression MakeSequence(params Expression[] items)
        {
            return new SequenceExpression(items);
        }

        public static UnaryExpression MakeUnaryExpr(OperatorType op, Expression operand)
        {
            return new UnaryExpression(op, operand);
        }

        public static BinaryExpression MakeBinaryExpr(OperatorType op, Expression lhs, Expression rhs)
        {
            return new BinaryExpression(lhs, rhs, op);
        }

        public static SequenceInitializer MakeSeqInitializer(AstType type, IEnumerable<Expression> initializeList)
        {
            return new SequenceInitializer(type, initializeList);
        }

        public static SequenceInitializer MakeSeqInitializer(AstType type, params Expression[] initializers)
        {
            return new SequenceInitializer(type, initializers);
        }

        public static ObjectCreationExpression MakeObjectCreation(PathExpression path, IEnumerable<Identifier> names,
            IEnumerable<Expression> values)
        {
            return new ObjectCreationExpression(path, names.Select(ident => new PathExpression(ident)),
                values);
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
            AstType objType)
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

        public static SelfReferenceExpression MakeSelfRef(TextLocation start)
        {
            return new SelfReferenceExpression(start);
        }

        public static SuperReferenceExpression MakeSuperRef(TextLocation start)
        {
            return new SuperReferenceExpression(start);
        }

        public static MemberReference MakeMemRef(Expression target, Identifier subscript)
        {
            return new MemberReference(target, subscript);
        }

        public static IntegerSequenceExpression MakeIntSeq(Expression start, Expression end, Expression step, bool upperInclusive)
        {
            return new IntegerSequenceExpression(start, end, step, upperInclusive);
        }

        public static NewExpression MakeNewExpr(ObjectCreationExpression creationExpr)
        {
            return new NewExpression(creationExpr);
        }

        public static PathExpression MakePath(IEnumerable<Identifier> paths)
        {
            return new PathExpression(paths);
        }

        public static PathExpression MakePath(params Identifier[] paths)
        {
            return new PathExpression(paths);
        }

        public static ParenthesizedExpression MakeParen(Expression expr)
        {
            return new ParenthesizedExpression(expr);
        }

        public static IndexerExpression MakeIndexer(Expression target, IEnumerable<Expression> args)
        {
            return new IndexerExpression(target, args);
        }

        public static IndexerExpression MakeIndexer(Expression target, params Expression[] args)
        {
            return new IndexerExpression(target, args);
        }
        #endregion
    }
}
