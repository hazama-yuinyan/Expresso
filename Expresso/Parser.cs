using System.Collections.Generic;
using System.Linq;
using Expresso.Ast;
using Expresso.BuiltIns;
using Expresso.Interpreter;
using Expresso.Helpers;





using System;



public class Parser {
	public const int _EOF = 0;
	public const int _double_dots = 1;
	public const int _rbracket = 2;
	public const int _colon = 3;
	public const int _semicolon = 4;
	public const int _lcurly = 5;
	public const int _lparen = 6;
	public const int _lbracket = 7;
	public const int _rparen = 8;
	public const int _rcurly = 9;
	public const int _comma = 10;
	public const int _ident = 11;
	public const int _integer = 12;
	public const int _float = 13;
	public const int _string_literal = 14;
	public const int _keyword_in = 15;
	public const int _keyword_for = 16;
	public const int maxT = 87;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

internal Scope cur_scope = new Scope();		//the current scope of variables
	static private Scope funcs = new Scope();	//the namespace for funcTable
	public Block root = new Block();
	static public Function main_func = null;	//the main function
	static private List<BreakableStatement> breakables = new List<BreakableStatement>();	//the current parent breakbles
	bool ignore_parent_scope = false;
	
	static Parser()
	{			//Add built-in functions
		Function[] native_funcs = {
			new NativeFunctionUnary<double, double>(
				"abs", new Argument{Ident = new Identifier("val", TYPES.FLOAT, 0)}, Math.Abs
			),
			new NativeFunctionUnary<double, double>(
				"sqrt", new Argument{Ident = new Identifier("val", TYPES.FLOAT, 0)}, Math.Sqrt
			),
			new NativeFunctionUnary<int, double>(
				"toInt", new Argument{Ident = new Identifier("val", TYPES.FLOAT, 0)}, (double x) => (int)x
			)
		};
		foreach(var tmp in native_funcs)
			funcs.AddFunction(tmp);
	}
	
	Constant CreateConstant(TYPES type)
	{
		Constant result = null;
		
		switch(type){
		case TYPES.INTEGER:
			result = new Constant{ValType = type, Value = 0};
			break;
			
		case TYPES.BOOL:
			result = new Constant{ValType = type, Value = false};
			break;
			
		case TYPES.FLOAT:
			result = new Constant{ValType = type, Value = 0.0};
			break;
			
		case TYPES.STRING:
			result = new Constant{ValType = type, Value = ""};
			break;
			
		default:
			SemErr("Unknown object type");
			break;
		}
		
		return result;
	}
	
	bool NotFollowedByDoubleDots()
	{
		Token x = la;
		scanner.ResetPeek();
		if(x.kind != _lbracket) return true;
		while(x.kind != 0 && x.kind != _double_dots && x.kind != _rbracket && x.kind != _semicolon && x.kind != _rparen && x.kind != _keyword_in)
			x = scanner.Peek();
		
		return x.kind != _double_dots;
	}
	
	bool IdentIsNotCallable()
	{
		Token x = scanner.Peek();
		return x.kind != _lparen;
	}
	
	bool IsObjectInitializer()
	{
		Token x = la;
		if(x.kind != _lcurly) return true;
		scanner.ResetPeek();
		while(x.kind != 0 && x.kind != _keyword_for && x.kind != _rcurly) x = scanner.Peek();
		return x.kind != _keyword_for;
	}
	
	Identifier DeclareVariable(ref Expression rhs, string name, TYPES type)
	{
		if(rhs == null){
  			if(type == TYPES._INFERENCE) SemErr("Can not infer the type of the variable \"" + name + "\" without initialization!");
  			rhs = CreateConstant(type);
  		}else{
  			if(type == TYPES._INFERENCE){
  				if(rhs is Constant){
  					var constant = (Constant)rhs;
  					type = constant.ValType;
  				}else if(rhs is ObjectInitializer){
  					var initializer = (ObjectInitializer)rhs;
  					type = initializer.ObjType;
  				}else if(rhs is Comprehension){
  					var comprehen = (Comprehension)rhs;
  					type = comprehen.ObjType;
  				}else if(rhs is NewExpression){
  					type = TYPES.CLASS;
  				}else if(rhs is Call){
  					var call = (Call)rhs;
  					if(call.Function != null){
  						type = call.Function.ReturnType;
  					}else{
  						
  					}
  				}else{
  					SemErr("Cannot infer the type of the variable \"" + name + "\" from that type of expression.");
  				}
  			}
  		}
  		
		var variable = new Identifier(name, type);
 		cur_scope.AddLocal(ref variable);
 		return variable;
	}
	
	Identifier DeclareArgument(string name, TYPES type)
	{
		var argument = new Identifier(name, type);
		
		cur_scope.AddLocal(ref argument);
		return argument;
	}
	
	static ExprStatement MakeExprStatement(List<Expression> exprs)
	{
		return new ExprStatement{Expressions = exprs};
	}
	
	static BreakStatement MakeBreakStatement(int count)
	{
		var tmp = new List<BreakableStatement>();
		for(int len = Parser.breakables.Count, i = 0, j = count; j > 0; ++i){
			var enclosing = Parser.breakables[len - 1 - i];
			tmp.Add(enclosing);
			if(enclosing.Type == NodeType.WhileStatement || enclosing.Type == NodeType.ForStatement) --j;
		}
		return new BreakStatement{Count = count, Enclosings = tmp};
	}
	
	static IfStatement MakeIfStatement(Expression condition, Statement trueBlock, Statement falseBlock)
	{
		return new IfStatement{Condition = condition, TrueBlock = trueBlock, FalseBlock = falseBlock};
	}
	
	static WhileStatement MakeWhileStatement()
	{
		return new WhileStatement();
	}
	
	static ForStatement MakeForStatement()
	{
		return new ForStatement();
	}
	
