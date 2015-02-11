using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents a nested type.
    /// A nested type is notated as a path like "std::collections::vector".
    /// </summary>
    public class MemberType : AstType
    {
        public static readonly Role<AstType> TargetRole = new Role<AstType>("Target", AstType.Null);

        /// <summary>
        /// Gets or sets the target type.
        /// </summary>
        /// <value>The target.</value>
        public AstType Target{
            get{return GetChildByRole(TargetRole);}
            set{SetChildByRole(TargetRole, value);}
        }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        /// <value>The name of the member.</value>
        public string MemberName{
            get{return MemberNameToken.Name;}
        }

        public Identifier MemberNameToken{
            get{return GetChildByRole(Roles.Identifier);}
            set{SetChildByRole(Roles.Identifier, value);}
        }

        public AstNodeCollection<AstType> TypeArguments{
            get{return GetChildrenByRole(Roles.TypeArgument);}
        }

        public MemberType(AstType target, Identifier member)
        {
            Target = target;
            MemberNameToken = member;
        }

        public MemberType(AstType target, Identifier member, IEnumerable<AstType> typeArgs)
        {
            Target = target;
            MemberNameToken = member;
            foreach(var type_arg in typeArgs)
                AddChild(type_arg, Roles.TypeArgument);
        }

        public MemberType(AstType target, Identifier member, params AstType[] typeArgs)
            : this(target, member, (IEnumerable<AstType>)typeArgs)
        {
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
            return o != null && Target.DoMatch(o.Target, match) && MatchString(MemberName, o.MemberName)
                && TypeArguments.DoMatch(o.TypeArguments, match);
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

