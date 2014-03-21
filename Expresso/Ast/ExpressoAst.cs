using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Runtime.Operations;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

	/// <summary>
	/// ExpressoのASTのトップレベルノード。ファイルから生成された場合はモジュールを表すが、evalによって作られた場合もある。
	/// Top-level ast for all Expresso code. Typically represents a module but could also
	/// be exec or eval code.
	/// </summary>
	public class ExpressoAst : ScopeStatement
	{
		readonly string name;
		readonly bool is_module;

		/// <summary>
		/// モジュール名。Expressoには明示的なモジュール宣言がないので、たいていはファイル名になる。
		/// The name of the module. Since Expresso doesn't have an explicit module declaration, it would be very likely to be
		/// the file name.
		/// If the module contains the main function, it would be called the "main" module.
		/// </summary>
		public string Name{
            get{return is_module ? string.Format("<module {0}>", name) : ModuleName;}
		}

		/// <summary>
		/// Indicates whether the node represents a module.
		/// </summary>
		public bool IsModule{
			get{return is_module;}
		}

		/// <summary>
		/// 本体。このノードがモジュールだった場合にはrequire文とモジュールの定義文が含まれる。
		/// The body of this node. If this node represents a module, then the body includes the require statement, if any,
		/// and the definitions of the module.
		/// </summary>
        public IEnumerable<Statement> Body{
            get{return Children;}
		}

		/// <summary>
		/// Bodyのn番目の定義をエクスポートするかどうかをあらわす。
		/// A map indicating which declarations would be exported.
		/// </summary>
		public BitArray ExportMap{get; internal set;}

		public string ModuleName{
			get{
				return is_module ? name : "<not a module>";
			}
		}

		public override NodeType Type{
            get{return NodeType.Toplevel;}
        }

		public ExpressoAst(Statement[] bodyStmts, string maybeModuleName = null, bool[] exportMap = null)
		{
			name = maybeModuleName;
			is_module = maybeModuleName != null;
			ExportMap = new BitArray(exportMap);

            foreach(var stmt in bodyStmts)
                AddChild(stmt);
		}

		/// <summary>
		/// Binds an AST and makes it capable of being reduced and compiled. Before calling Bind, an AST cannot successfully
		/// be reduced.
		/// </summary>
		public void Bind()
		{
			ExpressoNameBinder.BindAst(this, null);
		}

		#region Name binding support
		internal override bool ExposesLocalVariable(ExpressoVariable variable)
		{
			return true;
		}

		internal override ExpressoVariable BindReference(ExpressoNameBinder binder, ExpressoReference reference)
		{
			return EnsureVariable(reference.Name, reference.VariableType);
		}
		#endregion

        public override bool Equals(object obj)
        {
            var x = obj as ExpressoAst;

            if(x == null)
                return false;

            return this.name == x.name && Body.SequenceEqual(x.Body);
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode() ^ this.Body.GetHashCode();
        }

        public override void AcceptWalker(AstWalker walker)
		{
			if(walker.Walk(this)){
                if(Body != null){
                    foreach(var stmt in Body)
                        stmt.AcceptWalker(walker);
				}
			}
			walker.PostWalk(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.Walk(this);
        }

        public override string GetText()
		{
            return string.Format("<Module decl: {0}>", is_module ? "<anonymous>" : name);
		}

        public override string ToString()
        {
            return GetText();
        }

        public ExpressoUnresolvedFile ToTypeSystem()
        {
            if(string.IsNullOrEmpty(name))
                throw new InvalidOperationException("Can not use toTypeSystem() on a syntax tree without file name.");

            var type_def = new DefaultUnresolvedTypeDefinition("global");
            var walker = new TypeSystemConvertWalker(new ExpressoUnresolvedFile(name, type_def, null));
            walker.Walk(this);
            return walker.UnresolvedFile;
        }
	}
}

