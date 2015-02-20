using System;

using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/// <summary>
	/// If文。
    /// The If statement. In Expresso if statements can have 2 forms.
    /// One is a simple condition statement and the other is 
    /// "if" Pattern '{' { Statement } '}' [ "else" '{' { Statement } '}' ] ;
	/// </summary>
	public class IfStatement : Statement
	{
        public static readonly Role<PatternConstruct> ConditionRole =
            new Role<PatternConstruct>("Condition");
        public static readonly Role<Statement> TrueBlockRole = new Role<Statement>("TrueBlock");
        public static readonly TokenRole ElseKeywordRole = new TokenRole("else", ExpressoTokenNode.Null);
        public static readonly Role<Statement> FalseBlockRole = new Role<Statement>("FalseBlock");

        public ExpressoTokenNode IfToken{
            get{return GetChildByRole(Roles.IfToken);}
        }

		/// <summary>
        /// 条件式。
        /// The condition pattern. Because it is a pattern, it can bind or declare a new variable
        /// only alive for the inner blocks.
        /// It is an Expresso idiom 
        /// </summary>
        public PatternConstruct Condition{
            get{return GetChildByRole(ConditionRole);}
            set{SetChildByRole(ConditionRole, value);}
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
        /// It can be a null node if the if statement has no else clause.
        /// </summary>
        public Statement FalseBlock{
            get{return GetChildByRole(FalseBlockRole);}
            set{SetChildByRole(FalseBlockRole, value);}
        }

        public IfStatement(PatternConstruct test, Statement trueBlock, Statement falseBlock, TextLocation loc)
            : base(loc, (falseBlock == null) ? trueBlock.EndLocation : falseBlock.EndLocation)
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

