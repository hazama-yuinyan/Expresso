using System;
using System.Collections.Generic;
using Expresso.Builtins;

namespace Expresso.Interpreter
{
	/// <summary>
	/// あるスコープ内に存在する変数の実体をスタック形式で保持する。
	/// Holds instances of variables existing in a scope.
	/// This is not a symbol table, which holds just the names of variables.
	/// For a symbol table, see the AnalysisScope class definition.
	/// </summary>
	/// <exception cref='EvalException'>
	/// Represents errors that occur during application execution.
	/// </exception>
	/// <see cref="AnalysisScope"/>
	public sealed class VariableStore
	{
		/// <summary>
		/// 親のスコープの変数ストア。
		/// The parent variable store. It can be null if the store is the root.
		/// </summary>
		public VariableStore Parent{get; internal set;}
		
		/// <summary>
		/// 変数の実体を保持するリスト。
		/// The dictionary holding instances of variables.
		/// </summary>
		private List<object> store = new List<object>();

		private VariableStore TrackUpScope(int level)
		{
			var vars = this;
			for(; vars != null && level > 0; vars = vars.Parent, --level) ;

			if(level != 0)
				throw new EvalException("The requested scope doesn't seem to exist.");

			return vars;
		}
		
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
		public void Add(int offset, object obj)
		{
			if(offset >= store.Capacity)
				store.Capacity = offset + 1;

			if(offset < store.Count)
				store.Insert(offset, obj);
			else
				store.Add(obj);
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
		public void Assign(int offset, object obj)
		{
			store[offset] = obj;
		}

		public void Assign(int level, int offset, object obj)
		{
			var store = TrackUpScope(level);
			store.Assign(offset, obj);
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
		public object Get(int offset, int level = 0)
		{
			var store = (level > 0) ? TrackUpScope(level) : this;
			return Get(offset, store);
		}
		
		static object Get(int offset, VariableStore vars)
		{
			if(offset >= vars.store.Count)
				throw new EvalException("Attempt to refer to an inexsistent variable");
			
			return vars.store[offset];
		}
		
		/// <summary>
		/// スコープからある特定の変数を削除する。
		/// Delete a variable.
		/// </summary>
		/// <param name='name'>
		/// Name.
		/// </param>
		public void Remove(int offset)
		{
			store.RemoveAt(offset);
		}

		public void Remove(int offset, int level = 0)
		{
			var store = (level > 0) ? TrackUpScope(level) : this;
			store.Remove(offset);
		}
	}
}

