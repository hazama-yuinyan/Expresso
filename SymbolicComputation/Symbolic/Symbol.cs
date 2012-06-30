using System;
using System.Collections.Generic;


namespace Expresso.Symbolic
{
	public class Symbol : CloningSymbolicInterface
	{
		public string Name{get; internal set;}
		public List<Symbolic> Parameters{get; internal set;}

		#region constructors
		public Symbol (Symbol s)
		{
		}

		public Symbol(string name, int count)
		{

		}
		#endregion

		public override Symbolic Subst(Symbolic a, Symbolic x, int count)
		{
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

