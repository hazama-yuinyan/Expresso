using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents the "null" keyword as an AST.
    /// The null indicates nothing in context of interoperating with .NET.
    /// In other contexts, it is prohibited.
    /// "null" ;
    /// </summary>
    public class NullReferenceExpression : Expression
    {
        public Identifier NullIdentifier{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public NullReferenceExpression(TextLocation loc)
            : base(loc, new TextLocation(loc.Line, loc.Column + "null".Length))
        {
            NullIdentifier = MakeIdentifier("null", new PlaceholderType(loc));
        }        

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitNullReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitNullReferenceExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitNullReferenceExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as NullReferenceExpression;
            return o != null && NullIdentifier.DoMatch(o.NullIdentifier, match);
        }
    }
}
