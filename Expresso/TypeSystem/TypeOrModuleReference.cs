using System;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using Expresso.Resolver;


namespace Expresso.TypeSystem
{
    /// <summary>
    /// Represents a reference which could point to a type or a module.
    /// </summary>
    [Serializable]
    public abstract class TypeOrModuleReference : ITypeReference
    {
        /// <summary>
        /// Resolves the reference and returns a ResolveResult.
        /// </summary>
        /// <param name="context">Context to use for resolving this type reference.
        /// Which kind of context is required depends on the which kind of type reference this is;
        /// please consult the documentation of the method that was used to create this type reference,
        /// or that of the class implementing this method.</param>
        /// <param name="resolver">Resolver.</param>
        public abstract ResolveResult Resolve(ExpressoResolver resolver);

        public abstract IType ResolveType(ExpressoResolver resolver);

        #region ITypeReference implementation

        public IType Resolve(ITypeResolveContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

