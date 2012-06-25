using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Expresso.BuiltIns;

namespace Expresso.Helpers
{
	/// <summary>
	/// Expressoの実装用のヘルパー関数郡。
	/// Helper functions for implementing Expresso.
	/// </summary>
	public static class ImplementaionHelpers
	{
		/// <summary>
		/// Determines whether the <paramref name="target"/> is of the specified type.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the <paramref name="target"/> is of the specified type; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='target'>
		/// An ExpressoObject to be tested.
		/// </param>
		/// <param name='type'>
		/// The target type that the object will be tested against.
		/// </param>
		public static bool IsOfType(ExpressoObj target, TYPES type)
		{
			return target.Type == type;
		}

		public static string Replace(this string str, Regex reg, string replacedWith)
		{
			return reg.Replace(str, replacedWith);
		}

		public static string Replace(this string str, Regex reg, MatchEvaluator replacer)
		{
			return reg.Replace(str, replacer);
		}
	}

	/// <summary>
	/// ExpressoのSequenceを生成するクラスの実装用のインターフェイス。
	/// </summary>
	public interface SequenceGenerator<C, V>
	{
		V Generate();
		C Take(int count);
	}

	/// <summary>
	/// リストのジェネレーター。Comprehension構文から生成される。
	/// </summary>
	public class ListGenerator : SequenceGenerator<ExpressoList, ExpressoObj>
	{
		public ExpressoObj Generate()
		{
			return null;
		}

		public ExpressoList Take(int count)
		{
			var tmp = new List<ExpressoObj>();
			for(int i = 0; i < count; ++i){

			}
			return null;
		}
	}
}

