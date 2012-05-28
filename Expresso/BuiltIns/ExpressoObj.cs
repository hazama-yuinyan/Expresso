using System;

namespace Expresso.BuiltIns
{
	public enum TYPES // types
	{
		UNDEF = 0,
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
		FUNCTION
	};
	
	/// <summary>
	/// Expresso object.
	/// </summary>
	public abstract class ExpressoObj
	{
		public TYPES Type{get; internal set;}
		
		public ExpressoObj ()
		{
		}
	}
}

