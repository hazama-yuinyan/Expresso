using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using Expresso.Ast;
using Expresso.Builtins;
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
	public const int _dot = 11;
	public const int _ident = 12;
	public const int _integer = 13;
	public const int _float = 14;
	public const int _hex_digit = 15;
	public const int _string_literal = 16;
	public const int _keyword_in = 17;
	public const int _keyword_for = 18;
	public const int maxT = 100;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

internal AnalysisScope cur_scope = new AnalysisScope{Parent = AnalysisScope.SymbolTable};		//the current scope of variables
	static private List<BreakableStatement> breakables = new List<BreakableStatement>();	//the current parent breakbles hierarchy
	bool ignore_parent_scope = false;
	public string ParsingFileName{get; set;}
	public ModuleDeclaration ParsingModule{get; private set;}	//the module the parser is parsing
	
	Parser()
	{
		//Add built-in functions
		Function[] native_funcs = {
			new NativeLambdaUnary("abs", ImplementationHelpers.MakeArg(new Identifier("val", new TypeAnnotation(ObjectTypes.VAR), null, 0)), ExpressoFunctions.Abs),
			new NativeLambdaUnary("sqrt", ImplementationHelpers.MakeArg(new Identifier("val", new TypeAnnotation(ObjectTypes.VAR), null, 0)), ExpressoFunctions.Sqrt),
			new NativeLambdaUnary("toInt", ImplementationHelpers.MakeArg(new Identifier("val", new TypeAnnotation(ObjectTypes.VAR), null, 0)), ExpressoFunctions.ToInt)
		};
		foreach(var tmp in native_funcs)
			cur_scope.AddFunction(tmp);
	}
	
	Constant CreateConstant(ObjectTypes type)
	{
		Constant result = null;
		
		switch(type){
		case ObjectTypes.INTEGER:
			result = new Constant{ValType = type, Value = 0};
			break;
			
		case ObjectTypes.BOOL:
			result = new Constant{ValType = type, Value = false};
			break;
			
		case ObjectTypes.FLOAT:
			result = new Constant{ValType = type, Value = 0.0};
			break;
			
		case ObjectTypes.STRING:
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
	
	bool ExpectIdentIsTypeName()
	{
		return la.kind == _dot;
	}
	
	bool IsObjectInitializer()
	{
		Token x = la;
		if(x.kind != _lcurly) return true;
		scanner.ResetPeek();
		while(x.kind != 0 && x.kind != _keyword_for && x.kind != _rcurly) x = scanner.Peek();
		return x.kind != _keyword_for;
	}
	
	Identifier DeclareVariable(ref Expression rhs, string name, TypeAnnotation type)
	{
		if(rhs == null){
  			if(type.ObjType == ObjectTypes._INFERENCE)
  				SemErr("Can not infer the type of the variable \"" + name + "\" without initialization!");
  			
  			rhs = CreateConstant(type.ObjType);
  		}else{
  			if(type.ObjType == ObjectTypes._INFERENCE){
  				if(rhs is Constant){
  					var constant = (Constant)rhs;
  					type.ObjType = constant.ValType;
  				}else if(rhs is ObjectInitializer){
  					var initializer = (ObjectInitializer)rhs;
  					type.ObjType = initializer.ObjType;
  				}else if(rhs is Comprehension){
  					var comprehen = (Comprehension)rhs;
  					type.ObjType = comprehen.ObjType;
  				}else if(rhs is NewExpression){
  					type.ObjType = ObjectTypes.INSTANCE;
  					//type.TypeName = ((NewExpression)rhs).TargetName;
  				}else if(rhs is Call){
  					var call = (Call)rhs;
  					if(call.Function != null)
  						type = call.Function.ReturnType;
  				}else if(rhs is MemberReference){
  					type.ObjType = ObjectTypes.VAR;
  				}else{
  					SemErr("Can not infer the type of the variable \"" + name + "\" from that type of expression.");
  				}
  			}
  		}
  		
		var variable = new Identifier(name, type);
 		cur_scope.AddLocal(ref variable);
 		return variable;
	}
	
	Identifier DeclareArgument(string name, TypeAnnotation type)
	{
		var argument = new Identifier(name, type);
		
		cur_scope.AddLocal(ref argument);
		return argument;
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
	
	static ContinueStatement MakeContinueStatement(int count)
	{
		var tmp = new List<BreakableStatement>();
		for(int len = Parser.breakables.Count, i = 0, j = count; j > 0; ++i){
			var enclosing = Parser.breakables[len - 1 - i];
			if(enclosing.Type != NodeType.WhileStatement && enclosing.Type != NodeType.ForStatement
				|| j != 1)			//don't include the loop on which we'll continue
				tmp.Add(enclosing);
			
			if(enclosing.Type == NodeType.WhileStatement || enclosing.Type == NodeType.ForStatement)
				--j;
		}
		return new ContinueStatement{Count = count, Enclosings = tmp};
	}
	
	Statement MakeDefaultCtor(string className)
	{
		cur_scope = new AnalysisScope{Parent = cur_scope};
		var ident_this = DeclareArgument("this", new TypeAnnotation(ObjectTypes.INSTANCE, className));
		var @params = new List<Argument>{new Argument{Ident = ident_this}};
		var ctor = Node.MakeFunc("constructor", @params, new Block(), TypeAnnotation.VoidType.Clone(), true);
		cur_scope = cur_scope.Parent;
		return ctor;
	}
	
/*--------------------------------------------------------------------------*/


	public Parser(Scanner scanner) : this() {
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
		ModuleDeclaration module_decl = null; 
		ModuleBody(out module_decl);
		this.ParsingModule = module_decl; 
	}

	void ModuleBody(out ModuleDeclaration decl) {
		var decls = new List<Statement>(); var export_map = new List<bool>();
		bool has_export = false; List<Statement> prog_defs = null; Statement stmt = null;
		
		if (la.kind == 20) {
			ProgramDefinition(out prog_defs);
		}
		if (la.kind == 19) {
			Get();
			has_export = true; 
		}
		if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 26) {
			FuncDecl(out stmt);
		} else if (la.kind == 22) {
			ClassDecl(out stmt);
		} else SynErr(101);
		decls.Add(stmt);
		export_map.Add(has_export);
		has_export = false;
		
		while (StartOf(2)) {
			if (la.kind == 19) {
				Get();
				has_export = true; 
			}
			if (StartOf(1)) {
				ExprStmt(out stmt);
			} else if (la.kind == 26) {
				FuncDecl(out stmt);
			} else if (la.kind == 22) {
				ClassDecl(out stmt);
			} else SynErr(102);
			decls.Add(stmt);
			export_map.Add(has_export);
			has_export = false;
			
		}
		var main_func = cur_scope.GetFunction("main");
		var module_name = (main_func != null) ? "main" : ParsingFileName;
		decl = Node.MakeModuleDef(module_name, prog_defs, decls, export_map);
		
	}

	void ProgramDefinition(out List<Statement> requires ) {
		requires = new List<Statement>(); Statement tmp; 
		RequireStmt(out tmp);
		requires.Add(tmp); 
		while (la.kind == 20) {
			RequireStmt(out tmp);
			requires.Add(tmp); 
		}
	}

	void ExprStmt(out Statement stmt) {
		List<Expression> targets = null; List<Expression> expr_list;
		stmt = null; OperatorType op_type = OperatorType.NONE;
		
		if (la.kind == 68) {
			VarDecl(out targets);
		} else if (StartOf(3)) {
			RValueList(out targets);
			if (StartOf(4)) {
				AugAssignOpe(ref op_type);
				RValueList(out expr_list);
				stmt = Node.MakeAugumentedAssignment(targets, expr_list, op_type); 
			} else if (la.kind == 4 || la.kind == 29) {
				while (la.kind == 29) {
					Get();
					RValueList(out expr_list);
					stmt = Node.MakeAssignment(targets, expr_list); 
				}
			} else SynErr(103);
		} else SynErr(104);
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(105); Get();}
		Expect(4);
		if(stmt == null) stmt = Node.MakeExprStmt(targets); 
	}

	void FuncDecl(out Statement func) {
		string name; var type = TypeAnnotation.VariantType.Clone(); Statement block; var @params = new List<Argument>();
		Identifier ident_this = null;
		
		while (!(la.kind == 0 || la.kind == 26)) {SynErr(106); Get();}
		Expect(26);
		block = null; cur_scope = new AnalysisScope{Parent = cur_scope};
		ident_this = DeclareArgument("this", new TypeAnnotation(ObjectTypes.TYPE_MODULE));
		@params.Add(new Argument{Ident = ident_this, Option = null});
		
		Expect(12);
		name = t.val; 
		Expect(6);
		if (la.kind == 12) {
			ParamList(ref @params);
		}
		Expect(8);
		if (la.kind == 27) {
			Get();
			Type(out type);
		}
		Block(out block);
		func = Node.MakeFunc(name, @params, (Block)block, type);
		cur_scope = cur_scope.Parent;
		cur_scope.AddFunction(func as Function);
		
	}

	void ClassDecl(out Statement stmt) {
		Expression expr = null; var stmts = new List<Statement>(); List<Expression> decls = null;
		string name; var base_names = new List<string>(); Statement tmp = null; bool has_ctor = false;
		
		while (!(la.kind == 0 || la.kind == 22)) {SynErr(107); Get();}
		Expect(22);
		cur_scope = new AnalysisScope{Parent = cur_scope}; 
		Expect(12);
		name = t.val; 
		if (la.kind == 3) {
			Get();
			Expect(12);
			base_names.Add(t.val); 
			while (la.kind == 10) {
				Get();
				Expect(12);
				base_names.Add(t.val); 
			}
		}
		Expect(5);
		while (StartOf(5)) {
			if (la.kind == 23 || la.kind == 24) {
				if (la.kind == 23) {
					Get();
					expr = Node.MakeConstant(ObjectTypes._LABEL_PUBLIC, null); 
				} else {
					Get();
					expr = Node.MakeConstant(ObjectTypes._LABEL_PRIVATE, null); 
				}
				Expect(3);
				tmp = Node.MakeExprStmt(new List<Expression>{expr});
				stmts.Add(tmp);
				
			} else if (la.kind == 25) {
				ConstructorDecl(out tmp, name);
				stmts.Add(tmp);
				has_ctor = true;
				
			} else if (la.kind == 26) {
				MethodDecl(out tmp, name);
				stmts.Add(tmp); 
			} else if (la.kind == 12) {
				FieldDecl(out decls);
				Expect(4);
				tmp = Node.MakeExprStmt(decls);
				stmts.Add(tmp);
				
			} else {
				ClassDecl(out tmp);
				stmts.Add(tmp); 
			}
		}
		while (!(la.kind == 0 || la.kind == 9)) {SynErr(108); Get();}
		Expect(9);
		if(!has_ctor){		//ã³ã³ãã¤ã©ã¼å®ç¾©ã®ããã©ã«ãã³ã³ã¹ãã©ã¯ã¿ãå®ç¾©ãã
		stmts.Add(MakeDefaultCtor(name));
		}
		stmt = Node.MakeClassDef(name, base_names, stmts);
		cur_scope = cur_scope.Parent;
		cur_scope.AddType(new Identifier(name, new TypeAnnotation(ObjectTypes.TYPE_CLASS, name)));
		
	}

	void RequireStmt(out Statement stmt) {
		string module_name, alias = null; var requires = new List<Expression>();
		Expression tmp; Identifier module_symbol = null; string[] split_name;
		
		while (!(la.kind == 0 || la.kind == 20)) {SynErr(109); Get();}
		Expect(20);
		Module(out module_name);
		if (la.kind == 21) {
			Get();
			Expect(12);
			alias = t.val; 
		}
		tmp = Node.MakeRequireExpr(module_name, alias);
		requires.Add(tmp);
		if(alias == null){
		split_name = module_name.Split('.');
		foreach(var name in split_name){
		module_symbol = new Identifier(name, new TypeAnnotation(ObjectTypes.TYPE_MODULE, name));
		cur_scope.AddType(module_symbol);
		}
		}else{
		module_symbol = new Identifier(module_name, new TypeAnnotation(ObjectTypes.TYPE_MODULE, module_name), alias);
		cur_scope.AddAlias(module_symbol);
		alias = null;
		}
		
		while (la.kind == 10) {
			Get();
			Module(out module_name);
			if (la.kind == 21) {
				Get();
				Expect(12);
				alias = t.val; 
			}
			tmp = Node.MakeRequireExpr(module_name, alias);
			requires.Add(tmp);
			if(alias == null){
			split_name = module_name.Split('.');
			foreach(var name in split_name){
			module_symbol = new Identifier(name, new TypeAnnotation(ObjectTypes.TYPE_MODULE));
			cur_scope.AddType(module_symbol);
			}
			}else{
			module_symbol = new Identifier(module_name, new TypeAnnotation(ObjectTypes.TYPE_MODULE, module_name), alias);
			cur_scope.AddAlias(module_symbol);
			alias = null;
			}
			
		}
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(110); Get();}
		Expect(4);
		stmt = Node.MakeExprStmt(requires); 
	}

	void Module(out string name) {
		var sb = new StringBuilder(); 
		Expect(12);
		sb.Append(t.val); 
		while (la.kind == 11) {
			Get();
			sb.Append('.'); 
			Expect(12);
			sb.Append(t.val); 
		}
		name = sb.ToString(); 
	}

	void ConstructorDecl(out Statement func, string className) {
		Statement block = null; var @params = new List<Argument>();
		Identifier ident_this; Argument exs_this = null;
		
		while (!(la.kind == 0 || la.kind == 25)) {SynErr(111); Get();}
		Expect(25);
		cur_scope = new AnalysisScope{Parent = cur_scope};
		ident_this = DeclareArgument("this", new TypeAnnotation(ObjectTypes.INSTANCE, className));
		exs_this = new Argument{
		Ident = ident_this,
		Option = null
		};
		
		Expect(6);
		if (la.kind == 12) {
			ParamList(ref @params);
		}
		@params.Insert(0, exs_this); 
		Expect(8);
		Block(out block);
		func = Node.MakeFunc("constructor", @params, (Block)block, TypeAnnotation.VoidType.Clone(), true);
		cur_scope = cur_scope.Parent;
		
	}

	void MethodDecl(out Statement func, string className) {
		string name; var type = TypeAnnotation.VariantType.Clone(); Statement block = null;
		var @params = new List<Argument>(); Identifier ident_this; Argument exs_this = null;
		
		while (!(la.kind == 0 || la.kind == 26)) {SynErr(112); Get();}
		Expect(26);
		cur_scope = new AnalysisScope{Parent = cur_scope};
		ident_this = DeclareArgument("this", new TypeAnnotation(ObjectTypes.INSTANCE, className));
		exs_this = new Argument{
		Ident = ident_this,
		Option = null
		};
		
		Expect(12);
		name = t.val; 
		Expect(6);
		if (la.kind == 12) {
			ParamList(ref @params);
		}
		@params.Insert(0, exs_this); 
		Expect(8);
		if (la.kind == 27) {
			Get();
			Type(out type);
		}
		Block(out block);
		func = Node.MakeFunc(name, @params, (Block)block, type);
		cur_scope = cur_scope.Parent;
		
	}

	void FieldDecl(out List<Expression> outs ) {
		string name; TypeAnnotation type = TypeAnnotation.InferenceType.Clone(); Expression rhs = null;
		Identifier variable; outs = new List<Expression>();
		var vars = new List<Identifier>(); var exprs = new List<Expression>();
		
		Expect(12);
		name = t.val; 
		if (la.kind == 28) {
			Get();
			Type(out type);
		}
		if (la.kind == 29) {
			Get();
			if (NotFollowedByDoubleDots()) {
				CondExpr(out rhs);
			} else if (la.kind == 7) {
				IntSeqExpr(out rhs);
			} else SynErr(113);
		}
		while (la.kind == 10) {
			Get();
			variable = DeclareVariable(ref rhs, name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			
			Expect(12);
			name = t.val; 
			if (la.kind == 28) {
				Get();
				Type(out type);
			}
			if (la.kind == 29) {
				Get();
				if (NotFollowedByDoubleDots()) {
					CondExpr(out rhs);
				} else if (la.kind == 7) {
					IntSeqExpr(out rhs);
				} else SynErr(114);
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

	void Type(out TypeAnnotation type) {
		type = TypeAnnotation.InferenceType.Clone(); 
		switch (la.kind) {
		case 30: {
			Get();
			type.ObjType = ObjectTypes.INTEGER; 
			break;
		}
		case 31: {
			Get();
			type.ObjType = ObjectTypes.BOOL; 
			break;
		}
		case 32: {
			Get();
			type.ObjType = ObjectTypes.FLOAT; 
			break;
		}
		case 33: {
			Get();
			type.ObjType = ObjectTypes.RATIONAL; 
			break;
		}
		case 34: {
			Get();
			type.ObjType = ObjectTypes.BIGINT; 
			break;
		}
		case 35: {
			Get();
			type.ObjType = ObjectTypes.STRING; 
			break;
		}
		case 36: {
			Get();
			type.ObjType = ObjectTypes.BYTEARRAY; 
			break;
		}
		case 37: {
			Get();
			type.ObjType = ObjectTypes.VAR; 
			break;
		}
		case 38: {
			Get();
			type.ObjType = ObjectTypes.TUPLE; 
			break;
		}
		case 39: {
			Get();
			type.ObjType = ObjectTypes.LIST; 
			break;
		}
		case 40: {
			Get();
			type.ObjType = ObjectTypes.DICT; 
			break;
		}
		case 41: {
			Get();
			type.ObjType = ObjectTypes.EXPRESSION; 
			break;
		}
		case 42: {
			Get();
			type.ObjType = ObjectTypes.FUNCTION; 
			break;
		}
		case 43: {
			Get();
			type.ObjType = ObjectTypes.SEQ; 
			break;
		}
		case 44: {
			Get();
			type.ObjType = ObjectTypes.UNDEF; 
			break;
		}
		case 12: {
			Get();
			type.ObjType = ObjectTypes.INSTANCE; type.TypeName = t.val; 
			break;
		}
		default: SynErr(115); break;
		}
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; 
		OrTest(out expr);
		if (la.kind == 73) {
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
		if(start == null) start = CreateConstant(ObjectTypes.INTEGER);
		if(step == null) step = Node.MakeConstant(ObjectTypes.INTEGER, 1);
		expr = Node.MakeIntSeq(start, end, step);
		
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(7)) {
			SimpleStmt(out stmt);
		} else if (StartOf(8)) {
			CompoundStmt(out stmt);
		} else SynErr(116);
	}

	void SimpleStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == _lcurly) {
			Block(out stmt);
		} else if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 45) {
			PrintStmt(out stmt);
		} else if (la.kind == 46) {
			ReturnStmt(out stmt);
		} else if (la.kind == 47) {
			BreakStmt(out stmt);
		} else if (la.kind == 49) {
			ContinueStmt(out stmt);
		} else if (la.kind == 50) {
			ThrowStmt(out stmt);
		} else if (la.kind == 51) {
			YieldStmt(out stmt);
		} else SynErr(117);
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; 
		switch (la.kind) {
		case 62: {
			while (!(la.kind == 0 || la.kind == 62)) {SynErr(118); Get();}
			IfStmt(out stmt);
			break;
		}
		case 64: {
			WhileStmt(out stmt);
			break;
		}
		case 18: {
			ForStmt(out stmt);
			break;
		}
		case 65: {
			SwitchStmt(out stmt);
			break;
		}
		case 26: {
			FuncDecl(out stmt);
			break;
		}
		case 69: {
			WithStmt(out stmt);
			break;
		}
		case 70: {
			TryStmt(out stmt);
			break;
		}
		default: SynErr(119); break;
		}
	}

	void PrintStmt(out Statement stmt) {
		List<Expression> exprs = null; bool trailing_comma = false; 
		Expect(45);
		if (StartOf(3)) {
			RValueList(out exprs);
		}
		if (la.kind == 10) {
			Get();
		}
		trailing_comma = true; 
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(120); Get();}
		Expect(4);
		stmt = Node.MakePrintStmt(exprs, trailing_comma); 
	}

	void ReturnStmt(out Statement stmt) {
		List<Expression> target_list = new List<Expression>(); 
		Expect(46);
		if (StartOf(3)) {
			RValueList(out target_list);
		}
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(121); Get();}
		Expect(4);
		stmt = Node.MakeReturnStmt(target_list); 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; 
		Expect(47);
		if (la.kind == 48) {
			Get();
			Expect(13);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(122); Get();}
		Expect(4);
		stmt = MakeBreakStatement(count); 
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; 
		Expect(49);
		if (la.kind == 48) {
			Get();
			Expect(13);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(123); Get();}
		Expect(4);
		stmt = MakeContinueStatement(count); 
	}

	void ThrowStmt(out Statement stmt) {
		Expression expr; 
		Expect(50);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(124); Get();}
		Expect(4);
		stmt = Node.MakeThrowStmt(expr); 
	}

	void YieldStmt(out Statement stmt) {
		Expression expr; 
		Expect(51);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(125); Get();}
		Expect(4);
		stmt = Node.MakeYieldStmt(expr); 
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
		case 52: {
			Get();
			type = OperatorType.PLUS; 
			break;
		}
		case 53: {
			Get();
			type = OperatorType.MINUS; 
			break;
		}
		case 54: {
			Get();
			type = OperatorType.TIMES; 
			break;
		}
		case 55: {
			Get();
			type = OperatorType.DIV; 
			break;
		}
		case 56: {
			Get();
			type = OperatorType.POWER; 
			break;
		}
		case 57: {
			Get();
			type = OperatorType.MOD; 
			break;
		}
		case 58: {
			Get();
			type = OperatorType.BIT_AND; 
			break;
		}
		case 59: {
			Get();
			type = OperatorType.BIT_OR; 
			break;
		}
		case 60: {
			Get();
			type = OperatorType.BIT_LSHIFT; 
			break;
		}
		case 61: {
			Get();
			type = OperatorType.BIT_RSHIFT; 
			break;
		}
		default: SynErr(126); break;
		}
	}

	void VarDecl(out List<Expression> outs ) {
		string name; var type = TypeAnnotation.InferenceType.Clone(); Expression rhs = null;
		Identifier variable; outs = new List<Expression>();
		var vars = new List<Identifier>(); var exprs = new List<Expression>();
		
		Expect(68);
		Expect(12);
		name = t.val; 
		if (la.kind == 28) {
			Get();
			Type(out type);
		}
		if (la.kind == 29) {
			Get();
			if (NotFollowedByDoubleDots()) {
				CondExpr(out rhs);
			} else if (la.kind == 7) {
				IntSeqExpr(out rhs);
			} else SynErr(127);
		}
		while (la.kind == 10) {
			Get();
			variable = DeclareVariable(ref rhs, name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			
			Expect(12);
			name = t.val; 
			if (la.kind == 28) {
				Get();
				Type(out type);
			}
			if (la.kind == 29) {
				Get();
				if (NotFollowedByDoubleDots()) {
					CondExpr(out rhs);
				} else if (la.kind == 7) {
					IntSeqExpr(out rhs);
				} else SynErr(128);
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
		Expect(62);
		Expect(6);
		CondExpr(out tmp);
		Expect(8);
		Stmt(out true_block);
		if (la.kind == 63) {
			Get();
			Stmt(out false_block);
		}
		stmt = Node.MakeIfStmt(tmp, true_block, false_block); 
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; Statement body = null; WhileStatement tmp; 
		Expect(64);
		tmp = Node.MakeWhileStmt();
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
		
		Expect(18);
		tmp = Node.MakeForStmt();
		Parser.breakables.Add(tmp);
		
		Expect(6);
		if (la.kind == 68) {
			LValueListWithLet(out target_list);
			if(target_list != null) has_let = true; 
		} else if (StartOf(9)) {
			LValueList(out target_list);
		} else SynErr(129);
		Expect(17);
		if (NotFollowedByDoubleDots()) {
			CondExpr(out rvalue);
		} else if (la.kind == 7) {
			IntSeqExpr(out rvalue);
		} else SynErr(130);
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
		Expect(65);
		Expect(6);
		CondExpr(out target);
		Expect(8);
		Expect(5);
		CaseClauseList(out cases);
		Expect(9);
		stmt = Node.MakeSwitchStmt(target, cases); 
	}

	void WithStmt(out Statement stmt) {
		Statement block = null; 
		Expect(69);
		Expect(6);
		Expect(8);
		Block(out block);
		stmt = null; 
	}

	void TryStmt(out Statement stmt) {
		Statement body, catch_body = null, finally_body = null; List<CatchClause> catches = null;
		TypeAnnotation excp_type = null; Identifier catch_ident = null; string name = null;
		
		Expect(70);
		Block(out body);
		while (la.kind == 71) {
			Get();
			if(catches == null) catches = new List<CatchClause>(); 
			Expect(6);
			Expect(12);
			name = t.val; 
			Expect(28);
			Type(out excp_type);
			Expect(8);
			catch_ident = new Identifier(name, excp_type); 
			Block(out catch_body);
			catches.Add(new CatchClause{Body = (Block)catch_body, Catcher = catch_ident}); 
		}
		if (la.kind == 72) {
			Get();
			Block(out finally_body);
		}
		stmt = Node.MakeTryStmt((Block)body, catches, (Block)finally_body); 
	}

	void LValueListWithLet(out List<Expression> targets ) {
		Expression tmp; Identifier ident = null; var type = TypeAnnotation.InferenceType; 
		targets = null; 
		Expect(68);
		targets = new List<Expression>(); 
		AddOpe(out tmp);
		if (la.kind == 28) {
			Get();
			Type(out type);
		}
		ident = tmp as Identifier;
		if(ident == null) SemErr("Expected a lvalue!");
		ident.ParamType = type;
		cur_scope.AddLocal(ref ident);
		targets.Add(ident);
		
		while (WeakSeparator(10,9,10) ) {
			AddOpe(out tmp);
			if (la.kind == 28) {
				Get();
				Type(out type);
			}
			ident = tmp as Identifier;
			if(ident == null) SemErr("Expected a lvalue!");
			ident.ParamType = type;
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
		clauses.Add(Node.MakeCaseClause(label_list, inner)); 
		while (la.kind == 66) {
			CaseLabelList(out label_list);
			Stmt(out inner);
			clauses.Add(Node.MakeCaseClause(label_list, inner)); 
		}
	}

	void CaseLabelList(out List<Expression> label_list ) {
		label_list = new List<Expression>(); Expression tmp; 
		CaseLabel(out tmp);
		label_list.Add(tmp); 
		while (la.kind == 66) {
			CaseLabel(out tmp);
			label_list.Add(tmp); 
		}
	}

	void CaseLabel(out Expression expr) {
		expr = null; 
		Expect(66);
		if (StartOf(11)) {
			Literal(out expr);
		} else if (la.kind == 7) {
			IntSeqExpr(out expr);
		} else if (la.kind == 67) {
			Get();
			expr = Node.MakeConstant(ObjectTypes._CASE_DEFAULT, "default"); 
		} else SynErr(131);
		while (!(la.kind == 0 || la.kind == 3)) {SynErr(132); Get();}
		Expect(3);
	}

	void Literal(out Expression expr) {
		expr = null; string tmp; bool has_suffix = false; 
		switch (la.kind) {
		case 13: {
			Get();
			tmp = t.val; 
			if (la.kind == 95 || la.kind == 96) {
				if (la.kind == 95) {
					Get();
					has_suffix = true; 
				} else {
					Get();
					has_suffix = true; 
				}
			}
			if(has_suffix)
			expr = Node.MakeConstant(ObjectTypes.BIGINT, BigInteger.Parse(tmp));
			else
			expr = Node.MakeConstant(ObjectTypes.INTEGER, Convert.ToInt32(tmp));
			
			break;
		}
		case 15: {
			Get();
			expr = Node.MakeConstant(ObjectTypes.INTEGER, Convert.ToInt32(t.val, 16)); 
			break;
		}
		case 14: {
			Get();
			expr = Node.MakeConstant(ObjectTypes.FLOAT, Convert.ToDouble(t.val)); 
			break;
		}
		case 16: {
			Get();
			tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = Node.MakeConstant(ObjectTypes.STRING, tmp);
			
			break;
		}
		case 97: case 98: {
			if (la.kind == 97) {
				Get();
			} else {
				Get();
			}
			expr = Node.MakeConstant(ObjectTypes.BOOL, Convert.ToBoolean(t.val)); 
			break;
		}
		case 99: {
			Get();
			expr = Node.MakeConstant(ObjectTypes.NULL, null); 
			break;
		}
		default: SynErr(133); break;
		}
	}

	void AddOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		while (la.kind == 88 || la.kind == 89) {
			if (la.kind == 88) {
				Get();
				type = OperatorType.PLUS; 
			} else {
				Get();
				type = OperatorType.MINUS; 
			}
			Term(out rhs);
			expr = Node.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Argument(out Argument arg) {
		string name; Expression default_val = null; var type = TypeAnnotation.VariantType.Clone(); 
		Expect(12);
		name = t.val; 
		if (la.kind == 28) {
			Get();
			Type(out type);
		}
		if (la.kind == 29) {
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
		while (la.kind == 74) {
			Get();
			AndTest(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.OR, expr, rhs); 
		}
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		NotTest(out expr);
		while (la.kind == 75) {
			Get();
			NotTest(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.AND, expr, rhs); 
		}
	}

	void NotTest(out Expression expr) {
		Expression term; expr = null; 
		if (la.kind == 76) {
			Get();
			NotTest(out term);
			expr = Node.MakeUnaryExpr(OperatorType.NOT, term); 
		} else if (StartOf(9)) {
			Comparison(out expr);
		} else SynErr(134);
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		BitOr(out expr);
		type = OperatorType.EQUAL; 
		if (StartOf(12)) {
			switch (la.kind) {
			case 77: {
				Get();
				type = OperatorType.EQUAL; 
				break;
			}
			case 78: {
				Get();
				type = OperatorType.NOTEQ; 
				break;
			}
			case 79: {
				Get();
				type = OperatorType.LESS; 
				break;
			}
			case 80: {
				Get();
				type = OperatorType.GREAT; 
				break;
			}
			case 81: {
				Get();
				type = OperatorType.LESE; 
				break;
			}
			case 82: {
				Get();
				type = OperatorType.GRTE; 
				break;
			}
			}
			BitOr(out rhs);
			expr = Node.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void BitOr(out Expression expr) {
		Expression rhs; 
		BitXor(out expr);
		while (la.kind == 83) {
			Get();
			BitXor(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.BIT_OR, expr, rhs); 
		}
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		while (la.kind == 84) {
			Get();
			BitAnd(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.BIT_XOR, expr, rhs); 
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOpe(out expr);
		while (la.kind == 85) {
			Get();
			ShiftOpe(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.BIT_AND, expr, rhs); 
		}
	}

	void ShiftOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOpe(out expr);
		while (la.kind == 86 || la.kind == 87) {
			if (la.kind == 86) {
				Get();
				type = OperatorType.BIT_LSHIFT; 
			} else {
				Get();
				type = OperatorType.BIT_RSHIFT; 
			}
			AddOpe(out rhs);
			expr = Node.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		Factor(out expr);
		while (la.kind == 90 || la.kind == 91 || la.kind == 92) {
			if (la.kind == 90) {
				Get();
				type = OperatorType.TIMES; 
			} else if (la.kind == 91) {
				Get();
				type = OperatorType.DIV; 
			} else {
				Get();
				type = OperatorType.MOD; 
			}
			Factor(out rhs);
			expr = Node.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Factor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; 
		if (StartOf(13)) {
			PowerOpe(out expr);
		} else if (la.kind == 88 || la.kind == 89) {
			if (la.kind == 89) {
				Get();
				type = OperatorType.MINUS; 
			} else {
				Get();
				type = OperatorType.PLUS; 
			}
			Factor(out factor);
			expr = Node.MakeUnaryExpr(type, factor); 
		} else SynErr(135);
	}

	void PowerOpe(out Expression expr) {
		Expression rhs; 
		Primary(out expr);
		if (la.kind == 93) {
			Get();
			Factor(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.POWER, expr, rhs); 
		}
	}

	void Primary(out Expression expr) {
		string name = null; expr = null; 
		if (StartOf(14)) {
			if (IdentIsNotCallable()) {
				Atom(out expr);
			} else {
				Get();
				name = t.val; 
			}
			while (la.kind == 6 || la.kind == 7 || la.kind == 11) {
				Trailer(ref expr, name);
			}
		} else if (la.kind == 94) {
			NewExpression(out expr);
		} else SynErr(136);
	}

	void Atom(out Expression expr) {
		string name; expr = null; 
		if (la.kind == 12) {
			Get();
			name = t.val;
			if(ExpectIdentIsTypeName()){
			expr = cur_scope.GetType(name);
			if(expr != null) return;
			}
			expr = cur_scope.GetVariable(name, !ignore_parent_scope);
			if(expr == null){
			expr = new Identifier(name);
			if(ignore_parent_scope){	//for the comprehension expression
			var ident = (Identifier)expr;
			ident.ParamType = TypeAnnotation.InferenceType.Clone();
			cur_scope.AddLocal(ref ident);
			expr = ident;
			}
			}
			
		} else if (StartOf(11)) {
			Literal(out expr);
		} else if (la.kind == 6) {
			Get();
			if (StartOf(3)) {
				SequenceMaker(out expr, ObjectTypes.TUPLE);
			}
			while (!(la.kind == 0 || la.kind == 8)) {SynErr(137); Get();}
			Expect(8);
			if(expr == null)
			expr = Node.MakeObjInitializer(ObjectTypes.TUPLE, new List<Expression>());
			
		} else if (la.kind == 7) {
			Get();
			if (StartOf(3)) {
				SequenceMaker(out expr, ObjectTypes.LIST);
			}
			while (!(la.kind == 0 || la.kind == 2)) {SynErr(138); Get();}
			Expect(2);
			if(expr == null)
			expr = Node.MakeObjInitializer(ObjectTypes.LIST, new List<Expression>());
			
		} else if (la.kind == 5) {
			Get();
			if (StartOf(3)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 9)) {SynErr(139); Get();}
			Expect(9);
			if(expr == null) expr = Node.MakeObjInitializer(ObjectTypes.DICT, new List<Expression>()); 
		} else SynErr(140);
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
			func = cur_scope.GetFunction(name);
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
			
		} else if (la.kind == 11) {
			Get();
			Expect(12);
			subscript = new Identifier(t.val, new TypeAnnotation(ObjectTypes._SUBSCRIPT));
			expr = new MemberReference{
			Parent = expr,
			Subscription = subscript
			};
			
		} else SynErr(141);
	}

	void NewExpression(out Expression expr) {
		Identifier ident = null; var args = new List<Expression>(); 
		Expect(94);
		TypeName(out expr);
		while (la.kind == 11) {
			Get();
			Expect(12);
			ident = new Identifier(t.val, new TypeAnnotation(ObjectTypes._SUBSCRIPT));
			expr = new MemberReference{
			Parent = expr,
			Subscription = ident
			};  
			
		}
		Expect(6);
		if (StartOf(3)) {
			ArgList(ref args);
		}
		Expect(8);
		expr = Node.MakeNewExpr(expr, args); 
	}

	void TypeName(out Expression expr) {
		string name; 
		Expect(12);
		name = t.val;
		expr = cur_scope.GetType(name);
		if(expr == null) SemErr(string.Format("Expected a type name but instead saw \"{0}\".", name));
		
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
		} else SynErr(142);
	}

	void SequenceMaker(out Expression expr, ObjectTypes ObjType) {
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
			expr = Node.MakeObjInitializer(ObjType, list); 
		} else if (la.kind == 5) {
			Get();
			cur_scope = new AnalysisScope{Parent = cur_scope};
			ignore_parent_scope = true;
			
			CondExpr(out tmp);
			CompFor(out comprehen);
			Expect(9);
			expr = Node.MakeComp(tmp, (ComprehensionFor)comprehen, ObjType);
			cur_scope = cur_scope.Parent;
			ignore_parent_scope = false;
			
		} else SynErr(143);
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
		if(list.Count > 0) expr = Node.MakeObjInitializer(ObjectTypes.DICT, list); 
	}

	void CompFor(out ComprehensionIter expr) {
		Expression rvalue = null; ComprehensionIter body = null; List<Expression> target_list; 
		Expect(18);
		ignore_parent_scope = true; 
		LValueList(out target_list);
		foreach(var target in target_list){
		var ident = target as Identifier;
		if(ident == null) SemErr("Expected a lvalue!");
		ident.ParamType = TypeAnnotation.InferenceType.Clone();
		if(!cur_scope.ContainsIn(ident.Name))
			cur_scope.AddLocal(ref ident);
		}
		ignore_parent_scope = false;
		
		Expect(17);
		if (NotFollowedByDoubleDots()) {
			CondExpr(out rvalue);
		} else if (la.kind == 7) {
			IntSeqExpr(out rvalue);
		} else SynErr(144);
		if (la.kind == 18 || la.kind == 62) {
			CompIter(out body);
		}
		expr = Node.MakeCompFor(target_list, rvalue, body); 
	}

	void CompIter(out ComprehensionIter expr) {
		expr = null; 
		if (la.kind == 18) {
			CompFor(out expr);
		} else if (la.kind == 62) {
			CompIf(out expr);
		} else SynErr(145);
	}

	void CompIf(out ComprehensionIter expr) {
		Expression tmp; ComprehensionIter body = null; 
		Expect(62);
		OrTest(out tmp);
		if (la.kind == 18 || la.kind == 62) {
			CompIter(out body);
		}
		expr = Node.MakeCompIf(tmp, body); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Expresso();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,T,T, T,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,T,T,T, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,T, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,T,T,T, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,T,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,T,T,T, x,x,x,x, x,x,x,x, x,x,T,x, T,T,x,x, T,T,T,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,T,T,T, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,T,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,T,T, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x}

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
			case 11: s = "dot expected"; break;
			case 12: s = "ident expected"; break;
			case 13: s = "integer expected"; break;
			case 14: s = "float expected"; break;
			case 15: s = "hex_digit expected"; break;
			case 16: s = "string_literal expected"; break;
			case 17: s = "keyword_in expected"; break;
			case 18: s = "keyword_for expected"; break;
			case 19: s = "\"export\" expected"; break;
			case 20: s = "\"require\" expected"; break;
			case 21: s = "\"as\" expected"; break;
			case 22: s = "\"class\" expected"; break;
			case 23: s = "\"public\" expected"; break;
			case 24: s = "\"private\" expected"; break;
			case 25: s = "\"constructor\" expected"; break;
			case 26: s = "\"def\" expected"; break;
			case 27: s = "\"->\" expected"; break;
			case 28: s = "\"(-\" expected"; break;
			case 29: s = "\"=\" expected"; break;
			case 30: s = "\"int\" expected"; break;
			case 31: s = "\"bool\" expected"; break;
			case 32: s = "\"float\" expected"; break;
			case 33: s = "\"rational\" expected"; break;
			case 34: s = "\"bigint\" expected"; break;
			case 35: s = "\"string\" expected"; break;
			case 36: s = "\"bytearray\" expected"; break;
			case 37: s = "\"var\" expected"; break;
			case 38: s = "\"tuple\" expected"; break;
			case 39: s = "\"list\" expected"; break;
			case 40: s = "\"dictionary\" expected"; break;
			case 41: s = "\"expression\" expected"; break;
			case 42: s = "\"function\" expected"; break;
			case 43: s = "\"intseq\" expected"; break;
			case 44: s = "\"void\" expected"; break;
			case 45: s = "\"print\" expected"; break;
			case 46: s = "\"return\" expected"; break;
			case 47: s = "\"break\" expected"; break;
			case 48: s = "\"upto\" expected"; break;
			case 49: s = "\"continue\" expected"; break;
			case 50: s = "\"throw\" expected"; break;
			case 51: s = "\"yield\" expected"; break;
			case 52: s = "\"+=\" expected"; break;
			case 53: s = "\"-=\" expected"; break;
			case 54: s = "\"*=\" expected"; break;
			case 55: s = "\"/=\" expected"; break;
			case 56: s = "\"**=\" expected"; break;
			case 57: s = "\"%=\" expected"; break;
			case 58: s = "\"&=\" expected"; break;
			case 59: s = "\"|=\" expected"; break;
			case 60: s = "\"<<=\" expected"; break;
			case 61: s = "\">>=\" expected"; break;
			case 62: s = "\"if\" expected"; break;
			case 63: s = "\"else\" expected"; break;
			case 64: s = "\"while\" expected"; break;
			case 65: s = "\"switch\" expected"; break;
			case 66: s = "\"case\" expected"; break;
			case 67: s = "\"default\" expected"; break;
			case 68: s = "\"let\" expected"; break;
			case 69: s = "\"with\" expected"; break;
			case 70: s = "\"try\" expected"; break;
			case 71: s = "\"catch\" expected"; break;
			case 72: s = "\"finally\" expected"; break;
			case 73: s = "\"?\" expected"; break;
			case 74: s = "\"||\" expected"; break;
			case 75: s = "\"&&\" expected"; break;
			case 76: s = "\"!\" expected"; break;
			case 77: s = "\"==\" expected"; break;
			case 78: s = "\"!=\" expected"; break;
			case 79: s = "\"<\" expected"; break;
			case 80: s = "\">\" expected"; break;
			case 81: s = "\"<=\" expected"; break;
			case 82: s = "\">=\" expected"; break;
			case 83: s = "\"|\" expected"; break;
			case 84: s = "\"^\" expected"; break;
			case 85: s = "\"&\" expected"; break;
			case 86: s = "\"<<\" expected"; break;
			case 87: s = "\">>\" expected"; break;
			case 88: s = "\"+\" expected"; break;
			case 89: s = "\"-\" expected"; break;
			case 90: s = "\"*\" expected"; break;
			case 91: s = "\"/\" expected"; break;
			case 92: s = "\"%\" expected"; break;
			case 93: s = "\"**\" expected"; break;
			case 94: s = "\"new\" expected"; break;
			case 95: s = "\"l\" expected"; break;
			case 96: s = "\"L\" expected"; break;
			case 97: s = "\"true\" expected"; break;
			case 98: s = "\"false\" expected"; break;
			case 99: s = "\"null\" expected"; break;
			case 100: s = "??? expected"; break;
			case 101: s = "invalid ModuleBody"; break;
			case 102: s = "invalid ModuleBody"; break;
			case 103: s = "invalid ExprStmt"; break;
			case 104: s = "invalid ExprStmt"; break;
			case 105: s = "this symbol not expected in ExprStmt"; break;
			case 106: s = "this symbol not expected in FuncDecl"; break;
			case 107: s = "this symbol not expected in ClassDecl"; break;
			case 108: s = "this symbol not expected in ClassDecl"; break;
			case 109: s = "this symbol not expected in RequireStmt"; break;
			case 110: s = "this symbol not expected in RequireStmt"; break;
			case 111: s = "this symbol not expected in ConstructorDecl"; break;
			case 112: s = "this symbol not expected in MethodDecl"; break;
			case 113: s = "invalid FieldDecl"; break;
			case 114: s = "invalid FieldDecl"; break;
			case 115: s = "invalid Type"; break;
			case 116: s = "invalid Stmt"; break;
			case 117: s = "invalid SimpleStmt"; break;
			case 118: s = "this symbol not expected in CompoundStmt"; break;
			case 119: s = "invalid CompoundStmt"; break;
			case 120: s = "this symbol not expected in PrintStmt"; break;
			case 121: s = "this symbol not expected in ReturnStmt"; break;
			case 122: s = "this symbol not expected in BreakStmt"; break;
			case 123: s = "this symbol not expected in ContinueStmt"; break;
			case 124: s = "this symbol not expected in ThrowStmt"; break;
			case 125: s = "this symbol not expected in YieldStmt"; break;
			case 126: s = "invalid AugAssignOpe"; break;
			case 127: s = "invalid VarDecl"; break;
			case 128: s = "invalid VarDecl"; break;
			case 129: s = "invalid ForStmt"; break;
			case 130: s = "invalid ForStmt"; break;
			case 131: s = "invalid CaseLabel"; break;
			case 132: s = "this symbol not expected in CaseLabel"; break;
			case 133: s = "invalid Literal"; break;
			case 134: s = "invalid NotTest"; break;
			case 135: s = "invalid Factor"; break;
			case 136: s = "invalid Primary"; break;
			case 137: s = "this symbol not expected in Atom"; break;
			case 138: s = "this symbol not expected in Atom"; break;
			case 139: s = "this symbol not expected in Atom"; break;
			case 140: s = "invalid Atom"; break;
			case 141: s = "invalid Trailer"; break;
			case 142: s = "invalid Subscript"; break;
			case 143: s = "invalid SequenceMaker"; break;
			case 144: s = "invalid CompFor"; break;
			case 145: s = "invalid CompIter"; break;

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
