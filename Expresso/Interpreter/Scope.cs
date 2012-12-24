using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Ast;
using Expresso.Builtins;

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
			Type,			//Type means the class, interface, struct and module type
			Any
		}

		internal NodeType Type;

		internal Node Node { set; get; }
	}

	/// <summary>
	/// スコープ内の識別子管理用のクラス。
	/// Class for managing identifiers in a scope.
	/// Generally it can be considered as a symbol table.
	/// </summary>
	internal class AnalysisScope
	{
		/// <summary>
		/// 親スコープ。
		/// The parent scope.
		/// </summary>
		public AnalysisScope Parent { get; set; }

		/// <summary>
		/// 識別子名 → 識別子の詳細テーブル。
		/// The symbol table.
		/// Namespaces are created for variables, functions and types.
		/// </summary>
		private Dictionary<string, List<ScopeItem>> table = new Dictionary<string, List<ScopeItem>>();

		static private AnalysisScope symbol_table = new AnalysisScope();
		static public AnalysisScope SymbolTable{get{return AnalysisScope.symbol_table;}}

		private int next_offset = 0;

		static AnalysisScope()
		{
			Identifier[] builtin_types = {
				new Identifier("File", new TypeAnnotation(ObjectTypes.TYPE_CLASS, "File")),
				new Identifier("Exception", new TypeAnnotation(ObjectTypes.TYPE_CLASS, "Exception")),
				new Identifier("math", new TypeAnnotation(ObjectTypes.TYPE_MODULE, "math"))
			};
			
			foreach(var builtin_type in builtin_types)
				symbol_table.AddType(builtin_type);
		}

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
		public bool ContainsIn(string name, ScopeItem.NodeType searchType = ScopeItem.NodeType.Any)
		{
			if(searchType == ScopeItem.NodeType.Any)
				return this.table.ContainsKey(name);
			else{
				List<ScopeItem> items;
				if(table.TryGetValue(name, out items)){
					return items.Any((item) => item.Type == searchType);
				}
				return false;
			}
		}

		/// <summary>
		/// 識別子が親も含めたスコープ内に存在するかどうか調べる。
		/// Determines whether the scope, including the parents, has the specified identifier.
		/// </summary>
		/// <returns>
		/// <c>true</c>, if the identifier is found, <c>false</c> otherwise.
		/// </returns>
		public bool ContainsOf(string name, ScopeItem.NodeType searchType = ScopeItem.NodeType.Any)
		{
			for(var s = this; s != null; s = s.Parent){
				if(s.ContainsIn(name, searchType))
					return true;
			}

			return false;
		}

		/// <summary>
		/// 型を取得。
		/// Gets a type name.
		/// </summary>
		/// <param name="name">
		/// 識別子名。
		/// The name of the identifier.
		/// </param>
		/// <returns>
		/// その名前の型が定義されていれば型の詳細を、なければnullを。
		/// If exists, returns the information on that type, otherwise returns null.
		/// </returns>
		public Identifier GetType(string name)
		{
			int level = 0;
			for(var s = this; s != null; s = s.Parent, ++level){
				var v = GetSymbol(name, ScopeItem.NodeType.Type, s);
				if (v != null && level == 0){
					return v;
				}else if(v != null){
					Identifier cloned = new Identifier(v.Name, v.ParamType, v.AliasName, v.Offset, level);
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
				for(var s = this; s != null; s = s.Parent, ++level){
					var v = GetSymbol(name, ScopeItem.NodeType.Local, s);
					if (v != null && level == 0){
						return v;
					}else if(v != null){
						Identifier cloned = new Identifier(v.Name, v.ParamType, v.AliasName, v.Offset, level);
						return cloned;
					}
				}

				return null;
			}else{
				return GetSymbol(name, ScopeItem.NodeType.Local, this);
			}
		}
		
		static Identifier GetSymbol(string name, ScopeItem.NodeType type, AnalysisScope scope)
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
				for (var s = this; s != null; s = s.Parent) {
					var f = GetFunction(name, s);
					if (f != null)
						return f;
				}

				return null;
			} else {
				return GetFunction(name, this);
			}
		}

		static Function GetFunction(string name, AnalysisScope scope)
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
		/// スコープに型を追加。
		/// Adds a type to the current scope.
		/// </summary>
		/// <param name='p'>
		/// The type info to be added.
		/// </param>
		public void AddType(Identifier p)
		{
			AddSymbol(p.Name, ScopeItem.NodeType.Type, p);
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

		/// <summary>
		/// スコープにエイリアスを追加する。
		/// Adds an alias identifier.
		/// </summary>
		public void AddAlias(Identifier p)
		{
			AddSymbol(p.AliasName, ScopeItem.NodeType.Type, p);
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
