using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using L = System.Linq.Expressions;
using ParamDictionary = System.Collections.Generic.Dictionary<string, System.Linq.Expressions.ParameterExpression>;

namespace Expresso.Symbolic
{
	/// <summary>
	/// Epression Tree node type.
	/// </summary>
	public enum ExpressionType
	{
		Constant,
		Parameter,
		Add,
		Subtract,
		Multiply,
		Divide,
		Negate,
		Call,
	}

	/// <summary>
	/// Base class for symbolic expressions.
	/// </summary>
	public abstract class Expression : IComparable
	{
		#region abstract methods

		/// <summary>
		/// get node type.
		/// </summary>
		public abstract ExpressionType NodeType { get; }

		/// <summary>
		/// convert to System.Linq.Expressions.Expression.
		/// </summary>
		/// <param name="paramters">map from parameter names to System.Linq.Expressions.ParameterExpression</param>
		/// <returns>convert result</returns>
		public abstract L::Expression ToLinqExpression(ParamDictionary paramters);

		#region equality check

		/// <summary>
		/// Determines if this is a constant c.
		/// </summary>
		/// <param name="c">constant</param>
		/// <returns>true if this is a constant</returns>
		public virtual bool IsConstant(double c)
		{
			return false;
		}

		/// <summary>
		/// Determines if this is a constant.
		/// </summary>
		/// <returns>true if this is a constant</returns>
		public virtual bool IsConstant()
		{
			return false;
		}

		/// <summary>
		/// Checks whether this is identical to e or not.
		/// (x + 1) and (1 + x) are identical in the result of this method.
		/// (This is the reason why the method name is not "Equals".)
		/// </summary>
		/// <param name="e">operand</param>
		/// <returns>true if identical</returns>
		/// <remarks>
		/// This method is not enough to check the identity completelly.
		/// The identity check could be failed when e1 and e2 are complex.
		/// For instance, so far, even in the case that
		/// e1 = (x + 1) + y and e2 = (y + 1) + x,
		/// the check is failed.
		/// </remarks>
		public abstract bool IsIdentical(Expression e);

		/// <summary>
		/// Checks whether e1 is identical to e2 or not, with null check.
		/// </summary>
		/// <param name="e1">operand 1</param>
		/// <param name="e2">operand 2</param>
		/// <returns>true if identical</returns>
		/// <remarks>
		/// returns true if both of e1 and e2 are null.
		/// </remarks>
		public static bool IsIdentical(Expression e1, Expression e2)
		{
			if (e1 == null)
				return e2 == null;
			else if (e2 == null)
				return false;

			return e1.IsIdentical(e2);
		}

		#endregion
		#region optimized arithmetic

		public virtual Expression Neg()
		{
			return -this;
		}

		public virtual Expression Add(Expression e)
		{
			if (e.IsConstant(0))
				return this;

			if (this.IsIdentical(e))
				return Expression.Constant(2).Mul(this);

			if (e.NodeType == ExpressionType.Negate)
			{
				Expression o = ((UnaryExpression)e).Operand;
				return this.Sub(o);
			}

			return this + e;
		}

		public virtual Expression Sub(Expression e)
		{
			if (e.IsConstant(0))
				return this;

			if (this.IsIdentical(e))
				return 0;

			if (e.NodeType == ExpressionType.Negate)
			{
				Expression o = ((UnaryExpression)e).Operand;
				return this.Add(o);
			}

			return this - e;
		}

		public virtual Expression Mul(Expression e)
		{
			if (e.IsConstant(1))
				return this;

			return this * e;
		}

		public virtual Expression Div(Expression e)
		{
			if (this.IsIdentical(e))
				return 1;

			return this / e;
		}

		#endregion

		#endregion
		#region constructor

