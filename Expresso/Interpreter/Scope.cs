using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expresso.Ast;

namespace Expresso.Interpreter
{
	/// <summary>
	/// スコープ内の識別子。
	/// </summary>
	internal class ScopeItem
	{
		internal enum NodeType
		{
			Local,
			Function,
			Class
		}

		internal NodeType Type;

		internal Node Node { set; get; }
	}

	/// <summary>
	/// スコープ内の識別子管理用のクラス。
	/// Class for managing identifiers in a scope.
	/// Generally it can be considered as a symbol table.
	/// </summary>
	internal class Scope
	{
		/// <summary>
		/// 親スコープ。
		/// The parent scope.
		/// </summary>
		public Scope Parent { get; set; }

		/// <summary>
		/// 識別子名 → 識別子の詳細テーブル。
		/// The symbol table.
		/// </summary>
		private Dictionary<string, ScopeItem> table = new Dictionary<string, ScopeItem> ();

		private int next_offset = 0;

		/// <summary>
		/// 識別子がスコープ内に含まれるかどうか。
		/// Determines whether the scope has the specified identifier.
		/// </summary>
		/// <param name="name">
		/// 識別子名。
		/// The name of the identifier.
		/// </param>
		/// <returns>
		/// 含まれていればtrue。
		/// Returns true if the scope has the identifier; otherwise returns false.
		/// </returns>
		public bool Contains(string name)
		{
			return this.table.ContainsKey(name);
		}

		/// <summary>
		/// 変数を取得。
		/// 親スコープもたどって探索（GetVariable(name, true) と同じ）。
		/// Gets a variable.
		/// </summary>
		/// <param name="name">
		/// 識別子名。
		/// The name of the identifier.
		/// </param>
		/// <returns>
		/// その名前の変数があれば変数の詳細を、なければnullを。
		/// If exists, returns the information on that variable, otherwise returns null.
		/// </returns>
		public Identifier GetVariable(string name)
		{
			return GetVariable(name, true);
		}
		
		/// <summary>
		/// 変数を取得。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <param name="searchParent">親スコープを探索するかどうか。</param>
		/// <returns>その名前の変数があれば変数の詳細を、なければnullを。</returns>
		public Identifier GetVariable(string name, bool searchParent)
		{
			if(searchParent){
				int level = 0;
				for (Scope s = this; s != null; s = s.Parent, ++level) {
					var v = GetVariable(name, s);
					if (v != null && level == 0){
						return v;
					}else if(v != null){
						Identifier cloned = new Identifier(v.Name, v.ParamType, v.Offset, level);
						return cloned;
					}
				}

				return null;
			}else{
				return GetVariable(name, this);
			}
		}
		
		static Identifier GetVariable(string name, Scope scope)
		{
			if (!scope.Contains(name))
				return null;

			var item = scope.table[name];

			if (item.Type != ScopeItem.NodeType.Local)
				return null;

			return item.Node as Identifier;
		}

		/// <summary>
		/// 関数を取得。
		/// 親スコープもたどって探索（GetFunction(name, true) と同じ）。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <returns>その名前の関数があれば関数の詳細を、なければnullを。</returns>
		public Function GetFunction(string name)
		{
			return this.GetFunction(name, true);
		}

		/// <summary>
		/// 関数を取得。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <param name="searchParent">親スコープを探索するかどうか。</param>
		/// <returns>その名前の関数があれば関数の詳細を、なければnullを。</returns>
		public Function GetFunction(string name, bool searchParent)
		{
			if (searchParent) {
				for (Scope s = this; s != null; s = s.Parent) {
					var f = GetFunction(name, s);
					if (f != null)
						return f;
				}

				return null;
			} else {
				return GetFunction(name, this);
			}
		}

		static Function GetFunction(string name, Scope scope)
		{
			if (!scope.Contains(name))
				return null;

			var item = scope.table[name];

			if (item.Type != ScopeItem.NodeType.Function)
				return null;

			return item.Node as Function;
		}

		/// <summary>
		/// スコープにローカル変数を追加。
		/// Add a local variable to the scope.
		/// </summary>
		/// <param name="p">変数。</param>
		public void AddLocal(ref Identifier p)
		{
			this.AddVariable(p.Name, ScopeItem.NodeType.Local, ref p);
		}

		void AddVariable(string name, ScopeItem.NodeType type, ref Identifier p)
		{
			if (this.table.ContainsKey(name))
				throw new ArgumentException ("The variable already defined in that scope!");

			if(p.Offset == -1)
				p.Offset = this.next_offset++;

			this.table[name] = new ScopeItem { Type = ScopeItem.NodeType.Local, Node = p };
		}

		/// <summary>
		/// スコープに関数を追加。
		/// Add a function to the scope.
		/// </summary>
		/// <param name="f">関数。</param>
		public void AddFunction(Function f)
		{
			if (this.table.ContainsKey(f.Name))
				throw new ArgumentException ("The identifier is already in use!");

			this.table[f.Name] = new ScopeItem { Type = ScopeItem.NodeType.Function, Node = f };
		}
	}
}
