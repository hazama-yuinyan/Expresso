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
			Class,
			Module
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
		private Dictionary<string, List<ScopeItem>> table = new Dictionary<string, List<ScopeItem>>();

		static private Scope symbol_table = new Scope();
		static public Scope SymbolTable{get{return Scope.symbol_table;}}

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
		public bool ContainsIn(string name)
		{
			return this.table.ContainsKey(name);
		}

		/// <summary>
		/// 識別子が親も含めたスコープ内に存在するかどうか調べる。
		/// Determines whether the scope, including the parents, has the specified identifier.
		/// </summary>
		/// <returns>
		/// <c>true</c>, if the identifier is found, <c>false</c> otherwise.
		/// </returns>
		public bool ContainsOf(string name)
		{
			for(Scope s = this; s != null; s = s.Parent){
				if(s.table.ContainsKey(name))
					return true;
			}

			return false;
		}

		/// <summary>
		/// クラスを取得。
		/// Gets a class name.
		/// </summary>
		/// <param name="name">
		/// 識別子名。
		/// The name of the identifier.
		/// </param>
		/// <returns>
		/// その名前のクラスが定義されていればクラスの詳細を、なければnullを。
		/// If exists, returns the information on that class, otherwise returns null.
		/// </returns>
		public Identifier GetClass(string name)
		{
			int level = 0;
			for(Scope s = this; s != null; s = s.Parent, ++level){
				var v = GetSymbol(name, ScopeItem.NodeType.Class, s);
				if (v != null && level == 0){
					return v;
				}else if(v != null){
					Identifier cloned = new Identifier(v.Name, v.ParamType, v.Offset, level);
					return cloned;
				}
			}

			return null;
		}

		/// <summary>
		/// モジュールを取得。
		/// Gets a module name.
		/// </summary>
		/// <param name="name">
		/// 識別子名。
		/// The name of the identifier.
		/// </param>
		/// <returns>
		/// その名前のモジュールが定義されていればモジュールの詳細を、なければnullを。
		/// If exists, returns the information on that module, otherwise returns null.
		/// </returns>
		public Identifier GetModule(string name)
		{
			int level = 0;
			for(Scope s = this; s != null; s = s.Parent, ++level){
				var v = GetSymbol(name, ScopeItem.NodeType.Module, s);
				if (v != null && level == 0){
					return v;
				}else if(v != null){
					Identifier cloned = new Identifier(v.Name, v.ParamType, v.Offset, level);
					return cloned;
				}
			}
			
			return null;
		}

		/// <summary>
		/// 変数を取得。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <param name="searchParent">親スコープを探索するかどうか。</param>
		/// <returns>その名前の変数があれば変数の詳細を、なければnullを。</returns>
		public Identifier GetVariable(string name, bool searchParent = true)
		{
			if(searchParent){
				int level = 0;
				for(Scope s = this; s != null; s = s.Parent, ++level){
					var v = GetSymbol(name, ScopeItem.NodeType.Local, s);
					if (v != null && level == 0){
						return v;
					}else if(v != null){
						Identifier cloned = new Identifier(v.Name, v.ParamType, v.Offset, level);
						return cloned;
					}
				}

				return null;
			}else{
				return GetSymbol(name, ScopeItem.NodeType.Local, this);
			}
		}
		
		static Identifier GetSymbol(string name, ScopeItem.NodeType type, Scope scope)
		{
			if(!scope.ContainsIn(name))
				return null;

			var list = scope.table[name];
			var item = 
				list
				.Where(x => x.Type == type)
				.ElementAtOrDefault(0);

			if(item == null) return null;
			return item.Node as Identifier;
		}

		/// <summary>
		/// 関数を取得。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <param name="searchParent">親スコープを探索するかどうか。</param>
		/// <returns>その名前の関数があれば関数の詳細を、なければnullを。</returns>
		public Function GetFunction(string name, bool searchParent = true)
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
			if (!scope.ContainsIn(name))
				return null;

			var list = scope.table[name];
			var item = 
				list
				.Where(x => x.Type == ScopeItem.NodeType.Function)
				.ElementAtOrDefault(0);

			if(item == null) return null;
			return item.Node as Function;
		}

		/// <summary>
		/// スコープにローカル変数を追加。
		/// Add a local variable to the scope.
		/// </summary>
		/// <param name="p">変数。</param>
		public void AddLocal(ref Identifier p)
		{
			this.AddSymbol(p.Name, ScopeItem.NodeType.Local, p);
			if(p.Offset == -1)
				p.Offset = this.next_offset++;
		}

		/// <summary>
		/// スコープにクラスを追加。
		/// Adds a class to the current scope.
		/// </summary>
		/// <param name='p'>
		/// The class info to be added.
		/// </param>
		public void AddClass(Identifier p)
		{
			AddSymbol(p.Name, ScopeItem.NodeType.Class, p);
		}

		/// <summary>
		/// スコープにモジュールを追加。
		/// Adds a module to the current scope.
		/// </summary>
		/// <param name='p'>
		/// The module info to be added.
		/// </param>
		public void AddModule(Identifier p)
		{
			AddSymbol(p.Name, ScopeItem.NodeType.Module, p);
		}

		/// <summary>
		/// スコープに関数を追加。
		/// Add a function to the scope.
		/// </summary>
		/// <param name="f">関数。</param>
		public void AddFunction(Function f)
		{
			AddSymbol(f.Name, ScopeItem.NodeType.Function, f);
		}

		void AddSymbol(string name, ScopeItem.NodeType type, Node p)
		{
			var new_item = new ScopeItem{Type = type, Node = p};
			
			if(this.table.ContainsKey(name))
				table[name].Add(new_item);
			else
			table[name] = new List<ScopeItem>{new_item};
		}
	}
}
