using ICSharpCode.NRefactory;

namespace Expresso.Ast
{
	/// <summary>
	/// 整数の数列をあらわす式.
	/// Represents an integer sequence.
    /// <see cref="Expresso.Runtime.Builtins.ExpressoIntegerSequence"/>
    /// BinaryExpression (".." | "...") BinaryExpression [ ':' BinaryExpression ] ;
    /// where BinaryExpression = Expression '|' Expression
	/// </summary>
    public class IntegerSequenceExpression : Expression
	{
        public static readonly Role<Expression> LowerRole = new Role<Expression>("Lower", Null);
        public static readonly Role<Expression> UpperRole = new Role<Expression>("Upper", Null);
        public static readonly Role<Expression> StepRole = new Role<Expression>("Step", Null);

		/// <summary>
		/// 整数列の下限.
		/// The lower bound of the integer sequence.
		/// The expression has to yield an integer.
		/// </summary>
		public Expression Lower{
            get{return GetChildByRole(LowerRole);}
            set{SetChildByRole(LowerRole, value);}
		}

		/// <summary>
		/// 整数列の上限.
		/// The upper bound of the integer sequence.
		/// The expression has to yield an integer.
		/// </summary>
		public Expression Upper{
            get{return GetChildByRole(UpperRole);}
            set{SetChildByRole(UpperRole, value);}
		}
		
		/// <summary>
		/// ステップ.
		/// The step by which an iteration proceeds at a time.
		/// The expression has to yield an integer.
		/// </summary>
		public Expression Step{
            get{return GetChildByRole(StepRole);}
            set{SetChildByRole(StepRole, value);}
		}

        public bool UpperInclusive{
            get; set;
        }

        public IntegerSequenceExpression(Expression start, Expression end, Expression stepExpr, bool upperInclusive, TextLocation startLoc, TextLocation endLoc)
            : base(startLoc, endLoc)
		{
            Lower = start;
            Upper = end;
            Step = stepExpr;
            UpperInclusive = upperInclusive;
		}
		
        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitIntegerSequenceExpression(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitIntegerSequenceExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitIntegerSequenceExpression(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as IntegerSequenceExpression;
            return o != null && Lower.DoMatch(o.Lower, match)
                && Upper.DoMatch(o.Upper, match) && Step.DoMatch(o.Step, match)
                && UpperInclusive == o.UpperInclusive;
        }

        #endregion
	}
}

