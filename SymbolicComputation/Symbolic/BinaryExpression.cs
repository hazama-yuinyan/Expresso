using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using L = System.Linq.Expressions;
using ParamDictionary = System.Collections.Generic.Dictionary<string, System.Linq.Expressions.ParameterExpression>;

//! 最適化をかけるに当たって、定数は左オペランドに来るようにする
//! 逆に、左側に定数があることを前提にした最適化コードを書いてる

namespace Expresso.Symbolic
{
	public abstract class BinaryExpression : Expression
	{
		internal BinaryExpression(
			Expression left, Expression right)
		{
			this.Left = left;
			this.Right = right;
		}

		public override abstract ExpressionType NodeType { get; }

		public Expression Left { get; protected set; }
		public Expression Right { get; protected set; }

		public override abstract L::Expression ToLinqExpression(ParamDictionary paramters);

		public override int CompareTo(object other)
		{
			Expression e = (Expression)other;
			if (e.NodeType != this.NodeType)
				return this.NodeType.CompareTo(e.NodeType);

			Expression l = ((BinaryExpression)e).Left;
			Expression r = ((BinaryExpression)e).Right;

			int lresult = this.Left.CompareTo(l);
			if (lresult != 0)
				return lresult;
			return this.Right.CompareTo(r);
		}

		public override string ToString()
		{
			string op;
			switch (this.NodeType)
			{
				case ExpressionType.Add: op = " + "; break;
				case ExpressionType.Subtract: op = " - "; break;
				case ExpressionType.Multiply: op = " * "; break;
				case ExpressionType.Divide: op = " / "; break;
				default: throw new LambdaException("unexpected error");
			}
			return "(" + this.Left.ToString() + op + this.Right.ToString() + ")";
		}

		public override bool IsIdentical(Expression e)
		{
			if (this.NodeType != e.NodeType)
				return false;

			BinaryExpression be = (BinaryExpression)e;
			return this.Left.IsIdentical(be.Left) && this.Right.IsIdentical(be.Right);
		}
	}

	public class AddExpression : BinaryExpression
	{
		public AddExpression(Expression l, Expression r)
			: base(l, r) {}

		public override ExpressionType NodeType
		{
			get { return ExpressionType.Add; }
		}

		public override L::Expression ToLinqExpression(ParamDictionary paramters)
		{
			L::Expression l = this.Left.ToLinqExpression(paramters);
			L::Expression r = this.Right.ToLinqExpression(paramters);

			return L::Expression.Add(l, r);
		}
		
		public override Expression SimplifyImpl()
		{
			return Cancel(this);
		}

		public override Expression DeriveImpl(string paramName)
		{
			Expression dl = this.Left.Derive(paramName);
			Expression dr = this.Right.Derive(paramName);
			return dl.Add(dr);
		}

		public override Expression Add(Expression e)
		{
			if (e.IsConstant(0))
				return this;

			if (e.IsConstant() && this.Left.IsConstant())
			{
				return this.Left.Add(e).Add(this.Right);
			}

			if (e.NodeType == ExpressionType.Add)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				l = l.Add(this.Left);
				r = r.Add(this.Right);
				if (l.NodeType != ExpressionType.Add)
					return l.Add(r);
				return l + r;
			}

			return base.Add(e);
		}

