using System;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// A special <see cref="Expresso.Ast.AstType"/> implementation that represents a placeholder node.
    /// It will be used in places where inference or type substitution is expected.
    /// </summary>
    public class PlaceholderType : AstType
    {
        public PlaceholderType(TextLocation start)
            : base(start, TextLocation.Empty)
        {
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            return other is PlaceholderType;
        }

        #endregion

        #region implemented abstract members of AstType

        public override ICSharpCode.NRefactory.TypeSystem.ITypeReference ToTypeReference(NameLookupMode lookupMode, ICSharpCode.NRefactory.TypeSystem.InterningProvider interningProvider = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

