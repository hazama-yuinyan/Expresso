using System;



namespace Expresso.Symbolic
{
	public abstract class SymbolicInterface : IComparable
	{
		public bool IsSimplified{get; internal set;}
		public bool IsExpanded{get; internal set;}
		public abstract string ToString();
		public abstract Symbolic Subst(Symbolic a, Symbolic x, int n);
		public abstract Simplified Simplify();
		public abstract int CompareTo(object other);
		public abstract Symbolic Df(Symbolic x);
		public abstract Symbolic Integrate(Symbolic x);
		public abstract Symbolic Coeff(Symbolic x);
		public abstract Expanded Expand();
		public abstract int Commute(Symbolic x);
	}

	public abstract class CloningSymbolicInterface : ICloneable
	{

	}

	public abstract class SymbolicProxy : SymbolicInterface
	{
		public string ToString();
		public Symbolic Subst(Symbolic a, Symbolic x, int n);
		public Simplified Simplify();
		public int CompareTo(object other);
		public Symbolic Df(Symbolic x);
		public Symbolic Integrate(Symbolic x);
		public Symbolic Coeff(Symbolic x);
		public Expanded Expand();
		public int Commute(Symbolic x);
	}
}

