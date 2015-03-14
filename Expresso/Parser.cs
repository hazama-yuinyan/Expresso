using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

using Expresso.Ast;
using Expresso.Ast.Analysis;
using Expresso.TypeSystem;

using ICSharpCode.NRefactory;


using ExpressoModifiers = Expresso.Ast.Modifiers;




using System;

namespace Expresso {



public class Parser {
	public const int _EOF = 0;
	public const int _double_dots = 1;
	public const int _triple_dots = 2;
	public const int _rbracket = 3;
	public const int _colon = 4;
	public const int _double_colon = 5;
	public const int _semicolon = 6;
	public const int _lcurly = 7;
	public const int _lparen = 8;
	public const int _lbracket = 9;
	public const int _rparen = 10;
	public const int _rcurly = 11;
	public const int _comma = 12;
	public const int _dot = 13;
	public const int _ident = 14;
	public const int _integer = 15;
	public const int _float = 16;
	public const int _hex_digit = 17;
	public const int _unicode_escape = 18;
	public const int _character_literal = 19;
	public const int _string_literal = 20;
	public const int _raw_string_literal = 21;
	public const int _keyword_for = 22;
	public const int _keyword_let = 23;
	public const int _keyword_var = 24;
	public const int maxT = 101;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

static uint ScopeId = 1;
    static Regex UnicodeEscapeFinder = new Regex(@"\\[uU]([\dA-Fa-f]{4}|[\dA-Fa-f]{6})", RegexOptions.Compiled);
    internal SymbolTable Symbols{get; set;}
    /// <summary>
    /// This flag determines whether we are doing post-parse processing including name binding,
    /// type validity check, type inference and flow analisys.
    /// </summary>
    public bool DoPostParseProcessing{get; set;}
	public ExpressoAst TopmostAst{get; private set;}	//the top-level AST the parser is parsing
    public TextLocation CurrentLocation{
        get{
            return new TextLocation(t.line, t.col);
        }
    }

