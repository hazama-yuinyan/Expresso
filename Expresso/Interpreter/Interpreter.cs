using System;
using System.Collections.Generic;
using System.Linq;
using Expresso.Ast;
using Expresso.BuiltIns;

/**
 *----------------------------------------------------
 * Expressoの簡易仕様書
 * 1.基本的には動的型付けであり、変数に型はなく値のみが型を持つ。
 * ただし、宣言文で変数の型を明示した場合は例外でこの場合、変数自体がインスタンスとなる。（つまり、代入文を書けばそれはオブジェクトの
 * ディープコピーを意味することになり、関数に渡せば値渡しとなってスタックにそのオブジェクトのコピーが作られることになる）
 * 一方で、宣言時に型名を省略した場合、または明示的に"var"型を宣言した場合には変数は参照となり代入や関数に渡してもオブジェクトは
 * コピーされなくなる。なお、Expressoでは"var(variadic)"型を「参照型」、それ以外の組み込み型も含めた全ての型を「値型」と呼ぶ。
 * 値型を関数に参照渡しするための演算子もある。（いずれ実装予定）
 * 2.Expressoの組み込み型の一つである"expression"型は式の先頭にバッククォートを付した形をしており、特に述語関数などとして使用する。
 * その実態はごく短小な定義を持つ関数であり、故に遅延評価される。また、expression型は、記号演算もサポートする。
 * なので、Computer algebra systemとしても使用できる。
 * 3.他の大多数のスクリプト言語同様、関数は第一級オブジェクトであり、関数の引数や戻り値として扱うことができる。
 * 4.Python譲りのcomprehension構文とHaskell系の記号によるcomprehension構文を備える。
 * 5.組み込み型としてFraction型や任意精度演算もサポートする。
 * 6.組み込み型の一種であるIntSeq型は、その名の通り整数の数列を表す型で、カウントアップ式のfor文が存在しないExpressoで
 * C言語におけるfor(int i = 0; i < max; ++i){...}のような処理を実現する他、配列やリストなどPythonにおいてSequenceと呼ばれる
 * オブジェクトに作用してそのシーケンスオブジェクトの一部または全体をコピーする際に用いられる。(PythonにおけるSliceに相当)
 *----------------------------------------------------
 */

namespace Expresso.Interpreter
{
	/// <summary>
	/// Expressoのインタプリタ.
	/// The Expresso's intepreter.
	/// </summary>
	public class Interpreter
	{
		/// <summary>
		/// 抽象構文木のルート要素を保持する。
		/// The root element for the AST.
		/// </summary>
		public Ast.Block Root{get; internal set;}
		
		/// <summary>
		/// グローバルな環境。主にそのプログラム中で定義されている関数を保持する。
		/// The global environment which holds all the functions defined in the program.
		/// </summary>
		private Scope environ = new Scope();
		
		/// <summary>
		/// グローバルな変数ストア。main関数を含む子スコープは、この変数ストアを親として持つ。
		/// The global variable store.
		/// </summary>
		private VariableStore var_store = new VariableStore();
		
		public Interpreter(Block root)
		{
			Root = root;
		}
		
		/// <summary>
		/// main関数をエントリーポイントとしてプログラムを実行する。
		/// Run the program with the "main" function as the entry point.
		/// </summary>
		/// <exception cref='EvalException'>
		/// Is thrown when the eval exception.
		/// </exception>
		public void Run()
		{
			Ast.Function main_fn = environ.GetFunction("main");
			if(main_fn == null)
				throw new EvalException("No entry point");
			
			var call = new Call{
				Function = main_fn,
				Arguments = new List<Ast.Expression>()
			};
			
			call.Run(var_store, environ);
		}
		
		/// <summary>
		/// グローバルに存在する変数宣言や関数定義文を実行して
		/// グローバルの環境を初期化する。
		/// Executes all the definitions of variables and functions in global
		/// in order to initialize the global environment.
		/// </summary>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		public void Initialize()
		{
			Block topmost = Root as Block;
			if(topmost == null)
				throw new Exception("Topmost block not found!");
			
			topmost.Run(var_store, environ);
		}
	}
}

