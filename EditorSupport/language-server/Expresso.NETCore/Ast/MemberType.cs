using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a nested type.
    /// A nested type is notated as a path like "TestModule::TestClass".
    /// </summary>
    public class MemberType : AstType
    {
        public static readonly Role<AstType> TargetRole = new Role<AstType>("Target", AstType.Null);
        public static readonly Role<SimpleType> ChildRole = new Role<SimpleType>("Child", SimpleType.Null);

        /// <summary>
        /// Gets or sets the target type.
        /// The target type is the parent type.
        /// </summary>
        public AstType Target{
            get{return GetChildByRole(TargetRole);}
            set{SetChildByRole(TargetRole, value);}
        }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        public string MemberName{
            get{return ChildType.Name;}
        }

        public override string Name{
            get{return MemberName;}
        }

        public override Identifier IdentifierNode{
            get{return ChildType.IdentifierNode;}
        }

        /// <summary>
        /// Gets or sets the child type.
        /// </summary>
        public SimpleType ChildType{
            get{return GetChildByRole(ChildRole);}
            set{SetChildByRole(ChildRole, value);}
        }

        public MemberType(AstType target, SimpleType childType, TextLocation loc)
            : base(target.StartLocation, loc)
        {
            Target = target;
            ChildType = childType;
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitMemberType(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitMemberType(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitMemberType(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as MemberType;
            return o != null && Target.DoMatch(o.Target, match)
                && ChildType.DoMatch(o.ChildType, match);
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