    public TextLocation NextLocation{
        get{
            return new TextLocation(la.line, la.col);
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
        DoPostParseProcessing = false;
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

    AstType CreateType(string keyword, TextLocation loc, bool isReference)
    {
        AstType type = new PrimitiveType(keyword, loc);
        if(isReference)
            type = new ReferenceType(type, loc);

        return type;
    }

    LiteralExpression CreateLiteral(string value, TextLocation loc)
    {
        string type_name = "int";
        object obj = null;
        string suffix = value.Substring(value.Length - 1);
        string numerics = value.Replace("_", "");
        switch(suffix){
        case "u":
        case "U":
        {
            type_name = "uint";
            uint u;
            if(uint.TryParse(numerics.Substring(0, numerics.Length - 1), out u))
                obj = u;
            else
                SemErr("Invalid uint representation!");
            break;
        }

        case "l":
        case "L":
            type_name = "bigint";
            obj = BigInteger.Parse(numerics.Substring(0, numerics.Length - 1));
            break;

        case "f":
        case "F":
        {
            type_name = "float";
            float f;
            if(float.TryParse(numerics.Substring(0, numerics.Length - 1), out f))
                obj = f;
            else
                SemErr("Invalid float representation!");
            break; 
        }

        default:
        {
            double d;
            int i;
            if(int.TryParse(numerics, out i)){
                obj = i;
                type_name = "int";
            }else if(double.TryParse(numerics, out d)){
                obj = d;
                type_name = "double";
            }else{
                SemErr("Unknown sequence for numeric literals! Make sure that you write a number!");
            }
            break;
        }
        }

        return Expression.MakeConstant(type_name, obj, loc);
    }

    LiteralExpression HandleEscapes(string typeName, string literal, TextLocation loc)
    {
        string tmp = literal.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t")
                            .Replace("\\v", "\v").Replace("\\f", "\f").Replace("\\0", "\0")
                            .Replace("\\b", "\b");
        tmp = UnicodeEscapeFinder.Replace(tmp, m => {
            return ((char)int.Parse(m.Value.Substring(2), NumberStyles.HexNumber)).ToString();
        });

        if(typeName == "char")
            return Expression.MakeConstant(typeName, Convert.ToChar(tmp), loc);
        else if(typeName == "string")
            return Expression.MakeConstant(typeName, tmp, loc);
        else
            throw new InvalidOperationException();
    }

    AstType ConvertPathToType(PathExpression path)
    {
        AstType type = null;
        foreach(var item in path.Items){
            item.Remove();
            if(type == null)
                type = new SimpleType(item, TextLocation.Empty);
            else
                type = new MemberType(type, item);
        }

        return type;
    }

    AstType CreateTypeWithArgs(string typeName, params AstType[] typeArgs)
    {
        return new SimpleType(typeName, typeArgs, TextLocation.Empty, TextLocation.Empty);
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
        Symbols = Symbols.Children[Symbols.Children.Count - 1];
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

    bool IsIdentifierPattern()
    {
        Token x = scanner.Peek();
        scanner.ResetPeek();
        return x.kind != _double_colon && x.kind != _lcurly;
    }

    bool NotFinalComma()
    {
        var t = la;
        var tt = scanner.Peek();
        scanner.ResetPeek();
        return t.kind == _comma && tt.kind != _rparen && tt.kind != _rbracket && tt.kind != _triple_dots;
    }
	
	public void SemanticError(string format, params object[] args)
	{
		//Convenient method for printing a semantic error with a format string
		SemErr(string.Format(format, args));
	}

    /// <summary>
    /// Reports a warning message.
    /// It is intended to be used from outside the Parser class.
    /// </summary>
    public void ReportWarning(string format, AstNode node, params object[] objects)
    {
        errors.Warning(string.Format(string.Format(format, objects), node.StartLocation));
    }

    /// <summary>
    /// Reports a semantical error message.
    /// It is intended to be used from outside the Parser class.
    /// </summary>
    public void ReportSemanticError(string format, AstNode node, params object[] objects)
    {
        errors.SemErr(string.Format(string.Format(format, objects), node.StartLocation));
    }

    /// <summary>
    /// Reports a semantical error message 
    /// </summary>
    public void ReportSemanticErrorRegional(string format, AstNode start, AstNode end, params object[] objects)
    {
        var real_message = string.Format("{0} ~ {1}: {2}", start.StartLocation, end.EndLocation, string.Format(format, objects));
        errors.SemErr(real_message);
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
		if(DoPostParseProcessing){
		   ExpressoNameBinder.BindAst(module_decl, this); //Here's the start of post-parse processing
		}
		this.TopmostAst = module_decl;	//Currently there is not so much code out there, though...
		
	}

	void ModuleBody(out ExpressoAst ast) {
		var decls = new List<EntityDeclaration>();
		string module_name; Modifiers modifiers = ExpressoModifiers.None;
		List<ImportDeclaration> prog_defs = null; EntityDeclaration decl = null;
		
		ModuleNameDefinition(out module_name);
		if (la.kind == 27) {
			ProgramDefinition(out prog_defs);
		}
		if (la.kind == 25) {
			Get();
			modifiers = ExpressoModifiers.Export; 
		}
		if (la.kind == 35) {
			FuncDecl(out decl, modifiers);
		} else if (la.kind == 30) {
			ClassDecl(out decl, modifiers);
		} else SynErr(102);
		decls.Add(decl);
		modifiers = ExpressoModifiers.None;
		
		while (la.kind == 25 || la.kind == 30 || la.kind == 35) {
			if (la.kind == 25) {
				Get();
				modifiers = ExpressoModifiers.Export; 
			}
			if (la.kind == 35) {
				FuncDecl(out decl, modifiers);
			} else if (la.kind == 30) {
				ClassDecl(out decl, modifiers);
			} else SynErr(103);
			decls.Add(decl);
			modifiers = ExpressoModifiers.None;
			
		}
		ast = AstNode.MakeModuleDef(module_name, decls, prog_defs); 
	}

	void ModuleNameDefinition(out string moduleName) {
		
		Expect(26);
		Expect(14);
		moduleName = t.val; 
		while (la.kind == 13) {
			Get();
			Expect(14);
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(104); Get();}
		Expect(6);
	}

	void ProgramDefinition(out List<ImportDeclaration> imports ) {
		imports = new List<ImportDeclaration>();
		ImportDeclaration tmp;
		
		ImportDecl(out tmp);
		imports.Add(tmp); 
		while (la.kind == 27) {
			ImportDecl(out tmp);
			imports.Add(tmp); 
		}
	}

	void FuncDecl(out EntityDeclaration decl, Modifiers modifiers) {
		Identifier ident = null;
		string name; AstType type = null; BlockStatement block;
		var @params = new List<ParameterDeclaration>();
		var start_loc = NextLocation;
		
		while (!(la.kind == 0 || la.kind == 35)) {SynErr(105); Get();}
		Expect(35);
		Symbols.AddScope(); 
		Expect(14);
		name = t.val;
		                 ident = AstNode.MakeIdentifier(name); 
		                 Symbols.AddSymbol(name, ident);
		              
		Expect(8);
		GoDownScope();
		Symbols.Name = "func " + name + "`" + ScopeId++;
		
		if (la.kind == 14) {
			ParamList(out @params);
		}
		Expect(10);
		if (la.kind == 36) {
			Get();
			Type(out type);
		}
		if(type == null)
		   type = new PlaceholderType(TextLocation.Empty);
		
		Block(out block);
		decl = EntityDeclaration.MakeFunc(ident, @params, block, type, modifiers, start_loc);
		        GoUpScope();
		     
	}

	void ClassDecl(out EntityDeclaration decl, Modifiers modifiers) {
		EntityDeclaration entity = null; var decls = new List<EntityDeclaration>(); AstType type_path;
		string name; var bases = new List<AstType>(); Modifiers cur_flag; var start_loc = CurrentLocation;
		Identifier ident = null;
		
		while (!(la.kind == 0 || la.kind == 30)) {SynErr(106); Get();}
		Expect(30);
		Symbols.AddScope(); 
		Expect(14);
		name = t.val;
		                    ident = AstNode.MakeIdentifier(name);
		                    Symbols.AddTypeSymbol(name, ident);
		                 
		if (la.kind == 4) {
			Get();
			TypePathExpression(out type_path);
			bases.Add(type_path); 
			while (la.kind == 12) {
				Get();
				TypePathExpression(out type_path);
				bases.Add(type_path); 
			}
		}
		Expect(7);
		GoDownScope();
		Symbols.Name = "class " + name + "`" + ScopeId++;
		
		while (StartOf(1)) {
			cur_flag = ExpressoModifiers.Private; 
			while (StartOf(2)) {
				Modifiers(ref cur_flag);
			}
			
			if (la.kind == 35) {
				FuncDecl(out entity, cur_flag);
				decls.Add(entity); 
			} else if (la.kind == 23 || la.kind == 24) {
				FieldDecl(out entity, cur_flag);
				decls.Add(entity); 
			} else if (la.kind == 30) {
				ClassDecl(out entity, cur_flag);
				decls.Add(entity); 
			} else SynErr(107);
		}
		while (!(la.kind == 0 || la.kind == 11)) {SynErr(108); Get();}
		Expect(11);
		decl = EntityDeclaration.MakeClassDecl(ident, bases, decls, modifiers, start_loc, CurrentLocation);
		                  GoUpScope();
		               
	}

	void ImportDecl(out ImportDeclaration decl) {
		decl = null; var has_in = false; PathExpression path;
		string alias = null; var entities = new List<PathExpression>();
		
		while (!(la.kind == 0 || la.kind == 27)) {SynErr(109); Get();}
		Expect(27);
		PathExpression(out path);
		entities.Add(path); 
		if (la.kind == 28) {
			Get();
			Expect(14);
			alias = t.val; 
		} else if (la.kind == 12 || la.kind == 29) {
			while (la.kind == 12) {
				Get();
				PathExpression(out path);
				entities.Add(path); 
			}
			Expect(29);
			has_in = true; 
			PathExpression(out path);
		} else SynErr(110);
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(111); Get();}
		Expect(6);
		if(has_in)
		   decl = AstNode.MakeImportDecl(path, entities);
		else
		   decl = AstNode.MakeImportDecl(entities[0], alias);
		
	}

	void PathExpression(out PathExpression path) {
		var paths = new List<Identifier>(); 
		Expect(14);
		paths.Add(AstNode.MakeIdentifier(t.val)); 
		while (la.kind == 5) {
			Get();
			Expect(14);
			paths.Add(AstNode.MakeIdentifier(t.val)); 
		}
		var last_ident = paths.Last();
		last_ident.Type = new PlaceholderType(TextLocation.Empty);
		path = Expression.MakePath(paths);
		
	}

	void TypePathExpression(out AstType type) {
		Expect(14);
		type = new SimpleType(t.val, TextLocation.Empty); 
		while (la.kind == 5) {
			Get();
			Expect(14);
			type = new MemberType(type, AstNode.MakeIdentifier(t.val)); 
		}
	}

	void Modifiers(ref Modifiers modifiers) {
		if (la.kind == 31) {
			Get();
			modifiers &= ~ExpressoModifiers.Private;
			modifiers |= ExpressoModifiers.Public;
			
		} else if (la.kind == 32) {
			Get();
			modifiers &= ~ExpressoModifiers.Private;
			modifiers |= ExpressoModifiers.Protected;
			
		} else if (la.kind == 33) {
			Get();
			modifiers |= ExpressoModifiers.Private; 
		} else if (la.kind == 34) {
			Get();
			modifiers |= ExpressoModifiers.Static; 
		} else SynErr(112);
	}

	void FieldDecl(out EntityDeclaration field, Modifiers modifiers) {
		string name; AstType type; Expression rhs; Identifier ident;
		var idents = new List<Identifier>(); var exprs = new List<Expression>();
		var start_loc = NextLocation;
		
		if (la.kind == 23) {
			Get();
		} else if (la.kind == 24) {
			Get();
		} else SynErr(113);
		VarDef(out name, out type, out rhs);
		ident = AstNode.MakeIdentifier(name, type);
		idents.Add(ident);
		exprs.Add(rhs);
		Symbols.AddSymbol(name, ident);
		
		while (la.kind == 12) {
			Get();
			VarDef(out name, out type, out rhs);
			ident = AstNode.MakeIdentifier(name, type);
			idents.Add(ident);
			exprs.Add(rhs);
			Symbols.AddSymbol(name, ident);
			
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(114); Get();}
		Expect(6);
		field = EntityDeclaration.MakeField(idents, exprs, modifiers, start_loc, CurrentLocation); 
	}

	void ParamList(out List<ParameterDeclaration> @params ) {
		@params = new List<ParameterDeclaration>(); ParameterDeclaration param; bool seen_option = false; 
		Parameter(out param);
		if(param.Option != null)
		   seen_option = true;
		
		@params.Add(param);
		
		while (WeakSeparator(12,3,4) ) {
			Parameter(out param);
			if(seen_option && param.Option == null)
			   SemErr("You can't put optional parameters before non-optional parameters");
			else if(!seen_option && param.Option != null)
			   seen_option = true;
			
			@params.Add(param);
			
		}
	}

	void Type(out AstType type) {
		var start_loc = NextLocation; type = new PlaceholderType(NextLocation); var is_reference = false; 
		if (la.kind == 39) {
			Get();
			is_reference = true; 
		}
		switch (la.kind) {
		case 40: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 41: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 42: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 43: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 44: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 45: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 46: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 47: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 48: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 8: {
			TupleTypeSignature(out type);
			break;
		}
		case 49: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 50: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 51: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 52: {
			Get();
			type = CreateType(t.val, start_loc, is_reference); 
			break;
		}
		case 53: {
			Get();
			type = new SimpleType("tuple", Enumerable.Empty<AstType>(), start_loc, CurrentLocation); 
			break;
		}
		case 14: {
			TypePathExpression(out type);
			if(is_reference) type = new ReferenceType(type, TextLocation.Empty); 
			break;
		}
		default: SynErr(115); break;
		}
		start_loc = CurrentLocation; 
		while (la.kind == 9) {
			Get();
			Expect(3);
			if(type.IsNull)
			   SemErr("Array of unknown type is specified. Unknown type is just unknown!");
			
			type = new SimpleType("array", new []{type}, start_loc, CurrentLocation);
			
		}
	}

	void Block(out BlockStatement block) {
		List<Statement> stmts = new List<Statement>();
		Statement stmt; var start_loc = NextLocation;
		Symbols.AddScope();
		
		Expect(7);
		GoDownScope();
		Symbols.Name = "block`" + ScopeId++;
		
		Stmt(out stmt);
		stmts.Add(stmt); 
		while (StartOf(5)) {
			Stmt(out stmt);
			stmts.Add(stmt); 
		}
		Expect(11);
		block = Statement.MakeBlock(stmts, start_loc, CurrentLocation);
		               GoUpScope();
		            
	}

	void Parameter(out ParameterDeclaration param) {
		string name; Expression option = null; AstType type = null; Identifier identifier; 
		Expect(14);
		name = t.val; 
		if (la.kind == 37) {
			Get();
			Type(out type);
		}
		identifier = AstNode.MakeIdentifier(name, type ?? new PlaceholderType(TextLocation.Empty));
		Symbols.AddSymbol(name, identifier);
		
		if (la.kind == 38) {
			Get();
			Literal(out option);
		}
		if(type == null && option == null)
		   SemErr("You can't omit both the type annotation and the default value!");
		
		param = EntityDeclaration.MakeParameter(identifier, option);
		
	}

	void Literal(out Expression expr) {
		expr = null; string tmp;
		  var start_loc = NextLocation;
		
		switch (la.kind) {
		case 15: {
			Get();
			expr = CreateLiteral(t.val, start_loc); 
			break;
		}
		case 17: {
			Get();
			expr = Expression.MakeConstant("int", Convert.ToInt32(t.val, 16), start_loc); 
			break;
		}
		case 16: {
			Get();
			expr = CreateLiteral(t.val, start_loc); 
			break;
		}
		case 19: {
			Get();
			tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = HandleEscapes("char", tmp, start_loc);
			
			break;
		}
		case 20: {
			Get();
			tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = HandleEscapes("string", tmp, start_loc);
			
			break;
		}
		case 21: {
			Get();
			tmp = t.val.Substring(1);
			if(tmp.StartsWith("#")){
			   int index_double_quote = tmp.IndexOf('"');
			   int start_end_hashes = tmp.Length - index_double_quote - 1;
			   int index_end_double_quote = tmp.LastIndexOf('"');
			   if(start_end_hashes != index_end_double_quote + 1)
			       SemErr("The number of opening and closing hash symbols in a raw string must match!");
			
			   tmp = tmp.Substring(index_double_quote, tmp.Length - index_end_double_quote - index_double_quote);
			}
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = Expression.MakeConstant("string", tmp, start_loc);
			
			break;
		}
		case 97: {
			Get();
			expr = Expression.MakeConstant("bool", true, start_loc); 
			break;
		}
		case 98: {
			Get();
			expr = Expression.MakeConstant("bool", false, start_loc); 
			break;
		}
		case 99: {
			Get();
			expr = Expression.MakeSelfRef(start_loc); 
			break;
		}
		case 100: {
			Get();
			expr = Expression.MakeSuperRef(start_loc); 
			break;
		}
		default: SynErr(116); break;
		}
	}

	void VarDef(out string name, out AstType type, out Expression option) {
		type = null; option = null; 
		Expect(14);
		name = t.val; 
		if (la.kind == 37) {
			Get();
			Type(out type);
		}
		if(type == null)
		   type = new PlaceholderType(TextLocation.Empty);
		
		if (la.kind == 38) {
			Get();
			CondExpr(out option);
		}
		if(type == null && option == null)
		   SemanticError("Give me some context or I can't infer the type of {0}", name);
		
	}

	void TupleTypeSignature(out AstType type) {
		var inners = new List<AstType>(); var start_loc = NextLocation; 
		Expect(8);
		while (StartOf(6)) {
			Type(out type);
			inners.Add(type); 
		}
		Expect(10);
		type = new SimpleType("tuple", inners, start_loc, CurrentLocation); 
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(7)) {
			SimpleStmt(out stmt);
		} else if (StartOf(8)) {
			CompoundStmt(out stmt);
		} else SynErr(117);
	}

	void SimpleStmt(out Statement stmt) {
		stmt = null; BlockStatement block = null; 
		switch (la.kind) {
		case 7: {
			Block(out block);
			stmt = block; 
			break;
		}
		case 14: {
			ExprStmt(out stmt);
			break;
		}
		case 23: case 24: {
			VarDeclStmt(out stmt);
			break;
		}
		case 54: {
			ReturnStmt(out stmt);
			break;
		}
		case 55: {
			BreakStmt(out stmt);
			break;
		}
		case 57: {
			ContinueStmt(out stmt);
			break;
		}
		case 58: {
			YieldStmt(out stmt);
			break;
		}
		case 6: {
			EmptyStmt(out stmt);
			break;
		}
		default: SynErr(118); break;
		}
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; 
		if (la.kind == 69) {
			while (!(la.kind == 0 || la.kind == 69)) {SynErr(119); Get();}
			IfStmt(out stmt);
		} else if (la.kind == 71) {
			WhileStmt(out stmt);
		} else if (la.kind == 22) {
			ForStmt(out stmt);
		} else if (la.kind == 72) {
			MatchStmt(out stmt);
		} else SynErr(120);
	}

	void ExprStmt(out Statement stmt) {
		SequenceExpression lhs = null, seq = null;
		var start_loc = NextLocation; stmt = null;
		OperatorType op_type = OperatorType.None;
		        AssignmentExpression assign = null;
		
		LValueList(out lhs);
		if (StartOf(9)) {
			AugAssignOpe(ref op_type);
			RValueList(out seq);
			if(lhs.Count != seq.Count)  //See if both sides have the same number of items or not
			   SemErr("An augumented assignment must have both sides balanced.");
			
			stmt = Statement.MakeAugumentedAssignment(lhs, seq, op_type, start_loc, CurrentLocation);
			
		} else if (la.kind == 38) {
			Get();
			RValueList(out seq);
			assign = Expression.MakeAssignment(lhs, seq); 
			while (la.kind == 38) {
				Get();
				RValueList(out seq);
				assign = Expression.MakeMultipleAssignment(assign, seq); 
			}
			stmt = Statement.MakeExprStmt(assign, start_loc, CurrentLocation); 
		} else SynErr(121);
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(122); Get();}
		Expect(6);
		if(stmt == null)
		stmt = Statement.MakeExprStmt(lhs, start_loc, CurrentLocation);
		
	}

