using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Expresso.Ast;
using Expresso.TypeSystem;


namespace Expresso.CodeGen
{
    public static class CSharpCompilerHelper
    {
        public static Regex InterpolateStringRegexp =
            new Regex(@"%([a-zA-Z_][a-zA-Z_0-9]*)");

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

            default:
                return typeof(object);
            }
        }

        public static Type GetContainerType(PrimitiveType containerType)
        {
            if(containerType == null)
                throw new ArgumentNullException("containerType");

            switch(containerType.KnownTypeCode){
            case KnownTypeCode.Dictionary:
                return typeof(Dictionary<,>);

            case KnownTypeCode.Array:
                return typeof(Array);

            case KnownTypeCode.Tuple:
                return typeof(Tuple);

            case KnownTypeCode.Vector:
                return typeof(List<>);

            default:
                throw new EmitterException("Unknown container type!");
            }
        }

        public static Type GetNativeType(AstType astType)
        {
            var primitive = astType as PrimitiveType;
            if(primitive != null){
                return GetPrimitiveType(primitive);
            }

            var simple = astType as SimpleType;
            if(simple != null){

            }
        }

        public static string InterpolateString(string original)
        {
            return InterpolateStringRegexp.Replace(original, m => {
                return m.Groups[1].Value;
            });
        }

        public static Type GuessTupleType(IEnumerable<Type> elementTypes)
        {
            var tuple_type = typeof(Tuple);
            return tuple_type.MakeGenericType(elementTypes.ToArray());
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
    }
}

