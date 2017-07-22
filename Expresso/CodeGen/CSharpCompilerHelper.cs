using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Expresso.Ast;
using Expresso.TypeSystem;
using Expresso.Runtime.Builtins;
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

        static Dictionary<string, string> SpecialNamesMap = new Dictionary<string, string>{
            {"intseq", "ExpressoIntegerSequence"},
            {"function", "Func"},
            {"bool", "Boolean"},
            {"int", "Int32"},
            {"uint", "UInt32"},     //
            {"char", "UInt32"},     // In Expresso, char is encoded in UTF-8.
            {"string", "UTF8String"},
            {"array", "Array"},
            {"vector", "List"},
            {"tuple", "Tuple"},
            {"dictionary", "Dictionary"},
            {"void", "Void"}
        };

        /// <summary>
        /// Gets the assembly name list.
        /// </summary>
        /// <value>The assembly names.</value>
        public static List<string> AssemblyNames{
            get{return _AssemblyNames;}
        }

        /// <summary>
        /// Helper method to convert a PrimitiveType to C#'s type.
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
            var primitive = astType as PrimitiveType;
            if(primitive != null)
                return GetPrimitiveType(primitive);

            var simple = astType as SimpleType;
            if(simple != null){
                var name = ConvertToDotNetTypeName(simple.Identifier);
                Type type = null;
                foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){
                    var types = GetTypes(asm);
                    type = types.Where(t => t.Name.StartsWith(name) && t.Name.IndexOf('`') != -1 && t.Name.IndexOf('`') == name.Length || t.Name == name)
                        .FirstOrDefault();

                    if(type != null)
                        break;
                }

                if(simple.Identifier == "array"){
                    var type_arg = simple.TypeArguments
                        .Select(ta => GetNativeType(ta))
                        .First();

                    var array = System.Array.CreateInstance(type_arg, 1);
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
                }

                return type;
            }

            var member = astType as MemberType;
            if(member != null){
                return null;
            }

            var func = astType as FunctionType;
            if(func != null){
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

            var placeholder = astType as PlaceholderType;
            if(placeholder != null)
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
                throw new EmitterException("Expresso in .NET doesn't support that many tuple elements");
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
                throw new Exception("There is no candidate methods '" + methodName + "'");

            return results.First().MakeGenericMethod(parameterTypes);
        }

        public static string ConvertToDotNetTypeName(string originalName)
        {
            if(SpecialNamesMap.ContainsKey(originalName))
                return SpecialNamesMap[originalName];
            else
                return originalName;
        }

        public static void Prepare()
        {
            foreach(var name in _AssemblyNames)
                AppDomain.CurrentDomain.Load(name);
        }

        public static string ConvertToCLRFunctionName(string name)
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

        private static string ExpandList<T>(IEnumerable<T> enumerable)
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

        private static string ExpandDictionary<T, S>(Dictionary<T, S> dict)
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
            catch(Exception){
                return Enumerable.Empty<Type>();
            }
        }

        /*static Type CreateActionType(Type[] paramTypes)
        {
            switch(paramTypes.Length){
            case 0:
                return typeof(Action);

            case 1:
                return typeof(Action<>).MakeGenericType(paramTypes);

            case 2:
                return typeof(Action<,>).MakeGenericType(paramTypes);

            case 3:
                return typeof(Action<,,>).MakeGenericType(paramTypes);

            case 4:
                return typeof(Action<,,,>).MakeGenericType(paramTypes);

            case 5:
                return typeof(Action<,,,,>).MakeGenericType(paramTypes);

            case 6:
                return typeof(Action<,,,,,>).MakeGenericType(paramTypes);

            case 7:
                return typeof(Action<,,,,,,>).MakeGenericType(paramTypes);

            case 8:
                return typeof(Action<,,,,,,,>).MakeGenericType(paramTypes);

            case 9:
                return typeof(Action<,,,,,,,,>).MakeGenericType(paramTypes);

            case 10:
                return typeof(Action<,,,,,,,,,>).MakeGenericType(paramTypes);

            case 11:
                return typeof(Action<,,,,,,,,,,>).MakeGenericType(paramTypes);

            case 12:
                return typeof(Action<,,,,,,,,,,,>).MakeGenericType(paramTypes);

            case 13:
                return typeof(Action<,,,,,,,,,,,,>).MakeGenericType(paramTypes);

            case 14:
                return typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(paramTypes);

            case 15:
                return typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(paramTypes);

            case 16:
                return typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(paramTypes);

            default:
                throw new InvalidOperationException("Too many arguments on Action");
            }
        }

        static Type CreateFuncType(Type[] paramTypes, Type returnType)
        {
            var real_types = paramTypes.Concat(new []{returnType}).ToArray();
            switch(paramTypes.Length){
            case 0:
                return typeof(Func<>).MakeGenericType(real_types);

            case 1:
                return typeof(Func<,>).MakeGenericType(real_types);

            case 2:
                return typeof(Func<,,>).MakeGenericType(real_types);

            case 3:
                return typeof(Func<,,,>).MakeGenericType(real_types);

            case 4:
                return typeof(Func<,,,,>).MakeGenericType(real_types);

            case 5:
                return typeof(Func<,,,,,>).MakeGenericType(real_types);

            case 6:
                return typeof(Func<,,,,,,>).MakeGenericType(real_types);

            case 7:
                return typeof(Func<,,,,,,,>).MakeGenericType(real_types);

            case 8:
                return typeof(Func<,,,,,,,,>).MakeGenericType(real_types);

            case 9:
                return typeof(Func<,,,,,,,,,>).MakeGenericType(real_types);

            case 10:
                return typeof(Func<,,,,,,,,,,>).MakeGenericType(real_types);

            case 11:
                return typeof(Func<,,,,,,,,,,,>).MakeGenericType(real_types);

            case 12:
                return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(real_types);

            case 13:
                return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(real_types);

            case 14:
                return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(real_types);

            case 15:
                return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(real_types);

            case 16:
                return typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(real_types);

            default:
                throw new InvalidOperationException("Too many arguments on Func");
            }
        }*/
    }
}

