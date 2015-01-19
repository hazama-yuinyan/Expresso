using System;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Semantics;


namespace Expresso.TypeSystem
{
    public class SimpleTypeOrModule : TypeOrModuleReference, ISupportsInterning
    {
        public SimpleTypeOrModule()
        {
        }

        #region implemented abstract members of TypeOrModuleReference

        public override ResolveResult Resolve(Expresso.Resolver.ExpressoResolver resolver)
        {
            throw new NotImplementedException();
        }

        public override IType ResolveType(Expresso.Resolver.ExpressoResolver resolver)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISupportsInterning implementation

        public int GetHashCodeForInterning()
        {
            throw new NotImplementedException();
        }

        public bool EqualsForInterning(ISupportsInterning other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

