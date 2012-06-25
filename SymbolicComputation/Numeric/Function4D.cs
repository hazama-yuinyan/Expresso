using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Func4 = System.Func<double, double, double, double, double>;

namespace Expresso.Numeric
{
	public class Vector4D : Vector
	{
		public Vector4D(double x0, double x1, double x2, double x3)
		{
			this.X0 = x0;
			this.X1 = x1;
			this.X2 = x2;
			this.X3 = x3;
		}

		public double X0 { get; set; }
		public double X1 { get; set; }
		public double X2 { get; set; }
		public double X3 { get; set; }

		Vector4D Add(Vector4D v)
		{
			return new Vector4D(this.X0 + v.X0, this.X1 + v.X1, this.X2 + v.X2, this.X3 + v.X3);
		}

		Vector4D Sub(Vector4D v)
		{
			return new Vector4D(this.X0 - v.X0, this.X1 - v.X1, this.X2 - v.X2, this.X3 - v.X3);
		}

		public double Abs()
		{
			double abs = this.X0 * this.X0;
			abs += this.X1 * this.X1;
			abs += this.X2 * this.X2;
			abs += this.X3 * this.X3;
			return Math.Sqrt(abs);
		}

		public override string ToString()
		{
			return string.Format("({0}, {1}, {2}, {3})", this.X0, this.X1, this.X2, this.X3);
		}

		#region Vector メンバ

		protected override Vector Add(Vector v)
		{
			return this.Add((Vector4D)v);
		}

		protected override Vector Sub(Vector v)
		{
			return this.Sub((Vector4D)v);
		}

		protected override Vector Mul(double c)
		{
			return new Vector4D(c * this.X0, c * this.X1, c * this.X2, c * this.X3);
		}

		protected override Vector Div(double c)
		{
			return new Vector4D(this.X0 / c, this.X1 / c, this.X2 / c, this.X3 / c);
		}

		#endregion
	}

	public class Function4D : VectorFunction
	{
		Func4 f0, f1, f2, f3;

		public Function4D(
			Func4 f0,
			Func4 f1,
			Func4 f2,
			Func4 f3)
		{
			this.f0 = f0;
			this.f1 = f1;
			this.f2 = f2;
			this.f3 = f3;
		}

		public Vector4D GetValue(Vector4D v)
		{
			return new Vector4D(
				this.f0(v.X0, v.X1, v.X2, v.X3),
				this.f1(v.X0, v.X1, v.X2, v.X3),
				this.f2(v.X0, v.X1, v.X2, v.X3),
				this.f3(v.X0, v.X1, v.X2, v.X3));
		}

		#region VectorFunction メンバ

		Vector VectorFunction.GetValue(Vector v)
		{
			return this.GetValue((Vector4D)v);
		}

		#endregion
	}
}
