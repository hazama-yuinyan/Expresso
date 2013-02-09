using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using Expresso.Ast;
using Expresso.Builtins;
using Expresso.Compiler.Meta;
using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Utils;





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
	public const int maxT = 101;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

internal ScopeStatement cur_scope = null;		//the current scope of variables
	static private List<BreakableStatement> breakables = new List<BreakableStatement>();	//the current parent breakables hierarchy
	public string ParsingFileName{get; set;}
	public ExpressoAst TopmostAst{get; private set;}	//the top-level AST the parser is parsing
	
	///<summary>
	/// Parser Implementation details:
	/// 	During parsing we'll resolve break and continue statements.(Find which a break or continue statement would have its effect
	/// 	on which a loop statement) And in post-parse process, do flow analysis and type validity check, including local name bindings.
	///		Note that the identifiers are just placeholders until after doing name binding. 
	/// 	(Thus referecing them cause runtime exceptions)
	///</summary>
	Parser()
	{
		//Add built-in functions
		/*FunctionDefinition[] native_funcs = {
			new NativeLambdaUnary("abs", ImplementationHelpers.MakeArg(new Identifier("val", new TypeAnnotation(ObjectTypes.VAR), null, 0)), ExpressoFunctions.Abs),
			new NativeLambdaUnary("sqrt", ImplementationHelpers.MakeArg(new Identifier("val", new TypeAnnotation(ObjectTypes.VAR), null, 0)), ExpressoFunctions.Sqrt),
			new NativeLambdaUnary("toInt", ImplementationHelpers.MakeArg(new Identifier("val", new TypeAnnotation(ObjectTypes.VAR), null, 0)), ExpressoFunctions.ToInt)
		};
		foreach(var tmp in native_funcs)
			cur_scope.AddFunction(tmp);*/
	}
	
	Constant CreateConstant(ObjectTypes type)
	{
		Constant result = null;
		
		switch(type){
		case ObjectTypes.INTEGER:
			result = Node.MakeConstant(type, 0);
			break;
			
		case ObjectTypes.BOOL:
			result = Node.MakeConstant(type, false);
			break;
			
		case ObjectTypes.FLOAT:
			result = Node.MakeConstant(type, 0.0);
			break;
			
		case ObjectTypes.STRING:
			result = Node.MakeConstant(type, "");
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
	
	bool IsSequenceInitializer()
	{
		Token x = la;
		if(x.kind != _lcurly) return true;
		scanner.ResetPeek();
		while(x.kind != 0 && x.kind != _keyword_for && x.kind != _rcurly) x = scanner.Peek();
		return x.kind != _keyword_for;
	}
	
	static BreakStatement MakeBreakStatement(int count)
	{
		var tmp = new List<BreakableStatement>();
		for(int len = Parser.breakables.Count, i = 0, j = count; j > 0; ++i){
			var enclosing = Parser.breakables[len - 1 - i];
			tmp.Add(enclosing);
			if(enclosing.Type == NodeType.WhileStatement || enclosing.Type == NodeType.ForStatement) --j;
		}
		return Node.MakeBreakStmt(count, tmp);
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
		return Node.MakeContinueStmt(count, tmp);
	}
	
	Statement MakeDefaultCtor(string className)
	{
		var arg_this = Node.MakeArg("this", new TypeAnnotation(ObjectTypes.INSTANCE, className));
		var @params = new List<Argument>{arg_this};
		var ctor = Node.MakeFunc("constructor", @params, new Block(), TypeAnnotation.VoidType.Clone(), true);
		return ctor;
	}
	
	public void SemanticError(string format, params object[] args)
	{
		//Convenient method for printing a semantic error with a format string
		SemErr(string.Format(format, args));
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
		ExpressoAst module_decl = null; 
		ModuleBody(out module_decl);
		ExpressoNameBinder.BindAst(module_decl, this); //Here's the start of post-parse processing
		this.TopmostAst = module_decl;	//Currently there is not so much code out there, though...
		
	}

	void ModuleBody(out ExpressoAst decl) {
		var decls = new List<Statement>(); var export_map = new List<bool>(); bool has_main = false;
		bool has_export = false; List<Statement> prog_defs = null; Statement stmt = null;
		
		if (la.kind == 20) {
			ProgramDefinition(out prog_defs);
			if(prog_defs !=null) decls.AddRange(prog_defs); 
		}
		if (la.kind == 19) {
			Get();
			has_export = true; 
		}
		if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 27) {
			FuncDecl(out stmt);
		} else if (la.kind == 22) {
			ClassDecl(out stmt);
		} else SynErr(102);
		if(stmt is FunctionDefinition && ((FunctionDefinition)stmt).Name == "main")
		has_main = true;
		
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
			} else if (la.kind == 27) {
				FuncDecl(out stmt);
			} else if (la.kind == 22) {
				ClassDecl(out stmt);
			} else SynErr(103);
			if(stmt is FunctionDefinition && ((FunctionDefinition)stmt).Name == "main")
			has_main = true;
			
			decls.Add(stmt);
			export_map.Add(has_export);
			has_export = false;
			
		}
		var module_name = has_main ? "main" : ParsingFileName;
		decl = Node.MakeModuleDef(module_name, decls, export_map);
		
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
		List<Expression> lvalues = null; var targets = new List<SequenceExpression>();
		SequenceExpression seq = null;
		stmt = null; OperatorType op_type = OperatorType.NONE;
		
		if (la.kind == 69) {
			VarDecl(out lvalues);
		} else if (StartOf(3)) {
			LValueList(out seq);
			targets.Add(seq); 
			while (StartOf(4)) {
				if (StartOf(5)) {
					AugAssignOpe(ref op_type);
				} else {
					Get();
				}
				RValueList(out seq);
				targets.Add(seq); 
			}
			seq = targets[targets.Count - 1];
			targets.RemoveAt(targets.Count - 1);
			if(op_type != OperatorType.NONE && targets.Count != 1)
			SemErr("An augumented assignment can't have multiple left-hand-side.");
			if(op_type != OperatorType.NONE &&		//See if it is an augumented assignment and
			targets[0].Count != seq.Count)		//both sides have the same number of items
			SemErr("An augumented assignment must have both sides balanced.");
			
			if(op_type != OperatorType.NONE)
			stmt = Node.MakeAugumentedAssignment(targets[0], seq, op_type);
			else
			stmt = Node.MakeAssignment(targets.Cast<Expression>(), seq);
			
		} else SynErr(104);
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(105); Get();}
		Expect(4);
		if(stmt == null)
		stmt = Node.MakeExprStmt(lvalues);
		
	}

	void FuncDecl(out Statement func) {
		string name; var type = TypeAnnotation.VariantType.Clone(); Statement block;
		var @params = new List<Argument>(); Argument arg_this = null;
		
		while (!(la.kind == 0 || la.kind == 27)) {SynErr(106); Get();}
		Expect(27);
		block = null;
		arg_this = Node.MakeArg("this", new TypeAnnotation(ObjectTypes.TYPE_MODULE));
		@params.Add(arg_this);
		
		Expect(12);
		name = t.val; 
		Expect(6);
		if (la.kind == 12) {
			ParamList(ref @params);
		}
		Expect(8);
		if (la.kind == 28) {
			Get();
			Type(out type);
		}
		Block(out block);
		func = Node.MakeFunc(name, @params, (Block)block, type); 
	}

	void ClassDecl(out Statement stmt) {
		Expression expr = null; var stmts = new List<Statement>(); List<Expression> decls = null;
		string name; var base_names = new List<string>(); Statement tmp = null; bool has_ctor = false;
		
		while (!(la.kind == 0 || la.kind == 22)) {SynErr(107); Get();}
		Expect(22);
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
		while (StartOf(6)) {
			if (la.kind == 23 || la.kind == 24 || la.kind == 25) {
				if (la.kind == 23) {
					Get();
					expr = Node.MakeConstant(ObjectTypes._LABEL_PUBLIC, null); 
				} else if (la.kind == 24) {
					Get();
					expr = Node.MakeConstant(ObjectTypes._LABEL_PROTECTED, null); 
				} else {
					Get();
					expr = Node.MakeConstant(ObjectTypes._LABEL_PRIVATE, null); 
				}
				Expect(3);
				tmp = Node.MakeExprStmt(new List<Expression>{expr});
				stmts.Add(tmp);
				
			} else if (la.kind == 26) {
				ConstructorDecl(out tmp, name);
				stmts.Add(tmp);
				has_ctor = true;
				
			} else if (la.kind == 27) {
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
		if(!has_ctor){		//Define the default constructor
		stmts.Add(MakeDefaultCtor(name));
		}
		stmt = Node.MakeClassDef(name, base_names, stmts);
		
	}

	void RequireStmt(out Statement stmt) {
		string module_name, alias = null; var module_names = new List<string>();
		var aliases = new List<string>();
		
		while (!(la.kind == 0 || la.kind == 20)) {SynErr(109); Get();}
		Expect(20);
		ModuleName(out module_name);
		if (la.kind == 21) {
			Get();
			Expect(12);
			alias = t.val; 
		}
		module_names.Add(module_name);
		aliases.Add(alias);
		
		while (la.kind == 10) {
			Get();
			ModuleName(out module_name);
			if (la.kind == 21) {
				Get();
				Expect(12);
				alias = t.val; 
			}
			module_names.Add(module_name);
			aliases.Add(alias);
			
		}
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(110); Get();}
		Expect(4);
		stmt = Node.MakeRequireStmt(module_names, aliases); 
	}

	void ModuleName(out string name) {
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
		Argument arg_this = null;
		
		while (!(la.kind == 0 || la.kind == 26)) {SynErr(111); Get();}
		Expect(26);
		arg_this = Node.MakeArg("this", new TypeAnnotation(ObjectTypes.INSTANCE, className));
		@params.Add(arg_this);
		
		Expect(6);
		if (la.kind == 12) {
			ParamList(ref @params);
		}
		Expect(8);
		Block(out block);
		func = Node.MakeFunc("constructor", @params, (Block)block, TypeAnnotation.VoidType.Clone(), true);
		
	}

	void MethodDecl(out Statement func, string className) {
		string name; var type = TypeAnnotation.InferenceType.Clone(); Statement block = null;
		var @params = new List<Argument>(); Argument arg_this = null;
		
		while (!(la.kind == 0 || la.kind == 27)) {SynErr(112); Get();}
		Expect(27);
		arg_this = Node.MakeArg("this", new TypeAnnotation(ObjectTypes.INSTANCE, className));
		@params.Add(arg_this);
		
		Expect(12);
		name = t.val; 
		Expect(6);
		if (la.kind == 12) {
			ParamList(ref @params);
		}
		Expect(8);
		if (la.kind == 28) {
			Get();
			Type(out type);
		}
		Block(out block);
		func = Node.MakeFunc(name, @params, (Block)block, type); 
	}

	void FieldDecl(out List<Expression> outs ) {
		string name; TypeAnnotation type = TypeAnnotation.InferenceType.Clone(); Expression rhs = null;
		Identifier variable; outs = new List<Expression>();
		var vars = new List<Identifier>(); var exprs = new List<Expression>();
		
		Expect(12);
		name = t.val; 
		if (la.kind == 29) {
			Get();
			Type(out type);
		}
		if (la.kind == 30) {
			Get();
			if (NotFollowedByDoubleDots()) {
				CondExpr(out rhs);
			} else if (la.kind == 7) {
				IntSeqExpr(out rhs);
			} else SynErr(113);
		}
		while (la.kind == 10) {
			Get();
			variable = Node.MakeField(name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			
			Expect(12);
			name = t.val; 
			if (la.kind == 29) {
				Get();
				Type(out type);
			}
			if (la.kind == 30) {
				Get();
				if (NotFollowedByDoubleDots()) {
					CondExpr(out rhs);
				} else if (la.kind == 7) {
					IntSeqExpr(out rhs);
				} else SynErr(114);
			}
		}
		variable = Node.MakeField(name, type);
			vars.Add(variable);
			exprs.Add(rhs);
		
			outs.Add(Node.MakeVarDecl(vars, exprs));
		
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
		while (StartOf(7)) {
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
		case 31: {
			Get();
			type.ObjType = ObjectTypes.INTEGER; 
			break;
		}
		case 32: {
			Get();
			type.ObjType = ObjectTypes.BOOL; 
			break;
		}
		case 33: {
			Get();
			type.ObjType = ObjectTypes.FLOAT; 
			break;
		}
		case 34: {
			Get();
			type.ObjType = ObjectTypes.RATIONAL; 
			break;
		}
		case 35: {
			Get();
			type.ObjType = ObjectTypes.BIGINT; 
			break;
		}
		case 36: {
			Get();
			type.ObjType = ObjectTypes.STRING; 
			break;
		}
		case 37: {
			Get();
			type.ObjType = ObjectTypes.BYTEARRAY; 
			break;
		}
		case 38: {
			Get();
			type.ObjType = ObjectTypes.VAR; 
			break;
		}
		case 39: {
			Get();
			type.ObjType = ObjectTypes.TUPLE; 
			break;
		}
		case 40: {
			Get();
			type.ObjType = ObjectTypes.LIST; 
			break;
		}
		case 41: {
			Get();
			type.ObjType = ObjectTypes.DICT; 
			break;
		}
		case 42: {
			Get();
			type.ObjType = ObjectTypes.EXPRESSION; 
			break;
		}
		case 43: {
			Get();
			type.ObjType = ObjectTypes.FUNCTION; 
			break;
		}
		case 44: {
			Get();
			type.ObjType = ObjectTypes.SEQ; 
			break;
		}
		case 45: {
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
		if (la.kind == 74) {
			Get();
			OrTest(out true_expr);
			Expect(3);
			CondExpr(out false_expr);
			expr = Node.MakeCondExpr(expr, true_expr, false_expr); 
		}
	}

	void IntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null; 
		Expect(7);
		if (StartOf(8)) {
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
		if (StartOf(9)) {
			SimpleStmt(out stmt);
		} else if (StartOf(10)) {
			CompoundStmt(out stmt);
		} else SynErr(116);
	}

	void SimpleStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == _lcurly) {
			Block(out stmt);
		} else if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 46) {
			PrintStmt(out stmt);
		} else if (la.kind == 47) {
			ReturnStmt(out stmt);
		} else if (la.kind == 48) {
			BreakStmt(out stmt);
		} else if (la.kind == 50) {
			ContinueStmt(out stmt);
		} else if (la.kind == 51) {
			ThrowStmt(out stmt);
		} else if (la.kind == 52) {
			YieldStmt(out stmt);
		} else if (la.kind == 4) {
			EmptyStmt(out stmt);
		} else SynErr(117);
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; 
		switch (la.kind) {
		case 63: {
			while (!(la.kind == 0 || la.kind == 63)) {SynErr(118); Get();}
			IfStmt(out stmt);
			break;
		}
		case 65: {
			WhileStmt(out stmt);
			break;
		}
		case 18: {
			ForStmt(out stmt);
			break;
		}
		case 66: {
			SwitchStmt(out stmt);
			break;
		}
		case 27: {
			FuncDecl(out stmt);
			break;
		}
		case 70: {
			WithStmt(out stmt);
			break;
		}
		case 71: {
			TryStmt(out stmt);
			break;
		}
		default: SynErr(119); break;
		}
	}

	void PrintStmt(out Statement stmt) {
		SequenceExpression exprs = null; bool trailing_comma = false; 
		Expect(46);
		if (StartOf(8)) {
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
		SequenceExpression items = null; 
		Expect(47);
		if (StartOf(8)) {
			RValueList(out items);
		}
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(121); Get();}
		Expect(4);
		stmt = Node.MakeReturnStmt(items); 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; 
		Expect(48);
		if (la.kind == 49) {
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
		Expect(50);
		if (la.kind == 49) {
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
		Expect(51);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(124); Get();}
		Expect(4);
		stmt = Node.MakeThrowStmt(expr); 
	}

	void YieldStmt(out Statement stmt) {
		Expression expr; 
		Expect(52);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(125); Get();}
		Expect(4);
		stmt = Node.MakeYieldStmt(expr); 
	}

	void EmptyStmt(out Statement stmt) {
		Expect(4);
		stmt = Node.MakeEmptyStmt(); 
	}

	void RValueList(out SequenceExpression seq) {
		Expression tmp; var exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (la.kind == 10) {
			while (!(la.kind == 0 || la.kind == 10)) {SynErr(126); Get();}
			Get();
			CondExpr(out tmp);
			exprs.Add(tmp);	
		}
		seq = Node.MakeSequence(exprs); 
	}

	void AugAssignOpe(ref OperatorType type) {
		switch (la.kind) {
		case 53: {
			Get();
			type = OperatorType.PLUS; 
			break;
		}
		case 54: {
			Get();
			type = OperatorType.MINUS; 
			break;
		}
		case 55: {
			Get();
			type = OperatorType.TIMES; 
			break;
		}
		case 56: {
			Get();
			type = OperatorType.DIV; 
			break;
		}
		case 57: {
			Get();
			type = OperatorType.POWER; 
			break;
		}
		case 58: {
			Get();
			type = OperatorType.MOD; 
			break;
		}
		case 59: {
			Get();
			type = OperatorType.BIT_AND; 
			break;
		}
		case 60: {
			Get();
			type = OperatorType.BIT_OR; 
			break;
		}
		case 61: {
			Get();
			type = OperatorType.BIT_LSHIFT; 
			break;
		}
		case 62: {
			Get();
			type = OperatorType.BIT_RSHIFT; 
			break;
		}
		default: SynErr(127); break;
		}
	}

	void VarDecl(out List<Expression> outs ) {
		string name; var type = TypeAnnotation.InferenceType.Clone(); Expression rhs = null;
		Identifier variable; outs = new List<Expression>();
		var vars = new List<Identifier>(); var exprs = new List<Expression>();
		
		Expect(69);
		Expect(12);
		name = t.val; 
		if (la.kind == 29) {
			Get();
			Type(out type);
		}
		if (la.kind == 30) {
			Get();
			if (NotFollowedByDoubleDots()) {
				CondExpr(out rhs);
			} else if (la.kind == 7) {
				IntSeqExpr(out rhs);
			} else SynErr(128);
		}
		variable = Node.MakeLocalVar(name, type);
		vars.Add(variable);
		exprs.Add(rhs);
		rhs = null;
		
		while (la.kind == 10) {
			Get();
			Expect(12);
			name = t.val; 
			if (la.kind == 29) {
				Get();
				Type(out type);
			}
			if (la.kind == 30) {
				Get();
				if (NotFollowedByDoubleDots()) {
					CondExpr(out rhs);
				} else if (la.kind == 7) {
					IntSeqExpr(out rhs);
				} else SynErr(129);
			}
			variable = Node.MakeLocalVar(name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			rhs = null;
			
		}
		outs.Add(Node.MakeVarDecl(vars, exprs)); 
	}

	void LValueList(out SequenceExpression target) {
		Expression tmp; var exprs = new List<Expression>(); 
		Primary(out tmp);
		exprs.Add(tmp); 
		while (la.kind == 10) {
			while (!(la.kind == 0 || la.kind == 10)) {SynErr(130); Get();}
			Get();
			Primary(out tmp);
			exprs.Add(tmp); 
		}
		target = Node.MakeSequence(exprs); 
	}

	void IfStmt(out Statement stmt) {
		Expression tmp; Statement true_block, false_block = null; 
		Expect(63);
		Expect(6);
		CondExpr(out tmp);
		Expect(8);
		Stmt(out true_block);
		if (la.kind == 64) {
			Get();
			Stmt(out false_block);
		}
		stmt = Node.MakeIfStmt(tmp, true_block, false_block); 
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; Statement body = null; WhileStatement tmp; 
		Expect(65);
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
		SequenceExpression left = null; Expression rvalue = null; Statement body;
		ForStatement tmp; bool has_let = false;
		
		Expect(18);
		tmp = Node.MakeForStmt();
		Parser.breakables.Add(tmp);
		
		Expect(6);
		if (la.kind == 69) {
			LValueListWithLet(out left);
			if(left != null) has_let = true; 
		} else if (StartOf(3)) {
			LValueList(out left);
		} else SynErr(131);
		Expect(17);
		if (NotFollowedByDoubleDots()) {
			CondExpr(out rvalue);
		} else if (la.kind == 7) {
			IntSeqExpr(out rvalue);
		} else SynErr(132);
		Expect(8);
		Stmt(out body);
		tmp.Left = left;
		tmp.Target = rvalue;
		tmp.Body = body;
		tmp.HasLet = has_let;
		Parser.breakables.RemoveLast();
		stmt = tmp;
		
	}

	void SwitchStmt(out Statement stmt) {
		Expression target; List<CaseClause> cases; 
		Expect(66);
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
		Expect(70);
		Expect(6);
		Expect(8);
		Block(out block);
		stmt = null; 
	}

	void TryStmt(out Statement stmt) {
		Statement body, catch_body = null, finally_body = null; List<CatchClause> catches = null;
		TypeAnnotation excp_type = null; Identifier catch_ident = null; string name = null;
		
		Expect(71);
		Block(out body);
		while (la.kind == 72) {
			Get();
			if(catches == null) catches = new List<CatchClause>(); 
			Expect(6);
			Expect(12);
			name = t.val; 
			Expect(29);
			Type(out excp_type);
			Expect(8);
			catch_ident = Node.MakeLocalVar(name, excp_type); 
			Block(out catch_body);
			catches.Add(Node.MakeCatchClause((Block)catch_body, catch_ident)); 
		}
		if (la.kind == 73) {
			Get();
			Block(out finally_body);
		}
		stmt = Node.MakeTryStmt((Block)body, catches, (Block)finally_body); 
	}

	void LValueListWithLet(out SequenceExpression target) {
		Identifier ident = null; string name;
		var type = TypeAnnotation.InferenceType; var items = new List<Expression>();
		
		Expect(69);
		Expect(12);
		name = t.val; 
		if (la.kind == 29) {
			Get();
			Type(out type);
		}
		ident = Node.MakeLocalVar(name, type);
		type = TypeAnnotation.InferenceType;
		items.Add(ident);
		
		while (la.kind == 10) {
			while (!(la.kind == 0 || la.kind == 10)) {SynErr(133); Get();}
			Get();
			Expect(12);
			name = t.val; 
			if (la.kind == 29) {
				Get();
				Type(out type);
			}
			ident = Node.MakeLocalVar(name, type);
			type = TypeAnnotation.InferenceType;
			items.Add(ident);
			
		}
		target = Node.MakeSequence(items); 
	}

	void CaseClauseList(out List<CaseClause> clauses ) {
		clauses = new List<CaseClause>(); List<Expression> label_list; Statement inner; 
		CaseLabelList(out label_list);
		Stmt(out inner);
		clauses.Add(Node.MakeCaseClause(label_list, inner)); 
		while (la.kind == 67) {
			CaseLabelList(out label_list);
			Stmt(out inner);
			clauses.Add(Node.MakeCaseClause(label_list, inner)); 
		}
	}

	void CaseLabelList(out List<Expression> label_list ) {
		label_list = new List<Expression>(); Expression tmp; 
		CaseLabel(out tmp);
		label_list.Add(tmp); 
		while (la.kind == 67) {
			CaseLabel(out tmp);
			label_list.Add(tmp); 
		}
	}

	void CaseLabel(out Expression expr) {
		expr = null; 
		Expect(67);
		if (StartOf(11)) {
			Literal(out expr);
		} else if (la.kind == 7) {
			IntSeqExpr(out expr);
		} else if (la.kind == 68) {
			Get();
			expr = Node.MakeConstant(ObjectTypes._CASE_DEFAULT, "default"); 
		} else SynErr(134);
		while (!(la.kind == 0 || la.kind == 3)) {SynErr(135); Get();}
		Expect(3);
	}

	void Literal(out Expression expr) {
		expr = null; string tmp; bool has_suffix = false; 
		switch (la.kind) {
		case 13: {
			Get();
			tmp = t.val; 
			if (la.kind == 96 || la.kind == 97) {
				if (la.kind == 96) {
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
		case 98: case 99: {
			if (la.kind == 98) {
				Get();
			} else {
				Get();
			}
			expr = Node.MakeConstant(ObjectTypes.BOOL, Convert.ToBoolean(t.val)); 
			break;
		}
		case 100: {
			Get();
			expr = Node.MakeConstant(ObjectTypes.NULL, null); 
			break;
		}
		default: SynErr(136); break;
		}
	}

	void Primary(out Expression expr) {
		expr = null; 
		if (StartOf(12)) {
			Atom(out expr);
			while (la.kind == 6 || la.kind == 7 || la.kind == 11) {
				Trailer(ref expr);
			}
		} else if (la.kind == 95) {
			NewExpression(out expr);
		} else SynErr(137);
	}

	void Argument(out Argument arg) {
		string name; Expression default_val = null; var type = TypeAnnotation.VariantType.Clone(); 
		Expect(12);
		name = t.val; 
		if (la.kind == 29) {
			Get();
			Type(out type);
		}
		if (la.kind == 30) {
			Get();
			Literal(out default_val);
		}
		arg = Node.MakeArg(name, type, default_val); 
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		while (la.kind == 75) {
			Get();
			AndTest(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.OR, expr, rhs); 
		}
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		NotTest(out expr);
		while (la.kind == 76) {
			Get();
			NotTest(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.AND, expr, rhs); 
		}
	}

	void NotTest(out Expression expr) {
		Expression term; expr = null; 
		if (la.kind == 77) {
			Get();
			NotTest(out term);
			expr = Node.MakeUnaryExpr(OperatorType.NOT, term); 
		} else if (StartOf(13)) {
			Comparison(out expr);
		} else SynErr(138);
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		BitOr(out expr);
		type = OperatorType.EQUAL; 
		if (StartOf(14)) {
			switch (la.kind) {
			case 78: {
				Get();
				type = OperatorType.EQUAL; 
				break;
			}
			case 79: {
				Get();
				type = OperatorType.NOTEQ; 
				break;
			}
			case 80: {
				Get();
				type = OperatorType.LESS; 
				break;
			}
			case 81: {
				Get();
				type = OperatorType.GREAT; 
				break;
			}
			case 82: {
				Get();
				type = OperatorType.LESE; 
				break;
			}
			case 83: {
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
		while (la.kind == 84) {
			Get();
			BitXor(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.BIT_OR, expr, rhs); 
		}
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		while (la.kind == 85) {
			Get();
			BitAnd(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.BIT_XOR, expr, rhs); 
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOpe(out expr);
		while (la.kind == 86) {
			Get();
			ShiftOpe(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.BIT_AND, expr, rhs); 
		}
	}

	void ShiftOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOpe(out expr);
		while (la.kind == 87 || la.kind == 88) {
			if (la.kind == 87) {
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

	void AddOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		while (la.kind == 89 || la.kind == 90) {
			if (la.kind == 89) {
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

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		Factor(out expr);
		while (la.kind == 91 || la.kind == 92 || la.kind == 93) {
			if (la.kind == 91) {
				Get();
				type = OperatorType.TIMES; 
			} else if (la.kind == 92) {
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
		if (StartOf(3)) {
			PowerOpe(out expr);
		} else if (la.kind == 89 || la.kind == 90) {
			if (la.kind == 90) {
				Get();
				type = OperatorType.MINUS; 
			} else {
				Get();
				type = OperatorType.PLUS; 
			}
			Factor(out factor);
			expr = Node.MakeUnaryExpr(type, factor); 
		} else SynErr(139);
	}

	void PowerOpe(out Expression expr) {
		Expression rhs; 
		Primary(out expr);
		if (la.kind == 94) {
			Get();
			Factor(out rhs);
			expr = Node.MakeBinaryExpr(OperatorType.POWER, expr, rhs); 
		}
	}

	void Atom(out Expression expr) {
		string name; expr = null; 
		if (la.kind == 12) {
			Get();
			name = t.val;
			expr = Node.MakeIdentifier(name);
			
		} else if (StartOf(11)) {
			Literal(out expr);
		} else if (la.kind == 6) {
			Get();
			if (StartOf(8)) {
				SequenceMaker(out expr, ObjectTypes.TUPLE);
			}
			while (!(la.kind == 0 || la.kind == 8)) {SynErr(140); Get();}
			Expect(8);
			if(expr == null)
			expr = Node.MakeSeqInitializer(ObjectTypes.TUPLE, new List<Expression>());
			
		} else if (la.kind == 7) {
			Get();
			if (StartOf(8)) {
				SequenceMaker(out expr, ObjectTypes.LIST);
			}
			while (!(la.kind == 0 || la.kind == 2)) {SynErr(141); Get();}
			Expect(2);
			if(expr == null)
			expr = Node.MakeSeqInitializer(ObjectTypes.LIST, new List<Expression>());
			
		} else if (la.kind == 5) {
			Get();
			if (StartOf(8)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 9)) {SynErr(142); Get();}
			Expect(9);
			if(expr == null) expr = Node.MakeSeqInitializer(ObjectTypes.DICT, new List<Expression>()); 
		} else SynErr(143);
	}

	void Trailer(ref Expression expr) {
		var args = new List<Expression>(); Expression subscript; 
		if (la.kind == 6) {
			Get();
			args.Add(null); 
			if (StartOf(8)) {
				ArgList(ref args);
			}
			Expect(8);
			expr = Node.MakeCallExpr(expr, args); 
		} else if (la.kind == 7) {
			Get();
			Subscript(out subscript);
			Expect(2);
			expr = Node.MakeMemRef(expr, subscript); 
		} else if (la.kind == 11) {
			Get();
			Expect(12);
			expr = Node.MakeMemRef(expr, Node.MakeField(t.val, TypeAnnotation.Subscription)); 
		} else SynErr(144);
	}

	void NewExpression(out Expression expr) {
		var args = new List<Expression>(); 
		Expect(95);
		Expect(12);
		expr = Node.MakeIdentifier(t.val); 
		while (la.kind == 11) {
			Get();
			Expect(12);
			expr = Node.MakeMemRef(expr, Node.MakeField(t.val, TypeAnnotation.Subscription)); 
		}
		Expect(6);
		if (StartOf(8)) {
			ArgList(ref args);
		}
		Expect(8);
		expr = Node.MakeNewExpr(expr, args); 
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
		} else SynErr(145);
	}

	void SequenceMaker(out Expression expr, ObjectTypes ObjType) {
		Expression tmp = null; List<Expression> list = new List<Expression>();
		expr = null; ComprehensionIter comprehen = null;
		
		if (IsSequenceInitializer()) {
			CondExpr(out tmp);
			if(tmp != null) list.Add(tmp); 
			while (la.kind == 10) {
				Get();
				CondExpr(out tmp);
				if(tmp != null) list.Add(tmp); 
			}
			expr = Node.MakeSeqInitializer(ObjType, list); 
		} else if (la.kind == 5) {
			Get();
			CondExpr(out tmp);
			CompFor(out comprehen);
			Expect(9);
			expr = Node.MakeComp(tmp, (ComprehensionFor)comprehen, ObjType); 
		} else SynErr(146);
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
		if(list.Count > 0) expr = Node.MakeSeqInitializer(ObjectTypes.DICT, list); 
	}

	void CompFor(out ComprehensionIter expr) {
		Expression rvalue = null; ComprehensionIter body = null; SequenceExpression target; 
		Expect(18);
		LValueList(out target);
		Expect(17);
		if (NotFollowedByDoubleDots()) {
			CondExpr(out rvalue);
		} else if (la.kind == 7) {
			IntSeqExpr(out rvalue);
		} else SynErr(147);
		if (la.kind == 18 || la.kind == 63) {
			CompIter(out body);
		}
		expr = Node.MakeCompFor(target, rvalue, body); 
	}

	void CompIter(out ComprehensionIter expr) {
		expr = null; 
		if (la.kind == 18) {
			CompFor(out expr);
		} else if (la.kind == 63) {
			CompIf(out expr);
		} else SynErr(148);
	}

	void CompIf(out ComprehensionIter expr) {
		Expression tmp; ComprehensionIter body = null; 
		Expect(63);
		OrTest(out tmp);
		if (la.kind == 18 || la.kind == 63) {
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
		{T,x,T,T, T,x,x,x, T,T,T,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, T,T,T,T, x,x,x,x, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,T,T, T,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,x,T,T, T,x,x},
		{x,x,x,x, T,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,x,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x}

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
			case 24: s = "\"protected\" expected"; break;
			case 25: s = "\"private\" expected"; break;
			case 26: s = "\"constructor\" expected"; break;
			case 27: s = "\"def\" expected"; break;
			case 28: s = "\"->\" expected"; break;
			case 29: s = "\"(-\" expected"; break;
			case 30: s = "\"=\" expected"; break;
			case 31: s = "\"int\" expected"; break;
			case 32: s = "\"bool\" expected"; break;
			case 33: s = "\"float\" expected"; break;
			case 34: s = "\"rational\" expected"; break;
			case 35: s = "\"bigint\" expected"; break;
			case 36: s = "\"string\" expected"; break;
			case 37: s = "\"bytearray\" expected"; break;
			case 38: s = "\"var\" expected"; break;
			case 39: s = "\"tuple\" expected"; break;
			case 40: s = "\"list\" expected"; break;
			case 41: s = "\"dictionary\" expected"; break;
			case 42: s = "\"expression\" expected"; break;
			case 43: s = "\"function\" expected"; break;
			case 44: s = "\"intseq\" expected"; break;
			case 45: s = "\"void\" expected"; break;
			case 46: s = "\"print\" expected"; break;
			case 47: s = "\"return\" expected"; break;
			case 48: s = "\"break\" expected"; break;
			case 49: s = "\"upto\" expected"; break;
			case 50: s = "\"continue\" expected"; break;
			case 51: s = "\"throw\" expected"; break;
			case 52: s = "\"yield\" expected"; break;
			case 53: s = "\"+=\" expected"; break;
			case 54: s = "\"-=\" expected"; break;
			case 55: s = "\"*=\" expected"; break;
			case 56: s = "\"/=\" expected"; break;
			case 57: s = "\"**=\" expected"; break;
			case 58: s = "\"%=\" expected"; break;
			case 59: s = "\"&=\" expected"; break;
			case 60: s = "\"|=\" expected"; break;
			case 61: s = "\"<<=\" expected"; break;
			case 62: s = "\">>=\" expected"; break;
			case 63: s = "\"if\" expected"; break;
			case 64: s = "\"else\" expected"; break;
			case 65: s = "\"while\" expected"; break;
			case 66: s = "\"switch\" expected"; break;
			case 67: s = "\"case\" expected"; break;
			case 68: s = "\"default\" expected"; break;
			case 69: s = "\"let\" expected"; break;
			case 70: s = "\"with\" expected"; break;
			case 71: s = "\"try\" expected"; break;
			case 72: s = "\"catch\" expected"; break;
			case 73: s = "\"finally\" expected"; break;
			case 74: s = "\"?\" expected"; break;
			case 75: s = "\"||\" expected"; break;
			case 76: s = "\"&&\" expected"; break;
			case 77: s = "\"!\" expected"; break;
			case 78: s = "\"==\" expected"; break;
			case 79: s = "\"!=\" expected"; break;
			case 80: s = "\"<\" expected"; break;
			case 81: s = "\">\" expected"; break;
			case 82: s = "\"<=\" expected"; break;
			case 83: s = "\">=\" expected"; break;
			case 84: s = "\"|\" expected"; break;
			case 85: s = "\"^\" expected"; break;
			case 86: s = "\"&\" expected"; break;
			case 87: s = "\"<<\" expected"; break;
			case 88: s = "\">>\" expected"; break;
			case 89: s = "\"+\" expected"; break;
			case 90: s = "\"-\" expected"; break;
			case 91: s = "\"*\" expected"; break;
			case 92: s = "\"/\" expected"; break;
			case 93: s = "\"%\" expected"; break;
			case 94: s = "\"**\" expected"; break;
			case 95: s = "\"new\" expected"; break;
			case 96: s = "\"l\" expected"; break;
			case 97: s = "\"L\" expected"; break;
			case 98: s = "\"true\" expected"; break;
			case 99: s = "\"false\" expected"; break;
			case 100: s = "\"null\" expected"; break;
			case 101: s = "??? expected"; break;
			case 102: s = "invalid ModuleBody"; break;
			case 103: s = "invalid ModuleBody"; break;
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
			case 126: s = "this symbol not expected in RValueList"; break;
			case 127: s = "invalid AugAssignOpe"; break;
			case 128: s = "invalid VarDecl"; break;
			case 129: s = "invalid VarDecl"; break;
			case 130: s = "this symbol not expected in LValueList"; break;
			case 131: s = "invalid ForStmt"; break;
			case 132: s = "invalid ForStmt"; break;
			case 133: s = "this symbol not expected in LValueListWithLet"; break;
			case 134: s = "invalid CaseLabel"; break;
			case 135: s = "this symbol not expected in CaseLabel"; break;
			case 136: s = "invalid Literal"; break;
			case 137: s = "invalid Primary"; break;
			case 138: s = "invalid NotTest"; break;
			case 139: s = "invalid Factor"; break;
			case 140: s = "this symbol not expected in Atom"; break;
			case 141: s = "this symbol not expected in Atom"; break;
			case 142: s = "this symbol not expected in Atom"; break;
			case 143: s = "invalid Atom"; break;
			case 144: s = "invalid Trailer"; break;
			case 145: s = "invalid Subscript"; break;
			case 146: s = "invalid SequenceMaker"; break;
			case 147: s = "invalid CompFor"; break;
			case 148: s = "invalid CompIter"; break;

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
