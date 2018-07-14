using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
#if WINDOWS
using System.Runtime.CompilerServices;
#endif

namespace Expresso.CodeGen
{
    /// <summary>
    /// A specialized type builder that allows users to delay the implementation of types.
    /// Most of the implementation is taken from the following web site "http://takeshik.org/blog/2011/12/14/expression-trees-with-il-emit/".
    /// </summary>
    public class LazyTypeBuilder
    {
        readonly TypeBuilder interface_type_builder;
        readonly Type[] types;
        readonly TypeBuilder impl_type_builder;
        readonly MethodBuilder prologue, static_prologue;
        readonly Dictionary<string, MemberInfo> members;
        readonly List<string> has_initializer_list = new List<string>();
        readonly Dictionary<string, LazyTypeBuilder> nested_types = new Dictionary<string, LazyTypeBuilder>();
        Type type_cache;
        bool is_created, is_raw_value_enum;

        /// <summary>
        /// Represents whether the interface type is already created.
        /// </summary>
        /// <value><c>true</c> if the interface type is defined; otherwise, <c>false</c>.</value>
        public bool IsDefined => type_cache != null;

        /// <summary>
        /// Gets the base type.
        /// </summary>
        /// <value>The base type.</value>
        public Type BaseType => interface_type_builder.BaseType;

        /// <summary>
        /// Gets the interface type as <see cref="InterfaceType"/>.
        /// </summary>
        /// <value>The interface type as <see cref="InterfaceType"/>.</value>
        public Type InterfaceType{
            get{
                if(type_cache == null)
                    throw new InvalidOperationException("The interface type is yet to be defined");
                
                return type_cache;
            }
        }

        /// <summary>
        /// Gets the type of the interface type using the <see cref="TypeInfo.AsType"/> method.
        /// </summary>
        /// <value>The type of the interface type as.</value>
        public Type TypeAsType => interface_type_builder.AsType();

        /// <summary>
        /// Gets the <see cref="System.Reflection.Emit.TypeBuilder"/> that represents the interface type.
        /// </summary>
        /// <value>The interface type builder.</value>
        public TypeBuilder TypeBuilder => interface_type_builder;

        /// <summary>
        /// Gets the name of this type.
        /// </summary>
        /// <value>The name.</value>
        public string Name => interface_type_builder.Name;

        public bool HasStaticFields{
            get{
                return members.Values
                              .OfType<FieldBuilder>()
                              .Where(fb => fb.IsStatic)
                              .Any();
            }
        }

        public LazyTypeBuilder(ModuleBuilder module, string name, TypeAttributes attr, IEnumerable<Type> baseTypes, bool isGlobalFunctions, bool isTupleStyleEnum)
            : this(module.DefineType(name, attr, baseTypes.Any() ? baseTypes.First() : typeof(object), baseTypes.Skip(1).ToArray()), isGlobalFunctions, isTupleStyleEnum)
        {
        }

        LazyTypeBuilder(TypeBuilder builder, bool isGlobalFunctions, bool isTupleStyleEnum)
        {
            interface_type_builder = builder;
            types = isGlobalFunctions ? new Type[]{} : new []{ builder };
            impl_type_builder = interface_type_builder.DefineNestedType("<Impl>", TypeAttributes.NestedPrivate);
            prologue = impl_type_builder.DefineMethod("Prologue", MethodAttributes.Assembly | MethodAttributes.Static, typeof(void), types);
            static_prologue = impl_type_builder.DefineMethod("StaticPrologue", MethodAttributes.Assembly | MethodAttributes.Static, typeof(void), null);
#if WINDOWS
            members = new Dictionary<string, MemberInfo>();
#else
            members = builder.GetMethods().OfType<MethodBuilder>()
                             .Cast<MemberInfo>()
                             .ToDictionary(mi => mi.Name);
#endif
            is_created = false;
            is_raw_value_enum = isTupleStyleEnum;
        }

