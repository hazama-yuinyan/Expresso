using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// Try文。
	/// The Try statement.
	/// </summary>
	public class TryStatement : Statement, CompoundStatement
	{
		readonly Block body;
		readonly CatchClause[] handlers;
		readonly FinallyClause finally_clause;

		/// <summary>
        /// 例外の捕捉を行うブロック。
		/// The block in which we'll catch exceptions.
        /// </summary>
        public Block Body {
			get{return body;}
		}

        /// <summary>
        /// Catch節。
		/// Represents the catch clause of this try statement.
		/// It can be null if none is specified.
        /// </summary>
        public CatchClause[] Catches{
			get{return handlers;}
		}

		/// <summary>
		/// Finally節。
		/// Represents the finally clause of this try statement.
		/// It can be null if none is specified.
		/// </summary>
		public FinallyClause FinallyClause{
			get{return finally_clause;}
		}

        public override NodeType Type
        {
            get { return NodeType.TryStatement; }
        }

		public TryStatement(Block bodyBlock, CatchClause[] catches, FinallyClause finallyClause)
		{
			body = bodyBlock;
			handlers = catches;
			finally_clause = finallyClause;
		}

        public override bool Equals(object obj)
        {
            var x = obj as TryStatement;

            if (x == null) return false;

            return body == x.body
                && handlers.Equals(x.handlers) && finally_clause == x.finally_clause;
        }

        public override int GetHashCode()
        {
            return body.GetHashCode() ^ handlers.GetHashCode() ^ finally_clause.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			try{
				Body.Run(varStore);
			}
			catch(ExpressoThrowException e){
				foreach(var clause in Catches){
					if(clause.Catcher.ParamType.TypeName == e.Thrown.Name){
						clause.Run(varStore);
						return null;
					}
				}
				throw e;
			}
			finally{
				if(FinallyClause != null)
					FinallyClause.Run(varStore);
			}

			return null;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				body.Walk(walker);
				if(handlers != null){
					foreach(var handler in handlers)
						handler.Walk(walker);
				}
				if(finally_clause != null)
					finally_clause.Walk(walker);
			}
			walker.PostWalk(this);
		}

		public IEnumerable<Identifier> CollectLocalVars()
		{
			var in_body = ImplementationHelpers.CollectLocalVars(Body);
			var in_catch = Enumerable.Empty<Identifier>();
			if(Catches != null){
				foreach(var clause in Catches){
					var in_clause = ImplementationHelpers.CollectLocalVars(clause);
					in_catch = in_catch.Concat(in_clause);
				}
			}
			var in_finally = ImplementationHelpers.CollectLocalVars(FinallyClause);

			return in_body.Concat(in_catch).Concat(in_finally);
		}
	}

	/// <summary>
	/// Represents a catch clause in a try statement.
	/// </summary>
	public class CatchClause : Statement
	{
		readonly Identifier handler;
		readonly Block body;

		/// <summary>
        /// catchの対象となる例外の型とその識別子名。
		/// The target type and the name which this node will catch.
        /// </summary>
		public Identifier Catcher{
			get{return handler;}
		}

        /// <summary>
        /// 実行対象のブロック。
		/// The body block.
        /// </summary>
        public Block Body{
			get{return body;}
		}

        public override NodeType Type
        {
            get { return NodeType.CatchClause; }
        }

		public CatchClause(Identifier catcher, Block bodyBlock)
		{
			handler = catcher;
			body = bodyBlock;
		}

        public override bool Equals(object obj)
        {
            var x = obj as CatchClause;

            if (x == null) return false;

            return handler.Equals(x.handler) && body.Equals(x.body);
        }

        public override int GetHashCode()
        {
            return handler.GetHashCode() ^ body.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			return Body.Run(varStore);
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){
				handler.Walk(walker);
				body.Walk(walker);
			}
			walker.PostWalk(this);
		}
	}

	/// <summary>
	/// Represents a finally clause in a try statement.
	/// </summary>
	public class FinallyClause : Statement
	{
		readonly Block body;

        /// <summary>
        /// 実行対象のブロック。
		/// The body block.
        /// </summary>
        public Block Body{
			get{return body;}
		}

        public override NodeType Type
        {
            get { return NodeType.FinallyClause; }
        }

		public FinallyClause(Block bodyBlock)
		{
			body = bodyBlock;
		}

        public override bool Equals(object obj)
        {
            var x = obj as FinallyClause;

            if (x == null) return false;

            return body.Equals(x.body);
        }

        public override int GetHashCode()
        {
            return this.body.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			return Body.Run(varStore);
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this))
				body.Walk(walker);

			walker.PostWalk(this);
		}
	}
}

