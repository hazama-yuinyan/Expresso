using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using Expresso.Ast;
using Expresso.Compiler.Meta;
using Expresso.Runtime;
using Expresso.Utils;





using System;



public class Parser {
	public const int _EOF = 0;
	public const int _double_dots = 1;
	public const int _triple_dots = 2;
	public const int _rbracket = 3;
	public const int _colon = 4;
	public const int _semicolon = 5;
	public const int _lcurly = 6;
	public const int _lparen = 7;
	public const int _lbracket = 8;
	public const int _rparen = 9;
	public const int _rcurly = 10;
	public const int _comma = 11;
	public const int _dot = 12;
	public const int _ident = 13;
	public const int _integer = 14;
	public const int _float = 15;
	public const int _hex_digit = 16;
	public const int _string_literal = 17;
	public const int _keyword_in = 18;
	public const int _keyword_for = 19;
	public const int maxT = 107;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

internal ScopeStatement cur_scope = null;		//the current scope of variables
	static List<BreakableStatement> breakables = new List<BreakableStatement>();	//the current parent breakables hierarchy
	public string ParsingFileName{get; set;}
	public ExpressoAst TopmostAst{get; private set;}	//the top-level AST the parser is parsing
	
	///<summary>
	/// Parser Implementation details:
	/// 	During parsing we'll resolve break and continue statements.(Find which a break or continue statement would have its effect
	/// 	on which a loop statement) And in post-parse process, do flow analysis and type validity check, including local name bindings.
	///		Note that the identifiers are just placeholders until after doing name binding. 
	/// 	(Thus referencing them causes runtime exceptions)
	///</summary>
	Parser()
	{
		//Add built-in functions
		/*FunctionDefinition[] native_funcs = {
			new NativeLambdaUnary("abs", ImplementationHelpers.MakeArg(new Identifier("val", new TypeAnnotation(ObjectTypes.Var), null, 0)), ExpressoFunctions.Abs),
			new NativeLambdaUnary("sqrt", ImplementationHelpers.MakeArg(new Identifier("val", new TypeAnnotation(ObjectTypes.Var), null, 0)), ExpressoFunctions.Sqrt),
			new NativeLambdaUnary("toInt", ImplementationHelpers.MakeArg(new Identifier("val", new TypeAnnotation(ObjectTypes.Var), null, 0)), ExpressoFunctions.ToInt)
		};
		foreach(var tmp in native_funcs)
			cur_scope.AddFunction(tmp);*/
	}
	
	LiteralExpression CreateConstant(ObjectTypes type)
	{
		LiteralExpression result = null;
		
		switch(type){
		case ObjectTypes.Integer:
			result = AstNode.MakeConstant(type, 0);
			break;
			
		case ObjectTypes.Bool:
			result = AstNode.MakeConstant(type, false);
			break;
			
		case ObjectTypes.Float:
			result = AstNode.MakeConstant(type, 0.0);
			break;
			
		case ObjectTypes.String:
			result = AstNode.MakeConstant(type, "");
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
		if(x.kind != _lbracket)
            return true;
		
        while(x.kind != 0 && x.kind != _double_dots && x.kind != _rbracket && x.kind != _semicolon && x.kind != _rparen && x.kind != _keyword_in)
			x = scanner.Peek();
		
		return x.kind != _double_dots;
	}
	
	bool IsSequenceInitializer()
	{
		Token x = la;
        scanner.ResetPeek();
		if(x.kind != _comma)
            return true;
		
		while(x.kind != 0 && x.kind != _comma && x.kind != _keyword_for)
            x = scanner.Peek();
		
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
		return AstNode.MakeBreakStmt(count, tmp);
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
		return AstNode.MakeContinueStmt(count, tmp);
	}
	
	Statement MakeDefaultCtor(string className)
	{
		var ctor = AstNode.MakeFunc("constructor", null, new BlockStatement(), TypeAnnotation.VoidType.Clone(), Modifiers.Public);
		return ctor;
	}

    Statement MakeDefaultDtor(string className)
    {
        var dtor = AstNode.MakeFunc("destructor", null, new BlockStatement(), TypeAnnotation.VoidType.Clone(), Modifiers.Public);
        return dtor;
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
		try{
		        ExpressoNameBinder.BindAst(module_decl, this); //Here's the start of post-parse processing
		    }
		    catch(ParserException pe){
		        SemanticError(pe.Message, pe.Objects);
		    }
		this.TopmostAst = module_decl;	//Currently there is not so much code out there, though...
		
	}

	void ModuleBody(out ExpressoAst decl) {
		var decls = new List<Statement>();
		  string module_name; Modifiers modifiers = Modifiers.None;
		List<Statement> prog_defs = null; Statement stmt = null;
		
		ModuleNameDefinition(out module_name);
		if (la.kind == 22) {
			ProgramDefinition(out prog_defs);
			if(prog_defs !=null) decls.AddRange(prog_defs); 
		}
		if (la.kind == 20) {
			Get();
			modifiers = Modifiers.Export; 
		}
		if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 30) {
			FuncDecl(out stmt, modifiers);
		} else if (la.kind == 24) {
			ClassDecl(out stmt, modifiers);
		} else SynErr(108);
		decls.Add(stmt);
		modifiers = Modifiers.None;
		
		while (StartOf(2)) {
			if (la.kind == 20) {
				Get();
				modifiers = Modifiers.Export; 
			}
			if (StartOf(1)) {
				ExprStmt(out stmt);
			} else if (la.kind == 30) {
				FuncDecl(out stmt, modifiers);
			} else if (la.kind == 24) {
				ClassDecl(out stmt, modifiers);
			} else SynErr(109);
			decls.Add(stmt);
			modifiers = Modifiers.None;
			
		}
		decl = AstNode.MakeModuleDef(module_name, decls, export_map);
		
	}

