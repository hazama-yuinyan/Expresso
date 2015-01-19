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
        static internal ExpressionStatement MakeAssignment(SequenceExpression lhs, SequenceExpression rhs,
            TextLocation start, TextLocation end)
        {
            return new ExpressionStatement(new AssignmentExpression(lhs, rhs), start, end);
        }

        static internal ExpressionStatement MakeAugumentedAssignment(SequenceExpression targets,
            SequenceExpression rhs, OperatorType opType, TextLocation start, TextLocation end)
        {
            return new ExpressionStatement(new AssignmentExpression(targets, rhs, opType), start, end);
        }

        static internal BreakStatement MakeBreakStmt(LiteralExpression count, TextLocation start, TextLocation end)
        {
            return new BreakStatement(count, start, end);
        }

        static internal ContinueStatement MakeContinueStmt(LiteralExpression count, TextLocation start, TextLocation end)
        {
            return new ContinueStatement(count, start, end);
        }

        static internal BlockStatement MakeBlock(IEnumerable<Statement> stmts, TextLocation start, TextLocation end)
        {
            return new BlockStatement(stmts, start, end);
        }

        static internal VariableDeclarationStatement MakeVarDecl(IEnumerable<Identifier> lhs, IEnumerable<Expression> rhs,
            Modifiers modifiers, TextLocation start, TextLocation end)
        {
            return new VariableDeclarationStatement(lhs, rhs, modifiers, start, end);
        }

        static internal ExpressionStatement MakeExprStmt(Expression expr, TextLocation start, TextLocation end)
        {
            return new ExpressionStatement(expr, start, end);
        }

        static internal ReturnStatement MakeReturnStmt(Expression expr)
        {
            return new ReturnStatement(expr);
        }

        static internal IfStatement MakeIfStmt(Expression condition, BlockStatement trueBlock,
            BlockStatement falseBlock, TextLocation loc)
        {
            return new IfStatement(condition, trueBlock, falseBlock, loc);
        }

        static internal WhileStatement MakeWhileStmt(Expression condition, BlockStatement body,
            TextLocation loc)
        {
            return new WhileStatement(condition, body, loc);
        }

        static internal ForStatement MakeForStmt(PatternConstruct pattern, Expression rvalue,
            BlockStatement body, TextLocation start)
        {
            return new ForStatement(pattern, rvalue, body, start);
        }

        static internal EmptyStatement MakeEmptyStmt(TextLocation start)
        {
            return new EmptyStatement(start);
        }

        static internal MatchStatement MakeMatchStmt(Expression target, IEnumerable<MatchPatternClause> clauses,
            TextLocation start, TextLocation end)
        {
            return new MatchStatement(target, clauses, start, end);
        }

        static internal MatchPatternClause MakeMatchClause(IEnumerable<PatternConstruct> patterns, Statement body)
        {
            return new MatchPatternClause(patterns, body);
        }

        static internal YieldStatement MakeYieldStmt(Expression expr, TextLocation start, TextLocation end)
        {
            return new YieldStatement(expr, start, end);
        }
        #endregion
    }
}
