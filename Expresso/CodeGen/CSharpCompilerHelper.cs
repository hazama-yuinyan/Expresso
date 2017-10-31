﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Expresso.Ast;
using Expresso.TypeSystem;
using Expresso.Runtime.Builtins;
using Expresso.Ast.Analysis;
using System.Text;

namespace Expresso.CodeGen
{
    using CSharpExpr = System.Linq.Expressions.Expression;

    /// <summary>
    /// Contains helper methods for Expresso compilation.
    /// </summary>
    public static class CSharpCompilerHelper
    {
        public static Regex InterpolateStringRegexp =
            new Regex(@"%([a-zA-Z_][a-zA-Z_0-9]*)");

        static List<string> _AssemblyNames =
            new List<string>{"mscorlib", "System", "System.Core", "System.Numerics", "ExpressoRuntime"};

        static Dictionary<string, Tuple<string, uint>> SpecialNamesMap = new Dictionary<string, Tuple<string, uint>>{
            {"intseq", Tuple.Create("Expresso.Runtime.Builtins.ExpressoIntegerSequence", 1_000_000_003u)},
            {"function", Tuple.Create("System.Func", 1_000_000_004u)},
            {"bool", Tuple.Create("System.Boolean", 1_000_000_005u)},
            {"int", Tuple.Create("System.Int32", 1_000_000_006u)},
            {"uint", Tuple.Create("System.UInt32", 1_000_000_007u)},
            {"float", Tuple.Create("System.Float", 1_000_000_008u)},
            {"double", Tuple.Create("System.Double", 1_000_000_009u)},
            {"char", Tuple.Create("System.Char", 1_000_000_010u)},
            {"byte", Tuple.Create("System.Byte", 1_000_000_011u)},
            {"string", Tuple.Create("System.String", 1_000_000_012u)},
            {"array", Tuple.Create("System.Array", 1_000_000_013u)},
            {"vector", Tuple.Create("System.Collections.Generic.List", 1_000_000_014u)},
            {"tuple", Tuple.Create("System.Tuple", 1_000_000_015u)},
            {"dictionary", Tuple.Create("System.Collections.Generic.Dictionary", 1_000_000_016u)}
        };

        /// <summary>
        /// Gets the assembly name list.
        /// </summary>
        /// <value>The assembly names.</value>
        public static List<string> AssemblyNames{
            get{return _AssemblyNames;}
        }

        /// <summary>
        /// Helper method to convert a PrimitiveType to a C#'s type.
        /// </summary>
        /// <returns>A <see cref="System.Type"/> object.</returns>
        /// <param name="type">Type.</param>
        public static Type GetPrimitiveType(PrimitiveType type)
        {
            if(type == null)
                throw new ArgumentNullException(nameof(type));

            switch(type.KnownTypeCode){
            case KnownTypeCode.Bool:
                return typeof(bool);

            case KnownTypeCode.Int:
                return typeof(int);

            case KnownTypeCode.UInt:
                return typeof(uint);

            case KnownTypeCode.Byte:
                return typeof(byte);

            case KnownTypeCode.Char:
                return typeof(char);

            case KnownTypeCode.Float:
                return typeof(float);

            case KnownTypeCode.Double:
                return typeof(double);

            case KnownTypeCode.BigInteger:
                return typeof(BigInteger);

            case KnownTypeCode.String:
                return typeof(string);

            case KnownTypeCode.Vector:
                return typeof(List<>);

            case KnownTypeCode.Tuple:
                return typeof(Tuple);

            case KnownTypeCode.Dictionary:
                return typeof(Dictionary<,>);

            case KnownTypeCode.IntSeq:
                return typeof(ExpressoIntegerSequence);

            default:
                return typeof(object);
            }
        }

        /// <summary>
        /// Helper method to convert a SimpleType to a C#'s type.
        /// </summary>
        /// <returns>A <see cref="System.Type"/> object.</returns>
        /// <param name="containerType">Container type.</param>
        public static Type GetContainerType(SimpleType containerType)
        {
            if(containerType == null)
                throw new ArgumentNullException(nameof(containerType));

            switch(containerType.Name){
            case "dictionary":
                return typeof(Dictionary<,>);

            case "array":
                return typeof(Array);

            case "tuple":
                return typeof(Tuple);

            case "vector":
                return typeof(List<>);

            default:
                throw new EmitterException("Unknown container type!");
            }
        }

