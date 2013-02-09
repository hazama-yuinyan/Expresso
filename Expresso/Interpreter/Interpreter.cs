using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

using Expresso.Ast;
using Expresso.Builtins;
using Expresso.Compiler.Meta;
using Expresso.Runtime;
using Expresso.Runtime.Exceptions;
using Expresso.Runtime.Operations;

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
 * 8.Pythonライクなモジュール機構を持つ。つまり、ソースファイル一つが１モジュールを定義し、モジュール内に別のモジュールが含まれることはない。
 * 9.Expressoにおいて、関数はモジュールに属し、メソッドはクラスに属する。これらのサブルーチンはそれぞれ、thisの値として自分が所属するモジュールかクラス
 * のインスタンスを暗黙の第一引数として取る。つまり、関数内でthisは所属するモジュールインスタンスを、メソッド内ではクラスインスタンスを参照する。
 * 10.プログラムのエントリーポイントとしてmain関数を定義する必要がある。main関数を定義したモジュールはメインモジュールと呼ばれ、そのプログラム内での
 * トップレベルモジュールとなる。
 * 11.Expressoにおけるグローバル変数とは、自身が定義されているモジュールスコープの変数を指す。つまり、main関数内でのグローバル変数は
 * mainモジュールの変数を指す。したがってグローバル変数と呼ばれていても、requireしたモジュールの場合、export指定されていない限りはその変数に
 * アクセスすることは出来ない。
 * 
 * Expresso組み込みの型に関して
 * int           : いわゆる整数型。C#のint型を使用。
 * bool          : C#のboolean型を使用。
 * float         : いわゆる浮動小数点型。６４ビット精度版のみ実装。C#ではdoubleを使用。
 * bigint        : いわゆる多倍長整数型。C#では、BigIntegerクラスを使用。
 * rational      : 分数型。分子と分母をBigInteger型でもつため、メモリーの許す限り有理数を完璧な精度で保持できる。
 * string        : いわゆる文字列型。C#のstring型を使用。C#以外で実装する場合、文字列の比較をオブジェクトの参照の比較で行うように実装すること。
 * bytearray     : Cで言うところのchar[]型。要するにバイトの配列。C#では、char型の配列で実装。
 * var(variant)  : 総称型。実装上は、どんな型の変数でも指し示すことのできるポインターや参照。
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
 * 一度目は、組み込み型すらC#上でクラスを作成していたためにコピーが発生するたびに新たなインスタンスを生成していたのを、基本的な型（具体的にはExpresso上の
 * int, float, string, bool型である）に関してはC#の組み込み型を使用するようにしたことである。
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
	/// The Expresso's intepreter. Because it interprets the AST node directly it is very slow to run the code.
	/// </summary>
	public sealed class Interpreter
	{
		/// <summary>
		/// プログラムのエントリーポイントを含むメインモジュール。そのプログラム内でのトップレベルモジュールでもある。
		/// The main module, which has the main function(the entry point for an Expresso program).
		/// </summary>
		/// <value>
		/// The main module.
		/// </value>
		public static ExpressoAst MainModule{get; set;}

		public static Interpreter CurRuntime{get; internal set;}

		public string CurOpenedSourceFileName{get; set;}

		/// <summary>
		/// トップレベルのコンテクスト。
		/// The toplevel context of a runtime.
		/// </summary>
		private ExpressoContext global_context = new ExpressoContext();

		public ExpressoContext GlobalContext{
			get{return global_context;}
		}

		/// <summary>
		/// main関数をエントリーポイントとしてプログラムを実行する。
		/// Run the program with the "main" function as the entry point.
		/// </summary>
		/// <param name="args">
		/// Arguments passed into the C#'s main function.
		/// </param>
		/// <exception cref='EvalException'>
		/// Is thrown when the eval exception.
		/// </exception>
		/// <returns>The return value of the Expresso's main function</returns>
		public object Run(List<object> args = null)
		{
			Interpreter.CurRuntime = this;
			if(MainModule == null)
				throw ExpressoOps.ReferenceError("Missing main module!");

			Interpret(MainModule, global_context.SharedContext);

			var main_module = global_context.GetModule("main");
			var main_func = main_module.LookupMember("main") as FunctionDefinition;
			if(main_func == null)
				throw ExpressoOps.RuntimeError("No entry point");

			if(args == null)
				args = new List<object>();

			var mod_context = new ModuleContext(main_module, global_context);
			var main_args = ExpressoOps.Slice(args, new ExpressoIntegerSequence(1, args.Count, 1));
			var call = Node.MakeCallExpr(
				Node.MakeConstant(ObjectTypes.FUNCTION, main_func),
				new Expression[]{
					Node.MakeConstant(ObjectTypes.INSTANCE, main_module),
					Node.MakeConstant(ObjectTypes.LIST, main_args)
				}
			);

			return Interpret(call, mod_context.GlobalContext);
		}

		/// <summary>
		/// あるASTノードを対象にインタープリターを起動する。
		/// Run the interpreter on a specified AST node.
		/// Can be useful for interactive mode.
		/// </summary>
		public object Run(Node node, bool useShared)
		{
			if(node == null)
				throw new ArgumentNullException("node", "Can not evaluate a null node.");

			if(useShared)
				return Interpret(node, global_context.SharedContext);
			else{
				var mod_context = new ModuleContext(new Dictionary<object, object>(), global_context);
				return Interpret(node, mod_context.GlobalContext);
			}
		}
		
		public string GetRelativePathToCurrentSource(string path)
		{
			var last_slash_pos = this.CurOpenedSourceFileName.LastIndexOf('/');
			var parent_dir = this.CurOpenedSourceFileName.Substring(0, last_slash_pos + 1);
			return parent_dir + path;
		}

		internal object Interpret(Node inputNode, CodeContext context)
		{
			Node node = inputNode;	//The next node to be evaluated
			var flow_manager = new InterpreterFlowManager();

			try{
				while(true){
				MAIN_LOOP:	//sub-expressionを評価する必要があるノードの飛び先
					switch(node.Type){
					case NodeType.Argument:
						node = ((Argument)node).Option;
						goto MAIN_LOOP;

					case NodeType.AssertStatement:
						break;

					case NodeType.Assignment:
					{
						var assignment = (Assignment)node;
						if(!flow_manager.IsEvaluating(assignment)){		//まず右辺値を評価する
							flow_manager.Push(assignment);
							node = assignment.Right;
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();

							ExpressoTuple rhs = (ExpressoTuple)flow_manager.PopValue();
							for(int i = 0; i < assignment.Left.Length; ++i){		//その後左辺値に代入する
								var seq = (SequenceExpression)assignment.Left[i];

								for(int j = 0; j < rhs.Count; ++j){
									var assignable = seq.Items[j] as Assignable;
									if(assignable == null)
										throw ExpressoOps.RuntimeError("{0} is not an assignable reference.", seq.Items[j]);
							
									assignable.Assign(flow_manager.Top(), rhs[j]);
								}
							}
						}
						break;
					}

					case NodeType.BinaryExpression:
					{
						var binary_op = (BinaryExpression)node;
						if(!flow_manager.IsEvaluating(binary_op))
							flow_manager.Push(binary_op);

						switch(flow_manager.Top().StepCounter){
						case 0:
							node = binary_op.Left;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 1:
							node = binary_op.Right;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 2:
							flow_manager.Pop();

							object rhs = flow_manager.PopValue(), lhs = flow_manager.PopValue();
							if(lhs == null || rhs == null)
								throw ExpressoOps.RuntimeError("Can not apply the operation on null objects.");

							object ret = null;
							if((int)binary_op.Operator <= (int)OperatorType.MOD){
								if(lhs is int)
									ret = BinaryExprAsInt((int)lhs, (int)rhs, binary_op.Operator);
								else if(lhs is double)
									ret = BinaryExprAsDouble((double)lhs, (double)rhs, binary_op.Operator);
								else if(lhs is Fraction)
									ret = BinaryExprAsFraction((Fraction)lhs, rhs, binary_op.Operator);
								else
									ret = BinaryExprAsString((string)lhs, rhs, binary_op.Operator);
							}else if((int)binary_op.Operator < (int)OperatorType.AND){
								ret = EvalComparison(lhs as IComparable, rhs as IComparable, binary_op.Operator);
							}else if((int)binary_op.Operator < (int)OperatorType.BIT_OR){
								ret = EvalLogicalOperation((bool)lhs, (bool)rhs, binary_op.Operator);
							}else{
								ret = EvalBitOperation((int)lhs, (int)rhs, binary_op.Operator);
							}
							flow_manager.PushValue(ret);
							break;
						}
						break;
					}

					case NodeType.Block:
					{
						var block = (Block)node;
						if(!flow_manager.IsEvaluating(block))
							flow_manager.Push(block);

						if(flow_manager.Top().ChildCounter < block.Statements.Count){
							node = block.Statements[flow_manager.Top().ChildCounter++];
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();
						}
						break;
					}

					case NodeType.BreakStatement:
					{
						var break_stmt = (BreakStatement)node;
						for(int i = break_stmt.Count; i > 0;){		//Count階層分ループを遡るまで出会ったbreakableをノードスタックから取り除く
							Node popped = flow_manager.Pop().TargetNode;
							if(popped.Type == NodeType.ForStatement || popped.Type == NodeType.WhileStatement)
								--i;
						}
						break;
					}

					case NodeType.Call:
					{
						var call = (Call)node;
						if(!flow_manager.IsEvaluating(call))
							flow_manager.Push(call);

						switch(flow_manager.Top().StepCounter){
						case 0:
							node = call.Target;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 1:
							var fn = flow_manager.Top().Get(0) as FunctionDefinition;
							if(flow_manager.Top().ChildCounter < fn.Parameters.Length){	//実引数を評価してスタックに積む
								var index = flow_manager.Top().ChildCounter++;
								var param = fn.Parameters[index];
								node = (index < call.Arguments.Length) ? call.Arguments[index] : param;
								goto MAIN_LOOP;
							}

							flow_manager.Top().Remove(0);	//呼び出す関数オブジェクトをスタックから除く
							node = fn.Body;
							flow_manager.Top().StepCounter = fn.HasReturn ? 2 : 3;
							goto MAIN_LOOP;

						case 2:
							var ret = flow_manager.PopValue();
							flow_manager.Top().Clear();		//関数呼び出しに使ったスタック領域を解放する
							flow_manager.PushValue(ret);
							flow_manager.Pop();
							break;

						case 3:
							flow_manager.Top().Clear();		//関数呼び出しに使ったスタック領域を解放する
							flow_manager.Pop();
							break;
						}
						break;
					}

					case NodeType.CaseClause:
					{
						var case_clause = (CaseClause)node;
						if(!flow_manager.IsEvaluating(case_clause))
							flow_manager.Push(case_clause);

						switch(flow_manager.Top().StepCounter){
						case 0:
							if(flow_manager.Top().ChildCounter < case_clause.Labels.Length){
								node = case_clause.Labels[flow_manager.Top().ChildCounter++];
								goto MAIN_LOOP;
							}else{
								flow_manager.PushValue(false);
								flow_manager.Pop();
							}
							break;

						case 1:
							var label_obj = flow_manager.PopValue();
							var target = flow_manager.TopValue();
							if(target is int && label_obj is ExpressoIntegerSequence){
								var int_seq = (ExpressoIntegerSequence)label_obj;
								if(int_seq.Includes((int)target)){
									node = case_clause.Body;
									flow_manager.Top().StepCounter++;
									goto MAIN_LOOP;
								}else{
									flow_manager.Top().StepCounter--;
								}
							}else if(label_obj is Constant && ((Constant)label_obj).ValType == ObjectTypes._CASE_DEFAULT){
								node = case_clause.Body;
								flow_manager.Top().StepCounter++;
								goto MAIN_LOOP;
							}else if(label_obj != null && label_obj.Equals(target)){
								node = case_clause.Body;
								flow_manager.Top().StepCounter++;
								goto MAIN_LOOP;
							}else{
								flow_manager.Top().StepCounter--;
							}
							break;

						case 2:
							flow_manager.Pop();
							flow_manager.PushValue(true);
							break;
						}

						break;
					}

					case NodeType.CastExpression:
					{
						var cast_expr = (CastExpression)node;
						if(!flow_manager.IsEvaluating(cast_expr))
							flow_manager.Push(cast_expr);

						switch(flow_manager.Top().StepCounter){
						case 0:
							node = cast_expr.Target;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 1:
							node = cast_expr.ToExpression;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 2:
							flow_manager.Pop();

							var to_expr = flow_manager.PopValue();
							var target = flow_manager.PopValue();
							break;
						}
						break;
					}

					case NodeType.CatchClause:
					{
						var catch_clause = (CatchClause)node;
						var exception = flow_manager.TopValue() as ExpressoThrowException;
						if(exception == null)
							throw ExpressoOps.SystemError("The top value of stack is not an exception.");
						else if(catch_clause.Catcher.ParamType.TypeName == exception.Thrown.Name){
							node = catch_clause.Body;
							goto case NodeType.Block;
						}
						break;
					}

					case NodeType.Comprehension:
					{
						break;
					}

					case NodeType.ComprehensionFor:
					{
						break;
					}

					case NodeType.ComprehensionIf:
					{
						break;
					}

					case NodeType.ConditionalExpression:
					{
						var cond_expr = (ConditionalExpression)node;
						if(!flow_manager.IsEvaluating(cond_expr)){
							node = cond_expr.Condition;
							flow_manager.Push(cond_expr);
						}else{
							flow_manager.Pop();
							node = (bool)flow_manager.PopValue() ? cond_expr.TrueExpression : cond_expr.FalseExpression;
						}
						goto MAIN_LOOP;
					}

					case NodeType.Constant:
						flow_manager.PushValue(((Constant)node).Value);
						break;

					case NodeType.ContinueStatement:
					{
						var continue_stmt = (ContinueStatement)node;
						for(int i = continue_stmt.Count; i > 0;){		//Count階層分ループを遡るまで出会ったbreakableをノードスタックから取り除く
							Node popped = flow_manager.Pop().TargetNode;
							if(popped.Type == NodeType.ForStatement || popped.Type == NodeType.WhileStatement &&
							   i != 1)				//continueすべきループはスタックから外さない
								--i;
						}
						break;
					}

					case NodeType.ExprStatement:
					{
						var expr_stmt = (ExprStatement)node;
						if(!flow_manager.IsEvaluating(expr_stmt))
							flow_manager.Push(expr_stmt);

						if(flow_manager.Top().ChildCounter < expr_stmt.Expressions.Length){
							node = expr_stmt.Expressions[flow_manager.Top().ChildCounter++];
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();
						}
						break;
					}

					case NodeType.FinallyClause:
					{
						var finally_clause = (FinallyClause)node;
						node = finally_clause.Body;
						goto case NodeType.Block;
					}

					case NodeType.ForStatement:
					{
						var for_stmt = (ForStatement)node;
						if(!flow_manager.IsEvaluating(for_stmt))
							flow_manager.Push(for_stmt);

						switch(flow_manager.Top().StepCounter){
						case 0:
							node = for_stmt.Target;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 1:
						{
							object iterable = flow_manager.TopValue();
							if(!(iterable is IEnumerable))
								throw ExpressoOps.InvalidTypeError("Can not evaluate the expression to an iterable object!");

							Identifier[] lvalues = new Identifier[for_stmt.Left.Count];
							for(int i = 0; i < for_stmt.Left.Count; ++i){
								lvalues[i] = for_stmt.Left.Items[i] as Identifier;
								if(lvalues[i] == null)
									throw ExpressoOps.ReferenceError("The left-hand-side of the \"in\" keyword must be a lvalue(a referencible value such as variables)");
							}
							flow_manager.PushValue(lvalues);
							flow_manager.Top().StepCounter++;
							goto case 2;
						}

						case 2:
							Identifier[] lhs = (Identifier[])flow_manager.TopValue();
							object iterable_obj = flow_manager.GetValue(flow_manager.ValueCount - 2);
							var rvalue = ExpressoOps.Enumerate(iterable_obj);
							if(rvalue.MoveNext()){
								for(int j = 0; j < lhs.Length; ++j){
									lhs[j].Assign(flow_manager.Top(), rvalue.Current);
									if(j + 1 != lhs.Length){
										if(!rvalue.MoveNext())
											throw ExpressoOps.RuntimeError("The number of rvalues must be some multiple of that of lvalues.");
									}
								}

								node = for_stmt.Body;
								goto MAIN_LOOP;
							}else{
								flow_manager.Pop();
							}
							break;
						}
						break;
					}

					case NodeType.FunctionDef:
						//node = ((FunctionDefinition)node).Body;
						//goto MAIN_LOOP;
						flow_manager.PushValue(node);
						break;

					case NodeType.Identifier:
					{
						var ident = (Identifier)node;
						if(ident.ParamType.ObjType == ObjectTypes._SUBSCRIPT)
							flow_manager.PushValue(ident);
						else if(ident.IsResolved)
							flow_manager.Top().Dup(ident.Offset);
						else if(ident.ParamType.ObjType == ObjectTypes.TYPE_CLASS){
							var cur_module = flow_manager.Top().Get(0) as ExpressoObj;
							if(cur_module == null)
								throw ExpressoOps.RuntimeError("\"this\" doesn't refer to the enclosing class instance.");
							
							flow_manager.PushValue(cur_module.AccessMember(ident, true));
						}else if(ident.ParamType.ObjType == ObjectTypes.TYPE_MODULE){
							var module = context.LanguageContext.GetModule(ident.Name);
							if(module == null)
								throw ExpressoOps.RuntimeError(string.Format("The requested module \"{0}\" doesn't exist.", ident.Name));
							
							flow_manager.PushValue(module);
						}

						break;
					}

					case NodeType.IfStatement:
					{
						var if_stmt = (IfStatement)node;
						if(!flow_manager.IsEvaluating(if_stmt)){
							node = if_stmt.Condition;
							flow_manager.Push(if_stmt);
						}else{
							flow_manager.Pop();

							node = (bool)flow_manager.PopValue() ? if_stmt.TrueBlock : if_stmt.FalseBlock;
						}
						goto MAIN_LOOP;
					}

					case NodeType.Initializer:
					{
						var initializer = (SequenceInitializer)node;
						if(!flow_manager.IsEvaluating(initializer))
							flow_manager.Push(initializer);

						if(flow_manager.Top().ChildCounter < initializer.Items.Length){
							node = initializer.Items[flow_manager.Top().ChildCounter++];
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();

							switch(initializer.ObjType){
							case ObjectTypes.LIST:
							{
								var values = new List<object>(initializer.Items.Length);
								for(int i = initializer.Items.Length; i > 0; --i)
									values.Add(flow_manager.PopValue());

								values.Reverse();	//スタックからポップした順番では、逆順になってしまうので、順序を入れ替える
								flow_manager.PushValue(ExpressoOps.MakeList(values));
								break;
							}

							case ObjectTypes.TUPLE:
							{
								var values = new List<object>(initializer.Items.Length);
								for(int i = initializer.Items.Length; i > 0; --i)
									values.Add(flow_manager.PopValue());

								values.Reverse();	//スタックからポップした順番では、逆順になってしまうので、順序を入れ替える
								flow_manager.PushValue(ExpressoOps.MakeTuple(values));
								break;
							}

							case ObjectTypes.DICT:
							{
								var keys = new List<object>(initializer.Items.Length / 2);
								var values = new List<object>(initializer.Items.Length / 2);
								for(int i = initializer.Items.Length; i > 0; --i){
									if(i % 2 == 1)
										keys.Add(flow_manager.PopValue());
									else
										values.Add(flow_manager.PopValue());
								}
								flow_manager.PushValue(ExpressoOps.MakeDict(keys, values));
								break;
							}

							default:
								throw ExpressoOps.RuntimeError("Unknown type of initializer!");
							}
						}
						break;
					}

					case NodeType.MemRef:
					{
						var mem_ref = (MemberReference)node;
						if(!flow_manager.IsEvaluating(mem_ref))
							flow_manager.Push(mem_ref);

						switch(flow_manager.Top().StepCounter){
						case 0:
							node = mem_ref.Target;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 1:
							node = mem_ref.Subscription;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 2:
							flow_manager.Pop();

							var subscription = flow_manager.PopValue();
							var obj = flow_manager.PopValue();
							if(subscription is ExpressoIntegerSequence){
								var seq = (ExpressoIntegerSequence)subscription;
								flow_manager.PushValue(ExpressoOps.Slice(obj, seq));
							}else if(obj is ExpressoObj){
								var exs_obj = (ExpressoObj)obj;
								var member = exs_obj.AccessMember(subscription, obj == flow_manager.Top().Get(0));
								flow_manager.PushValue(member);
							}else{
								var member = ExpressoOps.AccessMember(obj, subscription);
								flow_manager.PushValue(member);
							}
							break;

						default:
							throw ExpressoOps.SystemError("Unknown path reached while evaluating a member reference expression!");
						}
						break;
					}

					case NodeType.Toplevel:
					{
						var toplevel = (ExpressoAst)node;
						if(!flow_manager.IsEvaluating(toplevel))
							flow_manager.Push(toplevel);

						if(flow_manager.Top().ChildCounter < toplevel.Body.Length){
							node = toplevel.Body[flow_manager.Top().ChildCounter++];
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();

							if(toplevel.IsModule){
								var internals = new Dictionary<string, int>();
								var exported = new Dictionary<string, int>();
								Dictionary<string, int> decl_target = null;
								var members = new List<object>();
								int offset = 0;

								for(int i = 0; i < toplevel.Body.Length; ++i){
									decl_target = toplevel.ExportMap.Get(i) ? exported : internals;
									var stmt = toplevel.Body[i];
									object val = flow_manager.Top().Get(offset);

									if(stmt is ExprStatement){
										var expr_stmt = (ExprStatement)stmt;
										if(expr_stmt.Expressions[0] is VarDeclaration){
											var var_decls = (VarDeclaration)expr_stmt.Expressions[0];
											
											for(int j = 0; j < var_decls.Left.Length; ++j){
												val = flow_manager.Top().Get(offset);
												decl_target.Add(var_decls.Left[i].Name, offset++);
												members.Add(val);
												val = flow_manager.Top().Get(offset);
												//varStore.Add(var_decls.Variables[i].Offset, obj);	//モジュールスコープの変数ストアにも実体を追加しておく
											}
										}else{
											throw ExpressoOps.InvalidTypeError("A module declaration can not have that type of statements!");
										}
									}else if(stmt is FunctionDefinition){
										var method = (FunctionDefinition)stmt;
										decl_target.Add(method.Name, offset++);
										members.Add(val);
									}else if(stmt is TypeDefinition){
										var type_decl = (TypeDefinition)stmt;
										var type_def = flow_manager.Top().Get(offset);
										decl_target.Add(type_decl.Name, offset++);
										members.Add(type_def);
									}else{
										throw ExpressoOps.InvalidTypeError("A module declaration can not have that type of statements!");
									}
								}

								var module_def = new ModuleDefinition(toplevel.ModuleName, internals, exported, members.ToArray());
								var module_inst = new ExpressoModule(module_def);
								context.LanguageContext.PublishModule(toplevel.ModuleName, module_inst);
							}
						}
						break;
					}

					case NodeType.New:
					{
						var new_expr = (NewExpression)node;
						if(!flow_manager.IsEvaluating(new_expr)){
							node = new_expr.TargetExpr;
							flow_manager.Push(new_expr);
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();

							var type_def = flow_manager.PopValue() as BaseDefinition;
							if(type_def == null)
								throw ExpressoOps.InvalidTypeError("{0} doesn't refer to a type name.", new_expr.TargetExpr);

							flow_manager.PushValue(ExpressoObj.CreateInstance(null, type_def, new_expr.Arguments));
						}
						break;
					}

					case NodeType.Print:
					{
						var print_stmt = (PrintStatement)node;
						if(!flow_manager.IsEvaluating(print_stmt))
							flow_manager.Push(print_stmt);

						if(flow_manager.Top().ChildCounter < print_stmt.Expressions.Length){
							node = print_stmt.Expressions[flow_manager.Top().ChildCounter++];
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();

							var values = new List<object>(print_stmt.Expressions.Length);
							for(int i = print_stmt.Expressions.Length; i > 0; --i)
								values.Add(flow_manager.PopValue());

							ExpressoOps.DoPrint(values, print_stmt.HasTrailing);
						}
						break;
					}

					case NodeType.Require:
					{
						var require_expr = (RequireStatement)node;
						foreach(var module_name in require_expr.ModuleNames){
							var path = module_name.Replace('.', '/');
							path += ".exs";
							path = GetRelativePathToCurrentSource(path);
							var module_parser = new Parser(new Scanner(path));
							module_parser.ParsingFileName = module_name;
							module_parser.Parse();
						}
						
						//module_parser.TopmostAst.Run(varStore);
						break;
					}

					case NodeType.Return:
					{
						var return_stmt = (ReturnStatement)node;
						if(return_stmt.Expression != null){
							if(!flow_manager.IsEvaluating(return_stmt)){
								flow_manager.Push(return_stmt);
								node = return_stmt.Expression;
								goto MAIN_LOOP;
							}else{
								flow_manager.Pop();

								ExpressoTuple ret = (ExpressoTuple)flow_manager.PopValue();
								if(ret.Count == 1)
									flow_manager.PushValue(ret[0]);
								else
									flow_manager.PushValue(ret);
							}
						}
						break;
					}

					case NodeType.IntSequence:
					{
						var intseq_expr = (IntSeqExpression)node;
						if(!flow_manager.IsEvaluating(intseq_expr))
							flow_manager.Push(intseq_expr);

						switch(flow_manager.Top().StepCounter){
						case 0:
							node = intseq_expr.Lower;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 1:
							node = intseq_expr.Upper;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 2:
							node = intseq_expr.Step;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 3:
							flow_manager.Pop();

							var step = flow_manager.PopValue();
							var end = flow_manager.PopValue();
							var start = flow_manager.PopValue();
							if(!(start is int) || !(end is int) || !(step is int))
								throw ExpressoOps.InvalidTypeError("The start, end and step expressions of the IntSeq expression must yield an integer.");

							flow_manager.PushValue(new ExpressoIntegerSequence((int)start, (int)end, (int)step));
							break;

						default:
							throw ExpressoOps.SystemError("An error occurred while evaluating an IntegerSequence expression.");
						}
						break;
					}

					case NodeType.Sequence:
					{
						var seq_expr = (SequenceExpression)node;
						if(!flow_manager.IsEvaluating(seq_expr))
							flow_manager.Push(seq_expr);

						if(flow_manager.Top().ChildCounter < seq_expr.Count){
							node = seq_expr.Items[flow_manager.Top().ChildCounter++];
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();

							var item_values = new object[seq_expr.Count];
							for(int i = seq_expr.Count - 1; i >= 0; --i)
								item_values[i] = flow_manager.PopValue();

							flow_manager.PushValue(ExpressoOps.MakeTuple(item_values));
						}
						break;
					}

					case NodeType.SwitchStatement:
					{
						var switch_stmt = (SwitchStatement)node;
						if(!flow_manager.IsEvaluating(switch_stmt))
							flow_manager.Push(switch_stmt);

						switch(flow_manager.Top().StepCounter){
						case 0:
							node = switch_stmt.Target;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 1:
							if(flow_manager.Top().ChildCounter < switch_stmt.Cases.Length){
								if(flow_manager.Top().ChildCounter != 0){
									object top = flow_manager.PopValue();
									if(top is bool && (bool)top)
										goto CLEANUP_SWITCH;
								}
								node = switch_stmt.Cases[flow_manager.Top().ChildCounter++];
								goto MAIN_LOOP;
							}else{
								flow_manager.Pop();
							}
						CLEANUP_SWITCH:
							flow_manager.PopValue();		//caseラベルと照合する値をスタックから外す
							break;

						default:
							throw ExpressoOps.SystemError("Unknown path reached while evaluating a switch statement!");
						}
						break;
					}

					case NodeType.ThrowStatement:
					{
						var throw_stmt = (ThrowStatement)node;
						if(!flow_manager.IsEvaluating(throw_stmt)){
							node = throw_stmt.Expression;
							flow_manager.Push(throw_stmt);
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();
						}
						break;
					}

					case NodeType.TryStatement:
					{
						var try_stmt = (TryStatement)node;
						if(!flow_manager.IsEvaluating(try_stmt)){
							node = try_stmt.Body;
							flow_manager.Push(try_stmt);
							goto case NodeType.Block;
						}else{		//ノードスタックのトップがこのtry文だった場合、try文の本体の実行は終わっている
							var top = flow_manager.TopValue();
							if(top is ExpressoThrowException){
								if(try_stmt.Catches != null){
									node = try_stmt.Catches[flow_manager.Top().ChildCounter++];
									goto case NodeType.CatchClause;
								}else{
									flow_manager.Pop();
									while(flow_manager.Count > 0){	//一つ上のtry文にたどり着くまでノードスタックを巻き戻す
										flow_manager.Pop();
										if(flow_manager.Top().TargetNode.Type == NodeType.TryStatement)
											break;	//NOTE: 次のループに入る前に親のノードを参照するので、ここでは何もしない
									}

									if(flow_manager.Count == 0)	//このtry文を内包するtry文が見つからなかったので、C#側の例外ハンドラに処理を投げる
										throw (ExpressoThrowException)top;
								}
							}

							if(try_stmt.FinallyClause != null){
								node = try_stmt.FinallyClause;
								if(flow_manager.IsEvaluating(try_stmt))	//finally節を実行したらこのtry文には用はなくなる
									flow_manager.Pop();

								goto case NodeType.FinallyClause;
							}

							flow_manager.Pop();		//ここに到達するのはfinally節がなく、try文の本体で例外が発生しなかった場合だけ
						}
						break;
					}

					case NodeType.TypeDef:
					{
						var type_def = (TypeDefinition)node;
						if(!flow_manager.IsEvaluating(type_def))
							flow_manager.Push(type_def);

						if(flow_manager.Top().ChildCounter < type_def.Body.Length){
							node = type_def.Body[flow_manager.Top().ChildCounter++];
							goto MAIN_LOOP;
						}else{

						}
						break;
					}

					case NodeType.UnaryExpression:
					{
						var unary_op = (UnaryExpression)node;
						if(!flow_manager.IsEvaluating(unary_op)){
							node = unary_op.Operand;
							flow_manager.Push(unary_op);
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();

							var ope = flow_manager.PopValue();
							if(ope == null)
								throw ExpressoOps.InvalidTypeError("Invalid object type!");
							
							flow_manager.PushValue(EvalUnaryOperation(unary_op.Operator, ope));
						}
						break;
					}

					case NodeType.VarDecl:
					{
						var var_decl = (VarDeclaration)node;
						if(!flow_manager.IsEvaluating(var_decl))
							flow_manager.Push(var_decl);

						int child_counter = flow_manager.Top().ChildCounter;
						if(child_counter < var_decl.Left.Length){
							node = var_decl.Expressions[flow_manager.Top().ChildCounter++];
							goto MAIN_LOOP;
						}else{
							flow_manager.Pop();
						}
						break;
					}

					case NodeType.WhileStatement:
					{
						var while_stmt = (WhileStatement)node;
						if(!flow_manager.IsEvaluating(while_stmt))
							flow_manager.Push(while_stmt);

						switch(flow_manager.Top().StepCounter){
						case 0:
							node = while_stmt.Condition;
							flow_manager.Top().StepCounter++;
							goto MAIN_LOOP;

						case 1:
							var cond = flow_manager.PopValue();
							try{
								if(cond != null && (bool)cond){
									node = while_stmt.Body;
									flow_manager.Top().StepCounter--;
									goto MAIN_LOOP;
								}else
									flow_manager.Pop();
							}
							catch(Exception){
								if(!(cond is bool))
									throw ExpressoOps.InvalidTypeError("Invalid expression! The condition of a while statement must yield a boolean!");
							}
							break;

						default:
							throw ExpressoOps.SystemError("Unknown path reached while evaluating a while statement!");
						}
						break;
					}

					case NodeType.WithStatement:
						break;

					case NodeType.YieldStatement:
						break;

					default:
						throw ExpressoOps.RuntimeError("Unknown AST type!");
					}

					if(flow_manager.Count != 0)
						node = flow_manager.Top().TargetNode;
					else
						break;		//評価スタックがなくなったので、ループを抜ける
				}
			}
			catch(Exception ex){
				Type exception_type = ex.GetType();
				Console.WriteLine("{0}: {1}", exception_type.FullName, ex.Message);
			}

			return (flow_manager.ValueCount > 0) ? flow_manager.PopValue() : null;
		}

		private static int BinaryExprAsInt(int lhs, int rhs, OperatorType opType)
		{
			int result;
			
			switch (opType) {
			case OperatorType.PLUS:
				result = lhs + rhs;
				break;
				
			case OperatorType.MINUS:
				result = lhs - rhs;
				break;
				
			case OperatorType.TIMES:
				result = lhs * rhs;
				break;
				
			case OperatorType.DIV:
				result = lhs / rhs;
				break;
				
			case OperatorType.POWER:
				result = (int)Math.Pow(lhs, rhs);
				break;
				
			case OperatorType.MOD:
				result = lhs % rhs;
				break;
				
			default:
				throw ExpressoOps.SystemError("Unreachable code");
			}
			
			return result;
		}
		
		private static double BinaryExprAsDouble(double lhs, double rhs, OperatorType opType)
		{
			double result;
			
			switch (opType) {
			case OperatorType.PLUS:
				result = lhs + rhs;
				break;
				
			case OperatorType.MINUS:
				result = lhs - rhs;
				break;
				
			case OperatorType.TIMES:
				result = lhs * rhs;
				break;
				
			case OperatorType.DIV:
				result = lhs / rhs;
				break;
				
			case OperatorType.POWER:
				result = Math.Pow(lhs, rhs);
				break;
				
			case OperatorType.MOD:
				result = Math.IEEERemainder(lhs, rhs);
				break;
				
			default:
				throw ExpressoOps.SystemError("Unreachable code");
			}
			
			return result;
		}
		
		private static Fraction BinaryExprAsFraction(Fraction lhs, object rhs, OperatorType opType)
		{
			if(!(rhs is Fraction) && !(rhs is long) && !(rhs is int) && !(rhs is double))
				throw ExpressoOps.InvalidTypeError("The right operand have to be either a long, int, double or fraction!");
			
			Fraction result;
			
			switch(opType){
			case OperatorType.PLUS:
				result = lhs + rhs;
				break;
				
			case OperatorType.MINUS:
				result = lhs - rhs;
				break;
				
			case OperatorType.TIMES:
				result = lhs * rhs;
				break;
				
			case OperatorType.DIV:
				result = lhs / rhs;
				break;
				
			case OperatorType.POWER:
				result = lhs.Power(rhs);
				break;
				
			case OperatorType.MOD:
				result = lhs % rhs;
				break;
				
			default:
				throw ExpressoOps.SystemError("Unreachable code");
			}
			
			return result;
		}
		
		private static string BinaryExprAsString(string lhs, object rhs, OperatorType opType)
		{
			string result;
			
			switch(opType){
			case OperatorType.PLUS:
				result = String.Concat(lhs, rhs.ToString());
				break;
				
			case OperatorType.TIMES:
				if(!(rhs is int))
					throw ExpressoOps.InvalidTypeError("Can not muliply string by objects other than an integer.");
				
				int times = (int)rhs;
				var sb = new StringBuilder(lhs.Length * times);
				for(; times > 0; --times) sb.Append(lhs);
				
				result = sb.ToString();
				break;
				
			default:
				throw ExpressoOps.RuntimeError("Strings don't support that operation!");
			}
			
			return result;
		}
		
		private static bool EvalComparison(IComparable lhs, IComparable rhs, OperatorType opType)
		{
			if(lhs == null || rhs == null)
				throw ExpressoOps.InvalidTypeError("The operands can not be compared");
			
			switch (opType) {
			case OperatorType.EQUAL:
				return object.Equals(lhs, rhs);
				
			case OperatorType.GREAT:
				return lhs.CompareTo(rhs) > 0;
				
			case OperatorType.GRTE:
				return lhs.CompareTo(rhs) >= 0;
				
			case OperatorType.LESE:
				return lhs.CompareTo(rhs) <= 0;
				
			case OperatorType.LESS:
				return lhs.CompareTo(rhs) < 0;
				
			case OperatorType.NOTEQ:
				return !object.Equals(lhs, rhs);
				
			default:
				return false;
			}
		}
		
		private static bool EvalLogicalOperation(bool lhs, bool rhs, OperatorType opType)
		{
			switch (opType) {
			case OperatorType.AND:
				return lhs && rhs;
				
			case OperatorType.OR:
				return lhs || rhs;
				
			default:
				return false;
			}
		}
		
		private static int EvalBitOperation(int lhs, int rhs, OperatorType opType)
		{
			switch (opType) {
			case OperatorType.BIT_AND:
				return lhs & rhs;
				
			case OperatorType.BIT_XOR:
				return lhs ^ rhs;
				
			case OperatorType.BIT_OR:
				return lhs | rhs;
				
			case OperatorType.BIT_LSHIFT:
				return lhs << rhs;
				
			case OperatorType.BIT_RSHIFT:
				return lhs >> rhs;
				
			default:
				throw ExpressoOps.RuntimeError("Invalid Operation!");
			}
		}

		private static object EvalUnaryOperation(OperatorType opType, object operand)
		{
			object result = null;
			if(opType == OperatorType.MINUS){
				if(operand is int)
					result = -(int)operand;
				else if(operand is double)
					result = -(double)operand;
				else if(operand is Fraction)
					result = -(Fraction)operand;
				else
					throw ExpressoOps.InvalidTypeError("The minus operator is not applicable to the operand!");
			}else if(opType == OperatorType.NOT){
				if(operand is bool)
					result = !(bool)operand;
				else
					throw ExpressoOps.InvalidTypeError("The not operator is not applicable to the operand!");
			}

			return result;
		}
	}
}

