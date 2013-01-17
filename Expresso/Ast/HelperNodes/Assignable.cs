using System;

using Expresso.Interpreter;

namespace Expresso.Ast
{
	/// <summary>
	/// 左辺式になれるノード。
	/// Represents an assignable node.
	/// </summary>
	public abstract class Assignable : Expression
	{
		internal abstract void Assign(EvaluationFrame frame, object val);
	}
}

