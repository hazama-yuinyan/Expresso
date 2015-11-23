using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
    /// <summary>
    /// 文の共通基底。
    /// Base class for all the statements.
    /// </summary>
    public abstract class Statement : AstNode
    {
        #region Null
        public static new Statement Null = new NullStatement();

        sealed class NullStatement : Statement
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

            protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        #region PatternPlaceholder
        public static implicit operator Statement(Pattern pattern)
        {
            return (pattern != null) ? new PatternPlaceholder(pattern) : null;
        }

        sealed class PatternPlaceholder : Statement, INode
        {
            readonly Pattern child;

            public PatternPlaceholder(Pattern child)
            {
                this.child = child;
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

            protected internal override bool DoMatch(AstNode other, Match match)
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
                return NodeType.Statement;
            }
        }

        protected Statement()
        {
        }

        protected Statement(TextLocation start, TextLocation end)
            : base(start, end)
        {
        }

        public new Statement Clone()
        {
            return (Statement)base.Clone();
        }

        public Statement ReplaceWith(Func<Statement, Statement> replaceFunction)
        {
            if(replaceFunction == null)
                throw new ArgumentNullException("replaceFunction");

            return (Statement)base.ReplaceWith(node => replaceFunction((Statement)node));
        }

        #region Factory methods
        public static ExpressionStatement MakeSingleAssignment(SequenceExpression lhs, SequenceExpression rhs,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return MakeExprStmt(Expression.MakeSingleAssignment(lhs, rhs), start, end);
        }

        public static ExpressionStatement MakeSingleAugmentedAssignment(OperatorType opType, SequenceExpression lhs, SequenceExpression rhs,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return MakeExprStmt(Expression.MakeSingleAugmentedAssignment(opType, lhs, rhs), start, end);
        }

        public static ExpressionStatement MakeAugmentedAssignment(OperatorType opType, SequenceExpression targets,
            SequenceExpression rhs, TextLocation start = default(TextLocation),
            TextLocation end = default(TextLocation))
        {
            return MakeExprStmt(Expression.MakeAugumentedAssignment(opType, targets, rhs), start, end);
        }

        public static BreakStatement MakeBreakStmt(LiteralExpression count,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new BreakStatement(count, start, end);
        }

        public static ContinueStatement MakeContinueStmt(LiteralExpression count,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new ContinueStatement(count, start, end);
        }

        public static BlockStatement MakeBlock(IEnumerable<Statement> stmts,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new BlockStatement(stmts, start, end);
        }

        public static BlockStatement MakeBlock(params Statement[] stmts)
        {
            return new BlockStatement(stmts, stmts[0].StartLocation, stmts[stmts.Length - 1].EndLocation);
        }

        public static VariableDeclarationStatement MakeVarDecl(IEnumerable<Identifier> lhs, IEnumerable<Expression> rhs,
            Modifiers modifiers, TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new VariableDeclarationStatement(lhs, rhs, modifiers, start, end);
        }

        public static ExpressionStatement MakeExprStmt(Expression expr,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new ExpressionStatement(Expression.MakeSequence(expr), start, end);
        }

        public static ExpressionStatement MakeExprStmt(SequenceExpression seqExpr,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new ExpressionStatement(seqExpr, start, end);
        }

        public static ReturnStatement MakeReturnStmt(Expression expr)
        {
            return new ReturnStatement(expr);
        }

        public static IfStatement MakeIfStmt(PatternConstruct condition, BlockStatement trueBlock,
            BlockStatement falseBlock, TextLocation loc = default(TextLocation))
        {
            return new IfStatement(condition, trueBlock, falseBlock, loc);
        }

        public static WhileStatement MakeWhileStmt(Expression condition, BlockStatement body,
            TextLocation loc = default(TextLocation))
        {
            return new WhileStatement(condition, body, loc);
        }

        public static ForStatement MakeForStmt(PatternConstruct pattern, Expression rvalue,
            BlockStatement body, TextLocation loc = default(TextLocation))
        {
            return new ForStatement(pattern, rvalue, body, loc);
        }

        public static ValueBindingForStatement MakeValueBindingForStmt(Modifiers modifiers, IEnumerable<VariableInitializer> initializers,
            BlockStatement body, TextLocation loc = default(TextLocation))
        {
            return new ValueBindingForStatement(modifiers, initializers, body, loc);
        }

        public static ValueBindingForStatement MakeValueBindingForStmt(Modifiers modifiers,BlockStatement body, params VariableInitializer[] initializers)
        {
            return new ValueBindingForStatement(modifiers, initializers, body, TextLocation.Empty);
        }

        public static EmptyStatement MakeEmptyStmt(TextLocation loc = default(TextLocation))
        {
            return new EmptyStatement(loc);
        }

        public static MatchStatement MakeMatchStmt(Expression target, IEnumerable<MatchPatternClause> clauses,
            TextLocation start = default(TextLocation), TextLocation end = default(TextLocation))
        {
            return new MatchStatement(target, clauses, start, end);
        }

        public static MatchStatement MakeMatchStmt(Expression target, params MatchPatternClause[] clauses)
        {
            return new MatchStatement(target, clauses, TextLocation.Empty, TextLocation.Empty);
        }

        public static MatchPatternClause MakeMatchClause(IEnumerable<PatternConstruct> patterns,
            Expression guard, Statement body)
        {
            return new MatchPatternClause(patterns, guard, body);
        }

        public static MatchPatternClause MakeMatchClause(Expression guard, Statement body, params PatternConstruct[] patterns)
        {
            return new MatchPatternClause(patterns, guard, body);
        }

        public static YieldStatement MakeYieldStmt(Expression expr, TextLocation start = default(TextLocation), 
            TextLocation end = default(TextLocation))
        {
            return new YieldStatement(expr, start, end);
        }
        #endregion
    }
}
