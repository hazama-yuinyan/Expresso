using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Runtime;
using Expresso.Compiler;
using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/// <summary>
	/// If文。
	/// The If statement.
    /// "if" Expression Block [ "else" Statement ]
	/// </summary>
	public class IfStatement : Statement
	{
        public static readonly TokenRole IfKeywordRole = new TokenRole("if");
        public static readonly Role<Expression> ConditionRole = new Role<Expression>("Condition", Expression.Null);
        public static readonly Role<Statement> TrueBlockRole = new Role<Statement>("TrueBlock", Statement.Null);
        public static readonly TokenRole ElseKeywordRole = new TokenRole("else");
        public static readonly Role<Statement> FalseBlockRole = new Role<Statement>("FalseBlock", Statement.Null);

        public ExpressoTokenNode IfToken{
            get{GetChildByRole(IfKeywordRole);}
        }

        public ExpressoTokenNode LPar{
            get{return GetChildByRole(Roles.LParenthesisToken);}
        }

		/// <summary>
        /// 条件式。
		/// The condition.
        /// </summary>
        public Expression Condition{
            get{return GetChildByRole(ConditionRole);}
            set{SetChildByRole(ConditionRole, value);}
		}

        public ExpressoTokenNode RPar{
            get{return GetChildByRole(Roles.RParenthesisToken);}
        }

        /// <summary>
        /// 条件が真の時に評価する文(郡)。
		/// The statements to be operated when the condition is evaluated to true.
        /// </summary>
        public Statement TrueBlock{
            get{return GetChildByRole(TrueBlockRole);}
            set{SetChildByRole(TrueBlockRole, value);}
		}

        public ExpressoTokenNode ElseToken{
            get{return GetChildByRole(ElseKeywordRole);}
        }

        /// <summary>
        /// 条件が偽の時に評価する文(郡)。
		/// The statements to be operated when the condition is evaluated to false.
		/// It can be null if the if statement has no else clause.
        /// </summary>
        public Statement FalseBlock{
            get{return GetChildByRole(FalseBlockRole);}
            set{SetChildByRole(FalseBlockRole, value);}
        }

        public override NodeType NodeType{
            get{return NodeType.Statement;}
        }

        public IfStatement()
        {
        }

		public IfStatement(Expression test, Statement trueBlock, Statement falseBlock)
		{
            Condition = test;
            TrueBlock = trueBlock;
            FalseBlock = falseBlock;
		}

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitIfStatement(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitIfStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitIfStatement(this, data);
        }

        internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            IfStatement o = other as IfStatement;
            return o != null && this.Condition.DoMatch(o.Condition, match)
                && this.TrueBlock.DoMatch(o.TrueBlock, match)
                && this.FalseBlock.DoMatch(o.FalseBlock, match);
        }
	}
}

