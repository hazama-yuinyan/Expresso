using System;
using System.Diagnostics;

using Expresso.Runtime.Operations;

namespace Expresso.Runtime
{
	/// <summary>
	/// Expressoのオブジェクトのデバッガー用プロクシー。
	/// Expresso object debug view.
	/// </summary>
	[DebuggerDisplay("{Value}", Name = "{Name}", Type = "{GetClassName()}")]
	internal class ExpressoObjectDebugView
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly string name;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly object value;
		
		public ExpressoObjectDebugView(object objName, object val)
		{
			name = objName.ToString();
			value = val;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public string Name{
			get{
				return name;
			}
		}
		
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object Value{
			get {
				return value;
			}
		}
		
		public string GetClassName()
		{
			var type_annot = ExpressoOps.GetTypeAnnotInExpresso(value.GetType());
			return type_annot.ObjType.ToString();
		}
	}
}