        /// <summary>
        /// Defines a new field on this type.
        /// </summary>
        /// <returns>The field.</returns>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        /// <param name="attr">Attr.</param>
        public FieldBuilder DefineField(string name, Type type, bool hasInitializer, FieldAttributes attr = FieldAttributes.Public)
        {
            var field = interface_type_builder.DefineField(name, type, attr);
            if(hasInitializer)
                has_initializer_list.Add(name);
            
            members.Add(name, field);
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
        public MethodBuilder DefineMethod(string name, MethodAttributes attr, Type returnType, Type[] parameterTypes, CodeGenerator generator = null,
                                          CSharpEmitterContext context = null, Ast.FunctionDeclaration funcDecl = null)
        {
            var method = interface_type_builder.DefineMethod(name, attr, returnType, parameterTypes);
            if(funcDecl != null){
                var return_value_builder = method.DefineParameter(0, ParameterAttributes.Retval, null);
                context.CustomAttributeSetter = return_value_builder.SetCustomAttribute;
                context.AttributeTarget = AttributeTargets.ReturnValue;
                funcDecl.Attribute.AcceptWalker(generator, context);

                foreach(var pair in Enumerable.Range(0, parameterTypes.Length).Zip(funcDecl.Parameters, (p, arg) => new {Index = p, ParameterDeclaration = arg})){
                    var param_attr = pair.ParameterDeclaration.Option.IsNull ? ParameterAttributes.None : ParameterAttributes.HasDefault | ParameterAttributes.Optional;
                    var param_builder = method.DefineParameter(pair.Index + 1, param_attr, pair.ParameterDeclaration.Name);
                    if(!pair.ParameterDeclaration.Option.IsNull){
                        var option = pair.ParameterDeclaration.Option;
                        var default_value = (option is Ast.LiteralExpression literal) ? literal.Value : throw new InvalidOperationException(string.Format("Invalid default value: {0}!", option));;
                        param_builder.SetConstant(default_value);
                    }

                    context.CustomAttributeSetter = param_builder.SetCustomAttribute;
                    context.AttributeTarget = AttributeTargets.Parameter;
                    pair.ParameterDeclaration.Attribute.AcceptWalker(generator, context);
                }
            }

            var il = method.GetILGenerator();
            // Emit call to the implementation method no matter whether we actually need it.
            // TODO: Consider a better solution
            var real_params = types.Concat(parameterTypes).ToArray();
            var impl_method = impl_type_builder.DefineMethod(name + "_Impl", MethodAttributes.Assembly | MethodAttributes.Static, returnType, real_params);
            LoadArgs(il, (real_params.Length == 0) ? new int[]{} : Enumerable.Range(0, real_params.Length));
            il.Emit(OpCodes.Call, impl_method);
            il.Emit(OpCodes.Ret);

            members.Add(name, impl_method);
            return method;
        }

        /// <summary>
        /// Defines a new constructor on this type.
        /// </summary>
        /// <returns>The constructor.</returns>
        /// <param name="parameterTypes">Parameter types.</param>
        /// <param name="body">Body.</param>
        ConstructorBuilder DefineConstructor(Type[] parameterTypes, Expression body = null)
        {
            var ctor = interface_type_builder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard, parameterTypes
            );
            var il = ctor.GetILGenerator();
            if(!interface_type_builder.BaseType.Attributes.HasFlag(TypeAttributes.Interface)){
                LoadArgs(il, 0);
                il.Emit(OpCodes.Call, interface_type_builder.BaseType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null));
            }

            LoadArgs(il, 0);
            il.Emit(OpCodes.Call, prologue);

            members.Add("constructor", ctor);
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
            var tmp = new LazyTypeBuilder(interface_type_builder.DefineNestedType(name, real_attr, baseTypes.Any() ? baseTypes.First() : typeof(object), baseTypes.Skip(1).ToArray()), false, is_raw_value_enum);
            nested_types.Add(name, tmp);
            return tmp;
        }

        /// <summary>
        /// Completes defining the interface type(the outer type that the user actually accesses).
        /// </summary>
        /// <returns>The interface type.</returns>
        public Type CreateInterfaceType()
        {
            if(is_raw_value_enum || types.Any()){
                var fields = members.Values
                                    .OfType<FieldBuilder>()
                                    .Where(fb => !has_initializer_list.Contains(fb.Name));
                var param_types = fields.Select(fb => fb.FieldType)
                                        .ToArray();
                var ctor = DefineConstructor(param_types);

                var il_generator = ctor.GetILGenerator();
                foreach(var pair in Enumerable.Range(1, fields.Count()).Zip(fields, (i, r) => new {Counter = i, Field = r})){
                    LoadArgs(il_generator, new []{0});
                    LoadArgs(il_generator, new []{pair.Counter});

                    il_generator.Emit(OpCodes.Stfld, pair.Field);
                }
                il_generator.Emit(OpCodes.Ret);
            }

            if(HasStaticFields)
                DefineStaticConstructor();

            if(type_cache == null)
                type_cache = interface_type_builder.CreateType();

            return type_cache;
        }

