using System;
using System.Collections.Generic;


namespace Expresso.Interpreter
{
	/// <summary>
	/// A stack that is capable of referencing any element in the stack as well as the general operations stacks support
	/// </summary>
	public class ValueStack
	{
		private List<object> list;

		public int Size{
			get{
				return list.Count;
			}
		}

		public ValueStack(int initialCapacity)
		{
			list = new List<object>(initialCapacity);
		}

		public object Top()
		{
			return (list.Count > 0) ? list[list.Count - 1] : null;
		}

		public object Pop()
		{
			if(list.Count > 0){
				var elem = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				return elem;
			}
			return null;
		}

		public IEnumerable<object> PopRange(int max)
		{
			if(list.Count < max)
				return null;

			return null;
			//var range = from item in list
			//	where 
		}

		public void Push(object elem)
		{
			if(list.Count >= list.Capacity)
				list.Capacity = list.Capacity * 2;

			list.Add(elem);
		}

		public void Clear()
		{
			list.Clear();
		}

		public void Resize(int size)
		{
			if(list.Capacity < size)
				list.Capacity = size;
		}

		public object PeekAt(int index)
		{
			if(index > list.Count)
				throw new ArgumentOutOfRangeException("index");

			return list[index];
		}
	}
}

