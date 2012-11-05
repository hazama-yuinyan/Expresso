using System.Collections.Generic;
using Expresso.Ast;
using Expresso.BuiltIns;
using Expresso.Interpreter;
using Expresso.Helpers;





using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _integer = 2;
	public const int _float = 3;
	public const int _string_literal = 4;
	public const int _double_dots = 5;
	public const int _colon = 6;
	public const int _semicolon = 7;
	public const int _lcurly = 8;
	public const int _lparen = 9;
	public const int maxT = 71;

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
	
	static Parser()
	{			//Add built-in functions
		Function[] native_funcs = {
			new NativeFunctionUnary<double, double>(
				"abs", new Argument{Name = "val", ParamType = TYPES.FLOAT}, Math.Abs
			),
			new NativeFunctionUnary<double, double>(
				"sqrt", new Argument{Name = "val", ParamType = TYPES.FLOAT}, Math.Sqrt
			),
			new NativeFunctionUnary<int, double>(
				"toInt", new Argument{Name = "val", ParamType = TYPES.FLOAT}, (double x) => (int)x
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
			result = new Constant{ValType = type, Value = new ExpressoPrimitive{Value = 0}};
			break;
			
		case TYPES.BOOL:
			result = new Constant{ValType = type, Value = new ExpressoPrimitive{Value = false}};
			break;
			
		case TYPES.FLOAT:
			result = new Constant{ValType = type, Value = new ExpressoPrimitive{Value = 0.0}};
			break;
			
		case TYPES.STRING:
			result = new Constant{ValType = type, Value = new ExpressoPrimitive{Value = ""}};
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
		while(x.kind == _integer) x = scanner.Peek();
		return x.kind != _double_dots;
	}
	
	bool NotLookAtCurly()
	{
		return t.kind != _lcurly;
	}
	
	bool IdentIsNotCallable()
	{
		Token x = scanner.Peek();
		return x.kind != _lparen;
	}
	
	Parameter DeclareVariable(ref Expression rhs, string name, TYPES type)
	{
		var variable = new Parameter{
  			Name = name,
  			ParamType = type
  		};
  		if(rhs == null){
  			if(type == TYPES.VAR) SemErr("Can not declare a variable of \"var\" without initialization!");
  			rhs = CreateConstant(type);
  		}
  							
 		cur_scope.AddLocal(variable);
 		return variable;
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
		return new ObjectInitializer{InitializeList = initializeList, ObjType = type};
	}
	
	static Assignment MakeAssignment(List<Expression> targets, List<Expression> expressions)
	{
		return new Assignment{Targets = targets, Expressions = expressions};
	}
	
	static Iteration MakeIteration(List<Expression> targets, List<Expression> exprs)
	{
		return new Iteration{Targets = targets, Expressions = exprs};
	}
	
	static Comprehension MakeComp(Expression body, Expression child)
	{
		return new Comprehension{Body = body, Child = child};
	}
	
	static ComprehensionFor MakeCompFor(Expression iteration, Expression body)
	{
		return new ComprehensionFor{Iteration = iteration, Body = body};
	}
	
	static ComprehensionIf MakeCompIf(Expression condition, Expression body)
	{
		return new ComprehensionIf{Condition = condition, Body = body};
	}
	
	static Constant MakeConstant(TYPES type, object val)
	{
		return new Constant{ValType = type, Value = val};
	}
	
	static IntSeqExpression MakeIntSeq(int start, int end, int step)
	{
		return new IntSeqExpression{Start = start, End = end, Step = step};
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
		} else if (la.kind == 43) {
			FuncDecl(out stmt);
		} else SynErr(72);
		root.Statements.Add(stmt); 
		while (StartOf(2)) {
			if (StartOf(1)) {
				ExprStmt(out stmt);
				root.Statements.Add(stmt); 
			} else {
				FuncDecl(out stmt);
				root.Statements.Add(stmt); 
			}
		}
		Parser.main_func = funcs.GetFunction("main"); 
	}

	void ExprStmt(out Statement stmt) {
		List<Expression> targets = null; List<Expression> expr_list; stmt = null; 
		if (la.kind == 36) {
			VarDecl(out targets);
		} else if (StartOf(3)) {
			RValueList(out targets);
			while (la.kind == 29) {
				Get();
				RValueList(out expr_list);
				stmt = MakeAssignment(targets, expr_list); 
			}
		} else SynErr(73);
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(74); Get();}
		Expect(7);
		if(stmt == null) stmt = MakeExprStatement(targets); 
	}

	void FuncDecl(out Statement func) {
		string name; TYPES type; Statement block; List<Argument>@params = new List<Argument>(); 
		while (!(la.kind == 0 || la.kind == 43)) {SynErr(75); Get();}
		Expect(43);
		type = TYPES.VAR; block = null; cur_scope = new Scope{Parent = cur_scope}; 
		Expect(1);
		name = t.val; 
		Expect(9);
		if (la.kind == 1) {
			ParamList(ref @params);
		}
		Expect(32);
		if (la.kind == 42) {
			Get();
			Type(out type);
		}
		Block(out block, false);
		func = MakeFunc(name, @params, (Block)block, type);
		funcs.AddFunction(func as Function);
		
	}

	void Type(out TYPES type) {
		type = TYPES.UNDEF; 
		switch (la.kind) {
		case 10: {
			Get();
			type = TYPES.INTEGER; 
			break;
		}
		case 11: {
			Get();
			type = TYPES.BOOL; 
			break;
		}
		case 12: {
			Get();
			type = TYPES.FLOAT; 
			break;
		}
		case 13: {
			Get();
			type = TYPES.RATIONAL; 
			break;
		}
		case 14: {
			Get();
			type = TYPES.BIGINT; 
			break;
		}
		case 15: {
			Get();
			type = TYPES.STRING; 
			break;
		}
		case 16: {
			Get();
			type = TYPES.BYTEARRAY; 
			break;
		}
		case 17: {
			Get();
			type = TYPES.VAR; 
			break;
		}
		case 18: {
			Get();
			type = TYPES.TUPLE; 
			break;
		}
		case 19: {
			Get();
			type = TYPES.LIST; 
			break;
		}
		case 20: {
			Get();
			type = TYPES.DICT; 
			break;
		}
		case 21: {
			Get();
			type = TYPES.EXPRESSION; 
			break;
		}
		case 22: {
			Get();
			type = TYPES.FUNCTION; 
			break;
		}
		case 23: {
			Get();
			type = TYPES.SEQ; 
			break;
		}
		default: SynErr(76); break;
		}
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(4)) {
			SimpleStmt(out stmt);
		} else if (StartOf(5)) {
			CompoundStmt(out stmt);
		} else SynErr(77);
	}

	void SimpleStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == 8) {
			Block(out stmt);
		} else if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 24) {
			PrintStmt(out stmt);
		} else if (la.kind == 25) {
			ReturnStmt(out stmt);
		} else if (la.kind == 26) {
			BreakStmt(out stmt);
		} else if (la.kind == 28) {
			ContinueStmt(out stmt);
		} else SynErr(78);
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == 31) {
			while (!(la.kind == 0 || la.kind == 31)) {SynErr(79); Get();}
			IfStmt(out stmt);
		} else if (la.kind == 34) {
			WhileStmt(out stmt);
		} else if (la.kind == 35) {
			ForStmt(out stmt);
		} else if (la.kind == 38) {
			SwitchStmt(out stmt);
		} else if (la.kind == 43) {
			FuncDecl(out stmt);
		} else SynErr(80);
	}

	void Block(out Statement block, bool needsScope = true) {
		Block tmp;
		if(needsScope) cur_scope = new Scope{Parent = cur_scope}; Statement stmt;
		
		Expect(8);
		tmp = new Block();
		Parser.breakables.Add(tmp);
		
		Stmt(out stmt);
		tmp.Statements.Add(stmt); 
		while (StartOf(6)) {
			Stmt(out stmt);
			tmp.Statements.Add(stmt); 
		}
		Expect(30);
		cur_scope = cur_scope.Parent; block = tmp;
		Parser.breakables.RemoveLast();
		
	}

	void PrintStmt(out Statement stmt) {
		List<Expression> exprs; 
		Expect(24);
		RValueList(out exprs);
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(81); Get();}
		Expect(7);
		stmt = new PrintStatement{Expressions = exprs}; 
	}

	void ReturnStmt(out Statement stmt) {
		List<Expression> target_list = new List<Expression>(); 
		Expect(25);
		if (StartOf(3)) {
			RValueList(out target_list);
		}
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(82); Get();}
		Expect(7);
		stmt = new Return{Expressions = target_list}; 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; 
		Expect(26);
		if (la.kind == 27) {
			Get();
			Expect(2);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(83); Get();}
		Expect(7);
		stmt = MakeBreakStatement(count); 
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; 
		Expect(28);
		if (la.kind == 27) {
			Get();
			Expect(2);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(84); Get();}
		Expect(7);
		stmt = new ContinueStatement{Count = count}; 
	}

	void RValueList(out List<Expression> exprs ) {
		Expression tmp; 
		exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (la.kind == 41) {
			Get();
			CondExpr(out tmp);
			exprs.Add(tmp); 
		}
	}

	void VarDecl(out List<Expression> outs ) {
		string name; TYPES type; Expression rhs = null; Parameter variable; outs = new List<Expression>();
		var vars = new List<Parameter>(); var exprs = new List<Expression>();
		
		Expect(36);
		type = TYPES.VAR; 
		Variable(out name);
		if (la.kind == 42) {
			Get();
			Type(out type);
		}
		if (la.kind == 29) {
			Get();
			CondExpr(out rhs);
		}
		while (la.kind == 41) {
			Get();
			variable = DeclareVariable(ref rhs, name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			
			Variable(out name);
			if (la.kind == 42) {
				Get();
				Type(out type);
			}
			if (la.kind == 29) {
				Get();
				CondExpr(out rhs);
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
		Expect(31);
		Expect(9);
		CondExpr(out tmp);
		Expect(32);
		Stmt(out true_block);
		if (la.kind == 33) {
			Get();
			Stmt(out false_block);
		}
		stmt = MakeIfStatement(tmp, true_block, false_block); 
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; Statement body = null; WhileStatement tmp; 
		Expect(34);
		tmp = MakeWhileStatement();
		Parser.breakables.Add(tmp);
		
		Expect(9);
		CondExpr(out cond);
		Expect(32);
		Stmt(out body);
		tmp.Condition = cond;
		tmp.Body = body;
		Parser.breakables.RemoveLast();
		stmt = tmp;
		
	}

	void ForStmt(out Statement stmt) {
		List<Expression> target_list; Expression rvalue; Statement body; ForStatement tmp; 
		Expect(35);
		tmp = MakeForStatement();
		Parser.breakables.Add(tmp);
		
		Expect(9);
		if (la.kind == 36) {
			Get();
		}
		LValueList(out target_list);
		Expect(37);
		CondExpr(out rvalue);
		Expect(32);
		Stmt(out body);
		tmp.LValues = target_list;
		tmp.Target = rvalue;
		tmp.Body = body;
		Parser.breakables.RemoveLast();
		stmt = tmp;
		
	}

	void SwitchStmt(out Statement stmt) {
		Expression target; List<CaseClause> cases; 
		Expect(38);
		Expect(9);
		CondExpr(out target);
		Expect(32);
		Expect(8);
		CaseClauseList(out cases);
		Expect(30);
		stmt = MakeSwitchStatement(target, cases); 
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; 
		OrTest(out expr);
		if (la.kind == 44) {
			Get();
			OrTest(out true_expr);
			Expect(6);
			CondExpr(out false_expr);
			expr = new ConditionalExpression{
			Condition = expr,
			TrueExpression = true_expr,
			FalseExpression = false_expr
			};
			
		}
	}

	void LValueList(out List<Expression> targets ) {
		Expression tmp; 
		targets = new List<Expression>(); 
		AddOpe(out tmp);
		targets.Add(tmp); 
		while (WeakSeparator(41,7,8) ) {
			AddOpe(out tmp);
			targets.Add(tmp); 
		}
	}

	void CaseClauseList(out List<CaseClause> clauses ) {
		clauses = new List<CaseClause>(); List<Expression> label_list; Statement inner; 
		CaseLabelList(out label_list);
		Stmt(out inner);
		clauses.Add(MakeCaseClause(label_list, inner)); 
		while (la.kind == 39) {
			CaseLabelList(out label_list);
			Stmt(out inner);
			clauses.Add(MakeCaseClause(label_list, inner)); 
		}
	}

	void CaseLabelList(out List<Expression> label_list ) {
		label_list = new List<Expression>(); Expression tmp; 
		CaseLabel(out tmp);
		label_list.Add(tmp); 
		while (la.kind == 39) {
			CaseLabel(out tmp);
			label_list.Add(tmp); 
		}
	}

	void CaseLabel(out Expression expr) {
		expr = null; 
		Expect(39);
		if (StartOf(9)) {
			Literal(out expr);
		} else if (la.kind == 9) {
			Get();
			IntSeqExpr(out expr);
			Expect(32);
		} else if (la.kind == 40) {
			Get();
			expr = MakeConstant(TYPES._CASE_DEFAULT, "default"); 
		} else SynErr(85);
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(86); Get();}
		Expect(6);
	}

	void Literal(out Expression expr) {
		expr = null; 
		if (la.kind == 2) {
			Get();
			expr = MakeConstant(TYPES.INTEGER, Convert.ToInt32(t.val)); 
		} else if (la.kind == 3) {
			Get();
			expr = MakeConstant(TYPES.FLOAT, Convert.ToDouble(t.val)); 
		} else if (la.kind == 4) {
			Get();
			string tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = MakeConstant(TYPES.STRING, tmp);
			
		} else if (la.kind == 68 || la.kind == 69) {
			if (la.kind == 68) {
				Get();
			} else {
				Get();
			}
			expr = MakeConstant(TYPES.BOOL, Convert.ToBoolean(t.val)); 
		} else if (la.kind == 70) {
			Get();
			expr = MakeConstant(TYPES.NULL, null); 
		} else SynErr(87);
	}

	void IntSeqExpr(out Expression expr) {
		int start = 0, end = -1, step = 1; 
		if (la.kind == 2) {
			Get();
		}
		start = Convert.ToInt32(t.val); 
		Expect(5);
		Expect(2);
		end = Convert.ToInt32(t.val); 
		if (la.kind == 6) {
			Get();
			Expect(2);
			step = Convert.ToInt32(t.val); 
		}
		expr = MakeIntSeq(start, end, step); 
	}

	void AddOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		while (la.kind == 59 || la.kind == 60) {
			if (la.kind == 59) {
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

	void Variable(out string name) {
		Expect(1);
		name = t.val; 
	}

	void ParamList(ref List<Argument> @params ) {
		Argument expr; 
		Argument(out expr);
		@params.Add(expr); 
		while (la.kind == 41) {
			Get();
			Argument(out expr);
			@params.Add(expr); 
		}
	}

	void Argument(out Argument arg) {
		string name; Expression default_val = null; TYPES type = TYPES.VAR; 
		Variable(out name);
		if (la.kind == 42) {
			Get();
			Type(out type);
		}
		if (la.kind == 29) {
			Get();
			Literal(out default_val);
		}
		Parameter param = new Parameter{Name = name, ParamType = type};
		cur_scope.AddLocal(param);
		arg = new Argument{
		Name = name,
		ParamType = type,
		Option = default_val
		};
		
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		while (la.kind == 45) {
			Get();
			AndTest(out rhs);
			expr = MakeBinaryExpr(OperatorType.OR, expr, rhs); 
		}
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		NotTest(out expr);
		while (la.kind == 46) {
			Get();
			NotTest(out rhs);
			expr = MakeBinaryExpr(OperatorType.AND, expr, rhs); 
		}
	}

	void NotTest(out Expression expr) {
		Expression term; expr = null; 
		if (la.kind == 47) {
			Get();
			NotTest(out term);
			expr = MakeUnaryExpr(OperatorType.MINUS, term); 
		} else if (StartOf(7)) {
			Comparison(out expr);
		} else SynErr(88);
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		BitOr(out expr);
		type = OperatorType.EQUAL; 
		if (StartOf(10)) {
			switch (la.kind) {
			case 48: {
				Get();
				type = OperatorType.EQUAL; 
				break;
			}
			case 49: {
				Get();
				type = OperatorType.NOTEQ; 
				break;
			}
			case 50: {
				Get();
				type = OperatorType.LESS; 
				break;
			}
			case 51: {
				Get();
				type = OperatorType.GREAT; 
				break;
			}
			case 52: {
				Get();
				type = OperatorType.LESE; 
				break;
			}
			case 53: {
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
		while (la.kind == 54) {
			Get();
			BitXor(out rhs);
			expr = MakeBinaryExpr(OperatorType.BIT_OR, expr, rhs); 
		}
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		while (la.kind == 55) {
			Get();
			BitAnd(out rhs);
			expr = MakeBinaryExpr(OperatorType.BIT_XOR, expr, rhs); 
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOpe(out expr);
		while (la.kind == 56) {
			Get();
			ShiftOpe(out rhs);
			expr = MakeBinaryExpr(OperatorType.BIT_AND, expr, rhs); 
		}
	}

	void ShiftOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOpe(out expr);
		while (la.kind == 57 || la.kind == 58) {
			if (la.kind == 57) {
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
		while (la.kind == 61 || la.kind == 62 || la.kind == 63) {
			if (la.kind == 61) {
				Get();
				type = OperatorType.TIMES; 
			} else if (la.kind == 62) {
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
		if (StartOf(11)) {
			PowerOpe(out expr);
		} else if (la.kind == 59 || la.kind == 60) {
			if (la.kind == 60) {
				Get();
				type = OperatorType.MINUS; 
			} else {
				Get();
				type = OperatorType.PLUS; 
			}
			Factor(out factor);
			expr = MakeUnaryExpr(type, factor); 
		} else SynErr(89);
	}

	void PowerOpe(out Expression expr) {
		Expression rhs; 
		Primary(out expr);
		if (la.kind == 64) {
			Get();
			Factor(out rhs);
			expr = MakeBinaryExpr(OperatorType.POWER, expr, rhs); 
		}
	}

	void Primary(out Expression expr) {
		string name = ""; expr = null; 
		if (IdentIsNotCallable()) {
			Atom(out expr);
		} else if (la.kind == 1) {
			Get();
			name = t.val; 
		} else SynErr(90);
		while (la.kind == 9 || la.kind == 65 || la.kind == 67) {
			Trailer(ref expr, name);
		}
	}

	void Atom(out Expression expr) {
		string name; expr = null; 
		if (la.kind == 1) {
			Variable(out name);
			expr = cur_scope.GetVariable(name);
			if(expr == null){
			SemErr(string.Format("Attempt to refer to an undefined variable \"{0}\"", name));
			}
			
		} else if (StartOf(12)) {
			if (NotFollowedByDoubleDots()) {
				Literal(out expr);
			} else {
				IntSeqExpr(out expr);
			}
		} else if (la.kind == 9) {
			Get();
			if (StartOf(3)) {
				TupleMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 32)) {SynErr(91); Get();}
			Expect(32);
		} else if (la.kind == 65) {
			Get();
			if (StartOf(3)) {
				ListMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 66)) {SynErr(92); Get();}
			Expect(66);
		} else if (la.kind == 8) {
			Get();
			if (StartOf(3)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 30)) {SynErr(93); Get();}
			Expect(30);
		} else SynErr(94);
	}

	void Trailer(ref Expression expr, string name) {
		Function func; List<Expression> args = new List<Expression>(); Expression subscript; 
		if (la.kind == 9) {
			Get();
			if (StartOf(3)) {
				ArgList(ref args);
			}
			Expect(32);
			func = funcs.GetFunction(name);
			if(func == null){
			SemErr("The function is not defined : " + name);
			}
			expr = new Call{
			Function = func,
			Arguments = args
			};
			
		} else if (la.kind == 65) {
			Get();
			Subscript(out subscript);
			Expect(66);
			expr = new MemberReference{
			Parent = expr,
			Subscription = subscript
			};
			
		} else if (la.kind == 67) {
			Get();
			Expect(1);
			subscript = new Parameter{
			Name = t.val,
			ParamType = TYPES.SUBSCRIPT
			};
			expr = new MemberReference{
			Parent = expr,
			Subscription = subscript
			};
			
		} else SynErr(95);
	}

	void ArgList(ref List<Expression> args ) {
		Expression expr; 
		CondExpr(out expr);
		args.Add(expr); 
		while (la.kind == 41) {
			Get();
			CondExpr(out expr);
			args.Add(expr); 
		}
	}

	void Subscript(out Expression subscript) {
		subscript = null; 
		CondExpr(out subscript);
	}

	void TupleMaker(out Expression expr) {
		Expression tmp; List<Expression> list = new List<Expression>(); expr = null; 
		CondExpr(out tmp);
		list.Add(tmp); 
		while (la.kind == 41) {
			Get();
			CondExpr(out tmp);
			list.Add(tmp); 
		}
		expr = MakeObjInitializer(TYPES.TUPLE, list); 
	}

	void ListMaker(out Expression expr) {
		expr = null; Expression tmp; List<Expression> list = new List<Expression>(); 
		CondExpr(out tmp);
		list.Add(tmp); 
		if (la.kind == 35) {
			CompFor(out tmp);
			expr = MakeComp(list[0], tmp); 
		} else if (la.kind == 41 || la.kind == 66) {
			while (la.kind == 41) {
				Get();
				CondExpr(out tmp);
				list.Add(tmp); 
			}
		} else SynErr(96);
		if(expr == null) expr = MakeObjInitializer(TYPES.LIST, list); 
	}

	void DictMaker(out Expression expr) {
		Expression lhs, rhs; List<Expression> list = new List<Expression>(); 
		CondExpr(out lhs);
		list.Add(lhs); 
		Expect(6);
		CondExpr(out rhs);
		list.Add(rhs); 
		while (la.kind == 41) {
			Get();
			CondExpr(out lhs);
			list.Add(lhs); 
			Expect(6);
			CondExpr(out rhs);
			list.Add(rhs); 
		}
		expr = MakeObjInitializer(TYPES.DICT, list); 
	}

	void CompFor(out Expression expr) {
		Expression iter, body = null; List<Expression> expr_list, test_list; 
		Expect(35);
		if (la.kind == 36) {
			Get();
		}
		LValueList(out expr_list);
		Expect(37);
		RValueList(out test_list);
		iter = MakeIteration(expr_list, test_list); 
		if (la.kind == 31 || la.kind == 35) {
			CompIter(out body);
		}
		expr = MakeCompFor(iter, body); 
	}

	void CompIter(out Expression expr) {
		expr = null; 
		if (la.kind == 35) {
			CompFor(out expr);
		} else if (la.kind == 31) {
			CompIf(out expr);
		} else SynErr(97);
	}

	void CompIf(out Expression expr) {
		Expression tmp, body = null; 
		Expect(31);
		OrTest(out tmp);
		if (la.kind == 31 || la.kind == 35) {
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
		{T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, T,T,T,x, x},
		{x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, T,T,T,x, x},
		{x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, T,T,T,x, x},
		{x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, T,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, T,x,x,T, x,x,T,T, T,x,T,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, T,T,T,x, x},
		{x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, T,T,T,x, x},
		{x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x}

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
			case 1: s = "ident expected"; break;
			case 2: s = "integer expected"; break;
			case 3: s = "float expected"; break;
			case 4: s = "string_literal expected"; break;
			case 5: s = "double_dots expected"; break;
			case 6: s = "colon expected"; break;
			case 7: s = "semicolon expected"; break;
			case 8: s = "lcurly expected"; break;
			case 9: s = "lparen expected"; break;
			case 10: s = "\"int\" expected"; break;
			case 11: s = "\"bool\" expected"; break;
			case 12: s = "\"float\" expected"; break;
			case 13: s = "\"rational\" expected"; break;
			case 14: s = "\"bigint\" expected"; break;
			case 15: s = "\"string\" expected"; break;
			case 16: s = "\"bytearray\" expected"; break;
			case 17: s = "\"var\" expected"; break;
			case 18: s = "\"tuple\" expected"; break;
			case 19: s = "\"list\" expected"; break;
			case 20: s = "\"dictionary\" expected"; break;
			case 21: s = "\"expression\" expected"; break;
			case 22: s = "\"function\" expected"; break;
			case 23: s = "\"intseq\" expected"; break;
			case 24: s = "\"print\" expected"; break;
			case 25: s = "\"return\" expected"; break;
			case 26: s = "\"break\" expected"; break;
			case 27: s = "\"upto\" expected"; break;
			case 28: s = "\"continue\" expected"; break;
			case 29: s = "\"=\" expected"; break;
			case 30: s = "\"}\" expected"; break;
			case 31: s = "\"if\" expected"; break;
			case 32: s = "\")\" expected"; break;
			case 33: s = "\"else\" expected"; break;
			case 34: s = "\"while\" expected"; break;
			case 35: s = "\"for\" expected"; break;
			case 36: s = "\"let\" expected"; break;
			case 37: s = "\"in\" expected"; break;
			case 38: s = "\"switch\" expected"; break;
			case 39: s = "\"case\" expected"; break;
			case 40: s = "\"default\" expected"; break;
			case 41: s = "\",\" expected"; break;
			case 42: s = "\"(-\" expected"; break;
			case 43: s = "\"def\" expected"; break;
			case 44: s = "\"?\" expected"; break;
			case 45: s = "\"or\" expected"; break;
			case 46: s = "\"and\" expected"; break;
			case 47: s = "\"not\" expected"; break;
			case 48: s = "\"==\" expected"; break;
			case 49: s = "\"!=\" expected"; break;
			case 50: s = "\"<\" expected"; break;
			case 51: s = "\">\" expected"; break;
			case 52: s = "\"<=\" expected"; break;
			case 53: s = "\">=\" expected"; break;
			case 54: s = "\"|\" expected"; break;
			case 55: s = "\"^\" expected"; break;
			case 56: s = "\"&\" expected"; break;
			case 57: s = "\"<<\" expected"; break;
			case 58: s = "\">>\" expected"; break;
			case 59: s = "\"+\" expected"; break;
			case 60: s = "\"-\" expected"; break;
			case 61: s = "\"*\" expected"; break;
			case 62: s = "\"/\" expected"; break;
			case 63: s = "\"%\" expected"; break;
			case 64: s = "\"**\" expected"; break;
			case 65: s = "\"[\" expected"; break;
			case 66: s = "\"]\" expected"; break;
			case 67: s = "\".\" expected"; break;
			case 68: s = "\"true\" expected"; break;
			case 69: s = "\"false\" expected"; break;
			case 70: s = "\"null\" expected"; break;
			case 71: s = "??? expected"; break;
			case 72: s = "invalid Expresso"; break;
			case 73: s = "invalid ExprStmt"; break;
			case 74: s = "this symbol not expected in ExprStmt"; break;
			case 75: s = "this symbol not expected in FuncDecl"; break;
			case 76: s = "invalid Type"; break;
			case 77: s = "invalid Stmt"; break;
			case 78: s = "invalid SimpleStmt"; break;
			case 79: s = "this symbol not expected in CompoundStmt"; break;
			case 80: s = "invalid CompoundStmt"; break;
			case 81: s = "this symbol not expected in PrintStmt"; break;
			case 82: s = "this symbol not expected in ReturnStmt"; break;
			case 83: s = "this symbol not expected in BreakStmt"; break;
			case 84: s = "this symbol not expected in ContinueStmt"; break;
			case 85: s = "invalid CaseLabel"; break;
			case 86: s = "this symbol not expected in CaseLabel"; break;
			case 87: s = "invalid Literal"; break;
			case 88: s = "invalid NotTest"; break;
			case 89: s = "invalid Factor"; break;
			case 90: s = "invalid Primary"; break;
			case 91: s = "this symbol not expected in Atom"; break;
			case 92: s = "this symbol not expected in Atom"; break;
			case 93: s = "this symbol not expected in Atom"; break;
			case 94: s = "invalid Atom"; break;
			case 95: s = "invalid Trailer"; break;
			case 96: s = "invalid ListMaker"; break;
			case 97: s = "invalid CompIter"; break;

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
