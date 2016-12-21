using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;


namespace Expresso.CodeGen
{
    /// <summary>
    /// A specialized type builder that allows users to delay the implementation of types.
    /// Most of the implementation is taken from the following web site "http://takeshik.org/blog/2011/12/14/expression-trees-with-il-emit/".
    /// </summary>
    public class LazyTypeBuilder
    {
        readonly TypeBuilder interface_type;
        readonly Type[] types;
        readonly TypeBuilder impl_type;
        readonly MethodBuilder prologue;
        readonly List<Tuple<Expression, MethodBuilder>> implementers;
        readonly List<MemberInfo> members;
        Type type_cache;
        bool is_created;

        public bool IsInterfaceDefined{
            get{ return type_cache != null; }
        }

        public Type BaseType{
            get{ return interface_type.BaseType; }
        }

        public LazyTypeBuilder(ModuleBuilder module, string name, TypeAttributes attr, IEnumerable<Type> baseTypes)
            : this(module.DefineType(name, attr, baseTypes.Any() ? baseTypes.First() : typeof(object), baseTypes.Skip(1).ToArray()))
        {
        }

        LazyTypeBuilder(TypeBuilder builder)
        {
            interface_type = builder;
            types = new []{ builder };
            impl_type = interface_type.DefineNestedType("<Impl>", TypeAttributes.NestedPrivate);
            prologue = impl_type.DefineMethod("Prologue", MethodAttributes.Assembly | MethodAttributes.Static, typeof(void), types);
            implementers = new List<Tuple<Expression, MethodBuilder>>();
            members = new List<MemberInfo>();
            is_created = false;
        }

        /// <summary>
        /// Defines a new field on this type.
        /// </summary>
        /// <returns>The field.</returns>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        /// <param name="attr">Attr.</param>
        /// <param name="initializer">Initializer.</param>
        public FieldBuilder DefineField(string name, Type type, FieldAttributes attr = FieldAttributes.Public, Expression initializer = null)
        {
            var field = interface_type.DefineField(name, type, attr);
            if(initializer != null)
                SetBody(field, initializer);

            members.Add(field);
            return field;
        }

        /// <summary>
        /// Defines a new method on this type.
        /// </summary>
        /// <returns>The method.</returns>
        /// <param name="name">Name.</param>
        /// <param name="returnType">Return type.</param>
        /// <param name="parameterTypes">Parameter types.</param>
        /// <param name="body">Body.</param>
        public MethodBuilder DefineMethod(string name, MethodAttributes attr, Type returnType, IList<Type> parameterTypes, Expression body = null)
        {
            var method = interface_type.DefineMethod(name, attr, returnType, parameterTypes.ToArray());
            var il = method.GetILGenerator();
            //if(body != null){
            // Emit call to the implementation method no matter whether we actually need it.
            // TODO: Consider a better solution
            var impl_method = impl_type.DefineMethod(name + "_Impl", MethodAttributes.Assembly | MethodAttributes.Static, returnType, types.Concat(parameterTypes).ToArray());
            LoadArgs(il, Enumerable.Range(0, parameterTypes.Count + 1));
            il.Emit(OpCodes.Call, impl_method);
            il.Emit(OpCodes.Ret);
            if(body == null)
                body = Expression.Empty();
            
            AddImplementer(body, impl_method);
            /*}else{
                if(returnType.IsValueType){
                    switch(Type.GetTypeCode(returnType)){
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Char:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Boolean:
                        il.Emit(OpCodes.Ldc_I4_0);
                        break;

                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Conv_I8);
                        break;

                    case TypeCode.Single:
                        il.Emit(OpCodes.Ldc_R4, 0.0f);
                        break;

                    case TypeCode.Double:
                        il.Emit(OpCodes.Ldc_R8, 0.0);
                        break;

                    default:
                        if(returnType != typeof(void)){
                            il.Emit(OpCodes.Ldloca_S, (short)1);
                            il.Emit(OpCodes.Initobj, returnType);
                        }
                        break;
                    }
                }else{
                    il.Emit(OpCodes.Ldnull);
                }

                il.Emit(OpCodes.Ret);
            }*/

            members.Add(method);
            return method;
        }

        /// <summary>
        /// Defines a new constructor on this type.
        /// </summary>
        /// <returns>The constructor.</returns>
        /// <param name="parameterTypes">Parameter types.</param>
        /// <param name="body">Body.</param>
        public ConstructorBuilder DefineConstructor(IList<Type> parameterTypes, Expression body = null)
        {
            var ctor = interface_type.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                           CallingConventions.Standard, parameterTypes.ToArray()
                       );
            var il = ctor.GetILGenerator();
            LoadArgs(il, 0);
            il.Emit(OpCodes.Call, interface_type.BaseType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null));

            LoadArgs(il, 0);
            il.Emit(OpCodes.Call, prologue);

