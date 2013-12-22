using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// モジュールインポート文。
	/// Reperesents a require statement.
    /// </summary>
    public class RequireStatement : Statement
    {
		readonly string[] module_names;
		readonly string[] alias_names;

        /// <summary>
        /// 対象となるモジュール名。
		/// The target module names to be required. It can contain a '.'(which can be used to point to a nested module)
        /// or a '/'(which can be used to point to an external source file).
        /// </summary>
        public string[] ModuleNames{
			get{return module_names;}
		}

		/// <summary>
		/// モジュールに対して与えるエイリアス名。
        /// Alias names that can be used to refer to the modules within the scope.
		/// It can be null if none is specified.
		/// </summary>
		public string[] AliasNames{
			get{return alias_names;}
		}

		internal ExpressoVariable[] Variables{get; set;}

        public override NodeType Type{
            get{return NodeType.Require;}
        }

		public RequireStatement(string[] moduleNames, string[] aliasNames = null)
		{
			module_names = moduleNames;
			alias_names = aliasNames;
		}

        public override bool Equals(object obj)
        {
            var x = obj as RequireStatement;

            if(x == null)
                return false;

            return this.ModuleNames == x.ModuleNames && this.AliasNames == x.AliasNames;
        }

        public override int GetHashCode()
        {
            return this.ModuleNames.GetHashCode() ^ this.AliasNames.GetHashCode();
        }

        /*internal override object Run(VariableStore varStore)
        {
			var path = ModuleName.Replace('.', '/');
			path += ".exs";
			path = Interpreter.Interpreter.CurRuntime.GetRelativePathToCurrentSource(path);
			var module_parser = new Parser(new Scanner(path));
			module_parser.ParsingFileName = ModuleName;
			module_parser.Parse();

			module_parser.TopmostAst.Run(varStore);

			return null;
        }*/

		internal override CSharpExpr Compile(Emitter<CSharpExpr> emitter)
		{
			return emitter.Emit(this);
		}

		internal override void Walk(ExpressoWalker walker)
		{
			if(walker.Walk(this)){}
			walker.PostWalk(this);
		}

		public override string ToString()
		{
			var sb = new StringBuilder("require ");
			if(AliasNames != null){
				foreach(var pair in ModuleNames.Zip(AliasNames, (name, alias) => new Tuple<string, string>(name, alias)))
					sb.AppendFormat("{0} as {1},", pair.Item1, pair.Item2);
			}else{
				foreach(var name in ModuleNames)
					sb.Append(name + ",");
			}
			return sb.ToString();
		}
    }
}
