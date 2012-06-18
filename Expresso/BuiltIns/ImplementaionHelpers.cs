using System;
using Expresso.BuiltIns;

namespace Expresso.Helpers
{
	/// <summary>
	/// Expressoの実装用のヘルパー関数郡。
	/// </summary>
	public static class ImplementaionHelpers
	{
		public static bool IsOfType(ExpressoObj target, TYPES type)
		{
			return target.Type == type;
		}
	}

	public interface SequenceGenerator<C, V>
	{
		V Generate();
		C Take(int count);
	}

	public class ListGenerator : SequenceGenerator<ExpressoList, ExpressoObj>
	{
		public ExpressoObj Generate()
		{
			return null;
		}

		public ExpressoList Take(int count)
		{
			return null;
		}
	}
}

