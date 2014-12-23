using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace Expresso.Utils
{
	/// <summary>
	/// 組み込みの配列型に対する拡張メソッド定義。
	/// Array utils.
	/// </summary>
	public static class ArrayUtils
	{
		internal sealed class FunctorComparer<T> : IComparer<T>
		{
			readonly Comparison<T> comparison;
			
			public FunctorComparer(Comparison<T> comparison)
			{
				Assert.NotNull(comparison);
				this.comparison = comparison;
			}
			
			public int Compare(T x, T y)
			{
				return comparison(x, y);
			}
		}
		
		public static readonly string[] EmptyStrings = new string[0];
		public static readonly object[] EmptyObjects = new object[0];
		
		public static IComparer<T> ToComparer<T>(Comparison<T> comparison)
		{
			return new FunctorComparer<T>(comparison);
		}
		
		public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] input, Converter<TInput, TOutput> conv)
		{
			return System.Array.ConvertAll<TInput, TOutput>(input, conv);
		}
		
		public static T[] FindAll<T>(T[] array, Predicate<T> match)
		{
			return System.Array.FindAll(array, match);
		}
		
		public static void PrintTable(StringBuilder output, string[,] table)
		{
			ContractUtils.RequiresNotNull(output, "output");
			ContractUtils.RequiresNotNull(table, "table");
			
			int max_width = 0;
			for(int i = 0; i < table.GetLength(0); ++i){
				if(table[i, 0].Length > max_width)
					max_width = table[i, 0].Length;
			}
			
			for(int i = 0; i < table.GetLength(0); ++i){
				output.Append(" ");
				output.Append(table[i, 0]);
				
				for(int j = table[i, 0].Length; j < max_width + 1; ++j)
					output.Append(' ');
				
				output.AppendLine(table[i, 1]);
			}
		}
		
		public static T[] Copy<T>(T[] array)
		{
			return (array.Length > 0) ? (T[])array.Clone() : array;
		}
		
		/// <summary>
		/// Converts a generic ICollection of T into an array of R using a given conversion.  
		/// 
		/// If the collection is already an array of R the original collection is returned.
		/// </summary>
		public static TResult[] ToArray<TElement, TResult>(ICollection<TElement> list, Func<TElement, TResult> convertor)
		{
			TResult[] res = list as TResult[];
			if(res == null){
				res = new TResult[list.Count];
				int i = 0;
				foreach(TElement obj in list)
					res[i++] = convertor(obj);
			}
			return res;
		}
		
		public static T[] MakeArray<T>(ICollection<T> list)
		{
            if(list.Count == 0)
                return new T[0];
			
            T[] res = new T[list.Count];
            list.CopyTo(res, 0);
            return res;
        }

		
        public static T[] MakeArray<T>(ICollection<T> elements, int reservedSlotsBefore, int reservedSlotsAfter)
		{
			if(reservedSlotsAfter < 0) throw new ArgumentOutOfRangeException("reservedSlotsAfter");
			if(reservedSlotsBefore < 0) throw new ArgumentOutOfRangeException("reservedSlotsBefore");
			
			if(elements == null)
				return new T[reservedSlotsBefore + reservedSlotsAfter];
			
			T[] result = new T[reservedSlotsBefore + elements.Count + reservedSlotsAfter];
			elements.CopyTo(result, reservedSlotsBefore);
			return result;
		}
		
		public static T[] RotateRight<T>(T[] array, int count)
		{
			ContractUtils.RequiresNotNull(array, "array");
			if ((count < 0) || (count > array.Length)) throw new ArgumentOutOfRangeException("count");
			
			T[] result = new T[array.Length];
			// The head of the array is shifted, and the tail will be rotated to the head of the resulting array
			int sizeOfShiftedArray = array.Length - count;
			Array.Copy(array, 0, result, count, sizeOfShiftedArray);
			Array.Copy(array, sizeOfShiftedArray, result, 0, count);
			return result;
		}
		
		public static T[] ShiftRight<T>(T[] array, int count)
		{
			ContractUtils.RequiresNotNull(array, "array");
			if(count < 0) throw new ArgumentOutOfRangeException("count");
			
			T[] result = new T[array.Length + count];
			System.Array.Copy(array, 0, result, count, array.Length);
			return result;
		}
		
		public static T[] ShiftLeft<T>(T[] array, int count)
		{
			ContractUtils.RequiresNotNull(array, "array");
			if(count < 0) throw new ArgumentOutOfRangeException("count");
			
			T[] result = new T[array.Length - count];
			System.Array.Copy(array, count, result, 0, result.Length);
			return result;
		}
		
		public static T[] Insert<T>(T item, IList<T> list)
		{
			T[] res = new T[list.Count + 1];
			res[0] = item;
			list.CopyTo(res, 1);
			return res;
		}
		
		public static T[] Insert<T>(T item1, T item2, IList<T> list)
		{
			T[] res = new T[list.Count + 2];
			res[0] = item1;
			res[1] = item2;
			list.CopyTo(res, 2);
			return res;
		}
		
		public static T[] Insert<T>(T item, T[] array)
		{
			T[] result = ShiftRight(array, 1);
			result[0] = item;
			return result;
		}
		
		public static T[] Insert<T>(T item1, T item2, T[] array)
		{
			T[] result = ShiftRight(array, 2);
			result[0] = item1;
			result[1] = item2;
			return result;
		}
		
		public static T[] Append<T>(T[] array, T item)
		{
			ContractUtils.RequiresNotNull(array, "array");
			
			System.Array.Resize<T>(ref array, array.Length + 1);
			array[array.Length - 1] = item;
			return array;
		}
		
		public static T[] AppendRange<T>(T[] array, IList<T> items)
		{
			return AppendRange<T>(array, items, 0);
		}
		
		public static T[] AppendRange<T>(T[] array, IList<T> items, int additionalItemCount)
		{
			ContractUtils.RequiresNotNull(array, "array");
			if(additionalItemCount < 0) throw new ArgumentOutOfRangeException("additionalItemCount");
			
			int j = array.Length;
			
			System.Array.Resize<T>(ref array, array.Length + items.Count + additionalItemCount);
			
			for(int i = 0; i < items.Count; ++i, ++j)
				array[j] = items[i];
			
			return array;
		}
		
		public static T[] RemoveFirst<T>(IList<T> list)
		{
			return ShiftLeft(MakeArray(list), 1);
		}
		
		public static T[] RemoveFirst<T>(T[] array)
		{
			return ShiftLeft(array, 1);
		}
		
		public static T[] RemoveLast<T>(T[] array)
		{
			ContractUtils.RequiresNotNull(array, "array");
			
			System.Array.Resize(ref array, array.Length - 1);
			return array;
		}

		/// <summary>
		/// Removes the last element of the list.
		/// </summary>
		/// <param name='list'>
		/// The target List.
		/// </param>
		/// <typeparam name='T'>
		/// The element type of the list.
		/// </typeparam>
		public static void RemoveLast<T>(this IList<T> list)
		{
			var len = list.Count;
			if(len <= 0) throw ExpressoOps.MakeSystemError("Can not delete last element of a list with zero elements!");
			list.RemoveAt(len - 1);
		}
		
		public static T[] RemoveAt<T>(IList<T> list, int indexToRemove)
		{
			return RemoveAt(MakeArray(list), indexToRemove);
		}
		
		public static T[] RemoveAt<T>(T[] array, int indexToRemove)
		{
			ContractUtils.RequiresNotNull(array, "array");
			ContractUtils.Requires(indexToRemove >= 0 && indexToRemove < array.Length, "index");
			
			T[] result = new T[array.Length - 1];
			if(indexToRemove > 0)
				Array.Copy(array, 0, result, 0, indexToRemove);

			int remaining = array.Length - indexToRemove - 1;
			if(remaining > 0)
				Array.Copy(array, array.Length - remaining, result, result.Length - remaining, remaining);

			return result;
		}
		
		public static T[] InsertAt<T>(IList<T> list, int index, params T[] items)
		{
			return InsertAt(MakeArray(list), index, items);
		}
		
		public static T[] InsertAt<T>(T[] array, int index, params T[] items)
		{
			ContractUtils.RequiresNotNull(array, "array");
			ContractUtils.RequiresNotNull(items, "items");
			ContractUtils.Requires(index >= 0 && index <= array.Length, "index");
			
			if(items.Length == 0)
				return Copy(array);
			
			T[] result = new T[array.Length + items.Length];
			if(index > 0)
				Array.Copy(array, 0, result, 0, index);

			Array.Copy(items, 0, result, index, items.Length);
			
			int remaining = array.Length - index;
			if(remaining > 0)
				Array.Copy(array, array.Length - remaining, result, result.Length - remaining, remaining);

			return result;
		}
		
		public static bool ValueEquals<T>(this T[] array, T[] other)
		{
			if(other.Length != array.Length)
				return false;
			
			for(int i = 0; i < array.Length; ++i){
				if(!Object.Equals(array[i], other[i]))
					return false;
			}
			
			return true;
		}
		
		public static int GetValueHashCode<T>(this T[] array)
		{
			return GetValueHashCode<T>(array, 0, array.Length);
		}
		
		public static int GetValueHashCode<T>(this T[] array, int start, int count)
		{
			ContractUtils.RequiresNotNull(array, "array");
			ContractUtils.RequiresArrayRange(array.Length, start, count, "start", "count");
			
			if(count == 0)
				return 0;
			
			int result = array[start].GetHashCode();
			for(int i = 1; i < count; ++i)
				result = ((result << 5) | (result >> 27)) ^ array[start + i].GetHashCode();
			
			return result;
		}
		
		public static T[] Reverse<T>(this T[] array)
		{
			T[] res = new T[array.Length];
			for(int i = 0; i < array.Length; ++i)
				res[array.Length - i - 1] = array[i];

			return res;
		}

		public static T[] MakeDuplicatedArray<T>(T element, int count)
		{
			var ret = new T[count];
			for(int i = count; i > 0; --i) ret[i] = element;
			return ret;
		}
	}
}