		public override bool IsIdentical(Expression e)
		{
			AddExpression be = e as AddExpression;
			if (be == null)
				return false;
			return 
				(this.Left.IsIdentical(be.Left) && this.Right.IsIdentical(be.Right))
				||
				(this.Left.IsIdentical(be.Right) && this.Right.IsIdentical(be.Left))
				;
		}
	}

	public class SubtractExpression : BinaryExpression
	{
		public SubtractExpression(Expression l, Expression r)
			: base(l, r) { }

		public override ExpressionType NodeType
		{
			get { return ExpressionType.Subtract; }
		}

		public override L::Expression ToLinqExpression(ParamDictionary paramters)
		{
			L::Expression l = this.Left.ToLinqExpression(paramters);
			L::Expression r = this.Right.ToLinqExpression(paramters);

			return L::Expression.Subtract(l, r);
		}

		public override Expression SimplifyImpl()
		{
			return Cancel(this);
		}

		public override Expression DeriveImpl(string paramName)
		{
			Expression dl = this.Left.Derive(paramName);
			Expression dr = this.Right.Derive(paramName);
			return dl.Sub(dr);
		}

		public override Expression Neg()
		{
			return this.Right.Sub(this.Left);
		}
	}

	public class MultiplyExpression : BinaryExpression
	{
		public MultiplyExpression(Expression l, Expression r)
			: base(l, r) { }

		public override ExpressionType NodeType
		{
			get { return ExpressionType.Multiply; }
		}

		public override L::Expression ToLinqExpression(ParamDictionary paramters)
		{
			L::Expression l = this.Left.ToLinqExpression(paramters);
			L::Expression r = this.Right.ToLinqExpression(paramters);

			return L::Expression.Multiply(l, r);
		}

		public override Expression SimplifyImpl()
		{
			return Reduce(this);
		}

		public override Expression DeriveImpl(string paramName)
		{
			Expression l = this.Left;
			Expression r = this.Right;
			Expression dl = l.Derive(paramName);
			Expression dr = r.Derive(paramName);
			return l.Mul(dr).Add(dl.Mul(r));
		}

		public override Expression Neg()
		{
			return this.Left.Neg() * this.Right;
		}

		public override Expression Mul(Expression e)
		{
			if (e.IsConstant(0))
				return 0;

			if (e.IsConstant(1))
				return this;

			if (e.IsConstant())
			{
				return e.Mul(this.Left).Mul(this.Right);
			}

			if (e.NodeType == ExpressionType.Multiply)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				l = l.Mul(this.Left);
				r = r.Mul(this.Right);
				if (l.NodeType != ExpressionType.Multiply)
					return l.Mul(r);
				return l * r;
			}

			return this.Left.Mul(this.Right.Mul(e));
		}

		public override Expression Div(Expression e)
		{
			if (e.IsConstant(1))
				return this;

			if (e.IsConstant())
			{
				return (1 / e).Mul(this.Left).Mul(this.Right);
			}

			if (e.NodeType == ExpressionType.Multiply)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				l = this.Left.Div(l);
				r = this.Right.Div(r);
				if (l.NodeType != ExpressionType.Multiply)
					return l.Mul(r);
				return l * r;
			}

			if (e.NodeType == ExpressionType.Divide)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				l = this.Left.Div(l);
				r = this.Right.Mul(r);
				if (l.NodeType != ExpressionType.Multiply)
					return l.Mul(r);
				return l * r;
			}

			return this.Left.Mul(this.Right.Div(e));
		}

		public override bool IsIdentical(Expression e)
		{
			MultiplyExpression be = e as MultiplyExpression;
			if (be == null)
				return false;
			return
				(this.Left.IsIdentical(be.Left) && this.Right.IsIdentical(be.Right))
				||
				(this.Left.IsIdentical(be.Right) && this.Right.IsIdentical(be.Left))
				;
		}
	}

	public class DivideExpression : BinaryExpression
	{
		public DivideExpression(Expression l, Expression r)
			: base(l, r) { }

		public override ExpressionType NodeType
		{
			get { return ExpressionType.Divide; }
		}

		public override L::Expression ToLinqExpression(ParamDictionary paramters)
		{
			L::Expression l = this.Left.ToLinqExpression(paramters);
			L::Expression r = this.Right.ToLinqExpression(paramters);

			return L::Expression.Divide(l, r);
		}

		public override Expression SimplifyImpl()
		{
			if (this.Left.IsConstant(0)) return 0;
			if (this.Right.IsConstant(0)) return 0;
			if (this.Right.IsConstant(1)) return this.Left;

			if (this.Left.NodeType == ExpressionType.Negate)
			{
				Expression l = ((UnaryExpression)this.Left).Operand;
				if (this.Right.NodeType == ExpressionType.Negate)
				{
					Expression r = ((UnaryExpression)this.Right).Operand;
					return l.Div(r);
				}
				return l.Div(this.Right).Neg();
			}
			if (this.Right.NodeType == ExpressionType.Negate)
			{
				Expression r = ((UnaryExpression)this.Right).Operand;
				return r.Div(this.Left).Neg();
			}

			return Reduce(this);
		}

		public override Expression DeriveImpl(string paramName)
		{
			Expression l = this.Left;
			Expression r = this.Right;
			Expression dl = l.Derive(paramName);
			Expression dr = r.Derive(paramName);
			return dl.Mul(r).Sub(l.Mul(dr)).Div(r.Mul(r));
		}

		public override Expression Neg()
		{
			return this.Left.Neg() / this.Right;
		}

		public override Expression Mul(Expression e)
		{
			if (e.IsConstant(0))
				return 0;

			if (e.IsConstant(1))
				return this;

			if (e.IsConstant())
			{
				return e.Mul(this.Left).Div(this.Right);
			}

			if (e.NodeType == ExpressionType.Multiply)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				l = l.Mul(this.Left);
				r = r.Div(this.Right);
				if (l.NodeType != ExpressionType.Multiply)
					return l.Mul(r);
				return l * r;
			}

			return this.Left.Mul(e).Div(this.Right);
		}

		public override Expression Div(Expression e)
		{
			if (e.IsConstant(1))
				return this;

			if (e.IsConstant())
			{
				return (1 / e).Mul(this.Left).Div(this.Right);
			}

			if (e.NodeType == ExpressionType.Multiply)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				l = this.Left.Div(l);
				r = this.Right.Div(r);
				if (l.NodeType != ExpressionType.Multiply)
					return l.Mul(r);
				return l * r;
			}

			if (e.NodeType == ExpressionType.Divide)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				l = this.Left.Div(l);
				r = this.Right.Div(r);
				if (l.NodeType != ExpressionType.Multiply)
					return l.Mul(r);
				return l * r;
			}

			return this.Left.Div(this.Right.Mul(e));
		}
	}
}
