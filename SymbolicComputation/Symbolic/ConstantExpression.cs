using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using L = System.Linq.Expressions;
using ParamDictionary = System.Collections.Generic.Dictionary<string, System.Linq.Expressions.ParameterExpression>;

namespace Expresso.Symbolic
{
	public class ConstantExpression : Expression
	{
		internal ConstantExpression(double val)
		{
			this.Value = val;
		}

		public double Value { get; protected internal set; }

		public override ExpressionType NodeType
		{
			get { return ExpressionType.Constant; }
		}

		public override L::Expression ToLinqExpression(ParamDictionary paramters)
		{
			return L::Expression.Constant(this.Value);
		}

		public override int CompareTo(object other)
		{
			Expression e = (Expression)other;
			if (e.NodeType != this.NodeType)
				return this.NodeType.CompareTo(e.NodeType);

			double v = ((ConstantExpression)e).Value;
			return this.Value.CompareTo(v);
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public override bool IsIdentical(Expression e)
		{
			if (e.NodeType != ExpressionType.Constant)
				return false;

			return this.Value == ((ConstantExpression)e).Value;
		}

		public override Expression SimplifyImpl()
		{
			return this;
		}

		public override Expression DeriveImpl(string paramName)
		{
			return Expression.Constant(0.0);
		}

		#region predicate

		public override bool IsConstant(double c)
		{
			return this.Value == c;
		}

		public override bool IsConstant()
		{
			return true;
		}

		#endregion
		#region optimized arithmetic

		public override Expression Neg()
		{
			return Expression.Constant(-this.Value);
		}

		public override Expression Add(Expression e)
		{
			if (e.IsConstant())
			{
				double c = ((ConstantExpression)e).Value;
				return Expression.Constant(this.Value + c);
			}

			return base.Add(e);
		}

		public override Expression Sub(Expression e)
		{
			if (e.NodeType == ExpressionType.Constant)
			{
				double c = ((ConstantExpression)e).Value;
				return Expression.Constant(this.Value - c);
			}

			return base.Sub(e);
		}

		public override Expression Mul(Expression e)
		{
			if (e.NodeType == ExpressionType.Constant)
			{
				double c = ((ConstantExpression)e).Value;
				return Expression.Constant(this.Value * c);
			}

			if (this.Value == 0)
				return 0;

			if (this.Value == 1)
				return e;

			if (e.NodeType == ExpressionType.Negate)
			{
				return Expression.Constant(-this.Value).Mul(((UnaryExpression)e).Operand);
			}

			return base.Mul(e);
		}

		public override Expression Div(Expression e)
		{
			if (e.NodeType == ExpressionType.Constant)
			{
				double c = ((ConstantExpression)e).Value;
				return Expression.Constant(this.Value / c);
			}

			if (e.NodeType == ExpressionType.Negate)
			{
				return Expression.Constant(-this.Value).Div(((UnaryExpression)e).Operand);
			}

			return base.Div(e);
		}

		#endregion
	}
}