        public static Type GetNativeType(AstType astType)
        {
            if(astType is PrimitiveType primitive)
                return GetPrimitiveType(primitive);

            if(astType is SimpleType simple){
                var name = ConvertToDotNetTypeName(simple.Identifier);
                Type type = null;
                if(simple.Identifier == "tuple" && !simple.TypeArguments.Any())
                    return typeof(void);
                
                if(simple.TypeArguments.Any()){
                    name += "`" + simple.TypeArguments.Count + "[";
                    bool first = true;
                    foreach(var type_arg in simple.TypeArguments){
                        if(first)
                            first = false;
                        else
                            name += ",";

                        name += GetNativeType(type_arg).FullName;
                    }
                    name += "]";
                }

                var asms = AppDomain.CurrentDomain.GetAssemblies();
                foreach(var asm in asms){
                    type = asm.GetType(name);
                    if(type != null)
                        break;
                }
                /*foreach(var asm in asms){
                    var types = GetTypes(asm);
                    type = types.Where(t => t.Name.StartsWith(name, StringComparison.CurrentCulture) && t.Name.IndexOf('`') != -1 && t.Name.IndexOf('`') == name.Length || t.Name == name)
                        .FirstOrDefault();

                    if(type != null)
                        break;
                }

                if(simple.Identifier == "array"){
                    var type_arg = simple.TypeArguments
                        .Select(ta => GetNativeType(ta))
                        .First();

                    var array = Array.CreateInstance(type_arg, 1);
                    return array.GetType();
                }else if(simple.Identifier == "dictionary" || simple.Identifier == "vector"){
                    var generic_type = GetContainerType(simple);
                    var type_args =
                        from ta in simple.TypeArguments
                        select GetNativeType(ta);

                    var substituted = generic_type.MakeGenericType(type_args.ToArray());
                    if(substituted == null){
                        throw new EmitterException("Type `{0}` is expected to have {1} type arguments, but it actually has {2}",
                            type, type_args.Count(), type.GenericTypeArguments.Length
                        );
                    }
                    return substituted;
                }else if(simple.Identifier == "tuple"){
                    if(!simple.TypeArguments.Any())
                        return typeof(void);

                    var type_args =
                        from ta in simple.TypeArguments
                        select GetNativeType(ta);

                    var tuple = GuessTupleType(type_args);
                    return tuple;
                }else if(simple.Identifier == "void"){
                    return typeof(void);
                }else if(type == null){
                    throw new EmitterException("Type `{0}` is not found!", simple.Identifier);
                }

                if(simple.TypeArguments.Count > 0){
                    if(!type.IsGenericType){
                        throw new EmitterException("Type `{0}` is used as a generic type but native type `{1}` is not",
                            simple.Identifier, type.Name
                        );
                    }

                    var type_args =
                        from ta in simple.TypeArguments
                        select GetNativeType(ta);

                    var substituted = type.MakeGenericType(type_args.ToArray());
                    if(substituted == null){
                        throw new EmitterException("Type `{0}` is expected to have {1} type arguments, but it acutually has {2}",
                            type, type_args.Count(), type.GenericTypeArguments.Length
                        );
                    }

                    return substituted;
                }*/

                if(type == null)
                    throw new EmitterException("Type `{0}` is not found!", simple.Identifier);

                return type;
            }

            if(astType is MemberType member){
                foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){
                    var types = GetTypes(asm);
                    var type = types.Where(t => t.Name.StartsWith(member.MemberName, StringComparison.CurrentCulture)).FirstOrDefault();
                    if(type != null)
                        return type;
                }

                return null;
            }

            if(astType is FunctionType func){
                var param_types = func.Parameters
                                      .Select(p => GetNativeType(p))
                                      .ToArray();

                var return_type = GetNativeType(func.ReturnType);
                if(return_type == typeof(void)){
                    return CSharpExpr.GetActionType(param_types);
                }else{
                    var type_args = param_types.Concat(new []{return_type})
                                               .ToArray();
                    return CSharpExpr.GetFuncType(type_args);
                }
            }

            if(astType is PlaceholderType placeholder)
                throw new EmitterException("Unknown placeholder!");

            throw new EmitterException("Unknown AstType!");
        }

        public static string InterpolateString(string original)
        {
            return InterpolateStringRegexp.Replace(original, m => {
                return m.Groups[1].Value;
            });
        }

        public static string FormatString(string str, params object[] objs)
        {
            return "";
        }

