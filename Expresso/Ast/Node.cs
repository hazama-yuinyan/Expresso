using System.Collections.Generic;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;

namespace Expresso.Ast
{
	using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// 抽象構文木のノードタイプ。
	/// The node type of AST.
    /// </summary>
    public enum NodeType
    {
        Block,
        UnaryExpression,
        BinaryExpression,
        ConditionalExpression,
        Constant,
        Identifier,
		Argument,
        Call,
        Assignment,
        Function,
        Return,
		Print,
		Sequence,
		MemRef,
		Comprehension,
		ComprehensionFor,
		ComprehensionIf,
		VarDecl,
		ExprStatement,
		IfStatement,
		WhileStatement,
		ForStatement,
		BreakStatement,
		ContinueStatement,
		AssertStatement,
		TryStatement,
		Initializer,
		SwitchStatement,
		CaseClause,
		TypeDecl,
		ModuleDecl,
		New,
		Require,
		WithStatement,
		CatchClause,
		FinallyClause,
		ThrowStatement,
		YieldStatement
    }

    /// <summary>
    /// 抽象構文木のノードの共通基底。
    /// The base class for all the abstract syntax trees.
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// ノードタイプ。
		/// The node's type.
        /// </summary>
        public abstract NodeType Type { get; }

        internal protected Node() { }

        #region 抽象構文木を実行する。

        /// <summary>
        /// operator== とかをオーバーロードしてるクラスでも、参照に基づいてDictionaryとかを使うために。
        /// </summary>
        class ReferenceEqual<T> : IEqualityComparer<T>
        {
            bool IEqualityComparer<T>.Equals(T x, T y)
            {
                return object.ReferenceEquals(x, y);
            }

            int IEqualityComparer<T>.GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }

        /// <summary>
        /// インタプリターの実行。このノードがあらわす処理を実行する。
        /// Run the code.
        /// </summary>
        /// <param name="varStore">ローカル変数テーブル。</param>
        /// <returns>そのコードを評価した結果の戻り値など。</returns>
        internal abstract object Run(VariableStore varStore);
        #endregion

		/// <summary>
		/// このノードがあらわすコードをC#の式木にコンパイルする。
		/// Compile the content of the node to the corresponding C#'s expression tree.
		/// </summary>
		/// <param name='emitter'>
		/// Emitter.
		/// </param>
		internal abstract CSharpExpr Compile(Emitter<CSharpExpr> emitter);

		static internal ExprStatement MakeExprStmt(List<Expression> exprs)
		{
			return new ExprStatement{Expressions = exprs};
		}

		static internal PrintStatement MakePrintStmt(List<Expression> exprs, bool hasTrailingComma)
		{
			return new PrintStatement{Expressions = exprs, HasTrailing = hasTrailingComma};
		}

		static internal ReturnStatement MakeReturnStmt(List<Expression> exprs)
		{
			return new ReturnStatement{Expressions = exprs};
		}
		
		static internal IfStatement MakeIfStmt(Expression condition, Statement trueBlock, Statement falseBlock)
		{
			return new IfStatement{Condition = condition, TrueBlock = trueBlock, FalseBlock = falseBlock};
		}
		
		static internal WhileStatement MakeWhileStmt()
		{
			return new WhileStatement();
		}
		
		static internal ForStatement MakeForStmt()
		{
			return new ForStatement();
		}
		
		static internal SwitchStatement MakeSwitchStmt(Expression target, List<CaseClause> cases)
		{
			return new SwitchStatement{Target = target, Cases = cases};
		}
		
		static internal CaseClause MakeCaseClause(List<Expression> labels, Statement body)
		{
			return new CaseClause{Labels = labels, Body = body};
		}
		
		static internal Function MakeFunc(string name, List<Argument> parameters, Block body, TypeAnnotation returnType, bool isStatic = false)
		{
			return new Function(name, parameters, body, returnType, isStatic);
		}
		
