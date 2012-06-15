using System.Collections.Generic;
using Expresso.Ast;
using Expresso.BuiltIns;
using Expresso.Interpreter;





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
	public const int maxT = 67;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

internal Scope cur_scope = new Scope();		//the current scope of variables
	private Scope funcs = new Scope();			//the namespace for funcTable
	public Block root = new Block();
	
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
		Statement stmt; 
		FuncDecl(out stmt);
		root.Statements.Add(stmt); 
		while (StartOf(1)) {
			if (StartOf(2)) {
				ExprStmt(out stmt);
				root.Statements.Add(stmt); 
			} else {
				FuncDecl(out stmt);
				root.Statements.Add(stmt); 
			}
		}
	}

	void FuncDecl(out Statement func) {
		string name; TYPES type; Statement block; List<Argument>@params; 
		Expect(40);
		type = TYPES.VAR; block = null; 
		Expect(1);
		name = t.val; 
		Expect(31);
		ParamList(out @params);
		Expect(32);
		if (la.kind == 39) {
			Get();
			Type(out type);
		}
		Block(out block);
		func = new Function{
		Name = name,
		Parameters = @params,
		Body = (Block)block,
		ReturnType = type
		};
		funcs.AddFunction(func as Function);
		
	}

	void ExprStmt(out Statement stmt) {
		List<Expression> targets = null; List<Expression> expr_list; stmt = null; 
		if (la.kind == 38) {
			VarDecl(out targets);
		} else if (StartOf(3)) {
			RValueList(out targets);
			while (la.kind == 28) {
				Get();
				RValueList(out expr_list);
				stmt = new Assignment{
				Targets = targets,
				Expressions = expr_list
				};
				
			}
		} else SynErr(68);
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(69); Get();}
		Expect(7);
		if(stmt == null){
		stmt = new ExprStatement{Expressions = targets};
		}
		
	}

	void Type(out TYPES type) {
		type = TYPES.UNDEF; 
		switch (la.kind) {
		case 9: {
			Get();
			type = TYPES.INTEGER; 
			break;
		}
		case 10: {
			Get();
			type = TYPES.BOOL; 
			break;
		}
		case 11: {
			Get();
			type = TYPES.FLOAT; 
			break;
		}
		case 12: {
			Get();
			type = TYPES.RATIONAL; 
			break;
		}
		case 13: {
			Get();
			type = TYPES.BIGINT; 
			break;
		}
		case 14: {
			Get();
			type = TYPES.STRING; 
			break;
		}
		case 15: {
			Get();
			type = TYPES.CHARSEQ; 
			break;
		}
		case 16: {
			Get();
			type = TYPES.VAR; 
			break;
		}
		case 17: {
			Get();
			type = TYPES.TUPLE; 
			break;
		}
		case 18: {
			Get();
			type = TYPES.LIST; 
			break;
		}
		case 19: {
			Get();
			type = TYPES.DICT; 
			break;
		}
		case 20: {
			Get();
			type = TYPES.EXPRESSION; 
			break;
		}
		case 21: {
			Get();
			type = TYPES.FUNCTION; 
			break;
		}
		case 22: {
			Get();
			type = TYPES.RANGE; 
			break;
		}
		default: SynErr(70); break;
		}
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(4)) {
			SimpleStmt(out stmt);
		} else if (StartOf(5)) {
			CompoundStmt(out stmt);
		} else SynErr(71);
	}

	void SimpleStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == 8) {
			Block(out stmt);
		} else if (StartOf(2)) {
			ExprStmt(out stmt);
		} else if (la.kind == 23) {
			PrintStmt(out stmt);
		} else if (la.kind == 24) {
			ReturnStmt(out stmt);
		} else if (la.kind == 25) {
			BreakStmt(out stmt);
		} else if (la.kind == 27) {
			ContinueStmt(out stmt);
		} else SynErr(72);
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == 30) {
			IfStmt(out stmt);
		} else if (la.kind == 34) {
			WhileStmt(out stmt);
		} else if (la.kind == 35) {
			ForStmt(out stmt);
		} else if (la.kind == 40) {
			FuncDecl(out stmt);
		} else SynErr(73);
	}

	void Block(out Statement block) {
		Block tmp; Scope scope = new Scope{Parent = cur_scope}; cur_scope = scope; Statement stmt; 
		Expect(8);
		tmp = new Block(); 
		Stmt(out stmt);
		tmp.Statements.Add(stmt); 
		while (StartOf(6)) {
			Stmt(out stmt);
			tmp.Statements.Add(stmt); 
		}
		while (!(la.kind == 0 || la.kind == 29)) {SynErr(74); Get();}
		Expect(29);
		cur_scope = cur_scope.Parent; block = tmp; 
	}

	void PrintStmt(out Statement stmt) {
		List<Expression> exprs; 
		Expect(23);
		RValueList(out exprs);
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(75); Get();}
		Expect(7);
		stmt = new PrintStatement{Expressions = exprs}; 
	}

	void ReturnStmt(out Statement stmt) {
		List<Expression> target_list = new List<Expression>(); 
		Expect(24);
		if (StartOf(3)) {
			RValueList(out target_list);
		}
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(76); Get();}
		Expect(7);
		stmt = new Return{Expressions = target_list}; 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; 
		Expect(25);
		if (la.kind == 26) {
			Get();
			Expect(2);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(77); Get();}
		Expect(7);
		stmt = new BreakStatement{Count = count}; 
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; 
		Expect(27);
		if (la.kind == 26) {
			Get();
			Expect(2);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 7)) {SynErr(78); Get();}
		Expect(7);
		stmt = new ContinueStatement{Count = count}; 
	}

	void RValueList(out List<Expression> exprs ) {
		Expression tmp; 
		exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (la.kind == 37) {
			Get();
			CondExpr(out tmp);
			exprs.Add(tmp); 
		}
	}

	void VarDecl(out List<Expression> outs ) {
		string name; TYPES type; Expression rhs = null; Parameter variable; outs = new List<Expression>();
		var vars = new List<Parameter>(); var exprs = new List<Expression>();
		
		Expect(38);
		type = TYPES.VAR; 
		Variable(out name);
		if (la.kind == 39) {
			Get();
			Type(out type);
		}
		if (la.kind == 28) {
			Get();
			CondExpr(out rhs);
		}
		while (la.kind == 37) {
			Get();
			variable = DeclareVariable(ref rhs, name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			
			Variable(out name);
			if (la.kind == 39) {
				Get();
				Type(out type);
			}
			if (la.kind == 28) {
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
		Expect(30);
		Expect(31);
		CondExpr(out tmp);
		Expect(32);
		Stmt(out true_block);
		if (la.kind == 33) {
			Get();
			Stmt(out false_block);
		}
		stmt = new IfStatement{
		Condition = tmp,
		TrueBlock = true_block,
		FalseBlock = false_block
		};
		
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; Statement body = null; 
		Expect(34);
		Expect(31);
		CondExpr(out cond);
		Expect(32);
		Stmt(out body);
		stmt = new WhileStatement{
		Condition = cond,
		Body = body
		};
		
	}

	void ForStmt(out Statement stmt) {
		List<Expression> target_list; Expression rvalue; Statement body; 
		Expect(35);
		Expect(31);
		LValueList(out target_list);
		Expect(36);
		CondExpr(out rvalue);
		Expect(32);
		Stmt(out body);
		stmt = new ForStatement{
		LValues = target_list,
		Target = rvalue,
		Body = body
		};
		
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; 
		OrTest(out expr);
		if (la.kind == 41) {
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
		while (WeakSeparator(37,7,8) ) {
			AddOpe(out tmp);
			targets.Add(tmp); 
		}
	}

	void AddOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		while (la.kind == 56 || la.kind == 57) {
			if (la.kind == 56) {
				Get();
				type = OperatorType.PLUS; 
			} else {
				Get();
				type = OperatorType.MINUS; 
			}
			Term(out rhs);
			expr = new BinaryExpression{
			Operator = type,
			Left = expr,
			Right = rhs
			};
			
		}
	}

	void Variable(out string name) {
		ExpectWeak(64, 9);
		Expect(1);
		name = t.val; 
	}

	void Argument(out Argument arg) {
		string name; Expression default_val = null; TYPES type = TYPES.VAR; 
		Variable(out name);
		if (la.kind == 39) {
			Get();
			Type(out type);
		}
		if (la.kind == 28) {
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

	void Literal(out Expression expr) {
		expr = null; 
		if (la.kind == 2) {
			Get();
			expr = new Constant{
			ValType = TYPES.INTEGER,
			Value = new ExpressoPrimitive{Value = Convert.ToInt32(t.val)}
			};
			
		} else if (la.kind == 3) {
			Get();
			expr = new Constant{
			ValType = TYPES.FLOAT,
			Value = new ExpressoPrimitive{Value = Convert.ToDouble(t.val)}
			};
			
		} else if (la.kind == 4) {
			Get();
			string tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = new Constant{
			ValType = TYPES.STRING,
			Value = new ExpressoPrimitive{Value = tmp}
			};
			
		} else if (la.kind == 65 || la.kind == 66) {
			if (la.kind == 65) {
				Get();
			} else {
				Get();
			}
			expr = new Constant{
			ValType = TYPES.BOOL,
			Value = new ExpressoPrimitive{Value = Convert.ToBoolean(t.val)}
			};
			
		} else SynErr(79);
	}

	void ParamList(out List<Argument> @params ) {
		@params = new List<Argument>(); Argument expr; 
		if (la.kind == 64) {
			Argument(out expr);
			@params.Add(expr); 
			while (la.kind == 37) {
				Get();
				Argument(out expr);
				@params.Add(expr); 
			}
		}
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		while (la.kind == 42) {
			Get();
			AndTest(out rhs);
			expr = Node.Or(expr, rhs); 
		}
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		NotTest(out expr);
		while (la.kind == 43) {
			Get();
			NotTest(out rhs);
			expr = Node.And(expr, rhs); 
		}
	}

	void NotTest(out Expression expr) {
		Expression term; expr = null; 
		if (la.kind == 44) {
			Get();
			NotTest(out term);
			expr = Node.Negate(term); 
		} else if (StartOf(7)) {
			Comparison(out expr);
		} else SynErr(80);
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		BitOr(out expr);
		type = OperatorType.EQUAL; 
		if (StartOf(10)) {
			switch (la.kind) {
			case 45: {
				Get();
				type = OperatorType.EQUAL; 
				break;
			}
			case 46: {
				Get();
				type = OperatorType.NOTEQ; 
				break;
			}
			case 47: {
				Get();
				type = OperatorType.LESS; 
				break;
			}
			case 48: {
				Get();
				type = OperatorType.GREAT; 
				break;
			}
			case 49: {
				Get();
				type = OperatorType.LESE; 
				break;
			}
			case 50: {
				Get();
				type = OperatorType.GRTE; 
				break;
			}
			}
			BitOr(out rhs);
			expr = new BinaryExpression{
			Operator = type,
			Left = expr,
			Right = rhs
			};
			
		}
	}

	void BitOr(out Expression expr) {
		Expression rhs; 
		BitXor(out expr);
		while (la.kind == 51) {
			Get();
			BitXor(out rhs);
			expr = new BinaryExpression{
			Operator = OperatorType.BIT_OR,
			Left = expr,
			Right = rhs
			};
			
		}
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		while (la.kind == 52) {
			Get();
			BitAnd(out rhs);
			expr = new BinaryExpression{
			Operator = OperatorType.BIT_XOR,
			Left = expr,
			Right = rhs
			};
			
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOpe(out expr);
		while (la.kind == 53) {
			Get();
			ShiftOpe(out rhs);
			expr = new BinaryExpression{
			Operator = OperatorType.BIT_AND,
			Left = expr,
			Right = rhs
			};
			
		}
	}

	void ShiftOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOpe(out expr);
		while (la.kind == 54 || la.kind == 55) {
			if (la.kind == 54) {
				Get();
				type = OperatorType.BIT_LSHIFT; 
			} else {
				Get();
				type = OperatorType.BIT_RSHIFT; 
			}
			AddOpe(out rhs);
			expr = new BinaryExpression{
			Operator = type,
			Left = expr,
			Right = rhs
			};
			
		}
	}

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		Factor(out expr);
		while (la.kind == 58 || la.kind == 59 || la.kind == 60) {
			if (la.kind == 58) {
				Get();
				type = OperatorType.TIMES; 
			} else if (la.kind == 59) {
				Get();
				type = OperatorType.DIV; 
			} else {
				Get();
				type = OperatorType.MOD; 
			}
			Factor(out rhs);
			expr = new BinaryExpression{
			Operator = type,
			Left = expr,
			Right = rhs
			}; 
			
		}
	}

	void Factor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; 
		if (StartOf(11)) {
			PowerOpe(out expr);
		} else if (la.kind == 56 || la.kind == 57) {
			if (la.kind == 57) {
				Get();
				type = OperatorType.MINUS; 
			} else {
				Get();
				type = OperatorType.PLUS; 
			}
			Factor(out factor);
			expr = Node.Unary(type, factor); 
		} else SynErr(81);
	}

	void PowerOpe(out Expression expr) {
		Expression rhs; 
		Primary(out expr);
		if (la.kind == 61) {
			Get();
			Factor(out rhs);
			expr = Node.Power(expr, rhs); 
		}
	}

	void Primary(out Expression expr) {
		string name = ""; expr = null; Function func; List<Expression> args; 
		if (StartOf(12)) {
			Atom(out expr);
		} else if (la.kind == 1) {
			Get();
			name = t.val; 
		} else SynErr(82);
		while (la.kind == 31) {
			Get();
			ArgList(out args);
			Expect(32);
			func = funcs.GetFunction(name);
			if(func == null){
			SemErr("The function is not defined : " + name);
			}
			expr = new Call{
			Function = func,
			Arguments = args
			};
			
		}
	}

	void ArgList(out List<Expression> args ) {
		args = new List<Expression>(); Expression expr; 
		if (StartOf(11)) {
			Primary(out expr);
			args.Add(expr); 
			while (la.kind == 37) {
				Get();
				Primary(out expr);
				args.Add(expr); 
			}
		}
	}

	void Atom(out Expression expr) {
		string name; expr = null; 
		if (la.kind == 64) {
			Variable(out name);
			expr = cur_scope.GetVariable(name);
			if(expr == null){
			SemErr(string.Format("Attempt to refer to an undefined variable \"{0}\"", name));
			}
			
		} else if (StartOf(13)) {
			Literal(out expr);
		} else if (la.kind == 31) {
			Get();
			if (StartOf(3)) {
				TupleOrRange(out expr);
			}
			while (!(la.kind == 0 || la.kind == 32)) {SynErr(83); Get();}
			Expect(32);
		} else if (la.kind == 62) {
			Get();
			if (StartOf(3)) {
				ListMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 63)) {SynErr(84); Get();}
			Expect(63);
		} else if (la.kind == 8) {
			Get();
			if (StartOf(3)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 29)) {SynErr(85); Get();}
			Expect(29);
		} else SynErr(86);
	}

	void TupleOrRange(out Expression expr) {
		Expression tmp; List<Expression> list = new List<Expression>(); expr = null; 
		if (NotFollowedByDoubleDots()) {
			CondExpr(out tmp);
			list.Add(tmp); 
			while (la.kind == 37) {
				Get();
				CondExpr(out tmp);
				list.Add(tmp); 
			}
			expr = new ObjectInitializer{
			InitializeList = list,
			ObjType = TYPES.TUPLE
			};
			
		} else if (la.kind == 2) {
			RangeExpr(out expr);
		} else SynErr(87);
	}

	void ListMaker(out Expression expr) {
		Expression tmp; List<Expression> list = new List<Expression>(); 
		CondExpr(out tmp);
		list.Add(tmp); 
		while (la.kind == 37) {
			Get();
			CondExpr(out tmp);
			list.Add(tmp); 
		}
		expr = new ObjectInitializer{
		InitializeList = list,
		ObjType = TYPES.LIST
		};
		
	}

	void DictMaker(out Expression expr) {
		Expression lhs, rhs; List<Expression> list = new List<Expression>(); 
		CondExpr(out lhs);
		list.Add(lhs); 
		Expect(6);
		CondExpr(out rhs);
		list.Add(rhs); 
		while (la.kind == 37) {
			Get();
			CondExpr(out lhs);
			list.Add(lhs); 
			Expect(6);
			CondExpr(out rhs);
			list.Add(rhs); 
		}
		expr = new ObjectInitializer{
		InitializeList = list,
		ObjType = TYPES.DICT
		};
		
	}

	void RangeExpr(out Expression expr) {
		int start, end, step; 
		Expect(2);
		start = Convert.ToInt32(t.val); end = -1; step = 1; 
		Expect(5);
		if (la.kind == 2) {
			Get();
		}
		end = Convert.ToInt32(t.val); 
		if (la.kind == 6) {
			Get();
			Expect(2);
			step = Convert.ToInt32(t.val); 
		}
		expr = new RangeExpression{
		Start = start,
		End = end,
		Step = step
		};
		
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Expresso();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,T,T,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,T,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,T,T,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,T,T,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,x,T, x,x,x,T, x,x,x,x, x,x,T,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,x,T, x,x,T,T, x,x,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,T,T,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,T,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,T,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,T,T,x, x},
		{x,x,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x}

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
			case 9: s = "\"int\" expected"; break;
			case 10: s = "\"bool\" expected"; break;
			case 11: s = "\"float\" expected"; break;
			case 12: s = "\"rational\" expected"; break;
			case 13: s = "\"big_int\" expected"; break;
			case 14: s = "\"string\" expected"; break;
			case 15: s = "\"char_seq\" expected"; break;
			case 16: s = "\"var\" expected"; break;
			case 17: s = "\"tuple\" expected"; break;
			case 18: s = "\"list\" expected"; break;
			case 19: s = "\"dictionary\" expected"; break;
			case 20: s = "\"expression\" expected"; break;
			case 21: s = "\"function\" expected"; break;
			case 22: s = "\"range\" expected"; break;
			case 23: s = "\"print\" expected"; break;
			case 24: s = "\"return\" expected"; break;
			case 25: s = "\"break\" expected"; break;
			case 26: s = "\"upto\" expected"; break;
			case 27: s = "\"continue\" expected"; break;
			case 28: s = "\"=\" expected"; break;
			case 29: s = "\"}\" expected"; break;
			case 30: s = "\"if\" expected"; break;
			case 31: s = "\"(\" expected"; break;
			case 32: s = "\")\" expected"; break;
			case 33: s = "\"else\" expected"; break;
			case 34: s = "\"while\" expected"; break;
			case 35: s = "\"for\" expected"; break;
			case 36: s = "\"in\" expected"; break;
			case 37: s = "\",\" expected"; break;
			case 38: s = "\"let\" expected"; break;
			case 39: s = "\"(-\" expected"; break;
			case 40: s = "\"def\" expected"; break;
			case 41: s = "\"?\" expected"; break;
			case 42: s = "\"or\" expected"; break;
			case 43: s = "\"and\" expected"; break;
			case 44: s = "\"not\" expected"; break;
			case 45: s = "\"==\" expected"; break;
			case 46: s = "\"!=\" expected"; break;
			case 47: s = "\"<\" expected"; break;
			case 48: s = "\">\" expected"; break;
			case 49: s = "\"<=\" expected"; break;
			case 50: s = "\">=\" expected"; break;
			case 51: s = "\"|\" expected"; break;
			case 52: s = "\"^\" expected"; break;
			case 53: s = "\"&\" expected"; break;
			case 54: s = "\"<<\" expected"; break;
			case 55: s = "\">>\" expected"; break;
			case 56: s = "\"+\" expected"; break;
			case 57: s = "\"-\" expected"; break;
			case 58: s = "\"*\" expected"; break;
			case 59: s = "\"/\" expected"; break;
			case 60: s = "\"%\" expected"; break;
			case 61: s = "\"**\" expected"; break;
			case 62: s = "\"[\" expected"; break;
			case 63: s = "\"]\" expected"; break;
			case 64: s = "\"$\" expected"; break;
			case 65: s = "\"true\" expected"; break;
			case 66: s = "\"false\" expected"; break;
			case 67: s = "??? expected"; break;
			case 68: s = "invalid ExprStmt"; break;
			case 69: s = "this symbol not expected in ExprStmt"; break;
			case 70: s = "invalid Type"; break;
			case 71: s = "invalid Stmt"; break;
			case 72: s = "invalid SimpleStmt"; break;
			case 73: s = "invalid CompoundStmt"; break;
			case 74: s = "this symbol not expected in Block"; break;
			case 75: s = "this symbol not expected in PrintStmt"; break;
			case 76: s = "this symbol not expected in ReturnStmt"; break;
			case 77: s = "this symbol not expected in BreakStmt"; break;
			case 78: s = "this symbol not expected in ContinueStmt"; break;
			case 79: s = "invalid Literal"; break;
			case 80: s = "invalid NotTest"; break;
			case 81: s = "invalid Factor"; break;
			case 82: s = "invalid Primary"; break;
			case 83: s = "this symbol not expected in Atom"; break;
			case 84: s = "this symbol not expected in Atom"; break;
			case 85: s = "this symbol not expected in Atom"; break;
			case 86: s = "invalid Atom"; break;
			case 87: s = "invalid TupleOrRange"; break;

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