		/// <summary>
		/// Convert from System.Linq.Expressions.Expression.
		/// </summary>
		/// <param name="e">System.Linq.Expressions.Expression</param>
		/// <returns>The result</returns>
		internal static Expression New(L::Expression e)
		{
			switch (e.NodeType)
			{
				case L::ExpressionType.Constant:
					{
						double val = (double)((L::ConstantExpression)e).Value;
						return Expression.Constant(val);
					}
				case L::ExpressionType.Parameter:
					{
						string name = ((L::ParameterExpression)e).Name;
						return Expression.Parameter(name);
					}
				case L::ExpressionType.Negate:
					{
						return Expression.Negate(
							Expression.New(((L::UnaryExpression)e).Operand));
					}
				case L::ExpressionType.Add:
					{
						Expression l = Expression.New(((L::BinaryExpression)e).Left);
						Expression r = Expression.New(((L::BinaryExpression)e).Right);
						return Expression.Add(l, r);
					}
				case L::ExpressionType.Subtract:
					{
						Expression l = Expression.New(((L::BinaryExpression)e).Left);
						Expression r = Expression.New(((L::BinaryExpression)e).Right);
						return Expression.Subtract(l, r);
					}
				case L::ExpressionType.Multiply:
					{
						Expression l = Expression.New(((L::BinaryExpression)e).Left);
						Expression r = Expression.New(((L::BinaryExpression)e).Right);
						return Expression.Multiply(l, r);
					}
				case L::ExpressionType.Divide:
					{
						Expression l = Expression.New(((L::BinaryExpression)e).Left);
						Expression r = Expression.New(((L::BinaryExpression)e).Right);
						return Expression.Divide(l, r);
					}
				case L::ExpressionType.Call:
					{
						L::MethodCallExpression me = ((L::MethodCallExpression)e);
						MethodInfo mi = me.Method;

						// currently, only static functions in System.Math are supported.
						if (!mi.IsStatic || mi.DeclaringType.FullName != "System.Math")
							throw new LambdaException("Not supported function: " +
								mi.DeclaringType + "/" + mi.Name);

						var arguments =
							from x in me.Arguments
							select Expression.New(x);

						return Expression.Call(mi.Name, arguments.ToArray());
					}
				default:
					throw new LambdaException(
						"Not implemented expression type: " + e.NodeType.ToString());
			}
		}

		#endregion
		#region create expression

		// Construction with imitating System.Linq.Expressions.Expression.
		// However, these are not necessary, because my Expression.XXX method has one-to-one relationship to XXXExpression class.

		public static Expression Constant(double val)
		{
			return new ConstantExpression(val);
		}

		public static Expression Parameter(string name)
		{
			return new ParameterExpression(name);
		}

		public static Expression Negate(Expression e)
		{
			return new NegateExpression(e);
		}

		public static Expression Add(Expression e1, Expression e2)
		{
			return new AddExpression(e1, e2);
		}

		public static Expression Subtract(Expression e1, Expression e2)
		{
			return new SubtractExpression(e1, e2);
		}

		public static Expression Multiply(Expression e1, Expression e2)
		{
			return new MultiplyExpression(e1, e2);
		}

		public static Expression Divide(Expression e1, Expression e2)
		{
			return new DivideExpression(e1, e2);
		}

		public static Expression Call(string name, params Expression[] arguments)
		{
			return new CallExpression(name, arguments);
		}

		#endregion
		#region operator

		/// <summary>
		/// negate an expression.
		/// </summary>
		/// <param name="e">operand</param>
		/// <returns>result</returns>
		public static Expression operator -(Expression e)
		{
			return Expression.Negate(e);
		}

		/// <summary>
		/// add expressions.
		/// </summary>
		/// <param name="e1">operand 1</param>
		/// <param name="e2">operand 2</param>
		/// <returns>result</returns>
		public static Expression operator +(Expression e1, Expression e2)
		{
			Expression e = Expression.Add(e1, e2);
			return e;
		}

		/// <summary>
		/// subtract expressions.
		/// </summary>
		/// <param name="e1">operand 1</param>
		/// <param name="e2">operand 2</param>
		/// <returns>result</returns>
		public static Expression operator -(Expression e1, Expression e2)
		{
			Expression e = Expression.Subtract(e1, e2);
			return e;
		}

