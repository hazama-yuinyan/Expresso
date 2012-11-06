using System;
using System.Collections.Generic;
using Expresso.BuiltIns;

namespace Expresso.Interpreter
{
	/// <summary>
	/// あるスコープ内に存在する変数の実体を保持する。
	/// Holds instances of variables existing in a scope.
	/// This is not a symbol table, which holds just the names of variables.
	/// </summary>
	/// <exception cref='EvalException'>
	/// Represents errors that occur during application execution.
	/// </exception>
	public sealed class VariableStore
	{
		/// <summary>
		/// 親のスコープの変数ストア。
		/// The parent variable store.
		/// </summary>
		public VariableStore Parent{get; internal set;}
		
		/// <summary>
		/// 変数の実体を保持する辞書。
		/// The dictionary holding instances of variables.
		/// </summary>
		private Dictionary<string, object> store = new Dictionary<string, object>();
		
		/// <summary>
		/// 変数の実体をスコープに追加する。
		/// Add an instance of a variable.
		/// </summary>
		/// <param name='name'>
		/// 変数名
		/// The name of the variable.
		/// </param>
		/// <param name='obj'>
		/// 変数の中身
		/// The value.
		/// </param>
		public void Add(string name, object obj)
		{
			store.Add(name, obj);
		}

		/// <summary>
		/// スコープに存在する変数の値を変える。
		/// Changes the value of a variable.
		/// </summary>
		/// <param name='name'>
		/// 変数名
		/// </param>
		/// <param name='obj'>
		/// 変更先の値
		/// </param>
		public void Assign(string name, object obj)
		{
			store[name] = obj;
		}
		
		/// <summary>
		/// スコープ内に存在する変数を参照する。
		/// Refer to a variable.
		/// </summary>
		/// <param name='name'>
		/// 参照する変数名
		/// </param>
		/// <exception cref='EvalException'>
		/// スコープ内にその識別子の変数が存在しない場合に発生する。
		/// </exception>
		public object Get(string name)
		{
			return Get(name, true);
		}
		
		public object Get(string name, bool searchParent)
		{
			if(searchParent){
				for(VariableStore vars = this; vars != null; vars = vars.Parent){
					var v = Get(name, vars);
					if(v != null)
						return v;
				}
				
				throw new EvalException("Attempt to refer to an inexsistent variable");
			}else{
				var v = Get(name, this);
				if(v == null)
					throw new EvalException("Attempt to refer to an inexsistent variable");
				
				return v;
			}
		}
		
		static object Get(string name, VariableStore vars)
		{
			if(!vars.store.ContainsKey(name))
				return null;
			
			return vars.store[name];
		}
		
		/// <summary>
		/// スコープからある特定の変数を削除する。
		/// Delete a variable.
		/// </summary>
		/// <param name='name'>
		/// Name.
		/// </param>
		public void Remove(string name)
		{
			store.Remove(name);
		}
	}
}

