using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using L = System.Linq.Expressions;
using ParamDictionary = System.Collections.Generic.Dictionary<string, System.Linq.Expressions.ParameterExpression>;

namespace Expresso.Symbolic
{
	public class CallExpression : Expression
	{
		public string Name { get; protected set; }

		public System.Collections.ObjectModel.ReadOnlyCollection<Expression> Arguments
		{
			get;
			protected set;
		}

		internal CallExpression(string name,
			params Expression[] arguments)
		{
			this.Name = name;
			this.Arguments = new System.Collections.ObjectModel.ReadOnlyCollection<Expression>(arguments);
		}

		public override ExpressionType NodeType
		{
			get { return ExpressionType.Call; }
		}

		public override L::Expression ToLinqExpression(ParamDictionary paramters)
		{
			IEnumerable<L::Expression> arguments =
				from x in this.Arguments
				select x.ToLinqExpression(paramters);

			return MathCall(this.Name, arguments.ToArray());
		}

		/// <summary>
		/// create an expression which contains System.Math method call.
		/// </summary>
		/// <param name="methodName">method name</param>
		/// <param name="arguments">arguments of the method</param>
		/// <returns>expression</returns>
		static L::Expression MathCall(string methodName,
			L::Expression[] arguments)
		{
			Type[] types = Enumerable.Range(0, arguments.Length).Select(x => typeof(double)).ToArray();

			return L::Expression.Call(null,
				typeof(System.Math).GetMethod(methodName, types),
				arguments);
		}

		public override int CompareTo(object other)
		{
			Expression e = (Expression)other;
			if (e.NodeType != this.NodeType)
				return this.NodeType.CompareTo(e.NodeType);

			CallExpression fe = (CallExpression)e;

			int result = this.Name.CompareTo(fe.Name);
			if (result != 0)
				return result;

			result = this.Arguments.Count.CompareTo(fe.Arguments.Count);
			if (result != 0)
				return result;

			for (int i = 0; i < this.Arguments.Count; ++i)
			{
				result = this.Arguments[i].CompareTo(fe.Arguments[i]);
				if (result != 0)
					return result;
			}
			return 0;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(this.Name);
			sb.Append('(');
			sb.Append(this.Arguments[0].ToString());
			for (int i = 1; i < this.Arguments.Count; ++i)
			{
				sb.Append(", ");
				sb.Append(this.Arguments[i].ToString());
			}
			sb.Append(')');
			return sb.ToString();
		}

		public override Expression SimplifyImpl()
		{
			var simplified =
				from x in this.Arguments
				select x.SimplifyImpl();
			return Expression.Call(this.Name, simplified.ToArray());
		}

		public override Expression DeriveImpl(string paramName)
		{
			Expression d = this.Arguments[0].Derive(paramName);

			switch (this.Name)
			{
				case "Sqrt":
					return Expression.Constant(0.5).Mul(d.Div(this));
				case "Sin":
					return d.Mul(MathCall("Cos", this.Arguments));
				case "Cos":
					return d.Mul(MathCall("Sin", this.Arguments)).Neg();
				case "Tan":
					{
						Expression cos = MathCall("Cos", this.Arguments);

						return d.Div(cos.Mul(cos));
					}
				case "Exp":
					return d * this;
				case "Log":
					if (this.Arguments.Count != 1)
						throw new LambdaException("Not implemented function: " +
								this.Name);
					return d.Div(this.Arguments[0]);

				//! so far, log_x(y) is not supported,
				//  but, log_x(y) could be supported as log_x(y) = log(y)/log(x)

				case "Pow":
					{
						Expression dx = d;
						Expression dy = this.Arguments[1].Derive(paramName);

						// a^f(x) (a does not contain x)
						if (dx.IsConstant(0))
						{
							return MathCall("Log", this.Arguments[0]).Mul(d).Mul(this);
						}
						// f(x)^a (a does not contain x)
						if (dy.IsConstant(0))
						{
							return d.Mul(
								MathCall("Pow", this.Arguments[0],
									this.Arguments[1].Sub(Expression.Constant(1.0))));
						}

						throw new LambdaException("Not implemented function: " +
								this.Name);
						//! so far, f(x)^g(x) is not supported.
						// Its derivative is so complex.
					}

				default:
					throw new LambdaException("Not implemented function: " +
							this.Name);
			}
		}

		public override bool IsIdentical(Expression e)
		{
			if (e.NodeType != ExpressionType.Call)
				return false;

			CallExpression fe = (CallExpression)e;

			return
				this.Name == fe.Name
				&&
				SequenceIsIdentical(this.Arguments, fe.Arguments);
		}

		/// <summary>
		/// create an expression which contains System.Math method call.
		/// </summary>
		/// <param name="methodName">method name</param>
		/// <param name="arguments">arguments of the method</param>
		/// <returns>expression</returns>
		static Expression MathCall(string methodName,
			IEnumerable<Expression> arguments)
		{
			return Expression.Call(methodName, arguments.ToArray());
		}

		static Expression MathCall(string methodName,
			params Expression[] arguments)
		{
			return MathCall(methodName, arguments);
		}

		static bool SequenceIsIdentical(
			ICollection<Expression> args1,
			ICollection<Expression> args2)
		{
			if (args1.Count != args2.Count) return false;

			var enum1 = args1.GetEnumerator();
			var enum2 = args2.GetEnumerator();

			while (enum1.MoveNext() && enum2.MoveNext())
			{
				if (!IsIdentical(enum1.Current, enum2.Current))
					return false;
			}

			return true;
		}
	}
}
