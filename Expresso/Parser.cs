using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

using Expresso.Ast;
using Expresso.Ast.Analysis;
using Expresso.CodeGen;
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
	public const int _keyword_if = 22;
	public const int _keyword_for = 23;
	public const int _keyword_in = 24;
	public const int _keyword_while = 25;
	public const int _keyword_match = 26;
	public const int _keyword_let = 27;
	public const int _keyword_var = 28;
	public const int _keyword_as = 29;
	public const int maxT = 107;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

string cur_class_name;
    ExpressoModifiers cur_modifiers;
	bool is_first_comprehension_for_clause = true, defining_closure_parameters = false;
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
        CSharpCompilerHelper.Prepare();
        Symbols = SymbolTable.Create();
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
			SemanticError("Error ES0030: Unknown object type");
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
                SemanticError("Error ES0040: Invalid uint representation!");
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
                SemanticError("Error ES0050: Invalid float representation!");
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
                SemanticError("Error ES0051: Unknown sequence for numeric literals! Make sure that you write a number!");
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
                type = AstType.MakeSimpleType(item, TextLocation.Empty);
            else
                type = AstType.MakeMemberType(type, AstType.MakeSimpleType(item, TextLocation.Empty), TextLocation.Empty);
        }

        return type;
    }

    SimpleType CreateTypeWithArgs(string typeName, params AstType[] typeArgs)
    {
        return new SimpleType(typeName, typeArgs, TextLocation.Empty, TextLocation.Empty);
    }

    bool IsPrimitiveGenericType(string name)
    {
        return name != null && (name == "dictionary" || name == "vector");
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
        var la = this.la;
        var x = scanner.Peek();
        scanner.ResetPeek();
        return la.kind == _ident && x.kind != _double_colon && x.kind != _lcurly;
    }

    bool NotFinalComma()
    {
        var t = la;
        var tt = scanner.Peek();
        scanner.ResetPeek();
        return t.kind == _comma && tt.kind != _rparen && tt.kind != _rbracket && tt.kind != _triple_dots;
    }

    bool IsObjectCreation()
    {
        var t = la;
        if(t.kind != _lcurly && t.kind != _double_colon)
            return false;

        var x = t;
        while(x.kind == _ident || x.kind == _double_colon)
            x = scanner.Peek();

        if(x.kind != _lcurly){
            scanner.ResetPeek();
            return false;
        }

        var key = scanner.Peek();
        var tt = scanner.Peek();
        scanner.ResetPeek();
        return key.kind == _ident && tt.kind == _colon;
    }

    bool IsStartOfAnotherType()
    {
        var t = la;
        var tt = scanner.Peek();
        scanner.ResetPeek();
        return t.val != "|" || tt.val != "->";
    }

    bool IsStartOfBlockScope()
    {
        return la.val == "{";
    }

    bool IsStartOfKeyValuePair()
    {
        var t = la;
        var token = scanner.Peek();
        scanner.ResetPeek();
        return t.kind == _ident && token.kind == _colon;
    }

    bool CheckKeyword(string name)
    {
        if(KnownTypeReference.Keywords.Contains(name)){
            SemanticError("Error ES0005: {0} is reserverd for a keyword.", name);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Reports a semantic error message.
    /// It is intended to be used inside the Parser class.
    /// </summary>
	public void SemanticError(string format, params object[] args)
	{
		//Convenient method for printing a semantic error with a format string
		errors.SemErr(string.Format(format, args));
	}

    /// <summary>
    /// Reports a semantic error message with a range.
    /// It is intended to be used inside the Parser class.
    /// </summary>
    public void SemanticError(TextLocation loc, string format, params object[] args)
    {
        errors.SemErr(string.Format("{0} ~ {1} -- {2}", loc, CurrentLocation, string.Format(format, args)));
    }

    /// <summary>
    /// Reports a warning message.
    /// It is intended to be used from outside the Parser class.
    /// </summary>
    public void ReportWarning(string format, AstNode node, params object[] objects)
    {
        errors.Warning(string.Format("{0} -- {1}", node.StartLocation, string.Format(format, objects)));
    }

    /// <summary>
    /// Reports a semantic error message.
    /// It is intended to be used from outside the Parser class.
    /// </summary>
    public void ReportSemanticError(string format, AstNode node, params object[] objects)
    {
        errors.SemErr(string.Format("{0} -- {1}", node.StartLocation, string.Format(format, objects)));
    }

    /// <summary>
    /// Reports a semantic error message with a range information.
    /// It is intended to be used from outside the Parser class.
    /// </summary>
    public void ReportSemanticErrorRegional(string format, AstNode start, AstNode end, params object[] objects)
    {
        var real_message = string.Format("{0} ~ {1} -- {2}", start.StartLocation, end.EndLocation, string.Format(format, objects));
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
		     try{
		  
		ModuleBody(out module_decl);
		Debug.Assert(Symbols.Parent.Name == "root", "The symbol table should indicate \"programRoot\" before name binding ");
		if(DoPostParseProcessing)
		   ExpressoNameBinder.BindAst(module_decl, this); //Here's the start of post-parse processing
		}
		catch(ParserException e){
		errors.SemErr(e.ToString());
		throw e;
		}
		catch(FatalError ex){
		// Do nothing with a FatalError
		// It only signals that the program should immediately exit
		throw ex;
		}
		catch(StackOverflowException e){
		Console.WriteLine(e.Message);
		}
		
		this.TopmostAst = module_decl;	//Currently there is not so much code out there, though...
		
	}

	void ModuleBody(out ExpressoAst ast) {
		var decls = new List<EntityDeclaration>();
		string module_name;
		List<ImportDeclaration> prog_defs = null; EntityDeclaration decl = null;
		
		ModuleNameDefinition(out module_name);
		if (la.kind == 32) {
			ProgramDefinition(out prog_defs);
		}
		if (la.kind == 30) {
			Get();
			cur_modifiers = ExpressoModifiers.Export; 
		}
		if (la.kind == 34) {
			FuncDecl(out decl, cur_modifiers);
		} else if (la.kind == 27 || la.kind == 28) {
			FieldDecl(out decl);
		} else if (la.kind == 36) {
			ClassDecl(out decl, cur_modifiers);
		} else if (la.kind == 33) {
			InterfaceDecl(out decl, cur_modifiers);
		} else SynErr(108);
		decls.Add(decl);
		cur_modifiers = ExpressoModifiers.None;
		
		while (StartOf(1)) {
			if (la.kind == 30) {
				Get();
				cur_modifiers = ExpressoModifiers.Export; 
			}
			if (la.kind == 34) {
				FuncDecl(out decl, cur_modifiers);
			} else if (la.kind == 27 || la.kind == 28) {
				FieldDecl(out decl);
			} else if (la.kind == 36) {
				ClassDecl(out decl, cur_modifiers);
			} else if (la.kind == 33) {
				InterfaceDecl(out decl, cur_modifiers);
			} else SynErr(109);
			decls.Add(decl);
			cur_modifiers = ExpressoModifiers.None;
			
		}
		ast = AstNode.MakeModuleDef(module_name, decls, prog_defs); 
	}

	void ModuleNameDefinition(out string moduleName) {
		
		Expect(31);
		Expect(14);
		moduleName = t.val; 
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(110); Get();}
		Expect(6);
	}

	void ProgramDefinition(out List<ImportDeclaration> imports ) {
		imports = new List<ImportDeclaration>();
		ImportDeclaration tmp;
		
		ImportDecl(out tmp);
		imports.Add(tmp); 
		while (la.kind == 32) {
			ImportDecl(out tmp);
			imports.Add(tmp); 
		}
	}

	void FuncDecl(out EntityDeclaration decl, Modifiers modifiers) {
		Identifier ident = null;
		string name; AstType type = null; BlockStatement block;
		var type_params = new List<ParameterType>();
		var @params = new List<ParameterDeclaration>();
		var start_loc = NextLocation;
		var replacer = new ParameterTypeReplacer(type_params);
		
		while (!(la.kind == 0 || la.kind == 34)) {SynErr(111); Get();}
		Expect(34);
		Symbols.AddScope();
		var ident_start_loc = CurrentLocation;
		
		Expect(14);
		name = t.val;
		if(!CheckKeyword(name)){
		   ident = AstNode.MakeIdentifier(name, AstType.MakePlaceholderType(CurrentLocation), modifiers, ident_start_loc); 
		   Symbols.AddSymbol(name, ident);
		}else{
		   // The name is unsuitable for a method or a function name.
		   // Leave the parser to recover its state.
		}
		
		if (la.kind == 45) {
			GenericTypeParameters(ref type_params);
		}
		Expect(8);
		GoDownScope();
		Symbols.Name = "func " + name + "`" + ScopeId++;
		
		if (la.kind == 14) {
			ParamList(type_params, ref @params);
		}
		Expect(10);
		if (la.kind == 35) {
			Get();
			Type(out type);
		}
		if(type == null)
		   type = new PlaceholderType(TextLocation.Empty);
		
		type.AcceptWalker(replacer);
		
		Block(out block);
		decl = EntityDeclaration.MakeFunc(ident, @params, block, type, modifiers, start_loc);
		        GoUpScope();
		     
	}

	void FieldDecl(out EntityDeclaration field) {
		Expression rhs; PatternConstruct pattern; var start_loc = NextLocation;
		var patterns = new List<PatternConstruct>(); var exprs = new List<Expression>();
		
		if (la.kind == 27) {
			Get();
			cur_modifiers |= ExpressoModifiers.Immutable; 
		} else if (la.kind == 28) {
			Get();
		} else SynErr(112);
		VarDef(out pattern, out rhs);
		if(pattern is PatternWithType pattern_type && !(pattern_type.Pattern is IdentifierPattern))
		  SemanticError("Error ES0021: A field can only contain an identifier pattern; `{0}`", pattern_type.Pattern);
		
		patterns.Add(pattern);
		exprs.Add(rhs);
		
		while (la.kind == 12) {
			Get();
			VarDef(out pattern, out rhs);
			if(pattern is PatternWithType pattern_type2 && !(pattern_type2.Pattern is IdentifierPattern))
			  SemanticError("Error ES0021: A field can only contain an indentifier pattern; `{0}`", pattern_type2.Pattern);
			
			patterns.Add(pattern);
			exprs.Add(rhs);
			
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(113); Get();}
		Expect(6);
		field = EntityDeclaration.MakeField(patterns, exprs, cur_modifiers, start_loc, CurrentLocation);
		                  cur_modifiers = ExpressoModifiers.None; 
		               
	}

	void ClassDecl(out EntityDeclaration decl, Modifiers modifiers) {
		EntityDeclaration entity = null; var decls = new List<EntityDeclaration>(); AstType type_path;
		string name; var bases = new List<AstType>(); var start_loc = NextLocation;
		Identifier ident = null;
		
		while (!(la.kind == 0 || la.kind == 36)) {SynErr(114); Get();}
		Expect(36);
		Symbols.AddScope(ClassType.Class);
		var ident_start_loc = CurrentLocation;
		
		Expect(14);
		name = t.val;
		                    if(!CheckKeyword(name)){
		                        ident = AstNode.MakeIdentifier(name, modifiers, ident_start_loc);
		                        Symbols.AddTypeSymbol(name, ident);
		                        cur_class_name = name;
		                    }else{
		                      // Failed to parse an identifier.
		                      // Leave the parser to recover its state.
		                    }
		                 
		if (la.kind == 4) {
			Get();
			Type(out type_path);
			bases.Add(type_path); 
			while (la.kind == 12) {
				Get();
				Type(out type_path);
				bases.Add(type_path); 
			}
		}
		Expect(7);
		GoDownScope();
		Symbols.Name = "type_" + name + "`" + ScopeId++;
		
		while (StartOf(2)) {
			cur_modifiers = ExpressoModifiers.Private; 
			while (StartOf(3)) {
				Modifiers();
			}
			
			if (la.kind == 34) {
				FuncDecl(out entity, cur_modifiers);
				decls.Add(entity); 
			} else if (la.kind == 27 || la.kind == 28) {
				FieldDecl(out entity);
				decls.Add(entity); 
			} else if (la.kind == 36) {
				ClassDecl(out entity, cur_modifiers);
				decls.Add(entity); 
			} else if (la.kind == 33) {
				InterfaceDecl(out entity, cur_modifiers);
				decls.Add(entity); 
			} else SynErr(115);
		}
		while (!(la.kind == 0 || la.kind == 11)) {SynErr(116); Get();}
		Expect(11);
		decl = EntityDeclaration.MakeClassDecl(ident, bases, decls, modifiers, start_loc, CurrentLocation);
		GoUpScope();
		cur_modifiers = ExpressoModifiers.None;
		
	}

	void InterfaceDecl(out EntityDeclaration decl, Modifiers modifiers) {
		string name = null; Identifier ident = null;
		AstType type_path = null;
		EntityDeclaration method = null;
		var bases = new List<AstType>();
		var decls = new List<EntityDeclaration>();
		var start_loc = CurrentLocation;
		
		while (!(la.kind == 0 || la.kind == 33)) {SynErr(117); Get();}
		Expect(33);
		Symbols.AddScope(ClassType.Interface);
		var ident_start_loc = CurrentLocation;
		
		Expect(14);
		name = t.val;
		if(!CheckKeyword(name)){
		   ident = AstNode.MakeIdentifier(name, modifiers, ident_start_loc);
		   Symbols.AddTypeSymbol(name, ident);
		
		}
		
		if (la.kind == 4) {
			Get();
			Type(out type_path);
			bases.Add(type_path); 
			while (la.kind == 12) {
				Get();
				Type(out type_path);
				bases.Add(type_path); 
			}
		}
		Expect(7);
		GoDownScope();
		Symbols.Name = "type_" + name + "`" + ScopeId++;
		
		MethodSignature(out method);
		decls.Add(method); 
		Expect(11);
		decl = EntityDeclaration.MakeInterfaceDecl(ident, bases, decls, modifiers, start_loc, CurrentLocation);
		GoUpScope();
		
	}

	void ImportDecl(out ImportDeclaration decl) {
		decl = null; /*var has_in = false; PathExpression path;*/ Identifier symbol = null;
		Identifier alias = null; var entities = new List<Identifier>();
		
		while (!(la.kind == 0 || la.kind == 32)) {SynErr(118); Get();}
		Expect(32);
		Expect(20);
		var path = t.val.Substring(1, t.val.Length - 2);
		symbol = AstNode.MakeIdentifier(path, ExpressoModifiers.None, CurrentLocation);
		Symbols.AddTypeSymbol(path, symbol);
		if(!symbol.Name.EndsWith(".exs", StringComparison.CurrentCulture))
		   Symbols.AddNativeSymbolTable(symbol);
		
		Expect(29);
		var alias_start_loc = CurrentLocation; 
		Expect(14);
		if(!CheckKeyword(t.val)){
		   alias = AstNode.MakeIdentifier(t.val, ExpressoModifiers.None, alias_start_loc);
		   Symbols.AddTypeSymbol(t.val, symbol);
		}else{
		   // Failed to parse an alias name.
		   // Leave the parser to recover its state.
		}
		
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(119); Get();}
		Expect(6);
		decl = AstNode.MakeImportDecl(symbol, alias);
		
	}

	void Type(out AstType type) {
		var start_loc = NextLocation; type = new PlaceholderType(NextLocation);
		var is_reference = false; string name = null;
		
		if (StartOf(4)) {
			if (la.kind == 47) {
				Get();
				is_reference = true; 
			}
			switch (la.kind) {
			case 48: {
				Get();
				type = CreateType(t.val, start_loc, is_reference); 
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
				type = CreateType(t.val, start_loc, is_reference); 
				break;
			}
			case 54: {
				Get();
				type = CreateType(t.val, start_loc, is_reference); 
				break;
			}
			case 55: {
				Get();
				type = CreateType(t.val, start_loc, is_reference); 
				break;
			}
			case 56: {
				Get();
				type = CreateType(t.val, start_loc, is_reference); 
				break;
			}
			case 8: {
				TupleTypeSignature(out type);
				break;
			}
			case 57: {
				Get();
				name = t.val; 
				break;
			}
			case 58: {
				Get();
				name = t.val; 
				break;
			}
			case 59: {
				Get();
				type = CreateType(t.val, start_loc, is_reference); 
				break;
			}
			case 60: {
				Get();
				type = AstType.MakeSimpleType("tuple", Enumerable.Empty<AstType>(), start_loc, CurrentLocation); 
				break;
			}
			case 14: {
				TypePathExpression(out type);
				if(is_reference) type = AstType.MakeReferenceType(type, TextLocation.Empty); 
				break;
			}
			default: SynErr(120); break;
			}
			start_loc = NextLocation; 
			if (la.kind == 45) {
				GenericTypeSignature(name, is_reference, start_loc, out type);
				if(!IsPrimitiveGenericType(name)){
				   SemanticError("Error ES0006: `{0}` is not a generic type!", name);
				   return;
				}
				
			}
			while (la.kind == 9) {
				Get();
				Expect(3);
				if(type.IsNull)
				   SemanticError("Error ES0007: Array of unknown type is specified. Unknown type is just unknown!");
				
				type = AstType.MakeSimpleType("array", start_loc, CurrentLocation, type);
				
			}
		} else if (la.kind == 61) {
			FunctionTypeSignature(out type);
		} else SynErr(121);
	}

	void MethodSignature(out EntityDeclaration method) {
		Identifier ident = null;
		string name = null; AstType type = null;
		var type_params = new List<ParameterType>();
		var @params = new List<ParameterDeclaration>();
		var start_loc = CurrentLocation;
		
		while (!(la.kind == 0 || la.kind == 34)) {SynErr(122); Get();}
		Expect(34);
		Symbols.AddScope();
		var ident_start_loc = CurrentLocation;
		
		Expect(14);
		name = t.val;
		if(!CheckKeyword(name)){
		   ident = AstNode.MakeIdentifier(name, AstType.MakePlaceholderType(CurrentLocation), ExpressoModifiers.Public, ident_start_loc); 
		   Symbols.AddSymbol(name, ident);
		}else{
		   // The name is unsuitable for a method or a function name.
		   // Leave the parser to recover its state.
		}
		
		if (la.kind == 45) {
			GenericTypeParameters(ref type_params);
		}
		Expect(8);
		GoDownScope();
		Symbols.Name = "method_signature " + name + "`" + ScopeId++;
		
		if (la.kind == 14) {
			ParamList(type_params, ref @params);
		}
		Expect(10);
		Expect(35);
		Type(out type);
		Expect(6);
		method = EntityDeclaration.MakeFunc(ident, @params, null, type, ExpressoModifiers.Public, start_loc);
		GoUpScope();
		
	}

	void GenericTypeParameters(ref List<ParameterType> types ) {
		Expect(45);
		Expect(14);
		types.Add(AstType.MakeParameterType(t.val)); 
		while (la.kind == 12) {
			Get();
			Expect(14);
			types.Add(AstType.MakeParameterType(t.val)); 
		}
		Expect(46);
	}

	void ParamList(List<ParameterType> typeParams, ref List<ParameterDeclaration> @params ) {
		ParameterDeclaration param; bool seen_option = false; var replacer = new ParameterTypeReplacer(typeParams); 
		Parameter(out param);
		if(!param.Option.IsNull)
		   seen_option = true;
		
		param.ReturnType.AcceptWalker(replacer);
		@params.Add(param);
		
		while (WeakSeparator(12,5,6) ) {
			Parameter(out param);
			if(seen_option && param.Option.IsNull)
			   SemanticError("Error ES0002: You can't put optional parameters before non-optional parameters");
			else if(!seen_option && !param.Option.IsNull)
			   seen_option = true;
			
			param.ReturnType.AcceptWalker(replacer);
			@params.Add(param);
			
		}
	}

	void Modifiers() {
		switch (la.kind) {
		case 37: {
			Get();
			cur_modifiers &= ~ExpressoModifiers.Private;
			cur_modifiers |= ExpressoModifiers.Public;
			
			break;
		}
		case 38: {
			Get();
			cur_modifiers &= ~ExpressoModifiers.Private;
			cur_modifiers |= ExpressoModifiers.Protected;
			
			break;
		}
		case 39: {
			Get();
			cur_modifiers |= ExpressoModifiers.Private; 
			break;
		}
		case 40: {
			Get();
			cur_modifiers |= ExpressoModifiers.Static; 
			break;
		}
		case 41: {
			Get();
			cur_modifiers |= ExpressoModifiers.Mutating; 
			break;
		}
		case 42: {
			Get();
			cur_modifiers |= ExpressoModifiers.Override; 
			break;
		}
		default: SynErr(123); break;
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
		while (StartOf(7)) {
			Stmt(out stmt);
			stmts.Add(stmt); 
		}
		while (!(la.kind == 0 || la.kind == 11)) {SynErr(124); Get();}
		Expect(11);
		block = Statement.MakeBlock(stmts, start_loc, CurrentLocation);
		GoUpScope();
		
	}

	void Parameter(out ParameterDeclaration param) {
		string name; Identifier identifier; Expression option = null; AstType type = null; bool is_variadic = false; var start_loc = CurrentLocation; 
		Expect(14);
		name = t.val;
		if(CheckKeyword(name)){
		// Failed to parse a name.
		// Stop parsing parameters.
		param = null;
		return;
		}
		
		if (la.kind == 2) {
			Get();
			is_variadic = true; 
		}
		if (la.kind == 43) {
			Get();
			Type(out type);
			if(is_variadic && la.kind == _ident)
			SemanticError("Error ES0010: The variadic parameter has to be placed in the last position of a parameter list");
			else if(is_variadic && (type == null || !(type is SimpleType simple) || simple.Name != "array"))
			SemanticError("Error ES0001: The variadic parameter must be an array!");
			
		}
		if (la.kind == 44) {
			Get();
			Literal(out option);
		}
		identifier = AstNode.MakeIdentifier(name, type ?? new PlaceholderType(CurrentLocation), ExpressoModifiers.None, start_loc);
		if(!defining_closure_parameters && type == null && option == null)
		SemanticError("Error ES0004: You can't omit both the type annotation and the default value in a function parameter definition!; `{0}`", name);
		
		Symbols.AddSymbol(name, identifier);
		param = EntityDeclaration.MakeParameter(identifier, option, is_variadic, start_loc);
		
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
			       SemanticError("Error ES0008: The number of opening and closing hash symbols in a raw string must match!");
			
			   tmp = tmp.Substring(index_double_quote, tmp.Length - index_end_double_quote - index_double_quote);
			}
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = Expression.MakeConstant("string", tmp, start_loc);
			
			break;
		}
		case 102: {
			Get();
			expr = Expression.MakeConstant("bool", true, start_loc); 
			break;
		}
		case 103: {
			Get();
			expr = Expression.MakeConstant("bool", false, start_loc); 
			break;
		}
		case 105: {
			SelfReferenceExpression(out expr);
			break;
		}
		case 106: {
			SuperReferenceExpression(out expr);
			break;
		}
		case 104: {
			Get();
			expr = Expression.MakeNullRef(start_loc); 
			break;
		}
		default: SynErr(125); break;
		}
	}

	void VarDef(out PatternConstruct pattern, out Expression option) {
		option = null; var loc = NextLocation; 
		PatternWithType(out pattern);
		if (la.kind == 44) {
			Get();
			CondExpr(out option);
		}
		if(pattern is PatternWithType inner && inner.Pattern is IdentifierPattern ident_pat){
		   if(inner.Type is PlaceholderType && option == null)
		       SemanticError(loc, "Error ES0003: Give me some context or I can't infer the type of {0}", ident_pat.Identifier.Name);
		}
		
	}

	void TupleTypeSignature(out AstType type) {
		var inners = new List<AstType>(); var start_loc = NextLocation; 
		Expect(8);
		if (StartOf(8)) {
			Type(out type);
			inners.Add(type); 
		}
		while (la.kind == 12) {
			Get();
			Type(out type);
			inners.Add(type); 
		}
		Expect(10);
		type = AstType.MakeSimpleType("tuple", inners, start_loc, CurrentLocation); 
	}

	void TypePathExpression(out AstType type) {
		Expect(14);
		type = AstType.MakeSimpleType(t.val, CurrentLocation); 
		while (la.kind == 5) {
			Get();
			Expect(14);
			type = AstType.MakeMemberType(type, AstType.MakeSimpleType(t.val, CurrentLocation), NextLocation);
			
		}
	}

	void GenericTypeSignature(string name, bool isReference, TextLocation startLoc, out AstType genericType) {
		var type_args = new List<AstType>(); AstType child_type; 
		Expect(45);
		Type(out child_type);
		type_args.Add(child_type); 
		while (la.kind == 12) {
			Get();
			Type(out child_type);
			type_args.Add(child_type); 
		}
		Expect(46);
		genericType = AstType.MakeSimpleType(name, type_args, startLoc, CurrentLocation);
		if(isReference)
		   genericType = AstType.MakeReferenceType(genericType, CurrentLocation);
		
	}

	void FunctionTypeSignature(out AstType type) {
		var start_loc = NextLocation; AstType inner_type; List<AstType> arg_types = new List<AstType>(); 
		Expect(61);
		while (IsStartOfAnotherType()) {
			Type(out inner_type);
			arg_types.Add(inner_type); 
		}
		Expect(61);
		Expect(35);
		Type(out inner_type);
		type = AstType.MakeFunctionType("closure", inner_type, arg_types, start_loc, CurrentLocation); 
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(9)) {
			SimpleStmt(out stmt);
		} else if (StartOf(10)) {
			CompoundStmt(out stmt);
		} else SynErr(126);
	}

	void SimpleStmt(out Statement stmt) {
		var start_loc = NextLocation; stmt = null; 
		switch (la.kind) {
		case 14: case 105: case 106: {
			ExprStmt(out stmt);
			break;
		}
		case 27: case 28: {
			VarDeclStmt(out stmt);
			break;
		}
		case 62: {
			ReturnStmt(out stmt);
			break;
		}
		case 63: {
			BreakStmt(out stmt);
			break;
		}
		case 65: {
			ContinueStmt(out stmt);
			break;
		}
		case 66: {
			YieldStmt(out stmt);
			break;
		}
		case 67: {
			ThrowStmt(out stmt);
			break;
		}
		case 6: {
			EmptyStmt(out stmt);
			break;
		}
		default: SynErr(127); break;
		}
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; BlockStatement block = null; 
		switch (la.kind) {
		case 7: {
			Block(out block);
			stmt = block; 
			break;
		}
		case 22: {
			IfStmt(out stmt);
			break;
		}
		case 25: {
			WhileStmt(out stmt);
			break;
		}
		case 23: {
			ForStmt(out stmt);
			break;
		}
		case 26: {
			MatchStmt(out stmt);
			break;
		}
		case 80: {
			TryStmt(out stmt);
			break;
		}
		default: SynErr(128); break;
		}
	}

	void ExprStmt(out Statement stmt) {
		SequenceExpression lhs = null, seq = null;
		var start_loc = NextLocation; stmt = null;
		OperatorType op_type = OperatorType.None;
		        AssignmentExpression assign = null;
		
		LValueList(out lhs);
		if (StartOf(11)) {
			if (StartOf(12)) {
				AugmentedAssignOperators(ref op_type);
				RValueList(out seq);
				if(lhs.Count != seq.Count)  //See if both sides have the same number of items or not
				   SemanticError("Error ES0007: An augmented assignment must have both sides balanced.");
				
				stmt = Statement.MakeAugmentedAssignment(op_type, lhs, seq, start_loc, CurrentLocation);
				
			} else {
				Get();
				RValueList(out seq);
				assign = Expression.MakeAssignment(lhs, seq); 
				while (la.kind == 44) {
					Get();
					RValueList(out seq);
					assign = Expression.MakeMultipleAssignment(assign, seq); 
				}
				stmt = Statement.MakeExprStmt(assign, start_loc, CurrentLocation); 
			}
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(129); Get();}
		Expect(6);
		if(stmt == null)
		stmt = Statement.MakeExprStmt(lhs, start_loc, CurrentLocation);
		
	}

	void VarDeclStmt(out Statement stmt) {
		Expression rhs = null; var start_loc = NextLocation;
		PatternConstruct pattern; 
		var patterns = new List<PatternConstruct>(); var exprs = new List<Expression>();
		
		if (la.kind == 27) {
			Get();
			cur_modifiers = ExpressoModifiers.Immutable; 
		} else if (la.kind == 28) {
			Get();
		} else SynErr(130);
		VarDef(out pattern, out rhs);
		patterns.Add(pattern);
		exprs.Add(rhs ?? Expression.Null);
		rhs = null;
		
		while (WeakSeparator(12,13,14) ) {
			VarDef(out pattern, out rhs);
			patterns.Add(pattern);
			exprs.Add(rhs ?? Expression.Null);
			rhs = null;
			
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(131); Get();}
		Expect(6);
		stmt = Statement.MakeVarDecl(patterns, exprs, cur_modifiers, start_loc, CurrentLocation);
		              cur_modifiers = ExpressoModifiers.None;
		           
	}

	void ReturnStmt(out Statement stmt) {
		SequenceExpression items = null; var start_loc = NextLocation; 
		Expect(62);
		if (StartOf(15)) {
			RValueList(out items);
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(132); Get();}
		Expect(6);
		stmt = Statement.MakeReturnStmt(items, start_loc); 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; var start_loc = NextLocation; 
		Expect(63);
		if (la.kind == 64) {
			Get();
			Expect(15);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(133); Get();}
		Expect(6);
		stmt = Statement.MakeBreakStmt(
		   Expression.MakeConstant("int", count, start_loc), start_loc, CurrentLocation
		);
		
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; var start_loc = NextLocation; 
		Expect(65);
		if (la.kind == 64) {
			Get();
			Expect(15);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(134); Get();}
		Expect(6);
		stmt = Statement.MakeContinueStmt(
		                  Expression.MakeConstant("int", count, start_loc), start_loc, CurrentLocation
		              );
		           
	}

	void YieldStmt(out Statement stmt) {
		Expression expr; var start_loc = NextLocation; 
		Expect(66);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(135); Get();}
		Expect(6);
		stmt = Statement.MakeYieldStmt(expr, start_loc, CurrentLocation); 
	}

	void ThrowStmt(out Statement stmt) {
		Expression obj; var start_loc = NextLocation; AstType type_path; 
		Expect(67);
		Type(out type_path);
		ObjectCreation(type_path, out obj);
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(136); Get();}
		Expect(6);
		stmt = Statement.MakeThrowStmt((ObjectCreationExpression)obj, start_loc); 
	}

	void EmptyStmt(out Statement stmt) {
		var start_loc = NextLocation; 
		while (!(la.kind == 0 || la.kind == 6)) {SynErr(137); Get();}
		Expect(6);
		stmt = Statement.MakeEmptyStmt(start_loc); 
	}

	void RValueList(out SequenceExpression seq) {
		Expression tmp; var exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (la.kind == 12) {
			Get();
			CondExpr(out tmp);
			exprs.Add(tmp);	
		}
		seq = Expression.MakeSequenceExpression(exprs); 
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; expr = null; 
		if (StartOf(16)) {
			OrTest(out expr);
			if (la.kind == 85) {
				Get();
				OrTest(out true_expr);
				Expect(4);
				CondExpr(out false_expr);
				expr = Expression.MakeCondExpr(expr, true_expr, false_expr); 
			}
		} else if (la.kind == 61) {
			ClosureLiteral(out expr);
		} else SynErr(138);
	}

	void ObjectCreation(AstType typePath, out Expression expr) {
		var fields = new List<Identifier>(); var values = new List<Expression>(); var start_loc = CurrentLocation;
		/*Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "ObjectCreation`" + ScopeId++;*/
		
		Expect(7);
		if (la.kind == 14) {
			Get();
			fields.Add(AstNode.MakeIdentifier(t.val, ExpressoModifiers.None, CurrentLocation)); 
			Expect(4);
			CondExpr(out expr);
			values.Add(expr); 
		}
		while (la.kind == 12) {
			Get();
			Expect(14);
			fields.Add(AstNode.MakeIdentifier(t.val, ExpressoModifiers.None, CurrentLocation)); 
			Expect(4);
			CondExpr(out expr);
			values.Add(expr); 
		}
		while (!(la.kind == 0 || la.kind == 11)) {SynErr(139); Get();}
		Expect(11);
		expr = Expression.MakeObjectCreation(typePath, fields, values, start_loc, CurrentLocation);
		//GoUpScope();
		
	}

	void AugmentedAssignOperators(ref OperatorType type) {
		switch (la.kind) {
		case 68: {
			Get();
			type = OperatorType.Plus; 
			break;
		}
		case 69: {
			Get();
			type = OperatorType.Minus; 
			break;
		}
		case 70: {
			Get();
			type = OperatorType.Times; 
			break;
		}
		case 71: {
			Get();
			type = OperatorType.Divide; 
			break;
		}
		case 72: {
			Get();
			type = OperatorType.Power; 
			break;
		}
		case 73: {
			Get();
			type = OperatorType.Modulus; 
			break;
		}
		case 74: {
			Get();
			type = OperatorType.BitwiseAnd; 
			break;
		}
		case 75: {
			Get();
			type = OperatorType.BitwiseOr; 
			break;
		}
		case 76: {
			Get();
			type = OperatorType.BitwiseShiftLeft; 
			break;
		}
		case 77: {
			Get();
			type = OperatorType.BitwiseShiftRight; 
			break;
		}
		default: SynErr(140); break;
		}
	}

	void LValueList(out SequenceExpression lhs) {
		var lvalues = new List<Expression>(); Expression tmp; 
		LhsPrimary(out tmp);
		lvalues.Add(tmp); 
		while (WeakSeparator(12,17,18) ) {
			LhsPrimary(out tmp);
			lvalues.Add(tmp); 
		}
		lhs = Expression.MakeSequenceExpression(lvalues); 
	}

	void LhsPrimary(out Expression expr) {
		expr = null; PathExpression path; 
		if (la.kind == 14) {
			PathExpression(out path);
			expr = path; 
		} else if (la.kind == 105) {
			SelfReferenceExpression(out expr);
		} else if (la.kind == 106) {
			SuperReferenceExpression(out expr);
		} else SynErr(141);
		while (la.kind == 8 || la.kind == 9 || la.kind == 13) {
			Trailer(ref expr);
		}
	}

	void IfStmt(out Statement stmt) {
		PatternConstruct pattern = null; BlockStatement true_block, false_block = null;
		      var start_loc = NextLocation;
		   
		Expect(22);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "if`" + ScopeId++;
		
		if (StartOf(19)) {
			ExpressionPattern(out pattern);
		} else if (la.kind == 27 || la.kind == 28) {
			ValueBindingPattern(out pattern);
		} else SynErr(142);
		Block(out true_block);
		if (la.kind == 78) {
			Get();
			Block(out false_block);
		}
		stmt = Statement.MakeIfStmt(pattern, true_block, false_block, start_loc);
		       GoUpScope();
		    
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; BlockStatement body; var start_loc = NextLocation; 
		Expect(25);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "while`" + ScopeId++;
		
		CondExpr(out cond);
		Block(out body);
		stmt = Statement.MakeWhileStmt(cond, body, start_loc);
		GoUpScope();
		
	}

	void ForStmt(out Statement stmt) {
		PatternConstruct left = null; Expression rvalue; BlockStatement body; cur_modifiers = ExpressoModifiers.None;
		     var start_loc = NextLocation;
		
		Expect(23);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "for`" + ScopeId++;
		
		if (la.kind == 27) {
			Get();
			cur_modifiers = ExpressoModifiers.Immutable; 
		} else if (la.kind == 28) {
			Get();
		} else SynErr(143);
		PatternWithType(out left);
		Expect(24);
		CondExpr(out rvalue);
		Block(out body);
		if(left != null){
		   var initializer = AstNode.MakeVariableInitializer(left, rvalue);
		   stmt = Statement.MakeValueBindingForStmt(cur_modifiers, body, start_loc, initializer);
		}else{
		   stmt = Statement.MakeForStmt(left, rvalue, body, start_loc);
		}
		                        GoUpScope();
		                        cur_modifiers = ExpressoModifiers.None;
		                     
	}

	void MatchStmt(out Statement stmt) {
		Expression target; List<MatchPatternClause> matches;
		    var start_loc = NextLocation;
		 
		Expect(26);
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

	void TryStmt(out Statement stmt) {
		var start_loc = NextLocation; BlockStatement body; var catches = new List<CatchClause>(); FinallyClause @finally = null; 
		Expect(80);
		Block(out body);
		if (la.kind == 81) {
			CatchClauses(out catches);
		}
		if (la.kind == 82) {
			FinallyClause(out @finally);
		}
		if(catches.Count == 0 && @finally == null){
		   SemErr("Error ES0020: A try statement must include either a catch clause or the finally clause");
		}
		stmt = Statement.MakeTryStmt(body, catches, @finally, start_loc);
		
	}

	void ExpressionPattern(out PatternConstruct pattern) {
		Expression expr; 
		PatternOrTest(out expr);
		pattern = PatternConstruct.MakeExpressionPattern(expr); 
	}

	void ValueBindingPattern(out PatternConstruct pattern) {
		VariableInitializer init; var modifiers = ExpressoModifiers.None; 
		if (la.kind == 27) {
			Get();
			modifiers = ExpressoModifiers.Immutable; 
		} else if (la.kind == 28) {
			Get();
		} else SynErr(144);
		var inits = new List<VariableInitializer>(); 
		PatternVarDef(out init);
		inits.Add(init); 
		while (la.kind == 12) {
			Get();
			PatternVarDef(out init);
			inits.Add(init); 
		}
		pattern = PatternConstruct.MakeValueBindingPattern(inits, modifiers); 
	}

	void PatternWithType(out PatternConstruct pattern) {
		pattern = PatternConstruct.Null; AstType type = new PlaceholderType(NextLocation); 
		if (la.kind == 83) {
			WildcardPattern(out pattern);
		} else if (IsIdentifierPattern()) {
			IdentifierPattern(out pattern);
		} else if (la.kind == 8) {
			TuplePattern(out pattern);
		} else if (la.kind == 9 || la.kind == 14) {
			DestructuringPattern(out pattern);
		} else SynErr(145);
		if (la.kind == 43) {
			Get();
			Type(out type);
		}
		pattern = PatternConstruct.MakePatternWithType(pattern, type); 
	}

	void MatchPatternList(out List<MatchPatternClause> clauses ) {
		clauses = new List<MatchPatternClause>(); List<PatternConstruct> pattern_list;
		Statement inner; Expression guard;
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "arm`" + ScopeId++;
		
		PatternList(out pattern_list, out guard);
		MatchArmStmt(out inner);
		clauses.Add(Statement.MakeMatchClause(pattern_list, guard, inner));
		    GoUpScope();
		 
		while (WeakSeparator(12,20,21) ) {
			Symbols.AddScope();
			GoDownScope();
			Symbols.Name = "arm`" + ScopeId++;
			
			PatternList(out pattern_list, out guard);
			MatchArmStmt(out inner);
			clauses.Add(Statement.MakeMatchClause(pattern_list, guard, inner));
			     GoUpScope();
			  
		}
	}

	void PatternList(out List<PatternConstruct> patterns, out Expression guard ) {
		patterns = new List<PatternConstruct>(); PatternConstruct tmp; guard = null; 
		Pattern(out tmp);
		patterns.Add(tmp); 
		while (la.kind == 61) {
			while (!(la.kind == 0 || la.kind == 61)) {SynErr(146); Get();}
			Get();
			Pattern(out tmp);
			patterns.Add(tmp); 
		}
		if (la.kind == 22) {
			Get();
			CondExpr(out guard);
		}
		Expect(79);
	}

	void MatchArmStmt(out Statement stmt) {
		stmt = null; BlockStatement block = null; 
		switch (la.kind) {
		case 7: {
			Block(out block);
			stmt = block; 
			break;
		}
		case 14: case 105: case 106: {
			ExprStmt(out stmt);
			break;
		}
		case 27: case 28: {
			VarDeclStmt(out stmt);
			break;
		}
		case 62: {
			ReturnStmt(out stmt);
			break;
		}
		case 63: {
			BreakStmt(out stmt);
			break;
		}
		case 65: {
			ContinueStmt(out stmt);
			break;
		}
		case 66: {
			YieldStmt(out stmt);
			break;
		}
		case 67: {
			ThrowStmt(out stmt);
			break;
		}
		default: SynErr(147); break;
		}
	}

	void Pattern(out PatternConstruct pattern) {
		pattern = null; 
		if (la.kind == 83) {
			WildcardPattern(out pattern);
		} else if (la.kind == 8) {
			TuplePattern(out pattern);
		} else if (IsIdentifierPattern()) {
			IdentifierPattern(out pattern);
		} else if (la.kind == 9 || la.kind == 14) {
			DestructuringPattern(out pattern);
		} else if (StartOf(22)) {
			RValuePattern(out pattern);
		} else SynErr(148);
	}

	void CatchClauses(out List<CatchClause> catches ) {
		catches = new List<CatchClause>(); CatchClause @catch; 
		CatchClause(out @catch);
		catches.Add(@catch); 
		while (la.kind == 81) {
			CatchClause(out @catch);
			catches.Add(@catch); 
		}
	}

	void FinallyClause(out FinallyClause @finally) {
		var start_loc = NextLocation; BlockStatement body; 
		Expect(82);
		Block(out body);
		@finally = Statement.MakeFinallyClause(body, start_loc); 
	}

	void CatchClause(out CatchClause @catch) {
		var start_loc = NextLocation; Identifier ident; BlockStatement body; 
		Expect(81);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "catch`" + ScopeId++;
		cur_modifiers = ExpressoModifiers.None;
		
		Identifier(out ident);
		if(ident.Type is PlaceholderType)
		   SemanticError(start_loc, "Error ES0010: A CatchClause identifier has to be explicitly type annotated; {0}", ident.Name);
		
		Symbols.AddSymbol(ident.Name, ident);
		
		Block(out body);
		@catch = Statement.MakeCatchClause(ident, body, start_loc);
		GoUpScope();
		
	}

	void Identifier(out Identifier ident) {
		string name; var loc = CurrentLocation; 
		Expect(14);
		name = t.val;
		if(CheckKeyword(t.val)){
		   ident = null;
		   return;
		}
		AstType type = AstType.MakePlaceholderType(CurrentLocation);
		
		if (la.kind == 43) {
			Get();
			Type(out type);
		}
		ident = AstNode.MakeIdentifier(name, type, cur_modifiers, loc); 
	}

	void PatternVarDef(out VariableInitializer init) {
		Identifier ident; Expression expr = null; 
		Identifier(out ident);
		if (la.kind == 44) {
			Get();
			PatternOrTest(out expr);
		}
		init = AstNode.MakeVariableInitializer(PatternConstruct.MakeIdentifierPattern(ident, null), expr); 
	}

	void PatternOrTest(out Expression expr) {
		Expression rhs; 
		PatternAndTest(out expr);
		if (la.kind == 86) {
			Get();
			PatternOrTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalOr, expr, rhs); 
		}
	}

	void WildcardPattern(out PatternConstruct pattern) {
		Expect(83);
		pattern = PatternConstruct.MakeWildcardPattern(); 
	}

	void TuplePattern(out PatternConstruct pattern) {
		var inners = new List<PatternConstruct>(); pattern = null; 
		Expect(8);
		if (StartOf(23)) {
			TupleElementPattern(out pattern);
			inners.Add(pattern); 
			Expect(12);
			if (StartOf(24)) {
				if (StartOf(23)) {
					TupleElementPattern(out pattern);
				} else {
					Get();
					pattern = PatternConstruct.MakeIgnoringRestPattern(CurrentLocation); 
				}
				inners.Add(pattern); 
			}
		}
		while (la.kind == 12) {
			Get();
			if (StartOf(23)) {
				TupleElementPattern(out pattern);
			} else if (la.kind == 1) {
				Get();
				pattern = PatternConstruct.MakeIgnoringRestPattern(CurrentLocation); 
			} else SynErr(149);
			inners.Add(pattern); 
		}
		Expect(10);
		pattern = PatternConstruct.MakeTuplePattern(inners); 
	}

	void IdentifierPattern(out PatternConstruct pattern) {
		PatternConstruct inner = null; string name; AstType type = new PlaceholderType(TextLocation.Empty); var loc = CurrentLocation; 
		Expect(14);
		name = t.val;
		var ident = AstNode.MakeIdentifier(name, type, cur_modifiers, loc);
		Symbols.AddSymbol(name, ident);
		
		if (la.kind == 84) {
			Get();
			Pattern(out inner);
		}
		pattern = PatternConstruct.MakeIdentifierPattern(ident, inner); 
	}

	void DestructuringPattern(out PatternConstruct pattern) {
		pattern = PatternConstruct.Null; AstType type_path;
		var patterns = new List<PatternConstruct>(); bool is_vector = false; string key = null;
		
		if (la.kind == 14) {
			TypePathExpression(out type_path);
			Expect(7);
			if (StartOf(20)) {
				if (IsStartOfKeyValuePair()) {
					Expect(14);
					key = t.val; 
					Expect(4);
				}
				Pattern(out pattern);
				if(key != null){
				   pattern = PatternConstruct.MakeKeyValuePattern(key, pattern);
				   key = null;
				}
				
				patterns.Add(pattern);
				
			}
			while (la.kind == 12) {
				Get();
				if (IsStartOfKeyValuePair()) {
					Expect(14);
					key = t.val; 
					Expect(4);
				}
				if (StartOf(20)) {
					Pattern(out pattern);
					if(key != null){
					   pattern = PatternConstruct.MakeKeyValuePattern(key, pattern);
					   key = null;
					}
					
					patterns.Add(pattern);
					
				} else if (la.kind == 1) {
					Get();
					patterns.Add(PatternConstruct.MakeIgnoringRestPattern(CurrentLocation)); 
				} else SynErr(150);
			}
			Expect(11);
			pattern = PatternConstruct.MakeDestructuringPattern(type_path, patterns); 
		} else if (la.kind == 9) {
			Get();
			if (StartOf(20)) {
				Pattern(out pattern);
				patterns.Add(pattern); 
			}
			while (NotFinalComma()) {
				ExpectWeak(12, 25);
				if (StartOf(20)) {
					Pattern(out pattern);
					patterns.Add(pattern); 
				} else if (la.kind == 1) {
					Get();
					patterns.Add(PatternConstruct.MakeIgnoringRestPattern(CurrentLocation)); 
				} else SynErr(151);
			}
			if (la.kind == 12) {
				Get();
				Expect(2);
				is_vector = true; 
			}
			Expect(3);
			pattern = PatternConstruct.MakeCollectionPattern(patterns, is_vector); 
		} else SynErr(152);
	}

	void RValuePattern(out PatternConstruct pattern) {
		Expression expr; 
		LiteralIntSeqExpr(out expr);
		pattern = PatternConstruct.MakeExpressionPattern(expr); 
	}

	void LiteralIntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null; bool upper_inclusive = false; 
		Literal(out start);
		expr = start; 
		if (la.kind == 1 || la.kind == 2) {
			RangeOperator(ref upper_inclusive);
			Literal(out end);
			if (la.kind == 4) {
				Get();
				Literal(out step);
			}
			if(step == null)
			   step = Expression.MakeConstant("int", 1, TextLocation.Empty);
			
			expr = Expression.MakeIntSeq(start, end, step, upper_inclusive);
			
		}
	}

	void TupleElementPattern(out PatternConstruct pattern) {
		pattern = PatternConstruct.Null; 
		if (la.kind == 83) {
			WildcardPattern(out pattern);
		} else if (la.kind == 27 || la.kind == 28) {
			ValueBindingPattern(out pattern);
		} else if (la.kind == 8) {
			TuplePattern(out pattern);
		} else if (IsIdentifierPattern()) {
			IdentifierPattern(out pattern);
		} else if (la.kind == 9 || la.kind == 14) {
			DestructuringPattern(out pattern);
		} else if (StartOf(19)) {
			ExpressionPattern(out pattern);
		} else SynErr(153);
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		if (la.kind == 86) {
			Get();
			OrTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalOr, expr, rhs); 
		}
	}

	void ClosureLiteral(out Expression expr) {
		var parameters = new List<ParameterDeclaration>(); BlockStatement body_block = null;
		var empty_params = new List<ParameterType>(); Expression body_expr; var start_loc = NextLocation;
		
		Expect(61);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "closure`" + ScopeId++;
		defining_closure_parameters = true;
		
		if (la.kind == 14) {
			ParamList(empty_params, ref parameters);
		}
		Expect(61);
		if(la.val != "->" && la.val != "{"){
		   Symbols.AddScope();
		   GoDownScope();
		   Symbols.Name = "block`" + ScopeId++;
		}
		defining_closure_parameters = false;
		AstType return_type = AstType.MakePlaceholderType(CurrentLocation);
		
		if (la.kind == 35) {
			Get();
			Type(out return_type);
			if(la.val != "{"){
			   Symbols.AddScope();
			   GoDownScope();
			   Symbols.Name = "block`" + ScopeId++;
			}
			
		}
		if (IsStartOfBlockScope()) {
			Block(out body_block);
		} else if (StartOf(15)) {
			CondExpr(out body_expr);
			var seq_expr = Expression.MakeSequenceExpression(body_expr);
			body_block = Statement.MakeBlock(Statement.MakeReturnStmt(seq_expr, seq_expr.StartLocation));
			
		} else SynErr(154);
		expr = Expression.MakeClosureExpression(parameters, return_type, body_block, start_loc);
		if(t.val != "}")
		   GoUpScope();
		
		GoUpScope();
		
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		Comparison(out expr);
		if (la.kind == 87) {
			Get();
			AndTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalAnd, expr, rhs); 
		}
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		IntSeqExpr(out expr);
		type = OperatorType.Equality; 
		if (StartOf(26)) {
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
		case 88: {
			Get();
			opType = OperatorType.Equality; 
			break;
		}
		case 89: {
			Get();
			opType = OperatorType.InEquality; 
			break;
		}
		case 45: {
			Get();
			opType = OperatorType.LessThan; 
			break;
		}
		case 46: {
			Get();
			opType = OperatorType.GreaterThan; 
			break;
		}
		case 90: {
			Get();
			opType = OperatorType.LessThanOrEqual; 
			break;
		}
		case 91: {
			Get();
			opType = OperatorType.GreaterThanOrEqual; 
			break;
		}
		default: SynErr(155); break;
		}
	}

	void BitOr(out Expression expr) {
		Expression rhs; 
		BitXor(out expr);
		if (la.kind == 61) {
			Get();
			BitOr(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseOr, expr, rhs); 
		}
	}

	void RangeOperator(ref bool upper_inclusive) {
		if (la.kind == 1) {
			Get();
			upper_inclusive = false; 
		} else if (la.kind == 2) {
			Get();
			upper_inclusive = true; 
		} else SynErr(156);
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		if (la.kind == 92) {
			Get();
			BitXor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ExclusiveOr, expr, rhs); 
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOp(out expr);
		if (la.kind == 47) {
			Get();
			BitAnd(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseAnd, expr, rhs); 
		}
	}

	void ShiftOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOp(out expr);
		if (la.kind == 93 || la.kind == 94) {
			ShiftOperator(out type);
			ShiftOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AddOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		if (la.kind == 95 || la.kind == 96) {
			AdditiveOperator(out type);
			AddOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void ShiftOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 93) {
			Get();
			opType = OperatorType.BitwiseShiftLeft; 
		} else if (la.kind == 94) {
			Get();
			opType = OperatorType.BitwiseShiftRight; 
		} else SynErr(157);
	}

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		PowerOp(out expr);
		if (la.kind == 97 || la.kind == 98 || la.kind == 99) {
			MultiplicativeOperator(out type);
			Term(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AdditiveOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 95) {
			Get();
			opType = OperatorType.Plus; 
		} else if (la.kind == 96) {
			Get();
			opType = OperatorType.Minus; 
		} else SynErr(158);
	}

	void PowerOp(out Expression expr) {
		Expression rhs; 
		Factor(out expr);
		if (la.kind == 100) {
			Get();
			Factor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.Power, expr, rhs); 
		}
	}

	void MultiplicativeOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 97) {
			Get();
			opType = OperatorType.Times; 
		} else if (la.kind == 98) {
			Get();
			opType = OperatorType.Divide; 
		} else if (la.kind == 99) {
			Get();
			opType = OperatorType.Modulus; 
		} else SynErr(159);
	}

	void Factor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; var start_loc = CurrentLocation; 
		if (StartOf(27)) {
			Primary(out expr);
		} else if (StartOf(28)) {
			UnaryOperator(out type);
			Factor(out factor);
			expr = Expression.MakeUnaryExpr(type, factor, start_loc); 
		} else SynErr(160);
	}

	void Primary(out Expression expr) {
		expr = null; PathExpression path; AstType type_path = null; 
		if (la.kind == 14) {
			PathExpression(out path);
			expr = path; 
			if (IsObjectCreation()) {
				type_path = ConvertPathToType(path); 
				ObjectCreation(type_path, out expr);
			}
		} else if (StartOf(29)) {
			Atom(out expr);
		} else SynErr(161);
		while (la.kind == 8 || la.kind == 9 || la.kind == 13) {
			Trailer(ref expr);
		}
		if (la.kind == 29) {
			Get();
			Type(out type_path);
			expr = Expression.MakeCastExpr(expr, type_path); 
		}
	}

	void UnaryOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 95 || la.kind == 96) {
			AdditiveOperator(out opType);
		} else if (la.kind == 101) {
			Get();
			opType = OperatorType.Not; 
		} else if (la.kind == 47) {
			Get();
			opType = OperatorType.Reference; 
		} else if (la.kind == 97) {
			Get();
			opType = OperatorType.Dereference; 
		} else SynErr(162);
	}

	void PathExpression(out PathExpression path) {
		var paths = new List<Identifier>(); var loc = CurrentLocation; 
		Expect(14);
		var name = t.val;
		if(CheckKeyword(name)){
		   path = null;
		   return;
		}
		var ident = AstNode.MakeIdentifier(name, AstType.MakePlaceholderType(new TextLocation(loc.Line, loc.Column + name.Length)), ExpressoModifiers.None, loc);
		paths.Add(ident);
		
		while (la.kind == 5) {
			Get();
			loc = CurrentLocation; 
			Expect(14);
			name = t.val;
			if(CheckKeyword(name)){
			   path = null;
			   return;
			}
			var ident2 = AstNode.MakeIdentifier(name, AstType.MakePlaceholderType(new TextLocation(loc.Line, loc.Column + name.Length)), ExpressoModifiers.None, loc);
			paths.Add(ident2);
			
		}
		path = Expression.MakePath(paths); 
	}

	void Atom(out Expression expr) {
		var exprs = new List<Expression>(); expr = null; bool seen_trailing_comma = false; var loc = CurrentLocation; 
		if (StartOf(22)) {
			Literal(out expr);
		} else if (la.kind == 8) {
			Get();
			if (la.kind == 10) {
				Get();
				expr = Expression.MakeParen(Expression.MakeSequenceExpression(null)); 
			} else if (StartOf(15)) {
				CondExpr(out expr);
				exprs.Add(expr); 
				while (NotFinalComma()) {
					ExpectWeak(12, 30);
					CondExpr(out expr);
					exprs.Add(expr); 
				}
				if (la.kind == 12) {
					Get();
					seen_trailing_comma = true; 
				}
				Expect(10);
				if(exprs.Count == 1)
				   expr = Expression.MakeParen(seen_trailing_comma ? Expression.MakeSequenceExpression(exprs[0]) : exprs[0]);
				else
				   expr = Expression.MakeParen(Expression.MakeSequenceExpression(exprs));
				
			} else SynErr(163);
		} else if (la.kind == 9) {
			Get();
			if (StartOf(31)) {
				SequenceMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 3)) {SynErr(164); Get();}
			Expect(3);
			if(expr == null){
			                     var type = CreateTypeWithArgs("array", AstType.MakePlaceholderType(loc));
			expr = Expression.MakeSequenceInitializer(type, Enumerable.Empty<Expression>());
			                 }
			
		} else if (la.kind == 7) {
			Get();
			if (StartOf(16)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 11)) {SynErr(165); Get();}
			Expect(11);
			if(expr == null){
			   var type = CreateTypeWithArgs("dictionary", AstType.MakePlaceholderType(loc), AstType.MakePlaceholderType(loc));
			   expr = Expression.MakeSequenceInitializer(type, Enumerable.Empty<Expression>());
			}
			
		} else SynErr(166);
	}

	void Trailer(ref Expression expr) {
		var args = new List<Expression>(); var loc = CurrentLocation; 
		if (la.kind == 8) {
			Get();
			if (StartOf(15)) {
				ArgList(out args);
			}
			Expect(10);
			expr = Expression.MakeCallExpr(expr, args, CurrentLocation); 
		} else if (la.kind == 9) {
			Get();
			ArgList(out args);
			Expect(3);
			expr = Expression.MakeIndexer(expr, args, CurrentLocation); 
		} else if (la.kind == 13) {
			Get();
			Expect(14);
			expr = Expression.MakeMemRef(expr, AstNode.MakeIdentifier(t.val, AstType.MakePlaceholderType(new TextLocation(loc.Line, loc.Column + t.val.Length)), ExpressoModifiers.None, loc)); 
		} else SynErr(167);
	}

	void PatternAndTest(out Expression expr) {
		Expression rhs; 
		PatternComparison(out expr);
		if (la.kind == 87) {
			Get();
			PatternAndTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalAnd, expr, rhs); 
		}
	}

	void PatternComparison(out Expression expr) {
		Expression rhs; OperatorType type = OperatorType.None; 
		PatternIntSeqExpr(out expr);
		if (StartOf(26)) {
			ComparisonOperator(out type);
			PatternComparison(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void PatternIntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null;
		bool upper_inclusive = true;
		
		PatternBitOr(out start);
		expr = start; 
		if (la.kind == 1 || la.kind == 2) {
			RangeOperator(ref upper_inclusive);
			PatternBitOr(out end);
			if (la.kind == 4) {
				Get();
				PatternBitOr(out step);
			}
			if(step == null) step = Expression.MakeConstant("int", 1, TextLocation.Empty);
			expr = Expression.MakeIntSeq(start, end, step, upper_inclusive);
			
		}
	}

	void PatternBitOr(out Expression expr) {
		Expression rhs; 
		PatternBitXor(out expr);
		if (la.kind == 61) {
			Get();
			PatternBitOr(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseOr, expr, rhs); 
		}
	}

	void PatternBitXor(out Expression expr) {
		Expression rhs; 
		PatternBitAnd(out expr);
		if (la.kind == 92) {
			Get();
			PatternBitXor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ExclusiveOr, expr, rhs); 
		}
	}

	void PatternBitAnd(out Expression expr) {
		Expression rhs; 
		PatternShiftOp(out expr);
		if (la.kind == 47) {
			Get();
			PatternBitAnd(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseAnd, expr, rhs); 
		}
	}

	void PatternShiftOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		PatternAddOp(out expr);
		if (la.kind == 93 || la.kind == 94) {
			ShiftOperator(out type);
			PatternShiftOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void PatternAddOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		PatternTerm(out expr);
		if (la.kind == 95 || la.kind == 96) {
			AdditiveOperator(out type);
			PatternAddOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void PatternTerm(out Expression expr) {
		Expression rhs; OperatorType type; 
		PatternPowerOp(out expr);
		if (la.kind == 97 || la.kind == 98 || la.kind == 99) {
			MultiplicativeOperator(out type);
			PatternTerm(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void PatternPowerOp(out Expression expr) {
		Expression rhs; 
		PatternFactor(out expr);
		if (la.kind == 100) {
			Get();
			PatternFactor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.Power, expr, rhs); 
		}
	}

	void PatternFactor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; var start_loc = CurrentLocation; 
		if (StartOf(32)) {
			PatternPrimary(out expr);
		} else if (StartOf(28)) {
			UnaryOperator(out type);
			PatternFactor(out factor);
			expr = Expression.MakeUnaryExpr(type, factor, start_loc); 
		} else SynErr(168);
	}

	void PatternPrimary(out Expression expr) {
		expr = null; PathExpression path; 
		if (la.kind == 14) {
			PathExpression(out path);
			expr = path; 
		} else if (StartOf(22)) {
			Literal(out expr);
		} else SynErr(169);
		while (la.kind == 8 || la.kind == 9 || la.kind == 13) {
			Trailer(ref expr);
		}
	}

	void SelfReferenceExpression(out Expression expr) {
		var start_loc = NextLocation; 
		Expect(105);
		expr = Expression.MakeSelfRef(start_loc);
		// Don't add self symbol because we only need one ParameterExpression instance per a type.
		//Symbols.AddSymbol(cur_class_name + "self", self_expr.SelfIdentifier);
		
	}

	void SuperReferenceExpression(out Expression expr) {
		var start_loc = NextLocation; 
		Expect(106);
		expr = Expression.MakeSuperRef(start_loc);
		// Don't add super symbol because we only need one ParameterExpression instance per a type.
		//Symbols.AddSymbol(cur_class_name + "super", super_expr.SuperIdentifier);
		
	}

	void ArgList(out List<Expression> args ) {
		args = new List<Expression>(); Expression expr; 
		CondExpr(out expr);
		args.Add(expr); 
		while (la.kind == 12) {
			Get();
			CondExpr(out expr);
			args.Add(expr); 
		}
	}

	void SequenceMaker(out Expression expr) {
		var exprs = new List<Expression>();
		expr = null; ComprehensionIter comp = null;
		string seq_type_name = "array"; var loc = CurrentLocation;
		
		if (la.kind == 2) {
			Get();
			expr = Expression.MakeSequenceInitializer(CreateTypeWithArgs("vector", AstType.MakePlaceholderType(loc)), Enumerable.Empty<Expression>(), loc, CurrentLocation); 
		} else if (StartOf(15)) {
			CondExpr(out expr);
			exprs.Add(expr); 
			if (la.kind == 3 || la.kind == 12) {
				while (NotFinalComma()) {
					ExpectWeak(12, 30);
					CondExpr(out expr);
					exprs.Add(expr); 
				}
				if (la.kind == 12) {
					Get();
					Expect(2);
					seq_type_name = "vector"; 
				}
				var type = CreateTypeWithArgs(seq_type_name, AstType.MakePlaceholderType(loc));
				expr = Expression.MakeSequenceInitializer(type, exprs, loc, CurrentLocation);
				
			} else if (la.kind == 23) {
				CompFor(out comp);
				var type = CreateTypeWithArgs("vector", AstType.MakePlaceholderType(loc));
				expr = Expression.MakeComp(expr, (ComprehensionForClause)comp, type);
				GoUpScope();
				is_first_comprehension_for_clause = true;
				
			} else SynErr(170);
		} else SynErr(171);
	}

	void DictMaker(out Expression expr) {
		Expression key, val; var list = new List<KeyValueLikeExpression>();
		      KeyValueLikeExpression pair; ComprehensionIter comp; expr = null;
		      var type = CreateTypeWithArgs("dictionary", AstType.MakePlaceholderType(CurrentLocation), AstType.MakePlaceholderType(CurrentLocation));
		   
		BitOr(out key);
		Expect(4);
		CondExpr(out val);
		pair = Expression.MakeKeyValuePair(key, val);
		list.Add(pair);
		
		if (la.kind == 11 || la.kind == 12) {
			while (WeakSeparator(12,16,21) ) {
				BitOr(out key);
				Expect(4);
				CondExpr(out val);
				pair = Expression.MakeKeyValuePair(key, val);
				list.Add(pair);
				
			}
			expr = Expression.MakeSequenceInitializer(type, list); 
		} else if (la.kind == 23) {
			CompFor(out comp);
			expr = Expression.MakeComp(pair, (ComprehensionForClause)comp, type);
			GoDownScope();
			is_first_comprehension_for_clause = true;
			
		} else SynErr(172);
	}

	void CompFor(out ComprehensionIter expr) {
		Expression rvalue = null; ComprehensionIter body = null; PatternConstruct target; 
		Expect(23);
		if(is_first_comprehension_for_clause){
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "Comprehension`" + ScopeId++;
		is_first_comprehension_for_clause = false;
		}
		
		PatternWithType(out target);
		Expect(24);
		CondExpr(out rvalue);
		if (la.kind == 22 || la.kind == 23) {
			CompIter(out body);
		}
		expr = Expression.MakeCompFor(target, rvalue, body);
		
	}

	void CompIter(out ComprehensionIter expr) {
		expr = null; 
		if (la.kind == 23) {
			CompFor(out expr);
		} else if (la.kind == 22) {
			CompIf(out expr);
		} else SynErr(173);
	}

	void CompIf(out ComprehensionIter expr) {
		Expression tmp; ComprehensionIter body = null; 
		Expect(22);
		OrTest(out tmp);
		if (la.kind == 22 || la.kind == 23) {
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
		{_T,_x,_x,_T, _x,_x,_T,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_T,_x, _x,_T,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_T,_T,_T, _T,_T,_T,_x, _x},
		{_T,_T,_x,_T, _x,_x,_T,_x, _T,_T,_x,_T, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_x,_x,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_x, _x},
		{_T,_x,_x,_T, _x,_x,_T,_T, _T,_T,_x,_T, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_T,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_T,_T,_T, _T,_T,_T,_x, _x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_x, _x}

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
			case 22: s = "keyword_if expected"; break;
			case 23: s = "keyword_for expected"; break;
			case 24: s = "keyword_in expected"; break;
			case 25: s = "keyword_while expected"; break;
			case 26: s = "keyword_match expected"; break;
			case 27: s = "keyword_let expected"; break;
			case 28: s = "keyword_var expected"; break;
			case 29: s = "keyword_as expected"; break;
			case 30: s = "\"export\" expected"; break;
			case 31: s = "\"module\" expected"; break;
			case 32: s = "\"import\" expected"; break;
			case 33: s = "\"interface\" expected"; break;
			case 34: s = "\"def\" expected"; break;
			case 35: s = "\"->\" expected"; break;
			case 36: s = "\"class\" expected"; break;
			case 37: s = "\"public\" expected"; break;
			case 38: s = "\"protected\" expected"; break;
			case 39: s = "\"private\" expected"; break;
			case 40: s = "\"static\" expected"; break;
			case 41: s = "\"mutating\" expected"; break;
			case 42: s = "\"override\" expected"; break;
			case 43: s = "\"(-\" expected"; break;
			case 44: s = "\"=\" expected"; break;
			case 45: s = "\"<\" expected"; break;
			case 46: s = "\">\" expected"; break;
			case 47: s = "\"&\" expected"; break;
			case 48: s = "\"int\" expected"; break;
			case 49: s = "\"uint\" expected"; break;
			case 50: s = "\"bool\" expected"; break;
			case 51: s = "\"float\" expected"; break;
			case 52: s = "\"double\" expected"; break;
			case 53: s = "\"bigint\" expected"; break;
			case 54: s = "\"string\" expected"; break;
			case 55: s = "\"byte\" expected"; break;
			case 56: s = "\"char\" expected"; break;
			case 57: s = "\"vector\" expected"; break;
			case 58: s = "\"dictionary\" expected"; break;
			case 59: s = "\"intseq\" expected"; break;
			case 60: s = "\"void\" expected"; break;
			case 61: s = "\"|\" expected"; break;
			case 62: s = "\"return\" expected"; break;
			case 63: s = "\"break\" expected"; break;
			case 64: s = "\"upto\" expected"; break;
			case 65: s = "\"continue\" expected"; break;
			case 66: s = "\"yield\" expected"; break;
			case 67: s = "\"throw\" expected"; break;
			case 68: s = "\"+=\" expected"; break;
			case 69: s = "\"-=\" expected"; break;
			case 70: s = "\"*=\" expected"; break;
			case 71: s = "\"/=\" expected"; break;
			case 72: s = "\"**=\" expected"; break;
			case 73: s = "\"%=\" expected"; break;
			case 74: s = "\"&=\" expected"; break;
			case 75: s = "\"|=\" expected"; break;
			case 76: s = "\"<<=\" expected"; break;
			case 77: s = "\">>=\" expected"; break;
			case 78: s = "\"else\" expected"; break;
			case 79: s = "\"=>\" expected"; break;
			case 80: s = "\"try\" expected"; break;
			case 81: s = "\"catch\" expected"; break;
			case 82: s = "\"finally\" expected"; break;
			case 83: s = "\"_\" expected"; break;
			case 84: s = "\"@\" expected"; break;
			case 85: s = "\"?\" expected"; break;
			case 86: s = "\"||\" expected"; break;
			case 87: s = "\"&&\" expected"; break;
			case 88: s = "\"==\" expected"; break;
			case 89: s = "\"!=\" expected"; break;
			case 90: s = "\"<=\" expected"; break;
			case 91: s = "\">=\" expected"; break;
			case 92: s = "\"^\" expected"; break;
			case 93: s = "\"<<\" expected"; break;
			case 94: s = "\">>\" expected"; break;
			case 95: s = "\"+\" expected"; break;
			case 96: s = "\"-\" expected"; break;
			case 97: s = "\"*\" expected"; break;
			case 98: s = "\"/\" expected"; break;
			case 99: s = "\"%\" expected"; break;
			case 100: s = "\"**\" expected"; break;
			case 101: s = "\"!\" expected"; break;
			case 102: s = "\"true\" expected"; break;
			case 103: s = "\"false\" expected"; break;
			case 104: s = "\"null\" expected"; break;
			case 105: s = "\"self\" expected"; break;
			case 106: s = "\"super\" expected"; break;
			case 107: s = "??? expected"; break;
			case 108: s = "invalid ModuleBody"; break;
			case 109: s = "invalid ModuleBody"; break;
			case 110: s = "this symbol not expected in ModuleNameDefinition"; break;
			case 111: s = "this symbol not expected in FuncDecl"; break;
			case 112: s = "invalid FieldDecl"; break;
			case 113: s = "this symbol not expected in FieldDecl"; break;
			case 114: s = "this symbol not expected in ClassDecl"; break;
			case 115: s = "invalid ClassDecl"; break;
			case 116: s = "this symbol not expected in ClassDecl"; break;
			case 117: s = "this symbol not expected in InterfaceDecl"; break;
			case 118: s = "this symbol not expected in ImportDecl"; break;
			case 119: s = "this symbol not expected in ImportDecl"; break;
			case 120: s = "invalid Type"; break;
			case 121: s = "invalid Type"; break;
			case 122: s = "this symbol not expected in MethodSignature"; break;
			case 123: s = "invalid Modifiers"; break;
			case 124: s = "this symbol not expected in Block"; break;
			case 125: s = "invalid Literal"; break;
			case 126: s = "invalid Stmt"; break;
			case 127: s = "invalid SimpleStmt"; break;
			case 128: s = "invalid CompoundStmt"; break;
			case 129: s = "this symbol not expected in ExprStmt"; break;
			case 130: s = "invalid VarDeclStmt"; break;
			case 131: s = "this symbol not expected in VarDeclStmt"; break;
			case 132: s = "this symbol not expected in ReturnStmt"; break;
			case 133: s = "this symbol not expected in BreakStmt"; break;
			case 134: s = "this symbol not expected in ContinueStmt"; break;
			case 135: s = "this symbol not expected in YieldStmt"; break;
			case 136: s = "this symbol not expected in ThrowStmt"; break;
			case 137: s = "this symbol not expected in EmptyStmt"; break;
			case 138: s = "invalid CondExpr"; break;
			case 139: s = "this symbol not expected in ObjectCreation"; break;
			case 140: s = "invalid AugmentedAssignOperators"; break;
			case 141: s = "invalid LhsPrimary"; break;
			case 142: s = "invalid IfStmt"; break;
			case 143: s = "invalid ForStmt"; break;
			case 144: s = "invalid ValueBindingPattern"; break;
			case 145: s = "invalid PatternWithType"; break;
			case 146: s = "this symbol not expected in PatternList"; break;
			case 147: s = "invalid MatchArmStmt"; break;
			case 148: s = "invalid Pattern"; break;
			case 149: s = "invalid TuplePattern"; break;
			case 150: s = "invalid DestructuringPattern"; break;
			case 151: s = "invalid DestructuringPattern"; break;
			case 152: s = "invalid DestructuringPattern"; break;
			case 153: s = "invalid TupleElementPattern"; break;
			case 154: s = "invalid ClosureLiteral"; break;
			case 155: s = "invalid ComparisonOperator"; break;
			case 156: s = "invalid RangeOperator"; break;
			case 157: s = "invalid ShiftOperator"; break;
			case 158: s = "invalid AdditiveOperator"; break;
			case 159: s = "invalid MultiplicativeOperator"; break;
			case 160: s = "invalid Factor"; break;
			case 161: s = "invalid Primary"; break;
			case 162: s = "invalid UnaryOperator"; break;
			case 163: s = "invalid Atom"; break;
			case 164: s = "this symbol not expected in Atom"; break;
			case 165: s = "this symbol not expected in Atom"; break;
			case 166: s = "invalid Atom"; break;
			case 167: s = "invalid Trailer"; break;
			case 168: s = "invalid PatternFactor"; break;
			case 169: s = "invalid PatternPrimary"; break;
			case 170: s = "invalid SequenceMaker"; break;
			case 171: s = "invalid SequenceMaker"; break;
			case 172: s = "invalid DictMaker"; break;
			case 173: s = "invalid CompIter"; break;

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