		/// <summary>
		/// Multiply expressions.
		/// </summary>
		/// <param name="e1">operand 1</param>
		/// <param name="e2">operand 2</param>
		/// <returns>result</returns>
		public static Expression operator *(Expression e1, Expression e2)
		{
			Expression e = Expression.Multiply(e1, e2);
			return e;
		}

		/// <summary>
		/// divide expressions.
		/// </summary>
		/// <param name="e1">operand 1</param>
		/// <param name="e2">operand 2</param>
		/// <returns>result</returns>
		public static Expression operator /(Expression e1, Expression e2)
		{
			Expression e = Expression.Divide(e1, e2);
			return e;
		}

		/// <summary>
		/// implict cast operator from double to Expression.
		/// </summary>
		/// <param name="c">operand</param>
		/// <returns>result</returns>
		public static implicit operator Expression(double c)
		{
			return Expression.Constant(c);
		}

		#endregion
		#region simplify

		/// <summary>
		/// implementation body of Simplify.
		/// </summary>
		/// <returns>simplified result</returns>
		public virtual Expression SimplifyImpl()
		{
			return this;
		}

		/// <summary>
		/// The flag indicating if this instance is already simplified.
		/// </summary>
		protected bool isSimplified = false;

		/// <summary>
		/// Simplifies the Expression
		/// by reducing a common denominator and etc..
		/// </summary>
		/// <returns>result</returns>
		/// <remarks>
		/// This method has a kind-of cache mechanism,
		/// and returns the instance as-is if it has been already simplified.
		/// </remarks>
		public Expression Simplify()
		{
			if (this.isSimplified)
				return this;

			Expression e = SpecialOptimization(this);
			e = e.SimplifyImpl();
			e.isSimplified = true;
			return e;
		}

		/// <summary>
		/// Do special optimizations such as sin^2 + cos^2 -> 1.
		/// </summary>
		/// <param name="e">The target expression</param>
		/// <returns>The result</returns>
		static Expression SpecialOptimization(Expression e)
		{
			if (e.NodeType == ExpressionType.Multiply)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				if (l.IsIdentical(r))
				{
					if (l.NodeType == ExpressionType.Call)
					{
						CallExpression fe = (CallExpression)l;

						if (fe.Name == "Sin")
						{
							return 0.5 - 0.5 * Expression.Call("Cos", 2 * fe.Arguments[0]);
						}

						if (fe.Name == "Cos")
						{
							return 0.5 + 0.5 * Expression.Call("Cos", 2 * fe.Arguments[0]);
						}
					}
				}

				if (l.NodeType == ExpressionType.Call
					&& r.NodeType == ExpressionType.Call)
				{
					CallExpression fl = (CallExpression)l;
					CallExpression fr = (CallExpression)r;

					if (fl.Arguments.Count == 1 && fr.Arguments.Count == 1
						&& fl.Arguments[0].IsIdentical(fr.Arguments[0]))
					{
						if ((fl.Name == "Tan" && fr.Name == "Cos")
							|| (fl.Name == "Cos" && fr.Name == "Tan"))
						{
							return Expression.Call("Sin", fl.Arguments[0]);
						}

						if ((fl.Name == "Sin" && fr.Name == "Cos")
							|| (fl.Name == "Cos" && fr.Name == "Sin"))
						{
							return 0.5 * Expression.Call("Sin", 2 * fl.Arguments[0]);
						}
					}
				}
			}

			if (e.NodeType == ExpressionType.Divide)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				if (l.NodeType == ExpressionType.Call
					&& r.NodeType == ExpressionType.Call)
				{
					CallExpression fl = (CallExpression)l;
					CallExpression fr = (CallExpression)r;

					if (fl.Arguments.Count == 1 && fr.Arguments.Count == 1
						&& fl.Arguments[0].IsIdentical(fr.Arguments[0]))
					{
						if (fl.Name == "Tan" && fr.Name == "Sin")
						{
							return 1 / Expression.Call("Cos", fl.Arguments[0]);
						}
					}
				}
			}

