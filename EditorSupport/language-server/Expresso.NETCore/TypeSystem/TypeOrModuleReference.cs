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
        public abstract ResolveResult Resolve(ExpressoResolver resolver);

        /// <summary>
        /// Returns the type that is referenced; or an <see cref="UnknownType"/> if the type isn't found.
        /// </summary>
        /// <returns>The type.</returns>
        public abstract IType ResolveType(ExpressoResolver resolver);

        #region ITypeReference implementation

        public IType Resolve(ITypeResolveContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

