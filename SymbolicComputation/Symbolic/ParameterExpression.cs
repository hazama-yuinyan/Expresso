using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using L = System.Linq.Expressions;
using ParamDictionary = System.Collections.Generic.Dictionary<string, System.Linq.Expressions.ParameterExpression>;

namespace Expresso.Symbolic
{
	public class ParameterExpression : Expression
	{
		internal ParameterExpression(string name)
		{
			this.Name = name;
		}

		public string Name { get; protected set; }

		public override ExpressionType NodeType
		{
			get { return ExpressionType.Parameter; }
		}

		public override L::Expression ToLinqExpression(ParamDictionary paramters)
		{
			if (!paramters.Keys.Contains(this.Name))
				throw new LambdaException("invalid paramter name: " + this.Name);

			return paramters[this.Name];
		}

		public override int CompareTo(object other)
		{
			Expression e = (Expression)other;
			if (e.NodeType != this.NodeType)
				return this.NodeType.CompareTo(e.NodeType);

			string name = ((ParameterExpression)e).Name;
			return this.Name.CompareTo(name);
		}

		public override bool IsIdentical(Expression e)
		{
			if (e.NodeType != ExpressionType.Parameter)
				return false;

			return this.Name.Equals(((ParameterExpression)e).Name);
		}

		public override string ToString()
		{
			return this.Name;
		}

		public override Expression SimplifyImpl()
		{
			return this;
		}

		public override Expression DeriveImpl(string paramName)
		{
			if (this.Name == paramName)
				return Expression.Constant(1.0);
			else
				return Expression.Constant(0.0);
		}
	}
}
