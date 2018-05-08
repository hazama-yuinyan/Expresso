using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Expresso.Ast;
using Expresso.Runtime.Builtins;
using Expresso.TypeSystem;

namespace Expresso.Utilities
{
    /// <summary>
    /// Contains logics for retrieving types
    /// </summary>
    public static class TypeHelpers
    {
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

            case KnownTypeCode.Slice:
                return typeof(Slice<,>);

            default:
                return typeof(object);
            }
        }

        public static Type GetNativeType(AstType astType)
        {
            if(astType is PrimitiveType primitive)
                return GetPrimitiveType(primitive);

            if(astType is SimpleType simple){
                var name = simple.Identifier;
                Type type = null;
                if(simple.Identifier == "tuple" && !simple.TypeArguments.Any())
                    return typeof(void);

                if(simple.Identifier == "array"){
                    var type_arg = simple.TypeArguments
                                         .Select(ta => GetNativeType(ta))
                                         .First();

                    var array = Array.CreateInstance(type_arg, 1);
                    return array.GetType();
                }

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

                if(type == null && !astType.IdentifierNode.Type.IsNull)
                    return GetNativeType(astType.IdentifierNode.Type);
                else if(type == null)
                    throw new FormattedException("The simple type `{0}`(`{1}`) is not found!", simple, name);

                return type;
            }

            if(astType is MemberType member){
                Type type = null;
                foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){
                    var types = GetTypes(asm);
                    //type = asm.GetType(member.MemberName);
                    type = types.Where(t => t.Name.StartsWith(member.MemberName, StringComparison.CurrentCulture)).FirstOrDefault();
                    if(type != null)
                        break;
                }

                if(type == null)
                    throw new FormattedException("The member type `{0}` is not found!", member);

                return type;
            }

            if(astType is FunctionType func){
                var param_types = func.Parameters
                                      .Select(p => GetNativeType(p))
                                      .ToArray();

                var return_type = GetNativeType(func.ReturnType);
                if(return_type == typeof(void)){
                    return System.Linq.Expressions.Expression.GetActionType(param_types);
                }else{
                    var type_args = param_types.Concat(new []{return_type})
                                               .ToArray();
                    return System.Linq.Expressions.Expression.GetFuncType(type_args);
                }
            }

            if(astType is ReferenceType reference){
                var native_type = GetNativeType(reference.BaseType);
                return native_type.MakeByRefType();
            }

            if(astType is PlaceholderType placeholder)
                throw new InvalidOperationException("Unresolved type found!");

            throw new InvalidOperationException("Unknown AstType!");
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
