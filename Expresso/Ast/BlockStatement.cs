using System.Collections.Generic;
using System.Linq;
using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// 複文ブロック。
	/// Represents a block of statements.
    /// </summary>
	/// <seealso cref="BreakableStatement"/>
    public class Block : BreakableStatement, CompoundStatement
    {
        List<Statement> statements = new List<Statement>();

        /// <summary>
        /// ブロックの中身の文。
		/// The body statements
        /// </summary>
        public List<Statement> Statements{
			get{return this.statements; }
		}

        /// <summary>
        /// ブロック中で定義された変数一覧。
		/// The local variables defined in the block.
        /// </summary>
        public IEnumerable<Identifier> LocalVariables{
            get{
                return CollectLocalVars();
            }
        }

        public override NodeType Type{
            get{return NodeType.Block;}
        }

        public override bool Equals(object obj)
        {
            var x = obj as Block;

            if(x == null)
                return false;

            if(this.Statements.Count != x.Statements.Count)
                return false;

            for(int i = 0; i < this.Statements.Count; i++){
                if(!this.Statements[i].Equals(x.Statements[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return this.Statements.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			object result = null;
			can_continue = true;
			
			for(int i = 0; can_continue && i < statements.Count; ++i)
				result = statements[i].Run(varStore);
			
			return result;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				foreach(var stmt in statements)
					stmt.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			var vars = 
				from p in Statements
                    where p.Type == NodeType.ExprStatement || p.Type == NodeType.ForStatement || p.Type == NodeType.IfStatement ||
						p.Type == NodeType.SwitchStatement || p.Type == NodeType.WhileStatement || p.Type == NodeType.TryStatement
					select ImplementationHelpers.CollectLocalVars(p) into t
					from q in t
					select q;
			
			return vars;
		}
		
		public override string ToString()
		{
			return string.Format("[Statements.length={0}, ({1})]", Statements.Count, LocalVariables);
		}
    }
}
