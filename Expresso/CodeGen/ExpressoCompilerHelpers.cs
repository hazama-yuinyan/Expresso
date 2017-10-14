using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Expresso.Ast;
using Expresso.Ast.Analysis;

namespace Expresso.CodeGen
{
    public static class ExpressoCompilerHelpers
    {
        static uint IdentifierId = 1000000003u;
        static readonly string TypePrefix = "type_";
        static readonly string[] IgnoreList = new []{"equals", "getHashCode"};

        static Dictionary<string, string> SpecialNamesMapInverse = new Dictionary<string, string>{
            {"ExpressoIntegerSequence", "intseq"},
            {"Func", "function"},
            {"Boolean", "bool"},
            {"Int32", "int"},
            {"UInt32", "uint"},
            {"Float", "float"},
            {"Double", "double"},
            {"Char", "char"},
            {"Byte", "byte"},
            {"String", "string"},
            {"Array", "array"},
            {"List", "vector"},
            {"Tuple", "tuple"},
            {"Dictionary", "dictionary"},
            {"Void", "void"}
        };

        public static string ConvertToExpressoFunctionName(string name)
        {
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }

        public static string GetExpressoTypeName(string csharpFullName)
        {
            var start_index = (csharpFullName.LastIndexOf('.') != -1) ? csharpFullName.LastIndexOf('.') : 0;
            var backquote_index = csharpFullName.IndexOf('`');
            var csharp_name = csharpFullName.Substring(start_index, (backquote_index == -1) ? csharpFullName.Length - start_index : backquote_index - start_index);
            if(SpecialNamesMapInverse.ContainsKey(csharp_name))
                return SpecialNamesMapInverse[csharp_name];
            else
                return csharpFullName;
        }

        public static PrimitiveType GetPrimitiveAstType(string type)
        {
            try{
                var type_code = PrimitiveType.GetKnownTypeCodeForPrimitiveType(type, null);
                return AstType.MakePrimitiveType(type);
            }
            catch(ParserException){
                return null;
            }
        }

        public static void AddNativeSymbolTables(SymbolTable table)
        {
            foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){
                var types = GetTypes(asm);
                foreach(var type in types){
                    var type_name = type.Name;
                    var expresso_type_name = GetExpressoTypeName(type.FullName);
                    var expresso_name_builder = new StringBuilder(expresso_type_name);
                    if(type_name.StartsWith("<>")){
                        // ignore compiler-generated classes
                        continue;
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
                            type_args.Add(AstType.MakeSimpleType(type_arg.Name));
                        }
                    }
                    var converted_name_full = converted_name.ToString();
                    var expresso_type_name_full = expresso_name_builder.ToString();
                    var new_table = table.Children.Where(t => t.Name == converted_name_full).FirstOrDefault();
                    bool table_was_created = new_table != null;
                    new_table = new_table ?? new SymbolTable();
                    new_table.Name = converted_name_full;

                    foreach(var public_method in type.GetMethods()){
                        var method_name = public_method.Name;
                        method_name = ConvertToExpressoFunctionName(method_name);
                        if(IgnoreList.Contains(method_name) || method_name.StartsWith("op_") || new_table.GetSymbol(method_name) != null)
                            continue;
                        
                        var return_type = GetExpressoType(public_method.ReturnType);
                        var param_types =
                            from param in public_method.GetParameters()
                                                       select GetExpressoType(param.ParameterType);
                        var method_type = AstType.MakeFunctionType(method_name, return_type, param_types);
                        new_table.AddSymbol(method_name, method_type);

                        new_table.GetSymbol(method_name).IdentifierId = IdentifierId++;
                    }

                    if(!table_was_created){
                        table.Children.Add(new_table);
                        table.AddTypeSymbol(expresso_type_name_full, AstType.MakeSimpleType(expresso_type_name, type_args));
                    }
                }
            }
        }

        static AstType GetExpressoType(Type type)
        {
            var index = type.Name.IndexOf("`");
            var actual_type_name = type.Name.Substring(0, index == -1 ? type.Name.Length : index);
            var type_name = SpecialNamesMapInverse.ContainsKey(actual_type_name) ? SpecialNamesMapInverse[actual_type_name] : type.Name;
            if(type_name == "void")
                return AstType.MakeSimpleType("tuple");
            
            var primitive = GetPrimitiveAstType(type_name);
            if(primitive != null)
                return primitive;

            var type_args =
                from arg in type.GetGenericArguments()
                                select AstType.MakeSimpleType(arg.Name);
            return AstType.MakeSimpleType(type_name, type_args);
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