			if (e.NodeType == ExpressionType.Call)
			{
				CallExpression fe1 = (CallExpression)e;
				if (fe1.Arguments.Count == 1
					&& fe1.Arguments[0].NodeType == ExpressionType.Call)
				{
					CallExpression fe2 = (CallExpression)fe1.Arguments[0];

					if (fe1.Name == "Log" && fe2.Name == "Exp")
						return fe2.Arguments[0];

					if (fe1.Name == "Exp" && fe2.Name == "Log"
						&& fe2.Arguments.Count == 1)
						return fe2.Arguments[0];
				}
			}

			return e;
		}

		#endregion
		#region Derive

		/// <summary>
		/// implementation body of Derive.
		/// </summary>
		/// <returns>derivative</returns>
		public abstract Expression DeriveImpl(string paramName);

		Dictionary<string, Expression> deriveCache = new Dictionary<string, Expression>();

		/// <summary>
		/// calculate symbolically a partial derivative with respect to i-th parameter.
		/// </summary>
		/// <typeparam name="T">type of Function</typeparam>
		/// <param name="e">expression to be differentiated</param>
		/// <param name="i">paramter index</param>
		/// <returns>total derivative of e</returns>
		/// <remarks>
		/// This method has a cache mechanism.
		/// </remarks>
		public Expression Derive(string paramName)
		{
			if (!this.deriveCache.Keys.Contains(paramName))
			{
				var e = this.Simplify();
				e = e.DeriveImpl(paramName).Simplify();
				this.deriveCache[paramName] = e;
				return e;
			}

			return this.deriveCache[paramName];
		}

		#endregion
		#region Term (inner type)

		/// <summary>
		/// express a term in styles like { constant, body }.
		/// e.g. 2 * x * 3 * y -> { 6, x * y }.
		/// </summary>
		protected class Term
		{
			public double Constant { get; set; }
			public Expression Body { get; set; }
			public Term(double c) { this.Constant = c; this.Body = null; }
			public Term(Expression b) { this.Constant = 1.0; this.Body = b; }
			public Term(double c, Expression b) { this.Constant = c; this.Body = b; }

			public Expression ToExpression()
			{
				if (this.Constant == 0)
					return Expression.Constant(0.0);
				if (this.Body == null)
					return Expression.Constant(this.Constant);
				if (this.Constant == 1)
					return this.Body;

				return Expression.Constant(this.Constant).Mul(this.Body);
			}

			public override string ToString()
			{
				return string.Format("{0} : {1}", this.Constant, this.Body);
			}
		}

		#endregion
		#region common terms cancellations

		/// <summary>
		/// Cancels common terms in sum.
		/// </summary>
		/// <param name="e">terget</param>
		/// <returns>result</returns>
		protected static Expression Cancel(Expression e)
		{
			List<Term> terms = new List<Term>();
			e.DeconstructSum(false, terms);
			return ConstructSum(terms);
		}

		List<Term> termsCaches = null;

		/// <summary>
		/// deconstruct sum into list.
		/// 4 * x + y - 2 * x -> { {2, x}, {1, y} }.
		/// </summary>
		/// <param name="minus">negate sign of e if minus == true</param>
		/// <param name="terms">list into which deconstructed terms are stored</param>
		/// <remarks>
		/// This method has a cache mechanism.
		/// </remarks>
		void DeconstructSum(bool minus, List<Term> terms)
		{
			if (this.termsCaches == null)
			{
				this.termsCaches = new List<Term>();
				DeconstructSum(this, false, this.termsCaches);
			}

			if (minus)
			{
				var negated =
					from x in this.termsCaches
					select new Term(-x.Constant, x.Body);
				AddWithFolding(terms, negated);
			}
			else
				AddWithFolding(terms, this.termsCaches);
		}

		/// <summary>
		/// add a new term into a list with folding.
		/// </summary>
		/// <param name="terms">term list</param>
		/// <param name="t">new term</param>
		static void AddWithFolding(List<Term> terms, Term t)
		{
			int i = terms.FindIndex(ti => IsIdentical(t.Body, ti.Body));
			if (i < 0)
				terms.Add(t);
			else
				terms[i].Constant += t.Constant;
		}

		/// <summary>
		/// add new terms into a list with folding.
		/// </summary>
		/// <param name="to">term list</param>
		/// <param name="adding">new terms</param>
		static void AddWithFolding(List<Term> to, IEnumerable<Term> toBeAdded)
		{
			foreach (var t in toBeAdded)
			{
				AddWithFolding(to, t);
			}
		}

		/// <summary>
		/// implementation body of DeconstructSum (of instance method).
		/// </summary>
		/// <param name="e">expression to be deconstructed</param>
		/// <param name="minus">negate sign of e if minus == true</param>
		/// <param name="terms">list into which deconstructed terms are stored</param>
		static void DeconstructSum(Expression e, bool minus, List<Term> terms)
		{
			if (e.NodeType == ExpressionType.Constant)
			{
				double c = ((ConstantExpression)e).Value;
				if (minus)
					AddWithFolding(terms, new Term(-c));
				else
					AddWithFolding(terms, new Term(c));
				return;
			}

			if (e.NodeType == ExpressionType.Parameter
				|| e.NodeType == ExpressionType.Call)
			{
				if (minus)
					AddWithFolding(terms, new Term(-1, e));
				else
					AddWithFolding(terms, new Term(e));
				return;
			}

			if (e.NodeType == ExpressionType.Negate)
			{
				var op = ((UnaryExpression)e).Operand;
				op.DeconstructSum(!minus, terms);
				return;
			}
			if (e.NodeType == ExpressionType.Add)
			{
				Expression l = ((BinaryExpression)e).Left.Simplify();
				Expression r = ((BinaryExpression)e).Right.Simplify();
				l.DeconstructSum(minus, terms);
				r.DeconstructSum(minus, terms);
				return;
			}
			if (e.NodeType == ExpressionType.Subtract)
			{
				Expression l = ((BinaryExpression)e).Left.Simplify();
				Expression r = ((BinaryExpression)e).Right.Simplify();
				l.DeconstructSum(minus, terms);
				r.DeconstructSum(!minus, terms);
				return;
			}

			Term t = e.FoldConstants();
			if (minus)
				t.Constant = -t.Constant;

			AddWithFolding(terms, t);
		}

		/// <summary>
		/// Constructs a sum from a term list.
		/// { { 1, x }, { 2, y }, { 3, z } } -> x + 2 * y + 3 * z.
		/// </summary>
		/// <param name="terms">list in which expressions are stored</param>
		/// <returns>sum of terms</returns>
		static Expression ConstructSum(List<Term> terms)
		{
#if false
			Expression sum = null;
			foreach (var t in terms)
			{
				sum = Add(sum, t);
			}
			return sum;
#else
			// find a common factor and pull it out.
			// computationally high cost.
			// ↓
			if (terms.Count == 1)
				return terms[0].ToExpression();

			var deconst = terms.Select(
				term =>
				{
					var num = new List<Expression>();
					var denom = new List<Expression>();

					if (term.Body == null)
						return new { num, denom };

					term.Body.DeconstructProduct(num, denom);
					num.Sort();
					denom.Sort();
					return new { num, denom };
				}).ToArray();

			var intersectNum = deconst.Aggregate(
				deconst[0].num, (c, s) => Util.Intersect(c, s.num)).ToList();
			//var intersectDenom = deconst.Aggregate(
			//	deconst[0].denom, (c, s) => Util.Intersect(c, s.denom)).ToList();

			for (int i = 0; i < terms.Count; ++i)
			{
				var d = deconst[i];
				Util.Remove(d.num, intersectNum);
				//Util.Remove(d.denom, intersectDenom);
				Term t = ConstructProduct(d.num, d.denom);
				terms[i].Constant *= t.Constant;
				terms[i].Body = t.Body;
			}

			var terms0 = (
				from term in terms
				group term.Constant by term.Body into g
				select new Term(g.Aggregate((c, s) => c + s), g.Key)
				).ToList();

			Expression sum = null;
			foreach (var t in terms0)
			{
				sum = Add(sum, t);
			}

			Term tNum = ConstructProduct(intersectNum);
			//Term tDenom = ConstructProduct(intersectDenom);
			//sum = Expression.Construct(sum, tNum, tDenom);
			sum = Expression.Multiply(sum, tNum);

			return sum;
#endif
		}

		/// <summary>
		/// Adds a Term to an Expression.
		/// </summary>
		/// <param name="e">operand 1</param>
		/// <param name="t">operand 2</param>
		/// <returns>result</returns>
		static Expression Add(Expression e, Term t)
		{
			if (e == null)
				return t.ToExpression().Simplify();

			if (t.Body == null)
			{
				if (t.Constant == 0)
					return e;

				return e.Add(Expression.Constant(t.Constant));
			}

			if (e.IsConstant(0))
				return t.ToExpression().Simplify();

			if (t.Constant == 0)
				return e;

			Expression body = t.ToExpression().Simplify();
			return e.Add(body);
		}

		/// <summary>
		/// Multiply an Exression by a Term.
		/// </summary>
		/// <param name="e">operand 1</param>
		/// <param name="t">operand 2</param>
		/// <returns>result</returns>
		static Expression Multiply(Expression e, Term t)
		{
			if (t.Body == null)
			{
				if (t.Constant == 1)
					return e;
				if (e.NodeType == ExpressionType.Constant)
				{
					return Expression.Constant(
						((ConstantExpression)e).Value * t.Constant);
				}
				return Expression.Multiply(Expression.Constant(t.Constant), e);
			}

			Expression body = t.Body.Simplify();

			if (t.Constant == 1)
				return Expression.Multiply(e, body);
			if (e.NodeType == ExpressionType.Constant)
			{
				return Expression.Multiply(
					Expression.Constant(
					((ConstantExpression)e).Value * t.Constant),
					body);
			}
			return Expression.Multiply(
				Expression.Multiply(
					Expression.Constant(t.Constant),
					e),
				body);
		}

		#endregion
		#region about reducing common denominators

		Term foldCache = null;

		/// <summary>
		/// Optimizes a term by folding constants.
		/// for example, 2 * x * 3 * x * 4 -> 24 * x * x.
		/// </summary>
		/// <returns>result</returns>
		/// <remarks>
		/// This method has a cache mechanism.
		/// </remarks>
		Term FoldConstants()
		{
			if (this.foldCache == null)
			{
				this.foldCache = FoldConstants(this);
			}
			return this.foldCache;
		}

		/// <summary>
		/// implementation body of FoldConstants (of instance method).
		/// </summary>
		/// <param name="e">Expression to be optimized</param>
		static Term FoldConstants(Expression e)
		{
			List<Expression> n = new List<Expression>();
			List<Expression> d = new List<Expression>();
			e.DeconstructProduct(n, d);
			return ConstructProduct(n, d);
		}

		/// <summary>
		/// reduce a common denominator.
		/// </summary>
		/// <param name="e">terget</param>
		/// <returns>result</returns>
		protected static Expression Reduce(Expression e)
		{
			return FoldConstants(e).ToExpression();
		}

		List<Expression> numCaches = null;
		List<Expression> denomCaches = null;

		/// <summary>
		/// deconstruct product into list.
		/// x / a * y * z / b / c -> num = {x, y, z}, denom = {a, b, c}.
		/// </summary>
		/// <param name="e">expression to be deconstructed</param>
		/// <param name="list">list into which deconstructed expressions are stored</param>
		/// <remarks>
		/// This method has a cache mechanism.
		/// </remarks>
		protected void DeconstructProduct(List<Expression> num, List<Expression> denom)
		{
			if (this.numCaches == null)
			{
				this.numCaches = new List<Expression>();
				this.denomCaches = new List<Expression>();
				DeconstructProduct(this, this.numCaches, this.denomCaches);
			}

			num.AddRange(this.numCaches);
			denom.AddRange(this.denomCaches);
		}

		/// <summary>
		/// implementation body of DeconstructProduct (of instance method).
		/// </summary>
		/// <param name="e">expression to be deconstructed</param>
		/// <param name="list">list into which deconstructed expressions are stored</param>
		static void DeconstructProduct(Expression e, List<Expression> num, List<Expression> denom)
		{
			if (e == null) return;

			if (e.NodeType == ExpressionType.Constant
				|| e.NodeType == ExpressionType.Parameter)
			{
				num.Add(e);
				return;
			}

			if (e.NodeType == ExpressionType.Call)
			{
				num.Add(e.Simplify());
				return;
			}

			if (e.NodeType == ExpressionType.Multiply)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				l.DeconstructProduct(num, denom);
				r.DeconstructProduct(num, denom);
				return;
			}

			if (e.NodeType == ExpressionType.Divide)
			{
				Expression l = ((BinaryExpression)e).Left;
				Expression r = ((BinaryExpression)e).Right;

				l.DeconstructProduct(num, denom);
				r.DeconstructProduct(denom, num);
				return;
			}

			Expression simplified = e.Simplify();

			// The result of e.Simplify() could be a form of
			// a * x, a / x or a * x / y where a is constant.
			if (simplified.NodeType == ExpressionType.Multiply)
			{
				Expression left = ((BinaryExpression)simplified).Left;
				Expression right = ((BinaryExpression)simplified).Right;
				num.Add(left);
				if (right.NodeType == ExpressionType.Divide)
				{
					left = ((BinaryExpression)right).Left;
					right = ((BinaryExpression)right).Right;
					num.Add(left);
					denom.Add(right);
				}
				else
					num.Add(right);
			}
			else if (simplified.NodeType == ExpressionType.Divide)
			{
				Expression left = ((BinaryExpression)simplified).Left;
				Expression right = ((BinaryExpression)simplified).Right;
				num.Add(left);
				denom.Add(right);
			}
			else
				num.Add(simplified);
		}

		/// <summary>
		/// Constructs a product from a list.
		/// {x, y, z} -> x * y * z.
		/// </summary>
		/// <param name="list">A list containing expressions</param>
		static Term ConstructProduct(IEnumerable<Expression> list)
		{
			double c = 1;
			Expression prod = null;
			foreach (var e in list)
			{
				if (e == null) continue;

				if (e.IsConstant())
					c *= (double)((ConstantExpression)e).Value;
				else if (prod == null)
					prod = e;
				else
					prod = prod.Mul(e);
			}
			return new Term(c, prod);
		}

		/// <summary>
		/// Constructs a fraction from lists.
		/// num = {x, y, z}, denom = {a, b, c} -> (x * y * z) / (a * b * c).
		/// </summary>
		/// <param name="num">A list containing expressions of numerators</param>
		/// <param name="denom">A list containing expressions of denominators</param>
		static Term ConstructProduct(List<Expression> num, List<Expression> denom)
		{
			double c = 1;

			num.Sort();
			denom.Sort();
			List<Expression> intersect = Util.Intersect(num, denom);
			Util.Remove(num, intersect);
			Util.Remove(denom, intersect);

			Term n = ConstructProduct(num);
			Term d = ConstructProduct(denom);
			n.Constant *= c;

			return Div(n, d);
		}

		/// <summary>
		/// divide terms with optimization such as x / x -> 1.
		/// </summary>
		/// <param name="t1">operand 1</param>
		/// <param name="t2">operand 2</param>
		/// <returns>result</returns>
		static Term Div(Term t1, Term t2)
		{
			double c1 = t1.Constant;
			double c2 = t2.Constant;
			Expression b1 = t1.Body;
			Expression b2 = t2.Body;

			if (c1 == 0)
				return new Term(0);

			double c = c1 / c2;

			if (b1 == null)
			{
				if (b2 == null)
					return new Term(c);
				return new Term(Expression.Constant(c).Div(b2));
			}
			if (b2 == null)
				return new Term(c, b1);

			if (b1.IsIdentical(b2))
			{
				return new Term(c);
			}

			return new Term(c, b1.Div(b2));
		}

		#endregion
		#region IComparable<Expression> member

		public abstract int CompareTo(object other);

		#endregion
	}
}
