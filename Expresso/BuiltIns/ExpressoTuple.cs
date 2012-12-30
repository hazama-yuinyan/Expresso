using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Expresso.Runtime;
using Expresso.Runtime.Operations;

namespace Expresso.Builtins
{
	/// <summary>
	/// Expresso組み込みのtupleオブジェクト。
	/// The built-in Tuple.
	/// </summary>
	/// <seealso cref="ExpressoContainer"/>
	[DebuggerTypeProxy(typeof(CollectionDebugProxy)), DebuggerDisplay("tuple, {Count} items")]
	[ExpressoType("tuple")]
	public class ExpressoTuple : ICollection, IEnumerable, IEnumerable<object>, IList<object>,
		IStructuralEquatable, IStructuralComparable
	{
		private readonly object[] _content;
		
		/// <summary>
		/// Tupleの中身。
		/// The content of the tuple.
		/// </summary>
		/// <value>
		/// The contents.
		/// </value>
		public object[] Content{get{return this._content;}}
		
		public ExpressoTuple(object[] content)
		{
			this._content = content;
		}
		
		public ExpressoTuple(List<object> content)
		{
			this._content = content.ToArray();
		}

		public object this[int index]
		{
			get{
				return _content[index];
			}
		}
		
		#region ICollection members
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}
		
		public int Count
		{
			[ExpressoHidden]
			get{ return _content.Length; }
		}

		[ExpressoHidden]
		public void CopyTo(Array array, int index)
		{
			Array.Copy(_content, 0, array, index, _content.Length);
		}
		
		object ICollection.SyncRoot
		{
			get {
				return this;
			}
		}
		#endregion

		#region IEnumerable members
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _content.GetEnumerator();
		}
		#endregion

		#region IEnumerable<object> members
		public IEnumerator<object> GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region IList<object> Members
		[ExpressoHidden]
		public int IndexOf(object item)
		{
			for(int i = 0; i < Count; i++){
				if(this[i].Equals(item)) return i;
			}
			return -1;
		}
		
		void IList<object>.Insert(int index, object item)
		{
			throw new InvalidOperationException("Tuple is readonly");
		}
		
		void IList<object>.RemoveAt(int index)
		{
			throw new InvalidOperationException("Tuple is readonly");
		}
		
		object IList<object>.this[int index]
		{
			get {
				return this[index];
			}
			set {
				throw new InvalidOperationException("Tuple is readonly");
			}
		}
		#endregion

		#region ICollection<object> Members
		void ICollection<object>.Add(object item)
		{
			throw new InvalidOperationException("Tuple is readonly");
		}
		
		void ICollection<object>.Clear()
		{
			throw new InvalidOperationException("Tuple is readonly");
		}
		
		[ExpressoHidden]
		public bool Contains(object item)
		{
			for(int i = 0; i < _content.Length; ++i){
				if(_content[i].Equals(item)){
					return true;
				}
			}
			
			return false;
		}

		public bool Empty()
		{
			return _content.Length == 0;
		}
		
		[ExpressoHidden]
		public void CopyTo(object[] array, int arrayIndex)
		{
			for(int i = 0; i < Count; ++i){
				array[arrayIndex + i] = this[i];
			}
		}
		
		bool ICollection<object>.IsReadOnly{
			get { return true; }
		}
		
		bool ICollection<object>.Remove(object item)
		{
			throw new InvalidOperationException("Tuple is readonly");
		}
		#endregion

		#region IStructuralComparable Members
		int IStructuralComparable.CompareTo(object obj, IComparer comparer)
		{
			ExpressoTuple other = obj as ExpressoTuple;
			if(other == null){
				throw new ArgumentException("expected tuple");
			}
			
			return ExpressoOps.CompareArrays(_content, _content.Length, other._content, other._content.Length, comparer);
		}
		#endregion

		#region IStructualEquatable members
		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			// Optimization for when comparer is IronPython's default IEqualityComparer
			/*PythonContext.PythonEqualityComparer pythonComparer = comparer as PythonContext.PythonEqualityComparer;
			if (pythonComparer != null) {
				return GetHashCode(pythonComparer.Context.InitialHasher);
			}*/
			
			return GetHashCode(comparer);
		}
		
		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!Object.ReferenceEquals(other, this)) {
				ExpressoTuple l = other as ExpressoTuple;
				if (l == null || _content.Length != l._content.Length) {
					return false;
				}
				
				for (int i = 0; i < _content.Length; i++) {
					object obj1 = _content[i], obj2 = l._content[i];
					
					if(Object.ReferenceEquals(obj1, obj2)){
						continue;
					}else if(!comparer.Equals(obj1, obj2)){
						return false;
					}
				}
			}
			return true;
		}
		#endregion
		
		public override bool Equals(object obj)
		{
			if(!Object.ReferenceEquals(this, obj)){
				ExpressoTuple other = obj as ExpressoTuple;
				if(other == null || _content.Length != other._content.Length){
					return false;
				}
				
				for(int i = 0; i < _content.Length; i++){
					object obj1 = this[i], obj2 = other[i];
					
					if(Object.ReferenceEquals(obj1, obj2)){
						continue;
					}else if(obj1 != null){
						if(!obj1.Equals(obj2)){
							return false;
						}
					}else{
						return false;
					}
				}
			}
			return true;
		}
		
		public override int GetHashCode()
		{
			int hash1 = 6551;
			int hash2 = hash1;
			
			for(int i = 0; i < _content.Length; i += 2){
				hash1 = ((hash1 << 27) + ((hash2 + 1) << 1) + (hash1 >> 5)) ^ _content[i].GetHashCode();
				
				if(i == _content.Length - 1){
					break;
				}
				hash2 = ((hash2 << 5) + ((hash1 - 1) >> 1) + (hash2 >> 27)) ^ _content[i + 1].GetHashCode();
			}
			return hash1 + (hash2 * 1566083941);
		}

		private int GetHashCode(IEqualityComparer comparer)
		{
			int hash1 = 6551;
			int hash2 = hash1;
			
			for(int i = 0; i < _content.Length; i += 2){
				hash1 = ((hash1 << 27) + ((hash2 + 1) << 1) + (hash1 >> 5)) ^ comparer.GetHashCode(_content[i]);
				
				if(i == _content.Length - 1){
					break;
				}
				hash2 = ((hash2 << 5) + ((hash1 - 1) >> 1) + (hash2 >> 27)) ^ comparer.GetHashCode(_content[i + 1]);
			}
			return hash1 + (hash2 * 1566083941);
		}
		
		#region The enumerator for ExpressoTuple
		public struct Enumerator : IEnumerator<object>, IEnumerator
		{
			private IEnumerator er;
			
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}
			
			public object Current
			{
				get
				{
					return er.Current;
				}
			}
			
			internal Enumerator(ExpressoTuple tuple)
			{
				this.er = tuple._content.GetEnumerator();
			}
			
			public void Dispose()
			{
			}
			
			void IEnumerator.Reset()
			{
				er.Reset();
			}
			
			public bool MoveNext()
			{
				return er.MoveNext();
			}
		}
		#endregion
	}
}

