using System;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using Expresso.TypeSystem;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

namespace Expresso.Resolver
{
    /// <summary>
    /// Contains main resolve logic.
    /// </summary>
    public class ExpressoResolver : ICodeContext
    {
        static readonly ResolveResult errorResult = ErrorResolveResult.UnknownError;
        readonly ICompilation compilation;
        readonly TypeDefinitionCache type_def_cache;
        readonly ExpressoTypeResolveContext context;
        bool is_within_lambda_expression;

        #region Properties
        #region ITypeResolveContext implementation

        ITypeResolveContext ITypeResolveContext.WithCurrentTypeDefinition(ITypeDefinition typeDefinition)
        {
            return WithCurrentTypeDefinition(typeDefinition);
        }

        ITypeResolveContext ITypeResolveContext.WithCurrentMember(IMember member)
        {
            return WithCurrentMember(member);
        }

        public IAssembly CurrentAssembly => context.CurrentAssembly;

        public ITypeDefinition CurrentTypeDefinition => context.CurrentTypeDefinition;

        public IMember CurrentMember => context.CurrentMember;

        #endregion

        #region ICodeContext implementation

        #region Local variables management
        // This data structure is used to allow efficient cloning of the resolver with its local variable context.
        readonly ImmutableStack<IVariable> local_variable_stack = ImmutableStack<IVariable>.Empty;

        ExpressoResolver WithLocalVariableStack(ImmutableStack<IVariable> stack)
        {
            return new ExpressoResolver(compilation, context, type_def_cache, stack, IsWithinLambdaExpression);
        }

        /// <summary>
        /// Opens a new scope for local variables.
        /// </summary>
        public ExpressoResolver PushBlock()
        {
            return WithLocalVariableStack(local_variable_stack.Push(null));
        }

        /// <summary>
        /// Closes the current scope for local variables; removing all variables in that scope.
        /// </summary>
        public ExpressoResolver PopBlock()
        {
            var stack = local_variable_stack;
            IVariable removed_var;
            do{
                removed_var = stack.Peek();
                stack = stack.Pop();
            }while(removed_var != null);

            return WithLocalVariableStack(stack);
        }

        /// <summary>
        /// Adds a new variable or lambda parameter to the current block.
        /// </summary>
        public ExpressoResolver AddVariable(IVariable variable)
        {
            if(variable == null)
                throw new ArgumentNullException(nameof(variable));

            return WithLocalVariableStack(local_variable_stack.Push(variable));
        }

        /// <summary>
        /// Removes the variable that was just added.
        /// </summary>
        public ExpressoResolver PopLastVariable()
        {
            if(local_variable_stack.Peek() == null)
                throw new InvalidOperationException("There is no variable within the current block.");

            return WithLocalVariableStack(local_variable_stack.Pop());
        }

        /// <summary>
        /// Gets all currently visible local variables and lambda parameters.
        /// Does not include method parameters.
        /// </summary>
        public IEnumerable<IVariable> LocalVariables => local_variable_stack.Where(v => v != null);
        #endregion

        public bool IsWithinLambdaExpression => is_within_lambda_expression;

        ExpressoResolver WithIsWithinLambdaExpression(bool isWithinLambdaExpression)
        {
            return new ExpressoResolver(Compilation, context, type_def_cache, local_variable_stack, isWithinLambdaExpression);
        }

        #endregion

        #region ICompilationProvider implementation

        public ICompilation Compilation => compilation;

        #endregion

        public ExpressoTypeResolveContext CurrentTypeResolveContext => context;

        ExpressoResolver WithContext(ExpressoTypeResolveContext newContext)
        {
            return new ExpressoResolver(newContext);
        }
        #endregion

        #region Constructors
        public ExpressoResolver(ICompilation compilation)
        {
            if(compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            this.compilation = compilation;
            context = new ExpressoTypeResolveContext(compilation.MainAssembly);
        }

        public ExpressoResolver(ExpressoTypeResolveContext context)
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));
            
