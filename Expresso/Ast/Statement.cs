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
	
	public class StatementList : Statement
	{
		public List<Statement> Statements{get; internal set;}
		
		public override NodeType Type
        {
            get { return NodeType.StatementList; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as StatementList;

            if (x == null) return false;
			
			return Statements.Equals(x.Statements);
        }

        public override int GetHashCode()
        {
            return this.Statements.GetHashCode();
        }

        internal override object Run(VariableStore varStore, Scope funcTable)
        {
			foreach (var stmt in Statements) {
				stmt.Run(varStore, funcTable);
			}
			return null;
        }
	}
}