	static SwitchStatement MakeSwitchStatement(Expression target, List<CaseClause> cases)
	{
		return new SwitchStatement{Target = target, Cases = cases};
	}
	
	static CaseClause MakeCaseClause(List<Expression> labels, Statement body)
	{
		return new CaseClause{Labels = labels, Body = body};
	}
	
	static Function MakeFunc(string name, List<Argument> parameters, Block body, TYPES returnType)
	{
		return new Function{Name = name, Parameters = parameters, Body = body, ReturnType = returnType};
	}
	
	static UnaryExpression MakeUnaryExpr(OperatorType op, Expression operand)
	{
		return new UnaryExpression{Operator = op, Operand = operand};
	}
	
	static BinaryExpression MakeBinaryExpr(OperatorType op, Expression lhs, Expression rhs)
	{
		return new BinaryExpression{Operator = op, Left = lhs, Right = rhs};
	}
	
	static ObjectInitializer MakeObjInitializer(TYPES type, List<Expression> initializeList)
	{
		return new ObjectInitializer{Initializer = initializeList, ObjType = type};
	}
	
	static Assignment MakeAssignment(List<Expression> targets, List<Expression> expressions)
	{
		return new Assignment{Targets = targets, Expressions = expressions};
	}
	
	static Assignment MakeAugumentedAssignment(List<Expression> targets, List<Expression> expressions, OperatorType opType)
	{
		var rvalues = new List<Expression>();
		for(int i = 0; i < expressions.Count; ++i){
			var rvalue = new BinaryExpression{Left = targets[i], Right = expressions[i], Operator = opType};
			rvalues.Add(rvalue);
		}
		return new Assignment{Targets = targets, Expressions = rvalues};
	}
	
	static Comprehension MakeComp(Expression yieldExpr, ComprehensionFor body, TYPES objType)
	{
		return new Comprehension{YieldExpr = yieldExpr, Body = body, ObjType = objType};
	}
	
	static ComprehensionFor MakeCompFor(List<Expression> lValues, Expression target, ComprehensionIter body)
	{
		return new ComprehensionFor{LValues = lValues, Target = target, Body = body};
	}
	
	static ComprehensionIf MakeCompIf(Expression condition, ComprehensionIter body)
	{
		return new ComprehensionIf{Condition = condition, Body = body};
	}
	
	static Constant MakeConstant(TYPES type, object val)
	{
		return new Constant{ValType = type, Value = val};
	}
	
	static IntSeqExpression MakeIntSeq(Expression start, Expression end, Expression step)
	{
		return new IntSeqExpression{Start = start, End = end, Step = step};
	}
	
	static ClassDeclaration MakeClassDef(string className, List<string> bases, List<Statement> decls)
	{
		return new ClassDeclaration{Name = className, Bases = bases, Declarations = decls};
	}
	
