using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents an attribute.
    /// Identifier '(' { [ ',' ] CondExpr } ')' ;
    /// </summary>
    public class AttributeNode : AstNode
    {
        public override NodeType NodeType => NodeType.Unknown;

        /// <summary>
        /// Represents the attribute's type.
        /// </summary>
        /// <value>The type.</value>
        public AstType Type{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
        }

        /// <summary>
        /// Represents the arugments passed to the attribute type.
        /// </summary>
        /// <value>The arguments.</value>
        public AstNodeCollection<Expression> Arguments => GetChildrenByRole(Roles.Expression);

        public AttributeNode(AstType type, IEnumerable<Expression> arguments, TextLocation start, TextLocation end)
            : base(start, end)
        {
            Type = type;
            if(arguments != null)
                Arguments.AddRange(arguments);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitAttributeNode(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitAttributeNode(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitAttributeNode(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as AttributeNode;
            return o != null && Type.DoMatch(o.Type, match) && Arguments.DoMatch(o.Arguments, match);
        }
    }
}
