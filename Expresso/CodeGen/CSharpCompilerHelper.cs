using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Expresso.Ast;
using Expresso.TypeSystem;
using Expresso.Runtime.Builtins;


namespace Expresso.CodeGen
{
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
                throw new ArgumentNullException("type");

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
                throw new ArgumentNullException("containerType");

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
                    var types = GetExportedTypes(asm);
                    type = types.Where(t => t.Name.StartsWith(name) && t.Name.IndexOf('`') == name.Length)
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

        static IEnumerable<Type> GetExportedTypes(Assembly asm)
        {
            try{
                return asm.GetExportedTypes();
            }
            catch(Exception){
                return Enumerable.Empty<Type>();
            }
        }
    }
}