        public static Type GuessTupleType(IEnumerable<Type> elementTypes)
        {
            var types = elementTypes.ToArray();
            switch(types.Length){
            case 1:
                return typeof(Tuple<>).MakeGenericType(types);

            case 2:
                return typeof(Tuple<,>).MakeGenericType(types);

            case 3:
                return typeof(Tuple<,,>).MakeGenericType(types);

            case 4:
                return typeof(Tuple<,,,>).MakeGenericType(types);

            case 5:
                return typeof(Tuple<,,,,>).MakeGenericType(types);

            case 6:
                return typeof(Tuple<,,,,,>).MakeGenericType(types);

            case 7:
                return typeof(Tuple<,,,,,,>).MakeGenericType(types);

            case 8:
                return typeof(Tuple<,,,,,,,>).MakeGenericType(types);
            
            default:
                throw new EmitterException("Expresso on .NET doesn't support that many tuple elements");
            }
        }

        public static Assembly GetAssembly(AssemblyName name)
        {
            foreach(var loaded in AppDomain.CurrentDomain.GetAssemblies()){
                if(loaded.GetName() == name)
                    return loaded;
            }

            return AppDomain.CurrentDomain.Load(name);
        }

        public static Module GetModule(string moduleName)
        {
            var loaded_asms = AppDomain.CurrentDomain.GetAssemblies();
            Module module = null;
            foreach(var asm in loaded_asms){
                module = asm.GetModule(moduleName);
                if(module != null)
                    break;
            }

            return module;
        }

        public static MethodInfo GetGenericMethod(this Type type, string methodName, params Type[] parameterTypes)
        {
            return type.GetGenericMethod(methodName, BindingFlags.Public, parameterTypes);
        }

        public static MethodInfo GetGenericMethod(this Type type, string methodName, BindingFlags flags, params Type[] parameterTypes)
        {
            var generic_methods = 
                from m in type.GetMethods()
                    where m.Name == methodName && m.ContainsGenericParameters && m.GetParameters().Length == parameterTypes.Length
                select m;

            var results =
                from m in generic_methods
                    where flags.HasFlag(BindingFlags.Public) && m.IsPublic || flags.HasFlag(BindingFlags.Static) && m.IsStatic || flags.HasFlag(BindingFlags.Default)
                select m;

            if(results.Count() > 1)
                throw new Exception("Ambiguous methods! Multiple candidates found!");
            else if(!results.Any())
                throw new Exception("There is no candidate methods for '" + methodName + "'");

            return results.First().MakeGenericMethod(parameterTypes);
        }

        public static string ConvertToDotNetTypeName(string originalName)
        {
            if(SpecialNamesMap.ContainsKey(originalName))
                return SpecialNamesMap[originalName].Item1;
            else
                return originalName;
        }

        public static void Prepare()
        {
            foreach(var name in _AssemblyNames)
                AppDomain.CurrentDomain.Load(name);
        }

        public static string ConvertToPascalCase(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }

        public static string ExpandContainer(object obj)
        {
            var type = obj.GetType();
            if(type.IsGenericType){
	            var type_def = type.GetGenericTypeDefinition();
	            if(type_def == typeof(List<>)){
                    if(obj is IEnumerable<int> enumerable)
	                    return ExpandList(enumerable);
                    else if(obj is IEnumerable<uint> eumerable2)
	                    return ExpandList(eumerable2);
	                else
                        return ExpandList((IEnumerable<object>)obj);
	            }

	            if(type_def == typeof(Dictionary<,>)){
	                if(obj is Dictionary<int, int> dict)
	                    return ExpandDictionary(dict);
	                else if(obj is Dictionary<uint, int> dict2)
	                    return ExpandDictionary(dict2);
                    else if(obj is Dictionary<string, int> dict3)
                        return ExpandDictionary(dict3);
                    else if(obj is Dictionary<int, uint> dict4)
	                    return ExpandDictionary(dict4);
                    else if(obj is Dictionary<int, string> dict5)
                        return ExpandDictionary(dict5);
	                else if(obj is Dictionary<uint, uint> dict6)
	                    return ExpandDictionary(dict6);
                    else if(obj is Dictionary<uint, string> dict7)
                        return ExpandDictionary(dict7);
                    else if(obj is Dictionary<string, uint> dict8)
                        return ExpandDictionary(dict8);
                    else if(obj is Dictionary<string, string> dict9)
                        return ExpandDictionary(dict9);
	            }
            }else if(type.IsArray){
                if(obj is IEnumerable<int> enumerable)
                    return ExpandList(enumerable);
                else if(obj is IEnumerable<uint> enumerable2)
                    return ExpandList(enumerable2);
                else
                    return ExpandList((IEnumerable<object>)obj);
            }

            return obj.ToString();
        }

