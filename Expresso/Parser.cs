using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
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
	public const int _colon = 3;
	public const int _double_colon = 4;
	public const int _semicolon = 5;
	public const int _lcurly = 6;
	public const int _lparen = 7;
	public const int _langle_bracket = 8;
	public const int _lbracket = 9;
	public const int _rparen = 10;
	public const int _rcurly = 11;
	public const int _rangle_bracket = 12;
	public const int _rbracket = 13;
	public const int _comma = 14;
	public const int _dot = 15;
	public const int _thick_arrow = 16;
	public const int _equal = 17;
	public const int _ident = 18;
	public const int _integer = 19;
	public const int _float = 20;
	public const int _hex_digit = 21;
	public const int _unicode_escape = 22;
	public const int _character_literal = 23;
	public const int _string_literal = 24;
	public const int _raw_string_literal = 25;
	public const int _keyword_for = 26;
	public const int maxT = 117;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

static uint ScopeId = 1;
    string cur_class_name;
    static Regex UnicodeEscapeFinder = new Regex(@"\\[uU]([\dA-Fa-f]{4}|[\dA-Fa-f]{6})", RegexOptions.Compiled);
    ExpressoModifiers cur_modifiers;
    bool is_first_comprehension_for_clause = true, defining_closure_parameters = false;
    TextLocation parent_location;
    List<ParameterType> type_params = new List<ParameterType>();
    internal SymbolTable Symbols{get; set;}
    /// <summary>
    /// This flag determines whether we are doing post-parse processing including name binding,
    /// type validity check, type inference and flow analisys.
    /// </summary>
    public bool DoPostParseProcessing{get; set;}
	public ExpressoAst TopmostAst{get; private set;}	//the top-level AST the parser is parsing
    public TextLocation CurrentLocation{
        get{
            if(parent_location == default)
                return new TextLocation(parent_location.Line + t.line, parent_location.Column + t.col);
            else
                return new TextLocation(parent_location.Line + t.line - 1, parent_location.Column + t.col - 1);
        }
    }
    public TextLocation CurrentEndLocation{
        get{
            if(parent_location == default)
                return new TextLocation(parent_location.Line + t.line, parent_location.Column + t.col + t.val.Length);
            else
                return new TextLocation(parent_location.Line + t.line - 1, parent_location.Column + t.col + t.val.Length - 1);
        }
    }
    public TextLocation NextLocation{
        get{
            if(parent_location == default)
                return new TextLocation(parent_location.Line + la.line, parent_location.Column + la.col);
            else
                return new TextLocation(parent_location.Line + la.line - 1, parent_location.Column + la.col - 1);
        }
    }
    internal List<Parser> InnerParsers{get;} = new List<Parser>();

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
	Parser(TextLocation parentLocation)
	{
        DoPostParseProcessing = false;
        ExpressoCompilerHelpers.Prepare();
        Symbols = SymbolTable.Create();
        parent_location = parentLocation;
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
			throw new InvalidOperationException("Unknown object type");
		}
		
		return result;
	}

    AstType CreatePrimitiveType(string keyword, TextLocation loc, bool isReference)
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
            if(uint.TryParse(numerics.Substring(0, numerics.Length - 1), out var u))
                obj = u;
            else
                throw new InvalidOperationException("Unreachable!");
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
            if(float.TryParse(numerics.Substring(0, numerics.Length - 1), out var f))
                obj = f;
            else
                throw new InvalidOperationException("Unreachable!");
            break; 
        }

        default:
        {
            if(int.TryParse(numerics, out var i)){
                obj = i;
                type_name = "int";
            }else if(double.TryParse(numerics, out var d)){
                obj = d;
                type_name = "double";
            }else{
                throw new InvalidOperationException("Unreachable!");
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
                type = AstType.MakeSimpleType(item.Name, item.StartLocation);
            else
                type = AstType.MakeMemberType(type, AstType.MakeSimpleType(item.Name, item.StartLocation), item.EndLocation);
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
	
	bool IsSequenceInitializer()
	{
		var x = la;
		if(x.kind != _comma)
            return true;
		
		while(x.kind != 0 && x.kind != _comma && x.kind != _keyword_for)
            x = scanner.Peek();

        scanner.ResetPeek();
        return x.kind != _keyword_for;
	}

    bool IsIdentifierPattern()
    {
        var t = la;
        var x = scanner.Peek();
        scanner.ResetPeek();
        return t.kind == _ident && x.kind != _double_colon && x.kind != _lcurly;
    }

    bool IsDestructuringPattern()
    {
        var tt = la;
        var x = scanner.Peek();
        scanner.ResetPeek();
        return tt.kind == _ident && (x.kind == _double_colon || x.kind == _lcurly);
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
        if(t.kind != _lcurly)
            return false;

        var key = scanner.Peek();
        var tt = scanner.Peek();
        scanner.ResetPeek();
        return key.kind == _rcurly || (key.kind == _integer || key.kind == _ident) && tt.kind == _colon;
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

    bool IsStartOfImportPath()
    {
        var token = scanner.Peek();
        scanner.ResetPeek();
        return token.val != "::" && token.val != ".";
    }

    bool IsStartOfTypeNameOfImportPath()
    {
        var token = scanner.Peek();
        scanner.ResetPeek();
        return token.val != ".";
    }

    bool IsIntSeqColon()
    {
        var tt = la;
        var x = scanner.Peek();
        bool another_colon_found = false;
        while(x.kind != _semicolon){
            if(x.kind == _colon)
                another_colon_found = true;

            x = scanner.Peek();
        }
        scanner.ResetPeek();
        return tt.kind == _colon && !another_colon_found;
    }

    bool IsGenericTypeSignature()
    {
        var x = scanner.Peek();
        bool closing_bracket_found = false;
        while(x.kind != _semicolon){
            if(x.kind == _rangle_bracket)
                closing_bracket_found = true;

            x = scanner.Peek();
        }
        scanner.ResetPeek();
        return la.kind == _langle_bracket && closing_bracket_found;
    }

    bool IsArrayTypeSignature()
    {
        var tt = la;
        var x = scanner.Peek();
        scanner.ResetPeek();
        return tt.kind == _lbracket && x.kind == _rbracket;
    }

    bool IsTupleStyleEnumMember()
    {
        var x = scanner.Peek();
        scanner.ResetPeek();
        return x.kind == _lparen;
    }

    bool IsKeyIdentifier()
    {
        var x = scanner.Peek();
        scanner.ResetPeek();
        return x.kind == _colon;
    }

    bool DistinguishBetweenDestructuringPatternAndEnumMember()
    {
        var tt = la;
        var x = scanner.Peek();
        while(x.kind == _ident || x.kind == _double_colon)
            x = scanner.Peek();

        scanner.ResetPeek();
        return tt.kind == _ident && x.kind == _lcurly;
    }

    bool IsTypeParameter()
    {
        var tt = la;
        var x = scanner.Peek();
        scanner.ResetPeek();
        return tt.kind == _ident && x.kind == _equal; 
    }

    bool FollowsObjectCreation()
    {
        var tt = la;
        if(tt.kind != _langle_bracket)
            return false;

        var x = scanner.Peek();
        while(x.kind != _rangle_bracket && x.kind != _lcurly && x.kind != _semicolon)
            x = scanner.Peek();

        scanner.ResetPeek();
        return x.kind == _rangle_bracket;
    }

    bool IsTrailer()
    {
        var tt = la;
        if(tt.kind == _dot || tt.kind == _lbracket || tt.kind == _lparen)
            return true;

        if(tt.kind != _langle_bracket)
            return false;

        var x = scanner.Peek();
        while(x.kind != _rangle_bracket && x.kind != _semicolon && x.kind != _lcurly)
            x = scanner.Peek();

        scanner.ResetPeek();
        return x.kind == _rangle_bracket;
    }

    /*bool IsLValueList()
    {
        var tt = la;
        if(tt.kind == _semicolon || tt.kind == _equal)
            return false;

        var x = scanner.Peek();
        while(!x.val.Contains("=") || x.kind != _thick_arrow || x.kind != _semicolon)
            x = scanner.Peek();

        return la.kind == _comma && x.val.Contains("=");
    }

    bool IsRValueList()
    {
        var tt = la;
        return tt.kind == _comma;
    }*/

    bool CheckKeyword(string name)
    {
        if(KnownTypeReference.Keywords.Contains(name)){
            SemanticError("ES0009", "{0} is reserverd for a keyword.", name);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Reports a semantic error message.
    /// It is intended to be used inside the Parser class.
    /// </summary>
    /// <param name="errorCode">The error code. It must contain the string "ES" at the head.</param>
    /// <param name="format">The format string according to which args will be formatted.</param>
    /// <param name="args">Values that are formatted into `format`.</param>
	void SemanticError(string errorCode, string format, params object[] args)
	{
		//Convenient method for printing a semantic error with a format string
        //var body = string.Format("{0} -- {1}", CurrentLocation, string.Format(format, args));
        var node = AstType.MakeSimpleType("", null, CurrentLocation);
        throw new ParserException(format, errorCode, node, args);
		//errors.SemErr(string.Format("Error {0}: {1}", errorCode, body));
	}

    /// <summary>
    /// Reports a semantic error message.
    /// It is intended to be used inside the Parser class.
    /// </summary>
    /// <param name="loc">The location at which the error occurred.</param>
    /// <param name="errorCode">The error code. It must contain the string "ES" at the head.</param>
    /// <param name="format">The format string according to which args will be formatted.</param>
    /// <param name="args">Values that are formatted into `format`.</param>
    void SemanticError(TextLocation loc, string errorCode, string format, params object[] args)
    {
        //var body = string.Format("{0} -- {1}", loc, string.Format(format, args));
        var node = AstType.MakeSimpleType("", null, loc);
        throw new ParserException(format, errorCode, node, args);
        //errors.SemErr(string.Format("Error {0}: {1}", errorCode, body));
    }

    /// <summary>
    /// Reports a semantic error message with a range.
    /// It is intended to be used inside the Parser class.
    /// </summary>
    /// <param name="start">The location which is the start of the error range.</param>
    /// <param name="end">The location which is the end of the error range.</param>
    /// <param name="errorCode">The error code. It must contain the string "ES" at the head.</param>
    /// <param name="format">The format string according to which `args` will be formatted.</param>
    /// <param name="args">Values that are formatted into `format`</param>
    void SemanticError(TextLocation start, TextLocation end, string errorCode, string format, params object[] args)
    {
        //var body = string.Format("{0} ~ {1} -- {2}", start, end, string.Format(format, args));
        var start_node = AstType.MakeSimpleType("", null, start);
        var end_node = AstType.MakeSimpleType("", null, end);
        throw new ParserException(format, errorCode, start_node, end_node, args);
        //errors.SemErr(string.Format("Error {0}: {1}", errorCode, body));
        //ExpressoCompilerHelpers.DisplayHelp(new ParserException(null, errorCode, null));
    }

    /// <summary>
    /// Reports a warning message.
    /// It is intended to be used from outside the Parser class.
    /// </summary>
    /// <param name="format">The format string according to which `args` will be formatted.</param>
    /// <param name="errorCode">The error code. It must contain the string "ES" at the head.</param>
    /// <param name="node">The node on which the warning occurred.</param>
    /// <param name="args">Values that are formatted into `format`</param>
    public void ReportWarning(string format, string errorCode, AstNode node, params object[] args)
    {
#if DEBUG
        throw new ParserException(format, errorCode, node, args);
#else
        var msg = string.Format("{0} -- {1}", node.StartLocation, string.Format(format, args));
        errors.Warning(string.Format("Warning {0}: {1}", errorCode, msg));
        ExpressoCompilerHelpers.DisplayHelp(new ParserException(null, errorCode, node));
#endif
    }

    /// <summary>
    /// Reports a semantic error message.
    /// It is intended to be used from outside the Parser class.
    /// </summary>
    /// <param name="format">The format string according to which `args` will be formatted.</param>
    /// <param name="errorCode">The error code. It must contain the string "ES" at the head.</param>
    /// <param name="node">The node on which the error occurred.</param>
    /// <param name="args">Values that are formatted into `format`.</param>
    public void ReportSemanticError(string format, string errorCode, AstNode node, params object[] args)
    {
#if DEBUG
        throw new ParserException(format, errorCode, node, args);
#else
        var msg = string.Format("{0} -- {1}", node.StartLocation, string.Format(format, args));
        errors.SemErr(string.Format("Error {0}: {1}", errorCode, msg));
        ExpressoCompilerHelpers.DisplayHelp(new ParserException(null, errorCode, node));
#endif
    }

    /// <summary>
    /// Reports a semantic error message with a range information.
    /// It is intended to be used from outside the Parser class.
    /// </summary>
    /// <param name="format">The format string according to which `args` will be formatted.</param>
    /// <param name="errorCode">The error code. It must contain the string "ES" at the head.</param>
    /// <param name="start">The node which is the start of the error range.</param>
    /// <param name="end">The node which is the end of the error range.</param>
    /// <param name="args">Values that are formatted into `format`.</param>
    public void ReportSemanticErrorRegional(string format, string errorCode, AstNode start, AstNode end, params object[] args)
    {
#if DEBUG
        throw new ParserException(format, errorCode, start, end, args);
#else
        var real_message = string.Format("{0} ~ {1} -- {2}", start.StartLocation, end.EndLocation, string.Format(format, args));
        errors.SemErr(string.Format("Error {0}: {1}", errorCode, real_message));
        ExpressoCompilerHelpers.DisplayHelp(new ParserException(null, errorCode, start, end));
#endif
    }
	
/*--------------------------------------------------------------------------*/


	public Parser(Scanner scanner, TextLocation parentLocation = default) : this(parentLocation) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(NextLocation.Line, NextLocation.Column, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(CurrentLocation.Line, CurrentLocation.Column, msg);
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
		#if !DEBUG
		if(errors.count > 0)
		   throw new FatalError("Invalid syntax found!");
		#endif
		
		if(DoPostParseProcessing){
		   PreProcessor.PerformPreProcess(module_decl, this);
		   ExpressoNameBinder.BindAst(module_decl, this); //Here's the start of post-parse processing
		}
		}
		catch(ParserException e){
		errors.SemErr(e.ToString());
		ExpressoCompilerHelpers.DisplayHelp(e);
		#if !NETCOREAPP2_0
		Console.WriteLine(e.StackTrace);
		#endif
		throw e;
		}
		catch(FatalError ex){
		// Do nothing with a FatalError
		// It only signals that the program should immediately exit
		throw ex;
		}
		catch(StackOverflowException e){
		Console.Error.WriteLine(e.Message);
		}
		
		TopmostAst = module_decl;	//Currently there is not so much code out there, though...
		
	}

	void ModuleBody(out ExpressoAst ast) {
		var decls = new List<EntityDeclaration>();
		string module_name; List<AttributeSection> attributes; AttributeSection item_attribute = null;
		List<ImportDeclaration> prog_defs = null; EntityDeclaration decl = null;
		
		ModuleNameDefinition(out module_name, out attributes);
		#if !NETCOREAPP2_0
		Console.WriteLine("Parsing {0}...", module_name);
		#endif
		
		if (la.kind == 37) {
			ProgramDefinition(out prog_defs);
		}
		if (la.kind == 36) {
			AttributeSection(out item_attribute);
		}
		if (la.kind == 27) {
			Get();
			cur_modifiers = ExpressoModifiers.Export; 
		}
		if (la.kind == 41) {
			FuncDecl(out decl, cur_modifiers, item_attribute);
		} else if (la.kind == 50 || la.kind == 51) {
			FieldDecl(out decl, item_attribute);
		} else if (la.kind == 30) {
			ClassDecl(out decl, cur_modifiers, item_attribute);
		} else if (la.kind == 40) {
			InterfaceDecl(out decl, cur_modifiers, item_attribute);
		} else if (la.kind == 31) {
			EnumDecl(out decl, cur_modifiers, item_attribute);
		} else SynErr(118);
		decls.Add(decl);
		cur_modifiers = ExpressoModifiers.None;
		
		while (StartOf(1)) {
			item_attribute = null; 
			if (la.kind == 36) {
				AttributeSection(out item_attribute);
			}
			if (la.kind == 27) {
				Get();
				cur_modifiers = ExpressoModifiers.Export; 
			}
			if (la.kind == 41) {
				FuncDecl(out decl, cur_modifiers, item_attribute);
			} else if (la.kind == 50 || la.kind == 51) {
				FieldDecl(out decl, item_attribute);
			} else if (la.kind == 30) {
				ClassDecl(out decl, cur_modifiers, item_attribute);
			} else if (la.kind == 40) {
				InterfaceDecl(out decl, cur_modifiers, item_attribute);
			} else if (la.kind == 31) {
				EnumDecl(out decl, cur_modifiers, item_attribute);
			} else SynErr(119);
			decls.Add(decl);
			cur_modifiers = ExpressoModifiers.None;
			
		}
		ast = AstNode.MakeModuleDef(module_name, decls, prog_defs, attributes); 
	}

	void ModuleNameDefinition(out string moduleName, out List<AttributeSection> attributes ) {
		attributes = new List<AttributeSection>(); AttributeSection attribute = null; 
		while (la.kind == 36) {
			AttributeSection(out attribute);
			attributes.Add(attribute); 
		}
		Expect(29);
		Expect(18);
		moduleName = t.val; 
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(120); Get();}
		Expect(5);
	}

	void ProgramDefinition(out List<ImportDeclaration> imports ) {
		imports = new List<ImportDeclaration>();
		ImportDeclaration tmp;
		
		ImportDecl(out tmp);
		imports.Add(tmp); 
		while (la.kind == 37) {
			ImportDecl(out tmp);
			imports.Add(tmp); 
		}
	}

	void AttributeSection(out AttributeSection section ) {
		var attributes = new List<ObjectCreationExpression>(); ObjectCreationExpression attribute = null;
		string target = null; var start_loc = NextLocation;
		
		Expect(36);
		if (StartOf(2)) {
			AttributeTarget(out target);
			Expect(3);
		}
		Attribute(out attribute);
		attributes.Add(attribute); 
		while (la.kind == 14) {
			Get();
			Attribute(out attribute);
			attributes.Add(attribute); 
		}
		Expect(13);
		section = AstNode.MakeAttributeSection(target, attributes, start_loc, CurrentEndLocation); 
	}

	void FuncDecl(out EntityDeclaration decl, ExpressoModifiers modifiers, AttributeSection attribute) {
		Identifier ident = null;
		string name; AstType type = null; BlockStatement block;
		var type_params = new List<ParameterType>(this.type_params);
		var @params = new List<ParameterDeclaration>();
		var start_loc = NextLocation;
		var type_constraints = new List<TypeConstraint>();
		var replacer = new ParameterTypeReplacer(type_params);
		
		while (!(la.kind == 0 || la.kind == 41)) {SynErr(121); Get();}
		Expect(41);
		Symbols.AddScope();
		var ident_start_loc = NextLocation;
		
		Expect(18);
		name = t.val;
		if(!CheckKeyword(name)){
		   ident = AstNode.MakeIdentifier(name, AstType.MakePlaceholderType(CurrentLocation), modifiers, ident_start_loc); 
		   Symbols.AddSymbol(name, ident);
		}else{
		   // The name is unsuitable for a method or a function name.
		   // Leave the parser to recover its state.
		}
		
		if (la.kind == 8) {
			GenericTypeParameters(ref type_params);
		}
		Expect(7);
		GoDownScope();
		Symbols.Name = "func " + name + "`" + ScopeId++;
		
		if (la.kind == 18 || la.kind == 36) {
			ParamList(type_params, ref @params);
		}
		Expect(10);
		if (la.kind == 42) {
			Get();
			Type(out type);
		}
		if(type == null)
		   type = new PlaceholderType(CurrentLocation);
		
		if (la.kind == 52) {
			TypeConstraints(ref type_constraints);
		}
		if(!type_constraints.Any()){
		   type_constraints = type_params.Where(p => !this.type_params.Any(tp => tp.Name == p.Name))
		                                 .Select(p => AstNode.MakeTypeConstraint((ParameterType)p.Clone(), null)).ToList();
		}
		
		Block(out block);
		decl = EntityDeclaration.MakeFunc(ident, @params, block, type, modifiers, type_constraints, attribute, start_loc);
		decl.ReturnType.AcceptWalker(replacer);
		GoUpScope();
		
	}

	void FieldDecl(out EntityDeclaration field, AttributeSection attribute) {
		Expression rhs; PatternWithType typed_pattern; var start_loc = NextLocation;
		var patterns = new List<PatternWithType>(); var exprs = new List<Expression>();
		
		while (!(la.kind == 0 || la.kind == 50 || la.kind == 51)) {SynErr(122); Get();}
		if (la.kind == 50) {
			Get();
			cur_modifiers |= ExpressoModifiers.Immutable; 
		} else if (la.kind == 51) {
			Get();
		} else SynErr(123);
		VarDef(out typed_pattern, out rhs);
		if(!(typed_pattern.Pattern is IdentifierPattern))
		  SemanticError("ES0021", "A field can only contain an identifier pattern; actual: `{0}`", typed_pattern.Pattern);
		
		patterns.Add(typed_pattern);
		exprs.Add(rhs);
		
		while (la.kind == 14) {
			Get();
			VarDef(out typed_pattern, out rhs);
			if(!(typed_pattern.Pattern is IdentifierPattern))
			  SemanticError("ES0021", "A field can only contain an indentifier pattern; actual: `{0}`", typed_pattern.Pattern);
			
			patterns.Add(typed_pattern);
			exprs.Add(rhs);
			
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(124); Get();}
		Expect(5);
		field = EntityDeclaration.MakeField(patterns, exprs, cur_modifiers, attribute, start_loc, CurrentEndLocation);
		                  cur_modifiers = ExpressoModifiers.None; 
		               
	}

	void ClassDecl(out EntityDeclaration decl, ExpressoModifiers modifiers, AttributeSection attribute) {
		EntityDeclaration entity = null; var decls = new List<EntityDeclaration>(); AstType type_path;
		string name; var bases = new List<AstType>(); var start_loc = NextLocation;
		Identifier ident = null; AttributeSection item_attribute = null; var type_constraints = new List<TypeConstraint>();
		
		while (!(la.kind == 0 || la.kind == 30)) {SynErr(125); Get();}
		Expect(30);
		Symbols.AddScope(ClassType.Class);
		var ident_start_loc = NextLocation;
		
		Expect(18);
		name = t.val;
		                    if(!CheckKeyword(name)){
		                        ident = AstNode.MakeIdentifier(name, modifiers, ident_start_loc);
		                        Symbols.AddTypeSymbol(name, ident);
		                        cur_class_name = name;
		                    }else{
		                        // Failed to parse an identifier.
		                        // Leave the parser to recover its state.
		                    }
		                 
		if (la.kind == 8) {
			GenericTypeParameters(ref type_params);
		}
		if (la.kind == 3) {
			Get();
			Type(out type_path);
			bases.Add(type_path); 
			while (la.kind == 14) {
				Get();
				Type(out type_path);
				bases.Add(type_path); 
			}
		}
		if (la.kind == 52) {
			TypeConstraints(ref type_constraints);
		}
		if(!type_constraints.Any()){
		   type_constraints = type_params.Select(p => AstNode.MakeTypeConstraint((ParameterType)p.Clone(), null)).ToList();
		}
		
		Expect(6);
		GoDownScope();
		Symbols.Name = "type_" + name + "`" + ScopeId++;
		Symbols.AddTypeParameters(type_params);
		
		while (StartOf(3)) {
			cur_modifiers = ExpressoModifiers.Private; 
			item_attribute = null;
			
			if (la.kind == 36) {
				AttributeSection(out item_attribute);
			}
			while (StartOf(4)) {
				Modifiers();
			}
			if (la.kind == 41) {
				FuncDecl(out entity, cur_modifiers, item_attribute);
				decls.Add(entity); 
			} else if (la.kind == 50 || la.kind == 51) {
				FieldDecl(out entity, item_attribute);
				decls.Add(entity); 
			} else if (la.kind == 30) {
				ClassDecl(out entity, cur_modifiers, item_attribute);
				decls.Add(entity); 
			} else if (la.kind == 40) {
				InterfaceDecl(out entity, cur_modifiers, item_attribute);
				decls.Add(entity); 
			} else SynErr(126);
		}
		while (!(la.kind == 0 || la.kind == 11)) {SynErr(127); Get();}
		Expect(11);
		decl = EntityDeclaration.MakeClassDecl(ident, bases, decls, modifiers, type_constraints, attribute, start_loc, CurrentEndLocation);
		GoUpScope();
		cur_modifiers = ExpressoModifiers.None;
		type_params.Clear();
		
	}

	void InterfaceDecl(out EntityDeclaration decl, ExpressoModifiers modifiers, AttributeSection attribute) {
		string name = null; Identifier ident = null; AttributeSection item_attribute = null;
		AstType type_path = null;
		EntityDeclaration method = null;
		var bases = new List<AstType>();
		var decls = new List<EntityDeclaration>();
		var start_loc = NextLocation;
		var type_constraints = new List<TypeConstraint>();
		
		while (!(la.kind == 0 || la.kind == 40)) {SynErr(128); Get();}
		Expect(40);
		Symbols.AddScope(ClassType.Interface);
		var ident_start_loc = NextLocation;
		
		Expect(18);
		name = t.val;
		if(!CheckKeyword(name)){
		   ident = AstNode.MakeIdentifier(name, modifiers, ident_start_loc);
		   Symbols.AddTypeSymbol(name, ident);
		}
		
		if (la.kind == 8) {
			GenericTypeParameters(ref type_params);
		}
		if (la.kind == 3) {
			Get();
			Type(out type_path);
			bases.Add(type_path); 
			while (la.kind == 14) {
				Get();
				Type(out type_path);
				bases.Add(type_path); 
			}
		}
		if (la.kind == 52) {
			TypeConstraints(ref type_constraints);
		}
		if(!type_constraints.Any()){
		   type_constraints = type_params.Select(p => AstNode.MakeTypeConstraint((ParameterType)p.Clone(), null)).ToList();
		}
		
		Expect(6);
		GoDownScope();
		Symbols.Name = "type_" + name + "`" + ScopeId++;
		Symbols.AddTypeParameters(type_params);
		
		while (la.kind == 36 || la.kind == 41) {
			if (la.kind == 36) {
				AttributeSection(out item_attribute);
			}
			MethodSignature(out method, item_attribute);
			decls.Add(method); 
		}
		Expect(11);
		decl = EntityDeclaration.MakeInterfaceDecl(ident, bases, decls, modifiers, type_constraints, attribute, start_loc, CurrentEndLocation);
		GoUpScope();
		type_params.Clear();
		
	}

	void EnumDecl(out EntityDeclaration decl, ExpressoModifiers modifiers, AttributeSection attribute) {
		EntityDeclaration entity = null; var decls = new List<EntityDeclaration>(); string name;
		var start_loc = NextLocation; Identifier ident = null; AttributeSection item_attribute = null;
		int raw_value = 0; bool has_value_identifier_added = false; var type_constraints = new List<TypeConstraint>();
		
		while (!(la.kind == 0 || la.kind == 31)) {SynErr(129); Get();}
		Expect(31);
		Symbols.AddScope(ClassType.Enum);
		var ident_start_loc = NextLocation;
		
		Expect(18);
		name = t.val;
		if(!CheckKeyword(name)){
		   ident = AstNode.MakeIdentifier(name, modifiers, ident_start_loc);
		   Symbols.AddTypeSymbol(name, ident);
		   cur_class_name = name;
		}
		
		if (la.kind == 8) {
			GenericTypeParameters(ref type_params);
		}
		if (la.kind == 52) {
			TypeConstraints(ref type_constraints);
		}
		if(!type_constraints.Any()){
		   type_constraints = type_params.Select(p => AstNode.MakeTypeConstraint((ParameterType)p.Clone(), null)).ToList();
		}
		
		Expect(6);
		GoDownScope();
		Symbols.Name = "type_" + name + "`" + ScopeId++;
		Symbols.AddTypeParameters(type_params);
		
		while (StartOf(5)) {
			cur_modifiers = ExpressoModifiers.Private;
			item_attribute = null;
			
			if (la.kind == 36) {
				AttributeSection(out item_attribute);
			}
			while (StartOf(4)) {
				Modifiers();
			}
			if (la.kind == 41) {
				FuncDecl(out entity, cur_modifiers, item_attribute);
				decls.Add(entity); 
			} else if (IsTupleStyleEnumMember()) {
				TupleStyleEnumMember(out entity, item_attribute);
				decls.Add(entity); 
			} else if (la.kind == 18) {
				RawValueStyleEnumMember(out entity, item_attribute, ref raw_value);
				decls.Add(entity);
				if(!has_value_identifier_added){
				   var ident2 = AstNode.MakeIdentifier(Utilities.RawValueEnumValueFieldName, ExpressoModifiers.Public);
				   Symbols.AddSymbol(Utilities.RawValueEnumValueFieldName, ident2);
				   has_value_identifier_added = true;
				}
				
			} else SynErr(130);
			if (la.kind == 14) {
				Get();
			}
		}
		while (!(la.kind == 0 || la.kind == 11)) {SynErr(131); Get();}
		Expect(11);
		decl = EntityDeclaration.MakeEnumDecl(ident, decls, modifiers, type_constraints, attribute, start_loc, CurrentEndLocation);
		GoUpScope();
		cur_modifiers = ExpressoModifiers.None;
		
	}

	void AttributeTarget(out string target) {
		switch (la.kind) {
		case 28: {
			Get();
			break;
		}
		case 29: {
			Get();
			break;
		}
		case 30: {
			Get();
			break;
		}
		case 31: {
			Get();
			break;
		}
		case 32: {
			Get();
			break;
		}
		case 33: {
			Get();
			break;
		}
		case 34: {
			Get();
			break;
		}
		case 35: {
			Get();
			break;
		}
		default: SynErr(132); break;
		}
		target = t.val; 
	}

	void Attribute(out ObjectCreationExpression creation) {
		creation = null; Expression expr = null; string name = null; AstType type_path = null; var start_loc = NextLocation; 
		Expect(18);
		name = !t.val.EndsWith("Attribute", StringComparison.CurrentCulture) ? t.val + "Attribute" : t.val;
		type_path = AstType.MakeSimpleType(name, start_loc);
		
		if (la.kind == 6) {
			ObjectCreation(type_path, null, out expr);
			creation = (ObjectCreationExpression)expr; 
		}
		if(creation == null)
		   creation = Expression.MakeObjectCreation(type_path, CurrentEndLocation);
		
	}

	void ObjectCreation(AstType typePath, List<KeyValueType> typeArgs, out Expression expr ) {
		var fields = new List<Identifier>(); var values = new List<Expression>(); 
		Expect(6);
		if (la.kind == 18 || la.kind == 19) {
			if (la.kind == 19) {
				Get();
			} else {
				Get();
			}
			fields.Add(AstNode.MakeIdentifier(t.val, ExpressoModifiers.None, CurrentLocation)); 
			Expect(3);
			CondExpr(out expr);
			values.Add(expr); 
		}
		while (la.kind == 14) {
			Get();
			if (la.kind == 19) {
				Get();
			} else if (la.kind == 18) {
				Get();
			} else SynErr(133);
			fields.Add(AstNode.MakeIdentifier(t.val, ExpressoModifiers.None, CurrentLocation)); 
			Expect(3);
			CondExpr(out expr);
			values.Add(expr); 
		}
		while (!(la.kind == 0 || la.kind == 11)) {SynErr(134); Get();}
		Expect(11);
		expr = Expression.MakeObjectCreation(typePath, fields, values, typeArgs, CurrentEndLocation); 
	}

	void ImportDecl(out ImportDeclaration decl) {
		decl = null; bool seen_from = false;
		Identifier alias = null; Identifier target_file = null; var start_loc = NextLocation;
		var paths = new List<Identifier>(); var aliases = new List<Identifier>();
		
		while (!(la.kind == 0 || la.kind == 37)) {SynErr(135); Get();}
		Expect(37);
		ImportPaths(ref paths, start_loc);
		if (la.kind == 38) {
			Get();
			seen_from = true;
			var file_start_loc = NextLocation;
			
			Expect(24);
			var name = t.val.Substring(1, t.val.Length - 2);
			if(!scanner.ChildFileExists(name)){
			   SemanticError(file_start_loc, "ES0020", "The external file '{0}' doesn't exist.", name);
			}
			target_file = AstNode.MakeIdentifier(name, ExpressoModifiers.None, file_start_loc);
			
		}
		foreach(var path in paths){
		   if(!seen_from)
		       Symbols.AddNativeSymbolTable(path);
		}
		
		Expect(39);
		var alias_start_loc = NextLocation; 
		if (la.kind == 18) {
			ImportAlias(out alias, alias_start_loc);
			aliases.Add(alias); 
		} else if (la.kind == 6) {
			Get();
			ImportAlias(out alias, alias_start_loc);
			aliases.Add(alias); 
			while (la.kind == 14) {
				Get();
				alias_start_loc = NextLocation; 
				ImportAlias(out alias, alias_start_loc);
				aliases.Add(alias); 
			}
			Expect(11);
		} else SynErr(136);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(137); Get();}
		Expect(5);
		decl = AstNode.MakeImportDecl(paths, aliases, target_file, start_loc, CurrentEndLocation); 
	}

	void ImportPaths(ref List<Identifier> paths, TextLocation startLoc) {
		var builder = new StringBuilder(); Identifier ident = null; 
		if (IsStartOfImportPath()) {
			ImportPath(out ident, "", startLoc);
			paths.Add(ident); 
		} else if (la.kind == 18) {
			Get();
			builder.Append(t.val); 
		} else SynErr(138);
		if (la.kind == 4) {
			Get();
			builder.Append("::");
			var parent_namespace = builder.ToString();
			
			if (la.kind == 18) {
				if (IsStartOfTypeNameOfImportPath()) {
					ImportPath(out ident, parent_namespace, startLoc);
					paths.Add(ident); 
				} else {
					Get();
					builder.Append(t.val); 
				}
			} else if (la.kind == 6) {
				Get();
				var parent_namespace2 = builder.ToString(); 
				ImportPath(out ident, parent_namespace2, startLoc);
				paths.Add(ident); 
				while (la.kind == 14) {
					Get();
					ImportPath(out ident, parent_namespace2, startLoc);
					paths.Add(ident); 
				}
				Expect(11);
			} else SynErr(139);
		}
		while (la.kind == 15) {
			Get();
			builder.Append('.');
			var parent_namespace3 = builder.ToString();
			
			if (la.kind == 18) {
				if (IsStartOfTypeNameOfImportPath()) {
					ImportPath(out ident, parent_namespace3, startLoc);
					paths.Add(ident); 
				} else {
					Get();
					builder.Append(t.val); 
				}
			} else if (la.kind == 6) {
				Get();
				var parent_namespace4 = builder.ToString(); 
				ImportPath(out ident, parent_namespace4, startLoc);
				paths.Add(ident); 
				while (la.kind == 14) {
					Get();
					ImportPath(out ident, parent_namespace4, startLoc);
					paths.Add(ident); 
				}
				Expect(11);
			} else SynErr(140);
		}
	}

	void ImportAlias(out Identifier ident, TextLocation startLoc) {
		ident = null; 
		Expect(18);
		if(!CheckKeyword(t.val)){
		   ident = AstNode.MakeIdentifier(t.val, ExpressoModifiers.None, startLoc);
		   Symbols.AddTypeSymbol(ident.Name, ident);
		}else{
		   // Failed to parse an alias name
		   // Leave the parser to recover its state.
		}
		
	}

	void ImportPath(out Identifier ident, string parentNamespace, TextLocation startLoc) {
		ident = null; 
		Expect(18);
		ident = AstNode.MakeIdentifier(parentNamespace + t.val, ExpressoModifiers.None, startLoc);
		Symbols.AddTypeSymbol(ident.Name, ident);
		
	}

	void GenericTypeParameters(ref List<ParameterType> types ) {
		Expect(8);
		Expect(18);
		types.Add(AstType.MakeParameterType(t.val)); 
		while (la.kind == 14) {
			Get();
			Expect(18);
			types.Add(AstType.MakeParameterType(t.val)); 
		}
		Expect(12);
	}

	void Type(out AstType type) {
		var start_loc = NextLocation; type = new PlaceholderType(NextLocation);
		var is_reference = false;
		
		if (StartOf(6)) {
			if (la.kind == 53) {
				Get();
				is_reference = true;
				start_loc = NextLocation;
				
			}
			switch (la.kind) {
			case 54: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 55: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 56: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 57: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 58: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 59: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 60: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 61: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 62: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 7: {
				TupleTypeSignature(out type);
				break;
			}
			case 63: {
				Get();
				type = AstType.MakeSimpleType(t.val, Enumerable.Empty<AstType>(), start_loc, CurrentEndLocation); 
				break;
			}
			case 64: {
				Get();
				type = AstType.MakeSimpleType(t.val, Enumerable.Empty<AstType>(), start_loc, CurrentEndLocation); 
				break;
			}
			case 65: {
				Get();
				type = AstType.MakeSimpleType(t.val, Enumerable.Empty<AstType>(), start_loc, CurrentEndLocation); 
				break;
			}
			case 66: {
				Get();
				type = CreatePrimitiveType(t.val, start_loc, is_reference); 
				break;
			}
			case 67: {
				Get();
				type = AstType.MakeSimpleType("tuple", Enumerable.Empty<AstType>(), start_loc, CurrentEndLocation); 
				break;
			}
			case 18: {
				TypePathExpression(out type);
				if(is_reference)
				   type = AstType.MakeReferenceType(type, start_loc);
				
				var name = type.Name;
				if(type_params.Any(param => param.Name == name)){
				   var param_type = type_params.First(arg => arg.Name == name);
				   type = param_type.Clone();
				}
				
				break;
			}
			default: SynErr(141); break;
			}
			start_loc = NextLocation; 
			if (IsGenericTypeSignature()) {
				GenericTypeSignature(is_reference, ref type);
				
			}
			while (IsArrayTypeSignature()) {
				Expect(9);
				Expect(13);
				type = AstType.MakeSimpleType("array", start_loc, CurrentEndLocation, type);
				
			}
			var replacer = new ParameterTypeReplacer(type_params);
			type.AcceptWalker(replacer);
			
		} else if (la.kind == 68) {
			FunctionTypeSignature(out type);
		} else SynErr(142);
	}

	void TypeConstraints(ref List<TypeConstraint> constraints ) {
		TypeConstraint constraint = null; 
		TypeConstraint(out constraint);
		constraints.Add(constraint); 
		while (la.kind == 52) {
			TypeConstraint(out constraint);
			constraints.Add(constraint); 
		}
	}

	void MethodSignature(out EntityDeclaration method, AttributeSection attribute) {
		Identifier ident = null;
		string name = null; AstType type = null;
		var type_params = new List<ParameterType>(this.type_params);
		var @params = new List<ParameterDeclaration>();
		var start_loc = NextLocation;
		var type_constraints = new List<TypeConstraint>();
		var replacer = new ParameterTypeReplacer(type_params);
		
		while (!(la.kind == 0 || la.kind == 41)) {SynErr(143); Get();}
		Expect(41);
		Symbols.AddScope();
		var ident_start_loc = NextLocation;
		
		Expect(18);
		name = t.val;
		if(!CheckKeyword(name)){
		   ident = AstNode.MakeIdentifier(name, AstType.MakePlaceholderType(CurrentLocation), ExpressoModifiers.Public, ident_start_loc); 
		   Symbols.AddSymbol(name, ident);
		}else{
		   // The name is unsuitable for a method or a function name.
		   // Leave the parser to recover its state.
		}
		
		if (la.kind == 8) {
			GenericTypeParameters(ref type_params);
		}
		Expect(7);
		GoDownScope();
		Symbols.Name = "method_signature " + name + "`" + ScopeId++;
		
		if (la.kind == 18 || la.kind == 36) {
			ParamList(type_params, ref @params);
		}
		Expect(10);
		Expect(42);
		Type(out type);
		if (la.kind == 52) {
			TypeConstraints(ref type_constraints);
		}
		if(!type_constraints.Any()){
		   type_constraints = type_params.Where(p => !this.type_params.Any(tp => tp.Name == p.Name))
		                                 .Select(p => AstNode.MakeTypeConstraint((ParameterType)p.Clone(), null)).ToList();
		}
		
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(144); Get();}
		Expect(5);
		method = EntityDeclaration.MakeFunc(ident, @params, null, type, ExpressoModifiers.Public, type_constraints, attribute, start_loc);
		method.ReturnType.AcceptWalker(replacer);
		GoUpScope();
		
	}

	void ParamList(List<ParameterType> typeParams, ref List<ParameterDeclaration> @params ) {
		ParameterDeclaration param; bool seen_option = false; bool seen_variadic = false; var replacer = new ParameterTypeReplacer(typeParams); 
		Parameter(out param);
		if(!param.Option.IsNull)
		   seen_option = true;
		else if(param.IsVariadic)
		   seen_variadic = true;
		
		param.ReturnType.AcceptWalker(replacer);
		@params.Add(param);
		
		while (WeakSeparator(14,7,8) ) {
			Parameter(out param);
			if(seen_option && param.Option.IsNull)
			   SemanticError("ES0002", "You can't put optional parameters before non-optional parameters.");
			else if(seen_variadic)
			   SemanticError("ES0010", "The variadic parameter has to be placed in the last position of a parameter list.");
			else if(param.IsVariadic)
			   seen_variadic = true;
			else if(!seen_option && !param.Option.IsNull)
			   seen_option = true;
			
			param.ReturnType.AcceptWalker(replacer);
			@params.Add(param);
			
		}
	}

	void Modifiers() {
		switch (la.kind) {
		case 43: {
			Get();
			cur_modifiers &= ~ExpressoModifiers.Private;
			cur_modifiers |= ExpressoModifiers.Public;
			
			break;
		}
		case 44: {
			Get();
			cur_modifiers &= ~ExpressoModifiers.Private;
			cur_modifiers |= ExpressoModifiers.Protected;
			
			break;
		}
		case 45: {
			Get();
			cur_modifiers |= ExpressoModifiers.Private; 
			break;
		}
		case 46: {
			Get();
			cur_modifiers |= ExpressoModifiers.Static; 
			break;
		}
		case 47: {
			Get();
			cur_modifiers |= ExpressoModifiers.Mutating; 
			break;
		}
		case 48: {
			Get();
			cur_modifiers |= ExpressoModifiers.Override; 
			break;
		}
		default: SynErr(145); break;
		}
	}

	void TupleStyleEnumMember(out EntityDeclaration entity, AttributeSection attribute) {
		var start_loc = NextLocation; Identifier ident = null; AstType type = null; var types = new List<AstType>(); 
		Expect(18);
		var name = t.val; 
		Expect(7);
		if (StartOf(9)) {
			Type(out type);
			types.Add(type); 
		}
		while (la.kind == 14) {
			Get();
			Type(out type);
			types.Add(type); 
		}
		Expect(10);
		var ident_type = AstType.MakeSimpleType(cur_class_name);
		var pattern_type = AstType.MakeSimpleType("tuple", types);
		ident_type.IdentifierNode.Type = pattern_type;
		ident = AstNode.MakeIdentifier(name, ident_type, ExpressoModifiers.Public, start_loc);
		Symbols.AddSymbol(name, ident);
		
		var ident_pat = PatternConstruct.MakeIdentifierPattern(ident);
		var pattern = PatternConstruct.MakePatternWithType(ident_pat, pattern_type.Clone());
		entity = EntityDeclaration.MakeField(pattern, null, ExpressoModifiers.Public, attribute, start_loc, CurrentEndLocation);
		
	}

	void RawValueStyleEnumMember(out EntityDeclaration entity, AttributeSection attribute, ref int rawValue) {
		var start_loc = NextLocation; string name; Identifier ident = null; Expression expr = null; 
		Expect(18);
		name = t.val;
		var ident_type = AstType.MakeSimpleType(cur_class_name);
		var type = AstType.MakePrimitiveType("int", NextLocation);
		ident_type.IdentifierNode.Type = type;
		ident = AstNode.MakeIdentifier(name, ident_type, ExpressoModifiers.Public | ExpressoModifiers.Static, start_loc);
		Symbols.AddSymbol(name, ident);
		
		var ident_pat = PatternConstruct.MakeIdentifierPattern(ident);
		var pattern = PatternConstruct.MakePatternWithType(ident_pat, type.Clone());
		
		if (la.kind == 17) {
			Get();
			Literal(out expr);
			rawValue = (int)((LiteralExpression)expr).Value; 
		}
		if(expr == null)
		   expr = Expression.MakeConstant("int", rawValue);
		
		++rawValue;
		entity = EntityDeclaration.MakeField(pattern, expr, ExpressoModifiers.Public | ExpressoModifiers.Static, attribute, start_loc, CurrentEndLocation);
		
	}

	void Block(out BlockStatement block) {
		List<Statement> stmts = new List<Statement>();
		Statement stmt; var start_loc = NextLocation;
		Symbols.AddScope();
		
		Expect(6);
		GoDownScope();
		Symbols.Name = "block`" + ScopeId++;
		
		Stmt(out stmt);
		stmts.Add(stmt); 
		while (StartOf(10)) {
			Stmt(out stmt);
			stmts.Add(stmt); 
		}
		while (!(la.kind == 0 || la.kind == 11)) {SynErr(146); Get();}
		Expect(11);
		block = Statement.MakeBlock(stmts, start_loc, CurrentEndLocation);
		GoUpScope();
		
	}

	void Parameter(out ParameterDeclaration param) {
		string name; Identifier identifier; Expression option = null; AstType type = null; bool is_variadic = false;
		var start_loc = NextLocation; AttributeSection attribute = null;
		
		if (la.kind == 36) {
			AttributeSection(out attribute);
		}
		Expect(18);
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
		if (la.kind == 49) {
			Get();
			Type(out type);
			if(is_variadic && (type == null || !(type is SimpleType simple) || simple.Name != "array"))
			SemanticError("ES0001", "The variadic parameter must be an array!");
			
		}
		if (la.kind == 17) {
			Get();
			Literal(out option);
		}
		var modifiers = (type is ReferenceType) ? ExpressoModifiers.None : ExpressoModifiers.Immutable;
		identifier = AstNode.MakeIdentifier(name, type ?? new PlaceholderType(CurrentLocation), modifiers, start_loc);
		if(!defining_closure_parameters && type == null && option == null)
		SemanticError("ES0004", "You can't omit both the type annotation and the optional value in a function parameter definition!; `{0}`", name);
		
		Symbols.AddSymbol(name, identifier);
		param = EntityDeclaration.MakeParameter(identifier, option, attribute, is_variadic, start_loc);
		
	}

	void Literal(out Expression expr) {
		expr = null; string tmp;
		  var start_loc = NextLocation;
		
		switch (la.kind) {
		case 19: {
			Get();
			expr = CreateLiteral(t.val, start_loc); 
			break;
		}
		case 21: {
			Get();
			expr = Expression.MakeConstant("int", Convert.ToInt32(t.val, 16), start_loc); 
			break;
		}
		case 20: {
			Get();
			expr = CreateLiteral(t.val, start_loc); 
			break;
		}
		case 23: {
			Get();
			tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = HandleEscapes("char", tmp, start_loc);
			
			break;
		}
		case 24: {
			Get();
			tmp = t.val;
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = HandleEscapes("string", tmp, start_loc);
			
			break;
		}
		case 25: {
			Get();
			tmp = t.val.Substring(1);
			if(tmp.StartsWith("#")){
			   int index_double_quote = tmp.IndexOf('"');
			   int start_end_hashes = tmp.Length - index_double_quote - 1;
			   int index_end_double_quote = tmp.LastIndexOf('"');
			   if(start_end_hashes != index_end_double_quote + 1)
			       SemanticError(start_loc, "ES0005", "The number of opening and closing hash symbols in a raw string must match!");
			
			   tmp = tmp.Substring(index_double_quote, tmp.Length - index_end_double_quote - index_double_quote);
			}
			tmp = tmp.Substring(1, tmp.Length - 2);
			expr = Expression.MakeConstant("string", tmp, start_loc);
			
			break;
		}
		case 112: {
			Get();
			expr = Expression.MakeConstant("bool", true, start_loc); 
			break;
		}
		case 113: {
			Get();
			expr = Expression.MakeConstant("bool", false, start_loc); 
			break;
		}
		case 115: {
			SelfReferenceExpression(out expr);
			break;
		}
		case 116: {
			SuperReferenceExpression(out expr);
			break;
		}
		case 114: {
			Get();
			expr = Expression.MakeNullRef(start_loc); 
			break;
		}
		default: SynErr(147); break;
		}
	}

	void VarDef(out PatternWithType typed_pattern, out Expression option) {
		option = null; var loc = NextLocation; var replacer = new ParameterTypeReplacer(type_params); 
		PatternWithType(out typed_pattern);
		if (la.kind == 17) {
			Get();
			CondExpr(out option);
		}
		if(typed_pattern.Pattern is IdentifierPattern ident_pat){
		   if(typed_pattern.Type is PlaceholderType && option == null)
		       SemanticError(loc, "ES0003", "Give me some context or I can't infer the type of {0}.", ident_pat.Identifier.Name);
		
		   ident_pat.Identifier.Type.AcceptWalker(replacer);
		   typed_pattern.Type.AcceptWalker(replacer);
		}
		
	}

	void TypeConstraint(out TypeConstraint constraint) {
		AstType type = null; var types = new List<AstType>(); var start_loc = NextLocation; 
		Expect(52);
		Expect(18);
		var param_type = AstType.MakeParameterType(t.val); 
		Expect(3);
		Type(out type);
		types.Add(type); 
		while (la.kind == 14) {
			Get();
			Type(out type);
			types.Add(type); 
		}
		constraint = AstNode.MakeTypeConstraint(param_type, types, start_loc); 
	}

	void TupleTypeSignature(out AstType type) {
		var inners = new List<AstType>(); var start_loc = NextLocation; 
		Expect(7);
		if (StartOf(9)) {
			Type(out type);
			inners.Add(type); 
		}
		while (la.kind == 14) {
			Get();
			Type(out type);
			inners.Add(type); 
		}
		Expect(10);
		type = AstType.MakeSimpleType("tuple", inners, start_loc, CurrentEndLocation); 
	}

	void TypePathExpression(out AstType type) {
		Expect(18);
		type = AstType.MakeSimpleType(t.val, CurrentLocation); 
		while (la.kind == 4) {
			Get();
			Expect(18);
			type = AstType.MakeMemberType(type, AstType.MakeSimpleType(t.val, CurrentLocation), CurrentEndLocation); 
		}
	}

	void GenericTypeSignature(bool isReference, ref AstType type) {
		var type_args = new List<AstType>(); AstType child_type; var replacer = new ParameterTypeReplacer(type_params); 
		Expect(8);
		Type(out child_type);
		type_args.Add(child_type); 
		while (la.kind == 14) {
			Get();
			Type(out child_type);
			type_args.Add(child_type); 
		}
		Expect(12);
		if(type is SimpleType generic_type){
		   type = AstType.MakeSimpleType(generic_type.Name, type_args, generic_type.StartLocation, CurrentEndLocation);
		   type.AcceptWalker(replacer);
		}else if(type is MemberType member_type){
		   if(member_type.ChildType is SimpleType generic_type2){
		       var new_type = AstType.MakeSimpleType(generic_type2.Name, type_args, generic_type2.StartLocation, CurrentEndLocation);
		       generic_type2.ReplaceWith(new_type);
		       new_type.AcceptWalker(replacer);
		   }
		}
		
		if(isReference)
		   type = AstType.MakeReferenceType(type, CurrentEndLocation);
		
	}

	void FunctionTypeSignature(out AstType type) {
		var start_loc = NextLocation; AstType inner_type; List<AstType> arg_types = new List<AstType>(); 
		Expect(68);
		while (IsStartOfAnotherType()) {
			Type(out inner_type);
			arg_types.Add(inner_type); 
		}
		Expect(68);
		Expect(42);
		Type(out inner_type);
		type = AstType.MakeFunctionType("closure", inner_type, arg_types, start_loc, CurrentEndLocation); 
	}

	void GenericTypeArguments(ref List<KeyValueType> typeArgs ) {
		AstType type = null; ParameterType param_type = null; 
		Expect(8);
		if (IsTypeParameter()) {
			Expect(18);
			param_type = AstType.MakeParameterType(t.val); 
			Expect(17);
		}
		if(param_type == null)
		   param_type = AstType.MakeParameterType("");
		
		Type(out type);
		typeArgs.Add(AstType.MakeKeyValueType(param_type, type)); 
		while (la.kind == 14) {
			Get();
			param_type = null; 
			if (IsTypeParameter()) {
				Expect(18);
				param_type = AstType.MakeParameterType(t.val); 
				Expect(17);
			}
			if(param_type == null)
			   param_type = AstType.MakeParameterType(t.val);
			
			Type(out type);
			typeArgs.Add(AstType.MakeKeyValueType(param_type, type)); 
		}
		Expect(12);
	}

	void Stmt(out Statement stmt) {
		stmt = null; 
		if (StartOf(11)) {
			SimpleStmt(out stmt);
		} else if (StartOf(12)) {
			CompoundStmt(out stmt);
		} else SynErr(148);
	}

	void SimpleStmt(out Statement stmt) {
		var start_loc = NextLocation; stmt = null; 
		switch (la.kind) {
		case 18: case 115: case 116: {
			ExprStmt(out stmt);
			break;
		}
		case 50: case 51: {
			VarDeclStmt(out stmt);
			break;
		}
		case 35: {
			ReturnStmt(out stmt);
			break;
		}
		case 69: {
			BreakStmt(out stmt);
			break;
		}
		case 71: {
			ContinueStmt(out stmt);
			break;
		}
		case 72: {
			YieldStmt(out stmt);
			break;
		}
		case 73: {
			ThrowStmt(out stmt);
			break;
		}
		case 5: {
			EmptyStmt(out stmt);
			break;
		}
		default: SynErr(149); break;
		}
	}

	void CompoundStmt(out Statement stmt) {
		stmt = null; BlockStatement block = null; 
		switch (la.kind) {
		case 6: {
			Block(out block);
			stmt = block; 
			break;
		}
		case 84: {
			IfStmt(out stmt);
			break;
		}
		case 86: {
			WhileStmt(out stmt);
			break;
		}
		case 87: {
			DoWhileStmt(out stmt);
			break;
		}
		case 26: {
			ForStmt(out stmt);
			break;
		}
		case 89: {
			MatchStmt(out stmt);
			break;
		}
		case 90: {
			TryStmt(out stmt);
			break;
		}
		default: SynErr(150); break;
		}
	}

	void ExprStmt(out Statement stmt) {
		SequenceExpression lhs = null, seq = null;
		var start_loc = NextLocation; stmt = null;
		OperatorType op_type = OperatorType.None;
		        AssignmentExpression assign = null;
		
		LValueList(out lhs);
		if (StartOf(13)) {
			if (StartOf(14)) {
				AugmentedAssignOperators(ref op_type);
				RValueList(out seq);
				if(lhs.Count != seq.Count)  //See if both sides have the same number of items or not
				   SemanticError("ES0008", "An augmented assignment must have both sides balanced.");
				
				stmt = Statement.MakeAugmentedAssignment(op_type, lhs, seq, start_loc, CurrentEndLocation);
				
			} else {
				Get();
				RValueList(out seq);
				assign = Expression.MakeAssignment(lhs, seq); 
				while (la.kind == 17) {
					Get();
					RValueList(out seq);
					assign = Expression.MakeMultipleAssignment(assign, seq); 
				}
				stmt = Statement.MakeExprStmt(assign, start_loc, CurrentEndLocation); 
			}
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(151); Get();}
		Expect(5);
		if(stmt == null)
		stmt = Statement.MakeExprStmt(lhs, start_loc, CurrentEndLocation);
		
	}

	void VarDeclStmt(out Statement stmt) {
		Expression rhs = null; var start_loc = NextLocation;
		PatternWithType pattern; 
		var patterns = new List<PatternWithType>(); var exprs = new List<Expression>();
		cur_modifiers = ExpressoModifiers.None;
		
		if (la.kind == 50) {
			Get();
			cur_modifiers = ExpressoModifiers.Immutable; 
		} else if (la.kind == 51) {
			Get();
		} else SynErr(152);
		VarDef(out pattern, out rhs);
		patterns.Add(pattern);
		exprs.Add(rhs ?? Expression.Null);
		rhs = null;
		
		while (WeakSeparator(14,15,16) ) {
			VarDef(out pattern, out rhs);
			patterns.Add(pattern);
			exprs.Add(rhs ?? Expression.Null);
			rhs = null;
			
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(153); Get();}
		Expect(5);
		stmt = Statement.MakeVarDecl(patterns, exprs, cur_modifiers, start_loc, CurrentEndLocation); 
	}

	void ReturnStmt(out Statement stmt) {
		SequenceExpression items = null; var start_loc = NextLocation; 
		Expect(35);
		if (StartOf(17)) {
			RValueList(out items);
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(154); Get();}
		Expect(5);
		stmt = Statement.MakeReturnStmt(items, start_loc); 
	}

	void BreakStmt(out Statement stmt) {
		int count = 1; var start_loc = NextLocation; 
		Expect(69);
		if (la.kind == 70) {
			Get();
			Expect(19);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(155); Get();}
		Expect(5);
		stmt = Statement.MakeBreakStmt(Expression.MakeConstant("int", count, start_loc), start_loc, CurrentEndLocation); 
	}

	void ContinueStmt(out Statement stmt) {
		int count = 1; var start_loc = NextLocation; 
		Expect(71);
		if (la.kind == 70) {
			Get();
			Expect(19);
			count = Convert.ToInt32(t.val); 
		}
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(156); Get();}
		Expect(5);
		stmt = Statement.MakeContinueStmt(Expression.MakeConstant("int", count, start_loc), start_loc, CurrentEndLocation); 
	}

	void YieldStmt(out Statement stmt) {
		Expression expr; var start_loc = NextLocation; 
		Expect(72);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(157); Get();}
		Expect(5);
		stmt = Statement.MakeYieldStmt(expr, start_loc, CurrentEndLocation); 
	}

	void ThrowStmt(out Statement stmt) {
		Expression obj; var start_loc = NextLocation; AstType type_path; 
		Expect(73);
		Type(out type_path);
		ObjectCreation(type_path, null, out obj);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(158); Get();}
		Expect(5);
		stmt = Statement.MakeThrowStmt((ObjectCreationExpression)obj, start_loc); 
	}

	void EmptyStmt(out Statement stmt) {
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(159); Get();}
		Expect(5);
		stmt = Statement.MakeEmptyStmt(CurrentLocation); 
	}

	void RValueList(out SequenceExpression seq) {
		Expression tmp; var exprs = new List<Expression>(); 
		CondExpr(out tmp);
		exprs.Add(tmp); 
		while (la.kind == 14) {
			Get();
			CondExpr(out tmp);
			exprs.Add(tmp);	
		}
		seq = Expression.MakeSequenceExpression(exprs); 
	}

	void CondExpr(out Expression expr) {
		Expression true_expr, false_expr; expr = null; 
		if (StartOf(18)) {
			OrTest(out expr);
			if (la.kind == 93) {
				Get();
				OrTest(out true_expr);
				Expect(3);
				CondExpr(out false_expr);
				expr = Expression.MakeCondExpr(expr, true_expr, false_expr); 
			}
		} else if (la.kind == 68) {
			ClosureLiteral(out expr);
		} else SynErr(160);
	}

	void AugmentedAssignOperators(ref OperatorType type) {
		switch (la.kind) {
		case 74: {
			Get();
			type = OperatorType.Plus; 
			break;
		}
		case 75: {
			Get();
			type = OperatorType.Minus; 
			break;
		}
		case 76: {
			Get();
			type = OperatorType.Times; 
			break;
		}
		case 77: {
			Get();
			type = OperatorType.Divide; 
			break;
		}
		case 78: {
			Get();
			type = OperatorType.Power; 
			break;
		}
		case 79: {
			Get();
			type = OperatorType.Modulus; 
			break;
		}
		case 80: {
			Get();
			type = OperatorType.BitwiseAnd; 
			break;
		}
		case 81: {
			Get();
			type = OperatorType.BitwiseOr; 
			break;
		}
		case 82: {
			Get();
			type = OperatorType.BitwiseShiftLeft; 
			break;
		}
		case 83: {
			Get();
			type = OperatorType.BitwiseShiftRight; 
			break;
		}
		default: SynErr(161); break;
		}
	}

	void LValueList(out SequenceExpression lhs) {
		var lvalues = new List<Expression>(); Expression tmp; 
		LhsPrimary(out tmp);
		lvalues.Add(tmp); 
		while (WeakSeparator(14,19,20) ) {
			LhsPrimary(out tmp);
			lvalues.Add(tmp); 
		}
		lhs = Expression.MakeSequenceExpression(lvalues); 
	}

	void LhsPrimary(out Expression expr) {
		expr = null; PathExpression path; 
		if (la.kind == 18) {
			PathExpression(out path);
			expr = path; 
		} else if (la.kind == 115) {
			SelfReferenceExpression(out expr);
		} else if (la.kind == 116) {
			SuperReferenceExpression(out expr);
		} else SynErr(162);
		while (StartOf(21)) {
			Trailer(ref expr);
		}
	}

	void IfStmt(out Statement stmt) {
		PatternConstruct pattern = null; BlockStatement true_block = null, false_block = null;
		      var start_loc = NextLocation; Statement else_stmt = null;
		   
		Expect(84);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "if`" + ScopeId++;
		
		ExpressionPattern(out pattern);
		Block(out true_block);
		if (la.kind == 85) {
			Get();
			if (la.kind == 6) {
				Block(out false_block);
			} else if (la.kind == 84) {
				IfStmt(out else_stmt);
			} else SynErr(163);
		}
		stmt = Statement.MakeIfStmt(pattern, true_block, false_block ?? else_stmt, start_loc);
		       GoUpScope();
		    
	}

	void WhileStmt(out Statement stmt) {
		Expression cond; BlockStatement body; var start_loc = NextLocation; 
		Expect(86);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "while`" + ScopeId++;
		
		CondExpr(out cond);
		Block(out body);
		stmt = Statement.MakeWhileStmt(cond, body, start_loc);
		GoUpScope();
		
	}

	void DoWhileStmt(out Statement stmt) {
		var start_loc = NextLocation; Expression expr = null; BlockStatement body = null; 
		Expect(87);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "do-while`" + ScopeId++;
		
		Block(out body);
		Expect(86);
		CondExpr(out expr);
		while (!(la.kind == 0 || la.kind == 5)) {SynErr(164); Get();}
		Expect(5);
		stmt = Statement.MakeDoWhileStmt(expr, body, start_loc, CurrentEndLocation);
		GoUpScope();
		
	}

	void ForStmt(out Statement stmt) {
		PatternWithType left = null; Expression rvalue; BlockStatement body; cur_modifiers = ExpressoModifiers.None;
		     var start_loc = NextLocation;
		
		Expect(26);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "for`" + ScopeId++;
		
		if (la.kind == 50) {
			Get();
			cur_modifiers = ExpressoModifiers.Immutable; 
		} else if (la.kind == 51) {
			Get();
		} else SynErr(165);
		PatternWithType(out left);
		Expect(88);
		CondExpr(out rvalue);
		Block(out body);
		if(left != null)
		   stmt = Statement.MakeValueBindingForStmt(cur_modifiers, left, rvalue, body, start_loc);
		else
		   stmt = Statement.MakeForStmt(left, rvalue, body, start_loc);
		
		                        GoUpScope();
		                        cur_modifiers = ExpressoModifiers.None;
		                     
	}

	void MatchStmt(out Statement stmt) {
		List<MatchPatternClause> matches; var start_loc = NextLocation; Expression expr; 
		Expect(89);
		CondExpr(out expr);
		Expect(6);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "match`" + ScopeId++;
		
		MatchPatternList(out matches);
		Expect(11);
		stmt = Statement.MakeMatchStmt(expr, matches, start_loc, CurrentEndLocation);
		GoUpScope();
		
	}

	void TryStmt(out Statement stmt) {
		var start_loc = NextLocation; BlockStatement body; var catches = new List<CatchClause>(); FinallyClause @finally = null; 
		Expect(90);
		Block(out body);
		if (la.kind == 91) {
			CatchClauses(out catches);
		}
		if (la.kind == 92) {
			FinallyClause(out @finally);
		}
		if(catches.Count == 0 && @finally == null){
		   SemErr("Error ES0020: A try statement must include either a catch clause or the finally clause");
		}
		stmt = Statement.MakeTryStmt(body, catches, @finally, start_loc);
		
	}

	void ExpressionPattern(out PatternConstruct pattern) {
		Expression expr, true_expr; PatternConstruct false_pattern; pattern = null; 
		PatternOrTest(out expr);
		if (la.kind == 93) {
			Get();
			PatternOrTest(out true_expr);
			Expect(3);
			ExpressionPattern(out false_pattern);
			pattern = PatternConstruct.MakeExpressionPattern(Expression.MakeCondExpr(expr, true_expr, ((ExpressionPattern)false_pattern).Expression)); 
		}
		if(pattern == null)
		   pattern = PatternConstruct.MakeExpressionPattern(expr);
		
	}

	void PatternWithType(out PatternWithType typed_pattern) {
		typed_pattern = null; var pattern = PatternConstruct.Null; 
		if (la.kind == 94) {
			WildcardPattern(out pattern);
		} else if (IsIdentifierPattern()) {
			IdentifierPattern(out pattern);
		} else if (la.kind == 7) {
			TuplePattern(out pattern);
		} else if (la.kind == 9 || la.kind == 18) {
			DestructuringPattern(out pattern);
		} else SynErr(166);
		AstType type = new PlaceholderType(NextLocation); 
		if (la.kind == 49) {
			Get();
			Type(out type);
		}
		if(pattern is IdentifierPattern ident_pat)
		   ident_pat.Identifier.Type = type;
		
		typed_pattern = PatternConstruct.MakePatternWithType(pattern, type.Clone());
		
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
		 
		while (la.kind == 14) {
			while (!(la.kind == 0 || la.kind == 14)) {SynErr(167); Get();}
			Get();
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
		while (la.kind == 68) {
			while (!(la.kind == 0 || la.kind == 68)) {SynErr(168); Get();}
			Get();
			Pattern(out tmp);
			patterns.Add(tmp); 
		}
		if (la.kind == 84) {
			Get();
			CondExpr(out guard);
		}
		Expect(16);
	}

	void MatchArmStmt(out Statement stmt) {
		stmt = null; BlockStatement block = null; 
		switch (la.kind) {
		case 6: {
			Block(out block);
			stmt = block; 
			break;
		}
		case 18: case 115: case 116: {
			ExprStmt(out stmt);
			break;
		}
		case 50: case 51: {
			VarDeclStmt(out stmt);
			break;
		}
		case 35: {
			ReturnStmt(out stmt);
			break;
		}
		case 69: {
			BreakStmt(out stmt);
			break;
		}
		case 71: {
			ContinueStmt(out stmt);
			break;
		}
		case 72: {
			YieldStmt(out stmt);
			break;
		}
		case 73: {
			ThrowStmt(out stmt);
			break;
		}
		default: SynErr(169); break;
		}
	}

	void Pattern(out PatternConstruct pattern) {
		pattern = null; 
		if (la.kind == 94) {
			WildcardPattern(out pattern);
		} else if (la.kind == 7) {
			TuplePattern(out pattern);
		} else if (IsIdentifierPattern()) {
			IdentifierPattern(out pattern);
		} else if (DistinguishBetweenDestructuringPatternAndEnumMember()) {
			DestructuringPattern(out pattern);
		} else if (StartOf(22)) {
			RValuePattern(out pattern);
		} else if (la.kind == 18) {
			EnumMemberPattern(out pattern);
		} else SynErr(170);
	}

	void CatchClauses(out List<CatchClause> catches ) {
		catches = new List<CatchClause>(); CatchClause @catch; 
		CatchClause(out @catch);
		catches.Add(@catch); 
		while (la.kind == 91) {
			CatchClause(out @catch);
			catches.Add(@catch); 
		}
	}

	void FinallyClause(out FinallyClause @finally) {
		var start_loc = NextLocation; BlockStatement body; 
		Expect(92);
		Block(out body);
		@finally = Statement.MakeFinallyClause(body, start_loc); 
	}

	void CatchClause(out CatchClause @catch) {
		var start_loc = NextLocation; Identifier ident; BlockStatement body; 
		Expect(91);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "catch`" + ScopeId++;
		cur_modifiers = ExpressoModifiers.None;
		
		Identifier(out ident);
		if(ident.Type is PlaceholderType)
		   SemanticError(start_loc, "ES0011", "A CatchClause identifier has to be explicitly type annotated; {0}", ident.Name);
		
		Symbols.AddSymbol(ident.Name, ident);
		
		Block(out body);
		@catch = Statement.MakeCatchClause(ident, body, start_loc);
		GoUpScope();
		
	}

	void Identifier(out Identifier ident) {
		string name; var start_loc = NextLocation; 
		Expect(18);
		name = t.val;
		if(CheckKeyword(t.val)){
		   ident = Ast.Identifier.Null;
		   return;
		}
		AstType type = AstType.MakePlaceholderType(NextLocation);
		
		if (la.kind == 49) {
			Get();
			Type(out type);
		}
		ident = AstNode.MakeIdentifier(name, type, cur_modifiers, start_loc); 
	}

	void WildcardPattern(out PatternConstruct pattern) {
		Expect(94);
		pattern = PatternConstruct.MakeWildcardPattern(); 
	}

	void TuplePattern(out PatternConstruct pattern) {
		var inners = new List<PatternConstruct>(); pattern = null; 
		Expect(7);
		TupleElementPattern(out pattern);
		inners.Add(pattern); 
		Expect(14);
		if (StartOf(23)) {
			if (StartOf(24)) {
				TupleElementPattern(out pattern);
			} else {
				Get();
				pattern = PatternConstruct.MakeIgnoringRestPattern(CurrentLocation); 
			}
			inners.Add(pattern); 
		}
		while (la.kind == 14) {
			Get();
			if (StartOf(24)) {
				TupleElementPattern(out pattern);
			} else if (la.kind == 1) {
				Get();
				pattern = PatternConstruct.MakeIgnoringRestPattern(CurrentLocation); 
			} else SynErr(171);
			inners.Add(pattern); 
		}
		Expect(10);
		pattern = PatternConstruct.MakeTuplePattern(inners); 
	}

	void IdentifierPattern(out PatternConstruct pattern) {
		PatternConstruct inner = null; string name; var start_loc = NextLocation; 
		Expect(18);
		name = t.val;
		if(CheckKeyword(name)){
		   pattern = PatternConstruct.Null;
		   return;
		}
		var type = new PlaceholderType(NextLocation);
		var ident = AstNode.MakeIdentifier(name, type, cur_modifiers, start_loc);
		Symbols.AddSymbol(name, ident);
		
		if (la.kind == 95) {
			Get();
			Pattern(out inner);
		}
		pattern = PatternConstruct.MakeIdentifierPattern(ident, inner); 
	}

	void DestructuringPattern(out PatternConstruct pattern) {
		pattern = PatternConstruct.Null; AstType type_path;
		var patterns = new List<PatternConstruct>(); bool is_vector = false; string key = null;
		
		if (la.kind == 18) {
			TypePathExpression(out type_path);
			Expect(6);
			if (StartOf(25)) {
				if (IsKeyIdentifier()) {
					Expect(18);
					key = t.val; 
					Expect(3);
				}
				Pattern(out pattern);
				if(key != null){
				   pattern = PatternConstruct.MakeKeyValuePattern(key, pattern);
				   key = null;
				}
				
				patterns.Add(pattern);
				
			}
			while (la.kind == 14) {
				Get();
				if (IsKeyIdentifier()) {
					Expect(18);
					key = t.val; 
					Expect(3);
				}
				if (StartOf(25)) {
					Pattern(out pattern);
					if(key != null){
					   pattern = PatternConstruct.MakeKeyValuePattern(key, pattern);
					   key = null;
					}
					
					patterns.Add(pattern);
					
				} else if (la.kind == 1) {
					Get();
					patterns.Add(PatternConstruct.MakeIgnoringRestPattern(CurrentLocation)); 
				} else SynErr(172);
			}
			Expect(11);
			pattern = PatternConstruct.MakeDestructuringPattern(type_path, patterns); 
		} else if (la.kind == 9) {
			Get();
			if (StartOf(25)) {
				Pattern(out pattern);
				patterns.Add(pattern); 
			}
			while (NotFinalComma()) {
				ExpectWeak(14, 26);
				if (StartOf(25)) {
					Pattern(out pattern);
					patterns.Add(pattern); 
				} else if (la.kind == 1) {
					Get();
					patterns.Add(PatternConstruct.MakeIgnoringRestPattern(CurrentLocation)); 
				} else SynErr(173);
			}
			if (la.kind == 14) {
				Get();
				Expect(2);
				is_vector = true; 
			}
			Expect(13);
			pattern = PatternConstruct.MakeCollectionPattern(patterns, is_vector); 
		} else SynErr(174);
	}

	void RValuePattern(out PatternConstruct pattern) {
		Expression expr; 
		LiteralIntSeqExpr(out expr);
		pattern = PatternConstruct.MakeExpressionPattern(expr); 
	}

	void EnumMemberPattern(out PatternConstruct pattern) {
		AstType type_path; 
		TypePathExpression(out type_path);
		pattern = PatternConstruct.MakeTypePathPattern(type_path); 
	}

	void PatternOrTest(out Expression expr) {
		Expression rhs; 
		PatternAndTest(out expr);
		if (la.kind == 96) {
			Get();
			PatternOrTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalOr, expr, rhs); 
		}
	}

	void LiteralIntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null; bool upper_inclusive = false; 
		Literal(out start);
		expr = start; 
		if (la.kind == 1 || la.kind == 2) {
			RangeOperator(ref upper_inclusive);
			Literal(out end);
			if (la.kind == 3) {
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
		if (la.kind == 94) {
			WildcardPattern(out pattern);
		} else if (la.kind == 7) {
			TuplePattern(out pattern);
		} else if (IsIdentifierPattern()) {
			IdentifierPattern(out pattern);
		} else if (IsDestructuringPattern()) {
			DestructuringPattern(out pattern);
		} else if (StartOf(27)) {
			ExpressionPattern(out pattern);
		} else SynErr(175);
	}

	void OrTest(out Expression expr) {
		Expression rhs; 
		AndTest(out expr);
		if (la.kind == 96) {
			Get();
			OrTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalOr, expr, rhs); 
		}
	}

	void ClosureLiteral(out Expression expr) {
		var parameters = new List<ParameterDeclaration>(); BlockStatement body_block = null;
		var empty_params = new List<ParameterType>(); Expression body_expr; var start_loc = NextLocation;
		
		Expect(68);
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "closure`" + ScopeId++;
		defining_closure_parameters = true;
		
		if (la.kind == 18 || la.kind == 36) {
			ParamList(empty_params, ref parameters);
		}
		Expect(68);
		if(la.val != "->" && la.val != "{"){
		   Symbols.AddScope();
		   GoDownScope();
		   Symbols.Name = "block`" + ScopeId++;
		}
		defining_closure_parameters = false;
		AstType return_type = AstType.MakePlaceholderType(CurrentLocation);
		
		if (la.kind == 42) {
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
		} else if (StartOf(17)) {
			CondExpr(out body_expr);
			var seq_expr = Expression.MakeSequenceExpression(body_expr);
			body_block = Statement.MakeBlock(Statement.MakeReturnStmt(seq_expr, seq_expr.StartLocation));
			
		} else SynErr(176);
		expr = Expression.MakeClosureExpression(parameters, return_type, body_block, start_loc);
		if(t.val != "}")
		   GoUpScope();
		
		GoUpScope();
		
	}

	void AndTest(out Expression expr) {
		Expression rhs; 
		Comparison(out expr);
		if (la.kind == 97) {
			Get();
			AndTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalAnd, expr, rhs); 
		}
	}

	void Comparison(out Expression expr) {
		Expression rhs; OperatorType type; 
		IntSeqExpr(out expr);
		type = OperatorType.Equality; 
		if (StartOf(28)) {
			ComparisonOperator(out type);
			Comparison(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void IntSeqExpr(out Expression expr) {
		Expression start = null, end = null, step = null;
		bool upper_inclusive = true; var start_loc = NextLocation;
		
		BitOr(out start);
		expr = start; 
		if (la.kind == 1 || la.kind == 2) {
			RangeOperator(ref upper_inclusive);
			BitOr(out end);
			if (IsIntSeqColon()) {
				Expect(3);
				BitOr(out step);
			}
			if(step == null) step = Expression.MakeConstant("int", 1, TextLocation.Empty);
			expr = Expression.MakeIntSeq(start, end, step, upper_inclusive, start_loc, CurrentEndLocation);
			
		}
	}

	void ComparisonOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		switch (la.kind) {
		case 98: {
			Get();
			opType = OperatorType.Equality; 
			break;
		}
		case 99: {
			Get();
			opType = OperatorType.InEquality; 
			break;
		}
		case 8: {
			Get();
			opType = OperatorType.LessThan; 
			break;
		}
		case 12: {
			Get();
			opType = OperatorType.GreaterThan; 
			break;
		}
		case 100: {
			Get();
			opType = OperatorType.LessThanOrEqual; 
			break;
		}
		case 101: {
			Get();
			opType = OperatorType.GreaterThanOrEqual; 
			break;
		}
		default: SynErr(177); break;
		}
	}

	void BitOr(out Expression expr) {
		Expression rhs; 
		BitXor(out expr);
		if (la.kind == 68) {
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
		} else SynErr(178);
	}

	void BitXor(out Expression expr) {
		Expression rhs; 
		BitAnd(out expr);
		if (la.kind == 102) {
			Get();
			BitXor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ExclusiveOr, expr, rhs); 
		}
	}

	void BitAnd(out Expression expr) {
		Expression rhs; 
		ShiftOp(out expr);
		if (la.kind == 53) {
			Get();
			BitAnd(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseAnd, expr, rhs); 
		}
	}

	void ShiftOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		AddOp(out expr);
		if (la.kind == 103 || la.kind == 104) {
			ShiftOperator(out type);
			ShiftOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AddOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		Term(out expr);
		if (la.kind == 105 || la.kind == 106) {
			AdditiveOperator(out type);
			AddOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void ShiftOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 103) {
			Get();
			opType = OperatorType.BitwiseShiftLeft; 
		} else if (la.kind == 104) {
			Get();
			opType = OperatorType.BitwiseShiftRight; 
		} else SynErr(179);
	}

	void Term(out Expression expr) {
		Expression rhs; OperatorType type; 
		PowerOp(out expr);
		if (la.kind == 107 || la.kind == 108 || la.kind == 109) {
			MultiplicativeOperator(out type);
			Term(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void AdditiveOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 105) {
			Get();
			opType = OperatorType.Plus; 
		} else if (la.kind == 106) {
			Get();
			opType = OperatorType.Minus; 
		} else SynErr(180);
	}

	void PowerOp(out Expression expr) {
		Expression rhs; 
		Factor(out expr);
		if (la.kind == 110) {
			Get();
			Factor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.Power, expr, rhs); 
		}
	}

	void MultiplicativeOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 107) {
			Get();
			opType = OperatorType.Times; 
		} else if (la.kind == 108) {
			Get();
			opType = OperatorType.Divide; 
		} else if (la.kind == 109) {
			Get();
			opType = OperatorType.Modulus; 
		} else SynErr(181);
	}

	void Factor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; var start_loc = NextLocation; 
		if (StartOf(29)) {
			Primary(out expr);
		} else if (StartOf(30)) {
			UnaryOperator(out type);
			Factor(out factor);
			expr = Expression.MakeUnaryExpr(type, factor, start_loc); 
		} else SynErr(182);
	}

	void Primary(out Expression expr) {
		expr = null; PathExpression path; AstType type_path = null; var type_args = new List<KeyValueType>(); 
		if (la.kind == 18) {
			PathExpression(out path);
			expr = path; 
			if (FollowsObjectCreation()) {
				GenericTypeArguments(ref type_args);
			}
			if (IsObjectCreation()) {
				type_path = ConvertPathToType(path); 
				ObjectCreation(type_path, type_args, out expr);
			}
		} else if (StartOf(31)) {
			Atom(out expr);
		} else SynErr(183);
		while (IsTrailer()) {
			Trailer(ref expr);
		}
		if (la.kind == 39) {
			Get();
			Type(out type_path);
			expr = Expression.MakeCastExpr(expr, type_path); 
		}
	}

	void UnaryOperator(out OperatorType opType) {
		opType = OperatorType.None; 
		if (la.kind == 105 || la.kind == 106) {
			AdditiveOperator(out opType);
		} else if (la.kind == 111) {
			Get();
			opType = OperatorType.Not; 
		} else if (la.kind == 53) {
			Get();
			opType = OperatorType.Reference; 
		} else SynErr(184);
	}

	void PathExpression(out PathExpression path) {
		var paths = new List<Identifier>(); var start_loc = NextLocation; 
		Expect(18);
		var name = t.val;
		if(CheckKeyword(name)){
		   path = null;
		   return;
		}
		var ident = AstNode.MakeIdentifier(name, AstType.MakePlaceholderType(CurrentEndLocation), ExpressoModifiers.None, start_loc);
		paths.Add(ident);
		
		while (la.kind == 4) {
			Get();
			start_loc = NextLocation; 
			Expect(18);
			name = t.val;
			if(CheckKeyword(name)){
			   path = null;
			   return;
			}
			var ident2 = AstNode.MakeIdentifier(name, AstType.MakePlaceholderType(CurrentEndLocation), ExpressoModifiers.None, start_loc);
			paths.Add(ident2);
			
		}
		path = Expression.MakePath(paths); 
	}

	void Atom(out Expression expr) {
		var exprs = new List<Expression>(); expr = null; bool seen_trailing_comma = false; var start_loc = NextLocation; 
		if (StartOf(22)) {
			Literal(out expr);
		} else if (la.kind == 7) {
			Get();
			if (la.kind == 10) {
				Get();
				expr = Expression.MakeParen(null, start_loc, CurrentEndLocation); 
			} else if (StartOf(17)) {
				CondExpr(out expr);
				exprs.Add(expr); 
				while (NotFinalComma()) {
					ExpectWeak(14, 32);
					CondExpr(out expr);
					exprs.Add(expr); 
				}
				if (la.kind == 14) {
					Get();
					seen_trailing_comma = true; 
				}
				Expect(10);
				if(exprs.Count == 1)
				   expr = Expression.MakeParen(seen_trailing_comma ? Expression.MakeSequenceExpression(exprs[0]) : exprs[0], start_loc, CurrentEndLocation);
				else
				   expr = Expression.MakeParen(Expression.MakeSequenceExpression(exprs), start_loc, CurrentEndLocation);
				
			} else SynErr(185);
		} else if (la.kind == 9) {
			Get();
			if (StartOf(33)) {
				SequenceMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 13)) {SynErr(186); Get();}
			Expect(13);
			if(expr == null){
			                     var type = CreateTypeWithArgs("array", AstType.MakePlaceholderType(start_loc));
			expr = Expression.MakeSequenceInitializer(type, Enumerable.Empty<Expression>(), start_loc, CurrentEndLocation);
			                 }
			
		} else if (la.kind == 6) {
			Get();
			if (StartOf(18)) {
				DictMaker(out expr);
			}
			while (!(la.kind == 0 || la.kind == 11)) {SynErr(187); Get();}
			Expect(11);
			if(expr == null){
			   var type = CreateTypeWithArgs("dictionary", AstType.MakePlaceholderType(start_loc), AstType.MakePlaceholderType(start_loc));
			   expr = Expression.MakeSequenceInitializer(type, Enumerable.Empty<Expression>(), start_loc, CurrentEndLocation);
			}
			
		} else SynErr(188);
	}

	void Trailer(ref Expression expr) {
		var args = new List<Expression>(); var start_loc = NextLocation; var type_args = new List<KeyValueType>(); 
		if (la.kind == 7 || la.kind == 8) {
			if (la.kind == 8) {
				GenericTypeArguments(ref type_args);
			}
			Expect(7);
			if (StartOf(17)) {
				ArgList(out args);
			}
			Expect(10);
			expr = Expression.MakeCallExpr(expr, args, type_args, CurrentEndLocation); 
		} else if (la.kind == 9) {
			Get();
			ArgList(out args);
			Expect(13);
			expr = Expression.MakeIndexer(expr, args, CurrentEndLocation); 
		} else if (la.kind == 15) {
			Get();
			Expect(18);
			expr = Expression.MakeMemRef(expr, AstNode.MakeIdentifier(t.val, AstType.MakePlaceholderType(CurrentEndLocation), ExpressoModifiers.None, start_loc)); 
		} else SynErr(189);
	}

	void PatternAndTest(out Expression expr) {
		Expression rhs; 
		PatternComparison(out expr);
		if (la.kind == 97) {
			Get();
			PatternAndTest(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ConditionalAnd, expr, rhs); 
		}
	}

	void PatternComparison(out Expression expr) {
		Expression rhs; OperatorType type = OperatorType.None; 
		PatternIntSeqExpr(out expr);
		if (StartOf(28)) {
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
			if (la.kind == 3) {
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
		if (la.kind == 68) {
			Get();
			PatternBitOr(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseOr, expr, rhs); 
		}
	}

	void PatternBitXor(out Expression expr) {
		Expression rhs; 
		PatternBitAnd(out expr);
		if (la.kind == 102) {
			Get();
			PatternBitXor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.ExclusiveOr, expr, rhs); 
		}
	}

	void PatternBitAnd(out Expression expr) {
		Expression rhs; 
		PatternShiftOp(out expr);
		if (la.kind == 53) {
			Get();
			PatternBitAnd(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.BitwiseAnd, expr, rhs); 
		}
	}

	void PatternShiftOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		PatternAddOp(out expr);
		if (la.kind == 103 || la.kind == 104) {
			ShiftOperator(out type);
			PatternShiftOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void PatternAddOp(out Expression expr) {
		Expression rhs; OperatorType type; 
		PatternTerm(out expr);
		if (la.kind == 105 || la.kind == 106) {
			AdditiveOperator(out type);
			PatternAddOp(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void PatternTerm(out Expression expr) {
		Expression rhs; OperatorType type; 
		PatternPowerOp(out expr);
		if (la.kind == 107 || la.kind == 108 || la.kind == 109) {
			MultiplicativeOperator(out type);
			PatternTerm(out rhs);
			expr = Expression.MakeBinaryExpr(type, expr, rhs); 
		}
	}

	void PatternPowerOp(out Expression expr) {
		Expression rhs; 
		PatternFactor(out expr);
		if (la.kind == 110) {
			Get();
			PatternFactor(out rhs);
			expr = Expression.MakeBinaryExpr(OperatorType.Power, expr, rhs); 
		}
	}

	void PatternFactor(out Expression expr) {
		OperatorType type; Expression factor; expr = null; var start_loc = NextLocation; 
		if (StartOf(34)) {
			PatternPrimary(out expr);
		} else if (StartOf(30)) {
			UnaryOperator(out type);
			PatternFactor(out factor);
			expr = Expression.MakeUnaryExpr(type, factor, start_loc); 
		} else SynErr(190);
	}

	void PatternPrimary(out Expression expr) {
		expr = null; PathExpression path; 
		if (la.kind == 18) {
			PathExpression(out path);
			expr = path; 
		} else if (StartOf(22)) {
			Literal(out expr);
		} else SynErr(191);
		while (IsTrailer()) {
			Trailer(ref expr);
		}
	}

	void SelfReferenceExpression(out Expression expr) {
		var start_loc = NextLocation; 
		Expect(115);
		expr = Expression.MakeSelfRef(start_loc);
		// Don't add self symbol because we only need one ParameterExpression instance per a type.
		//Symbols.AddSymbol(cur_class_name + "self", self_expr.SelfIdentifier);
		
	}

	void SuperReferenceExpression(out Expression expr) {
		var start_loc = NextLocation; 
		Expect(116);
		expr = Expression.MakeSuperRef(start_loc);
		// Don't add super symbol because we only need one ParameterExpression instance per a type.
		//Symbols.AddSymbol(cur_class_name + "super", super_expr.SuperIdentifier);
		
	}

	void ArgList(out List<Expression> args ) {
		args = new List<Expression>(); Expression expr; 
		CondExpr(out expr);
		args.Add(expr); 
		while (la.kind == 14) {
			Get();
			CondExpr(out expr);
			args.Add(expr); 
		}
	}

	void SequenceMaker(out Expression expr) {
		var exprs = new List<Expression>();
		expr = null; ComprehensionIter comp = null;
		string seq_type_name = "array"; var start_loc = NextLocation;
		
		if (la.kind == 2) {
			Get();
			expr = Expression.MakeSequenceInitializer(CreateTypeWithArgs("vector", AstType.MakePlaceholderType(start_loc)), Enumerable.Empty<Expression>(), start_loc, CurrentEndLocation); 
		} else if (StartOf(17)) {
			CondExpr(out expr);
			exprs.Add(expr); 
			if (la.kind == 13 || la.kind == 14) {
				while (NotFinalComma()) {
					ExpectWeak(14, 32);
					CondExpr(out expr);
					exprs.Add(expr); 
				}
				if (la.kind == 14) {
					Get();
					Expect(2);
					seq_type_name = "vector"; 
				}
				var type = CreateTypeWithArgs(seq_type_name, AstType.MakePlaceholderType(start_loc));
				expr = Expression.MakeSequenceInitializer(type, exprs, start_loc, CurrentEndLocation);
				
			} else if (la.kind == 26) {
				CompFor(out comp);
				var type = CreateTypeWithArgs("vector", AstType.MakePlaceholderType(start_loc));
				expr = Expression.MakeComp(expr, (ComprehensionForClause)comp, type);
				GoUpScope();
				is_first_comprehension_for_clause = true;
				
			} else SynErr(192);
		} else SynErr(193);
	}

	void DictMaker(out Expression expr) {
		Expression key, val; var list = new List<KeyValueLikeExpression>();
		      KeyValueLikeExpression pair; ComprehensionIter comp; expr = null; var start_loc = NextLocation;
		      var type = CreateTypeWithArgs("dictionary", AstType.MakePlaceholderType(start_loc), AstType.MakePlaceholderType(start_loc));
		   
		BitOr(out key);
		Expect(3);
		CondExpr(out val);
		pair = Expression.MakeKeyValuePair(key, val);
		list.Add(pair);
		
		if (la.kind == 11 || la.kind == 14) {
			while (WeakSeparator(14,18,35) ) {
				BitOr(out key);
				Expect(3);
				CondExpr(out val);
				pair = Expression.MakeKeyValuePair(key, val);
				list.Add(pair);
				
			}
			expr = Expression.MakeSequenceInitializer(type, list, start_loc, CurrentEndLocation); 
		} else if (la.kind == 26) {
			CompFor(out comp);
			expr = Expression.MakeComp(pair, (ComprehensionForClause)comp, type);
			GoDownScope();
			is_first_comprehension_for_clause = true;
			
		} else SynErr(194);
	}

	void CompFor(out ComprehensionIter expr) {
		Expression rvalue = null; ComprehensionIter body = null; PatternWithType typed_pattern; 
		Expect(26);
		if(is_first_comprehension_for_clause){
		Symbols.AddScope();
		GoDownScope();
		Symbols.Name = "Comprehension`" + ScopeId++;
		is_first_comprehension_for_clause = false;
		}
		
		PatternWithType(out typed_pattern);
		Expect(88);
		CondExpr(out rvalue);
		if (la.kind == 26 || la.kind == 84) {
			CompIter(out body);
		}
		expr = Expression.MakeCompFor(typed_pattern, rvalue, body);
		
	}

	void CompIter(out ComprehensionIter expr) {
		expr = null; 
		if (la.kind == 26) {
			CompFor(out expr);
		} else if (la.kind == 84) {
			CompIf(out expr);
		} else SynErr(195);
	}

	void CompIf(out ComprehensionIter expr) {
		Expression tmp; ComprehensionIter body = null; 
		Expect(84);
		OrTest(out tmp);
		if (la.kind == 26 || la.kind == 84) {
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

	public Expression ParseExpression()
	{
		la = new Token();
		la.val = "";
		Get();

		Expression expr = null;
		CondExpr(out expr);
		Expect(0);
		return expr;
	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_T, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_T,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_T,_T, _x,_x,_x,_x, _T,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _T,_x,_x,_x, _T,_T,_x,_T, _T,_T,_T,_T, _T,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_T,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_T, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_T, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_x,_x},
		{_T,_T,_x,_x, _x,_T,_x,_T, _x,_T,_x,_T, _x,_T,_T,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_T,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_x,_x},
		{_T,_x,_x,_x, _x,_T,_T,_T, _x,_T,_x,_T, _x,_T,_T,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_T,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_T,_x, _x,_x,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x}

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
			case 3: s = "colon expected"; break;
			case 4: s = "double_colon expected"; break;
			case 5: s = "semicolon expected"; break;
			case 6: s = "lcurly expected"; break;
			case 7: s = "lparen expected"; break;
			case 8: s = "langle_bracket expected"; break;
			case 9: s = "lbracket expected"; break;
			case 10: s = "rparen expected"; break;
			case 11: s = "rcurly expected"; break;
			case 12: s = "rangle_bracket expected"; break;
			case 13: s = "rbracket expected"; break;
			case 14: s = "comma expected"; break;
			case 15: s = "dot expected"; break;
			case 16: s = "thick_arrow expected"; break;
			case 17: s = "equal expected"; break;
			case 18: s = "ident expected"; break;
			case 19: s = "integer expected"; break;
			case 20: s = "float expected"; break;
			case 21: s = "hex_digit expected"; break;
			case 22: s = "unicode_escape expected"; break;
			case 23: s = "character_literal expected"; break;
			case 24: s = "string_literal expected"; break;
			case 25: s = "raw_string_literal expected"; break;
			case 26: s = "keyword_for expected"; break;
			case 27: s = "\"export\" expected"; break;
			case 28: s = "\"asm\" expected"; break;
			case 29: s = "\"module\" expected"; break;
			case 30: s = "\"class\" expected"; break;
			case 31: s = "\"enum\" expected"; break;
			case 32: s = "\"fld\" expected"; break;
			case 33: s = "\"meth\" expected"; break;
			case 34: s = "\"param\" expected"; break;
			case 35: s = "\"return\" expected"; break;
			case 36: s = "\"#[\" expected"; break;
			case 37: s = "\"import\" expected"; break;
			case 38: s = "\"from\" expected"; break;
			case 39: s = "\"as\" expected"; break;
			case 40: s = "\"interface\" expected"; break;
			case 41: s = "\"def\" expected"; break;
			case 42: s = "\"->\" expected"; break;
			case 43: s = "\"public\" expected"; break;
			case 44: s = "\"protected\" expected"; break;
			case 45: s = "\"private\" expected"; break;
			case 46: s = "\"static\" expected"; break;
			case 47: s = "\"mutating\" expected"; break;
			case 48: s = "\"override\" expected"; break;
			case 49: s = "\"(-\" expected"; break;
			case 50: s = "\"let\" expected"; break;
			case 51: s = "\"var\" expected"; break;
			case 52: s = "\"where\" expected"; break;
			case 53: s = "\"&\" expected"; break;
			case 54: s = "\"int\" expected"; break;
			case 55: s = "\"uint\" expected"; break;
			case 56: s = "\"bool\" expected"; break;
			case 57: s = "\"float\" expected"; break;
			case 58: s = "\"double\" expected"; break;
			case 59: s = "\"bigint\" expected"; break;
			case 60: s = "\"string\" expected"; break;
			case 61: s = "\"byte\" expected"; break;
			case 62: s = "\"char\" expected"; break;
			case 63: s = "\"vector\" expected"; break;
			case 64: s = "\"dictionary\" expected"; break;
			case 65: s = "\"slice\" expected"; break;
			case 66: s = "\"intseq\" expected"; break;
			case 67: s = "\"void\" expected"; break;
			case 68: s = "\"|\" expected"; break;
			case 69: s = "\"break\" expected"; break;
			case 70: s = "\"upto\" expected"; break;
			case 71: s = "\"continue\" expected"; break;
			case 72: s = "\"yield\" expected"; break;
			case 73: s = "\"throw\" expected"; break;
			case 74: s = "\"+=\" expected"; break;
			case 75: s = "\"-=\" expected"; break;
			case 76: s = "\"*=\" expected"; break;
			case 77: s = "\"/=\" expected"; break;
			case 78: s = "\"**=\" expected"; break;
			case 79: s = "\"%=\" expected"; break;
			case 80: s = "\"&=\" expected"; break;
			case 81: s = "\"|=\" expected"; break;
			case 82: s = "\"<<=\" expected"; break;
			case 83: s = "\">>=\" expected"; break;
			case 84: s = "\"if\" expected"; break;
			case 85: s = "\"else\" expected"; break;
			case 86: s = "\"while\" expected"; break;
			case 87: s = "\"do\" expected"; break;
			case 88: s = "\"in\" expected"; break;
			case 89: s = "\"match\" expected"; break;
			case 90: s = "\"try\" expected"; break;
			case 91: s = "\"catch\" expected"; break;
			case 92: s = "\"finally\" expected"; break;
			case 93: s = "\"?\" expected"; break;
			case 94: s = "\"_\" expected"; break;
			case 95: s = "\"@\" expected"; break;
			case 96: s = "\"||\" expected"; break;
			case 97: s = "\"&&\" expected"; break;
			case 98: s = "\"==\" expected"; break;
			case 99: s = "\"!=\" expected"; break;
			case 100: s = "\"<=\" expected"; break;
			case 101: s = "\">=\" expected"; break;
			case 102: s = "\"^\" expected"; break;
			case 103: s = "\"<<\" expected"; break;
			case 104: s = "\">>\" expected"; break;
			case 105: s = "\"+\" expected"; break;
			case 106: s = "\"-\" expected"; break;
			case 107: s = "\"*\" expected"; break;
			case 108: s = "\"/\" expected"; break;
			case 109: s = "\"%\" expected"; break;
			case 110: s = "\"**\" expected"; break;
			case 111: s = "\"!\" expected"; break;
			case 112: s = "\"true\" expected"; break;
			case 113: s = "\"false\" expected"; break;
			case 114: s = "\"null\" expected"; break;
			case 115: s = "\"self\" expected"; break;
			case 116: s = "\"super\" expected"; break;
			case 117: s = "??? expected"; break;
			case 118: s = "invalid ModuleBody"; break;
			case 119: s = "invalid ModuleBody"; break;
			case 120: s = "this symbol not expected in ModuleNameDefinition"; break;
			case 121: s = "this symbol not expected in FuncDecl"; break;
			case 122: s = "this symbol not expected in FieldDecl"; break;
			case 123: s = "invalid FieldDecl"; break;
			case 124: s = "this symbol not expected in FieldDecl"; break;
			case 125: s = "this symbol not expected in ClassDecl"; break;
			case 126: s = "invalid ClassDecl"; break;
			case 127: s = "this symbol not expected in ClassDecl"; break;
			case 128: s = "this symbol not expected in InterfaceDecl"; break;
			case 129: s = "this symbol not expected in EnumDecl"; break;
			case 130: s = "invalid EnumDecl"; break;
			case 131: s = "this symbol not expected in EnumDecl"; break;
			case 132: s = "invalid AttributeTarget"; break;
			case 133: s = "invalid ObjectCreation"; break;
			case 134: s = "this symbol not expected in ObjectCreation"; break;
			case 135: s = "this symbol not expected in ImportDecl"; break;
			case 136: s = "invalid ImportDecl"; break;
			case 137: s = "this symbol not expected in ImportDecl"; break;
			case 138: s = "invalid ImportPaths"; break;
			case 139: s = "invalid ImportPaths"; break;
			case 140: s = "invalid ImportPaths"; break;
			case 141: s = "invalid Type"; break;
			case 142: s = "invalid Type"; break;
			case 143: s = "this symbol not expected in MethodSignature"; break;
			case 144: s = "this symbol not expected in MethodSignature"; break;
			case 145: s = "invalid Modifiers"; break;
			case 146: s = "this symbol not expected in Block"; break;
			case 147: s = "invalid Literal"; break;
			case 148: s = "invalid Stmt"; break;
			case 149: s = "invalid SimpleStmt"; break;
			case 150: s = "invalid CompoundStmt"; break;
			case 151: s = "this symbol not expected in ExprStmt"; break;
			case 152: s = "invalid VarDeclStmt"; break;
			case 153: s = "this symbol not expected in VarDeclStmt"; break;
			case 154: s = "this symbol not expected in ReturnStmt"; break;
			case 155: s = "this symbol not expected in BreakStmt"; break;
			case 156: s = "this symbol not expected in ContinueStmt"; break;
			case 157: s = "this symbol not expected in YieldStmt"; break;
			case 158: s = "this symbol not expected in ThrowStmt"; break;
			case 159: s = "this symbol not expected in EmptyStmt"; break;
			case 160: s = "invalid CondExpr"; break;
			case 161: s = "invalid AugmentedAssignOperators"; break;
			case 162: s = "invalid LhsPrimary"; break;
			case 163: s = "invalid IfStmt"; break;
			case 164: s = "this symbol not expected in DoWhileStmt"; break;
			case 165: s = "invalid ForStmt"; break;
			case 166: s = "invalid PatternWithType"; break;
			case 167: s = "this symbol not expected in MatchPatternList"; break;
			case 168: s = "this symbol not expected in PatternList"; break;
			case 169: s = "invalid MatchArmStmt"; break;
			case 170: s = "invalid Pattern"; break;
			case 171: s = "invalid TuplePattern"; break;
			case 172: s = "invalid DestructuringPattern"; break;
			case 173: s = "invalid DestructuringPattern"; break;
			case 174: s = "invalid DestructuringPattern"; break;
			case 175: s = "invalid TupleElementPattern"; break;
			case 176: s = "invalid ClosureLiteral"; break;
			case 177: s = "invalid ComparisonOperator"; break;
			case 178: s = "invalid RangeOperator"; break;
			case 179: s = "invalid ShiftOperator"; break;
			case 180: s = "invalid AdditiveOperator"; break;
			case 181: s = "invalid MultiplicativeOperator"; break;
			case 182: s = "invalid Factor"; break;
			case 183: s = "invalid Primary"; break;
			case 184: s = "invalid UnaryOperator"; break;
			case 185: s = "invalid Atom"; break;
			case 186: s = "this symbol not expected in Atom"; break;
			case 187: s = "this symbol not expected in Atom"; break;
			case 188: s = "invalid Atom"; break;
			case 189: s = "invalid Trailer"; break;
			case 190: s = "invalid PatternFactor"; break;
			case 191: s = "invalid PatternPrimary"; break;
			case 192: s = "invalid SequenceMaker"; break;
			case 193: s = "invalid SequenceMaker"; break;
			case 194: s = "invalid DictMaker"; break;
			case 195: s = "invalid CompIter"; break;

			default: s = "error " + n; break;
		}
		
		var prev_color = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		errorStream.WriteLine(errMsgFormat, line, col, s);
		Console.ForegroundColor = prev_color;
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		var prev_color = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		errorStream.WriteLine(errMsgFormat, line, col, s);
		Console.ForegroundColor = prev_color;
		count++;
	}
	
	public virtual void SemErr (string s) {
		var prev_color = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		errorStream.WriteLine(s);
		Console.ForegroundColor = prev_color;
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		var prev_color = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Yellow;
		errorStream.WriteLine(errMsgFormat, line, col, s);
		Console.ForegroundColor = prev_color;
	}
	
	public virtual void Warning(string s) {
		var prev_color = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Yellow;
		errorStream.WriteLine(s);
		Console.ForegroundColor = prev_color;
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}