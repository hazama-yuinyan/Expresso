using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using Expresso.Ast;
using Expresso.Compiler.Meta;
using Expresso.Parsing;
using Expresso.Runtime;
using Expresso.Utils;

using ICSharpCode.NRefactory;





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
	public const int maxT = 104;

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
    public TextLocation CurrentTokenLocation{
        get{
            return new TextLocation(t.line, t.col);
        }
    }

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
	
	LiteralExpression CreateConstant(KnownTypeCode type)
	{
		LiteralExpression result = null;
		var loc = CurrentTokenLocation;
        var primitive_type = new PrimitiveType(type, loc);

		switch(type){
		case ObjectTypes.Integer:
			result = Expression.MakeConstant(primitive_type, 0);
			break;
			
		case ObjectTypes.Bool:
			result = Expression.MakeConstant(primitive_type, false);
			break;
			
		case ObjectTypes.Float:
			result = Expression.MakeConstant(primitive_type, 0.0);
			break;
			
		case ObjectTypes.String:
			result = Expression.MakeConstant(primitive_type, "");
			break;
			
		default:
			SemErr("Unknown object type");
			break;
		}
		
		return result;
	}
	
	/*bool NotFollowedByDoubleDots()
	{
		Token x = la;
		scanner.ResetPeek();
		if(x.kind != _lbracket)
            return true;
		
        while(x.kind != 0 && x.kind != _double_dots && x.kind != _rbracket && x.kind != _semicolon && x.kind != _rparen && x.kind != _keyword_in)
			x = scanner.Peek();
		
		return x.kind != _double_dots;
	}*/
	
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
		List<ImportDeclaration> prog_defs = null; Statement stmt = null;
		
		ModuleNameDefinition(out module_name);
		if (la.kind == 22) {
			ProgramDefinition(out prog_defs);
		}
		if (la.kind == 20) {
			Get();
			modifiers = Modifiers.Export; 
		}
		if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 29) {
			FuncDecl(out stmt, modifiers);
		} else if (la.kind == 24) {
			ClassDecl(out stmt, modifiers);
		} else SynErr(105);
		decls.Add(stmt);
		modifiers = Modifiers.None;
		
		while (StartOf(2)) {
			if (la.kind == 20) {
				Get();
				modifiers = Modifiers.Export; 
			}
			if (StartOf(1)) {
				ExprStmt(out stmt);
			} else if (la.kind == 29) {
				FuncDecl(out stmt, modifiers);
			} else if (la.kind == 24) {
				ClassDecl(out stmt, modifiers);
			} else SynErr(106);
			decls.Add(stmt);
			modifiers = Modifiers.None;
			
		}
		decl = AstNode.MakeModuleDef(module_name, decls, prog_defs);
		
	}

	void ModuleNameDefinition(out string moduleName) {
		Expect(21);
		Expect(13);
		moduleName = t.val; 
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(107); Get();}
		Expect(5);
	}

	void ProgramDefinition(out List<ImportDeclaration> imports ) {
		imports = new List<ImportDeclatation>();
		ImportDeclaration tmp;
		
		ImportDecl(out tmp);
		imports.Add(tmp); 
		while (la.kind == 22) {
			ImportDecl(out tmp);
			imports.Add(tmp); 
		}
	}

	void ExprStmt(out Statement stmt) {
		List<Expression> lvalues = null; SequenceExpression targets = null;
		SequenceExpression seq = null;
		stmt = null; OperatorType op_type = OperatorType.None;
		
		if (la.kind == 31 || la.kind == 32) {
			VarDecl(out lvalues);
		} else if (StartOf(3)) {
			LValueList(out targets);
			while (StartOf(4)) {
				if (StartOf(5)) {
					AugAssignOpe(ref op_type);
				} else {
					Get();
				}
				RValueList(out seq);
			}
			if(targets.Count != seq.Count)      //See if both sides have the same number of items
			                                  SemErr("An augumented assignment must have both sides balanced.");
				
			if(op_type != OperatorType.None)
			stmt = Expression.MakeAugumentedAssignment(targets, seq, op_type);
			else
			stmt = Expression.MakeAssignment(targets, seq);
			
		} else SynErr(108);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(109); Get();}
		Expect(5);
		if(stmt == null)
		stmt = Statement.MakeExprStmt(lvalues);
		
	}

	void FuncDecl(out Statement func) {
		string name; var type = TypeAnnotation.VariantType.Clone(); BlockStatement block = null;
		var @params = new List<ParameterDeclaration>();
		
		while (!(la.kind == 0 || la.kind == 29)) {SynErr(110); Get();}
		Expect(29);
		Expect(13);
		name = t.val; 
		Expect(7);
		if (la.kind == 13) {
			ParamList(out @params);
		}
		Expect(9);
		if (la.kind == 30) {
			Get();
			Type(out type);
		}
		Block(out block);
		func = AstNode.MakeFunc(name, @params, block, type); 
	}

	void ClassDecl(out EntityDeclaration decl, Modifiers modifiers) {
		EntityDeclaration entity = null; var decls = new List<EntityDeclaration>();
		string name; var base_names = new List<Identifier>(); Modifiers cur_flag;
		
		while (!(la.kind == 0 || la.kind == 24)) {SynErr(111); Get();}
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
			Modifiers(out cur_flag);
			if (la.kind == 29) {
				MethodDecl(out entity, name, cur_flag);
				decls.Add(entity); 
			} else if (la.kind == 31 || la.kind == 32) {
				FieldDecl(out entity, cur_flag);
				Expect(5);
				decls.Add(entity); 
			} else if (la.kind == 24) {
				ClassDecl(out entity);
				decls.Add(entity); 
			} else SynErr(112);
		}
		while (!(la.kind == 0 || la.kind == 10)) {SynErr(113); Get();}
		Expect(10);
		decl = AstNode.MakeClassDecl(name, base_names, decls, modifiers); 
	}

	void ImportDecl(out ImportDeclaration decl) {
		string module_name, alias = null; var module_names = new List<string>();
		var aliases = new List<string>();
		
		while (!(la.kind == 0 || la.kind == 22)) {SynErr(114); Get();}
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
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(115); Get();}
		Expect(5);
		stmt = AstNode.MakeImportDecl(module_names, aliases); 
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

	void Modifiers(out Modifiers modifiers) {
		modifiers = Modifiers.None; 
		while (StartOf(7)) {
			if (la.kind == 25) {
				Get();
				modifiers |= Modifiers.Public; 
			} else if (la.kind == 26) {
				Get();
				modifiers |= Modifiers.Protected; 
			} else if (la.kind == 27) {
				Get();
				modifiers |= Modifiers.Private; 
			} else {
				Get();
				modifiers |= Modifiers.Static; 
			}
		}
		if(modifiers == Modifiers.None) modifiers.Private; 
	}

	void MethodDecl(out EntityDeclaration decl, string className, Modifiers modifiers) {
		string name; var type = TypeAnnotation.InferenceType.Clone(); BlockStatement block;
		var @params = new List<Argument>();
		
		while (!(la.kind == 0 || la.kind == 29)) {SynErr(116); Get();}
		Expect(29);
		Expect(13);
		name = t.val; 
		Expect(7);
		if (la.kind == 13) {
			ParamList(ref @params);
		}
		Expect(9);
		if (la.kind == 30) {
			Get();
			Type(out type);
		}
		Block(out block);
		decl = AstNode.MakeFunc(name, @params, block, type, modifiers); 
	}

	void FieldDecl(out List<EntityDeclaration> outs, Modifiers modifiers ) {
		string name; var type = TypeAnnotation.InferenceType.Clone(); Expression rhs = null;
		Identifier variable; outs = new List<Expression>();
		var vars = new List<Identifier>(); var exprs = new List<Expression>();
		
		if (la.kind == 31) {
			Get();
		} else if (la.kind == 32) {
			Get();
		} else SynErr(117);
		VarDef(out name, out type, out rhs);
		variable = AstNode.MakeField(name, type);
		vars.Add(variable);
		exprs.Add(rhs);
		rhs = null;
		
		while (la.kind == 11) {
			Get();
			VarDef(out name, out type, out rhs);
			variable = AstNode.MakeField(name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			
		}
		outs.Add(AstNode.MakeVarDecl(vars, exprs, flag)); 
	}

	void ParamList(out List<ParameterDeclaration> @params ) {
		@params = new List<ParameterDeclaration>(); ParameterDeclaration expr; 
		Parameter(out expr);
		@params.Add(expr); 
		while (WeakSeparator(11,8,9) ) {
			Parameter(out expr);
			@params.Add(expr); 
		}
	}

	void Type(out TypeAnnotation type) {
		type = TypeAnnotation.InferenceType.Clone(); int dimension = 0; 
		switch (la.kind) {
		case 33: {
			Get();
			type.ObjType = ObjectTypes.Integer; 
			break;
		}
		case 34: {
			Get();
			type.ObjType = ObjectTypes.Bool; 
			break;
		}
		case 35: {
			Get();
			type.ObjType = ObjectTypes.Float; 
			break;
		}
		case 36: {
			Get();
			type.ObjType = ObjectTypes.Double; 
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
		case 7: {
			TupleTypeSignature(out type);
			break;
		}
		case 41: {
			Get();
			type.ObjType = ObjectTypes.List; 
			break;
		}
		case 42: {
			Get();
			type.ObjType = ObjectTypes.Dict; 
			break;
		}
		case 43: {
			Get();
			type.ObjType = ObjectTypes.Expression; 
			break;
		}
		case 44: {
			Get();
			type.ObjType = ObjectTypes.Function; 
			break;
		}
		case 45: {
			Get();
			type.ObjType = ObjectTypes.Seq; 
			break;
		}
		case 46: {
			Get();
			type.ObjType = ObjectTypes.Undef; 
			break;
		}
		case 13: {
			Get();
			type.ObjType = ObjectTypes.Instance; type.TypeName = t.val; 
			break;
		}
		default: SynErr(118); break;
		}
		while (la.kind == 47) {
			Get();
			if(!type.IsArray)
			   type.IsArray = true;
			
			++dimension;
			
		}
		type.Dimension = dimension; 
	}

	void Block(out BlockStatement block) {
		List<Statement> stmts = new List<Statement>(); Statement stmt; 
		Expect(6);
		Stmt(out stmt);
		stmts.Add(stmt); 
		while (StartOf(10)) {
			Stmt(out stmt);
			stmts.Add(stmt); 
		}
		Expect(10);
		block = Statement.MakeBlock(stmts); 
	}

	void VarDef(out string name, out Type type, out Expression @default) {
		type = TypeAnnotation.InferenceType.Clone();
		@default = null;
		
		Expect(13);
		name = t.val; 
		if (la.kind == 71) {
			Get();
			Type(out type);
		}
		if (la.kind == 64) {
			Get();
			CondExpr(out @default);
		}
	}

	void TupleTypeSignature(out TypeAnnotation type) {
		TypeAnnotation inner; 
		Expect(7);
		while (StartOf(11)) {
			Type(out inner);
		}
		Expect(9);
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(12)) {
			SimpleStmt(out stmt);
		} else if (StartOf(13)) {
			CompoundStmt(out stmt);
		} else SynErr(119);
	}

	void SimpleStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == _lcurly) {
			Block(out stmt);
		} else if (StartOf(1)) {
			ExprStmt(out stmt);
		} else if (la.kind == 48) {
			ReturnStmt(out stmt);
		} else if (la.kind == 49) {
			BreakStmt(out stmt);
		} else if (la.kind == 51) {
			ContinueStmt(out stmt);
		} else if (la.kind == 52) {
			ThrowStmt(out stmt);
		} else if (la.kind == 53) {
			YieldStmt(out stmt);
		} else if (la.kind == 5) {
			EmptyStmt(out stmt);
		} else SynErr(120);
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; 
		switch (la.kind) {
		case 65: {
			while (!(la.kind == 0 || la.kind == 65)) {SynErr(121); Get();}
			IfStmt(out stmt);
			break;
		}
		case 67: {
			WhileStmt(out stmt);
			break;
		}
		case 19: {
			ForStmt(out stmt);
			break;
		}
		case 68: {
			MatchStmt(out stmt);
			break;
		}
		case 29: {
			FuncDecl(out stmt);
			break;
		}
		case 72: {
			TryStmt(out stmt);
			break;
		}
		default: SynErr(122); break;
		}
	}

	void ReturnStmt(out Statement stmt) {
		SequenceExpression items = null; 
		Expect(48);
		if (StartOf(14)) {
			RValueList(out items);
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(123); Get();}
		Expect(5);
		stmt = AstNode.MakeReturnStmt(items); 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; 
		Expect(49);
		if (la.kind == 50) {
			Get();
			Expect(14);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(124); Get();}
		Expect(5);
		stmt = MakeBreakStatement(count); 
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; 
		Expect(51);
		if (la.kind == 50) {
			Get();
			Expect(14);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(125); Get();}
		Expect(5);
		stmt = MakeContinueStatement(count); 
	}

	void ThrowStmt(out Statement stmt) {
		Expression expr; 
		Expect(52);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(126); Get();}
		Expect(5);
		stmt = AstNode.MakeThrowStmt(expr); 
	}

	void YieldStmt(out Statement stmt) {
		Expression expr; 
		Expect(53);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(127); Get();}
		Expect(5);
		stmt = AstNode.MakeYieldStmt(expr); 
	}

	void EmptyStmt(out Statement stmt) {
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(128); Get();}
		Expect(5);
		stmt = AstNode.MakeEmptyStmt(); 
	}

	void RValueList(out SequenceExpression seq) {
		Expression tmp; var exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (WeakSeparator(11,14,15) ) {
			CondExpr(out tmp);
			exprs.Add(tmp);	
		}
		seq = Expression.MakeSequence(exprs); 
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; 
		OrTest(out expr);
		if (la.kind == 76) {
			Get();
			OrTest(out true_expr);
			Expect(4);
			CondExpr(out false_expr);
			expr = Expression.MakeCondExpr(expr, true_expr, false_expr); 
		}
	}

	void AugAssignOpe(ref OperatorType type) {
		switch (la.kind) {
		case 54: {
			Get();
			type = OperatorType.Plus; 
			break;
		}
		case 55: {
			Get();
			type = OperatorType.Minus; 
			break;
		}
		case 56: {
			Get();
			type = OperatorType.Times; 
			break;
		}
		case 57: {
			Get();
			type = OperatorType.Divide; 
			break;
		}
		case 58: {
			Get();
			type = OperatorType.Power; 
			break;
		}
		case 59: {
			Get();
			type = OperatorType.Modulus; 
			break;
		}
		case 60: {
			Get();
			type = OperatorType.BitwiseAnd; 
			break;
		}
		case 61: {
			Get();
			type = OperatorType.BitwiseOr; 
			break;
		}
		case 62: {
			Get();
			type = OperatorType.BitwiseShiftLeft; 
			break;
		}
		case 63: {
			Get();
			type = OperatorType.BitwiseShiftRight; 
			break;
		}
		default: SynErr(129); break;
		}
	}

	void VarDecl(out List<Expression> outs ) {
		string name; var type = TypeAnnotation.InferenceType.Clone(); Expression rhs = null;
		Identifier variable; outs = new List<Expression>();
		var vars = new List<Identifier>(); var exprs = new List<Expression>();
		bool is_const = false;
		
		if (la.kind == 31) {
			Get();
			is_const = true; 
		} else if (la.kind == 32) {
			Get();
		} else SynErr(130);
		VarDef(out name, out type, out rhs);
		variable = AstNode.MakeLocalVar(name, type);
		 vars.Add(variable);
		exprs.Add(rhs);
		rhs = null;
		
		while (WeakSeparator(11,8,16) ) {
			VarDef(out name, out type, out rhs);
			variable = AstNode.MakeLocalVar(name, type);
			vars.Add(variable);
			exprs.Add(rhs);
			rhs = null;
			
		}
		outs.Add(Statement.MakeVarDecl(vars, exprs)); 
	}

	void LValueList(out SequenceExpression target) {
		Expression tmp; var exprs = new List<Expression>(); 
		Primary(out tmp);
		exprs.Add(tmp); 
		while (la.kind == 11) {
			while (!(la.kind == 0 || la.kind == 11)) {SynErr(131); Get();}
			Get();
			Primary(out tmp);
			exprs.Add(tmp); 
		}
		target = Expression.MakeSequence(exprs); 
	}

	void IfStmt(out Statement stmt) {
		Expression tmp; BlockStatement true_block, false_block = null; 
		Expect(65);
		CondExpr(out tmp);
		Block(out true_block);
		if (la.kind == 66) {
			Get();
			Block(out false_block);
		}
		stmt = Statement.MakeIfStmt(tmp, true_block, false_block); 
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; BlockStatement body; 
		Expect(67);
		CondExpr(out cond);
		Block(out body);
		stmt = Statement.MakeWhileStmt(cond, body); 
	}

	void ForStmt(out Statement stmt) {
		PatternConstruct left; Expression rvalue; BlockStatement body;
		
		Expect(19);
		LhsPattern(out left);
		Expect(18);
		CondExpr(out rvalue);
		Block(out body);
		stmt = Statement.MakeForStmt(left, rvalue, body); 
	}

	void MatchStmt(out Statement stmt) {
		PatternConstruct target; List<MatchClause> matches; 
		Expect(68);
		LhsPattern(out target);
		Expect(6);
		MatchPatternList(out matches);
		Expect(10);
		stmt = Statement.MakeMatchStmt(target, matches); 
	}

	void TryStmt(out Statement stmt) {
		BlockStatement body, catch_body = null, finally_body = null; List<CatchClause> catches = null;
		TypeAnnotation excp_type = null; Identifier catch_ident = null; string name = null;
		
		Expect(72);
		Block(out body);
		while (la.kind == 73) {
			Get();
			if(catches == null) catches = new List<CatchClause>(); 
			Expect(13);
			name = t.val; 
			Expect(71);
			Type(out excp_type);
			catch_ident = AstNode.MakeLocalVar(name, excp_type); 
			Block(out catch_body);
			catches.Add(AstNode.MakeCatchClause(catch_body, catch_ident)); 
		}
		if (la.kind == 74) {
			Get();
			Block(out finally_body);
		}
		stmt = Statement.MakeTryStmt(body, catches, finally_body); 
	}

	void LhsPattern(out PatternConstruct pattern) {
		if (la.kind == 75) {
			WildcardPattern(out pattern);
		} else if (la.kind == 13) {
			IdentifierPattern(out pattern);
		} else if (la.kind == 31 || la.kind == 32) {
			ValueBindingPattern(out pattern);
		} else SynErr(132);
	}

	void MatchPatternList(out List<MatchClause> clauses ) {
		clauses = new List<MatchClause>(); List<PatternConstruct> pattern_list; Statement inner; 
		PatternList(out pattern_list);
		Stmt(out inner);
		clauses.Add(AstNode.MakeMatchClause(pattern_list, inner)); 
		while (StartOf(17)) {
			PatternList(out pattern_list);
			Stmt(out inner);
			clauses.Add(AstNode.MakeMatchClause(pattern_list, inner)); 
		}
	}

	void PatternList(out List<PatternConstruct> patterns ) {
		patterns = new List<PatternConstruct>(); PatternConstruct tmp; 
		Pattern(out tmp);
		patterns.Add(tmp); 
		while (la.kind == 69) {
			while (!(la.kind == 0 || la.kind == 69)) {SynErr(133); Get();}
			Get();
			Pattern(out tmp);
			patterns.Add(tmp); 
		}
		Expect(70);
	}

	void Pattern(out PatternConstruct pattern) {
		if (StartOf(18)) {
			LhsPattern(out pattern);
		} else if (StartOf(14)) {
			ExpressionPattern(out pattern);
		} else SynErr(134);
	}

	void Primary(out Expression expr) {
		expr = null; 
		if (StartOf(3)) {
			Atom(out expr);
		} else if (la.kind == 96) {
			NewExpression(out expr);
		} else if (la.kind == 13) {
			ObjectCreation(out expr);
		} else SynErr(135);
		while (la.kind == 7 || la.kind == 8 || la.kind == 12) {
			Trailer(ref expr);
		}
	}

	void Parameter(out ParameterDeclaration arg) {
		string name; Expression default_val = null; var type = TypeAnnotation.VariantType.Clone(); 
		Expect(13);
		name = t.val; 
		Expect(71);
		Type(out type);
		if (la.kind == 64) {
			Get();
			Literal(out default_val);
		}
		arg = AstNode.MakeArg(name, type, default_val); 
	}

	void Literal(out Expression expr) {
		expr = null; string tmp; bool has_suffix = false; 
		switch (la.kind) {
		case 14: {
			Get();
			tmp = t.val; 
			if (la.kind == 97 || la.kind == 98) {
				if (la.kind == 97) {
					Get();
					has_suffix = true; 
				} else {
					Get();
					has_suffix = true; 
				}
			}
			if(has_suffix)
			expr = AstNode.MakeConstant(ObjectTypes.BigInteger, BigInteger.Parse(tmp));
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
		case 99: case 100: {
			if (la.kind == 99) {
				Get();
			} else {
				Get();
			}
			expr = AstNode.MakeConstant(ObjectTypes.Bool, Convert.ToBoolean(t.val)); 
			break;
		}
		case 101: {
			Get();
			expr = AstNode.MakeNullRef(); 
			break;
		}
		case 102: {
			Get();
			expr = AstNode.MakeThisRef(); 
			break;
		}
		case 103: {
			Get();
			expr = AstNode.MakeBaseRef(); 
			break;
		}
		default: SynErr(136); break;
		}
	}

	void ExpressionPattern(out PatternConstruct expr_pattern) {
		Expression expr; 
		CondExpr(out expr);
		expr_pattern = PatternConstruct.MakeExpressionPattern(expr); 
	}

	void WildcardPattern(out PatternConstruct pattern) {
		Expect(75);
		pattern = PatternConstruct.MakeWildcardPattern(); 
	}

	void IdentifierPattern(out PatternConstruct pattern) {
		var type = TypeAnnotation.InferenceType.Clone(); 
		Expect(13);
		if (la.kind == 71) {
			Get();
			Type(ref type);
		}
		pattern = PatternConstruct.MakeIdentifierPattern(t.val, type); 
	}

	void ValueBindingPattern(out PatternConstruct pattern) {
		PatternConstruct inner; 
		if (la.kind == 31) {
			Get();
		} else if (la.kind == 32) {
			Get();
		} else SynErr(137);
		Pattern(out inner);
		pattern = PatternConstruct.MakeValueBindingPattern(inner); 
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		if (la.kind == 77) {
			Get();
			OrTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalOr, expr, rhs); 
		}
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		NotTest(out expr);
		if (la.kind == 78) {
			Get();
			AndTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalAnd, expr, rhs); 
		}
	}

	void NotTest(out Expression expr) {
		Expression term; expr = null; 
		if (la.kind == 79) {
			Get();
			NotTest(out term);
			expr = Expression.MakeUnaryExpr(OperatorType.Not, term); 
		} else if (StartOf(3)) {
			Comparison(out expr);
		} else SynErr(138);
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		BitOr(out expr);
		type = OperatorType.Equality; 
		if (StartOf(19)) {
			switch (la.kind) {
			case 80: {
				Get();
				type = OperatorType.Equality; 
				break;
			}
			case 81: {
				Get();
				type = OperatorType.InEquality; 
				break;
			}
			case 82: {
				Get();
				type = OperatorType.LessThan; 
				break;
			}
			case 83: {
				Get();
				type = OperatorType.GreaterThan; 
				break;
			}
			case 84: {
				Get();
				type = OperatorType.LessThanOrEqual; 
				break;
			}
			case 85: {
				Get();
				type = OperatorType.GreaterThanOrEqual; 
				break;
			}
			}
			Comparison(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void BitOr(out Expression expr) {
		Expression rhs; 
		BitXor(out expr);
		if (la.kind == 69) {
			Get();
			BitOr(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseOr, expr, rhs); 
		}
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		if (la.kind == 86) {
			Get();
			BitXor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ExclusiveOr, expr, rhs); 
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOp(out expr);
		if (la.kind == 87) {
			Get();
			BitAnd(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseAnd, expr, rhs); 
		}
	}

	void ShiftOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOp(out expr);
		if (la.kind == 88 || la.kind == 89) {
			if (la.kind == 88) {
				Get();
				type = OperatorType.BitwiseShiftLeft; 
			} else {
				Get();
				type = OperatorType.BitwiseShiftRight; 
			}
			ShiftOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AddOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		if (la.kind == 90 || la.kind == 91) {
			if (la.kind == 90) {
				Get();
				type = OperatorType.Plus; 
			} else {
				Get();
				type = OperatorType.Minus; 
			}
			AddOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		Factor(out expr);
		if (la.kind == 92 || la.kind == 93 || la.kind == 94) {
			if (la.kind == 92) {
				Get();
				type = OperatorType.Times; 
			} else if (la.kind == 93) {
				Get();
				type = OperatorType.Divide; 
			} else {
				Get();
				type = OperatorType.Modulus; 
			}
			Term(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void Factor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; 
		if (StartOf(3)) {
			PowerOp(out expr);
		} else if (la.kind == 90 || la.kind == 91) {
			if (la.kind == 90) {
				Get();
				type = OperatorType.Plus; 
			} else {
				Get();
				type = OperatorType.Minus; 
			}
			Factor(out factor);
			expr = Expression.MakeUnaryExpr(type, factor); 
		} else SynErr(139);
	}

	void PowerOp(out Expression expr) {
		Expression rhs; 
		Primary(out expr);
		if (la.kind == 95) {
			Get();
			Factor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.Power, expr, rhs); 
		}
	}

	void Atom(out Expression expr) {
		string name; expr = null; 
		if (la.kind == 13) {
			Get();
			name = t.val;
			expr = AstNode.MakeIdentifier(name);
			
		} else if (StartOf(20)) {
			Literal(out expr);
		} else if (StartOf(3)) {
			IntSeqExpr(out expr);
		} else if (la.kind == 7) {
			Get();
			CondExpr(out expr);
			Expect(9);
			expr = Expression.MakeParenExpr(expr); 
		} else if (la.kind == 7) {
			Get();
			if (StartOf(14)) {
				SequenceMaker(out expr, ObjectTypes.Tuple);
			}
			while (!(la.kind == 0 || la.kind == 9)) {SynErr(140); Get();}
			Expect(9);
			if(expr == null)
			expr = Expression.MakeSeqInitializer(ObjectTypes.Tuple, new List<Expression>());
			
		} else if (la.kind == 8) {
			Get();
			if (StartOf(14)) {
				SequenceMaker(out expr, ObjectTypes.List);
			}
			while (!(la.kind == 0 || la.kind == 3)) {SynErr(141); Get();}
			Expect(3);
			if(expr == null)
			expr = Expression.MakeSeqInitializer(ObjectTypes.List, new List<Expression>());
			
		} else if (la.kind == 6) {
			Get();
			if (StartOf(14)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 10)) {SynErr(142); Get();}
			Expect(10);
			if(expr == null) expr = Expression.MakeSeqInitializer(ObjectTypes.Dict, new List<Expression>()); 
		} else SynErr(143);
	}

	void NewExpression(out Expression expr) {
		Expect(96);
		ObjectCreation(out expr);
		expr = Expression.MakeNewExpr(expr); 
	}

	void ObjectCreation(out Expression expr) {
		var fields = new List<Identifier>(); var values = new List<Expression>(); 
		Expect(13);
		expr = AstNode.MakeIdentifier(t.val); 
		while (la.kind == 12) {
			Get();
			Expect(13);
			expr = Expression.MakeMemRef(expr, AstNode.MakeIdentifier(t.val)); 
		}
		Expect(6);
		while (la.kind == 13) {
			Get();
			fields.Add(AstNode.MakeIdentifier(t.val)); 
			Expect(4);
			OrTest(out expr);
			values.Add(expr); 
		}
		Expect(10);
		Expression.MakeObjectCreation(fields, values); 
	}

	void Trailer(ref Expression expr) {
		var args = new List<Expression>(); 
		if (la.kind == 7) {
			Get();
			if (StartOf(14)) {
				ArgList(ref args);
			}
			Expect(9);
			expr = Expression.MakeCallExpr(expr, args); 
		} else if (la.kind == 8) {
			Get();
			ArgList(ref args);
			Expect(3);
			expr = Expression.MakeIndexer(expr, args); 
		} else if (la.kind == 12) {
			Get();
			Expect(13);
			expr = Expression.MakeMemRef(expr, AstNode.MakeIdent(t.val)); 
		} else SynErr(144);
	}

	void ArgList(ref List<Expression> args ) {
		Expression expr; 
		CondExpr(out expr);
		args.Add(expr); 
		while (WeakSeparator(11,14,21) ) {
			CondExpr(out expr);
			args.Add(expr); 
		}
	}

	void IntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null;
		bool upper_inclusive = true;
		
		if (StartOf(3)) {
			BitOr(out start);
		}
		if (la.kind == 1) {
			Get();
			upper_inclusive = false; 
		} else if (la.kind == 2) {
			Get();
		} else SynErr(145);
		BitOr(out end);
		if (la.kind == 4) {
			Get();
			BitOr(out step);
		}
		if(start == null) start = CreateConstant(ObjectTypes.Integer);
		if(step == null) step = Expression.MakeConstant(ObjectTypes.Integer, 1);
		expr = Expression.MakeIntSeq(start, end, step, upper_inclusive);
		
	}

	void SequenceMaker(out Expression expr, ObjectTypes ObjType) {
		Expression tmp = null; List<Expression> list = new List<Expression>();
		expr = null; ComprehensionIter comprehen = null;
		
		CondExpr(out tmp);
		if(tmp != null) list.Add(tmp); 
		if (la.kind == 3 || la.kind == 9 || la.kind == 11) {
			while (WeakSeparator(11,14,21) ) {
				CondExpr(out tmp);
				if(tmp != null) list.Add(tmp); 
			}
			expr = Expression.MakeSeqInitializer(ObjType, list); 
		} else if (StartOf(14)) {
			CondExpr(out tmp);
			CompFor(out comprehen);
			expr = Expression.MakeComp(tmp, (ComprehensionForClause)comprehen, ObjType); 
		} else SynErr(146);
	}

	void DictMaker(out Expression expr) {
		Expression key, val; List<Expression> list = new List<Expression>(); expr = null;
		      ComprehensionIter comp_for, comp_val;
		   
		if (StartOf(14)) {
			OrTest(out key);
			if(key != null) list.Add(key); 
			Expect(4);
			OrTest(out val);
			if(val != null) list.Add(val); 
			while (WeakSeparator(11,14,22) ) {
				OrTest(out key);
				if(key != null) list.Add(key); 
				Expect(4);
				OrTest(out val);
				if(val != null) list.Add(val); 
			}
			if(list.Count > 0) expr = Expression.MakeSeqInitializer(ObjectTypes.Dict, list); 
		} else if (StartOf(14)) {
			CondExpr(out key);
			CompFor(out comp_key);
			comp_key = Expression.MakeComp(key, (ComprehensionForClause)comp_key, Objtype); 
			Expect(4);
			CondExpr(out val);
			CompFor(out comp_val);
			comp_val = Expression.MakeComp(val, (ComprehensionForClause)comp_val);
			expr = Expression.MakeSeqInitializer(ObjType, new []{comp_key, comp_val});
			
		} else SynErr(147);
	}

	void CompFor(out ComprehensionIter expr) {
		Expression rvalue = null; ComprehensionIter body = null; PatternConstruct target; 
		Expect(19);
		LhsPattern(out target);
		Expect(18);
		CondExpr(out rvalue);
		if (la.kind == 19 || la.kind == 65) {
			CompIter(out body);
		}
		expr = Expression.MakeCompFor(target, rvalue, body); 
	}

	void CompIter(out ComprehensionIter expr) {
		expr = null; 
		if (la.kind == 19) {
			CompFor(out expr);
		} else if (la.kind == 65) {
			CompIf(out expr);
		} else SynErr(148);
	}

	void CompIf(out ComprehensionIter expr) {
		Expression tmp; ComprehensionIter body = null; 
		Expect(65);
		OrTest(out tmp);
		if (la.kind == 19 || la.kind == 65) {
			CompIter(out body);
		}
		expr = Expression.MakeCompIf(tmp, body); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Expresso();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,T, x,T,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,T, T,T,T,T, x,x},
		{x,T,T,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, T,x,x,x, x,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,T, T,T,T,T, x,x},
		{x,T,T,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,T, T,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,T,T,T, T,x,x,x, x,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,T, T,T,T,T, x,x},
		{x,x,x,x, x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,T, T,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,T, T,T,T,T, x,x},
		{x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,T, T,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,T, x,x},
		{x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x}

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
			case 28: s = "\"static\" expected"; break;
			case 29: s = "\"def\" expected"; break;
			case 30: s = "\"->\" expected"; break;
			case 31: s = "\"let\" expected"; break;
			case 32: s = "\"var\" expected"; break;
			case 33: s = "\"int\" expected"; break;
			case 34: s = "\"bool\" expected"; break;
			case 35: s = "\"float\" expected"; break;
			case 36: s = "\"double\" expected"; break;
			case 37: s = "\"rational\" expected"; break;
			case 38: s = "\"bigint\" expected"; break;
			case 39: s = "\"string\" expected"; break;
			case 40: s = "\"byte\" expected"; break;
			case 41: s = "\"list\" expected"; break;
			case 42: s = "\"dictionary\" expected"; break;
			case 43: s = "\"expression\" expected"; break;
			case 44: s = "\"function\" expected"; break;
			case 45: s = "\"intseq\" expected"; break;
			case 46: s = "\"void\" expected"; break;
			case 47: s = "\"[]\" expected"; break;
			case 48: s = "\"return\" expected"; break;
			case 49: s = "\"break\" expected"; break;
			case 50: s = "\"upto\" expected"; break;
			case 51: s = "\"continue\" expected"; break;
			case 52: s = "\"throw\" expected"; break;
			case 53: s = "\"yield\" expected"; break;
			case 54: s = "\"+=\" expected"; break;
			case 55: s = "\"-=\" expected"; break;
			case 56: s = "\"*=\" expected"; break;
			case 57: s = "\"/=\" expected"; break;
			case 58: s = "\"**=\" expected"; break;
			case 59: s = "\"%=\" expected"; break;
			case 60: s = "\"&=\" expected"; break;
			case 61: s = "\"|=\" expected"; break;
			case 62: s = "\"<<=\" expected"; break;
			case 63: s = "\">>=\" expected"; break;
			case 64: s = "\"=\" expected"; break;
			case 65: s = "\"if\" expected"; break;
			case 66: s = "\"else\" expected"; break;
			case 67: s = "\"while\" expected"; break;
			case 68: s = "\"match\" expected"; break;
			case 69: s = "\"|\" expected"; break;
			case 70: s = "\"=>\" expected"; break;
			case 71: s = "\"(-\" expected"; break;
			case 72: s = "\"try\" expected"; break;
			case 73: s = "\"catch\" expected"; break;
			case 74: s = "\"finally\" expected"; break;
			case 75: s = "\"_\" expected"; break;
			case 76: s = "\"?\" expected"; break;
			case 77: s = "\"||\" expected"; break;
			case 78: s = "\"&&\" expected"; break;
			case 79: s = "\"!\" expected"; break;
			case 80: s = "\"==\" expected"; break;
			case 81: s = "\"!=\" expected"; break;
			case 82: s = "\"<\" expected"; break;
			case 83: s = "\">\" expected"; break;
			case 84: s = "\"<=\" expected"; break;
			case 85: s = "\">=\" expected"; break;
			case 86: s = "\"^\" expected"; break;
			case 87: s = "\"&\" expected"; break;
			case 88: s = "\"<<\" expected"; break;
			case 89: s = "\">>\" expected"; break;
			case 90: s = "\"+\" expected"; break;
			case 91: s = "\"-\" expected"; break;
			case 92: s = "\"*\" expected"; break;
			case 93: s = "\"/\" expected"; break;
			case 94: s = "\"%\" expected"; break;
			case 95: s = "\"**\" expected"; break;
			case 96: s = "\"new\" expected"; break;
			case 97: s = "\"l\" expected"; break;
			case 98: s = "\"L\" expected"; break;
			case 99: s = "\"true\" expected"; break;
			case 100: s = "\"false\" expected"; break;
			case 101: s = "\"null\" expected"; break;
			case 102: s = "\"this\" expected"; break;
			case 103: s = "\"base\" expected"; break;
			case 104: s = "??? expected"; break;
			case 105: s = "invalid ModuleBody"; break;
			case 106: s = "invalid ModuleBody"; break;
			case 107: s = "this symbol not expected in ModuleNameDefinition"; break;
			case 108: s = "invalid ExprStmt"; break;
			case 109: s = "this symbol not expected in ExprStmt"; break;
			case 110: s = "this symbol not expected in FuncDecl"; break;
			case 111: s = "this symbol not expected in ClassDecl"; break;
			case 112: s = "invalid ClassDecl"; break;
			case 113: s = "this symbol not expected in ClassDecl"; break;
			case 114: s = "this symbol not expected in ImportDecl"; break;
			case 115: s = "this symbol not expected in ImportDecl"; break;
			case 116: s = "this symbol not expected in MethodDecl"; break;
			case 117: s = "invalid FieldDecl"; break;
			case 118: s = "invalid Type"; break;
			case 119: s = "invalid Stmt"; break;
			case 120: s = "invalid SimpleStmt"; break;
			case 121: s = "this symbol not expected in CompoundStmt"; break;
			case 122: s = "invalid CompoundStmt"; break;
			case 123: s = "this symbol not expected in ReturnStmt"; break;
			case 124: s = "this symbol not expected in BreakStmt"; break;
			case 125: s = "this symbol not expected in ContinueStmt"; break;
			case 126: s = "this symbol not expected in ThrowStmt"; break;
			case 127: s = "this symbol not expected in YieldStmt"; break;
			case 128: s = "this symbol not expected in EmptyStmt"; break;
			case 129: s = "invalid AugAssignOpe"; break;
			case 130: s = "invalid VarDecl"; break;
			case 131: s = "this symbol not expected in LValueList"; break;
			case 132: s = "invalid LhsPattern"; break;
			case 133: s = "this symbol not expected in PatternList"; break;
			case 134: s = "invalid Pattern"; break;
			case 135: s = "invalid Primary"; break;
			case 136: s = "invalid Literal"; break;
			case 137: s = "invalid ValueBindingPattern"; break;
			case 138: s = "invalid NotTest"; break;
			case 139: s = "invalid Factor"; break;
			case 140: s = "this symbol not expected in Atom"; break;
			case 141: s = "this symbol not expected in Atom"; break;
			case 142: s = "this symbol not expected in Atom"; break;
			case 143: s = "invalid Atom"; break;
			case 144: s = "invalid Trailer"; break;
			case 145: s = "invalid IntSeqExpr"; break;
			case 146: s = "invalid SequenceMaker"; break;
			case 147: s = "invalid DictMaker"; break;
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
