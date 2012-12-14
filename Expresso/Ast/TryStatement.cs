using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Helpers;
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
		/// <summary>
        /// 例外の捕捉を行うブロック。
        /// </summary>
        public Block Body { get; internal set; }

        /// <summary>
        /// Catch節。
		/// Represents the catch clause of this try statement.
		/// It can be null if none is specified.
        /// </summary>
        public List<CatchClause> Catches { get; internal set; }

		/// <summary>
		/// Finally節。
		/// Represents the finally clause of this try statement.
		/// It can be null if none is specified.
		/// </summary>
		public FinallyClause FinallyClause {get; internal set;}

        public override NodeType Type
        {
            get { return NodeType.TryStatement; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as TryStatement;

            if (x == null) return false;

            return this.Body == x.Body
                && this.Catches.Equals(x.Catches) && this.FinallyClause == x.FinallyClause;
        }

        public override int GetHashCode()
        {
            return this.Body.GetHashCode() ^ this.Catches.GetHashCode() ^ this.FinallyClause.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			try{
				Body.Run(varStore);
			}
			catch(ExpressoThrowException e){
				foreach(var clause in Catches){
					if(clause.Catcher.ParamType.TypeName == e.Thrown.ClassName){
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
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
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
		/// <summary>
        /// catchの対象となる例外の型とその識別子名。
        /// </summary>
		public Identifier Catcher { get; internal set; }

        /// <summary>
        /// 実行対象のブロック。
		/// The body block.
        /// </summary>
        public Block Body { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.CatchClause; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as CatchClause;

            if (x == null) return false;

            return this.Catcher.Equals(x.Catcher)
                && this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Catcher.GetHashCode() ^ this.Body.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			return Body.Run(varStore);
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}
	}

	/// <summary>
	/// Represents a finally clause in a try statement.
	/// </summary>
	public class FinallyClause : Statement
	{
        /// <summary>
        /// 実行対象のブロック。
		/// The body block.
        /// </summary>
        public Block Body { get; internal set; }

        public override NodeType Type
        {
            get { return NodeType.FinallyClause; }
        }

        public override bool Equals(object obj)
        {
            var x = obj as FinallyClause;

            if (x == null) return false;

            return this.Body.Equals(x.Body);
        }

        public override int GetHashCode()
        {
            return this.Body.GetHashCode();
        }

        internal override object Run(VariableStore varStore)
        {
			return Body.Run(varStore);
        }

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}
	}
}

