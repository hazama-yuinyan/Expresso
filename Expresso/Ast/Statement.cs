using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expresso.Interpreter;

namespace Expresso.Ast
{
    /// <summary>
    /// 文の共通基底。
    /// </summary>
    public abstract class Statement : Node
    {
    }

	public interface CompoundStatement
	{
		IEnumerable<Identifier> CollectLocalVars();
	}
}