            compilation = context.Compilation;
            this.context = context;
        }

        ExpressoResolver(ICompilation compilation, ExpressoTypeResolveContext context, TypeDefinitionCache typeDefinitionCache, ImmutableStack<IVariable> stack,
                         bool isWithinLambdaExpression)
        {
            this.compilation = compilation;
            this.context = context;
            type_def_cache = typeDefinitionCache;
            this.local_variable_stack = stack;
            is_within_lambda_expression = isWithinLambdaExpression;
        }
        #endregion

        public ExpressoResolver WithCurrentMember(IMember member)
        {
            return new ExpressoResolver(context.WithCurrentMember(member));
        }

        #region TypeDefitinionCache
        public ExpressoResolver WithCurrentTypeDefinition(ITypeDefinition typeDefinition)
        {
            if(CurrentTypeDefinition == typeDefinition)
                return this;

            var type_cache = (typeDefinition != null) ? new TypeDefinitionCache(typeDefinition) : null;
            return new ExpressoResolver(compilation, context.WithCurrentTypeDefinition(typeDefinition), type_cache, local_variable_stack, is_within_lambda_expression);
        }

        sealed class TypeDefinitionCache
        {
            public readonly ITypeDefinition TypeDefinition;
            public readonly Dictionary<string, ResolveResult> SimpleNameLookupCacheExpression = new Dictionary<string, ResolveResult>();
            public readonly Dictionary<string, ResolveResult> SimpleNameLookupInvocationTarget = new Dictionary<string, ResolveResult>();
            public readonly Dictionary<string, ResolveResult> SimpleTypeLookupCache = new Dictionary<string, ResolveResult>();

            public TypeDefinitionCache(ITypeDefinition typeDefinition)
            {
                TypeDefinition = typeDefinition;
            }
        }
        #endregion

        #region ResolveSimpleName
        public ResolveResult ResolveSimpleName(string identifier, IList<IType> typeArgs, bool isInvocationTarget = false)
        {
            return LookupSimpleNameOrTypeName(identifier, typeArgs, isInvocationTarget ? NameLookupMode.InvocationTarget : NameLookupMode.Expression);
        }

        public ResolveResult LookupSimpleNameOrTypeName(string identifier, IList<IType> typeArgs, NameLookupMode nameLookupMode)
        {
            if(identifier == null)
                throw new ArgumentNullException(nameof(identifier));
            
            if(typeArgs == null)
                throw new ArgumentNullException(nameof(typeArgs));

            int k = typeArgs.Count;

            if(k == 0){
                if(nameLookupMode == NameLookupMode.Expression || nameLookupMode == NameLookupMode.InvocationTarget){
                    // Look in local variables
                    foreach(var v in local_variable_stack){
                        if(v.Name == identifier)
                            return new LocalResolveResult(v);
                    }

                    // Look in paremters of current method
                    if(CurrentMember is IParameterizedMember parameterized_member){
                        foreach(var p in parameterized_member.Parameters){
                            if(p.Name == identifier)
                                return new LocalResolveResult(p);
                        }
                    }
                }

                // Look in type parameters of current method
                if(CurrentMember is IMethod method){
                    foreach(var tp in method.TypeParameters){
                        if(tp.Name == identifier)
                            return new TypeResolveResult(tp);
                    }
                }
            }

            bool parameterize_result_type = !(typeArgs.Count != 0 && typeArgs.All(t => t.Kind == TypeKind.UnboundTypeArgument));

            ResolveResult result = null;
            if(type_def_cache != null){
                Dictionary<string, ResolveResult> tmp_cache = null;
                bool found_in_cache = false;
                if(k == 0){
                    switch(nameLookupMode){
                    case NameLookupMode.Expression:
                        tmp_cache = type_def_cache.SimpleNameLookupCacheExpression;
                        break;

                    case NameLookupMode.InvocationTarget:
                        tmp_cache = type_def_cache.SimpleNameLookupInvocationTarget;
                        break;

                    case NameLookupMode.Type:
                        tmp_cache = type_def_cache.SimpleTypeLookupCache;
                        break;
                    }

                    if(tmp_cache != null){
                        lock(tmp_cache)
                            found_in_cache = tmp_cache.TryGetValue(identifier, out result);
                    }
                }

                if(found_in_cache){
                    result = (result != null) ? result.ShallowClone() : null;
                }else{
                    result = LookInCurrentType(identifier, typeArgs, nameLookupMode, parameterize_result_type);
                    if(tmp_cache != null){
                        // Cache missing members as well
                        lock(tmp_cache)
                            tmp_cache[identifier] = result;
                    }

                    if(result != null)
                        return result;
                }
            }

            return new UnknownIdentifierResolveResult(identifier, typeArgs.Count);
        }

        ResolveResult LookInCurrentType(string identifier, IList<IType> typeArgs, NameLookupMode lookupMode, bool parameterizeResultType)
        {
            int k = typeArgs.Count;
            var lookup = CreateMemberLookup(lookupMode);

            // Look in current type definitions, ascending the type inheritance hierarchy
            for(var t = CurrentTypeDefinition; t != null; t = t.DeclaringTypeDefinition){
                if(k == 0){
                    // Look for type parameter with that name
                    var type_params = t.TypeParameters;
                    // Look at all type parameters, including those copied from outer classes,
                    // so that we can fetch the version with the correct owner.
                    foreach(var tp in t.TypeParameters){
                        if(tp.Name == identifier)
                            return new TypeResolveResult(tp);
                    }
                }

                if(lookupMode == NameLookupMode.BaseTypeReference && t == CurrentTypeDefinition){
                    // Don't look in current type when resolving a base type reference
                    continue;
                }

                ResolveResult result;
                if(lookupMode == NameLookupMode.Expression || lookupMode == NameLookupMode.InvocationTarget){
                    var target_resolve_result = (t == CurrentTypeDefinition ? ResolveThisReference() : new TypeResolveResult(t));
                    result = lookup.Lookup(target_resolve_result, identifier, typeArgs, lookupMode == NameLookupMode.InvocationTarget);
                }else{
                    result = lookup.LookupType(t, identifier, typeArgs, parameterizeResultType);
                }

                // but do return AmbiguousMemberResolveResult
                if(!(result is UnknownMemberResolveResult))
                    return result;
            }

            return null;
        }
        #endregion

        #region ResolveCast
        public ResolveResult ResolveCast(IType targetType, ResolveResult expression)
        {
            return null;
        }
        #endregion

        #region ResolveMemberReference
        public MemberLookup CreateMemberLookup()
        {
            //bool is_in_enum_member_initializer = CurrentMember != null && CurrentMember.SymbolKind == SymbolKind.Field
            //                                                                           && CurrentTypeDefinition != null && CurrentTypeDefinition.Kind == TypeKind.Enum;
            return new MemberLookup(CurrentTypeDefinition, compilation.MainAssembly);
        }

        public MemberLookup CreateMemberLookup(NameLookupMode lookupMode)
        {
            if(lookupMode == NameLookupMode.BaseTypeReference && CurrentTypeDefinition != null){
                // When looking up a base type reference, treat us as being outside the current type definition
                // for accessibility purposes.
                // This avoids a stack overflow when referencing a protected class nested inside the base class
                // of a parent class. (NameLookupTests.InnerClassInheritingFromProtectedBaseInnerClassShouldNotCauseStackOverflow)
                return new MemberLookup(this.CurrentTypeDefinition.DeclaringTypeDefinition, this.Compilation.MainAssembly);
            }else{
                return CreateMemberLookup();
            }
        }
        #endregion

        #region ResolveLiteral
        public ResolveResult ResolveLiteral(object value)
        {
            if(value == null){
                return new ResolveResult(SpecialType.NullType);
            }else{
                var type_code = Type.GetTypeCode(value.GetType());
                var type = compilation.FindType(type_code);
                return new ConstantResolveResult(type, value);
            }
        }
        #endregion

        #region Resolve This reference
        public ResolveResult ResolveThisReference()
        {
            var t = CurrentTypeDefinition;
            if(t != null){
                if(t.TypeParameterCount != 0){
                    return new ThisResolveResult(new ParameterizedType(t, t.TypeParameters));
                }else{
                    return new ThisResolveResult(t);
                }
            }

            return errorResult;
        }
        #endregion
    }
}

