using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Expresso.Ast;
using Expresso.Ast.Analysis;
using ICSharpCode.NRefactory;

namespace Expresso.CodeGen
{
    /// <summary>
    /// Contains helper methods for native symbols.
    /// </summary>
    public static class ExpressoCompilerHelpers
    {
        static List<string> _AssemblyNames =
            new List<string>{"System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "ExpressoRuntime"};
        
        internal static uint StartOfIdentifierId = 1_000_000_002u;
        internal static Guid LanguageGuid = Guid.Parse("408e5e88-0566-4b8a-9c69-4d2f7c74baf9");
        static uint IdentifierId = StartOfIdentifierId + 16u;
        const string TypePrefix = "type_";
        static readonly string[] IgnoreList = {"equals", "getHashCode"};

        static Dictionary<string, Tuple<string, uint>> SpecialNamesMapInverse = new Dictionary<string, Tuple<string, uint>>{
            {"Expresso.Runtime.Builtins.ExpressoIntegerSequence", Tuple.Create("intseq", StartOfIdentifierId + 0)},
            {"Expresso.Runtime.Builtins.Slice", Tuple.Create("slice", StartOfIdentifierId + 1u)},
            {"System.Func", Tuple.Create("function", StartOfIdentifierId + 2u)},
            {"System.Boolean", Tuple.Create("bool", StartOfIdentifierId + 3u)},
            {"System.Int32", Tuple.Create("int", StartOfIdentifierId + 4u)},
            {"System.UInt32", Tuple.Create("uint", StartOfIdentifierId + 5u)},
            {"System.Single", Tuple.Create("float", StartOfIdentifierId + 6u)},
            {"System.Double", Tuple.Create("double", StartOfIdentifierId + 7u)},
            {"System.Char", Tuple.Create("char", StartOfIdentifierId + 8u)},
            {"System.Byte", Tuple.Create("byte", StartOfIdentifierId + 9u)},
            {"System.String", Tuple.Create("string", StartOfIdentifierId + 10u)},
            {"System.Array", Tuple.Create("array", StartOfIdentifierId + 11u)},
            {"System.Collections.Generic.List", Tuple.Create("vector", StartOfIdentifierId + 12u)},
            {"System.Tuple", Tuple.Create("tuple", StartOfIdentifierId + 13u)},
            {"System.Collections.Generic.Dictionary", Tuple.Create("dictionary", StartOfIdentifierId + 14u)},
            {"System.Numerics.BigInteger", Tuple.Create("bigint", StartOfIdentifierId + 15u)}
        };

        public static string GetExpressoTypeName(string csharpFullName)
        {
            //var start_index = (csharpFullName.LastIndexOf('.') != -1) ? csharpFullName.LastIndexOf('.') : 0;
            var backquote_index = csharpFullName.IndexOf('`');
            var csharp_name = csharpFullName.Substring(/*start_index*/ 0, (backquote_index == -1) ? csharpFullName.Length/* - start_index*/ : backquote_index/* - start_index*/);
            if(SpecialNamesMapInverse.ContainsKey(csharp_name))
                return SpecialNamesMapInverse[csharp_name].Item1;
            else
                return csharpFullName;
        }

        public static PrimitiveType GetPrimitiveAstType(string type)
        {
            var type_code = PrimitiveType.GetActualKnownTypeCodeForPrimitiveType(type, null);
            if(type_code == TypeSystem.KnownTypeCode.None)
                return null;
            
            return AstType.MakePrimitiveType(type);
        }

        public static void AddPrimitiveTypesSymbolTables(SymbolTable table)
        {
            foreach(var primitive in SpecialNamesMapInverse){
                table.AddTypeSymbol(primitive.Value.Item1, AstType.MakePrimitiveType(primitive.Value.Item1));
                table.GetTypeSymbol(primitive.Value.Item1).IdentifierId = primitive.Value.Item2;
                AddNativeSymbolTable(AstNode.MakeIdentifier(primitive.Key), table);
            }
        }

        public static void AddNativeSymbolTable(Identifier identifier, SymbolTable table)
        {
            Type type = null;
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            var regex = new Regex($"{identifier.Name}(?![a-zA-Z0-9])");
            foreach(var asm in asms){
                var types = GetTypes(asm);
                type = types.Where(t => regex.IsMatch(t.FullName))
                            .FirstOrDefault();
                if(type != null)
                    break;
            }
            if(type == null){
                throw new ParserException(
                    "The type '{0}' is not a native type.",
                    "ES5000",
                    identifier,
                    identifier.Name
                ){
                    HelpObject = identifier.Name.StartsWith("System", StringComparison.Ordinal)
                };
            }
                
            PopulateSymbolTable(table, type);
        }

        /// <summary>
        /// Stringifies a list of items.
        /// </summary>
        /// <returns>The list.</returns>
        /// <param name="enumerable">Enumerable.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static string StringifyList<T>(IEnumerable<T> enumerable)
        {
            if(!enumerable.Any())
                return string.Empty;
            
            var first_item = enumerable.First();
            var builder = new StringBuilder(first_item.ToString());
            foreach(var item in enumerable.Skip(1)){
                builder.Append(", ");
                builder.Append(item.ToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Loads appropriate assemblies to prepare for code analyzing.
        /// </summary>
        public static void Prepare()
        {
            foreach(var name in _AssemblyNames){
                var an = new AssemblyName(name);
                Assembly.Load(an);
            }
        }

        internal static void DisplayHelp(ParserException e)
        {
            var error_number = e.ErrorCode.Substring(2);
            if(error_number == "1000" || error_number == "1022" || error_number == "2000")
                return;
            
            var prev_color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("help: ");

            switch(error_number){
            case "0001":
                Console.WriteLine("Otherwise you can't receive all the parameters.");
                break;

            case "0002":
                Console.WriteLine("Because we don't have named parameters it isn't allowed.");
                break;

            case "0003":
                Console.WriteLine("I can't do nothing if there is no values from which I'll infer the type of the expression.");
                break;

            case "0004":
                Console.WriteLine("At least one of them have to be placed so that the compiler can detemine the type of the parameter.");
                break;

            case "0005":
                Console.WriteLine("Otherwise I can't determine where the string ends.");
                break;

            /*case "0006":
                Console.WriteLine("");
                break;

            case "0007":
                Console.WriteLine("Unknown type is just unknown!");
                break;*/

            case "0008":
                Console.WriteLine("Otherwise some values won't be assigned.");
                break;

            case "0009":
                Console.WriteLine("For a keyword list, see the online documentation.");
                break;

            case "0010":
                Console.WriteLine("This isn't due to a technical issue.");
                break;

            case "0011":
                Console.WriteLine("This is obviously a restriction but supporting it will definitly make the implementation more complex.");
                break;

            case "0020":
                Console.WriteLine("Please make sure that the file exists.");
                break;

            case "0021":
                Console.WriteLine("Other patterns don't make sense, do they?");
                break;

            /*case "0040":
                Console.WriteLine("See the C# documentation for the format of uint literals.");
                break;

            case "0050":
                Console.WriteLine("See the C# documentation for the format of float literals.");
                break;*/

            case "0051":
                Console.WriteLine("See the C# documentation for the format of number literals.");
                break;

            case "0100":
                Console.WriteLine("It's most likely that you misspelled the identifier name.");
                Console.WriteLine("So check for spelling, taking casing into account.");
                break;

            case "0101":
                Console.WriteLine("It's most likely that you misspelled the type symbol.");
                Console.WriteLine("So check for spelling, taking casing into account.");
                break;

            case "0102":
                Console.WriteLine("It's most likely that you misspelled the symbol.");
                Console.WriteLine("So check for spelling, taking casing into account.");
                break;

            case "0103":
                {
                    var is_importing_system = (bool)e.HelpObject;
                    if(is_importing_system){
                        Console.WriteLine("To import names from the System namespace(that is, from .NET), you don't have to write the from clause.");
                    }else{
                        Console.WriteLine("It's most likely that you misspelled the symbol name.");
                        Console.WriteLine("So check for spelling, taking casing into account.");
                    }
                }
                break;

            case "0110":
                Console.WriteLine("Can not cast the value to the field's type.");
                Console.WriteLine("With explicit casting, it may pass compilation.");
                break;

            case "0200":
                Console.WriteLine("Please make sure that the variable is initialized.");
                break;

            case "1000":
                break;

            case "1001":
                Console.WriteLine("The variable or field should be compatible with the value on the right-hand-side when assigned.");
                Console.WriteLine("In this context, 'compatible' means that the types of the both values match");
                Console.WriteLine("or the right-hand-side can be implicitly casted to the left-hand-side.");
                break;

            case "1002":
                Console.WriteLine("For combinations of the operators and the types that can be used with them, see the online documentation.");
                break;

            case "1003":
                Console.WriteLine("Aren't you trying to cast an object to the type the class isn't derived from?");
                Console.WriteLine("For combinations in which primitive types can be casted, see the online documentation.");
                break;

            case "1006":
                Console.WriteLine("Every expression has one type. So the conditional expression is.");
                Console.WriteLine("That's why we should determine the one type that the conditional expression will be.");
                break;

            case "1020":
                Console.WriteLine("Use module-level functions instead.");
                break;

            case "1021":
                Console.WriteLine("Use module-level functions instead.");
                break;

            case "1022":
                break;

            case "1023":
                Console.WriteLine("The value against which the value is tested must be exactly one type.");
                Console.WriteLine("Otherwise the compiler issues an compiler error at that point.");
                break;

            case "1030":
                Console.WriteLine("It's because methods that implement interfaces should have exactly the same signatures.");
                break;

            case "1100":
                Console.WriteLine("The variable or field should be compatible with the value on the right-hand-side when assigned.");
                Console.WriteLine("In this context, 'compatible' means that the types of the both values match");
                Console.WriteLine("or the right-hand-side can be implicitly casted to the left-hand-side.");
                break;

            case "1101":
                Console.WriteLine("This is an error because we can't take those functions into account.");
                break;

            case "1110":
                Console.WriteLine("Please specify a value with type `{0}`.", e.HelpObject);
                break;

            case "1200":
                Console.WriteLine("The operand must be of type `bool`.");
                break;

            case "1201":
                Console.WriteLine("For combinations of the operators and types that can be used with the operators, see the online documentation.");
                break;

            case "1202":
                Console.WriteLine("This is a warning because this fact by itself doesn't cause any problems.");
                break;

            case "1203":
                Console.WriteLine("This is a warning because this fact by itself doesn't cause any problems.");
                break;

            case "1300":
                Console.WriteLine("This generally means that you have no ways to make it runnable as it is.");
                break;

            case "1301":
                Console.WriteLine("For statemants can only be used for iterating over a sequence.");
                break;

            case "1302":
                Console.WriteLine("A comprehension expects a sequence object.");
                break;

            case "1303":
                Console.WriteLine("Expresso doesn't implicitly cast objects to other types.");
                Console.WriteLine("So consider adding an explicit casting.");
                break;

            case "1306":
                Console.WriteLine("Have you changed the expression? Because it's most likely to cause the problem.");
                Console.WriteLine("Of course, you can use type inference!");
                break;

            case "1310":
                Console.WriteLine("In Expresso you can only omit parameter types when there are optional values.");
                break;

            case "1311":
                Console.WriteLine("Without the initial value, I have no ways to infer the type of this variable!");
                break;

            case "1312":
                Console.WriteLine("Consider specifying the initial value or adding the type annotation.");
                break;

            case "1500":
                Console.WriteLine("It's most likely that you misspelled it.");
                Console.WriteLine("So check for spelling, taking casing into account.");
                break;

            case "1501":
                Console.WriteLine("Did you forget to import the module that the type was defined?");
                Console.WriteLine("Also you can check for spelling. Don't forget to take casing into account.");
                break;

            case "1502":
                Console.WriteLine("It's most likely that you misspelled the field name. So check for spelling, taking casing into account.");
                break;

            case "1602":
                Console.WriteLine("Unlike methods and functions, you may not omit the return types of method signatures in interfaces.");
                Console.WriteLine("This is due to the trouble implementing it and because interfaces should be explicit.");
                break;

            case "1700":
                Console.WriteLine("It's most likely that you forget to import some module.");
                Console.WriteLine("Or maybe you misspelled the type or symbol name. Check for spelling, taking casing into account.");
                break;

            case "1800":
                Console.WriteLine("Otherwise the return type of the function becomes void.");
                break;

            case "1805":
                Console.WriteLine("In Expresso callables include functions and closures.");
                break;

            case "1900":
                Console.WriteLine("Immutable variables are immutable becuase it's how it is.");
                Console.WriteLine("If you absolutely need to change the value, then consider changing the `let` keyword to `var` in the variable declaration.");
                Console.WriteLine("But think twice before doing so because mutability can cause evil things.");
                break;

            case "1901":
                Console.WriteLine("You can't omit the return type when there is no statements in the body!");
                break;

            case "1902":
                Console.WriteLine("Immutable fields can't be assigned values more than 1 time because it is the fate that the fields are imposed.");
                break;

            case "1910":
                Console.WriteLine("A derived class must implement all the functions that an interface defines.");
                break;

            case "1911":
                Console.WriteLine("In Expresso you can't derive classes from primitive types like int, uint and intseq");
                break;

            case "1912":
                Console.WriteLine("It's most likely that the type you are deriving is not exported.");
                Console.WriteLine("So consider adding the `export` modifier to the type.");
                break;

            case "2000":
                break;

            case "2001":
                Console.WriteLine("It's most likely that you misspelled the field or the property name. So check for spelling, taking casing into account.");
                break;

            case "2002":
                Console.WriteLine("It's most likely that you misspelled the method name. So check for spelling, taking casing into account.");
                Console.WriteLine("The second most likely is you specifying invalid signatures. So check for the argument' types.");
                break;

            case "2003":
                Console.WriteLine("It's most likely that you forgot to cast the object to the field type.");
                Console.WriteLine("Consider adding one.");
                break;

            case "2010":
                Console.WriteLine("Types are mismatched. Did you forget to cast arguments?");
                break;

            case "2100":
                Console.WriteLine("You can't call a mutable method on an immutable variable because it modifies the state of the object.");
                Console.WriteLine("If you absolutely need to do so, consider changing the `let` keyword to `var` in the variable declaration.");
                break;

            case "3000":
                Console.WriteLine("Currently you may not understand the cause of the problem. It's due to the weakness in reporting a string interpolation error.");
                Console.WriteLine("In future versions, it would be more clear what is problems in string interpolations.");
                break;

            case "3010":
                Console.WriteLine("In Expresso you can only omit the parameter and return types if and only if they are directly passed to functions or methods that take closures.");
                Console.WriteLine("This is obviously a restriction of the current implementation. So it may be eased off in the future.");
                break;

            case "3011":
                Console.WriteLine("In Expresso you can only apply the indexer operator on `vectors`, `arrays` and `dictionaries`");
                break;

            case "3012":
                Console.WriteLine("You can only use an intseq with the indexer operator on arrays or vectors.");
                break;

            case "3013":
                Console.WriteLine("In Expresso you can't use the indexer operator on an arbitrary object.");
                break;

            case "3014":
                Console.WriteLine("Currently Expresso doesn't allow you to declare more than 1 variable with the same name in the same scope.");
                Console.WriteLine("But in the future you would be able to use shadowing, though.");
                break;

            case "3200":
                Console.WriteLine("For combinations of the unary operators and the types, see the online documentation.");
                break;

            case "3300":
                Console.WriteLine("That is what it is.");
                break;

            case "3301":
                Console.WriteLine("That is what it is.");
                break;

            case "3303":
                Console.WriteLine("Consider adding `export` flag to the type.");
                break;

            case "4000":
                Console.WriteLine("Unfortunately for you, it's illegal in Expresso to write an expression that's evaluated to other types than `bool`");
                Console.WriteLine("in a conditional expression of an if statement.");
                break;

            case "4001":
                Console.WriteLine("An integer sequence expression expects an `Int`.");
                break;

            case "4002":
                Console.WriteLine("You are trying to pass {0} to the intseq constructor.", e.HelpObject);
                break;

            case "4003":
                Console.WriteLine("If you mean that the integer underflows and then reach `upper`, you can safely ignore this warning.");
                Console.WriteLine("I'm displaying this warning because it entirely consists of literal expressions.");
                Console.WriteLine("Otherwise I can't show this warning due to a technical difficulty.");
                break;

            case "4004":
                Console.WriteLine("If you mean that the integer overflows and then reach `upper`, you can safely ignore this warning.");
                Console.WriteLine("I'm displaying this warning because it entirely consists of literal expressions.");
                Console.WriteLine("Otherwise I can't show this warning due to a technical difficulty.");
                break;

            case "4005":
                Console.WriteLine("If you are making an inifinite series of integers, you can safely ignore this warning.");
                Console.WriteLine("I'm displaying this warning because it entirely consists of literal expressions.");
                Console.WriteLine("Otherwise I can't show this warning due to a technical difficulty.");
                break;

            case "4006":
                Console.WriteLine("Usually this is not an intended behavior. So consider changing either `lower` or `upper`.");
                Console.WriteLine("I'm displaying this warning because it entirely consists of literal expressions.");
                Console.WriteLine("Otherwise I can't show this warning due to a technical difficulty.");
                break;

            case "4010":
                Console.WriteLine("You can break out of loops {0} times at this point.", e.HelpObject);
                break;

            case "4011":
                Console.WriteLine("You can continue out of loops {0} times at this point.", e.HelpObject);
                break;

            case "4020":
                Console.WriteLine("The module that has the main function is usually called 'main' because it is the entry point to the program.");
                break;

            case "4021":
                Console.WriteLine("In this context, the following targets are allowed: {0}.", e.HelpObject);
                break;

            case "4022":
                Console.WriteLine("Arguments for attributes must be compile-time constants.");
                Console.WriteLine("Compile-time constants mean that there should be no object constructions nor function calls.");
                break;

            case "5000":
                {
                    var is_importing_system = (bool)e.HelpObject;
                    if(is_importing_system){
                        Console.WriteLine("It seems that you are just misspelling it. Check spelling, taking casing into account.");
                    }else{
                        Console.WriteLine("Did you forget adding the from clause? To import names from external files, you must add the from clause after the import clause.");
                        Console.WriteLine("The from clause tells the compiler about a file that it could find names.");
                        Console.WriteLine("It should be a relative path to this source file.");
                    }
                }
                break;

            default:
                throw new InvalidOperationException("Unreachable");
            }

            Console.ForegroundColor = prev_color;
        }

        public static SymbolTable GetSymbolTableForAssembly(string assemblyPath)
        {
            var asm = Assembly.LoadFrom(assemblyPath);
            var root_table = new SymbolTable();

            foreach(var type in asm.GetTypes()){
                PopulateSymbolTable(root_table, type);
                root_table.AddTypeSymbol(type.FullName, AstNode.MakeIdentifier(type.FullName, Modifiers.Export));
            }

            return root_table;
        }

        static void PopulateSymbolTable(SymbolTable table, Type type)
        {
            var type_name = type.Name;
            var expresso_type_name = GetExpressoTypeName(type.FullName);
            var expresso_name_builder = new StringBuilder(expresso_type_name);
            if(type_name.StartsWith("<>", StringComparison.CurrentCulture)){
                // ignore compiler-generated classes
                return;
            }

            var converted_name = new StringBuilder(TypePrefix + expresso_type_name);

            var type_args = new List<AstType>();
            if(type.IsGenericType){
                converted_name.Append("`");
                expresso_name_builder.Append("`");
                bool first = true;
                foreach(var type_arg in type.GetGenericArguments()){
                    if(first){
                        first = false;
                    }else{
                        converted_name.Append(", ");
                        expresso_name_builder.Append(", ");
                    }

                    converted_name.Append(type_arg.Name);
                    expresso_name_builder.Append(type_arg.Name);
                    type_args.Add(AstType.MakeParameterType(type_arg.Name));
                }
            }

            var converted_name_full = converted_name.ToString();
            var expresso_type_name_full = expresso_name_builder.ToString();
            var new_table = table.Children.Where(t => t.Name == converted_name_full).FirstOrDefault();
            bool table_was_created = new_table != null;
            new_table = new_table ?? new SymbolTable(ClassType.Class, true);
            new_table.Name = converted_name_full;

            if(type.IsEnum){
                var full_name = type.FullName;
                foreach(var enum_name in type.GetEnumNames()){
                    var ident = AstNode.MakeIdentifier(enum_name, AstType.MakeSimpleType(AstNode.MakeIdentifier(type_name, AstType.MakeSimpleType(full_name))));
                    ident.IdentifierId = IdentifierId++;
                    new_table.AddSymbol(enum_name, ident);
                }
            }else{
                // FIXME?: Think about changing the property methods' type
                foreach(var public_method in type.GetMethods()){
                    var method_name = public_method.Name;
                    if(IgnoreList.Contains(method_name) || method_name.StartsWith("op_", StringComparison.CurrentCulture))
                        continue;

                    var return_type = GetExpressoType(public_method.ReturnType);
                    var param_types =
                        from param in public_method.GetParameters()
                                                   select GetExpressoType(param.ParameterType);
                    var method_type = AstType.MakeFunctionType(method_name, return_type, param_types);
                    new_table.AddSymbol(method_name, method_type);

                    new_table.GetSymbol(method_name, method_type).IdentifierId = IdentifierId++;
                }

                foreach(var ctor in type.GetConstructors()){
                    var name = "constructor";

                    var return_type = AstType.MakeSimpleType("tuple");
                    var param_types =
                        from p in ctor.GetParameters()
                                      select GetExpressoType(p.ParameterType);
                    var ctor_type = AstType.MakeFunctionType(name, return_type, param_types);
                    new_table.AddSymbol(name, ctor_type);
                    new_table.GetSymbol(name, ctor_type).IdentifierId = IdentifierId++;
                }
            }

            if(!table_was_created){
                new_table.Parent = table;
                table.Children.Add(new_table);
                // Don't add a type symbol here bacause the Parser class has already done that
                //table.AddTypeSymbol(expresso_type_name_full, AstType.MakeSimpleType(expresso_type_name, type_args));
            }
        }

        static AstType GetExpressoType(Type type)
        {
            var name = type.FullName ?? type.Name;
            var index = name.IndexOf("`", StringComparison.CurrentCulture);
            var actual_type_name = name.Substring(0, (index == -1) ? name.Length : index);
            var type_name = SpecialNamesMapInverse.ContainsKey(actual_type_name) ? SpecialNamesMapInverse[actual_type_name].Item1 : type.Name;

            if(type_name == "Void")
                return AstType.MakeSimpleType("tuple");

            // This code is needed because in C# arrays are represented as *[] where * is any type
            // That is, arrays in C# isn't array type
            if(type.IsArray)
                return AstType.MakeSimpleType("array", TextLocation.Empty, TextLocation.Empty, GetExpressoType(type.GetElementType()));

            var primitive = GetPrimitiveAstType(type_name);
            if(primitive != null)
                return primitive;

            if(type.IsGenericParameter)
                return AstType.MakeParameterType(AstNode.MakeIdentifier(type.Name));

            var fully_qualified_name = type.FullName;
            var type_args =
                from arg in type.GetGenericArguments()
                                select GetExpressoType(arg);

            return (fully_qualified_name != null) ? AstType.MakeSimpleType(AstNode.MakeIdentifier(type_name, AstType.MakeSimpleType(fully_qualified_name)), type_args)
                                                               : AstType.MakeSimpleType(type_name, type_args);
        }

        static IEnumerable<Type> GetTypes(Assembly asm)
        {
            try{
                return asm.GetTypes();
            }
            catch(Exception){
                return Enumerable.Empty<Type>();
            }
        }
    }
}
