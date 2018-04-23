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
            IVariable removedVar;
            do{
                removedVar = stack.Peek();
                stack = stack.Pop();
            }while(removedVar != null);

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
    }
}

