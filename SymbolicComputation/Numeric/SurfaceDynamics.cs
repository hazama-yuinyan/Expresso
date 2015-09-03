using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Func2 = System.Func<double, double, double>;
using Func4 = System.Func<double, double, double, double, double>;

using Expression = System.Linq.Expressions.Expression;
using Expression2 = System.Linq.Expressions.Expression<System.Func<double, double, double>>;
using Expression4 = System.Linq.Expressions.Expression<System.Func<double, double, double, double, double>>;
using Expresso.Symbolic;

namespace Expresso.Numeric
{
  /// <summary>
  /// 曲面上（2変数）のハミルトン系の1体問題。
  /// </summary>
  /// <remarks>
  /// 親クラスの座標変数 vq、自励関数 f は、
  /// 一般化座標   q1 = vq[0], q2 = vq[1]、
  /// 一般化運動量 p1 = vq[2], p2 = vq[3]。
  /// ハミルトニアンを H として、
  /// f[0] =  (∂/∂p1)H、
  /// f[1] =  (∂/∂p2)H、
  /// f[2] = -(∂/∂q1)H、
  /// f[3] = -(∂/∂q2)H。
  /// </remarks>
  public class SurfaceDynamics : DynamicalSystem
  {
    #region フィールド

		/// <summary>
		/// 準備 OK かどうか。
		/// </summary>
		public bool IsRead
		{
			get
			{
				return this.X != null
					&& this.Y != null
					&& this.Z != null
					&& this.Phi != null;
			}
		}

    /// <summary>
    /// 一般化座標 → 直交座標の変換関数 x(q1, q2)。
    /// </summary>
		public Func2 X { set; get; }

    /// <summary>
    /// 一般化座標 → 直交座標の変換関数 y(q1, q2)。
    /// </summary>
		public Func2 Y { set; get; }

    /// <summary>
    /// 一般化座標 → 直交座標の変換関数 z(q1, q2)。
    /// </summary>
		public Func2 Z { set; get; }

    /// <summary>
    /// ポテンシャル φ(q1, q2)。
    /// </summary>
		public Func2 Phi { set; get; }

    /// <summary>
    /// 質点の質量。
    /// </summary>
    public double M { set; get; }

    #endregion
    #region 初期化

    public SurfaceDynamics(
			Expression2 x, Expression2 y, Expression2 z,
			Expression2 phi,
			double initQ1, double initQ2, double initP1, double initP2,
			double m)
		{
      this.M = m;
			this.SetCurrent(initQ1, initQ2, initP1, initP2);
			this.SetFunctions(x, y, z, phi);
    }

		public SurfaceDynamics()
		{
			this.M = 0;
			this.Current = new Vector4D(0, 0, 0, 0);
			this.Function = null;
		}

    #endregion
    #region プロパティ

		public new Vector4D Current
		{
			get
			{
				return (Vector4D)base.Current;
			}
			set
			{
				base.Current = value;
			}
		}

    public double CurrentQ1 { get { return this.Current.X0; } }
    public double CurrentQ2 { get { return this.Current.X1; } }
    public double CurrentP1 { get { return this.Current.X2; } }
    public double CurrentP2 { get { return this.Current.X3; } }

    public double CurrentX
    {
      get
      {
        return this.X(this.CurrentQ1, this.CurrentQ2);
      }
    }

    public double CurrentY
    {
      get
      {
				return this.Y(this.CurrentQ1, this.CurrentQ2);
      }
    }

    public double CurrentZ
    {
      get
      {
				return this.Z(this.CurrentQ1, this.CurrentQ2);
      }
    }

    #endregion
		#region 自励関数計算

		public void SetCurrent(
			double initQ1, double initQ2, double initP1, double initP2)
		{
			this.Current = new Vector4D(initQ1, initQ2, initP1, initP2);
		}

    /// <summary>
    /// X, Y, Z, Phi から自励関数 f(q) を計算。
    /// </summary>
		public void SetFunctions(
			Expression2 x, Expression2 y, Expression2 z,
			Expression2 phi)
    {
			var x_ = new Expresso.Symbolic.Lambda(x);
			var y_ = new Expresso.Symbolic.Lambda(y);
			var z_ = new Expresso.Symbolic.Lambda(z);
			var phi_ = new Expresso.Symbolic.Lambda(phi);

			this.X = (Func2)x_.Compile();
			this.Y = (Func2)y_.Compile();
			this.Z = (Func2)z_.Compile();
			this.Phi = (Func2)phi_.Compile();

			var x1 = x_.Derive(0);
			var y1 = y_.Derive(0);
			var z1 = z_.Derive(0);
			var x2 = x_.Derive(1);
			var y2 = y_.Derive(1);
			var z2 = z_.Derive(1);

			/*
			var g111 = x1 * x1;
			var g112 = y1 * y1;
			var g113 = g111 + g112;
			var g114 = g113.Simplify();
			var g115 = g114.Simplify();
			*/

			var g11 = x1 * x1 + y1 * y1 + z1 * z1; g11 = g11.Simplify();
			var g12 = x1 * x2 + y1 * y2 + z1 * z2; g12 = g12.Simplify();
			var g22 = x2 * x2 + y2 * y2 + z2 * z2; g22 = g22.Simplify();

			var det = g11 * g22 - g12 * g12; det = det.Simplify();

			var gi11 = g22 / det; gi11 = gi11.Simplify();
			var gi12 = -g12 / det; gi12 = gi12.Simplify();
			var gi22 = g11 / det; gi22 = gi22.Simplify();

			var f = new Lambda[4];

			var p1_ = new Lambda((p1, p2) => p1);
			var p2_ = new Lambda((p1, p2) => p2);

			var f0 = (
				Lambda.ExtendedMultiply(gi11, p1_) +
				Lambda.ExtendedMultiply(gi12, p2_)
				) / this.M;
			f0 = f0.Simplify();

			var f1 = (
				Lambda.ExtendedMultiply(gi12, p1_) +
				Lambda.ExtendedMultiply(gi22, p2_)
				) / this.M;
			f1 = f1.Simplify();

			var p11 = p1_ * p1_;
			var p12 = 2 * p1_ * p2_;
			var p22 = p2_ * p2_;
			var dummy = new Lambda((p1, p2) => 1.0);

			var fp =
					-(
					Lambda.ExtendedMultiply(gi11, p11) +
					Lambda.ExtendedMultiply(gi12, p12) +
					Lambda.ExtendedMultiply(gi22, p22)
					) / (2 * this.M) -
					Lambda.ExtendedMultiply(phi_, dummy);

			var f2 = fp.Derive(0);
			var f3 = fp.Derive(1);

			Function4D f4d = new Function4D(
				(Func4)f0.Compile(),
				(Func4)f1.Compile(),
				(Func4)f2.Compile(),
				(Func4)f3.Compile());

			this.Function = f4d;
		}

    #endregion
  }
}
