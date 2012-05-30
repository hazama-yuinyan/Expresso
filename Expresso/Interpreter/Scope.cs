using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
			Argument, //不要っぽい気もする。
			Function,
		}

		internal NodeType Type;

		internal Ast.Node Node { set; get; }
	}

	/// <summary>
	/// スコープ内の識別子管理用のクラス。
	/// </summary>
	/// <remarks>
	/// 今回作った言語の場合、
	/// 変数はローカルスコープのものしか参照不可で、
	/// 関数は上位のスコープを参照できる。
	/// </remarks>
	internal class Scope
	{
		/// <summary>
		/// 親スコープ。
		/// </summary>
		public Scope Parent { get; set; }

		/// <summary>
		/// 識別子名 → 識別子の詳細テーブル。
		/// </summary>
		private Dictionary<string, ScopeItem> table = new Dictionary<string, ScopeItem> ();

		/// <summary>
		/// 識別子がスコープ内に含まれるかどうか。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <returns>含まれていればtrue。</returns>
		public bool Contains(string name)
		{
			return this.table.ContainsKey(name);
		}

		/// <summary>
		/// 変数を取得。
		/// 親スコープもたどって探索（GetVariable(name, true) と同じ）。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <returns>その名前の変数があれば変数の詳細を、なければnullを。</returns>
		public Ast.Parameter GetVariable(string name)
		{
			return GetVariable(name, true);
		}
		
		/// <summary>
		/// 変数を取得。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <param name="searchParent">親スコープを探索するかどうか。</param>
		/// <returns>その名前の変数があれば変数の詳細を、なければnullを。</returns>
		public Ast.Parameter GetVariable(string name, bool searchParent)
		{
			if(searchParent){
				for (Scope s = this; s != null; s = s.Parent) {
					var v = GetVariable(name, s);
					if (v != null)
						return v;
				}

				return null;
			}else{
				return GetVariable(name, this);
			}
		}
		
		static Ast.Parameter GetVariable(string name, Scope scope)
		{
			if (!scope.Contains(name))
				return null;

			var item = scope.table[name];

			if (item.Type != ScopeItem.NodeType.Local && item.Type != ScopeItem.NodeType.Argument)
				return null;

			return item.Node as Ast.Parameter;
		}

		/// <summary>
		/// 関数を取得。
		/// 親スコープもたどって探索（GetFunction(name, true) と同じ）。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <returns>その名前の関数があれば関数の詳細を、なければnullを。</returns>
		public Ast.Function GetFunction(string name)
		{
			return this.GetFunction(name, true);
		}

		/// <summary>
		/// 関数を取得。
		/// </summary>
		/// <param name="name">識別子名。</param>
		/// <param name="searchParent">親スコープを探索するかどうか。</param>
		/// <returns>その名前の関数があれば関数の詳細を、なければnullを。</returns>
		public Ast.Function GetFunction(string name, bool searchParent)
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

		static Ast.Function GetFunction(string name, Scope scope)
		{
			if (!scope.Contains(name))
				return null;

			var item = scope.table[name];

			if (item.Type != ScopeItem.NodeType.Function)
				return null;

			return item.Node as Ast.Function;
		}

		/// <summary>
		/// スコープにローカル変数を追加。
		/// </summary>
		/// <param name="p">変数。</param>
		public void AddLocal(Ast.Parameter p)
		{
			this.AddVariable(p.Name, ScopeItem.NodeType.Local, p);
		}

		/// <summary>
		/// スコープに引数を追加。
		/// </summary>
		/// <param name="p">変数。</param>
		public void AddArgument (Ast.Parameter p)
		{
			// Local と分ける意味あんまりないかもなぁ。

			this.AddVariable(p.Name, ScopeItem.NodeType.Argument, p);
		}

		void AddVariable(string name, ScopeItem.NodeType type, Ast.Parameter p)
		{
			if (this.table.ContainsKey(name))
				throw new ArgumentException ("The variable already defined in that scope!");

			this.table[name] = new ScopeItem { Type = ScopeItem.NodeType.Local, Node = p };
		}

		/// <summary>
		/// スコープに関数を追加。
		/// </summary>
		/// <param name="f">関数。</param>
		public void AddFunction(Ast.Function f)
		{
			if (this.table.ContainsKey(f.Name))
				throw new ArgumentException ("The identifier is already in use!");

			this.table[f.Name] = new ScopeItem { Type = ScopeItem.NodeType.Function, Node = f };
		}
	}
}
