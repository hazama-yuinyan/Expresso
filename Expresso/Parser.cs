using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

using Expresso.Ast;
using Expresso.Ast.Analysis;
using Expresso.TypeSystem;

using ICSharpCode.NRefactory;


using ExpressoModifiers = Expresso.Ast.Modifiers;




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
	public const int _keyword_let = 20;
	public const int _keyword_var = 21;
	public const int maxT = 100;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

internal SymbolTable Symbols{get; set;}
    bool seen_let;
    public string ParsingFileName{get; set;}
	public ExpressoAst TopmostAst{get; private set;}	//the top-level AST the parser is parsing
    public TextLocation CurrentLocation{
        get{
            return new TextLocation(t.line, t.col);
        }
    }

	///<summary>
	/// Parser Implementation details:
	/// 	During parsing we'll construct the symbol table.
	/// 	And in post-parse process, do type validity check and flow analysis, including local name bindings
    ///     and type inference.
	///		Note that the identifiers are just placeholders until after doing name binding. 
	/// 	(Thus referencing them causes runtime exceptions)
	///</summary>
    ///<remarks>
    /// The Parser class itself is responsible for constructing the AST AND the symbol table.
    ///</remarks>
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
        Symbols = new SymbolTable();
	}
	
	LiteralExpression CreateDefaultValue(KnownTypeCode type)
	{
		LiteralExpression result = null;
		var loc = CurrentLocation;
        var type_name = type.ToString().ToLower();

		switch(type){
		case KnownTypeCode.Int:
        case KnownTypeCode.UInt:
        case KnownTypeCode.Byte:
			result = Expression.MakeConstant(type_name, 0, loc);
			break;
			
		case KnownTypeCode.Bool:
			result = Expression.MakeConstant(type_name, false, loc);
			break;
			
		case KnownTypeCode.Float:
			result = Expression.MakeConstant(type_name, 0.0, loc);
			break;
			
		case KnownTypeCode.String:
			result = Expression.MakeConstant(type_name, "", loc);
			break;

        case KnownTypeCode.Char:
            result = Expression.MakeConstant(type_name, '\0', loc);
			break;

		default:
			SemErr("Unknown object type");
			break;
		}
		
		return result;
	}

    PrimitiveType CreateType(string keyword, TextLocation loc)
    {
        return new PrimitiveType(keyword, loc);
    }

    /// <summary>
    /// Creates a new <see ref="ICSharpCode.NRefactory.TextLocation">
    /// that points to the location n characters before the current.
    /// </summary>
    /// <remarks>
    /// It doesn't take line breaks into account.
    /// </remarks>
    TextLocation CreateLocationBefore(int n)
    {
        return new TextLocation(t.line, t.col - n);
    }

    void GoDownScope()
    {
        Symbols = Symbols.Child;
    }

    void GoUpScope()
    {
        Symbols = Symbols.Parent;
    }
	
	bool IsDefiningLValue()
	{
        return la.kind == _keyword_let || la.kind == _keyword_var;
	}
	
	bool IsSequenceInitializer()
	{
		Token x = la;
		if(x.kind != _comma)
            return true;
		
		while(x.kind != 0 && x.kind != _comma && x.kind != _keyword_for)
            x = scanner.Peek();
		
        scanner.ResetPeek();
        return x.kind != _keyword_for;
	}

    /*bool IsObjectCreation()
    {
        scanner.StartPeek();
        Token x = la;
        if(x.kind == 
    }

    bool IsIntegerSequenceExpression()
    {
        scanner.StartPeek();
        var x = la;
        while(x.kind != Tokens.)
    }*/

    bool NotFinalComma()
    {
        return la.kind != _rparen || la.kind != _triple_dots;
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
		Debug.Assert(Symbols.Parent == null);
		try{
		   ExpressoNameBinder.BindAst(module_decl, this); //Here's the start of post-parse processing
		}
		catch(ParserException pe){
		   SemanticError(pe.Message, pe.Objects);
		}
		this.TopmostAst = module_decl;	//Currently there is not so much code out there, though...
		
	}

	void ModuleBody(out ExpressoAst ast) {
		var decls = new List<EntityDeclaration>();
		string module_name; Modifiers modifiers = ExpressoModifiers.None;
		List<ImportDeclaration> prog_defs = null; EntityDeclaration decl = null;
		
		ModuleNameDefinition(out module_name);
		if (la.kind == 24) {
			ProgramDefinition(out prog_defs);
		}
		if (la.kind == 22) {
			Get();
			modifiers = ExpressoModifiers.Export; 
		}
		if (la.kind == 31) {
			FuncDecl(out decl, modifiers);
		} else if (la.kind == 26) {
			ClassDecl(out decl, modifiers);
		} else SynErr(101);
		decls.Add(decl);
		modifiers = ExpressoModifiers.None;
		
		while (la.kind == 22 || la.kind == 26 || la.kind == 31) {
			if (la.kind == 22) {
				Get();
				modifiers = ExpressoModifiers.Export; 
			}
			if (la.kind == 31) {
				FuncDecl(out decl, modifiers);
			} else if (la.kind == 26) {
				ClassDecl(out decl, modifiers);
			} else SynErr(102);
			decls.Add(decl);
			modifiers = ExpressoModifiers.None;
			
		}
		ast = AstNode.MakeModuleDef(module_name, decls, prog_defs); 
	}

	void ModuleNameDefinition(out string moduleName) {
		Expect(23);
		Expect(13);
		moduleName = t.val; 
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(103); Get();}
		Expect(5);
	}

	void ProgramDefinition(out List<ImportDeclaration> imports ) {
		imports = new List<ImportDeclaration>();
		ImportDeclaration tmp;
		
		ImportDecl(out tmp);
		imports.Add(tmp); 
		while (la.kind == 24) {
			ImportDecl(out tmp);
			imports.Add(tmp); 
		}
	}

	void FuncDecl(out EntityDeclaration func, Modifiers modifiers) {
		string name; AstType type = null; BlockStatement block = null;
		var @params = new List<ParameterDeclaration>();
		var start_loc = CurrentLocation;
		
		while (!(la.kind == 0 || la.kind == 31)) {SynErr(104); Get();}
		Expect(31);
		Symbols.AddScope(); 
		Expect(13);
		name = t.val; Symbols.AddSymbol(name); 
		Expect(7);
		if (la.kind == 13) {
			ParamList(out @params);
		}
		Expect(9);
		if (la.kind == 32) {
			Get();
			Type(out type);
		}
		if(type == null)
		   type = new PlaceholderType(CurrentLocation);
		
		Symbols.AddSymbol(name, AstType.Null);
		
		Block(out block);
		func = EntityDeclaration.MakeFunc(name, @params, block, type, modifiers, start_loc); 
	}

	void ClassDecl(out EntityDeclaration decl, Modifiers modifiers) {
		EntityDeclaration entity = null; var decls = new List<EntityDeclaration>();
		string name; var base_names = new List<Identifier>(); Modifiers cur_flag;
		
		while (!(la.kind == 0 || la.kind == 26)) {SynErr(105); Get();}
		Expect(26);
		Symbols.AddScope(); 
		Expect(13);
		name = t.val; Symbols.AddTypeSymbol(name); 
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
		GoDownScope(); 
		while (StartOf(1)) {
			Modifiers(out cur_flag);
			if (la.kind == 31) {
				MethodDecl(out entity, cur_flag);
				decls.Add(entity); 
			} else if (la.kind == 20 || la.kind == 21) {
				FieldDecl(out entity, cur_flag);
				Expect(5);
				decls.Add(entity); 
			} else if (la.kind == 26) {
				ClassDecl(out entity, cur_flag);
				decls.Add(entity); 
			} else SynErr(106);
		}
		while (!(la.kind == 0 || la.kind == 10)) {SynErr(107); Get();}
		Expect(10);
		GoUpScope();
		
	}

	void ImportDecl(out ImportDeclaration decl) {
		string module_name, alias = null; var module_names = new List<string>();
		var aliases = new List<string>();
		
		while (!(la.kind == 0 || la.kind == 24)) {SynErr(108); Get();}
		Expect(24);
		ModuleName(out module_name);
		if (la.kind == 25) {
			Get();
			Expect(13);
			alias = t.val; 
		}
		module_names.Add(module_name);
		aliases.Add(alias);
		
		while (la.kind == 11) {
			Get();
			ModuleName(out module_name);
			if (la.kind == 25) {
				Get();
				Expect(13);
				alias = t.val; 
			}
			module_names.Add(module_name);
			aliases.Add(alias);
			
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(109); Get();}
		Expect(5);
		decl = AstNode.MakeImportDecl(module_names, aliases); 
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
		modifiers = ExpressoModifiers.Private; 
		while (StartOf(2)) {
			if (la.kind == 27) {
				Get();
				modifiers |= ExpressoModifiers.Public; 
			} else if (la.kind == 28) {
				Get();
				modifiers |= ExpressoModifiers.Protected; 
			} else if (la.kind == 29) {
				Get();
				modifiers |= ExpressoModifiers.Private; 
			} else {
				Get();
				modifiers |= ExpressoModifiers.Static; 
			}
		}
	}

	void MethodDecl(out EntityDeclaration decl, Modifiers modifiers) {
		string name; AstType type = null; BlockStatement block;
		var @params = new List<ParameterDeclaration>();
		                          var start_loc = CurrentLocation;
		
		while (!(la.kind == 0 || la.kind == 31)) {SynErr(110); Get();}
		Expect(31);
		Symbols.AddScope(); 
		Expect(13);
		name = t.val; Symbols.AddSymbol(name); 
		Expect(7);
		if (la.kind == 13) {
			ParamList(out @params);
		}
		Expect(9);
		if (la.kind == 32) {
			Get();
			Type(out type);
		}
		if(type == null)
		   type = new PlaceholderType(CurrentLocation);
		
		Block(out block);
		decl = EntityDeclaration.MakeFunc(name, @params, block, type, modifiers, start_loc); 
	}

	void FieldDecl(out EntityDeclaration field, Modifiers modifiers ) {
		string name; AstType type; Expression rhs; Identifier ident;
		var idents = new List<Identifier>(); var exprs = new List<Expression>();
		var start_loc = CurrentLocation;
		
		if (la.kind == 20) {
			Get();
		} else if (la.kind == 21) {
			Get();
		} else SynErr(111);
		VarDef(out name, out type, out rhs);
		ident = AstNode.MakeIdentifier(name, type);
		idents.Add(ident);
		exprs.Add(rhs);
		Symbols.AddSymbol(name, type);
		
		while (la.kind == 11) {
			Get();
			VarDef(out name, out type, out rhs);
			ident = AstNode.MakeIdentifier(name, type);
			idents.Add(ident);
			exprs.Add(rhs);
			Symbols.AddSymbol(name, type);
			
		}
		field = EntityDeclaration.MakeField(idents, exprs, modifiers, start_loc, CurrentLocation); 
	}

	void ParamList(out List<ParameterDeclaration> @params ) {
		@params = new List<ParameterDeclaration>(); ParameterDeclaration expr; 
		Parameter(out expr);
		@params.Add(expr); 
		while (WeakSeparator(11,3,4) ) {
			Parameter(out expr);
			@params.Add(expr); 
		}
	}

	void Type(out AstType type) {
		var start_loc = CurrentLocation; type = AstType.Null;
		switch (la.kind) {
		case 33: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 34: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 35: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 36: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 37: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 38: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 39: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 40: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 41: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 7: {
			TupleTypeSignature(out type);
			break;
		}
		case 42: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 43: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 44: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 45: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 46: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		case 13: {
			Get();
			type = CreateType(t.val, start_loc); 
			break;
		}
		default: SynErr(112); break;
		}
		start_loc = CurrentLocation; 
		while (la.kind == 47) {
			Get();
			if(type.IsNull)
			   SemErr("Array of unknown type is specified. Unknown type is just unknown!");
			
			type = new SimpleType("array", new []{type}, start_loc, CurrentLocation);
			
		}
	}

	void Block(out BlockStatement block) {
		List<Statement> stmts = new List<Statement>(); Statement stmt;
		var start_loc = CurrentLocation;
		
		Expect(6);
		GoDownScope(); 
		Stmt(out stmt);
		stmts.Add(stmt); 
		while (StartOf(5)) {
			Stmt(out stmt);
			stmts.Add(stmt); 
		}
		Expect(10);
		block = Statement.MakeBlock(stmts, start_loc, CurrentLocation);
		               GoUpScope();
		            
	}

	void VarDef(out string name, out AstType type, out Expression option) {
		type = null; option = null; 
		Expect(13);
		name = t.val; 
		if (la.kind == 70) {
			Get();
			Type(out type);
		}
		if(type == null)
		   type = new PlaceholderType(CurrentLocation);
		
		if (la.kind == 63) {
			Get();
			CondExpr(out option);
		}
		if(type == null && option == null)
		   SemanticError("Give me some context or I can't infer the type of {0}", name);
		
	}

	void TupleTypeSignature(out AstType type) {
		var inners = new List<AstType>(); var start_loc = CurrentLocation; 
		Expect(7);
		while (StartOf(6)) {
			Type(out type);
			inners.Add(type); 
		}
		Expect(9);
		type = new SimpleType("tuple", inners, start_loc, CurrentLocation); 
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(7)) {
			SimpleStmt(out stmt);
		} else if (StartOf(8)) {
			CompoundStmt(out stmt);
		} else SynErr(113);
	}

	void SimpleStmt(out Statement stmt) {
		stmt = null; BlockStatement block = null; 
		if (la.kind == _lcurly) {
			Block(out block);
			stmt = block; 
		} else if (StartOf(9)) {
			ExprStmt(out stmt);
		} else if (la.kind == 48) {
			ReturnStmt(out stmt);
		} else if (la.kind == 49) {
			BreakStmt(out stmt);
		} else if (la.kind == 51) {
			ContinueStmt(out stmt);
		} else if (la.kind == 52) {
			YieldStmt(out stmt);
		} else if (la.kind == 5) {
			EmptyStmt(out stmt);
		} else SynErr(114);
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == 64) {
			while (!(la.kind == 0 || la.kind == 64)) {SynErr(115); Get();}
			IfStmt(out stmt);
		} else if (la.kind == 66) {
			WhileStmt(out stmt);
		} else if (la.kind == 19) {
			ForStmt(out stmt);
		} else if (la.kind == 67) {
			MatchStmt(out stmt);
		} else SynErr(116);
	}

	void ExprStmt(out Statement stmt) {
		SequenceExpression lhs = null, seq = null;
		var start_loc = CurrentLocation; stmt = null;
		OperatorType op_type = OperatorType.None;
		
		if (la.kind == 20 || la.kind == 21) {
			VarDeclStmt(out stmt);
		} else if (StartOf(10)) {
			LValueList(out lhs);
			while (StartOf(11)) {
				if (StartOf(12)) {
					AugAssignOpe(ref op_type);
				} else {
					Get();
				}
				RValueList(out seq);
			}
			if(lhs.Count != seq.Count)      //See if both sides have the same number of items
			                                  SemErr("An augumented assignment must have both sides balanced.");
				
			if(op_type != OperatorType.None)
			stmt = Statement.MakeAugumentedAssignment(lhs, seq, op_type, start_loc, CurrentLocation);
			else
			stmt = Statement.MakeAssignment(lhs, seq, start_loc, CurrentLocation);
			
		} else SynErr(117);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(118); Get();}
		Expect(5);
		if(stmt == null)
		stmt = Statement.MakeExprStmt(lhs, start_loc, CurrentLocation);
		
	}

	void ReturnStmt(out Statement stmt) {
		SequenceExpression items = null; 
		Expect(48);
		if (StartOf(13)) {
			RValueList(out items);
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(119); Get();}
		Expect(5);
		stmt = Statement.MakeReturnStmt(items); 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; var start_loc = CurrentLocation; 
		Expect(49);
		if (la.kind == 50) {
			Get();
			Expect(14);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(120); Get();}
		Expect(5);
		stmt = Statement.MakeBreakStmt(
		                      Expression.MakeConstant("int", count, start_loc), start_loc, CurrentLocation
		                 );
		              
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; var start_loc = CurrentLocation; 
		Expect(51);
		if (la.kind == 50) {
			Get();
			Expect(14);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(121); Get();}
		Expect(5);
		stmt = Statement.MakeContinueStmt(
		                      Expression.MakeConstant("int", count, start_loc), start_loc, CurrentLocation
		                 );
		              
	}

	void YieldStmt(out Statement stmt) {
		Expression expr; var start_loc = CurrentLocation; 
		Expect(52);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(122); Get();}
		Expect(5);
		stmt = Statement.MakeYieldStmt(expr, start_loc, CurrentLocation); 
	}

	void EmptyStmt(out Statement stmt) {
		var start_loc = CurrentLocation; 
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(123); Get();}
		Expect(5);
		stmt = Statement.MakeEmptyStmt(start_loc); 
	}

	void RValueList(out SequenceExpression seq) {
		Expression tmp; var exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (WeakSeparator(11,13,14) ) {
			CondExpr(out tmp);
			exprs.Add(tmp);	
		}
		seq = Expression.MakeSequence(exprs); 
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; 
		OrTest(out expr);
		if (la.kind == 72) {
			Get();
			OrTest(out true_expr);
			Expect(4);
			CondExpr(out false_expr);
			expr = Expression.MakeCondExpr(expr, true_expr, false_expr); 
		}
	}

	void AugAssignOpe(ref OperatorType type) {
		switch (la.kind) {
		case 53: {
			Get();
			type = OperatorType.Plus; 
			break;
		}
		case 54: {
			Get();
			type = OperatorType.Minus; 
			break;
		}
		case 55: {
			Get();
			type = OperatorType.Times; 
			break;
		}
		case 56: {
			Get();
			type = OperatorType.Divide; 
			break;
		}
		case 57: {
			Get();
			type = OperatorType.Power; 
			break;
		}
		case 58: {
			Get();
			type = OperatorType.Modulus; 
			break;
		}
		case 59: {
			Get();
			type = OperatorType.BitwiseAnd; 
			break;
		}
		case 60: {
			Get();
			type = OperatorType.BitwiseOr; 
			break;
		}
		case 61: {
			Get();
			type = OperatorType.BitwiseShiftLeft; 
			break;
		}
		case 62: {
			Get();
			type = OperatorType.BitwiseShiftRight; 
			break;
		}
		default: SynErr(124); break;
		}
	}

	void VarDeclStmt(out Statement stmt) {
		string name; AstType type; Expression rhs = null;
		Identifier ident;
		var idents = new List<Identifier>(); var exprs = new List<Expression>();
		bool is_const = false; var start_loc = CurrentLocation;
		
		if (la.kind == 20) {
			Get();
			is_const = true; 
		} else if (la.kind == 21) {
			Get();
		} else SynErr(125);
		VarDef(out name, out type, out rhs);
		ident = AstNode.MakeIdentifier(name, type);
		 idents.Add(ident);
		exprs.Add(rhs);
		rhs = null;
		 Symbols.AddSymbol(name, type);
		
		while (WeakSeparator(11,3,15) ) {
			VarDef(out name, out type, out rhs);
			ident = AstNode.MakeIdentifier(name, type);
			idents.Add(ident);
			exprs.Add(rhs);
			rhs = null;
			Symbols.AddSymbol(name, type);
			
		}
		var modifiers = is_const ? ExpressoModifiers.Immutable : ExpressoModifiers.None;
		                    stmt = Statement.MakeVarDecl(idents, exprs, modifiers, start_loc, CurrentLocation);
		                 
	}

	void LValueList(out SequenceExpression lhs) {
		var lvalues = new List<Expression>(); Expression tmp; 
		Primary(out tmp);
		lvalues.Add(tmp); 
		while (StartOf(10)) {
			Primary(out tmp);
			lvalues.Add(tmp); 
		}
		lhs = Expression.MakeSequence(lvalues); 
	}

	void Primary(out Expression expr) {
		expr = null; PathExpression path; 
		if (la.kind == 13 || la.kind == 93) {
			PathExpression(out path);
			expr = path; 
			if (la.kind == 6) {
				ObjectCreation(path, out expr);
			}
		} else if (StartOf(16)) {
			Atom(out expr);
		} else if (la.kind == 92) {
			NewExpression(out expr);
		} else SynErr(126);
		while (la.kind == 7 || la.kind == 8 || la.kind == 12) {
			Trailer(ref expr);
		}
	}

	void IfStmt(out Statement stmt) {
		Expression tmp; BlockStatement true_block, false_block = null;
		      var start_loc = CurrentLocation;
		   
		Expect(64);
		CondExpr(out tmp);
		Block(out true_block);
		if (la.kind == 65) {
			Get();
			Block(out false_block);
		}
		stmt = Statement.MakeIfStmt(tmp, true_block, false_block, start_loc); 
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; BlockStatement body; var start_loc = CurrentLocation; 
		Expect(66);
		CondExpr(out cond);
		Block(out body);
		stmt = Statement.MakeWhileStmt(cond, body, start_loc); 
	}

	void ForStmt(out Statement stmt) {
		PatternConstruct left; Expression rvalue; BlockStatement body;
		     var start_loc = CurrentLocation;
		
		Expect(19);
		LhsPattern(out left);
		Expect(18);
		CondExpr(out rvalue);
		Block(out body);
		stmt = Statement.MakeForStmt(left, rvalue, body, start_loc); 
	}

	void MatchStmt(out Statement stmt) {
		Expression target; List<MatchPatternClause> matches;
		    var start_loc = CurrentLocation;
		 
		Expect(67);
		CondExpr(out target);
		Expect(6);
		MatchPatternList(out matches);
		Expect(10);
		stmt = Statement.MakeMatchStmt(target, matches, start_loc, CurrentLocation); 
	}

	void LhsPattern(out PatternConstruct pattern) {
		pattern = PatternConstruct.Null; 
		if (la.kind == 71) {
			WildcardPattern(out pattern);
		} else if (la.kind == 13) {
			IdentifierPattern(out pattern);
		} else if (la.kind == 20 || la.kind == 21) {
			ValueBindingPattern(out pattern);
		} else SynErr(127);
	}

	void MatchPatternList(out List<MatchPatternClause> clauses ) {
		clauses = new List<MatchPatternClause>(); List<PatternConstruct> pattern_list; Statement inner; 
		PatternList(out pattern_list);
		Stmt(out inner);
		clauses.Add(Statement.MakeMatchClause(pattern_list, inner)); 
		while (StartOf(17)) {
			PatternList(out pattern_list);
			Stmt(out inner);
			clauses.Add(Statement.MakeMatchClause(pattern_list, inner)); 
		}
	}

	void PatternList(out List<PatternConstruct> patterns ) {
		patterns = new List<PatternConstruct>(); PatternConstruct tmp; 
		Pattern(out tmp);
		patterns.Add(tmp); 
		while (la.kind == 68) {
			while (!(la.kind == 0 || la.kind == 68)) {SynErr(128); Get();}
			Get();
			Pattern(out tmp);
			patterns.Add(tmp); 
		}
		Expect(69);
	}

	void Pattern(out PatternConstruct pattern) {
		if (IsDefiningLValue()) {
			LhsPattern(out pattern);
		} else if (StartOf(13)) {
			ExpressionPattern(out pattern);
		} else SynErr(129);
	}

	void Parameter(out ParameterDeclaration param) {
		string name; Expression option = null; AstType type; 
		Expect(13);
		name = t.val; 
		Expect(70);
		Type(out type);
		Symbols.Child.AddSymbol(name, type); 
		if (la.kind == 63) {
			Get();
			Literal(out option);
		}
		param = EntityDeclaration.MakeParameter(name, type, option); 
	}

	void Literal(out Expression expr) {
		expr = null; string tmp; bool has_suffix = false;
		  var start_loc = CurrentLocation;
		
		switch (la.kind) {
		case 14: {
			Get();
			tmp = t.val; 
			if (la.kind == 94 || la.kind == 95) {
				if (la.kind == 94) {
					Get();
					has_suffix = true; 
				} else {
					Get();
					has_suffix = true; 
				}
			}
			if(has_suffix)
			expr = Expression.MakeConstant("bigint", BigInteger.Parse(tmp), start_loc);
			else
			expr = Expression.MakeConstant("int", Convert.ToInt32(tmp), start_loc);
			
			break;
		}
		case 16: {
			Get();
			expr = Expression.MakeConstant("int", Convert.ToInt32(t.val, 16), start_loc); 
			break;
		}
		case 15: {
			Get();
			expr = Expression.MakeConstant("double", Convert.ToDouble(t.val), start_loc); 
			break;
		}
		case 17: {
			Get();
			tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = Expression.MakeConstant("string", tmp, start_loc);
			
			break;
		}
		case 96: {
			Get();
			expr = Expression.MakeConstant("bool", true, start_loc); 
			break;
		}
		case 97: {
			Get();
			expr = Expression.MakeConstant("bool", false, start_loc); 
			break;
		}
		case 98: {
			Get();
			expr = Expression.MakeSelfRef(start_loc); 
			break;
		}
		case 99: {
			Get();
			expr = Expression.MakeSuperRef(start_loc); 
			break;
		}
		default: SynErr(130); break;
		}
	}

	void ExpressionPattern(out PatternConstruct expr_pattern) {
		Expression expr; 
		CondExpr(out expr);
		expr_pattern = PatternConstruct.MakeExpressionPattern(expr); 
	}

	void WildcardPattern(out PatternConstruct pattern) {
		Expect(71);
		pattern = PatternConstruct.MakeWildcardPattern(); 
	}

	void IdentifierPattern(out PatternConstruct pattern) {
		AstType type; 
		Expect(13);
		if (la.kind == 70) {
			Get();
			Type(out type);
		}
		pattern = PatternConstruct.MakeIdentifierPattern(t.val, type); 
	}

	void ValueBindingPattern(out PatternConstruct pattern) {
		PatternConstruct inner; 
		if (la.kind == 20) {
			Get();
		} else if (la.kind == 21) {
			Get();
		} else SynErr(131);
		Pattern(out inner);
		pattern = PatternConstruct.MakeValueBindingPattern(inner); 
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		if (la.kind == 73) {
			Get();
			AndTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalOr, expr, rhs); 
		}
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		Comparison(out expr);
		if (la.kind == 74) {
			Get();
			Comparison(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalAnd, expr, rhs); 
		}
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		IntSeqExpr(out expr);
		type = OperatorType.Equality; 
		if (StartOf(18)) {
			ComparisonOperator(out type);
			IntSeqExpr(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void IntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null;
		bool upper_inclusive = true;
		
		BitOr(out start);
		if (la.kind == 1 || la.kind == 2) {
			RangeOperator(out upper_inclusive);
			BitOr(out end);
			if (la.kind == 4) {
				Get();
				BitOr(out step);
			}
		}
		if(step == null) step = Expression.MakeConstant("int", 1);
		expr = Expression.MakeIntSeq(start, end, step, upper_inclusive);
		
	}

	void ComparisonOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		switch (la.kind) {
		case 75: {
			Get();
			opType = OperatorType.Equality; 
			break;
		}
		case 76: {
			Get();
			opType = OperatorType.InEquality; 
			break;
		}
		case 77: {
			Get();
			opType = OperatorType.LessThan; 
			break;
		}
		case 78: {
			Get();
			opType = OperatorType.GreaterThan; 
			break;
		}
		case 79: {
			Get();
			opType = OperatorType.LessThanOrEqual; 
			break;
		}
		case 80: {
			Get();
			opType = OperatorType.GreaterThanOrEqual; 
			break;
		}
		default: SynErr(132); break;
		}
	}

	void BitOr(out Expression expr) {
		Expression rhs; 
		BitXor(out expr);
		if (la.kind == 68) {
			Get();
			BitXor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseOr, expr, rhs); 
		}
	}

	void RangeOperator(out bool upper_inclusive) {
		upper_inclusive = false; 
		if (la.kind == 1) {
			Get();
		} else if (la.kind == 2) {
			Get();
			upper_inclusive = true; 
		} else SynErr(133);
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		if (la.kind == 81) {
			Get();
			BitAnd(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ExclusiveOr, expr, rhs); 
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOp(out expr);
		if (la.kind == 82) {
			Get();
			ShiftOp(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseAnd, expr, rhs); 
		}
	}

	void ShiftOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOp(out expr);
		if (la.kind == 83 || la.kind == 84) {
			ShiftOperator(out type);
			AddOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AddOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		if (la.kind == 85 || la.kind == 86) {
			AdditiveOperator(out type);
			Term(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void ShiftOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 83) {
			Get();
			opType = OperatorType.BitwiseShiftLeft; 
		} else if (la.kind == 84) {
			Get();
			opType = OperatorType.BitwiseShiftRight; 
		} else SynErr(134);
	}

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		PowerOp(out expr);
		if (la.kind == 87 || la.kind == 88 || la.kind == 89) {
			MultiplicativeOperator(out type);
			PowerOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AdditiveOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 85) {
			Get();
			opType = OperatorType.Plus; 
		} else if (la.kind == 86) {
			Get();
			opType = OperatorType.Minus; 
		} else SynErr(135);
	}

	void PowerOp(out Expression expr) {
		Expression rhs; 
		Factor(out expr);
		if (la.kind == 90) {
			Get();
			Factor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.Power, expr, rhs); 
		}
	}

	void MultiplicativeOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 87) {
			Get();
			opType = OperatorType.Times; 
		} else if (la.kind == 88) {
			Get();
			opType = OperatorType.Divide; 
		} else if (la.kind == 89) {
			Get();
			opType = OperatorType.Modulus; 
		} else SynErr(136);
	}

	void Factor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; 
		if (StartOf(10)) {
			Primary(out expr);
		} else if (StartOf(19)) {
			UnaryOperator(out type);
			Factor(out factor);
			expr = Expression.MakeUnaryExpr(type, factor); 
		} else SynErr(137);
	}

	void UnaryOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 85 || la.kind == 86) {
			AdditiveOperator(out opType);
		} else if (la.kind == 91) {
			Get();
			opType = OperatorType.Not; 
		} else if (la.kind == 82) {
			Get();
			opType = OperatorType.Reference; 
		} else if (la.kind == 87) {
			Get();
			opType = OperatorType.Dereference; 
		} else SynErr(138);
	}

	void PathExpression(out PathExpression path) {
		var paths = new List<Identifier>(); 
		if (la.kind == 93) {
			Get();
		}
		Expect(13);
		paths.Add(AstNode.MakeIdentifier(t.val)); 
		while (la.kind == 93) {
			Get();
			Expect(13);
			paths.Add(AstNode.MakeIdentifier(t.val)); 
		}
		path = Expression.MakePath(paths); 
	}

	void ObjectCreation(PathExpression path, out Expression expr) {
		var fields = new List<Identifier>(); var values = new List<Expression>();
		
		Expect(6);
		while (la.kind == 13) {
			Get();
			fields.Add(AstNode.MakeIdentifier(t.val)); 
			Expect(4);
			OrTest(out expr);
			values.Add(expr); 
		}
		Expect(10);
		expr = Expression.MakeObjectCreation(path, fields, values); 
	}

	void Atom(out Expression expr) {
		string name; var exprs = new List<Expression>(); expr = null; 
		if (StartOf(20)) {
			Literal(out expr);
		} else if (la.kind == 7) {
			Get();
			CondExpr(out expr);
			exprs.Add(expr); 
			while (NotFinalComma()) {
				ExpectWeak(11, 21);
				CondExpr(out expr);
				exprs.Add(expr); 
			}
			if (la.kind == 11) {
				Get();
			}
			Expect(9);
			if(exprs.Count == 1)
			   expr = Expression.MakeParen(exprs[0]);
			else
			   expr = Expression.MakeParen(Expression.MakeSequence(exprs));
			
		} else if (la.kind == 8) {
			Get();
			if (StartOf(22)) {
				SequenceMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 3)) {SynErr(139); Get();}
			Expect(3);
			if(expr == null)
			expr = Expression.MakeSeqInitializer("array", new List<Expression>());
			
		} else if (la.kind == 6) {
			Get();
			if (StartOf(22)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 10)) {SynErr(140); Get();}
			Expect(10);
			if(expr == null)
			   expr = Expression.MakeSeqInitializer("dictionary", null);
			
		} else SynErr(141);
	}

	void NewExpression(out Expression expr) {
		PathExpression path; 
		Expect(92);
		PathExpression(out path);
		ObjectCreation(path, out expr);
		expr = Expression.MakeNewExpr((ObjectCreationExpression)expr); 
	}

	void Trailer(ref Expression expr) {
		List<Expression> args; 
		if (la.kind == 7) {
			Get();
			if (StartOf(13)) {
				ArgList(out args);
			}
			Expect(9);
			expr = Expression.MakeCallExpr(expr, args); 
		} else if (la.kind == 8) {
			Get();
			ArgList(out args);
			Expect(3);
			expr = Expression.MakeIndexer(expr, args); 
		} else if (la.kind == 12) {
			Get();
			Expect(13);
			expr = Expression.MakeMemRef(expr, AstNode.MakeIdentifier(t.val)); 
		} else SynErr(142);
	}

	void ArgList(out List<Expression> args ) {
		args = new List<Expression>(); Expression expr; 
		CondExpr(out expr);
		args.Add(expr); 
		while (WeakSeparator(11,13,23) ) {
			CondExpr(out expr);
			args.Add(expr); 
		}
	}

	void SequenceMaker(out Expression expr) {
		var exprs = new List<Expression>();
		expr = null; ComprehensionIter comp = null;
		string seq_type_name = "array";
		
		if (StartOf(13)) {
			CondExpr(out expr);
			exprs.Add(expr); 
			while (NotFinalComma()) {
				ExpectWeak(11, 21);
				CondExpr(out expr);
				exprs.Add(expr); 
			}
			if (la.kind == 11) {
				Get();
				Expect(2);
				seq_type_name = "vector"; 
			}
			expr = Expression.MakeSeqInitializer(seq_type_name, exprs); 
		} else if (la.kind == 19) {
			CompFor(out comp);
			expr = Expression.MakeComp(expr, (ComprehensionForClause)comp, type); 
		} else SynErr(143);
	}

	void DictMaker(out Expression expr) {
		Expression key, val; var list = new List<KeyValueLikeExpression>();
		      KeyValueLikeExpression pair; ComprehensionIter comp;
		   
		if (StartOf(13)) {
			CondExpr(out key);
			Expect(4);
			CondExpr(out val);
			pair = Expression.MakeKeyValuePair(key, val);
			list.Add(pair);
			
			while (WeakSeparator(11,13,24) ) {
				CondExpr(out key);
				Expect(4);
				CondExpr(out val);
				pair = Expression.MakeKeyValuePair(key, val);
				list.Add(pair);
				
			}
			expr = Expression.MakeSeqInitializer("dictionary", list); 
		} else if (la.kind == 19) {
			CompFor(out comp);
			comp = Expression.MakeComp(pair, (ComprehensionForClause)comp);
			expr = Expression.MakeSeqInitializer("dictionary", comp);
			
		} else SynErr(144);
	}

	void CompFor(out ComprehensionIter expr) {
		Expression rvalue = null; ComprehensionIter body = null; PatternConstruct target; 
		Expect(19);
		LhsPattern(out target);
		Expect(18);
		CondExpr(out rvalue);
		if (la.kind == 19 || la.kind == 64) {
			CompIter(out body);
		}
		expr = Expression.MakeCompFor(target, rvalue, body); 
	}

	void CompIter(out ComprehensionIter expr) {
		expr = null; 
		if (la.kind == 19) {
			CompFor(out expr);
		} else if (la.kind == 64) {
			CompIf(out expr);
		} else SynErr(145);
	}

	void CompIf(out ComprehensionIter expr) {
		Expression tmp; ComprehensionIter body = null; 
		Expect(64);
		OrTest(out tmp);
		if (la.kind == 19 || la.kind == 64) {
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
		{T,x,x,T, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, T,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, T,T,T,T, x,x},
		{x,x,x,x, x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, T,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, T,T,T,T, x,x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, T,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,T,T, x,x,x,T, T,T,x,x, T,T,T,T, x,x},
		{x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,T,x, x,T,T,T, x,x,x,T, T,T,x,x, T,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,T,T, x,x,x,T, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x},
		{T,x,x,T, x,T,T,T, T,x,T,x, x,T,T,T, T,T,x,x, x,x,x,x, T,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,T,T, x,x,x,T, T,T,x,x, T,T,T,T, x,x},
		{x,x,x,x, x,x,T,T, T,x,x,x, x,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,T,T, x,x,x,T, T,T,x,x, T,T,T,T, x,x},
		{x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x}

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
			case 20: s = "keyword_let expected"; break;
			case 21: s = "keyword_var expected"; break;
			case 22: s = "\"export\" expected"; break;
			case 23: s = "\"module\" expected"; break;
			case 24: s = "\"import\" expected"; break;
			case 25: s = "\"as\" expected"; break;
			case 26: s = "\"class\" expected"; break;
			case 27: s = "\"public\" expected"; break;
			case 28: s = "\"protected\" expected"; break;
			case 29: s = "\"private\" expected"; break;
			case 30: s = "\"static\" expected"; break;
			case 31: s = "\"def\" expected"; break;
			case 32: s = "\"->\" expected"; break;
			case 33: s = "\"int\" expected"; break;
			case 34: s = "\"uint\" expected"; break;
			case 35: s = "\"bool\" expected"; break;
			case 36: s = "\"float\" expected"; break;
			case 37: s = "\"double\" expected"; break;
			case 38: s = "\"bigint\" expected"; break;
			case 39: s = "\"string\" expected"; break;
			case 40: s = "\"byte\" expected"; break;
			case 41: s = "\"char\" expected"; break;
			case 42: s = "\"vector\" expected"; break;
			case 43: s = "\"dictionary\" expected"; break;
			case 44: s = "\"function\" expected"; break;
			case 45: s = "\"intseq\" expected"; break;
			case 46: s = "\"void\" expected"; break;
			case 47: s = "\"[]\" expected"; break;
			case 48: s = "\"return\" expected"; break;
			case 49: s = "\"break\" expected"; break;
			case 50: s = "\"upto\" expected"; break;
			case 51: s = "\"continue\" expected"; break;
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
			case 63: s = "\"=\" expected"; break;
			case 64: s = "\"if\" expected"; break;
			case 65: s = "\"else\" expected"; break;
			case 66: s = "\"while\" expected"; break;
			case 67: s = "\"match\" expected"; break;
			case 68: s = "\"|\" expected"; break;
			case 69: s = "\"=>\" expected"; break;
			case 70: s = "\"(-\" expected"; break;
			case 71: s = "\"_\" expected"; break;
			case 72: s = "\"?\" expected"; break;
			case 73: s = "\"||\" expected"; break;
			case 74: s = "\"&&\" expected"; break;
			case 75: s = "\"==\" expected"; break;
			case 76: s = "\"!=\" expected"; break;
			case 77: s = "\"<\" expected"; break;
			case 78: s = "\">\" expected"; break;
			case 79: s = "\"<=\" expected"; break;
			case 80: s = "\">=\" expected"; break;
			case 81: s = "\"^\" expected"; break;
			case 82: s = "\"&\" expected"; break;
			case 83: s = "\"<<\" expected"; break;
			case 84: s = "\">>\" expected"; break;
			case 85: s = "\"+\" expected"; break;
			case 86: s = "\"-\" expected"; break;
			case 87: s = "\"*\" expected"; break;
			case 88: s = "\"/\" expected"; break;
			case 89: s = "\"%\" expected"; break;
			case 90: s = "\"**\" expected"; break;
			case 91: s = "\"!\" expected"; break;
			case 92: s = "\"new\" expected"; break;
			case 93: s = "\"::\" expected"; break;
			case 94: s = "\"l\" expected"; break;
			case 95: s = "\"L\" expected"; break;
			case 96: s = "\"true\" expected"; break;
			case 97: s = "\"false\" expected"; break;
			case 98: s = "\"self\" expected"; break;
			case 99: s = "\"super\" expected"; break;
			case 100: s = "??? expected"; break;
			case 101: s = "invalid ModuleBody"; break;
			case 102: s = "invalid ModuleBody"; break;
			case 103: s = "this symbol not expected in ModuleNameDefinition"; break;
			case 104: s = "this symbol not expected in FuncDecl"; break;
			case 105: s = "this symbol not expected in ClassDecl"; break;
			case 106: s = "invalid ClassDecl"; break;
			case 107: s = "this symbol not expected in ClassDecl"; break;
			case 108: s = "this symbol not expected in ImportDecl"; break;
			case 109: s = "this symbol not expected in ImportDecl"; break;
			case 110: s = "this symbol not expected in MethodDecl"; break;
			case 111: s = "invalid FieldDecl"; break;
			case 112: s = "invalid Type"; break;
			case 113: s = "invalid Stmt"; break;
			case 114: s = "invalid SimpleStmt"; break;
			case 115: s = "this symbol not expected in CompoundStmt"; break;
			case 116: s = "invalid CompoundStmt"; break;
			case 117: s = "invalid ExprStmt"; break;
			case 118: s = "this symbol not expected in ExprStmt"; break;
			case 119: s = "this symbol not expected in ReturnStmt"; break;
			case 120: s = "this symbol not expected in BreakStmt"; break;
			case 121: s = "this symbol not expected in ContinueStmt"; break;
			case 122: s = "this symbol not expected in YieldStmt"; break;
			case 123: s = "this symbol not expected in EmptyStmt"; break;
			case 124: s = "invalid AugAssignOpe"; break;
			case 125: s = "invalid VarDeclStmt"; break;
			case 126: s = "invalid Primary"; break;
			case 127: s = "invalid LhsPattern"; break;
			case 128: s = "this symbol not expected in PatternList"; break;
			case 129: s = "invalid Pattern"; break;
			case 130: s = "invalid Literal"; break;
			case 131: s = "invalid ValueBindingPattern"; break;
			case 132: s = "invalid ComparisonOperator"; break;
			case 133: s = "invalid RangeOperator"; break;
			case 134: s = "invalid ShiftOperator"; break;
			case 135: s = "invalid AdditiveOperator"; break;
			case 136: s = "invalid MultiplicativeOperator"; break;
			case 137: s = "invalid Factor"; break;
			case 138: s = "invalid UnaryOperator"; break;
			case 139: s = "this symbol not expected in Atom"; break;
			case 140: s = "this symbol not expected in Atom"; break;
			case 141: s = "invalid Atom"; break;
			case 142: s = "invalid Trailer"; break;
			case 143: s = "invalid SequenceMaker"; break;
			case 144: s = "invalid DictMaker"; break;
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
