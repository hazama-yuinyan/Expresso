using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;

namespace Expresso.Ast
{
    /// <summary>
    /// A special <see cref="Expresso.Ast.AstType"/> implementation that represents a placeholder node.
    /// It will be used in places where inference or type substitution (including functions, whether
    /// they may contain inference-required types or not) is expected or it may indicate that
    /// any types will be matched.
    /// </summary>
    public class PlaceholderType : AstType
    {
        public override string Name => null;

        public override Identifier IdentifierNode => Identifier.Null;

        public PlaceholderType(TextLocation loc)
            : base(loc, loc)
        {
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitPlaceholderType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitPlaceholderType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitPlaceholderType(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            // A placeholder can be substituted for any type node
            return true;
        }

        #endregion

        #region implemented abstract members of AstType

        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
        {
            return null;
        }

        #endregion
    }
}