        /// <summary>
        /// Completes defining the implementation type(the inner type that contains real implementations).
        /// </summary>
        /// <returns>The type.</returns>
        public Type CreateType()
        {
            if(interface_type_builder.Attributes.HasFlag(TypeAttributes.Interface))
                return null;

            if(type_cache == null)
                throw new InvalidOperationException("Call CreateInterfaceType before completing the type.");

            if(is_created)
                return type_cache;

/*#if WINDOWS
            var debug_info_generator = DebugInfoGenerator.CreatePdbGenerator();
            foreach(var implementer in implementers){
                var expr = implementer.Item1 as LambdaExpression;
                var lambda = expr ?? Expression.Lambda(implementer.Item1);
                lambda.CompileToMethod(implementer.Item2, debug_info_generator);
            }
#else
            var debug_info_generator = PortablePDBGenerator.CreatePortablePDBGenerator();
            foreach(var implementer in implementers){
                var expr = implementer.Item1 as LambdaExpression;
                var lambda = expr ?? Expression.Lambda(implementer.Item1);
                lambda.CompileToMethod(implementer.Item2, debug_info_generator);
            }*/

            /*var debug_info_generator = PortablePDBGenerator.CreatePortablePDBGenerator();
            #if !NETCOREAPP2_0
            Console.WriteLine("Emitting a PDB on type {0}...", interface_type_builder.Name);
            #endif
            debug_info_generator.WriteToFile(pdb_file_path);*/
            //#endif

            prologue.GetILGenerator().Emit(OpCodes.Ret);
            static_prologue.GetILGenerator().Emit(OpCodes.Ret);

            var type = impl_type_builder.CreateType();
            is_created = true;
            return type_cache;
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
                throw new InvalidOperationException(string.Format("The interface type is yet to be defined: you are trying to get the interface method of '{0}'.", name));

            return type_cache.GetMethod(name, flags);
        }

        /// <summary>
        /// Gets a method defined on this type. Note that it searches for non-public methods.
        /// </summary>
        /// <returns>The method.</returns>
        /// <param name="name">Name.</param>
        public MethodBuilder GetMethodBuilder(string name)
        {
            if(members.TryGetValue(name, out var method))
                return (MethodBuilder)method;
            else
                return null;
        }

        /// <summary>
        /// Gets a field defined on this type. Note that it searches for non-public fields.
        /// </summary>
        /// <returns>The field.</returns>
        /// <param name="name">Name.</param>
        public FieldBuilder GetFieldBuilder(string name)
        {
            if(members.TryGetValue(name, out var field))
                return (FieldBuilder)field;
            else
                return null;
        }

        public FieldInfo GetField(string name, BindingFlags flags)
        {
            if(type_cache == null)
                throw new InvalidOperationException("The interface type is yet to be defined");

            return type_cache.GetField(name, flags);
        }

        public ConstructorBuilder GetConstructor(Type[] paramTypes)
        {
            if(members.TryGetValue("constructor", out var ctor))
                return (ConstructorBuilder)ctor;
            else
                return null;
        }

        /// <summary>
        /// Gets a nested type.
        /// </summary>
        /// <returns>The nested type.</returns>
        /// <param name="name">The name to search.</param>
        public LazyTypeBuilder GetNestedType(string name)
        {
            return nested_types[name];
        }

        /// <summary>
        /// Gets the <see cref="ILGenerator"/> for initializing the field.
        /// </summary>
        /// <param name="field">Field.</param>
        public ILGenerator GetILGeneratorForFieldInit(FieldBuilder field)
        {
            if(field.IsStatic)
                return static_prologue.GetILGenerator();
            else
                return prologue.GetILGenerator();
        }

        public override string ToString()
        {
            return string.Format("[LazyTypeBuilder: Name={0}, BaseType={1}]", interface_type_builder.Name, BaseType);
        }

        ConstructorBuilder DefineStaticConstructor()
        {
            var cctor = interface_type_builder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard, null
            );
            var il = cctor.GetILGenerator();

            il.Emit(OpCodes.Call, static_prologue);

            il.Emit(OpCodes.Ret);
            members.Add("cctor", cctor);
            return cctor;
        }

        static void LoadArgs(ILGenerator ilGenerator, IEnumerable<int> indices)
        {
            foreach(var i in indices){
                switch(i){
                case 0:
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    break;

                case 1:
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    break;

                case 2:
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    break;

                case 3:
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    break;

                default:
                    if(i <= short.MaxValue)
                        ilGenerator.Emit(OpCodes.Ldarg_S, (short)i);
                    else
                        ilGenerator.Emit(OpCodes.Ldarg, i);

                    break;
                }
            }
        }

        static void LoadArgs(ILGenerator ilGenerator, params int[] indices)
        {
            LoadArgs(ilGenerator, (IEnumerable<int>)indices);
        }
    }
}