	void VarDeclStmt(out Statement stmt) {
		string name; AstType type; Expression rhs = null;
		Identifier ident;
		var idents = new List<Identifier>(); var exprs = new List<Expression>();
		bool is_const = false; var start_loc = NextLocation;
		
		if (la.kind == 23) {
			Get();
			is_const = true; 
		} else if (la.kind == 24) {
			Get();
		} else SynErr(123);
		VarDef(out name, out type, out rhs);
		ident = AstNode.MakeIdentifier(name, type);
		 idents.Add(ident);
		exprs.Add(rhs ?? Expression.Null);
		rhs = null;
		 Symbols.AddSymbol(name, ident);
		
		while (WeakSeparator(12,3,10) ) {
			VarDef(out name, out type, out rhs);
			ident = AstNode.MakeIdentifier(name, type);
			idents.Add(ident);
			exprs.Add(rhs ?? Expression.Null);
			rhs = null;
			Symbols.AddSymbol(name, ident);
			
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(124); Get();}
		Expect(6);
		var modifiers = is_const ? ExpressoModifiers.Immutable : ExpressoModifiers.None;
		                  stmt = Statement.MakeVarDecl(idents, exprs, modifiers, start_loc, CurrentLocation);
		               
	}

	void ReturnStmt(out Statement stmt) {
		SequenceExpression items = null; 
		Expect(54);
		if (StartOf(11)) {
			RValueList(out items);
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(125); Get();}
		Expect(6);
		stmt = Statement.MakeReturnStmt(items); 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; var start_loc = NextLocation; 
		Expect(55);
		if (la.kind == 56) {
			Get();
			Expect(15);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(126); Get();}
		Expect(6);
		stmt = Statement.MakeBreakStmt(
		                      Expression.MakeConstant("int", count, start_loc), start_loc, CurrentLocation
		                 );
		              
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; var start_loc = NextLocation; 
		Expect(57);
		if (la.kind == 56) {
			Get();
			Expect(15);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(127); Get();}
		Expect(6);
		stmt = Statement.MakeContinueStmt(
		                      Expression.MakeConstant("int", count, start_loc), start_loc, CurrentLocation
		                 );
		              
	}

