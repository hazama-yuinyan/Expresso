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
        static internal BreakStatement MakeBreakStmt(int count)
        {
            return new BreakStatement(count);
        }

        static internal ContinueStatement MakeContinueStmt(int count)
        {
            return new ContinueStatement(count);
        }

        static internal BlockStatement MakeBlock(IEnumerable<Statement> stmts)
        {
            return new BlockStatement(stmts);
        }

        static internal VariableDeclarationStatement MakeVarDecl(IEnumerable<Identifier> lhs, IEnumerable<Expression> rhs,
            Modifiers modifiers = Modifiers.None)
        {
            return new VariableDeclarationStatement(lhs, rhs, modifiers);
        }

        static internal ExpressionStatement MakeExprStmt(Expression expr)
        {
            return new ExpressionStatement(expr);
        }

        static internal ReturnStatement MakeReturnStmt(Expression expr)
        {
            return new ReturnStatement(expr);
        }

        static internal IfStatement MakeIfStmt(Expression condition, BlockStatement trueBlock, BlockStatement falseBlock)
        {
            return new IfStatement(condition, trueBlock, falseBlock);
        }

        static internal WhileStatement MakeWhileStmt(Expression condition, BlockStatement body)
        {
            return new WhileStatement(condition, body);
        }

        static internal ForStatement MakeForStmt(PatternConstruct pattern, Expression rvalue, BlockStatement body)
        {
            return new ForStatement(pattern, rvalue, body);
        }

        static internal MatchStatement MakeSwitchStmt(Expression target, IEnumerable<MatchPatternClause> cases)
        {
            return new MatchStatement(target, cases);
        }

        static internal EmptyStatement MakeEmptyStmt()
        {
            return new EmptyStatement();
        }

        static internal MatchPatternClause MakeCaseClause(IEnumerable<Expression> labels, Statement body)
        {
            return new MatchPatternClause(labels, body);
        }

        static internal TryStatement MakeTryStmt(BlockStatement body, IEnumerable<CatchClause> catches, BlockStatement finallyClause)
        {
            if(finallyClause != null)
                return new TryStatement(body, catches, new FinallyClause(finallyClause));
            else
                return new TryStatement(body, catches, null);
        }

        static internal CatchClause MakeCatchClause(BlockStatement body, Identifier ident)
        {
            return new CatchClause(ident, body);
        }

        static internal ThrowStatement MakeThrowStmt(Expression expr)
        {
            return new ThrowStatement(expr);
        }

        static internal YieldStatement MakeYieldStmt(Expression expr)
        {
            return new YieldStatement(expr);
        }
        #endregion
    }
}
