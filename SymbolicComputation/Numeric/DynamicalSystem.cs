using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expresso.Numeric
{
  /// <summary>
  /// 自励系の微分方程式を数値的にといて、解軌跡を得る。
  /// </summary>
  /// <remarks>
  /// 4次のルンゲクッタ法で逐次計算。
  /// </remarks>
  public class DynamicalSystem
  {
    #region protected フィールド

    /// <summary>
    /// 位相空間上の座標変数。
    /// </summary>
    Vector q;

    /// <summary>
    /// 自励系の右辺の関数。
    /// (d/dt)q = f(q)。
    /// </summary>
		VectorFunction f;

    #endregion
    #region 初期化

		public DynamicalSystem()
			: this(null, null) { }

		public DynamicalSystem(VectorFunction f, Vector initQ)
		{
			this.q = initQ;
			this.f = f;
		}

    #endregion
    #region プロパティ

		public VectorFunction Function
		{
			get { return this.f; }
			set { this.f = value; }
		}

		public Vector Current
		{
			get { return this.q; }
			protected set { this.q = value; }
		}

    #endregion
    #region 数値計算本体

    /// <summary>
    /// Δt 分更新。
    /// </summary>
    /// <param name="dt">Δt</param>
    public void Update(double dt)
    {
			if (this.f == null)
				return;

      /* ガウス法
        this.q += dt * this.f.GetValue(this.q);
      // */

			/* 中点法
			Vector k;
			k = dt * this.f.GetValue(this.q);
			k = dt * this.f.GetValue(this.q + k / 2);
			this.q += k;
			// */

			//* ルンゲクッタ法
			Vector k1, k2, k3, k4;
			k1 = dt * this.f.GetValue(this.q);
			k2 = dt * this.f.GetValue(this.q + k1 / 2);
			k3 = dt * this.f.GetValue(this.q + k2 / 2);
			k4 = dt * this.f.GetValue(this.q + k3);

			this.q += (k1 + 2 * (k2 + k3) + k4) / 6;
			// */
    }

    #endregion
  }
}
