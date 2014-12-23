using System;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using Expresso.TypeSystem;


namespace Expresso.Resolver
{
    /// <summary>
    /// Contains main resolve logic.
    /// </summary>
    public class ExpressoResolver : ICodeContext
    {
        static readonly ResolveResult ErrorResult = ErrorResolveResult.UnknownError;
        readonly ICompilation compilation;
        ExpressoTypeResolveContext context;

        #region Constructors
        public ExpressoResolver(ICompilation compilation)
        {
            if(compilation == null)
                throw new ArgumentNullException("compilation");

            this.compilation = compilation;
            context = new ExpressoTypeResolveContext(compilation.MainAssembly);
        }

        public ExpressoResolver(ExpressoTypeResolveContext context)
        {
            this.compilation = context.Compilation;
            this.context = context;
        }
        #endregion

        #region ITypeResolveContext implementation

        public ITypeResolveContext WithCurrentTypeDefinition(ITypeDefinition typeDefinition)
        {
            throw new NotImplementedException();
        }

        public ITypeResolveContext WithCurrentMember(IMember member)
        {
            throw new NotImplementedException();
        }

        public IAssembly CurrentAssembly{
            get{
                throw new NotImplementedException();
            }
        }

        public ITypeDefinition CurrentTypeDefinition{
            get{
                throw new NotImplementedException();
            }
        }

        public IMember CurrentMember{
            get{
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICodeContext implementation

        public System.Collections.Generic.IEnumerable<IVariable> LocalVariables{
            get{
                throw new NotImplementedException();
            }
        }

        public bool IsWithinLambdaExpression{
            get{
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICompilationProvider implementation

        public ICompilation Compilation{
            get{
                return compilation;
            }
        }

        #endregion
    }
}

