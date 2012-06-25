using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using L = System.Linq.Expressions;
using ParamDictionary = System.Collections.Generic.Dictionary<string, System.Linq.Expressions.ParameterExpression>;

namespace Expresso.Symbolic
{
	public class Lambda
	{
		#region fields

		string[] paramterNames;
		Expression body;

		#endregion
		#region constructors

		Lambda(Expression body, params string[] paramters)
		{
			this.body = body;
			this.paramterNames = paramters;
		}

		public Lambda(L::LambdaExpression e)
		{
			this.paramterNames = new string[e.Parameters.Count];

			for (int i = 0; i < e.Parameters.Count; ++i)
			{
				this.paramterNames[i] = e.Parameters[i].Name;
			}

			this.body = Expression.New(e.Body);
		}

		#region constructors for type inference

		public Lambda(L::Expression<Func<double, double>> e)
			: this((L::LambdaExpression)e) { }

		public Lambda(L::Expression<Func<double, double, double>> e)
			: this((L::LambdaExpression)e) { }

		public Lambda(L::Expression<Func<double, double, double, double>> e)
			: this((L::LambdaExpression)e) { }

		public Lambda(L::Expression<Func<double, double, double, double, double>> e)
			: this((L::LambdaExpression)e) { }

		#endregion
		#endregion
		#region operators

		public static Lambda operator -(Lambda x)
		{
			return new Lambda(-x.body, x.paramterNames);
		}

		public static Lambda operator +(Lambda x, Lambda y)
		{
			if (!x.MatchParamter(y))
				throw new LambdaException("Incorrect parameters");

			return new Lambda(x.body + y.body, x.paramterNames);
		}

		public static Lambda operator +(double c, Lambda x)
		{
			return new Lambda(c + x.body, x.paramterNames);
		}

		public static Lambda operator +(Lambda x, double c)
		{
			return new Lambda(x.body + c, x.paramterNames);
		}

		public static Lambda operator -(Lambda x, Lambda y)
		{
			if (!x.MatchParamter(y))
				throw new LambdaException("Incorrect parameters");

			return new Lambda(x.body - y.body, x.paramterNames);
		}

		public static Lambda operator -(double c, Lambda x)
		{
			return new Lambda(c - x.body, x.paramterNames);
		}

		public static Lambda operator -(Lambda x, double c)
		{
			return new Lambda(x.body - c, x.paramterNames);
		}

		public static Lambda operator *(Lambda x, Lambda y)
		{
			if (!x.MatchParamter(y))
				throw new LambdaException("Incorrect parameters");

			return new Lambda(x.body * y.body, x.paramterNames);
		}

		public static Lambda operator *(double c, Lambda x)
		{
			return new Lambda(c * x.body, x.paramterNames);
		}

		public static Lambda operator *(Lambda x, double c)
		{
			return new Lambda(x.body * c, x.paramterNames);
		}

		public static Lambda operator /(Lambda x, Lambda y)
		{
			if (!x.MatchParamter(y))
				throw new LambdaException("Incorrect parameters");

			return new Lambda(x.body / y.body, x.paramterNames);
		}

		public static Lambda operator /(double c, Lambda x)
		{
			return new Lambda(c / x.body, x.paramterNames);
		}

		public static Lambda operator /(Lambda x, double c)
		{
			return new Lambda(x.body / c, x.paramterNames);
		}

		#endregion
		#region

		/// <summary>
		/// if
		/// f = (x, y) => x * y,
		/// g = (z, w) => z * w,
		/// h = f * g
		/// then
		/// h == (x, y, z, w) => x * y * z * w
		/// </summary>
		/// <param name="f"></param>
		/// <param name="g"></param>
		/// <returns></returns>
		public static Lambda ExtendedMultiply(Lambda f, Lambda g)
		{
			var param = f.paramterNames.Concat(g.paramterNames).ToArray();
			return new Lambda(f.body * g.body, param);
		}

		#endregion
		#region parameter match

		bool MatchParamter(Lambda x)
		{
			return IsIdentical(this.paramterNames, x.paramterNames);
		}

		static bool IsIdentical(string[] x, string[] y)
		{
			if (x.Length != y.Length)
				return false;

			for (int i = 0; i < x.Length; ++i)
			{
				if (!x[i].Equals(y[i]))
					return false;
			}

			return true;
		}

		#endregion
		#region convert to System.Linq.Expressions.LambdaExpression

		public L::LambdaExpression ToLinqExpression()
		{
			ParamDictionary paramdic = new ParamDictionary();
			L::ParameterExpression[] paramarray = new L::ParameterExpression[this.paramterNames.Length];

			for (int i = 0; i < this.paramterNames.Length; ++i)
			{
				string name = this.paramterNames[i];
				L::ParameterExpression param = L::Expression.Parameter(typeof(double), name);

				paramarray[i] = param;
				paramdic[name] = param;
			}
			L::Expression body = this.body.ToLinqExpression(paramdic);

			return L::Expression.Lambda(body, paramarray);
		}

		public Delegate Compile()
		{
			L::LambdaExpression e = this.ToLinqExpression();
			return e.Compile();
		}

		#endregion
		#region Simplify

		/// <summary>
		/// simplify an Expression
		/// by reducing a common denominator and etc..
		/// </summary>
		/// <returns>result</returns>
		public Lambda Simplify()
		{
			// calc derivative
			Expression body = this.body.Simplify();
			return new Lambda(body, this.paramterNames);
		}

		#endregion
		#region Derive

		/// <summary>
		/// calculate symbolically a total derivative of a Lambda Expression e.
		/// </summary>
		/// <param name="e">expression to be differentiated</param>
		/// <returns>total derivative of e</returns>
		public Lambda Derive()
		{
			// check just one param (variable)
			if (this.paramterNames.Length != 1)
				throw new LambdaException("Incorrect number of parameters");

			// calc derivative
			Expression body = this.body.Derive(this.paramterNames[0]);
			return new Lambda(body, this.paramterNames);
		}

		/// <summary>
		/// calculate symbolically a partial derivative with respect to paramName.
		/// </summary>
		/// <param name="e">expression to be differentiated</param>
		/// <param name="paramName">parmter name</param>
		/// <returns>total derivative of e</returns>
		public Lambda Derive(string paramName)
		{
			// check params (variables)
			if (!this.paramterNames.Contains(paramName))
				return new Lambda(Expression.Constant(0.0),
					this.paramterNames);

			// calc derivative
			Expression body = this.body.Derive(paramName);
			return new Lambda(body, this.paramterNames);
		}

		/// <summary>
		/// calculate symbolically a partial derivative with respect to i-th parameter.
		/// </summary>
		/// <typeparam name="T">type of Function</typeparam>
		/// <param name="e">expression to be differentiated</param>
		/// <param name="i">paramter index</param>
		/// <returns>total derivative of e</returns>
		public Lambda Derive(int i)
		{
			// check params (variables)
			if (this.paramterNames.Length <= i)
				throw new LambdaException("Invalid paramter index");

			// calc derivative
			Expression body = this.body.Derive(this.paramterNames[i]);
			return new Lambda(body, this.paramterNames);
		}

		#endregion
		#region ToString

		public override string ToString()
		{
			return this.ToLinqExpression().ToString();
		}

		#endregion
	}
}
