using System;

using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/// <summary>
	/// If文。
    /// Expressoのif文には2種類あり、1種類目は他の大多数の言語同様、通常の右辺式をとり、既知の変数などに対して
    /// boolean演算を行うのに対し、2種類目はパターンマッチングを行うため、1行でOptional型の束縛から値の取り出しまでを行う
    /// 簡潔なnull値チェック式を書くことができる。
    /// The If statement. In Expresso, if statements can have 2 forms.
    /// One is a simple condition statement and the other tests against a pattern.
    /// Patterns can let-bind or declare variables so it's a common idiom of using it as a handy
    /// optional value extractor.
    /// "if" Pattern '{' { Statement } '}' [ "else" '{' { Statement } '}' ] ;
	/// </summary>
	public class IfStatement : Statement
	{
        public static readonly Role<PatternConstruct> ConditionRole =
            new Role<PatternConstruct>("Condition", PatternConstruct.Null);
        public static readonly Role<BlockStatement> TrueBlockRole =
            new Role<BlockStatement>("TrueBlock", BlockStatement.Null);
        public static readonly TokenRole ElseKeywordRole = new TokenRole("else", ExpressoTokenNode.Null);
        public static readonly Role<BlockStatement> FalseBlockRole =
            new Role<BlockStatement>("FalseBlock", BlockStatement.Null);

        public ExpressoTokenNode IfToken{
            get{return GetChildByRole(Roles.IfToken);}
        }

		/// <summary>
        /// 条件式。
        /// The condition pattern. Because it is a pattern, it can bind or declare a new variable
        /// only alive for the inner blocks.
        /// It is a common idiom in Expresso to use an if statement as a null-check-and-then-go
        /// statement.
        /// </summary>
        public PatternConstruct Condition{
            get{return GetChildByRole(ConditionRole);}
            set{SetChildByRole(ConditionRole, value);}
		}

        /// <summary>
        /// 条件が真の時に評価するブロック文。
		/// The block statement to be taken if the condition is evaluated to true.
        /// </summary>
        public BlockStatement TrueBlock{
            get{return GetChildByRole(TrueBlockRole);}
            set{SetChildByRole(TrueBlockRole, value);}
		}

        public ExpressoTokenNode ElseToken{
            get{return GetChildByRole(ElseKeywordRole);}
        }

        /// <summary>
        /// 条件が偽の時に評価するブロック文。
		/// The block statement to be taken if the condition is evaluated to false.
        /// It can be a null node if the statement has no else clause.
        /// </summary>
        public BlockStatement FalseBlock{
            get{return GetChildByRole(FalseBlockRole);}
            set{SetChildByRole(FalseBlockRole, value);}
        }

        public IfStatement(PatternConstruct test, BlockStatement trueBlock, BlockStatement falseBlock,
            TextLocation loc)
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

