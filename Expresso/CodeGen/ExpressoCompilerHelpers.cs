﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Expresso.Ast;
using Expresso.Ast.Analysis;

namespace Expresso.CodeGen
{
    /// <summary>
    /// Contains helper methods for native symbols.
    /// </summary>
    public static class ExpressoCompilerHelpers
    {
        static uint IdentifierId = 1000000019u;
        static readonly string TypePrefix = "type_";
        static readonly string[] IgnoreList = new []{"equals", "getHashCode"};

        static Dictionary<string, Tuple<string, uint>> SpecialNamesMapInverse = new Dictionary<string, Tuple<string, uint>>{
            {"Expresso.Runtime.Builtins.ExpressoIntegerSequence", Tuple.Create("intseq", 1_000_000_003u)},
            {"Expresso.Runtime.Builtins.Slice", Tuple.Create("slice", 1_000_000_004u)},
            {"System.Func", Tuple.Create("function", 1_000_000_005u)},
            {"System.Boolean", Tuple.Create("bool", 1_000_000_006u)},
            {"System.Int32", Tuple.Create("int", 1_000_000_007u)},
            {"System.UInt32", Tuple.Create("uint", 1_000_000_008u)},
            {"System.Single", Tuple.Create("float", 1_000_000_009u)},
            {"System.Double", Tuple.Create("double", 1_000_000_010u)},
            {"System.Char", Tuple.Create("char", 1_000_000_011u)},
            {"System.Byte", Tuple.Create("byte", 1_000_000_012u)},
            {"System.String", Tuple.Create("string", 1_000_000_013u)},
            {"System.Array", Tuple.Create("array", 1_000_000_014u)},
            {"System.Collections.Generic.List", Tuple.Create("vector", 1_000_000_015u)},
            {"System.Tuple", Tuple.Create("tuple", 1_000_000_016u)},
            {"System.Collections.Generic.Dictionary", Tuple.Create("dictionary", 1_000_000_017u)},
            {"System.Numerics.BigInteger", Tuple.Create("bigint", 1_000_000_018u)}
        };

        public static string ConvertToExpressoFunctionName(string name)
        {
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }

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
            try{
                var type_code = PrimitiveType.GetKnownTypeCodeForPrimitiveType(type, null);
                return AstType.MakePrimitiveType(type);
            }
            catch(ParserException){
                return null;
            }
        }

        public static void AddPrimitiveTypesSymbolTables(SymbolTable table)
        {
            foreach(var primitive in SpecialNamesMapInverse){
                table.AddTypeSymbol(primitive.Value.Item1, AstType.MakePrimitiveType(primitive.Value.Item1));
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
            if(type == null)
                throw new ParserException("Type '{0}' is not a native type", identifier, identifier.Name);
                
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
                if(IgnoreList.Contains(method_name) || method_name.StartsWith("op_", StringComparison.CurrentCulture) || new_table.GetSymbol(method_name) != null)
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
                new_table.Parent = table;
                table.Children.Add(new_table);
                // Don't add a type symbol here bacause Parser has already done that
                //table.AddTypeSymbol(expresso_type_name_full, AstType.MakeSimpleType(expresso_type_name, type_args));
                if(SpecialNamesMapInverse.ContainsKey(identifier.Name))
                    table.GetTypeSymbol(expresso_type_name).IdentifierId = IdentifierId++;
            }
        }

        static AstType GetExpressoType(Type type)
        {
            var name = type.FullName ?? type.Name;
            var index = name.IndexOf("`", StringComparison.CurrentCulture);
            var actual_type_name = name.Substring(0, index == -1 ? name.Length : index);
            var type_name = SpecialNamesMapInverse.ContainsKey(actual_type_name) ? SpecialNamesMapInverse[actual_type_name].Item1 : type.Name;
            if(type_name == "Void")
                return AstType.MakeSimpleType("tuple");
            
            var primitive = GetPrimitiveAstType(type_name);
            if(primitive != null)
                return primitive;

            if(type.IsGenericParameter)
                return AstType.MakeParameterType(AstNode.MakeIdentifier(type.Name));
            
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