	void ModuleNameDefinition(out string moduleName) {
		Expect(21);
		Expect(13);
		moduleName = t.val; 
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(110); Get();}
		Expect(5);
	}

	void ProgramDefinition(out List<Statement> imports ) {
		imports = new List<Statement>(); Statement tmp; 
		ImportStmt(out tmp);
		imports.Add(tmp); 
		while (la.kind == 22) {
			ImportStmt(out tmp);
			imports.Add(tmp); 
		}
	}

	void ExprStmt(out Statement stmt) {
		List<Expression> lvalues = null; var targets = new List<SequenceExpression>();
		SequenceExpression seq = null;
		stmt = null; OperatorType op_type = OperatorType.None;
		
		if (la.kind == 32 || la.kind == 33) {
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
			if(op_type != OperatorType.None && targets.Count != 1)
			SemErr("An augumented assignment can't have multiple left-hand-side.");
			if(op_type != OperatorType.None &&		//See if it is an augumented assignment and
			targets[0].Count != seq.Count)		//both sides have the same number of items
			SemErr("An augumented assignment must have both sides balanced.");
			
			if(op_type != OperatorType.None)
			stmt = AstNode.MakeAugumentedAssignment(targets[0], seq, op_type);
			else
			stmt = AstNode.MakeAssignment(targets.Cast<Expression>(), seq);
			
		} else SynErr(111);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(112); Get();}
		Expect(5);
		if(stmt == null)
		stmt = AstNode.MakeExprStmt(lvalues);
		
	}

	void FuncDecl(out Statement func) {
		string name; var type = TypeAnnotation.VariantType.Clone(); Statement block;
		var @params = new List<Argument>(); Argument arg_this = null;
		
		while (!(la.kind == 0 || la.kind == 30)) {SynErr(113); Get();}
		Expect(30);
		block = null;
		arg_this = AstNode.MakeArg("this", new TypeAnnotation(ObjectTypes.TypeModule));
		@params.Add(arg_this);
		
		Expect(13);
		name = t.val; 
		Expect(7);
		if (la.kind == 13) {
			ParamList(ref @params);
		}
		Expect(9);
		if (la.kind == 31) {
			Get();
			Type(out type);
		}
		Block(out block);
		func = AstNode.MakeFunc(name, @params, (Block)block, type); 
	}

	void ClassDecl(out EntityDeclaration decl, Modifiers modifiers) {
		EntityDeclaration entity = null; var decls = new List<EntityDeclaration>();
		string name; var base_names = new List<Identifier>(); bool has_ctor = false;
		Modifiers cur_flag = Modifiers.Private;
		
		while (!(la.kind == 0 || la.kind == 24)) {SynErr(114); Get();}
		Expect(24);
		Expect(13);
		name = t.val; 
		if (la.kind == 4) {
			Get();
			Expect(13);
			base_names.Add(AstNode.MakeIdentifier(t.val)); 
			while (la.kind == 11) {
				Get();
				Expect(13);
				base_names.Add(AstNode.MakeIdentifier(t.val)); 
			}
		}
		Expect(6);
		while (StartOf(6)) {
			switch (la.kind) {
			case 25: case 26: case 27: {
				if (la.kind == 25) {
					Get();
					cur_flag = Modifiers.Private; 
				} else if (la.kind == 26) {
					Get();
					cur_flag = Modifiers.Protected; 
				} else {
					Get();
					cur_flag = Modifiers.Public; 
				}
				Expect(4);
				break;
			}
			case 28: {
				ConstructorDecl(out entity, name, cur_flag);
				decls.Add(entity);
				has_ctor = true;
				
				break;
			}
			case 29: {
				DestructorDecl(out entity, name, cur_flag);
				decls.Add(entity); 
				break;
			}
			case 30: {
				MethodDecl(out entity, name, cur_flag);
				decls.Add(entity); 
				break;
			}
			case 32: case 33: {
				FieldDecl(out entity, cur_flag);
				Expect(5);
				decls.Add(entity); 
				break;
			}
			case 24: {
				ClassDecl(out entity);
				decls.Add(entity); 
				break;
			}
			}
		}
		while (!(la.kind == 0 || la.kind == 10)) {SynErr(115); Get();}
		Expect(10);
		if(!has_ctor){		//Define the default constructor
		decls.Add(MakeDefaultCtor(name));
		}
		decl = AstNode.MakeClassDecl(name, base_names, decls, modifiers);
		
	}

	void ImportStmt(out Statement stmt) {
		string module_name, alias = null; var module_names = new List<string>();
		var aliases = new List<string>();
		
		while (!(la.kind == 0 || la.kind == 22)) {SynErr(116); Get();}
		Expect(22);
		ModuleName(out module_name);
		if (la.kind == 23) {
			Get();
			Expect(13);
			alias = t.val; 
		}
		module_names.Add(module_name);
		aliases.Add(alias);
		
		while (la.kind == 11) {
			Get();
			ModuleName(out module_name);
			if (la.kind == 23) {
				Get();
				Expect(13);
				alias = t.val; 
			}
			module_names.Add(module_name);
			aliases.Add(alias);
			
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(117); Get();}
		Expect(5);
		stmt = AstNode.MakeImportStmt(module_names, aliases); 
	}

	void ModuleName(out string name) {
		var sb = new StringBuilder(); 
		Expect(13);
		sb.Append(t.val); 
		while (la.kind == 12) {
			Get();
			sb.Append('.'); 
			Expect(13);
			sb.Append(t.val); 
		}
		name = sb.ToString(); 
	}

	void ConstructorDecl(out EntityDeclaration decl, string className, Modifiers modifiers) {
		Statement block = null; var @params = new List<Argument>(); 
		while (!(la.kind == 0 || la.kind == 28)) {SynErr(118); Get();}
		Expect(28);
		Expect(7);
		if (la.kind == 13) {
			ParamList(ref @params);
		}
		Expect(9);
		Block(out block);
		decl = AstNode.MakeFunc("constructor", @params, (Block)block, TypeAnnotation.VoidType.Clone(), modifiers);
		
	}

	void DestructorDecl(out EntityDeclaration decl, string className, Modifiers modifiers) {
		Statement block = null; var @params = new List<Argument>(); 
		while (!(la.kind == 0 || la.kind == 29)) {SynErr(119); Get();}
		Expect(29);
		Expect(7);
		if (la.kind == 13) {
			ParamList(ref @params);
		}
		Expect(9);
		Block(out block);
		decl = AstNode.MakeFunc("destructor", @params, (Block)block, TypeAnnotation.VoidType.Clone(), modifiers); 
	}

	void MethodDecl(out EntityDeclaration decl, string className, Modifiers modifiers) {
		string name; var type = TypeAnnotation.InferenceType.Clone(); Statement block = null;
		var @params = new List<Argument>();
		
		while (!(la.kind == 0 || la.kind == 30)) {SynErr(120); Get();}
		Expect(30);
		Expect(13);
		name = t.val; 
		Expect(7);
		if (la.kind == 13) {
			ParamList(ref @params);
		}
		Expect(9);
		if (la.kind == 31) {
			Get();
			Type(out type);
		}
		Block(out block);
		decl = AstNode.MakeFunc(name, @params, (Block)block, type, modifiers); 
	}

	void FieldDecl(out List<EntityDeclaration> outs, Modifiers modifiers ) {
		string name; var type = TypeAnnotation.InferenceType.Clone(); Expression rhs = null;
		Identifier variable; outs = new List<Expression>();
		var vars = new List<Identifier>(); var exprs = new List<Expression>();
		
		if (la.kind == 32) {
			Get();
		} else if (la.kind == 33) {
			Get();
		} else SynErr(121);
		VarDef(out name, out type, out rhs);
		variable = AstNode.MakeField(name, type);
		vars.Add(variable);
		if(rhs == null)
		   rhs = AstNode.MakeDefaultExpr(type);
		
		exprs.Add(rhs);
		rhs = null;
		
		while (la.kind == 11) {
			Get();
			VarDef(out name, out type, out rhs);
			variable = AstNode.MakeField(name, type);
			vars.Add(variable);
			if(rhs == null)
			   rhs = AstNode.MakeDefaultExpr(type);
			
			exprs.Add(rhs);
			
		}
		outs.Add(AstNode.MakeVarDecl(vars, exprs, flag)); 
	}

	void ParamList(ref List<ParameterDeclaration> @params ) {
		ParameterDeclaration expr; 
		Argument(out expr);
		@params.Add(expr); 
		while (WeakSeparator(11,7,8) ) {
			Argument(out expr);
			@params.Add(expr); 
		}
	}

	void Block(out Statement block) {
		Block tmp; Statement stmt; 
		Expect(6);
		tmp = new Block();
		
		Stmt(out stmt);
		tmp.Statements.Add(stmt); 
		while (StartOf(9)) {
			Stmt(out stmt);
			tmp.Statements.Add(stmt); 
		}
		Expect(10);
		block = tmp;
		
	}

	void Type(out TypeAnnotation type) {
		type = TypeAnnotation.InferenceType.Clone(); int dimension = 0; 
		switch (la.kind) {
		case 34: {
			Get();
			type.ObjType = ObjectTypes.Integer; 
			break;
		}
		case 35: {
			Get();
			type.ObjType = ObjectTypes.Bool; 
			break;
		}
		case 36: {
			Get();
			type.ObjType = ObjectTypes.Float; 
			break;
		}
		case 37: {
			Get();
			type.ObjType = ObjectTypes.Rational; 
			break;
		}
		case 38: {
			Get();
			type.ObjType = ObjectTypes.BigInt; 
			break;
		}
		case 39: {
			Get();
			type.ObjType = ObjectTypes.String; 
			break;
		}
		case 40: {
			Get();
			type.ObjType = ObjectTypes.Byte; 
			break;
		}
		case 41: {
			Get();
			type.ObjType = ObjectTypes.Var; 
			break;
		}
		case 42: {
			Get();
			type.ObjType = ObjectTypes.Tuple; 
			break;
		}
		case 43: {
			Get();
			type.ObjType = ObjectTypes.List; 
			break;
		}
		case 44: {
			Get();
			type.ObjType = ObjectTypes.Dict; 
			break;
		}
		case 45: {
			Get();
			type.ObjType = ObjectTypes.Expression; 
			break;
		}
		case 46: {
			Get();
			type.ObjType = ObjectTypes.Function; 
			break;
		}
		case 47: {
			Get();
			type.ObjType = ObjectTypes.Seq; 
			break;
		}
		case 48: {
			Get();
			type.ObjType = ObjectTypes.Undef; 
			break;
		}
		case 13: {
			Get();
			type.ObjType = ObjectTypes.Instance; type.TypeName = t.val; 
			break;
		}
		default: SynErr(122); break;
		}
		while (la.kind == 49) {
			Get();
			if(!type.IsArray)
			   type.IsArray = true;
			
			++dimension;
			
		}
		type.Dimension = dimension; 
	}

	void VarDef(out string name, out Type type, out Expression @default) {
		type = TypeAnnotation.InferenceType.Clone();
		@default = null;
		
		Expect(13);
		name = t.val; 
		if (la.kind == 73) {
			Get();
			Type(out type);
		}
		if (la.kind == 66) {
			Get();
			if (NotFollowedByDoubleDots()) {
				CondExpr(out @default);
			} else if (la.kind == 8) {
				IntSeqExpr(out @default);
			} else SynErr(123);
		}
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(10)) {
			SimpleStmt(out stmt);
		} else if (StartOf(11)) {
			CompoundStmt(out stmt);
		} else SynErr(124);
	}

	void SimpleStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == _lcurly) {
			Block(out stmt);
		} else if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 50) {
			ReturnStmt(out stmt);
		} else if (la.kind == 51) {
			BreakStmt(out stmt);
		} else if (la.kind == 53) {
			ContinueStmt(out stmt);
		} else if (la.kind == 54) {
			ThrowStmt(out stmt);
		} else if (la.kind == 55) {
			YieldStmt(out stmt);
		} else if (la.kind == 5) {
			EmptyStmt(out stmt);
		} else SynErr(125);
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; 
		switch (la.kind) {
		case 67: {
			while (!(la.kind == 0 || la.kind == 67)) {SynErr(126); Get();}
			IfStmt(out stmt);
			break;
		}
		case 69: {
			WhileStmt(out stmt);
			break;
		}
		case 19: {
			ForStmt(out stmt);
			break;
		}
		case 70: {
			SwitchStmt(out stmt);
			break;
		}
		case 30: {
			FuncDecl(out stmt);
			break;
		}
		case 74: {
			WithStmt(out stmt);
			break;
		}
		case 75: {
			TryStmt(out stmt);
			break;
		}
		default: SynErr(127); break;
		}
	}

	void ReturnStmt(out Statement stmt) {
		SequenceExpression items = null; 
		Expect(50);
		if (StartOf(12)) {
			RValueList(out items);
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(128); Get();}
		Expect(5);
		stmt = AstNode.MakeReturnStmt(items); 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; 
		Expect(51);
		if (la.kind == 52) {
			Get();
			Expect(14);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(129); Get();}
		Expect(5);
		stmt = MakeBreakStatement(count); 
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; 
		Expect(53);
		if (la.kind == 52) {
			Get();
			Expect(14);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(130); Get();}
		Expect(5);
		stmt = MakeContinueStatement(count); 
	}

	void ThrowStmt(out Statement stmt) {
		Expression expr; 
		Expect(54);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(131); Get();}
		Expect(5);
		stmt = AstNode.MakeThrowStmt(expr); 
	}

	void YieldStmt(out Statement stmt) {
		Expression expr; 
		Expect(55);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(132); Get();}
		Expect(5);
		stmt = AstNode.MakeYieldStmt(expr); 
	}

	void EmptyStmt(out Statement stmt) {
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(133); Get();}
		Expect(5);
		stmt = AstNode.MakeEmptyStmt(); 
	}

	void RValueList(out SequenceExpression seq) {
		Expression tmp; var exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (WeakSeparator(11,12,13) ) {
			CondExpr(out tmp);
			exprs.Add(tmp);	
		}
		seq = AstNode.MakeSequence(exprs); 
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; 
		OrTest(out expr);
		if (la.kind == 78) {
			Get();
			OrTest(out true_expr);
			Expect(4);
			CondExpr(out false_expr);
			expr = AstNode.MakeCondExpr(expr, true_expr, false_expr); 
		}
	}

	void AugAssignOpe(ref OperatorType type) {
		switch (la.kind) {
		case 56: {
			Get();
			type = OperatorType.Plus; 
			break;
		}
		case 57: {
			Get();
			type = OperatorType.Minus; 
			break;
		}
		case 58: {
			Get();
			type = OperatorType.Times; 
			break;
		}
		case 59: {
			Get();
			type = OperatorType.Divide; 
			break;
		}
		case 60: {
			Get();
			type = OperatorType.Power; 
			break;
		}
		case 61: {
			Get();
			type = OperatorType.Modulus; 
			break;
		}
		case 62: {
			Get();
			type = OperatorType.BitwiseAnd; 
			break;
		}
		case 63: {
			Get();
			type = OperatorType.BitwiseOr; 
			break;
		}
		case 64: {
			Get();
			type = OperatorType.BitwiseShiftLeft; 
			break;
		}
		case 65: {
			Get();
			type = OperatorType.BitwiseShiftRight; 
			break;
		}
		default: SynErr(134); break;
		}
	}

	void VarDecl(out List<Expression> outs ) {
		string name; var type = TypeAnnotation.InferenceType.Clone(); Expression rhs = null;
		Identifier variable; outs = new List<Expression>();
		var vars = new List<Identifier>(); var exprs = new List<Expression>();
		bool is_const = false;
		
		if (la.kind == 32) {
			Get();
			is_const = true; 
		} else if (la.kind == 33) {
			Get();
		} else SynErr(135);
		VarDef(out name, out type, out rhs);
		variable = AstNode.MakeLocalVar(name, type);
		 vars.Add(variable);
		 if(rhs == null)
		     rhs = AstNode.MakeDefaultExpr(type);
		
		exprs.Add(rhs);
		rhs = null;
		
		while (WeakSeparator(11,7,14) ) {
			VarDef(out name, out type, out rhs);
			variable = AstNode.MakeLocalVar(name, type);
			vars.Add(variable);
			if(rhs == null)
			   rhs = AstNode.MakeDefaultExpr(type);
			
			exprs.Add(rhs);
			rhs = null;
			
		}
		outs.Add(AstNode.MakeVarDecl(vars, exprs)); 
	}

	void LValueList(out SequenceExpression target) {
		Expression tmp; var exprs = new List<Expression>(); 
		Primary(out tmp);
		exprs.Add(tmp); 
		while (la.kind == 11) {
			while (!(la.kind == 0 || la.kind == 11)) {SynErr(136); Get();}
			Get();
			Primary(out tmp);
			exprs.Add(tmp); 
		}
		target = AstNode.MakeSequence(exprs); 
	}

	void IfStmt(out Statement stmt) {
		Expression tmp; Statement true_block, false_block = null; 
		Expect(67);
		Expect(7);
		CondExpr(out tmp);
		Expect(9);
		Stmt(out true_block);
		if (la.kind == 68) {
			Get();
			Stmt(out false_block);
		}
		stmt = AstNode.MakeIfStmt(tmp, true_block, false_block); 
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; Statement body = null; WhileStatement tmp; 
		Expect(69);
		tmp = AstNode.MakeWhileStmt(); 
		Expect(7);
		CondExpr(out cond);
		Expect(9);
		Stmt(out body);
		tmp.Condition = cond;
		tmp.Body = body;
		stmt = tmp;
		
	}

	void ForStmt(out Statement stmt) {
		SequenceExpression left = null; Expression rvalue = null; Statement body;
		ForStatement tmp;
		
		Expect(19);
		tmp = AstNode.MakeForStmt(); 
		Expect(7);
		LValueList(out left);
		Expect(18);
		if (NotFollowedByDoubleDots()) {
			CondExpr(out rvalue);
		} else if (la.kind == 8) {
			IntSeqExpr(out rvalue);
		} else SynErr(137);
		Expect(9);
		Stmt(out body);
		tmp.Left = left;
		tmp.Target = rvalue;
		tmp.Body = body;
		stmt = tmp;
		
	}

	void SwitchStmt(out Statement stmt) {
		Expression target; List<CaseClause> cases; 
		Expect(70);
		Expect(7);
		CondExpr(out target);
		Expect(9);
		Expect(6);
		CaseClauseList(out cases);
		Expect(10);
		stmt = AstNode.MakeSwitchStmt(target, cases); 
	}

	void WithStmt(out Statement stmt) {
		Statement block = null; 
		Expect(74);
		Expect(7);
		Expect(9);
		Block(out block);
		stmt = null; 
	}

	void TryStmt(out Statement stmt) {
		Statement body, catch_body = null, finally_body = null; List<CatchClause> catches = null;
		TypeAnnotation excp_type = null; Identifier catch_ident = null; string name = null;
		
		Expect(75);
		Block(out body);
		while (la.kind == 76) {
			Get();
			if(catches == null) catches = new List<CatchClause>(); 
			Expect(7);
			Expect(13);
			name = t.val; 
			Expect(73);
			Type(out excp_type);
			Expect(9);
			catch_ident = AstNode.MakeLocalVar(name, excp_type); 
			Block(out catch_body);
			catches.Add(AstNode.MakeCatchClause((Block)catch_body, catch_ident)); 
		}
		if (la.kind == 77) {
			Get();
			Block(out finally_body);
		}
		stmt = AstNode.MakeTryStmt((Block)body, catches, (Block)finally_body); 
	}

	void IntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null;
		  bool upper_inclusive = true;
		
		Expect(8);
		if (StartOf(12)) {
			OrTest(out start);
		}
		if (la.kind == 1) {
			Get();
			upper_inclusive = false; 
		} else if (la.kind == 2) {
			Get();
		} else SynErr(138);
		OrTest(out end);
		if (la.kind == 4) {
			Get();
			OrTest(out step);
		}
		Expect(3);
		if(start == null) start = CreateConstant(ObjectTypes.Integer);
		if(step == null) step = AstNode.MakeConstant(ObjectTypes.Integer, 1);
		expr = AstNode.MakeIntSeq(start, end, step, upper_inclusive);
		
	}

	void CaseClauseList(out List<CaseClause> clauses ) {
		clauses = new List<CaseClause>(); List<Expression> label_list; Statement inner; 
		CaseLabelList(out label_list);
		Stmt(out inner);
		clauses.Add(AstNode.MakeCaseClause(label_list, inner)); 
		while (la.kind == 71) {
			CaseLabelList(out label_list);
			Stmt(out inner);
			clauses.Add(AstNode.MakeCaseClause(label_list, inner)); 
		}
	}

	void CaseLabelList(out List<Expression> label_list ) {
		label_list = new List<Expression>(); Expression tmp; 
		CaseLabel(out tmp);
		label_list.Add(tmp); 
		while (la.kind == 71) {
			CaseLabel(out tmp);
			label_list.Add(tmp); 
		}
	}

	void CaseLabel(out Expression expr) {
		expr = null; 
		Expect(71);
		if (StartOf(15)) {
			Literal(out expr);
		} else if (la.kind == 8) {
			IntSeqExpr(out expr);
		} else if (la.kind == 72) {
			Get();
			expr = AstNode.MakeConstant(ObjectTypes._CASE_DEFAULT, "default"); 
		} else SynErr(139);
		while (!(la.kind == 0 || la.kind == 4)) {SynErr(140); Get();}
		Expect(4);
	}

	void Literal(out Expression expr) {
		expr = null; string tmp; bool has_suffix = false; 
		switch (la.kind) {
		case 14: {
			Get();
			tmp = t.val; 
			if (la.kind == 100 || la.kind == 101) {
				if (la.kind == 100) {
					Get();
					has_suffix = true; 
				} else {
					Get();
					has_suffix = true; 
				}
			}
			if(has_suffix)
			expr = AstNode.MakeConstant(ObjectTypes.BigInt, BigInteger.Parse(tmp));
			else
			expr = AstNode.MakeConstant(ObjectTypes.Integer, Convert.ToInt32(tmp));
			
			break;
		}
		case 16: {
			Get();
			expr = AstNode.MakeConstant(ObjectTypes.Integer, Convert.ToInt32(t.val, 16)); 
			break;
		}
		case 15: {
			Get();
			expr = AstNode.MakeConstant(ObjectTypes.Float, Convert.ToDouble(t.val)); 
			break;
		}
		case 17: {
			Get();
			tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = AstNode.MakeConstant(ObjectTypes.String, tmp);
			
			break;
		}
		case 102: case 103: {
			if (la.kind == 102) {
				Get();
			} else {
				Get();
			}
			expr = AstNode.MakeConstant(ObjectTypes.Bool, Convert.ToBoolean(t.val)); 
			break;
		}
		case 104: {
			Get();
			expr = AstNode.MakeNullRef(); 
			break;
		}
		case 105: {
			Get();
			expr = AstNode.MakeThisRef(); 
			break;
		}
		case 106: {
			Get();
			expr = AstNode.MakeThisRef(); 
			break;
		}
		default: SynErr(141); break;
		}
	}

	void Primary(out Expression expr) {
		expr = null; 
		if (StartOf(16)) {
			Atom(out expr);
			while (la.kind == 7 || la.kind == 8 || la.kind == 12) {
				Trailer(ref expr);
			}
		} else if (la.kind == 99) {
			NewExpression(out expr);
		} else SynErr(142);
	}

	void Argument(out ParameterDeclaration arg) {
		string name; Expression default_val = null; var type = TypeAnnotation.VariantType.Clone(); 
		Expect(13);
		name = t.val; 
		if (la.kind == 73) {
			Get();
			Type(out type);
		}
		if (la.kind == 66) {
			Get();
			Literal(out default_val);
		}
		arg = AstNode.MakeArg(name, type, default_val); 
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		while (la.kind == 79) {
			Get();
			AndTest(out rhs);
			expr = AstNode.MakeBinaryExpr(OperatorType.ConditionalOr, expr, rhs); 
		}
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		NotTest(out expr);
		while (la.kind == 80) {
			Get();
			NotTest(out rhs);
			expr = AstNode.MakeBinaryExpr(OperatorType.ConditionalAnd, expr, rhs); 
		}
	}

	void NotTest(out Expression expr) {
		Expression term; expr = null; 
		if (la.kind == 81) {
			Get();
			NotTest(out term);
			expr = AstNode.MakeUnaryExpr(OperatorType.Not, term); 
		} else if (StartOf(17)) {
			Comparison(out expr);
		} else SynErr(143);
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		BitOr(out expr);
		type = OperatorType.Equality; 
		if (StartOf(18)) {
			switch (la.kind) {
			case 82: {
				Get();
				type = OperatorType.Equality; 
				break;
			}
			case 83: {
				Get();
				type = OperatorType.InEquality; 
				break;
			}
			case 84: {
				Get();
				type = OperatorType.LessThan; 
				break;
			}
			case 85: {
				Get();
				type = OperatorType.GreaterThan; 
				break;
			}
			case 86: {
				Get();
				type = OperatorType.LessThanOrEqual; 
				break;
			}
			case 87: {
				Get();
				type = OperatorType.GreaterThanOrEqual; 
				break;
			}
			}
			BitOr(out rhs);
			expr = AstNode.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void BitOr(out Expression expr) {
		Expression rhs; 
		BitXor(out expr);
		while (la.kind == 88) {
			Get();
			BitXor(out rhs);
			expr = AstNode.MakeBinaryExpr(OperatorType.BitwiseOr, expr, rhs); 
		}
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		while (la.kind == 89) {
			Get();
			BitAnd(out rhs);
			expr = AstNode.MakeBinaryExpr(OperatorType.ExclusiveOr, expr, rhs); 
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOpe(out expr);
		while (la.kind == 90) {
			Get();
			ShiftOpe(out rhs);
			expr = AstNode.MakeBinaryExpr(OperatorType.BitwiseAnd, expr, rhs); 
		}
	}

	void ShiftOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOpe(out expr);
		while (la.kind == 91 || la.kind == 92) {
			if (la.kind == 91) {
				Get();
				type = OperatorType.BitwiseShiftLeft; 
			} else {
				Get();
				type = OperatorType.BitwiseShiftRight; 
			}
			AddOpe(out rhs);
			expr = AstNode.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AddOpe(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		while (la.kind == 93 || la.kind == 94) {
			if (la.kind == 93) {
				Get();
				type = OperatorType.Plus; 
			} else {
				Get();
				type = OperatorType.Minus; 
			}
			Term(out rhs);
			expr = AstNode.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		Factor(out expr);
		while (la.kind == 95 || la.kind == 96 || la.kind == 97) {
			if (la.kind == 95) {
				Get();
				type = OperatorType.Times; 
			} else if (la.kind == 96) {
				Get();
				type = OperatorType.Divide; 
			} else {
				Get();
				type = OperatorType.Modulus; 
			}
			Factor(out rhs);
			expr = AstNode.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Factor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; 
		if (la.kind == 93 || la.kind == 94) {
			if (la.kind == 93) {
				Get();
				type = OperatorType.Plus; 
			} else {
				Get();
				type = OperatorType.Minus; 
			}
			Factor(out factor);
			expr = AstNode.MakeUnaryExpr(type, factor); 
		} else if (StartOf(3)) {
			PowerOpe(out expr);
		} else SynErr(144);
	}

	void PowerOpe(out Expression expr) {
		Expression rhs; 
		Primary(out expr);
		if (la.kind == 98) {
			Get();
			Factor(out rhs);
			expr = AstNode.MakeBinaryExpr(OperatorType.Power, expr, rhs); 
		}
	}

	void Atom(out Expression expr) {
		string name; expr = null; 
		if (la.kind == 13) {
			Get();
			name = t.val;
			expr = AstNode.MakeIdentifier(name);
			
		} else if (StartOf(15)) {
			Literal(out expr);
		} else if (la.kind == 7) {
			Get();
			if (StartOf(12)) {
				SequenceMaker(out expr, ObjectTypes.Tuple);
			}
			while (!(la.kind == 0 || la.kind == 9)) {SynErr(145); Get();}
			Expect(9);
			if(expr == null)
			expr = AstNode.MakeSeqInitializer(ObjectTypes.Tuple, new List<Expression>());
			
		} else if (la.kind == 8) {
			Get();
			if (StartOf(12)) {
				SequenceMaker(out expr, ObjectTypes.List);
			}
			while (!(la.kind == 0 || la.kind == 3)) {SynErr(146); Get();}
			Expect(3);
			if(expr == null)
			expr = AstNode.MakeSeqInitializer(ObjectTypes.List, new List<Expression>());
			
		} else if (la.kind == 6) {
			Get();
			if (StartOf(12)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 10)) {SynErr(147); Get();}
			Expect(10);
			if(expr == null) expr = AstNode.MakeSeqInitializer(ObjectTypes.Dict, new List<Expression>()); 
		} else SynErr(148);
	}

	void Trailer(ref Expression expr) {
		var args = new List<Expression>(); Expression subscript; 
		if (la.kind == 7) {
			Get();
			if(expr is MemberReference)
			                       args.Add(((MemberReference)expr).Target);
			                   else
			                       args.Add(AstNode.MakeConstant(ObjectTypes.Instance, this.TopmostAst));
			               
			if (StartOf(12)) {
				ArgList(ref args);
			}
			Expect(9);
			expr = AstNode.MakeCallExpr(expr, args); 
		} else if (la.kind == 8) {
			Get();
			Subscript(out subscript);
			Expect(3);
			expr = AstNode.MakeIndexer(expr, subscript); 
		} else if (la.kind == 12) {
			Get();
			Expect(13);
			expr = AstNode.MakeMemRef(expr, AstNode.MakeIdent(t.val)); 
		} else SynErr(149);
	}

	void NewExpression(out Expression expr) {
		var args = new List<Expression>(); 
		Expect(99);
		Expect(13);
		expr = AstNode.MakeIdentifier(t.val); 
		while (la.kind == 12) {
			Get();
			Expect(13);
			expr = AstNode.MakeMemRef(expr, AstNode.MakeIdentifier(t.val)); 
		}
		Expect(7);
		if (StartOf(12)) {
			ArgList(ref args);
		}
		Expect(9);
		expr = AstNode.MakeNewExpr(expr, args); 
	}

	void ArgList(ref List<Expression> args ) {
		Expression expr; 
		CondExpr(out expr);
		args.Add(expr); 
		while (WeakSeparator(11,12,8) ) {
			CondExpr(out expr);
			args.Add(expr); 
		}
	}

	void Subscript(out Expression subscript) {
		subscript = null; 
		if (NotFollowedByDoubleDots()) {
			CondExpr(out subscript);
		} else if (la.kind == 8) {
			IntSeqExpr(out subscript);
		} else SynErr(150);
	}

	void SequenceMaker(out Expression expr, ObjectTypes ObjType) {
		Expression tmp = null; List<Expression> list = new List<Expression>();
		expr = null; ComprehensionIter comprehen = null;
		
		CondExpr(out tmp);
		if(tmp != null) list.Add(tmp); 
		if (la.kind == 3 || la.kind == 9 || la.kind == 11) {
			while (WeakSeparator(11,12,19) ) {
				CondExpr(out tmp);
				if(tmp != null) list.Add(tmp); 
			}
			expr = AstNode.MakeSeqInitializer(ObjType, list); 
		} else if (StartOf(12)) {
			CondExpr(out tmp);
			CompFor(out comprehen);
			expr = AstNode.MakeComp(tmp, (ComprehensionForClause)comprehen, ObjType); 
		} else SynErr(151);
	}

	void DictMaker(out Expression expr) {
		Expression key, val; List<Expression> list = new List<Expression>(); expr = null; 
		CondExpr(out key);
		if(key != null) list.Add(key); 
		Expect(4);
		CondExpr(out val);
		if(val != null) list.Add(val); 
		while (la.kind == 11) {
			Get();
			CondExpr(out key);
			if(key != null) list.Add(key); 
			Expect(4);
			CondExpr(out val);
			if(val != null) list.Add(val); 
		}
		if(list.Count > 0) expr = AstNode.MakeSeqInitializer(ObjectTypes.Dict, list); 
	}

	void CompFor(out ComprehensionIter expr) {
		Expression rvalue = null; ComprehensionIter body = null; SequenceExpression target; 
		Expect(19);
		LValueList(out target);
		Expect(18);
		if (NotFollowedByDoubleDots()) {
			CondExpr(out rvalue);
		} else if (la.kind == 8) {
			IntSeqExpr(out rvalue);
		} else SynErr(152);
		if (la.kind == 19 || la.kind == 67) {
			CompIter(out body);
		}
		expr = AstNode.MakeCompFor(target, rvalue, body); 
	}

	void CompIter(out ComprehensionIter expr) {
		expr = null; 
		if (la.kind == 19) {
			CompFor(out expr);
		} else if (la.kind == 67) {
			CompIf(out expr);
		} else SynErr(153);
	}

	void CompIf(out ComprehensionIter expr) {
		Expression tmp; ComprehensionIter body = null; 
		Expect(67);
		OrTest(out tmp);
		if (la.kind == 19 || la.kind == 67) {
			CompIter(out body);
		}
		expr = AstNode.MakeCompIf(tmp, body); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Expresso();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,T, T,T,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,T,T,x, x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, T,x,x,x, x,x,T,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,T,T,x, x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, x,x,T,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,T,T,x, x},
		{x,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,x,T,T, T,T,T,x, x},
		{x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,x, x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,x, x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,x,T,T, T,T,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x}

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
			case 2: s = "triple_dots expected"; break;
			case 3: s = "rbracket expected"; break;
			case 4: s = "colon expected"; break;
			case 5: s = "semicolon expected"; break;
			case 6: s = "lcurly expected"; break;
			case 7: s = "lparen expected"; break;
			case 8: s = "lbracket expected"; break;
			case 9: s = "rparen expected"; break;
			case 10: s = "rcurly expected"; break;
			case 11: s = "comma expected"; break;
			case 12: s = "dot expected"; break;
			case 13: s = "ident expected"; break;
			case 14: s = "integer expected"; break;
			case 15: s = "float expected"; break;
			case 16: s = "hex_digit expected"; break;
			case 17: s = "string_literal expected"; break;
			case 18: s = "keyword_in expected"; break;
			case 19: s = "keyword_for expected"; break;
			case 20: s = "\"export\" expected"; break;
			case 21: s = "\"module\" expected"; break;
			case 22: s = "\"import\" expected"; break;
			case 23: s = "\"as\" expected"; break;
			case 24: s = "\"class\" expected"; break;
			case 25: s = "\"public\" expected"; break;
			case 26: s = "\"protected\" expected"; break;
			case 27: s = "\"private\" expected"; break;
			case 28: s = "\"constructor\" expected"; break;
			case 29: s = "\"destructor\" expected"; break;
			case 30: s = "\"def\" expected"; break;
			case 31: s = "\"->\" expected"; break;
			case 32: s = "\"let\" expected"; break;
			case 33: s = "\"var\" expected"; break;
			case 34: s = "\"Int\" expected"; break;
			case 35: s = "\"Bool\" expected"; break;
			case 36: s = "\"Float\" expected"; break;
			case 37: s = "\"Rational\" expected"; break;
			case 38: s = "\"Bigint\" expected"; break;
			case 39: s = "\"String\" expected"; break;
			case 40: s = "\"Byte\" expected"; break;
			case 41: s = "\"Variadic\" expected"; break;
			case 42: s = "\"Tuple\" expected"; break;
			case 43: s = "\"List\" expected"; break;
			case 44: s = "\"Dictionary\" expected"; break;
			case 45: s = "\"Expression\" expected"; break;
			case 46: s = "\"Function\" expected"; break;
			case 47: s = "\"Intseq\" expected"; break;
			case 48: s = "\"Void\" expected"; break;
			case 49: s = "\"[]\" expected"; break;
			case 50: s = "\"return\" expected"; break;
			case 51: s = "\"break\" expected"; break;
			case 52: s = "\"upto\" expected"; break;
			case 53: s = "\"continue\" expected"; break;
			case 54: s = "\"throw\" expected"; break;
			case 55: s = "\"yield\" expected"; break;
			case 56: s = "\"+=\" expected"; break;
			case 57: s = "\"-=\" expected"; break;
			case 58: s = "\"*=\" expected"; break;
			case 59: s = "\"/=\" expected"; break;
			case 60: s = "\"**=\" expected"; break;
			case 61: s = "\"%=\" expected"; break;
			case 62: s = "\"&=\" expected"; break;
			case 63: s = "\"|=\" expected"; break;
			case 64: s = "\"<<=\" expected"; break;
			case 65: s = "\">>=\" expected"; break;
			case 66: s = "\"=\" expected"; break;
			case 67: s = "\"if\" expected"; break;
			case 68: s = "\"else\" expected"; break;
			case 69: s = "\"while\" expected"; break;
			case 70: s = "\"switch\" expected"; break;
			case 71: s = "\"case\" expected"; break;
			case 72: s = "\"default\" expected"; break;
			case 73: s = "\"(-\" expected"; break;
			case 74: s = "\"with\" expected"; break;
			case 75: s = "\"try\" expected"; break;
			case 76: s = "\"catch\" expected"; break;
			case 77: s = "\"finally\" expected"; break;
			case 78: s = "\"?\" expected"; break;
			case 79: s = "\"||\" expected"; break;
			case 80: s = "\"&&\" expected"; break;
			case 81: s = "\"!\" expected"; break;
			case 82: s = "\"==\" expected"; break;
			case 83: s = "\"!=\" expected"; break;
			case 84: s = "\"<\" expected"; break;
			case 85: s = "\">\" expected"; break;
			case 86: s = "\"<=\" expected"; break;
			case 87: s = "\">=\" expected"; break;
			case 88: s = "\"|\" expected"; break;
			case 89: s = "\"^\" expected"; break;
			case 90: s = "\"&\" expected"; break;
			case 91: s = "\"<<\" expected"; break;
			case 92: s = "\">>\" expected"; break;
			case 93: s = "\"+\" expected"; break;
			case 94: s = "\"-\" expected"; break;
			case 95: s = "\"*\" expected"; break;
			case 96: s = "\"/\" expected"; break;
			case 97: s = "\"%\" expected"; break;
			case 98: s = "\"**\" expected"; break;
			case 99: s = "\"new\" expected"; break;
			case 100: s = "\"l\" expected"; break;
			case 101: s = "\"L\" expected"; break;
			case 102: s = "\"true\" expected"; break;
			case 103: s = "\"false\" expected"; break;
			case 104: s = "\"null\" expected"; break;
			case 105: s = "\"this\" expected"; break;
			case 106: s = "\"base\" expected"; break;
			case 107: s = "??? expected"; break;
			case 108: s = "invalid ModuleBody"; break;
			case 109: s = "invalid ModuleBody"; break;
			case 110: s = "this symbol not expected in ModuleNameDefinition"; break;
			case 111: s = "invalid ExprStmt"; break;
			case 112: s = "this symbol not expected in ExprStmt"; break;
			case 113: s = "this symbol not expected in FuncDecl"; break;
			case 114: s = "this symbol not expected in ClassDecl"; break;
			case 115: s = "this symbol not expected in ClassDecl"; break;
			case 116: s = "this symbol not expected in ImportStmt"; break;
			case 117: s = "this symbol not expected in ImportStmt"; break;
			case 118: s = "this symbol not expected in ConstructorDecl"; break;
			case 119: s = "this symbol not expected in DestructorDecl"; break;
			case 120: s = "this symbol not expected in MethodDecl"; break;
			case 121: s = "invalid FieldDecl"; break;
			case 122: s = "invalid Type"; break;
			case 123: s = "invalid VarDef"; break;
			case 124: s = "invalid Stmt"; break;
			case 125: s = "invalid SimpleStmt"; break;
			case 126: s = "this symbol not expected in CompoundStmt"; break;
			case 127: s = "invalid CompoundStmt"; break;
			case 128: s = "this symbol not expected in ReturnStmt"; break;
			case 129: s = "this symbol not expected in BreakStmt"; break;
			case 130: s = "this symbol not expected in ContinueStmt"; break;
			case 131: s = "this symbol not expected in ThrowStmt"; break;
			case 132: s = "this symbol not expected in YieldStmt"; break;
			case 133: s = "this symbol not expected in EmptyStmt"; break;
			case 134: s = "invalid AugAssignOpe"; break;
			case 135: s = "invalid VarDecl"; break;
			case 136: s = "this symbol not expected in LValueList"; break;
			case 137: s = "invalid ForStmt"; break;
			case 138: s = "invalid IntSeqExpr"; break;
			case 139: s = "invalid CaseLabel"; break;
			case 140: s = "this symbol not expected in CaseLabel"; break;
			case 141: s = "invalid Literal"; break;
			case 142: s = "invalid Primary"; break;
			case 143: s = "invalid NotTest"; break;
			case 144: s = "invalid Factor"; break;
			case 145: s = "this symbol not expected in Atom"; break;
			case 146: s = "this symbol not expected in Atom"; break;
			case 147: s = "this symbol not expected in Atom"; break;
			case 148: s = "invalid Atom"; break;
			case 149: s = "invalid Trailer"; break;
			case 150: s = "invalid Subscript"; break;
			case 151: s = "invalid SequenceMaker"; break;
			case 152: s = "invalid CompFor"; break;
			case 153: s = "invalid CompIter"; break;

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