		static internal Function MakeClosure(string name, List<Argument> parameters, Block body, TypeAnnotation returnType, VariableStore environ)
		{
			return new Function(name, parameters, body, returnType, false, environ);
		}
		
		static internal UnaryExpression MakeUnaryExpr(OperatorType op, Expression operand)
		{
			return new UnaryExpression{Operator = op, Operand = operand};
		}
		
		static internal BinaryExpression MakeBinaryExpr(OperatorType op, Expression lhs, Expression rhs)
		{
			return new BinaryExpression{Operator = op, Left = lhs, Right = rhs};
		}
		
		static internal ObjectInitializer MakeObjInitializer(ObjectTypes type, List<Expression> initializeList)
		{
			return new ObjectInitializer{Initializer = initializeList, ObjType = type};
		}
		
		static internal Assignment MakeAssignment(List<Expression> targets, List<Expression> expressions)
		{
			return new Assignment{Targets = targets, Expressions = expressions};
		}
		
		static internal Assignment MakeAugumentedAssignment(List<Expression> targets, List<Expression> expressions, OperatorType opType)
		{
			var rvalues = new List<Expression>();
			for(int i = 0; i < expressions.Count; ++i){
				var rvalue = new BinaryExpression{Left = targets[i], Right = expressions[i], Operator = opType};
				rvalues.Add(rvalue);
			}
			return new Assignment{Targets = targets, Expressions = rvalues};
		}
		
		static internal Comprehension MakeComp(Expression yieldExpr, ComprehensionFor body, ObjectTypes objType)
		{
			return new Comprehension{YieldExpr = yieldExpr, Body = body, ObjType = objType};
		}
		
		static internal ComprehensionFor MakeCompFor(List<Expression> lValues, Expression target, ComprehensionIter body)
		{
			return new ComprehensionFor{LValues = lValues, Target = target, Body = body};
		}
		
		static internal ComprehensionIf MakeCompIf(Expression condition, ComprehensionIter body)
		{
			return new ComprehensionIf{Condition = condition, Body = body};
		}
		
		static internal Constant MakeConstant(ObjectTypes type, object val)
		{
			return new Constant{ValType = type, Value = val};
		}
		
		static internal IntSeqExpression MakeIntSeq(Expression start, Expression end, Expression step)
		{
			return new IntSeqExpression{Start = start, End = end, Step = step};
		}
		
		static internal TypeDeclaration MakeClassDef(string className, List<string> bases, List<Statement> decls)
		{
			return new TypeDeclaration{TargetType = DeclType.Class, Name = className, Bases = bases, Declarations = decls};
		}

		static internal ModuleDeclaration MakeModuleDef(string moduleName, List<Statement> requires, List<Statement> decls,
		                                                List<bool> exportMap)
		{
			return new ModuleDeclaration{Name = moduleName, Requires = requires, Declarations = decls, ExportMap = exportMap};
		}
		
		static internal NewExpression MakeNewExpr(Expression target, List<Expression> args)
		{
			return new NewExpression{TargetDecl = target, Arguments = args};
		}
		
		static internal RequireExpression MakeRequireExpr(string moduleName, string aliasName)
		{
			return new RequireExpression{ModuleName = moduleName, AliasName = aliasName};
		}
		
		static internal TryStatement MakeTryStmt(Block body, List<CatchClause> catches, Block finallyClause)
		{
			if(finallyClause != null)
			return new TryStatement{Body = body, Catches = catches, FinallyClause = new FinallyClause{Body = finallyClause}};
			else
			return new TryStatement{Body = body, Catches = catches, FinallyClause = null};
		}
		
		static internal ThrowStatement MakeThrowStmt(Expression expr)
		{
			return new ThrowStatement{Expression = expr};
		}
		
		static internal YieldStatement MakeYieldStmt(Expression expr)
		{
			return new YieldStatement{Expression = expr};
		}
    }
}