            LoadArgs(il, Enumerable.Range(0, parameterTypes.Count + 1));
            var impl_method = impl_type.DefineMethod("Ctor_Impl", MethodAttributes.Assembly | MethodAttributes.Static, typeof(void), types.Concat(parameterTypes).ToArray());
            il.Emit(OpCodes.Call, impl_method);
            if(body == null)
                body = Expression.Empty();
            
            AddImplementer(body, impl_method);

            il.Emit(OpCodes.Ret);
            members.Add(ctor);
            return ctor;
        }

        /// <summary>
        /// Defines and creates a new LazyTypeBuilder instance as a nested type.
        /// </summary>
        /// <returns>The nested type.</returns>
        /// <param name="name">Name.</param>
        /// <param name="baseTypes">Base types.</param>
        public LazyTypeBuilder DefineNestedType(string name, TypeAttributes attr, IEnumerable<Type> baseTypes)
        {
            var real_attr = attr.HasFlag(TypeAttributes.Public) ? TypeAttributes.NestedPublic : TypeAttributes.NestedPrivate;
            return new LazyTypeBuilder(interface_type.DefineNestedType(name, real_attr, baseTypes.Any() ? baseTypes.First() : typeof(object), baseTypes.Skip(1).ToArray()));
        }

        /// <summary>
        /// Completes defining the interface type(the outer type that the user actually accesses).
        /// </summary>
        /// <returns>The interface type.</returns>
        public Type CreateInterfaceType()
        {
            if(!members.OfType<ConstructorBuilder>().Any())
                DefineConstructor(Type.EmptyTypes);

            if(type_cache == null)
                type_cache = interface_type.CreateType();

            return type_cache;
        }

        /// <summary>
        /// Completes defining the implementation type(the inner type that contains real implementations).
        /// </summary>
        /// <returns>The type.</returns>
        public Type CreateType()
        {
            if(is_created)
                return type_cache;

            foreach(var implementer in implementers){
                var expr = implementer.Item1 as LambdaExpression;
                var lambda = expr ?? Expression.Lambda(implementer.Item1);
                lambda.CompileToMethod(implementer.Item2);
            }

            prologue.GetILGenerator().Emit(OpCodes.Ret);

            impl_type.CreateType();
            is_created = true;
            return type_cache;
        }

        public FieldBuilder GetField(string name)
        {
            return members.OfType<FieldBuilder>()
                .Where(m => m.Name == name)
                .Select(m => m)
                .FirstOrDefault();
        }

        public MethodBuilder GetMethod(string name)
        {
            return members.OfType<MethodBuilder>()
                .Where(m => m.Name == name)
                .Select(m => m)
                .FirstOrDefault();
        }

        public void SetBody(FieldBuilder field, Expression body)
        {
            if(body == null)
                throw new ArgumentNullException("body");
            
            var impl_method = impl_type.DefineMethod(field.Name + "_Init", MethodAttributes.Assembly | MethodAttributes.Static, field.FieldType, types);
            var prologue_il = prologue.GetILGenerator();
            LoadArgs(prologue_il, 0, 0);
            prologue_il.Emit(OpCodes.Call, impl_method);
            prologue_il.Emit(OpCodes.Stfld, field);
            AddImplementer(body, impl_method);
        }

        public void SetBody(MethodBuilder method, Expression body)
        {
            if(body == null)
                throw new ArgumentNullException("body");
            
            var impl_method_name = method.Name + "_Impl";
            for(int i = 0; i < implementers.Count; ++i){
                if(implementers[i].Item2.Name == impl_method_name){
                    implementers[i] = Tuple.Create(body, implementers[i].Item2);
                    break;
                }
            }
        }

        public void SetBody(ConstructorBuilder ctor, Expression body)
        {
            if(body == null)
                throw new ArgumentNullException("body");
            
            var impl_method_name = "Ctor_Impl";
            for(int i = 0; i < implementers.Count; ++i){
                if(implementers[i].Item2.Name == impl_method_name){
                    implementers[i] = Tuple.Create(body, implementers[i].Item2);
                    break;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("[LazyTypeBuilder: Name={0}, BaseType={1}]", interface_type.Name, BaseType);
        }

        void AddImplementer(Expression body, MethodBuilder impl)
        {
            implementers.Add(Tuple.Create(body, impl));
        }

        static void LoadArgs(ILGenerator generator, IEnumerable<int> indexes)
        {
            foreach(var i in indexes){
                switch(i){
                case 0:
                    generator.Emit(OpCodes.Ldarg_0);
                    break;

                case 1:
                    generator.Emit(OpCodes.Ldarg_1);
                    break;

                case 2:
                    generator.Emit(OpCodes.Ldarg_2);
                    break;

                case 3:
                    generator.Emit(OpCodes.Ldarg_3);
                    break;

                default:
                    if(i <= short.MaxValue)
                        generator.Emit(OpCodes.Ldarg_S, (short)i);
                    else
                        generator.Emit(OpCodes.Ldarg, i);

                    break;
                }
            }
        }

        static void LoadArgs(ILGenerator generator, params int[] indexes)
        {
            LoadArgs(generator, (IEnumerable<int>)indexes);
        }
    }
}

