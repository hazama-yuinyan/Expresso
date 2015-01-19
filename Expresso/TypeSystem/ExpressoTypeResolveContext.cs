using System;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.TypeSystem
{
    /// <summary>
    /// Type resolve context for Expresso.
    /// </summary>
    public sealed class ExpressoTypeResolveContext : ITypeResolveContext
    {
        readonly IAssembly assembly;
        readonly ITypeDefinition current_type_def;
        readonly IMember current_member;

        public ExpressoTypeResolveContext(IAssembly assembly, ITypeDefinition typeDefinition = null, IMember currentMember = null)
        {
            if(assembly == null)
                throw new ArgumentNullException("assembly");

            this.assembly = assembly;
            current_type_def = typeDefinition;
            current_member = currentMember;
        }

        public ExpressoTypeResolveContext WithCurrentTypeDefinition(ITypeDefinition typeDefinition)
        {
            return new ExpressoTypeResolveContext(assembly, typeDefinition, current_member);
        }

        public ExpressoTypeResolveContext WithCurrentMember(IMember member)
        {
            return new ExpressoTypeResolveContext(assembly, current_type_def, member);
        }

        #region ITypeResolveContext implementation

        ITypeResolveContext ITypeResolveContext.WithCurrentTypeDefinition(ITypeDefinition typeDefinition)
        {
            return WithCurrentTypeDefinition(typeDefinition);
        }

        ITypeResolveContext ITypeResolveContext.WithCurrentMember(IMember member)
        {
            return WithCurrentMember(member);
        }

        public IAssembly CurrentAssembly{
            get{
                return assembly;
            }
        }

        public ITypeDefinition CurrentTypeDefinition{
            get{
                return current_type_def;
            }
        }

        public IMember CurrentMember{
            get{
                return current_member;
            }
        }

        #endregion

        #region ICompilationProvider implementation

        public ICompilation Compilation{
            get{
                return assembly.Compilation;
            }
        }

        #endregion
    }
}

