using System;
using System.Collections.Generic;
using Expresso.Ast;

namespace Expresso.BuiltIns
{
	public enum TYPES // types
	{
		UNDEF = 0,
		NULL,
		INTEGER,
		BOOL,
		FLOAT,
		RATIONAL,
		BIGINT,
		STRING,
		BYTEARRAY,
		VAR,
		TUPLE,
		LIST,
		DICT,
		EXPRESSION,
		FUNCTION,
		SEQ,
		ARRAY,
		_SUBSCRIPT,
		_METHOD,
		_CASE_DEFAULT
	};
	
	/// <summary>
	/// Expresso object.
	/// </summary>
	/*public abstract class ExpressoObj
	{
		public virtual TYPES Type{
			get{
				return TYPES.UNDEF;
			}
			internal set{}
		}

		protected List<ExpressoObj> members;

		static protected List<Function> methods;

		/// <summary>
		/// このインスタンスのメンバーにアクセスする。
		/// Accesses one of the members of this instance.
		/// </summary>
		public virtual ExpressoObj AccessMember(ExpressoObj subscription)
		{
			throw new NotImplementedException();
		}
	}*/
}

