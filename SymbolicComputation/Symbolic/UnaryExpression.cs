using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using L = System.Linq.Expressions;
using ParamDictionary = System.Collections.Generic.Dictionary<string, System.Linq.Expressions.ParameterExpression>;

namespace Expresso.Symbolic
{
	public abstract class UnaryExpression : Expression
	{
		public Expression Operand { get; protected set; }

		internal UnaryExpression(Expression operand)
		{
			this.Operand = operand;
		}

		public abstract override ExpressionType NodeType { get; }

		public abstract override L::Expression ToLinqExpression(ParamDictionary paramters);

		public abstract override int CompareTo(object other);

		public abstract override string ToString();

		public override bool IsIdentical(Expression e)
		{
			if (this.NodeType != e.NodeType)
				return false;
			UnaryExpression ue = (UnaryExpression)e;
			return this.Operand.IsIdentical(ue.Operand);
		}
	}

	public class NegateExpression : UnaryExpression
	{
		internal NegateExpression(Expression operand)
			: base(operand)
		{
		}

		public override ExpressionType NodeType
		{
			get { return ExpressionType.Negate; }
		}

		public override L::Expression ToLinqExpression(ParamDictionary paramters)
		{
			L::Expression operand = this.Operand.ToLinqExpression(paramters);

			return L::Expression.Negate(operand);
		}

		public override int CompareTo(object other)
		{
			Expression e = (Expression)other;
			if (e.NodeType != this.NodeType)
				return this.NodeType.CompareTo(e.NodeType);

			Expression o = ((UnaryExpression)e).Operand;
			return this.Operand.CompareTo(o);
		}

		public override string ToString()
		{
			return "-" + this.Operand.ToString();
		}

		public override Expression SimplifyImpl()
		{
			Expression op = this.Operand.Simplify();

			if (op.NodeType == ExpressionType.Negate)
			{
				return ((UnaryExpression)op).Operand;
			}

			if (op.NodeType == ExpressionType.Multiply
				|| op.NodeType == ExpressionType.Divide)
			{
				BinaryExpression b = (BinaryExpression)op;
				Expression l = b.Left;
				Expression r = b.Right;
				if (l.NodeType == ExpressionType.Constant)
				{
					((ConstantExpression)l).Value *= -1;
					return b;
				}
			}

			return op.Neg();
		}

		public override Expression DeriveImpl(string paramName)
		{
			Expression d = this.Operand.Derive(paramName);
			return d.Neg();
		}

		public override Expression Neg()
		{
			return this.Operand;
		}

		public override Expression Mul(Expression e)
		{
			if (e.NodeType == ExpressionType.Multiply)
			{
				BinaryExpression b = (BinaryExpression)e;
				if (b.Left.IsConstant())
				{
					double c = ((ConstantExpression)b.Left).Value;
					return Expression.Constant(-c).Mul(this.Operand).Mul(b.Right);
				}
			}

			if (e.NodeType == ExpressionType.Divide)
			{
				BinaryExpression b = (BinaryExpression)e;
				if (b.Left.IsConstant())
				{
					double c = ((ConstantExpression)b.Left).Value;
					return Expression.Constant(-c).Mul(this.Operand).Div(b.Right);
				}
			}

			return base.Mul(e);
		}
	}
}
