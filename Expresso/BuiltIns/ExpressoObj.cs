using System;

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
		CHARSEQ,
		VAR,
		TUPLE,
		LIST,
		DICT,
		EXPRESSION,
		FUNCTION,
		SEQ,
		ARRAY,
		SUBSCRIPT
	};
	
	/// <summary>
	/// Expresso object.
	/// </summary>
	public abstract class ExpressoObj
	{
		public virtual TYPES Type{
			get{
				return TYPES.UNDEF;
			}
			internal set{}
		}

		public virtual ExpressoObj AccessMember(ExpressoObj subscription)
		{
			throw new NotImplementedException();
		}
	}
}