	static NewExpression MakeNewExpr(string targetName, List<Expression> args)
	{
		return new NewExpression{TargetName = targetName, Arguments = args};
	}
	
/*--------------------------------------------------------------------------*/


	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void Expresso() {
		Statement stmt = null; 
		if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 21) {
			FuncDecl(out stmt);
		} else if (la.kind == 17) {
			ClassDecl(out stmt);
		} else SynErr(88);
		root.Statements.Add(stmt); 
		while (StartOf(2)) {
			if (StartOf(1)) {
				ExprStmt(out stmt);
			} else if (la.kind == 21) {
				FuncDecl(out stmt);
			} else {
				ClassDecl(out stmt);
			}
			root.Statements.Add(stmt); 
		}
		Parser.main_func = funcs.GetFunction("main"); 
	}

	void ExprStmt(out Statement stmt) {
		List<Expression> targets = null; List<Expression> expr_list;
		stmt = null; bool trailing_comma; OperatorType op_type = OperatorType.NONE;
		
		if (la.kind == 60) {
			VarDecl(out targets);
		} else if (StartOf(3)) {
			RValueList(out targets);
			if (StartOf(4)) {
				AugAssignOpe(ref op_type);
				RValueList(out expr_list);
				stmt = MakeAugumentedAssignment(targets, expr_list, op_type); 
			} else if (la.kind == 4 || la.kind == 23) {
				while (la.kind == 23) {
					Get();
					RValueList(out expr_list);
					stmt = MakeAssignment(targets, expr_list); 
				}
			} else SynErr(89);
		} else SynErr(90);
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(91); Get();}
		Expect(4);
		if(stmt == null) stmt = MakeExprStatement(targets); 
	}

	void FuncDecl(out Statement func) {
		string name; TYPES type = TYPES._INFERENCE; Statement block; var @params = new List<Argument>(); 
		while (!(la.kind == 0 || la.kind == 21)) {SynErr(92); Get();}
		Expect(21);
		block = null; cur_scope = new Scope{Parent = cur_scope}; 
		Expect(11);
		name = t.val; 
		Expect(6);
		if (la.kind == 11) {
			ParamList(ref @params);
		}
		Expect(8);
		if (la.kind == 22) {
			Get();
			Type(out type);
		}
		Block(out block);
		func = MakeFunc(name, @params, (Block)block, type);
		funcs.AddFunction(func as Function);
		cur_scope = cur_scope.Parent;
		
	}

	void ClassDecl(out Statement stmt) {
		Expression expr = null; var stmts = new List<Statement>(); List<Expression> decls = null;
		string name; var base_names = new List<string>(); Statement tmp = null;
		
		while (!(la.kind == 0 || la.kind == 17)) {SynErr(93); Get();}
		Expect(17);
		cur_scope = new Scope{Parent = cur_scope}; 
		Expect(11);
		name = t.val; 
		if (la.kind == 3) {
			Get();
			Expect(11);
			base_names.Add(t.val); 
			while (la.kind == 10) {
				Get();
				Expect(11);
				base_names.Add(t.val); 
			}
		}
		Expect(5);
		while (StartOf(5)) {
			if (la.kind == 18 || la.kind == 19) {
				if (la.kind == 18) {
					Get();
					expr = MakeConstant(TYPES._LABEL_PUBLIC, null); 
				} else {
					Get();
					expr = MakeConstant(TYPES._LABEL_PRIVATE, null); 
				}
				Expect(3);
				tmp = MakeExprStatement(new List<Expression>{expr});
				stmts.Add(tmp);
				
			} else if (la.kind == 20) {
				ConstructorDecl(out tmp);
				stmts.Add(tmp); 
			} else if (la.kind == 21) {
				MethodDecl(out tmp);
				stmts.Add(tmp); 
			} else {
				FieldDecl(out decls);
				Expect(4);
				tmp = MakeExprStatement(decls);
				stmts.Add(tmp);
				
			}
		}
		while (!(la.kind == 0 || la.kind == 9)) {SynErr(94); Get();}
		Expect(9);
		stmt = MakeClassDef(name, base_names, stmts);
		cur_scope = cur_scope.Parent;
		
	}

	void ConstructorDecl(out Statement func) {
		Statement block = null; var @params = new List<Argument>();
		Identifier ident_this; Argument exs_this = null;
		
		while (!(la.kind == 0 || la.kind == 20)) {SynErr(95); Get();}
		Expect(20);
		cur_scope = new Scope{Parent = cur_scope};
		ident_this = DeclareArgument("this", TYPES.CLASS);
		exs_this = new Argument{
		Ident = ident_this,
		Option = null
		};
		
		Expect(6);
		if (la.kind == 11) {
			ParamList(ref @params);
		}
		@params.Insert(0, exs_this); 
		Expect(8);
		Block(out block);
		func = MakeFunc("constructor", @params, (Block)block, TYPES.UNDEF);
		cur_scope = cur_scope.Parent;
		
	}

	void MethodDecl(out Statement func) {
		string name; TYPES type = TYPES._INFERENCE; Statement block = null; var @params = new List<Argument>();
		Identifier ident_this; Argument exs_this = null;
		
		while (!(la.kind == 0 || la.kind == 21)) {SynErr(96); Get();}
		Expect(21);
		cur_scope = new Scope{Parent = cur_scope};
		ident_this = DeclareArgument("this", TYPES.CLASS);
		exs_this = new Argument{
		Ident = ident_this,
		Option = null
		};
		
		Expect(11);
		name = t.val; 
		Expect(6);
		if (la.kind == 11) {
			ParamList(ref @params);
		}
		@params.Insert(0, exs_this); 
		Expect(8);
		if (la.kind == 22) {
			Get();
			Type(out type);
		}
		Block(out block);
		func = MakeFunc(name, @params, (Block)block, type);
		cur_scope = cur_scope.Parent;
		
	}

	void FieldDecl(out List<Expression> outs ) {
		string name; TYPES type = TYPES._INFERENCE; Expression rhs = null; Identifier variable;
		outs = new List<Expression>();
		var vars = new List<Identifier>(); var exprs = new List<Expression>();
		
		Variable(out name);
		if (la.kind == 22) {
			Get();
			Type(out type);
		}
		if (la.kind == 23) {
			Get();
			if (NotFollowedByDoubleDots()) {
				CondExpr(out rhs);
			} else if (la.kind == 7) {
				IntSeqExpr(out rhs);
			} else SynErr(97);
		}
		while (la.kind == 10) {
			Get();
			variable = DeclareVariable(ref rhs, name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			
			Variable(out name);
			if (la.kind == 22) {
				Get();
				Type(out type);
			}
			if (la.kind == 23) {
				Get();
				if (NotFollowedByDoubleDots()) {
					CondExpr(out rhs);
				} else if (la.kind == 7) {
					IntSeqExpr(out rhs);
				} else SynErr(98);
			}
		}
		variable = DeclareVariable(ref rhs, name, type);
		  			vars.Add(variable);
		  			exprs.Add(rhs);
		
		  			outs.Add(new VarDeclaration{
					Variables = vars,
					Expressions = exprs
		  			});
		
	}

	void ParamList(ref List<Argument> @params ) {
		Argument expr; 
		Argument(out expr);
		@params.Add(expr); 
		while (la.kind == 10) {
			Get();
			Argument(out expr);
			@params.Add(expr); 
		}
	}

	void Block(out Statement block) {
		Block tmp; Statement stmt; 
		Expect(5);
		tmp = new Block();
		Parser.breakables.Add(tmp);
		
		Stmt(out stmt);
		tmp.Statements.Add(stmt); 
		while (StartOf(6)) {
			Stmt(out stmt);
			tmp.Statements.Add(stmt); 
		}
		Expect(9);
		block = tmp;
		Parser.breakables.RemoveLast();
		
	}

	void Type(out TYPES type) {
		type = TYPES.UNDEF; 
		switch (la.kind) {
		case 24: {
			Get();
			type = TYPES.INTEGER; 
			break;
		}
		case 25: {
			Get();
			type = TYPES.BOOL; 
			break;
		}
		case 26: {
			Get();
			type = TYPES.FLOAT; 
			break;
		}
		case 27: {
			Get();
			type = TYPES.RATIONAL; 
			break;
		}
		case 28: {
			Get();
			type = TYPES.BIGINT; 
			break;
		}
		case 29: {
			Get();
			type = TYPES.STRING; 
			break;
		}
		case 30: {
			Get();
			type = TYPES.BYTEARRAY; 
			break;
		}
		case 31: {
			Get();
			type = TYPES.VAR; 
			break;
		}
		case 32: {
			Get();
			type = TYPES.TUPLE; 
			break;
		}
		case 33: {
			Get();
			type = TYPES.LIST; 
			break;
		}
		case 34: {
			Get();
			type = TYPES.DICT; 
			break;
		}
		case 35: {
			Get();
			type = TYPES.EXPRESSION; 
			break;
		}
		case 36: {
			Get();
			type = TYPES.FUNCTION; 
			break;
		}
		case 37: {
			Get();
			type = TYPES.SEQ; 
			break;
		}
		case 38: {
			Get();
			type = TYPES.UNDEF; 
			break;
		}
		default: SynErr(99); break;
		}
	}

	void Variable(out string name) {
		Expect(11);
		name = t.val; 
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; 
		OrTest(out expr);
		if (la.kind == 61) {
			Get();
			OrTest(out true_expr);
			Expect(3);
			CondExpr(out false_expr);
			expr = new ConditionalExpression{
			Condition = expr,
			TrueExpression = true_expr,
			FalseExpression = false_expr
			};
			
		}
	}

	void IntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null; 
		Expect(7);
		if (StartOf(3)) {
			OrTest(out start);
		}
		Expect(1);
		OrTest(out end);
		if (la.kind == 3) {
			Get();
			OrTest(out step);
		}
		Expect(2);
		if(start == null) start = CreateConstant(TYPES.INTEGER);
		if(step == null) step = MakeConstant(TYPES.INTEGER, 1);
		expr = MakeIntSeq(start, end, step);
		
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(7)) {
			SimpleStmt(out stmt);
		} else if (StartOf(8)) {
			CompoundStmt(out stmt);
		} else SynErr(100);
	}

	void SimpleStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == _lcurly) {
			Block(out stmt);
		} else if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 39) {
			PrintStmt(out stmt);
		} else if (la.kind == 40) {
			ReturnStmt(out stmt);
		} else if (la.kind == 41) {
			BreakStmt(out stmt);
		} else if (la.kind == 43) {
			ContinueStmt(out stmt);
		} else SynErr(101);
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == 54) {
			while (!(la.kind == 0 || la.kind == 54)) {SynErr(102); Get();}
			IfStmt(out stmt);
		} else if (la.kind == 56) {
			WhileStmt(out stmt);
		} else if (la.kind == 16) {
			ForStmt(out stmt);
		} else if (la.kind == 57) {
			SwitchStmt(out stmt);
		} else if (la.kind == 21) {
			FuncDecl(out stmt);
		} else SynErr(103);
	}

	void PrintStmt(out Statement stmt) {
		List<Expression> exprs = null; bool trailing_comma = false; 
		Expect(39);
		if (StartOf(3)) {
			RValueList(out exprs);
		}
		if (la.kind == 10) {
			Get();
		}
		trailing_comma = true; 
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(104); Get();}
		Expect(4);
		stmt = new PrintStatement{Expressions = exprs, HasTrailing = trailing_comma}; 
	}

	void ReturnStmt(out Statement stmt) {
		List<Expression> target_list = new List<Expression>(); /*bool trailing_comma;*/ 
		Expect(40);
		if (StartOf(3)) {
			RValueList(out target_list);
		}
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(105); Get();}
		Expect(4);
		stmt = new Return{Expressions = target_list}; 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; 
		Expect(41);
		if (la.kind == 42) {
			Get();
			Expect(12);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(106); Get();}
		Expect(4);
		stmt = MakeBreakStatement(count); 
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; 
		Expect(43);
		if (la.kind == 42) {
			Get();
			Expect(12);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(107); Get();}
		Expect(4);
		stmt = new ContinueStatement{Count = count}; 
	}

	void RValueList(out List<Expression> exprs ) {
		Expression tmp; 
		exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (la.kind == 10) {
			Get();
			CondExpr(out tmp);
			exprs.Add(tmp);	
		}
	}

	void AugAssignOpe(ref OperatorType type) {
		switch (la.kind) {
		case 44: {
			Get();
			type = OperatorType.PLUS; 
			break;
		}
		case 45: {
			Get();
			type = OperatorType.MINUS; 
			break;
		}
		case 46: {
			Get();
			type = OperatorType.TIMES; 
			break;
		}
		case 47: {
			Get();
			type = OperatorType.DIV; 
			break;
		}
		case 48: {
			Get();
			type = OperatorType.POWER; 
			break;
		}
		case 49: {
			Get();
			type = OperatorType.MOD; 
			break;
		}
		case 50: {
			Get();
			type = OperatorType.BIT_AND; 
			break;
		}
		case 51: {
			Get();
			type = OperatorType.BIT_OR; 
			break;
		}
		case 52: {
			Get();
			type = OperatorType.BIT_LSHIFT; 
			break;
		}
		case 53: {
			Get();
			type = OperatorType.BIT_RSHIFT; 
			break;
		}
		default: SynErr(108); break;
		}
	}

	void VarDecl(out List<Expression> outs ) {
		string name; TYPES type = TYPES._INFERENCE; Expression rhs = null; Identifier variable;
		outs = new List<Expression>();
				var vars = new List<Identifier>(); var exprs = new List<Expression>();
		
		Expect(60);
		Variable(out name);
		if (la.kind == 22) {
			Get();
			Type(out type);
		}
		if (la.kind == 23) {
			Get();
			if (NotFollowedByDoubleDots()) {
				CondExpr(out rhs);
			} else if (la.kind == 7) {
				IntSeqExpr(out rhs);
			} else SynErr(109);
		}
		while (la.kind == 10) {
			Get();
			variable = DeclareVariable(ref rhs, name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			
			Variable(out name);
			if (la.kind == 22) {
				Get();
				Type(out type);
			}
			if (la.kind == 23) {
				Get();
				if (NotFollowedByDoubleDots()) {
					CondExpr(out rhs);
				} else if (la.kind == 7) {
					IntSeqExpr(out rhs);
				} else SynErr(110);
			}
		}
		variable = DeclareVariable(ref rhs, name, type);
		  			vars.Add(variable);
		  			exprs.Add(rhs);
		
		  			outs.Add(new VarDeclaration{
					Variables = vars,
					Expressions = exprs
		  			});
		
	}

	void IfStmt(out Statement stmt) {
		Expression tmp; Statement true_block, false_block = null; 
		Expect(54);
		Expect(6);
		CondExpr(out tmp);
		Expect(8);
		Stmt(out true_block);
		if (la.kind == 55) {
			Get();
			Stmt(out false_block);
		}
		stmt = MakeIfStatement(tmp, true_block, false_block); 
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; Statement body = null; WhileStatement tmp; 
		Expect(56);
		tmp = MakeWhileStatement();
		Parser.breakables.Add(tmp);
		
		Expect(6);
		CondExpr(out cond);
		Expect(8);
		Stmt(out body);
		tmp.Condition = cond;
		tmp.Body = body;
		Parser.breakables.RemoveLast();
		stmt = tmp;
		
	}

	void ForStmt(out Statement stmt) {
		List<Expression> target_list = null; Expression rvalue = null; Statement body;
		ForStatement tmp; bool has_let = false;
		
		Expect(16);
		tmp = MakeForStatement();
		Parser.breakables.Add(tmp);
		
		Expect(6);
		if (la.kind == 60) {
			LValueListWithLet(out target_list);
			if(target_list != null) has_let = true; 
		} else if (StartOf(9)) {
			LValueList(out target_list);
		} else SynErr(111);
		Expect(15);
		if (NotFollowedByDoubleDots()) {
			CondExpr(out rvalue);
		} else if (la.kind == 7) {
			IntSeqExpr(out rvalue);
		} else SynErr(112);
		Expect(8);
		Stmt(out body);
		tmp.LValues = target_list;
		tmp.Target = rvalue;
		tmp.Body = body;
		tmp.HasLet = has_let;
		Parser.breakables.RemoveLast();
		stmt = tmp;
		
	}

	void SwitchStmt(out Statement stmt) {
		Expression target; List<CaseClause> cases; 
		Expect(57);
		Expect(6);
		CondExpr(out target);
		Expect(8);
		Expect(5);
		CaseClauseList(out cases);
		Expect(9);
		stmt = MakeSwitchStatement(target, cases); 
	}

	void LValueListWithLet(out List<Expression> targets ) {
		Expression tmp; Identifier ident = null; 
		targets = null; 
		Expect(60);
		targets = new List<Expression>(); 
		AddOpe(out tmp);
		ident = tmp as Identifier;
		if(ident == null) SemErr("Expected a lvalue!");
		cur_scope.AddLocal(ref ident);
		targets.Add(ident);
		
		while (WeakSeparator(10,9,10) ) {
			AddOpe(out tmp);
			ident = tmp as Identifier;
			if(ident == null) SemErr("Expected a lvalue!");
			cur_scope.AddLocal(ref ident);
			targets.Add(ident);
			
		}
	}

	void LValueList(out List<Expression> targets ) {
		Expression tmp; 
		targets = new List<Expression>(); 
		AddOpe(out tmp);
		targets.Add(tmp); 
		while (WeakSeparator(10,9,10) ) {
			AddOpe(out tmp);
			targets.Add(tmp); 
		}
	}

	void CaseClauseList(out List<CaseClause> clauses ) {
		clauses = new List<CaseClause>(); List<Expression> label_list; Statement inner; 
		CaseLabelList(out label_list);
		Stmt(out inner);
		clauses.Add(MakeCaseClause(label_list, inner)); 
		while (la.kind == 58) {
			CaseLabelList(out label_list);
			Stmt(out inner);
			clauses.Add(MakeCaseClause(label_list, inner)); 
		}
	}

	void CaseLabelList(out List<Expression> label_list ) {
		label_list = new List<Expression>(); Expression tmp; 
		CaseLabel(out tmp);
		label_list.Add(tmp); 
		while (la.kind == 58) {
			CaseLabel(out tmp);
			label_list.Add(tmp); 
		}
	}

	void CaseLabel(out Expression expr) {
		expr = null; 
		Expect(58);
		if (StartOf(11)) {
			Literal(out expr);
		} else if (la.kind == 7) {
			IntSeqExpr(out expr);
		} else if (la.kind == 59) {
			Get();
			expr = MakeConstant(TYPES._CASE_DEFAULT, "default"); 
		} else SynErr(113);
		while (!(la.kind == 0 || la.kind == 3)) {SynErr(114); Get();}
		Expect(3);
	}

	void Literal(out Expression expr) {
		expr = null; 
		if (la.kind == 12) {
			Get();
			expr = MakeConstant(TYPES.INTEGER, Convert.ToInt32(t.val)); 
		} else if (la.kind == 13) {
			Get();
			expr = MakeConstant(TYPES.FLOAT, Convert.ToDouble(t.val)); 
		} else if (la.kind == 14) {
			Get();
			string tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = MakeConstant(TYPES.STRING, tmp);
			
		} else if (la.kind == 84 || la.kind == 85) {
			if (la.kind == 84) {
				Get();
			} else {
				Get();
			}
			expr = MakeConstant(TYPES.BOOL, Convert.ToBoolean(t.val)); 
		} else if (la.kind == 86) {
			Get();
			expr = MakeConstant(TYPES.NULL, null); 
		} else SynErr(115);
	}

	void AddOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		while (la.kind == 76 || la.kind == 77) {
			if (la.kind == 76) {
				Get();
				type = OperatorType.PLUS; 
			} else {
				Get();
				type = OperatorType.MINUS; 
			}
			Term(out rhs);
			expr = MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Argument(out Argument arg) {
		string name; Expression default_val = null; TYPES type = TYPES.VAR; 
		Variable(out name);
		if (la.kind == 22) {
			Get();
			Type(out type);
		}
		if (la.kind == 23) {
			Get();
			Literal(out default_val);
		}
		Identifier param = DeclareArgument(name, type);
		 	arg = new Argument{
		Ident = param,
		Option = default_val
		 	};
		
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		while (la.kind == 62) {
			Get();
			AndTest(out rhs);
			expr = MakeBinaryExpr(OperatorType.OR, expr, rhs); 
		}
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		NotTest(out expr);
		while (la.kind == 63) {
			Get();
			NotTest(out rhs);
			expr = MakeBinaryExpr(OperatorType.AND, expr, rhs); 
		}
	}

	void NotTest(out Expression expr) {
		Expression term; expr = null; 
		if (la.kind == 64) {
			Get();
			NotTest(out term);
			expr = MakeUnaryExpr(OperatorType.MINUS, term); 
		} else if (StartOf(9)) {
			Comparison(out expr);
		} else SynErr(116);
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		BitOr(out expr);
		type = OperatorType.EQUAL; 
		if (StartOf(12)) {
			switch (la.kind) {
			case 65: {
				Get();
				type = OperatorType.EQUAL; 
				break;
			}
			case 66: {
				Get();
				type = OperatorType.NOTEQ; 
				break;
			}
			case 67: {
				Get();
				type = OperatorType.LESS; 
				break;
			}
			case 68: {
				Get();
				type = OperatorType.GREAT; 
				break;
			}
			case 69: {
				Get();
				type = OperatorType.LESE; 
				break;
			}
			case 70: {
				Get();
				type = OperatorType.GRTE; 
				break;
			}
			}
			BitOr(out rhs);
			expr = MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void BitOr(out Expression expr) {
		Expression rhs; 
		BitXor(out expr);
		while (la.kind == 71) {
			Get();
			BitXor(out rhs);
			expr = MakeBinaryExpr(OperatorType.BIT_OR, expr, rhs); 
		}
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		while (la.kind == 72) {
			Get();
			BitAnd(out rhs);
			expr = MakeBinaryExpr(OperatorType.BIT_XOR, expr, rhs); 
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOpe(out expr);
		while (la.kind == 73) {
			Get();
			ShiftOpe(out rhs);
			expr = MakeBinaryExpr(OperatorType.BIT_AND, expr, rhs); 
		}
	}

	void ShiftOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOpe(out expr);
		while (la.kind == 74 || la.kind == 75) {
			if (la.kind == 74) {
				Get();
				type = OperatorType.BIT_LSHIFT; 
			} else {
				Get();
				type = OperatorType.BIT_RSHIFT; 
			}
			AddOpe(out rhs);
			expr = MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		Factor(out expr);
		while (la.kind == 78 || la.kind == 79 || la.kind == 80) {
			if (la.kind == 78) {
				Get();
				type = OperatorType.TIMES; 
			} else if (la.kind == 79) {
				Get();
				type = OperatorType.DIV; 
			} else {
				Get();
				type = OperatorType.MOD; 
			}
			Factor(out rhs);
			expr = MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Factor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; 
		if (StartOf(13)) {
			PowerOpe(out expr);
		} else if (la.kind == 76 || la.kind == 77) {
			if (la.kind == 77) {
				Get();
				type = OperatorType.MINUS; 
			} else {
				Get();
				type = OperatorType.PLUS; 
			}
			Factor(out factor);
			expr = MakeUnaryExpr(type, factor); 
		} else SynErr(117);
	}

	void PowerOpe(out Expression expr) {
		Expression rhs; 
		Primary(out expr);
		if (la.kind == 81) {
			Get();
			Factor(out rhs);
			expr = MakeBinaryExpr(OperatorType.POWER, expr, rhs); 
		}
	}

	void Primary(out Expression expr) {
		string name = ""; expr = null; var args = new List<Expression>(); 
		if (StartOf(14)) {
			if (IdentIsNotCallable()) {
				Atom(out expr);
			} else {
				Get();
				name = t.val; 
			}
			while (la.kind == 6 || la.kind == 7 || la.kind == 83) {
				Trailer(ref expr, name);
			}
		} else if (la.kind == 82) {
			Get();
			Expect(11);
			name = t.val; 
			Expect(6);
			if (StartOf(3)) {
				ArgList(ref args);
			}
			Expect(8);
			expr = MakeNewExpr(name, args); 
		} else SynErr(118);
	}

	void Atom(out Expression expr) {
		string name; expr = null; 
		if (la.kind == 11) {
			Variable(out name);
			expr = cur_scope.GetVariable(name, !ignore_parent_scope);
			if(expr == null){
			expr = new Identifier(name);
			if(ignore_parent_scope){	//for the comprehension expression
				var ident = (Identifier)expr;
				cur_scope.AddLocal(ref ident);
				expr = ident;
			}
			}
			
		} else if (StartOf(11)) {
			Literal(out expr);
		} else if (la.kind == 6) {
			Get();
			if (StartOf(3)) {
				SequenceMaker(out expr, TYPES.TUPLE);
			}
			while (!(la.kind == 0 || la.kind == 8)) {SynErr(119); Get();}
			Expect(8);
			if(expr == null)
			expr = MakeObjInitializer(TYPES.TUPLE, new List<Expression>());
			
		} else if (la.kind == 7) {
			Get();
			if (StartOf(3)) {
				SequenceMaker(out expr, TYPES.LIST);
			}
			while (!(la.kind == 0 || la.kind == 2)) {SynErr(120); Get();}
			Expect(2);
			if(expr == null)
			expr = MakeObjInitializer(TYPES.LIST, new List<Expression>());
			
		} else if (la.kind == 5) {
			Get();
			if (StartOf(3)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 9)) {SynErr(121); Get();}
			Expect(9);
			if(expr == null) expr = MakeObjInitializer(TYPES.DICT, new List<Expression>()); 
		} else SynErr(122);
	}

	void Trailer(ref Expression expr, string name) {
		Function func; var args = new List<Expression>(); Expression subscript; 
		if (la.kind == 6) {
			Get();
			if (StartOf(3)) {
				ArgList(ref args);
			}
			Expect(8);
			if(expr != null){
				expr = new Call{
					Function = null,
					Arguments = args,
					Reference = expr
				};
				return;
			}
			func = funcs.GetFunction(name);
			if(func == null){
				SemErr("The function is not defined : " + name);
			}
			expr = new Call{
				Function = func,
			Arguments = args,
			Reference = null
			};
			
		} else if (la.kind == 7) {
			Get();
			Subscript(out subscript);
			Expect(2);
			expr = new MemberReference{
			Parent = expr,
			Subscription = subscript
			};
			
		} else if (la.kind == 83) {
			Get();
			Expect(11);
			subscript = new Identifier(t.val, TYPES._SUBSCRIPT);
			expr = new MemberReference{
			Parent = expr,
			Subscription = subscript
			};
			
		} else SynErr(123);
	}

	void ArgList(ref List<Expression> args ) {
		Expression expr; 
		CondExpr(out expr);
		args.Add(expr); 
		while (la.kind == 10) {
			Get();
			CondExpr(out expr);
			args.Add(expr); 
		}
	}

	void Subscript(out Expression subscript) {
		subscript = null; 
		if (NotFollowedByDoubleDots()) {
			CondExpr(out subscript);
		} else if (la.kind == 7) {
			IntSeqExpr(out subscript);
		} else SynErr(124);
	}

	void SequenceMaker(out Expression expr, TYPES ObjType) {
		Expression tmp = null; List<Expression> list = new List<Expression>();
		expr = null; ComprehensionIter comprehen = null;
		
		if (IsObjectInitializer()) {
			CondExpr(out tmp);
			if(tmp != null) list.Add(tmp); 
			while (la.kind == 10) {
				Get();
				CondExpr(out tmp);
				if(tmp != null) list.Add(tmp); 
			}
			expr = MakeObjInitializer(ObjType, list); 
		} else if (la.kind == 5) {
			Get();
			cur_scope = new Scope{Parent = cur_scope};
			ignore_parent_scope = true;
			
			CondExpr(out tmp);
			CompFor(out comprehen);
			Expect(9);
			expr = MakeComp(tmp, (ComprehensionFor)comprehen, ObjType);
			cur_scope = cur_scope.Parent;
			ignore_parent_scope = false;
			
		} else SynErr(125);
	}

	void DictMaker(out Expression expr) {
		Expression lhs, rhs; List<Expression> list = new List<Expression>(); expr = null; 
		CondExpr(out lhs);
		if(lhs != null) list.Add(lhs); 
		Expect(3);
		CondExpr(out rhs);
		if(rhs != null) list.Add(rhs); 
		while (la.kind == 10) {
			Get();
			CondExpr(out lhs);
			if(lhs != null) list.Add(lhs); 
			Expect(3);
			CondExpr(out rhs);
			if(rhs != null) list.Add(rhs); 
		}
		if(list.Count > 0) expr = MakeObjInitializer(TYPES.DICT, list); 
	}

	void CompFor(out ComprehensionIter expr) {
		Expression rvalue = null; ComprehensionIter body = null; List<Expression> target_list; 
		Expect(16);
		ignore_parent_scope = true; 
		LValueList(out target_list);
		foreach(var target in target_list){
		var ident = target as Identifier;
		if(ident == null) SemErr("Expected a lvalue!");
		if(!cur_scope.Contains(ident.Name))
			cur_scope.AddLocal(ref ident);
		}
		ignore_parent_scope = false;
		
		Expect(15);
		if (NotFollowedByDoubleDots()) {
			CondExpr(out rvalue);
		} else if (la.kind == 7) {
			IntSeqExpr(out rvalue);
		} else SynErr(126);
		if (la.kind == 16 || la.kind == 54) {
			CompIter(out body);
		}
		expr = MakeCompFor(target_list, rvalue, body); 
	}

	void CompIter(out ComprehensionIter expr) {
		expr = null; 
		if (la.kind == 16) {
			CompFor(out expr);
		} else if (la.kind == 54) {
			CompIf(out expr);
		} else SynErr(127);
	}

	void CompIf(out ComprehensionIter expr) {
		Expression tmp; ComprehensionIter body = null; 
		Expect(54);
		OrTest(out tmp);
		if (la.kind == 16 || la.kind == 54) {
			CompIter(out body);
		}
		expr = MakeCompIf(tmp, body); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Expresso();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,T,T, T,x,x,x, T,T,x,x, x,x,x,x, x,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,x,T, x,x,x,x, x,x,x,x, x,x,T,x, T,T,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "double_dots expected"; break;
			case 2: s = "rbracket expected"; break;
			case 3: s = "colon expected"; break;
			case 4: s = "semicolon expected"; break;
			case 5: s = "lcurly expected"; break;
			case 6: s = "lparen expected"; break;
			case 7: s = "lbracket expected"; break;
			case 8: s = "rparen expected"; break;
			case 9: s = "rcurly expected"; break;
			case 10: s = "comma expected"; break;
			case 11: s = "ident expected"; break;
			case 12: s = "integer expected"; break;
			case 13: s = "float expected"; break;
			case 14: s = "string_literal expected"; break;
			case 15: s = "keyword_in expected"; break;
			case 16: s = "keyword_for expected"; break;
			case 17: s = "\"class\" expected"; break;
			case 18: s = "\"public\" expected"; break;
			case 19: s = "\"private\" expected"; break;
			case 20: s = "\"constructor\" expected"; break;
			case 21: s = "\"def\" expected"; break;
			case 22: s = "\"(-\" expected"; break;
			case 23: s = "\"=\" expected"; break;
			case 24: s = "\"int\" expected"; break;
			case 25: s = "\"bool\" expected"; break;
			case 26: s = "\"float\" expected"; break;
			case 27: s = "\"rational\" expected"; break;
			case 28: s = "\"bigint\" expected"; break;
			case 29: s = "\"string\" expected"; break;
			case 30: s = "\"bytearray\" expected"; break;
			case 31: s = "\"var\" expected"; break;
			case 32: s = "\"tuple\" expected"; break;
			case 33: s = "\"list\" expected"; break;
			case 34: s = "\"dictionary\" expected"; break;
			case 35: s = "\"expression\" expected"; break;
			case 36: s = "\"function\" expected"; break;
			case 37: s = "\"intseq\" expected"; break;
			case 38: s = "\"void\" expected"; break;
			case 39: s = "\"print\" expected"; break;
			case 40: s = "\"return\" expected"; break;
			case 41: s = "\"break\" expected"; break;
			case 42: s = "\"upto\" expected"; break;
			case 43: s = "\"continue\" expected"; break;
			case 44: s = "\"+=\" expected"; break;
			case 45: s = "\"-=\" expected"; break;
			case 46: s = "\"*=\" expected"; break;
			case 47: s = "\"/=\" expected"; break;
			case 48: s = "\"**=\" expected"; break;
			case 49: s = "\"%=\" expected"; break;
			case 50: s = "\"&=\" expected"; break;
			case 51: s = "\"|=\" expected"; break;
			case 52: s = "\"<<=\" expected"; break;
			case 53: s = "\">>=\" expected"; break;
			case 54: s = "\"if\" expected"; break;
			case 55: s = "\"else\" expected"; break;
			case 56: s = "\"while\" expected"; break;
			case 57: s = "\"switch\" expected"; break;
			case 58: s = "\"case\" expected"; break;
			case 59: s = "\"default\" expected"; break;
			case 60: s = "\"let\" expected"; break;
			case 61: s = "\"?\" expected"; break;
			case 62: s = "\"or\" expected"; break;
			case 63: s = "\"and\" expected"; break;
			case 64: s = "\"not\" expected"; break;
			case 65: s = "\"==\" expected"; break;
			case 66: s = "\"!=\" expected"; break;
			case 67: s = "\"<\" expected"; break;
			case 68: s = "\">\" expected"; break;
			case 69: s = "\"<=\" expected"; break;
			case 70: s = "\">=\" expected"; break;
			case 71: s = "\"|\" expected"; break;
			case 72: s = "\"^\" expected"; break;
			case 73: s = "\"&\" expected"; break;
			case 74: s = "\"<<\" expected"; break;
			case 75: s = "\">>\" expected"; break;
			case 76: s = "\"+\" expected"; break;
			case 77: s = "\"-\" expected"; break;
			case 78: s = "\"*\" expected"; break;
			case 79: s = "\"/\" expected"; break;
			case 80: s = "\"%\" expected"; break;
			case 81: s = "\"**\" expected"; break;
			case 82: s = "\"new\" expected"; break;
			case 83: s = "\".\" expected"; break;
			case 84: s = "\"true\" expected"; break;
			case 85: s = "\"false\" expected"; break;
			case 86: s = "\"null\" expected"; break;
			case 87: s = "??? expected"; break;
			case 88: s = "invalid Expresso"; break;
			case 89: s = "invalid ExprStmt"; break;
			case 90: s = "invalid ExprStmt"; break;
			case 91: s = "this symbol not expected in ExprStmt"; break;
			case 92: s = "this symbol not expected in FuncDecl"; break;
			case 93: s = "this symbol not expected in ClassDecl"; break;
			case 94: s = "this symbol not expected in ClassDecl"; break;
			case 95: s = "this symbol not expected in ConstructorDecl"; break;
			case 96: s = "this symbol not expected in MethodDecl"; break;
			case 97: s = "invalid FieldDecl"; break;
			case 98: s = "invalid FieldDecl"; break;
			case 99: s = "invalid Type"; break;
			case 100: s = "invalid Stmt"; break;
			case 101: s = "invalid SimpleStmt"; break;
			case 102: s = "this symbol not expected in CompoundStmt"; break;
			case 103: s = "invalid CompoundStmt"; break;
			case 104: s = "this symbol not expected in PrintStmt"; break;
			case 105: s = "this symbol not expected in ReturnStmt"; break;
			case 106: s = "this symbol not expected in BreakStmt"; break;
			case 107: s = "this symbol not expected in ContinueStmt"; break;
			case 108: s = "invalid AugAssignOpe"; break;
			case 109: s = "invalid VarDecl"; break;
			case 110: s = "invalid VarDecl"; break;
			case 111: s = "invalid ForStmt"; break;
			case 112: s = "invalid ForStmt"; break;
			case 113: s = "invalid CaseLabel"; break;
			case 114: s = "this symbol not expected in CaseLabel"; break;
			case 115: s = "invalid Literal"; break;
			case 116: s = "invalid NotTest"; break;
			case 117: s = "invalid Factor"; break;
			case 118: s = "invalid Primary"; break;
			case 119: s = "this symbol not expected in Atom"; break;
			case 120: s = "this symbol not expected in Atom"; break;
			case 121: s = "this symbol not expected in Atom"; break;
			case 122: s = "invalid Atom"; break;
			case 123: s = "invalid Trailer"; break;
			case 124: s = "invalid Subscript"; break;
			case 125: s = "invalid SequenceMaker"; break;
			case 126: s = "invalid CompFor"; break;
			case 127: s = "invalid CompIter"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
