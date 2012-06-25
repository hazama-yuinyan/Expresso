using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expresso.Numeric
{
	public abstract class Vector
	{
		protected abstract Vector Add(Vector v);
		protected abstract Vector Sub(Vector v);
		protected abstract Vector Mul(double c);
		protected abstract Vector Div(double c);

		public static Vector operator +(Vector x, Vector y)
		{
			return x.Add(y);
		}

		public static Vector operator -(Vector x, Vector y)
		{
			return x.Sub(y);
		}

		public static Vector operator *(Vector x, double c)
		{
			return x.Mul(c);
		}

		public static Vector operator *(double c, Vector x)
		{
			return x.Mul(c);
		}

		public static Vector operator /(Vector x, double c)
		{
			return x.Div(c);
		}
	}

	public interface VectorFunction
	{
		Vector GetValue(Vector v);
	}
}
