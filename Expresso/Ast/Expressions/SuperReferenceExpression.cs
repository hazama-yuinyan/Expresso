using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the keyword "super" as expression.
    /// The super keyword references the parent object that is currently in scope.
    /// "super" ;
    /// </summary>
    public class SuperReferenceExpression : Expression
    {
        /// <summary>
        /// Gets the identifier that represents the "super" keyword.
        /// </summary>
        public Identifier SuperIdentifier{
            get => GetChildByRole(Roles.Identifier);
            private set => SetChildByRole(Roles.Identifier, value);
        }

        public SuperReferenceExpression(TextLocation loc)
            : base(loc, new TextLocation(loc.Line, loc.Column + "super".Length))
        {
            SuperIdentifier = MakeIdentifier("super", new PlaceholderType(loc));
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitSuperReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitSuperReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitSuperReferenceExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            return other is SuperReferenceExpression;
        }

        #endregion
    }
}

