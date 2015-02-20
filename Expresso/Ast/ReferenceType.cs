using System;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a reference type.
    /// A reference type is a reference type to some other type.
    /// </summary>
    public class ReferenceType : AstType
    {
        public AstType BaseType{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        public ReferenceType(AstType type, TextLocation loc)
            : base(loc, TextLocation.Empty)
        {
            BaseType = type;
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitReferenceType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitReferenceType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitReferenceType(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as ReferenceType;
            return o != null && BaseType.DoMatch(o.BaseType, match);
        }

        #endregion

        #region implemented abstract members of AstType

        public override ITypeReference ToTypeReference(NameLookupMode lookupMode, InterningProvider interningProvider = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

