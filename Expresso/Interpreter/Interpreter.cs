using System;
using System.Collections.Generic;
using System.Linq;
using Expresso.Ast;
using Expresso.BuiltIns;
using Expresso.Helpers;

/**
 *----------------------------------------------------
 * Expressoの簡易仕様書
 * 1.基本的には静的型付けであるが、極力プログラマーが型を明示しなくてもいいようになっている。
 * ただし、宣言文で変数の型を明示した場合は例外でこの場合、変数自体がインスタンスとなる。（つまり、代入文を書けばそれはオブジェクトの
 * ディープコピーを意味することになり、関数に渡せば値渡しとなってスタックにそのオブジェクトのコピーが作られることになる）
 * 一方で、宣言時に型名を省略した場合、または明示的に"var"型を宣言した場合には変数は参照となり代入や関数に渡してもオブジェクトは
 * コピーされなくなる。なお、Expressoでは"var(variadic)"型を「参照型」、それ以外の組み込み型も含めた全ての型を「値型」と呼ぶ。
 * 値型を関数に参照渡しするための演算子もある。（いずれ実装予定）
 * (この仕様は改める可能性が高い。現在の実装はこのようにはなっていない)
 * 2.Expressoの組み込み型の一つである"expression"型は式の先頭にバッククォートを付した形をしており、特に述語関数などとして使用する。
 * その実態はごく短小な定義を持つ関数であり、故に遅延評価される。また、expression型は、記号演算もサポートする。
 * なので、Computer algebra systemとしても使用できる。
 * 3.他の大多数のスクリプト言語同様、関数は第一級オブジェクトであり、関数の引数や戻り値として扱うことができる。
 * 4.Python譲りのcomprehension構文とHaskell系の記号によるcomprehension構文を備える。
 * 5.組み込み型としてFraction型や任意精度演算もサポートする。
 * 6.組み込み型の一種であるIntSeq型は、その名の通り整数の数列を表す型で、カウントアップ式のfor文が存在しないExpressoで
 * C言語におけるfor(int i = 0; i < max; ++i){...}のような処理を実現する他、配列やリストなどPythonにおいてSequenceと呼ばれる
 * オブジェクトに作用してそのシーケンスオブジェクトの一部または全体をコピーする際に用いられる。(PythonにおけるSliceに相当)
 * 7.変数のスコープは、JavaScriptなど同様、関数のみが持つ。
 * 
 * Expresso組み込みの型に関して
 * int           : いわゆる整数型。C#のint型を使用。
 * bool          : C#のboolean型を使用。
 * float         : いわゆる浮動小数点型。６４ビット精度版のみ実装。C#ではdoubleを使用。
 * bigint        : いわゆる多倍長整数型。C#では、BigIntegerクラスを使用。
 * rational      : 分数型。分子と分母を整数型でもつため、実用上有理数を完璧な精度で保持できる。
 * string        : いわゆる文字列型。C#のstring型を使用。C#以外で実装する場合、文字列の比較をオブジェクトの参照の比較で行うように実装すること。
 * bytearray     : Cで言うところのchar[]型。要するにバイトの配列。C#では、char型の配列で実装。
 * var(variadic) : 総称型。実装上は、どんな型の変数でも指し示すことのできるポインターや参照。
 * tuple         : Pythonなどで実装されているタプル型と同じ。長さ不変、書き換え不可な配列とも言える。
 * list          : データ構造でよく話題に上るリスト型と同じ。長さ可変、書き換え可能な配列とも言える。C#では、Listクラスで実装。
 * dictionary    : いわゆる辞書型。言語によっては、連想配列とも呼ばれるもの。C#では、Dictionaryクラスで実装。
 * expression    : 基本的にはワンライナーのクロージャーの糖衣構文。記号演算もサポートする点が通常のクロージャーと異なる。
 * function      : 普通の関数型。構文は違えど、クロージャーも実装上はこの型になる。
 * intseq        : PythonのxrangeオブジェクトやRubyのRangeオブジェクトと似たようなもの。整数の数列を作り出すジェネレーターと思えばいい。
 * 
 * その他、実装上の特徴
 * 現在はソースコードを解析してASTを生成し、それを直接実行するインタプリタ方式の実装になっているが、将来的にはせめてILぐらいのコードを生成するようにしたい。
 * インタプリタ方式ではあっても、初期の頃の実装とはだいぶ様変わりしており、calc_prime_nums.exsがほぼ同等のPythonコードの４倍程度の速度で動くようになった。
 * 最初の実装では、約１６倍かかっていたことを思えば、大変な進歩である。ここに至るまでに３回の大規模な仕様変更を行なっている。
 * 一度目は、組み込み型すらC#上でクラスを作成していたためにコピーが発生するたびに新たなインスタンスを生成していたのを、基本的な型に関してはC#の組み込み型を
 * 使用するようにしたことである。
 * 二度目の変更は、暫定的にwhile文によるCのfor文のようなカウントアップ式のイテレーションを行なっていたところをExpresso本来の姿である整数列型とfor文を
 * 使用した形に書き換えたことである。
 * そして、三度目の変更は、シンボルテーブルだけでなく、実行時の変数参照もC#のDictionary型を使用していたところを構文解析時に識別子をメモリーオフセットに
 * 変換しておいて、実際の変数参照はC#のList型を使用したものに変更したことである。
 * おそらくこれ以上の高速化は、ASTを直接実行、おまけにオブジェクト指向を使用した現在の形では不可能と思われる。
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
		public Block Root{get; set;}

		public Function MainFunc{get; set;}
		
		/// <summary>
		/// グローバルな変数ストア。main関数を含む子スコープは、この変数ストアを親として持つ。
		/// The global variable store.
		/// </summary>
		private VariableStore var_store = new VariableStore();
		
		/// <summary>
		/// main関数をエントリーポイントとしてプログラムを実行する。
		/// Run the program with the "main" function as the entry point.
		/// </summary>
		/// <exception cref='EvalException'>
		/// Is thrown when the eval exception.
		/// </exception>
		/// <returns>The return value of the main function</returns>
		public object Run()
		{
			if(MainFunc == null)
				throw new EvalException("No entry point");
			
			var call = new Call{
				Function = MainFunc,
				Arguments = new List<Expression>()
			};
			
			return call.Run(var_store);
		}
		
		/// <summary>
		/// グローバルに存在する変数宣言文を実行して
		/// グローバルの環境を初期化する。
		/// Executes all the definitions of variables in global
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

			foreach(var global_var in topmost.LocalVariables)	//グローバル変数を予め変数ストアに追加しておく
				var_store.Add(global_var.Offset, ImplementationHelpers.GetDefaultValueFor(global_var.ParamType));

			foreach(var var_decl in topmost.Statements.OfType<VarDeclaration>().ToArray())
				var_decl.Run(var_store);
		}

		public VariableStore GetGlobalVarStore()
		{
			return var_store;
		}
	}
}

