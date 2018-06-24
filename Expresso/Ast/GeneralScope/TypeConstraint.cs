using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a type constraint.
    /// '&lt;' Identifier { ',' Identifier } '&gt;'
    /// "where" Identifier ':' Type { Identifier ':' Type }
    /// </summary>
    public class TypeConstraint : AstNode
    {
        public override NodeType NodeType => NodeType.Unknown;

        /// <summary>
        /// Represents a type parameter.
        /// </summary>
        /// <value>The type of the target.</value>
        public ParameterType TypeParameter{
            get => GetChildByRole(Roles.TypeParameter);
            set => SetChildByRole(Roles.TypeParameter, value);
        }

        /// <summary>
        /// Gets the type constraint.
        /// </summary>
        /// <value>The type constraint.</value>
        public AstNodeCollection<AstType> TypeConstraints => GetChildrenByRole(Roles.Type);

        public TypeConstraint(ParameterType parameter, IEnumerable<AstType> typeConstraints, TextLocation loc)
            : base(loc, (typeConstraints != null) ? typeConstraints.Last().EndLocation : parameter.EndLocation)
        {
            TypeParameter = parameter;
            if(typeConstraints != null)
                TypeConstraints.AddRange(typeConstraints);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitTypeConstraint(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitTypeConstraint(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitTypeConstraint(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as TypeConstraint;
            return o != null && TypeParameter.DoMatch(o.TypeParameter, match) && TypeConstraints.DoMatch(o.TypeConstraints, match);
        }
    }
}
