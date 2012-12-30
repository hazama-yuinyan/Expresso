using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.IO;
using Expresso.Ast;
using Expresso.Interpreter;

namespace Expresso.Builtins
{
	#region Expressoの組み込みオブジェクト型郡

	/// <summary>
	/// Expressoの組み込み型の一つ、Expression型。基本的にはワンライナーのクロージャーだが、
	/// 記号演算もサポートする。
	/// The built-in expression class. It is, in most cases, just a function object
	/// with one line of code, but may contain symbolic expression. Therefore,
	/// it has the capability of symbolic computation.
	/// </summary>
	/*public class ExpressoExpression
	{
		public TYPES Type{get{return TYPES.EXPRESSION;}}

		private Function body;
	}*/
	#endregion
}
