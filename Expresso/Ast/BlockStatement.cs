using System.Collections.Generic;
using System.Linq;
using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;

namespace Expresso.Ast
{
    /// <summary>
    /// 複文ブロック。
	/// Represents a block of statements.
    /// </summary>
	/// <seealso cref="BreakableStatement"/>
    public class Block : BreakableStatement, CompoundStatement
    {
        /// <summary>
        /// ブロックの中身の文。
		/// The body statements
        /// </summary>
        public IEnumerable<Statement> Statements{
            get{
                foreach(var child in Children)
                    yield return child;
            }
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

            if(this.Statements.Count() != x.Statements.Count())
                return false;

            return Statements.SequenceEqual(x.Statements);
        }

        public override int GetHashCode()
        {
            return this.Statements.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){
                foreach(var stmt in Statements)
                    stmt.AcceptWalker(walker);
			}
			walker.PostWalk(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
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
		
        public override string GetText()
		{
            return string.Format("<Block: length={0}, ({1})>", Statements.Count, LocalVariables);
		}

        public override string ToString()
        {
            return GetText();
        }
    }
}
