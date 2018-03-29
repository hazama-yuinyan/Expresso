using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents the do while statement.
    /// "do" Block "while" Expression ';' ; 
    /// </summary>
    public class DoWhileStatement : Statement
    {
        public static readonly Role<WhileStatement> WhileStmtRole = 
            new Role<WhileStatement>("WhileStmt", WhileStatement.Null);
        
        /// <summary>
        /// Gets or sets the real AST.
        /// </summary>
        /// <value>The body.</value>
        public WhileStatement Delegator{
            get{return GetChildByRole(WhileStmtRole);}
            set{SetChildByRole(WhileStmtRole, value);}
        }

        public DoWhileStatement(WhileStatement stmt, TextLocation start, TextLocation end)
            : base(start, end)
        {
            Delegator = stmt;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitDoWhileStatement(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitDoWhileStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitDoWhileStatement(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as DoWhileStatement;
            return o != null && Delegator.DoMatch(o.Delegator, match);
        }
    }
}
