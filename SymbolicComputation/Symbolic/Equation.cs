using System;
using System.Collections.Generic;



namespace Expresso.Symbolic
{
	public class Equation : CloningSymbolicInterface
	{
		public Symbolic Lhs{get; internal set;}
		public Symbolic Rhs{get; internal set;}
		public List<Symbolic> Free{get; internal set;}

		#region constructors
		public Equation(Equation s) : base(s)
		{
			Lhs = s.Lhs;
			Rhs = s.Rhs;
			Free = s.Free;
		}

		public Equation(Equation s, Symbolic x) : this(s)
		{
			Free.Add(x);
		}

		public Equation(Symbolic s1, Symbolic s2)
		{
			Lhs = s1;
			Rhs = s2;
		}
		#endregion
	}
}

