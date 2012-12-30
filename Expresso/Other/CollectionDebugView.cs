using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Expresso.Runtime
{
	internal class CollectionDebugProxy {
		private readonly ICollection _collection;
		
		public CollectionDebugProxy(ICollection collection) {
			_collection = collection;
		}
		
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		internal IList Members {
			get {
				List<object> res = new List<object>(_collection.Count);
				foreach (object o in _collection) {
					res.Add(o);
				}
				return res;
			}
		}
	}
	
	internal class ObjectCollectionDebugProxy {
		private readonly ICollection<object> _collection;
		
		public ObjectCollectionDebugProxy(ICollection<object> collection) {
			_collection = collection;
		}
		
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		internal IList<object> Members {
			get {
				return new List<object>(_collection);
			}
		}
	}
}

