using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expresso
{
	public static class Util
	{
		#region list operations

		public static List<T> Intersect<T>(List<T> x, List<T> y)
			where T : IComparable
		{
			List<T> intersect = new List<T>();
			for (int i = 0, j = 0; i < x.Count && j < y.Count; )
			{
				var xi = x[i];
				var yj = y[j];
				int comp = xi.CompareTo(yj);
				if (comp < 0) ++i;
				else if (comp > 0) ++j;
				else
				{
					intersect.Add(x[i]);
					++i; ++j;
				}
			}
			return intersect;
		}

		public static void Remove<T>(List<T> x, List<T> remove)
			where T : IComparable
		{
			for (int i = 0, j = 0; i < x.Count && j < remove.Count; )
			{
				var xi = x[i];
				var yj = remove[j];
				int comp = xi.CompareTo(yj);
				if (comp < 0) ++i;
				else if (comp > 0) ++j;
				else
				{
					x[i] = default(T);
					++i; ++j;
				}
			}

			x.RemoveAll(p => p == null);
		}

		#endregion
	}
}