        public static void AddPrimitiveNativeSymbols()
        {
            CSharpEmitter.Symbols.Add(1000000000u, new ExpressoSymbol{
                Method = typeof(Console).GetMethod("Write", new []{
                    typeof(string), typeof(object[])
                })
            });
            CSharpEmitter.Symbols.Add(1000000001u, new ExpressoSymbol{
                Method = typeof(Console).GetMethod("WriteLine", new []{
                    typeof(string), typeof(object[])
                })
            });
            CSharpEmitter.Symbols.Add(1000000002u, new ExpressoSymbol{
                Method = typeof(Console).GetMethod("Write", new []{
                    typeof(string), typeof(object[])
                })
            });

            foreach(var builtin_pair in SpecialNamesMap){
                var primitive = AstType.MakePrimitiveType(builtin_pair.Key);
                var type = GetPrimitiveType(primitive);

                CSharpEmitter.Symbols.Add(builtin_pair.Value.Item2, new ExpressoSymbol{
                    Type = type
                });
                /*foreach(var method in type.GetMethods()){
                    CSharpEmitter.Symbols.Add(IdentifierId++, new ExpressoSymbol{
                        Method = method
                    });
                }*/
            }
        }

        public static void AddNativeSymbols(AstNodeCollection<ImportDeclaration> imports)
        {
            foreach(var import in imports){
                if(import.ModuleName.EndsWith(".exs", StringComparison.CurrentCulture)){
                    /*var module_name = import.ModuleName;
                    var start_index = module_name.LastIndexOf("/", StringComparison.CurrentCulture);
                    var actual_module_name = module_name.Substring(start_index + 1, module_name.LastIndexOf(".", StringComparison.CurrentCulture) - start_index - 1);
                    var simple_type = AstType.MakeSimpleType(actual_module_name);
                    var type = GetNativeType(simple_type);

                    CSharpEmitter.Symbols.Add(import.ModuleNameToken.IdentifierId, new ExpressoSymbol{
                        Type = type
                    });*/
                }else{
                //var start_index = (import.ModuleName.LastIndexOf('.') != -1) ? import.ModuleName.LastIndexOf('.') : 0;
                //var type_name = import.ModuleName.Substring(start_index);
                    var simple_type = AstType.MakeSimpleType(import.ModuleName);
                    var type = GetNativeType(simple_type);

                    CSharpEmitter.Symbols.Add(import.ModuleNameToken.IdentifierId, new ExpressoSymbol{
                        Type = type
                    });
                /*foreach(var method in type.GetMethods()){
                    CSharpEmitter.Symbols.Add(IdentifierId++, new ExpressoSymbol{
                        Method = method
                    });
                }*/
                }
            }
        }

        static string ExpandList<T>(IEnumerable<T> enumerable)
        {
            var builder = new StringBuilder();
            var type = enumerable.GetType();
            if(enumerable.Any()){
                builder.AppendFormat("[{0}", ExpandContainer(enumerable.First()));
                foreach(var elem in enumerable.Skip(1))
                    builder.AppendFormat(", {0}", ExpandContainer(elem));

                if(type.IsArray)
                    builder.Append("]");
                else
                    builder.Append("...]");
            }else{
                if(type.IsArray)
                    builder.Append("[]");
                else
                    builder.Append("[...]");
            }
            return builder.ToString();
        }

        static string ExpandDictionary<T, S>(Dictionary<T, S> dict)
        {
            var builder = new StringBuilder();
            if(dict.Any()){
                builder.AppendFormat("{{{0}: {1}", ExpandContainer(dict.First().Key), ExpandContainer(dict.First().Value));
                foreach(var pair in dict.Skip(1))
                    builder.AppendFormat(", {0}: {1}", ExpandContainer(pair.Key), ExpandContainer(pair.Value));

                builder.Append("}");
            }else{
                builder.Append("{}");
            }
            return builder.ToString();
        }

        static IEnumerable<Type> GetTypes(Assembly asm)
        {
            try{
                return asm.GetTypes();
            }
            catch(ReflectionTypeLoadException ex){
                foreach(var e in ex.LoaderExceptions)
                    Console.WriteLine(e.Message);

                return Enumerable.Empty<Type>();
            }
            catch(Exception){
                return Enumerable.Empty<Type>();
            }
        }
    }
}

