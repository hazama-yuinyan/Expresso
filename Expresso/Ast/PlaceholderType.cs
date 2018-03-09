using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// A special <see cref="Expresso.Ast.AstType"/> implementation that represents a placeholder node.
    /// It will be used in places where inference or type substitution (including functions, whether
    /// they may contain inference-required types or not) is expected.
    /// </summary>
    public class PlaceholderType : AstType
    {
        public override string Name{
            get{return null;}
        }

        public override Identifier IdentifierNode{
            get{return Identifier.Null;}
        }

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
            return other is PlaceholderType;
        }

        #endregion

        #region implemented abstract members of AstType

        public override ICSharpCode.NRefactory.TypeSystem.ITypeReference ToTypeReference(NameLookupMode lookupMode, ICSharpCode.NRefactory.TypeSystem.InterningProvider interningProvider = null)
        {
            return null;
        }

        #endregion
    }
}

