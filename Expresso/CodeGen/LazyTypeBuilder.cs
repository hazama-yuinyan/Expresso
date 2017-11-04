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
        readonly MethodBuilder prologue, static_prologue;
        readonly List<Tuple<Expression, MethodBuilder>> implementers;
        readonly List<MemberInfo> members;
        readonly List<string> has_initializer_list = new List<string>();
        Type type_cache;
        bool is_created;

        public bool IsInterfaceDefined{
            get{ return type_cache != null; }
        }

        public Type BaseType{
            get{ return interface_type.BaseType; }
        }

        public Type InterfaceType{
            get{
                if(type_cache == null)
                    throw new InvalidOperationException("The interface type is yet to be defined");
                
                return type_cache;
            }
        }

        public TypeBuilder InterfaceTypeBuilder{
            get{
                return interface_type;
            }
        }

        public string Name{
            get{
                return interface_type.Name;
            }
        }

        public bool HasStaticFields{
            get{
                return members.OfType<FieldBuilder>()
                              .Where(fb => fb.IsStatic)
                              .Any();
            }
        }

        public LazyTypeBuilder(ModuleBuilder module, string name, TypeAttributes attr, IEnumerable<Type> baseTypes, bool isGlobalFunctions)
            : this(module.DefineType(name, attr, baseTypes.Any() ? baseTypes.First() : typeof(object), baseTypes.Skip(1).ToArray()), isGlobalFunctions)
        {
        }

        LazyTypeBuilder(TypeBuilder builder, bool isGlobalFunctions)
        {
            interface_type = builder;
            types = isGlobalFunctions ? new Type[]{} : new []{ builder };
            impl_type = interface_type.DefineNestedType("<Impl>", TypeAttributes.NestedPrivate);
            prologue = impl_type.DefineMethod("Prologue", MethodAttributes.Assembly | MethodAttributes.Static, typeof(void), types);
            static_prologue = impl_type.DefineMethod("StaticPrologue", MethodAttributes.Assembly | MethodAttributes.Static, typeof(void), null);
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
        public FieldBuilder DefineField(string name, Type type, bool hasInitializer, FieldAttributes attr = FieldAttributes.Public, Expression initializer = null)
        {
            var field = interface_type.DefineField(name, type, attr);
            if(initializer != null)
                SetBody(field, initializer);

            if(hasInitializer)
                has_initializer_list.Add(name);
            
            members.Add(field);
            return field;
        }

        /// <summary>
        /// Defines a new method on this type.
        /// Returns a MethodBuilder that represents the interface method.
        /// </summary>
        /// <returns>The method.</returns>
        /// <param name="name">Name.</param>
        /// <param name="returnType">Return type.</param>
        /// <param name="parameterTypes">Parameter types.</param>
        /// <param name="body">Body.</param>
        public MethodBuilder DefineMethod(string name, MethodAttributes attr, Type returnType, Type[] parameterTypes, Expression body = null)
        {
            var method = interface_type.DefineMethod(name, attr, returnType, parameterTypes);
            var il = method.GetILGenerator();
            // Emit call to the implementation method no matter whether we actually need it.
            // TODO: Consider a better solution
            var real_params = types.Concat(parameterTypes).ToArray();
            var impl_method = impl_type.DefineMethod(name + "_Impl", MethodAttributes.Assembly | MethodAttributes.Static, returnType, real_params);
            LoadArgs(il, (real_params.Length == 0) ? new int[]{} : Enumerable.Range(0, real_params.Length));
            il.Emit(OpCodes.Call, impl_method);
            il.Emit(OpCodes.Ret);
            if(body == null)
                body = Expression.Empty();
            
            AddImplementer(body, impl_method);
            members.Add(method);
            return method;
        }

        /// <summary>
        /// Defines a new constructor on this type.
        /// </summary>
        /// <returns>The constructor.</returns>
        /// <param name="parameterTypes">Parameter types.</param>
        /// <param name="body">Body.</param>
        public ConstructorBuilder DefineConstructor(Type[] parameterTypes, Expression body = null)
        {
            var ctor = interface_type.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard, parameterTypes
            );
            var il = ctor.GetILGenerator();
            var real_params = types.Concat(parameterTypes).ToArray();
            LoadArgs(il, 0);
            il.Emit(OpCodes.Call, interface_type.BaseType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null));

            LoadArgs(il, 0);
            il.Emit(OpCodes.Call, prologue);

            LoadArgs(il, (real_params.Length == 0) ? new int[]{} : Enumerable.Range(0, real_params.Length));
            var impl_method = impl_type.DefineMethod("Ctor_Impl", MethodAttributes.Assembly | MethodAttributes.Static, typeof(void), real_params);
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
            return new LazyTypeBuilder(interface_type.DefineNestedType(name, real_attr, baseTypes.Any() ? baseTypes.First() : typeof(object), baseTypes.Skip(1).ToArray()), false);
        }

        /// <summary>
        /// Completes defining the interface type(the outer type that the user actually accesses).
        /// </summary>
        /// <returns>The interface type.</returns>
        public Type CreateInterfaceType()
        {
            ConstructorBuilder ctor = null;
            if(!members.OfType<ConstructorBuilder>().Any()){
                var param_types = members.OfType<FieldBuilder>()
                                         .Where(t => !has_initializer_list.Any(name => t.Name == name))
                                         .Select(t => t.FieldType)
                                         .ToArray();
                ctor = DefineConstructor(param_types);
            }

            if(HasStaticFields)
                DefineStaticConstructor();

            if(type_cache == null)
                type_cache = interface_type.CreateType();

            if(ctor != null){
                var parameters = members.OfType<FieldBuilder>()
                                        .Where(t => !has_initializer_list.Any(name => t.Name == name))
                                        .Select(t => Expression.Parameter(t.FieldType, t.Name))
                                        .ToList();

                var self_param = Expression.Parameter(type_cache, "self");
                var block_contents = parameters.Select(p => {
                    return Expression.Assign(
                        Expression.Field(self_param, p.Name),
                        p
                    );
                }).OfType<Expression>().ToList();
                // It's needed because the Ctor_Impl should return nothing
                block_contents.Add(Expression.Empty());

                parameters.Insert(0, self_param);
                var impl_tree = Expression.Lambda(
                    Expression.Block(block_contents),
                    parameters
                );
                SetBody(ctor, impl_tree);
            }

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
            static_prologue.GetILGenerator().Emit(OpCodes.Ret);

            var type = impl_type.CreateType();
            is_created = true;
            return type_cache;
        }

        /// <summary>
        /// Gets a method on the interface type.
        /// Note that it only gets public methods.
        /// </summary>
        /// <returns>The interface method.</returns>
        /// <param name="name">Name.</param>
        public MethodInfo GetInterfaceMethod(string name)
        {
            if(type_cache == null)
                throw new InvalidOperationException("The interface type is yet to be defined");

            return type_cache.GetMethod(name);
        }

        /// <summary>
        /// Gets a method on the interface type using name and flags.
        /// </summary>
        /// <returns>The interface method.</returns>
        /// <param name="name">Name.</param>
        /// <param name="flags">Flags.</param>
        public MethodInfo GetInterfaceMethod(string name, BindingFlags flags)
        {
            if(type_cache == null)
                throw new InvalidOperationException("The interface type is yet to be defined");

            return type_cache.GetMethod(name, flags);
        }

        /// <summary>
        /// Gets a method defined on this type. Note that it searches for non-public methods.
        /// </summary>
        /// <returns>The method.</returns>
        /// <param name="name">Name.</param>
        public MethodBuilder GetMethod(string name)
        {
            return members.OfType<MethodBuilder>()
                          .Where(mb => mb.Name == name)
                          .FirstOrDefault();
        }

        /// <summary>
        /// Gets a field defined on this type. Note that it searches for non-public fields.
        /// </summary>
        /// <returns>The field.</returns>
        /// <param name="name">Name.</param>
        public FieldBuilder GetField(string name)
        {
            return members.OfType<FieldBuilder>()
                          .Where(fb => fb.Name == name)
                          .FirstOrDefault();
        }

        public FieldInfo GetField(string name, BindingFlags flags)
        {
            if(type_cache == null)
                throw new InvalidOperationException("The interface type is yet to be defined");

            return type_cache.GetField(name, flags);
        }

        public ConstructorBuilder GetConstructor(Type[] paramTypes)
        {
            return members.OfType<ConstructorBuilder>()
                          .Where(cb => {
                return cb.GetParameters().Zip(paramTypes, (arg1, arg2) => Tuple.Create(arg1, arg2))
                         .All(t => t.Item1.ParameterType == t.Item2);
            }).First();
        }

        public void SetBody(FieldBuilder field, Expression body)
        {
            if(body == null)
                throw new ArgumentNullException(nameof(body));
            
            var impl_method = impl_type.DefineMethod(field.Name + "_Init", MethodAttributes.Assembly | MethodAttributes.Static, field.FieldType, types);
            if(field.IsStatic){
                var static_prologue_il = static_prologue.GetILGenerator();
                static_prologue_il.Emit(OpCodes.Call, impl_method);
                static_prologue_il.Emit(OpCodes.Stsfld, field);
            }else{
                var prologue_il = prologue.GetILGenerator();
                prologue_il.Emit(OpCodes.Ldarg_0);
                prologue_il.Emit(OpCodes.Call, impl_method);
                prologue_il.Emit(OpCodes.Stfld, field);
            }
            AddImplementer(body, impl_method);
        }

        public void SetBody(FieldBuilder field, Action<ILGenerator, FieldBuilder> ilEmitter)
        {
            if(ilEmitter == null)
                throw new ArgumentNullException(nameof(ilEmitter));

            var prologue_il = prologue.GetILGenerator();
            ilEmitter(prologue_il, field);
        }

        public void SetBody(MethodInfo method, Expression body)
        {
            if(body == null)
                throw new ArgumentNullException(nameof(body));
            
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
                throw new ArgumentNullException(nameof(body));
            
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

        ConstructorBuilder DefineStaticConstructor()
        {
            var cctor = interface_type.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard, null
            );
            var il = cctor.GetILGenerator();

            il.Emit(OpCodes.Call, static_prologue);

            il.Emit(OpCodes.Ret);
            members.Add(cctor);
            return cctor;
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

