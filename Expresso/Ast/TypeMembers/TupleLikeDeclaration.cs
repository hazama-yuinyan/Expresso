using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a tuple-like declaration, such as a variant of enums.
    /// Identifier ( '(' ')' | '(' Type { ',' Type } ')' ) ;
    /// </summary>
    public class TupleLikeDeclaration : EntityDeclaration
    {
        public override string Name => NameToken.Name;
        public override Identifier NameToken => GetChildByRole(Roles.Identifier);

        /// <summary>
        /// Represents the types.
        /// </summary>
        /// <value>The types.</value>
        public AstNodeCollection<AstType> Types => GetChildrenByRole(Roles.Type);

        public override SymbolKind SymbolKind => SymbolKind.Field;

        public TupleLikeDeclaration(Identifier identifier, IEnumerable<AstType> types, TextLocation start, TextLocation end)
            : base(start, end)
        {
            SetChildByRole(Roles.Identifier, identifier);
            Types.AddRange(types);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitTupleLikeDeclaration(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitTupleLikeDeclaration(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitTupleLikeDeclaration(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as TupleLikeDeclaration;
            return o != null && NameToken.DoMatch(o.NameToken, match) && Types.DoMatch(o.Types, match);
        }
    }
}
