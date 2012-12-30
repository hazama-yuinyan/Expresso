using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Expresso.Ast;
using Expresso.Builtins;
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
	/// The Expresso's intepreter.
	/// </summary>
	public class Interpreter
	{
		/// <summary>
		/// プログラムのエントリーポイントを含むメインモジュール。そのプログラム内でのトップレベルモジュールでもある。
		/// The main module, which has the main function(the entry point for an Expresso program).
		/// </summary>
		/// <value>
		/// The main module.
		/// </value>
		public static ModuleDeclaration MainModule{get; set;}

		public static Interpreter CurRuntime{get; internal set;}

		public string CurOpenedSourceFileName{get; set;}

		/// <summary>
		/// トップレベルの変数ストア。main関数を含むモジュール内の子スコープは、この変数ストアを親として持つ。
		/// The toplevel variable store of a module.
		/// </summary>
		private VariableStore var_store = new VariableStore();

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
			var main_module = ExpressoModule.GetModule("main");
			if(main_module == null)
				throw ExpressoOps.ReferenceError("Missing main module!");

			var main_func = main_module.AccessMember(new Identifier("main"), true) as Function;
			if(main_func == null)
				throw ExpressoOps.RuntimeError("No entry point");

			if(args == null)
				args = new List<object>();

			var main_args = ImplementationHelpers.Slice(args, new ExpressoIntegerSequence(1, args.Count, 1));
			var call = new Call{
				Function = main_func,
				Arguments = new List<Expression>{new Constant{ValType = ObjectTypes.LIST, Value = main_args}}
			};

			var_store.Add(0, main_module);
			return call.Run(var_store);
		}

		/// <summary>
		/// あるASTノードを対象にインタープリターを起動する。
		/// Run the interpreter on a specified AST node.
		/// Can be useful for interactive mode.
		/// </summary>
		public object Run(Node node)
		{
			if(node == null)
				throw new ArgumentNullException("node", "Can not evaluate a null node.");

			return node.Run(var_store);
		}
		
		/// <summary>
		/// メインモジュールの定義文を実行してグローバルの環境を初期化する。
		/// Executes all the definition statements in the main module
		/// in order to initialize the global environment.
		/// </summary>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		public void Initialize()
		{
			Interpreter.CurRuntime = this;
			if(MainModule == null)
				throw ExpressoOps.ReferenceError("The \"main\" module not found!");

			MainModule.Run(var_store);
		}

		public VariableStore GetGlobalVarStore()
		{
			return var_store;
		}

		public string GetRelativePathToCurrentSource(string path)
		{
			var last_slash_pos = this.CurOpenedSourceFileName.LastIndexOf('/');
			var parent_dir = this.CurOpenedSourceFileName.Substring(0, last_slash_pos + 1);
			return parent_dir + path;
		}

		private object Interpret(Node inputNode, VariableStore varStore)
		{
			Node node = inputNode;	//The next node to be evaluated
			Stack<Node> node_stack = new Stack<Node>();	//The parent nodes. If it is empty, then the node which is being evaluated has no parent nodes
			Stack<Expression> binary_exprs;
			ValueStack value_stack = new ValueStack(100),	//The stack for values. It can contain the return value for the entire function or a piece of code
				bin_expr_operands;	//if the operation is inline
			int next_child = 0,		//The index for the next child node to be evaluated
				next_step = 0;		//The counter for the next operation to be performed

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
						if(next_child != assignment.Expressions.Count){		//まず右辺値をすべて評価する
							node = assignment.Expressions[next_child++];
							goto MAIN_LOOP;
						}else{
							next_child = 0;
							for(int i = 0; i < assignment.Targets.Count; ++i){		//その後左辺値に代入する
								var assignable = assignment.Targets[i] as Assignable;
								if(assignable == null)
									throw ExpressoOps.RuntimeError("{0} is not an assignable reference.", assignment.Targets[i]);
							
								assignable.Assign(varStore, value_stack.PeekAt(i));
							}
							object ret_val = value_stack.PeekAt(0);
							value_stack.Clear();
							value_stack.Push(ret_val);	//x = y = 0;みたいな表記を許容するために右辺値の一番目を戻り値にする
						}
						break;
					}

					case NodeType.BinaryExpression:
					{
						/*var binary_op = (BinaryExpression)node;
						switch(value_stack.Count){
						case 0:
							node_stack.Push(binary_op);
							node = binary_op.Left;
							break;

						case 1:
							node = binary_op.Right;
							break;

						case 2:

						}
						object first = Interpret(binary_op.Left, varStore), second = Interpret(binary_op.Right, varStore);
						if(first == null || second == null)
							throw ExpressoOps.RuntimeError("Can not apply the operation on null objects.");
						
						if((int)binary_op.Operator <= (int)OperatorType.MOD){
							if(first is int)
								return BinaryExprAsInt((int)first, (int)second, binary_op.Operator);
							else if(first is double)
								return BinaryExprAsDouble((double)first, (double)second, binary_op.Operator);
							else if(first is Fraction)
								return BinaryExprAsFraction((Fraction)first, second, binary_op.Operator);
							else
								return BinaryExprAsString((string)first, second, binary_op.Operator);
						}else if((int)binary_op.Operator < (int)OperatorType.AND){
							return EvalComparison(first as IComparable, second as IComparable, binary_op.Operator);
						}else if((int)binary_op.Operator < (int)OperatorType.BIT_OR){
							return EvalLogicalOperation((bool)first, (bool)second, binary_op.Operator);
						}else{
							return EvalBitOperation((int)first, (int)second, binary_op.Operator);
						}*/
						break;
					}

					case NodeType.Block:
					{
						var block = (Block)node;
						if(next_child < block.Statements.Count){
							if(next_child == 0)
								node_stack.Push(node);

							node = block.Statements[next_child++];
							goto MAIN_LOOP;
						}else{
							next_child = 0;
							node_stack.Pop();
						}
						break;
					}

					case NodeType.BreakStatement:
					{
						var break_stmt = (BreakStatement)node;
						for(int i = break_stmt.Count; i > 0;){		//Count階層分ループを遡るまで出会ったbreakableをノードスタックから取り除く
							Node popped = node_stack.Pop();
							if(popped.Type == NodeType.ForStatement || popped.Type == NodeType.WhileStatement)
								--i;
						}
						next_child = 0;
						break;
					}

					case NodeType.Call:
					{
						var call = (Call)node;
						VariableStore child_store;
						var target = call.ResolveCallTarget(out child_store, varStore);
						var fn = target.Item1;

						for(int i = target.Item2 ? 1 : 0; i < fn.Parameters.Count; ++i){	//実引数をローカル変数として変数テーブルに追加する
							var param = fn.Parameters[i];
							child_store.Add(param.Offset, (i < call.Arguments.Count) ? Interpret(call.Arguments[i], varStore) : param.Option.Run(varStore));
						}
						
						var local_vars = fn.LocalVariables;
						if(local_vars.Any()){					//Checking for its emptiness
							foreach(var local in local_vars)	//関数内で定義されているローカル変数を予め初期化しておく
								child_store.Add(local.Offset, ImplementationHelpers.GetDefaultValueFor(local.ParamType.ObjType));
						}
						
						value_stack.Push(Interpret(fn.Body, child_store));
						break;
					}

					case NodeType.CaseClause:
					{
						var case_clause = (CaseClause)node;
						var target = value_stack.Top();
						break;
					}

					case NodeType.CatchClause:
					{
						var catch_clause = (CatchClause)node;
						var exception = value_stack.Top() as ExpressoThrowException;
						if(exception == null)
							throw ExpressoOps.SystemError("The top value of value_stack is not an exception.");
						else if(catch_clause.Catcher.ParamType.TypeName == exception.Thrown.Name){
							node = catch_clause.Body;
							goto case NodeType.Block;
						}
						break;
					}

					case NodeType.Comprehension:
						break;

					case NodeType.ComprehensionFor:
						break;

					case NodeType.ComprehensionIf:
						break;

					case NodeType.ConditionalExpression:
					{
						var cond_expr = (ConditionalExpression)node;
						if(node_stack.Peek() == cond_expr){
							node = (bool)value_stack.Pop() ? cond_expr.TrueExpression : cond_expr.FalseExpression;
							node_stack.Pop();
						}else{
							node = cond_expr.Condition;
							node_stack.Push(cond_expr);
							goto MAIN_LOOP;
						}
						break;
					}

					case NodeType.Constant:
						value_stack.Push(((Constant)node).Value);
						break;

					case NodeType.ContinueStatement:
					{
						var continue_stmt = (ContinueStatement)node;
						for(int i = continue_stmt.Count; i > 0;){		//Count階層分ループを遡るまで出会ったbreakableをノードスタックから取り除く
							Node popped = node_stack.Pop();
							if(popped.Type == NodeType.ForStatement || popped.Type == NodeType.WhileStatement &&
							   i != 1)				//continueすべきループはスタックから外さない
								--i;
						}
						next_child = 0;
						break;
					}

					case NodeType.ExprStatement:
					{
						var expr_stmt = (ExprStatement)node;
						if(next_child < expr_stmt.Expressions.Count){
							if(next_child == 0)
								node_stack.Push(expr_stmt);

							node = expr_stmt.Expressions[next_child++];
							goto MAIN_LOOP;
						}else{
							next_child = 0;
							node_stack.Pop();
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
						break;

					case NodeType.Function:
						value_stack.Push(Interpret(((Function)node).Body, varStore));
						break;

					case NodeType.Identifier:
					{
						var ident = (Identifier)node;
						if(ident.ParamType.ObjType == ObjectTypes._SUBSCRIPT)
							value_stack.Push(ident);
						else if(ident.ParamType.ObjType == ObjectTypes.TYPE_CLASS){
							var cur_module = varStore.Get(0) as ExpressoObj;
							if(cur_module == null)
								throw ExpressoOps.RuntimeError("\"this\" doesn't refer to the enclosing module instance.");
							
							value_stack.Push(cur_module.AccessMember(ident, true));
						}else if(ident.ParamType.ObjType == ObjectTypes.TYPE_MODULE){
							var module = ExpressoModule.GetModule(ident.Name);
							if(module == null)
								throw ExpressoOps.RuntimeError(string.Format("The requested module \"{0}\" doesn't exist.", ident.Name));
							
							value_stack.Push(module);
						}else
							value_stack.Push(varStore.Get(ident.Offset, ident.Level));

						break;
					}

					case NodeType.IfStatement:
					{
						var if_stmt = (IfStatement)node;
						if(node_stack.Peek() == if_stmt){
							node = (bool)value_stack.Pop() ? if_stmt.TrueBlock : if_stmt.FalseBlock;
							node_stack.Pop();
						}else{
							node = if_stmt.Condition;
							node_stack.Push(if_stmt);
						}
						goto MAIN_LOOP;
					}

					case NodeType.Initializer:
					{
						var initializer = (ObjectInitializer)node;
						if(next_child < initializer.Initializer.Count){
							if(next_child == 0)
								node_stack.Push(initializer);

							node = initializer.Initializer[next_child++];
							goto MAIN_LOOP;
						}else{
							switch(initializer.ObjType){
							case ObjectTypes.LIST:
								var values = new List<object>(initializer.Initializer.Count);
								for(int i = initializer.Initializer.Count; i > 0; --i)
									values.Add(value_stack.Pop());
								
								break;

							case ObjectTypes.TUPLE:
								break;

							case ObjectTypes.DICT:
								break;

							default:
								throw ExpressoOps.RuntimeError("Unknown type of initializer!");
							}

							node_stack.Pop();
							next_child = 0;
						}
						break;
					}

					case NodeType.MemRef:
					{
						var mem_ref = (MemberReference)node;
						switch(next_step){
						case 0:
							node = mem_ref.Parent;
							node_stack.Push(mem_ref);
							++next_step;
							goto MAIN_LOOP;

						case 1:
							node = mem_ref.Subscription;
							++next_step;
							goto MAIN_LOOP;

						case 2:
							var subscription = value_stack.Pop();
							var obj = value_stack.Pop();
							if(subscription is ExpressoIntegerSequence){
								var seq = (ExpressoIntegerSequence)subscription;
								return ImplementationHelpers.Slice(obj, seq);
							}
							
							if(obj is ExpressoObj){
								var exs_obj = (ExpressoObj)obj;
								var member = exs_obj.AccessMember(subscription, obj == varStore.Get(0, 0));
								value_stack.Push((member is Function) ? new MethodContainer(member as Function, obj) : member);
							}else{
								var member = ImplementationHelpers.AccessMember((Identifier)mem_ref.Parent, obj, subscription);
								value_stack.Push((member is Function) ? new MethodContainer(member as Function, obj) : member);
							}
							node_stack.Pop();
							next_step = 0;
							break;

						default:
							throw ExpressoOps.SystemError("Unknown path reached while evaluating a member reference expression!");
						}
						break;
					}

					case NodeType.ModuleDecl:
						break;

					case NodeType.New:
					{
						var new_expr = (NewExpression)node;
						if(node_stack.Peek() == new_expr){
							var type_def = value_stack.Pop() as BaseDefinition;
							if(type_def == null)
								throw ExpressoOps.InvalidTypeError("{0} doesn't refer to a type name.", new_expr.TargetDecl);

							value_stack.Push(ExpressoObj.CreateInstance(type_def, new_expr.Arguments, varStore));
							node_stack.Pop();
						}else{
							node = new_expr.TargetDecl;
							node_stack.Push(new_expr);
							goto MAIN_LOOP;
						}
						break;
					}

					case NodeType.Print:
					{
						var print_stmt = (PrintStatement)node;
						if(next_child < print_stmt.Expressions.Count){
							if(next_child == 0)
								node_stack.Push(print_stmt);

							node = print_stmt.Expressions[next_child++];
							goto MAIN_LOOP;
						}else{
							next_child = 0;
							node_stack.Pop();
							var values = new List<object>(print_stmt.Expressions.Count);
							for(int i = print_stmt.Expressions.Count; i > 0; --i)
								values.Add(value_stack.Pop());

							DoPrint(values, print_stmt.HasTrailing);
						}
						break;
					}

					case NodeType.Require:
					{
						var require_expr = (RequireExpression)node;
						var path = require_expr.ModuleName.Replace('.', '/');
						path += ".exs";
						path = GetRelativePathToCurrentSource(path);
						var module_parser = new Parser(new Scanner(path));
						module_parser.ParsingFileName = require_expr.ModuleName;
						module_parser.Parse();
						
						module_parser.ParsingModule.Run(varStore);
						break;
					}

					case NodeType.Return:
					{
						var return_stmt = (ReturnStatement)node;
						if(return_stmt.Expressions.Count == 0){
							value_stack.Push(new ExpressoTuple(new List<object>()));
						}else{
							if(next_child < return_stmt.Expressions.Count){
								if(next_child == 0)
									node_stack.Push(return_stmt);

								node = return_stmt.Expressions[next_child++];
								goto MAIN_LOOP;
							}else{
								next_child = 0;
								node_stack.Pop();
								if(return_stmt.Expressions.Count != 1){
									var objs = new List<object>();
									for(int i = return_stmt.Expressions.Count; i > 0; --i)
										objs.Add(value_stack.Pop());
									
									value_stack.Push(ExpressoOps.MakeTuple(objs));
								}
							}
						}
						break;
					}

					case NodeType.Sequence:
					{
						var intseq_expr = (IntSeqExpression)node;
						switch(next_step){
						case 0:
							node = intseq_expr.Start;
							node_stack.Push(intseq_expr);
							++next_step;
							goto MAIN_LOOP;

						case 1:
							node = intseq_expr.End;
							++next_step;
							goto MAIN_LOOP;

						case 2:
							node = intseq_expr.Step;
							++next_step;
							goto MAIN_LOOP;

						case 3:
							var step = value_stack.Pop();
							var end = value_stack.Pop();
							var start = value_stack.Pop();
							if(!(start is int) || !(end is int) || !(step is int))
								throw ExpressoOps.InvalidTypeError("The start, end and step expressions of the IntSeq expression must yield an integer.");

							value_stack.Push(new ExpressoIntegerSequence((int)start, (int)end, (int)step));
							next_step = 0;
							break;

						default:
							throw ExpressoOps.RuntimeError("An error occurred while evaluating an IntegerSequence expression.");
						}
						break;
					}

					case NodeType.SwitchStatement:
					{
						var switch_stmt = (SwitchStatement)node;
						switch(next_step){
						case 0:
							node = switch_stmt.Target;
							node_stack.Push(switch_stmt);
							++next_step;
							goto MAIN_LOOP;

						case 1:
							if(next_child < switch_stmt.Cases.Count){
								if(next_child != 0){
									object top = value_stack.Pop();
									if(top is bool && (bool)top)
										goto CLEANUP_SWITCH;
								}
								node = switch_stmt.Cases[next_child++];
								goto MAIN_LOOP;
							}else{
								next_step = 0;
								next_child = 0;
								node_stack.Pop();
							}
						CLEANUP_SWITCH:
							value_stack.Pop();		//caseラベルと照合する値をスタックから外す
							break;

						default:
							throw ExpressoOps.SystemError("Unknown path reached while evaluating a switch statement!");
						}
						break;
					}

					case NodeType.ThrowStatement:
					{
						var throw_stmt = (ThrowStatement)node;
						if(node_stack.Peek() == throw_stmt){
							node_stack.Pop();
						}else{
							node = throw_stmt.Expression;
							node_stack.Push(throw_stmt);
							goto MAIN_LOOP;
						}
						break;
					}

					case NodeType.TryStatement:
					{
						var try_stmt = (TryStatement)node;
						if(node_stack.Peek() == try_stmt){	//ノードスタックのトップがこのtry文だった場合、try文の本体の実行は終わっている
							var top = value_stack.Top();
							if(top is ExpressoThrowException){
								if(try_stmt.Catches.Count > 0){
									node = try_stmt.Catches[next_child++];
									goto case NodeType.CatchClause;
								}else{
									next_child = 0;
									while(node_stack.Count > 0){	//一つ上のtry文にたどり着くまでノードスタックを巻き戻す
										node_stack.Pop();
										if(node_stack.Peek().Type == NodeType.TryStatement)
											break;	//NOTE: 次のループに入る前に親のノードを参照するので、ここでは何もしない
									}

									if(node_stack.Count == 0)	//このtry文を内包するtry文が見つからなかったので、C#側の例外ハンドラに処理を投げる
										throw (ExpressoThrowException)top;
								}
							}

							if(try_stmt.FinallyClause != null){
								node = try_stmt.FinallyClause;
								if(node_stack.Peek() == try_stmt)	//finally節を実行したらこのtry文には用はなくなる
									node_stack.Pop();

								goto case NodeType.FinallyClause;
							}

							node_stack.Pop();		//ここに到達するのはfinally節がなく、try文の本体で例外が発生しなかった場合だけ
						}else{
							node = try_stmt.Body;
							node_stack.Push(try_stmt);
							goto case NodeType.Block;
						}
						break;
					}

					case NodeType.TypeDecl:
						break;

					case NodeType.UnaryExpression:
					{
						var unary_op = (UnaryExpression)node;
						if(node_stack.Peek() == unary_op){
							var ope = value_stack.Pop();
							if(ope == null)
								throw ExpressoOps.InvalidTypeError("Invalid object type!");
							
							value_stack.Push(EvalUnaryOperation(unary_op.Operator, ope));
							node_stack.Pop();
						}else{
							node = unary_op.Operand;
							node_stack.Push(unary_op);
							goto MAIN_LOOP;
						}
						break;
					}

					case NodeType.VarDecl:
						break;

					case NodeType.WhileStatement:
					{
						var while_stmt = (WhileStatement)node;
						switch(next_step){
						case 0:
							if(node_stack.Peek() != while_stmt)
								node_stack.Push(while_stmt);

							node = while_stmt.Condition;
							++next_step;
							goto MAIN_LOOP;

						case 1:
							var cond = value_stack.Pop();
							try{
								if(cond != null && (bool)cond){
									node = while_stmt.Body;
									goto MAIN_LOOP;
								}else
									node_stack.Pop();
							}
							catch(Exception){
								if(!(cond is bool))
									throw ExpressoOps.InvalidTypeError("Invalid expression! The condition of a while statement must yield a boolean!");
							}
							finally{
								next_step = 0;
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

					if(node_stack.Count != 0)
						node = node_stack.Peek();
				}
			}
			catch(RuntimeException ex){

			}

			return (value_stack.Size > 0) ? value_stack.Pop() : null;
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

		private static void DoPrint(List<object> values, bool hasTrailing)
		{
			var first = values[0];
			var sb = new StringBuilder();
			if(first is string){
				sb.Append(first);
				values.RemoveAt(0);
				for(int i = 1; i < values.Count; ++i){
					sb.Append("{" + (i - 1) + "}");
					if(i + 1 != values.Count)
						sb.Append(",");
				}
				
				var text = string.Format(sb.ToString(), values.ToArray());
				if(!hasTrailing)
					Console.WriteLine(text);
				else
					Console.Write(text);
			}else{
				sb.Append(first);
				bool print_comma = true;
				for(int i = 1; i < values.Count; ++i){
					if(print_comma)
						sb.Append(",");
					
					sb.Append(values[i]);
					print_comma = (values[i] is string) ? false : true;
				}
				
				var text = sb.ToString();
				if(!hasTrailing)
					Console.WriteLine(text);
				else
					Console.Write(text + ",");
			}
		}
	}
}

