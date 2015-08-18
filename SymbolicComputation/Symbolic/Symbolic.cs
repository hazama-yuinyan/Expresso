using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using L = System.Linq.Expressions;
using ParamDictionary = System.Collections.Generic.Dictionary<string, System.Linq.Expressions.ParameterExpression>;

namespace Expresso.Symbolic
{
	public class Simplified : SymbolicProxy
	{

	}

	public class Expanded : SymbolicProxy
	{

	}

	public class Symbolic : SymbolicProxy
	{
		public static bool AutoExpand{get; internal set;}
		public static int SubstCount{get; internal set;}

		#region constructors
		public Symbolic();
		public Symbolic(Symbolic other);
		public Symbolic(int n);
		public Symbolic(double n);
		public Symbolic(string n);
		#endregion

		public override Symbolic Subst(Symbolic a, Symbolic x, int count)
		{
			return base.Subst(a, x, count);
		}

		public override Simplified Simplify()
		{

		}

		public override int CompareTo(object other)
		{

		}

		public override Symbolic Df(Symbolic x)
		{

		}

		public override Symbolic Integrate(Symbolic x)
		{

		}

		public override Symbolic Coeff(Symbolic x)
		{

		}

		public override Expanded Expand()
		{

		}

		public override int Commute(Symbolic x)
		{

		}
	}
}