	void YieldStmt(out Statement stmt) {
		Expression expr; var start_loc = NextLocation; 
		Expect(58);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(128); Get();}
		Expect(6);
		stmt = Statement.MakeYieldStmt(expr, start_loc, CurrentLocation); 
	}

	void EmptyStmt(out Statement stmt) {
		var start_loc = NextLocation; 
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(129); Get();}
		Expect(6);
		stmt = Statement.MakeEmptyStmt(start_loc); 
	}

	void RValueList(out SequenceExpression seq) {
		Expression tmp; var exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (WeakSeparator(12,11,12) ) {
			CondExpr(out tmp);
			exprs.Add(tmp);	
		}
		seq = Expression.MakeSequence(exprs); 
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; 
		OrTest(out expr);
		if (la.kind == 77) {
			Get();
			OrTest(out true_expr);
			Expect(4);
			CondExpr(out false_expr);
			expr = Expression.MakeCondExpr(expr, true_expr, false_expr); 
		}
	}

	void AugAssignOpe(ref OperatorType type) {
		switch (la.kind) {
		case 59: {
			Get();
			type = OperatorType.Plus; 
			break;
		}
		case 60: {
			Get();
			type = OperatorType.Minus; 
			break;
		}
		case 61: {
			Get();
			type = OperatorType.Times; 
			break;
		}
		case 62: {
			Get();
			type = OperatorType.Divide; 
			break;
		}
		case 63: {
			Get();
			type = OperatorType.Power; 
			break;
		}
		case 64: {
			Get();
			type = OperatorType.Modulus; 
			break;
		}
		case 65: {
			Get();
			type = OperatorType.BitwiseAnd; 
			break;
		}
		case 66: {
			Get();
			type = OperatorType.BitwiseOr; 
			break;
		}
		case 67: {
			Get();
			type = OperatorType.BitwiseShiftLeft; 
			break;
		}
		case 68: {
			Get();
			type = OperatorType.BitwiseShiftRight; 
			break;
		}
		default: SynErr(130); break;
		}
	}

	void LValueList(out SequenceExpression lhs) {
		var lvalues = new List<Expression>(); Expression tmp; 
		LhsPrimary(out tmp);
		lvalues.Add(tmp); 
		while (WeakSeparator(12,3,13) ) {
			LhsPrimary(out tmp);
			lvalues.Add(tmp); 
		}
		lhs = Expression.MakeSequence(lvalues); 
	}

	void LhsPrimary(out Expression expr) {
		expr = null; PathExpression path; 
		PathExpression(out path);
		expr = path; 
		while (la.kind == 8 || la.kind == 9 || la.kind == 13) {
			Trailer(ref expr);
		}
	}

	void IfStmt(out Statement stmt) {
		PatternConstruct pattern; BlockStatement true_block, false_block = null;
		      var start_loc = NextLocation;
		   
		Expect(69);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "if`" + ScopeId++;
		
		LhsPattern(out pattern);
		Block(out true_block);
		if (la.kind == 70) {
			Get();
			Block(out false_block);
		}
		stmt = Statement.MakeIfStmt(pattern, true_block, false_block, start_loc);
		       GoUpScope();
		    
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; BlockStatement body; var start_loc = NextLocation; 
		Expect(71);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "while`" + ScopeId++;
		
		CondExpr(out cond);
		Block(out body);
		stmt = Statement.MakeWhileStmt(cond, body, start_loc);
		GoUpScope();
		
	}

	void ForStmt(out Statement stmt) {
		PatternConstruct left; Expression rvalue; BlockStatement body;
		     var start_loc = NextLocation;
		
		Expect(22);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "for`" + ScopeId++;
		
		LhsPattern(out left);
		Expect(29);
		CondExpr(out rvalue);
		Block(out body);
		stmt = Statement.MakeForStmt(left, rvalue, body, start_loc);
		             GoUpScope();
		          
	}

	void MatchStmt(out Statement stmt) {
		Expression target; List<MatchPatternClause> matches;
		    var start_loc = NextLocation;
		 
		Expect(72);
		CondExpr(out target);
		Expect(7);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "match`" + ScopeId++;
		
		MatchPatternList(out matches);
		Expect(11);
		stmt = Statement.MakeMatchStmt(target, matches, start_loc, CurrentLocation);
		                      GoUpScope();
		                   
	}

	void LhsPattern(out PatternConstruct pattern) {
		pattern = PatternConstruct.Null; 
		if (la.kind == 75) {
			WildcardPattern(out pattern);
		} else if (IsIdentifierPattern()) {
			IdentifierPattern(out pattern);
		} else if (la.kind == 8) {
			TuplePattern(out pattern);
		} else if (la.kind == 9 || la.kind == 14) {
			DestructuringPattern(out pattern);
		} else SynErr(131);
	}

	void MatchPatternList(out List<MatchPatternClause> clauses ) {
		clauses = new List<MatchPatternClause>(); List<PatternConstruct> pattern_list;
		Statement inner; Expression guard;
		
		PatternList(out pattern_list, out guard);
		Stmt(out inner);
		clauses.Add(Statement.MakeMatchClause(pattern_list, guard, inner)); 
		while (StartOf(14)) {
			PatternList(out pattern_list, out guard);
			Stmt(out inner);
			clauses.Add(Statement.MakeMatchClause(pattern_list, guard, inner)); 
		}
	}

	void PatternList(out List<PatternConstruct> patterns, out Expression guard ) {
		patterns = new List<PatternConstruct>(); PatternConstruct tmp; guard = null; 
		Pattern(out tmp);
		patterns.Add(tmp); 
		while (la.kind == 73) {
			while (!(la.kind == 0 || la.kind == 73)) {SynErr(132); Get();}
			Get();
			Pattern(out tmp);
			patterns.Add(tmp); 
		}
		if (la.kind == 69) {
			Get();
			CondExpr(out guard);
		}
		Expect(74);
	}

	void Pattern(out PatternConstruct pattern) {
		pattern = null; bool is_binding = false; Modifiers modifier = ExpressoModifiers.None; 
		if (StartOf(15)) {
			if (la.kind == 23 || la.kind == 24) {
				if (la.kind == 23) {
					Get();
					is_binding = true; modifier = ExpressoModifiers.Immutable; 
				} else {
					Get();
					is_binding = true; 
				}
			}
			LhsPattern(out pattern);
		} else if (StartOf(16)) {
			ExpressionPattern(out pattern);
			if(is_binding)
			   pattern = PatternConstruct.MakeValueBindingPattern(pattern, modifier);
			
		} else SynErr(133);
	}

	void ExpressionPattern(out PatternConstruct pattern) {
		Expression expr, upper = null, step = null;
		TextLocation loc; bool upper_inclusive = false;
		
		Literal(out expr);
		if (la.kind == 1 || la.kind == 2) {
			RangeOperator(ref upper_inclusive);
			loc = CurrentLocation; 
			Expect(15);
			upper = Expression.MakeConstant("int", t.val, loc); 
			if (la.kind == 4) {
				Get();
				loc = CurrentLocation; 
				Expect(15);
				step = Expression.MakeConstant("int", t.val, loc); 
			}
			expr = Expression.MakeIntSeq(expr, upper, step, upper_inclusive); 
		}
		pattern = PatternConstruct.MakeExpressionPattern(expr); 
	}

	void RangeOperator(ref bool upper_inclusive) {
		if (la.kind == 1) {
			Get();
			upper_inclusive = false; 
		} else if (la.kind == 2) {
			Get();
			upper_inclusive = true; 
		} else SynErr(134);
	}

	void PatternItem(out PatternConstruct pattern) {
		pattern = PatternConstruct.Null; 
		if (StartOf(16)) {
			ExpressionPattern(out pattern);
		} else if (la.kind == 14) {
			IdentifierPattern(out pattern);
		} else SynErr(135);
	}

	void IdentifierPattern(out PatternConstruct pattern) {
		PatternConstruct inner = null; string name; 
		Expect(14);
		name = t.val;
		Symbols.AddSymbol(name, new PlaceholderType(TextLocation.Empty));
		
		if (la.kind == 76) {
			Get();
			LhsPattern(out inner);
		}
		pattern = PatternConstruct.MakeIdentifierPattern(name, inner); 
	}

	void WildcardPattern(out PatternConstruct pattern) {
		Expect(75);
		pattern = PatternConstruct.MakeWildcardPattern(); 
	}

	void TuplePattern(out PatternConstruct pattern) {
		var inners = new List<PatternConstruct>(); 
		Expect(8);
		while (StartOf(17)) {
			if (StartOf(18)) {
				LhsPattern(out pattern);
			} else {
				ExpressionPattern(out pattern);
			}
			inners.Add(pattern); 
			Expect(12);
		}
		Expect(10);
		pattern = PatternConstruct.MakeTuplePattern(inners); 
	}

	void DestructuringPattern(out PatternConstruct pattern) {
		pattern = PatternConstruct.Null; AstType type_path;
		var patterns = new List<PatternConstruct>(); bool is_vector = false;
		
		if (la.kind == 14) {
			TypePathExpression(out type_path);
			Expect(7);
			if (StartOf(19)) {
				PatternItem(out pattern);
				patterns.Add(pattern); 
			}
			while (la.kind == 12) {
				Get();
				PatternItem(out pattern);
				patterns.Add(pattern); 
			}
			Expect(11);
			pattern = PatternConstruct.MakeDestructuringPattern(type_path, patterns); 
		} else if (la.kind == 9) {
			Get();
			if (StartOf(19)) {
				PatternItem(out pattern);
				patterns.Add(pattern); 
			}
			while (NotFinalComma()) {
				ExpectWeak(12, 20);
				PatternItem(out pattern);
				patterns.Add(pattern); 
			}
			if (la.kind == 12) {
				Get();
				Expect(2);
				is_vector = true; 
			}
			Expect(3);
			pattern = PatternConstruct.MakeCollectionPattern(patterns, is_vector); 
		} else SynErr(136);
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		if (la.kind == 78) {
			Get();
			OrTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalOr, expr, rhs); 
		}
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		Comparison(out expr);
		if (la.kind == 79) {
			Get();
			AndTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalAnd, expr, rhs); 
		}
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		IntSeqExpr(out expr);
		type = OperatorType.Equality; 
		if (StartOf(21)) {
			ComparisonOperator(out type);
			Comparison(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void IntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null;
		bool upper_inclusive = true;
		
		BitOr(out start);
		expr = start; 
		if (la.kind == 1 || la.kind == 2) {
			RangeOperator(ref upper_inclusive);
			BitOr(out end);
			if (la.kind == 4) {
				Get();
				BitOr(out step);
			}
			if(step == null) step = Expression.MakeConstant("int", 1, TextLocation.Empty);
			expr = Expression.MakeIntSeq(start, end, step, upper_inclusive);
			
		}
	}

	void ComparisonOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		switch (la.kind) {
		case 80: {
			Get();
			opType = OperatorType.Equality; 
			break;
		}
		case 81: {
			Get();
			opType = OperatorType.InEquality; 
			break;
		}
		case 82: {
			Get();
			opType = OperatorType.LessThan; 
			break;
		}
		case 83: {
			Get();
			opType = OperatorType.GreaterThan; 
			break;
		}
		case 84: {
			Get();
			opType = OperatorType.LessThanOrEqual; 
			break;
		}
		case 85: {
			Get();
			opType = OperatorType.GreaterThanOrEqual; 
			break;
		}
		default: SynErr(137); break;
		}
	}

	void BitOr(out Expression expr) {
		Expression rhs; 
		BitXor(out expr);
		if (la.kind == 73) {
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
		if (la.kind == 39) {
			Get();
			BitAnd(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseAnd, expr, rhs); 
		}
	}

	void ShiftOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOp(out expr);
		if (la.kind == 87 || la.kind == 88) {
			ShiftOperator(out type);
			ShiftOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AddOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		if (la.kind == 89 || la.kind == 90) {
			AdditiveOperator(out type);
			AddOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void ShiftOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 87) {
			Get();
			opType = OperatorType.BitwiseShiftLeft; 
		} else if (la.kind == 88) {
			Get();
			opType = OperatorType.BitwiseShiftRight; 
		} else SynErr(138);
	}

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		PowerOp(out expr);
		if (la.kind == 91 || la.kind == 92 || la.kind == 93) {
			MultiplicativeOperator(out type);
			Term(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AdditiveOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 89) {
			Get();
			opType = OperatorType.Plus; 
		} else if (la.kind == 90) {
			Get();
			opType = OperatorType.Minus; 
		} else SynErr(139);
	}

	void PowerOp(out Expression expr) {
		Expression rhs; 
		Factor(out expr);
		if (la.kind == 94) {
			Get();
			Factor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.Power, expr, rhs); 
		}
	}

	void MultiplicativeOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 91) {
			Get();
			opType = OperatorType.Times; 
		} else if (la.kind == 92) {
			Get();
			opType = OperatorType.Divide; 
		} else if (la.kind == 93) {
			Get();
			opType = OperatorType.Modulus; 
		} else SynErr(140);
	}

	void Factor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; 
		if (StartOf(22)) {
			Primary(out expr);
		} else if (StartOf(23)) {
			UnaryOperator(out type);
			Factor(out factor);
			expr = Expression.MakeUnaryExpr(type, factor); 
		} else SynErr(141);
	}

	void Primary(out Expression expr) {
		expr = null; PathExpression path; AstType type_path = null; 
		if (la.kind == 14) {
			PathExpression(out path);
			expr = path; 
			if (la.kind == 7) {
				type_path = ConvertPathToType(path); 
				ObjectCreation(type_path, out expr);
			}
		} else if (StartOf(24)) {
			Atom(out expr);
		} else if (la.kind == 96) {
			NewExpression(out expr);
		} else SynErr(142);
		while (la.kind == 8 || la.kind == 9 || la.kind == 13) {
			Trailer(ref expr);
		}
	}

	void UnaryOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 89 || la.kind == 90) {
			AdditiveOperator(out opType);
		} else if (la.kind == 95) {
			Get();
			opType = OperatorType.Not; 
		} else if (la.kind == 39) {
			Get();
			opType = OperatorType.Reference; 
		} else if (la.kind == 91) {
			Get();
			opType = OperatorType.Dereference; 
		} else SynErr(143);
	}

	void ObjectCreation(AstType typePath, out Expression expr) {
		var fields = new List<Identifier>(); var values = new List<Expression>();
		
		Expect(7);
		Expect(14);
		fields.Add(AstNode.MakeIdentifier(t.val)); 
		Expect(4);
		CondExpr(out expr);
		values.Add(expr); 
		while (WeakSeparator(12,3,25) ) {
			Expect(14);
			fields.Add(AstNode.MakeIdentifier(t.val)); 
			Expect(4);
			CondExpr(out expr);
			values.Add(expr); 
		}
		Expect(11);
		expr = Expression.MakeObjectCreation(typePath, fields, values); 
	}

	void Atom(out Expression expr) {
		var exprs = new List<Expression>(); expr = null; bool seen_trailing_comma = false; 
		if (StartOf(16)) {
			Literal(out expr);
		} else if (la.kind == 8) {
			Get();
			if (la.kind == 10) {
				Get();
				expr = Expression.MakeParen(Expression.MakeSequence(null)); 
			} else if (StartOf(11)) {
				CondExpr(out expr);
				exprs.Add(expr); 
				while (NotFinalComma()) {
					ExpectWeak(12, 26);
					CondExpr(out expr);
					exprs.Add(expr); 
				}
				if (la.kind == 12) {
					Get();
					seen_trailing_comma = true; 
				}
				Expect(10);
				if(exprs.Count == 1)
				   expr = Expression.MakeParen(seen_trailing_comma ? Expression.MakeSequence(exprs[0]) : exprs[0]);
				else
				   expr = Expression.MakeParen(Expression.MakeSequence(exprs));
				
			} else SynErr(144);
		} else if (la.kind == 9) {
			Get();
			if (StartOf(27)) {
				SequenceMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 3)) {SynErr(145); Get();}
			Expect(3);
			if(expr == null){
			                     var type = CreateTypeWithArgs("array", new PlaceholderType(TextLocation.Empty));
			expr = Expression.MakeSeqInitializer(type, Enumerable.Empty<Expression>());
			                 }
			
		} else if (la.kind == 7) {
			Get();
			if (StartOf(11)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 11)) {SynErr(146); Get();}
			Expect(11);
			if(expr == null){
			   var type = CreateTypeWithArgs("dictionary", new PlaceholderType(TextLocation.Empty), new PlaceholderType(TextLocation.Empty));
			   expr = Expression.MakeSeqInitializer(type, Enumerable.Empty<Expression>());
			}
			
		} else SynErr(147);
	}

	void NewExpression(out Expression expr) {
		AstType type_path; 
		Expect(96);
		TypePathExpression(out type_path);
		ObjectCreation(type_path, out expr);
		expr = Expression.MakeNewExpr((ObjectCreationExpression)expr); 
	}

	void Trailer(ref Expression expr) {
		var args = new List<Expression>(); 
		if (la.kind == 8) {
			Get();
			if (StartOf(11)) {
				ArgList(out args);
			}
			Expect(10);
			expr = Expression.MakeCallExpr(expr, args); 
		} else if (la.kind == 9) {
			Get();
			ArgList(out args);
			Expect(3);
			expr = Expression.MakeIndexer(expr, args); 
		} else if (la.kind == 13) {
			Get();
			Expect(14);
			expr = Expression.MakeMemRef(expr, AstNode.MakeIdentifier(t.val)); 
		} else SynErr(148);
	}

	void ArgList(out List<Expression> args ) {
		args = new List<Expression>(); Expression expr; 
		CondExpr(out expr);
		args.Add(expr); 
		while (WeakSeparator(12,11,28) ) {
			CondExpr(out expr);
			args.Add(expr); 
		}
	}

	void SequenceMaker(out Expression expr) {
		var exprs = new List<Expression>();
		expr = null; ComprehensionIter comp = null;
		string seq_type_name = "array";
		
		if (la.kind == 2) {
			Get();
			expr = Expression.MakeSeqInitializer(CreateTypeWithArgs("vector", new PlaceholderType(TextLocation.Empty)), Enumerable.Empty<Expression>()); 
		} else if (StartOf(11)) {
			CondExpr(out expr);
			exprs.Add(expr); 
			if (la.kind == 3 || la.kind == 12) {
				while (NotFinalComma()) {
					ExpectWeak(12, 26);
					CondExpr(out expr);
					exprs.Add(expr); 
				}
				if (la.kind == 12) {
					Get();
					Expect(2);
					seq_type_name = "vector"; 
				}
				var type = CreateTypeWithArgs(seq_type_name, new PlaceholderType(TextLocation.Empty));
				expr = Expression.MakeSeqInitializer(type, exprs);
				
			} else if (la.kind == 22) {
				CompFor(out comp);
				Symbols.AddScope();
				GoDownScope();
				
				var type = CreateTypeWithArgs("vector", new PlaceholderType(TextLocation.Empty));
				expr = Expression.MakeComp(expr, (ComprehensionForClause)comp, type);
				GoUpScope();
				
			} else SynErr(149);
		} else SynErr(150);
	}

	void DictMaker(out Expression expr) {
		Expression key, val; var list = new List<KeyValueLikeExpression>();
		      KeyValueLikeExpression pair; ComprehensionIter comp; expr = null;
		      var type = CreateTypeWithArgs("dictionary", new PlaceholderType(TextLocation.Empty), new PlaceholderType(TextLocation.Empty));
		   
		BitOr(out key);
		Expect(4);
		CondExpr(out val);
		pair = Expression.MakeKeyValuePair(key, val);
		list.Add(pair);
		
		if (la.kind == 11 || la.kind == 12) {
			while (WeakSeparator(12,11,25) ) {
				BitOr(out key);
				Expect(4);
				CondExpr(out val);
				pair = Expression.MakeKeyValuePair(key, val);
				list.Add(pair);
				
			}
			expr = Expression.MakeSeqInitializer(type, list); 
		} else if (la.kind == 22) {
			CompFor(out comp);
			expr = Expression.MakeComp(pair, (ComprehensionForClause)comp, type); 
		} else SynErr(151);
	}

	void CompFor(out ComprehensionIter expr) {
		Expression rvalue = null; ComprehensionIter body = null; PatternConstruct target; 
		Expect(22);
		LhsPattern(out target);
		Expect(29);
		CondExpr(out rvalue);
		if (la.kind == 22 || la.kind == 69) {
			CompIter(out body);
		}
		expr = Expression.MakeCompFor(target, rvalue, body); 
	}

	void CompIter(out ComprehensionIter expr) {
		expr = null; 
		if (la.kind == 22) {
			CompFor(out expr);
		} else if (la.kind == 69) {
			CompIf(out expr);
		} else SynErr(152);
	}

	void CompIf(out ComprehensionIter expr) {
		Expression tmp; ComprehensionIter body = null; 
		Expect(69);
		OrTest(out tmp);
		if (la.kind == 22 || la.kind == 69) {
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
		{T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,T,T, x,x,x,x, x,x,T,x, x,x,x,x, x,x,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,T,T,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, T,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,T,T, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,T, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,T, T,T,x,x, x,x,T,T, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,T, T,x,x},
		{x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,T, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, T,T,x,T, T,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x},
		{T,x,x,T, x,x,T,x, x,x,x,T, x,x,T,T, T,T,x,T, T,T,x,x, x,x,x,T, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,T, T,T,x,x, x,x,T,T, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,T, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,T, T,T,x,x, x,x,x,T, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{T,x,x,T, x,x,T,T, T,T,x,T, x,x,T,T, T,T,x,T, T,T,x,x, x,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,T, T,x,x},
		{x,x,T,x, x,x,x,T, T,T,x,x, x,x,T,T, T,T,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,T, T,T,T,T, T,x,x},
		{x,x,x,T, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x}

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
			case 5: s = "double_colon expected"; break;
			case 6: s = "semicolon expected"; break;
			case 7: s = "lcurly expected"; break;
			case 8: s = "lparen expected"; break;
			case 9: s = "lbracket expected"; break;
			case 10: s = "rparen expected"; break;
			case 11: s = "rcurly expected"; break;
			case 12: s = "comma expected"; break;
			case 13: s = "dot expected"; break;
			case 14: s = "ident expected"; break;
			case 15: s = "integer expected"; break;
			case 16: s = "float expected"; break;
			case 17: s = "hex_digit expected"; break;
			case 18: s = "unicode_escape expected"; break;
			case 19: s = "character_literal expected"; break;
			case 20: s = "string_literal expected"; break;
			case 21: s = "raw_string_literal expected"; break;
			case 22: s = "keyword_for expected"; break;
			case 23: s = "keyword_let expected"; break;
			case 24: s = "keyword_var expected"; break;
			case 25: s = "\"export\" expected"; break;
			case 26: s = "\"module\" expected"; break;
			case 27: s = "\"import\" expected"; break;
			case 28: s = "\"as\" expected"; break;
			case 29: s = "\"in\" expected"; break;
			case 30: s = "\"class\" expected"; break;
			case 31: s = "\"public\" expected"; break;
			case 32: s = "\"protected\" expected"; break;
			case 33: s = "\"private\" expected"; break;
			case 34: s = "\"static\" expected"; break;
			case 35: s = "\"def\" expected"; break;
			case 36: s = "\"->\" expected"; break;
			case 37: s = "\"(-\" expected"; break;
			case 38: s = "\"=\" expected"; break;
			case 39: s = "\"&\" expected"; break;
			case 40: s = "\"int\" expected"; break;
			case 41: s = "\"uint\" expected"; break;
			case 42: s = "\"bool\" expected"; break;
			case 43: s = "\"float\" expected"; break;
			case 44: s = "\"double\" expected"; break;
			case 45: s = "\"bigint\" expected"; break;
			case 46: s = "\"string\" expected"; break;
			case 47: s = "\"byte\" expected"; break;
			case 48: s = "\"char\" expected"; break;
			case 49: s = "\"vector\" expected"; break;
			case 50: s = "\"dictionary\" expected"; break;
			case 51: s = "\"function\" expected"; break;
			case 52: s = "\"intseq\" expected"; break;
			case 53: s = "\"void\" expected"; break;
			case 54: s = "\"return\" expected"; break;
			case 55: s = "\"break\" expected"; break;
			case 56: s = "\"upto\" expected"; break;
			case 57: s = "\"continue\" expected"; break;
			case 58: s = "\"yield\" expected"; break;
			case 59: s = "\"+=\" expected"; break;
			case 60: s = "\"-=\" expected"; break;
			case 61: s = "\"*=\" expected"; break;
			case 62: s = "\"/=\" expected"; break;
			case 63: s = "\"**=\" expected"; break;
			case 64: s = "\"%=\" expected"; break;
			case 65: s = "\"&=\" expected"; break;
			case 66: s = "\"|=\" expected"; break;
			case 67: s = "\"<<=\" expected"; break;
			case 68: s = "\">>=\" expected"; break;
			case 69: s = "\"if\" expected"; break;
			case 70: s = "\"else\" expected"; break;
			case 71: s = "\"while\" expected"; break;
			case 72: s = "\"match\" expected"; break;
			case 73: s = "\"|\" expected"; break;
			case 74: s = "\"=>\" expected"; break;
			case 75: s = "\"_\" expected"; break;
			case 76: s = "\"@\" expected"; break;
			case 77: s = "\"?\" expected"; break;
			case 78: s = "\"||\" expected"; break;
			case 79: s = "\"&&\" expected"; break;
			case 80: s = "\"==\" expected"; break;
			case 81: s = "\"!=\" expected"; break;
			case 82: s = "\"<\" expected"; break;
			case 83: s = "\">\" expected"; break;
			case 84: s = "\"<=\" expected"; break;
			case 85: s = "\">=\" expected"; break;
			case 86: s = "\"^\" expected"; break;
			case 87: s = "\"<<\" expected"; break;
			case 88: s = "\">>\" expected"; break;
			case 89: s = "\"+\" expected"; break;
			case 90: s = "\"-\" expected"; break;
			case 91: s = "\"*\" expected"; break;
			case 92: s = "\"/\" expected"; break;
			case 93: s = "\"%\" expected"; break;
			case 94: s = "\"**\" expected"; break;
			case 95: s = "\"!\" expected"; break;
			case 96: s = "\"new\" expected"; break;
			case 97: s = "\"true\" expected"; break;
			case 98: s = "\"false\" expected"; break;
			case 99: s = "\"self\" expected"; break;
			case 100: s = "\"super\" expected"; break;
			case 101: s = "??? expected"; break;
			case 102: s = "invalid ModuleBody"; break;
			case 103: s = "invalid ModuleBody"; break;
			case 104: s = "this symbol not expected in ModuleNameDefinition"; break;
			case 105: s = "this symbol not expected in FuncDecl"; break;
			case 106: s = "this symbol not expected in ClassDecl"; break;
			case 107: s = "invalid ClassDecl"; break;
			case 108: s = "this symbol not expected in ClassDecl"; break;
			case 109: s = "this symbol not expected in ImportDecl"; break;
			case 110: s = "invalid ImportDecl"; break;
			case 111: s = "this symbol not expected in ImportDecl"; break;
			case 112: s = "invalid Modifiers"; break;
			case 113: s = "invalid FieldDecl"; break;
			case 114: s = "this symbol not expected in FieldDecl"; break;
			case 115: s = "invalid Type"; break;
			case 116: s = "invalid Literal"; break;
			case 117: s = "invalid Stmt"; break;
			case 118: s = "invalid SimpleStmt"; break;
			case 119: s = "this symbol not expected in CompoundStmt"; break;
			case 120: s = "invalid CompoundStmt"; break;
			case 121: s = "invalid ExprStmt"; break;
			case 122: s = "this symbol not expected in ExprStmt"; break;
			case 123: s = "invalid VarDeclStmt"; break;
			case 124: s = "this symbol not expected in VarDeclStmt"; break;
			case 125: s = "this symbol not expected in ReturnStmt"; break;
			case 126: s = "this symbol not expected in BreakStmt"; break;
			case 127: s = "this symbol not expected in ContinueStmt"; break;
			case 128: s = "this symbol not expected in YieldStmt"; break;
			case 129: s = "this symbol not expected in EmptyStmt"; break;
			case 130: s = "invalid AugAssignOpe"; break;
			case 131: s = "invalid LhsPattern"; break;
			case 132: s = "this symbol not expected in PatternList"; break;
			case 133: s = "invalid Pattern"; break;
			case 134: s = "invalid RangeOperator"; break;
			case 135: s = "invalid PatternItem"; break;
			case 136: s = "invalid DestructuringPattern"; break;
			case 137: s = "invalid ComparisonOperator"; break;
			case 138: s = "invalid ShiftOperator"; break;
			case 139: s = "invalid AdditiveOperator"; break;
			case 140: s = "invalid MultiplicativeOperator"; break;
			case 141: s = "invalid Factor"; break;
			case 142: s = "invalid Primary"; break;
			case 143: s = "invalid UnaryOperator"; break;
			case 144: s = "invalid Atom"; break;
			case 145: s = "this symbol not expected in Atom"; break;
			case 146: s = "this symbol not expected in Atom"; break;
			case 147: s = "invalid Atom"; break;
			case 148: s = "invalid Trailer"; break;
			case 149: s = "invalid SequenceMaker"; break;
			case 150: s = "invalid SequenceMaker"; break;
			case 151: s = "invalid DictMaker"; break;
			case 152: s = "invalid CompIter"; break;

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
}