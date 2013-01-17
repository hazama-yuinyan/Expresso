using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Compiler;
using Expresso.Compiler.Meta;
using Expresso.Utils;

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
        FunctionDef,
		EmptyStatement,
        Return,
		Print,
		IntSequence,
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
		TypeDef,
		Toplevel,
		New,
		Require,
		WithStatement,
		CatchClause,
		FinallyClause,
		ThrowStatement,
		YieldStatement,
		CastExpression,
		IsExpression,
		Sequence,
		DefaultExpression
    }

    /// <summary>
    /// 抽象構文木のノードの共通基底。
    /// The base class for all the abstract syntax trees.
    /// </summary>
    public abstract class Node
    {
		private SourceLocation _start = SourceLocation.Invalid;
		private SourceLocation _end = SourceLocation.Invalid;

        /// <summary>
        /// ノードタイプ。
		/// The node's type.
        /// </summary>
        public abstract NodeType Type { get; }

        internal protected Node() { }

		public ScopeStatement Parent{
			get; internal set;
		}

		public void SetLoc(SourceLocation start, SourceLocation end)
		{
			_start = start;
			_end = end;
		}
		
		public void SetLoc(SourceSpan span)
		{
			_start = span.Start;
			_end = span.End;
		}

		internal ExpressoAst GlobalParent{
			get {
				Node cur = this;
				while(!(cur is ExpressoAst)){
					Debug.Assert(cur != null);
					cur = cur.Parent;
				}
				return (ExpressoAst)cur;
			}
		}
		
		public SourceLocation Start{
			get { return _start; }
			set { _start = value; }
		}
		
		public SourceLocation End{
			get { return _end; }
			set { _end = value; }
		}
		
		public SourceSpan Span{
			get {
				return new SourceSpan(_start, _end);
			}
		}

        /// <summary>
        /// インタプリターの実行。このノードがあらわす処理を実行する。
        /// Run the code.
        /// </summary>
        /// <param name="varStore">ローカル変数テーブル。</param>
        /// <returns>そのコードを評価した結果の戻り値など。</returns>
        //internal abstract object Run(VariableStore varStore);

		/// <summary>
		/// このノードがあらわすコードをC#の式木にコンパイルする。
		/// Compile the content of the node to the corresponding C#'s expression tree.
		/// </summary>
		/// <param name='emitter'>
		/// Emitter.
		/// </param>
		internal abstract CSharpExpr Compile(Emitter<CSharpExpr> emitter);

		/// <summary>
		/// Accepts an ExpressoWalker class instance.
		/// </summary>
		internal abstract void Walk(ExpressoWalker walker);

		#region AST node factory methods
		static internal BreakStatement MakeBreakStmt(int count, IEnumerable<BreakableStatement> loops)
		{
			return new BreakStatement(count, loops.ToArray());
		}

		static internal ContinueStatement MakeContinueStmt(int count, IEnumerable<BreakableStatement> loops)
		{
			return new ContinueStatement(count, loops.ToArray());
		}

		static internal ExprStatement MakeExprStmt(IEnumerable<Expression> exprs)
		{
			return new ExprStatement(exprs.ToArray());
		}

		static internal PrintStatement MakePrintStmt(SequenceExpression exprs, bool hasTrailingComma)
		{
			return new PrintStatement(exprs != null ? exprs.Items.ToArray() : null, hasTrailingComma);
		}

		static internal ReturnStatement MakeReturnStmt(Expression expr)
		{
			return new ReturnStatement(expr);
		}
		
		static internal IfStatement MakeIfStmt(Expression condition, Statement trueBlock, Statement falseBlock)
		{
			return new IfStatement(condition, trueBlock, falseBlock);
		}
		
		static internal WhileStatement MakeWhileStmt()
		{
			return new WhileStatement();
		}
		
		static internal ForStatement MakeForStmt()
		{
			return new ForStatement();
		}
		
		static internal SwitchStatement MakeSwitchStmt(Expression target, IEnumerable<CaseClause> cases)
		{
			return new SwitchStatement(target, cases.ToArray());
		}

		static internal EmptyStatement MakeEmptyStmt()
		{
			return new EmptyStatement();
		}
		
		static internal CaseClause MakeCaseClause(IEnumerable<Expression> labels, Statement body)
		{
			return new CaseClause(labels.ToArray(), body);
		}
		
		static internal FunctionDefinition MakeFunc(string name, IEnumerable<Argument> parameters, Block body, TypeAnnotation returnType, bool isStatic = false)
		{
			return new FunctionDefinition(name, parameters.ToArray(), body, returnType, isStatic);
		}
		
		static internal FunctionDefinition MakeClosure(string name, IEnumerable<Argument> parameters, Block body, TypeAnnotation returnType, VariableStore environ)
		{
			return new FunctionDefinition(name, parameters.ToArray(), body, returnType, false, environ);
		}

		static internal Call MakeCallExpr(Expression target, IEnumerable<Expression> args)
		{
			return new Call(target, args.ToArray());
		}

		static internal Identifier MakeIdentifier(string name)
		{
			return new Identifier(name, null);
		}

		static internal Identifier MakeLocalVar(string name, TypeAnnotation type)
		{
			return new Identifier(name, new ExpressoReference(name, new ExpressoVariable(name, type, VariableKind.Local)));
		}

		static internal Identifier MakeField(string name, TypeAnnotation type)
		{
			return new Identifier(name, new ExpressoReference(name, new ExpressoVariable(name, type, VariableKind.Field)));
		}

		static internal SequenceExpression MakeSequence(IEnumerable<Expression> items)
		{
			return new SequenceExpression(items.ToArray());
		}
		
		static internal UnaryExpression MakeUnaryExpr(OperatorType op, Expression operand)
		{
			return new UnaryExpression(op, operand);
		}
		
		static internal BinaryExpression MakeBinaryExpr(OperatorType op, Expression lhs, Expression rhs)
		{
			return new BinaryExpression(lhs, rhs, op);
		}
		
		static internal SequenceInitializer MakeSeqInitializer(ObjectTypes type, IEnumerable<Expression> initializeList)
		{
			return new SequenceInitializer(initializeList.ToArray(), type);
		}
		
		static internal Assignment MakeAssignment(IEnumerable<Expression> targets, SequenceExpression rhs)
		{
			return new Assignment(targets.ToArray(), rhs);
		}
		
		static internal Assignment MakeAugumentedAssignment(SequenceExpression targets, SequenceExpression rhs, OperatorType opType)
		{
			var rvalues = new List<Expression>();
			for(int i = 0; i < rhs.Count; ++i){
				var rvalue = new BinaryExpression(targets.Items[i], rhs.Items[i], opType);
				rvalues.Add(rvalue);
			}
			return new Assignment(new Expression[]{targets}, MakeSequence(rvalues));
		}

		static internal CastExpression MakeCastExpr(Expression target, Expression toExpr)
		{
			return new CastExpression(toExpr, target);
		}

		static internal ConditionalExpression MakeCondExpr(Expression test, Expression trueExpr, Expression falseExpr)
		{
			return new ConditionalExpression(test, trueExpr, falseExpr);
		}
		
		static internal Comprehension MakeComp(Expression yieldExpr, ComprehensionFor body, ObjectTypes objType)
		{
			return new Comprehension(yieldExpr, body, objType);
		}
		
		static internal ComprehensionFor MakeCompFor(SequenceExpression left, Expression target, ComprehensionIter body)
		{
			return new ComprehensionFor(left, target, body);
		}
		
		static internal ComprehensionIf MakeCompIf(Expression condition, ComprehensionIter body)
		{
			return new ComprehensionIf(condition, body);
		}
		
		static internal Constant MakeConstant(ObjectTypes type, object val)
		{
			return new Constant(val, type);
		}

		static internal Argument MakeArg(string name, TypeAnnotation type, Expression option = null)
		{
			return new Argument(option, new ExpressoVariable(name, type, VariableKind.Parameter));
		}

		static internal MemberReference MakeMemRef(Expression parent, Expression child)
		{
			return new MemberReference(parent, child);
		}
		
		static internal IntSeqExpression MakeIntSeq(Expression start, Expression end, Expression step)
		{
			return new IntSeqExpression(start, end, step);
		}

		static internal VarDeclaration MakeVarDecl(IEnumerable<Identifier> lhs, IEnumerable<Expression> rhs)
		{
			return new VarDeclaration(lhs.ToArray(), rhs.ToArray());
		}
		
		static internal TypeDefinition MakeClassDef(string className, IEnumerable<string> bases, IEnumerable<Statement> decls)
		{
			return new TypeDefinition(className, decls.ToArray(), Expresso.Ast.DeclType.Class, bases.ToArray());
		}

		static internal ExpressoAst MakeModuleDef(string moduleName, IEnumerable<Statement> decls, IEnumerable<bool> exportMap)
		{
			return new ExpressoAst(decls.ToArray(), moduleName, exportMap.ToArray());
		}
		
		static internal NewExpression MakeNewExpr(Expression target, IEnumerable<Expression> args)
		{
			return new NewExpression(target, args.ToArray());
		}
		
		static internal RequireStatement MakeRequireStmt(IEnumerable<string> moduleNames, IEnumerable<string> aliasNames)
		{
			return (aliasNames.Any((name) => name != null)) ? new RequireStatement(moduleNames.ToArray(), aliasNames.ToArray()) :
				new RequireStatement(moduleNames.ToArray());
		}
		
		static internal TryStatement MakeTryStmt(Block body, IEnumerable<CatchClause> catches, Block finallyClause)
		{
			if(finallyClause != null)
				return new TryStatement(body, catches.ToArray(), new FinallyClause(finallyClause));
			else
				return new TryStatement(body, catches.ToArray(), null);
		}

		static internal CatchClause MakeCatchClause(Block body, Identifier ident)
		{
			return new CatchClause(ident, body);
		}

		static internal WithStatement MakeWithStmt(Expression context, Statement body)
		{
			return new WithStatement(context, body);
		}
		
		static internal ThrowStatement MakeThrowStmt(Expression expr)
		{
			return new ThrowStatement(expr);
		}
		
		static internal YieldStatement MakeYieldStmt(Expression expr)
		{
			return new YieldStatement(expr);
		}
		#endregion
    }
}
