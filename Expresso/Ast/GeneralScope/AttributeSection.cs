using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents an attribute section.
    /// '[' [ Idenitifer ':' ] Attributes ']' ;
    /// </summary>
    public class AttributeSection : AstNode
    {
        #region Null
        public static readonly new AttributeSection Null = new NullAttributeSection();

        sealed class NullAttributeSection : AttributeSection
        {
            public override bool IsNull => true;

            public override void AcceptWalker(IAstWalker walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker) => walker.VisitNullNode(this);

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data) => walker.VisitNullNode(this, data);

            protected internal override bool DoMatch(AstNode other, Match match) => other == null || other.IsNull;
        }
        #endregion

        public override NodeType NodeType => NodeType.Unknown;

        /// <summary>
        /// Gets the attribute target as a string.
        /// </summary>
        /// <value>The attribute target.</value>
        public string AttributeTarget => AttributeTargetToken.Name;

        /// <summary>
        /// Represents the attribute target.
        /// </summary>
        /// <value>The attribute target token.</value>
        public Identifier AttributeTargetToken{
            get => GetChildByRole(Roles.Identifier);
            set => SetChildByRole(Roles.Identifier, value);
        }

        /// <summary>
        /// Represents the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public AstNodeCollection<ObjectCreationExpression> Attributes => GetChildrenByRole(Roles.ObjectCreation);

        protected AttributeSection()
        {
        }

        public AttributeSection(Identifier target, IEnumerable<ObjectCreationExpression> attributes, TextLocation start, TextLocation end)
            : base(start, end)
        {
            AttributeTargetToken = target;
            Attributes.AddRange(attributes);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitAttributeSection(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitAttributeSection(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitAttributeSection(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as AttributeSection;
            return o != null && AttributeTargetToken.DoMatch(o.AttributeTargetToken, match) && Attributes.DoMatch(o.Attributes, match);
        }

        public static AttributeTargets GetAttributeTargets(string identifier)
        {
            switch(identifier){
            case "assembly":
                return AttributeTargets.Assembly;

            case "module":
                return AttributeTargets.Module;

            case "type":
                return AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum;

            case "field":
                return AttributeTargets.Field;

            case "method":
                return AttributeTargets.Method;

            case "param":
            case "parameter":
                return AttributeTargets.Parameter;

            case "return":
                return AttributeTargets.ReturnValue;

            default:
                throw new InvalidOperationException("Invalid AttributeTargets identifier");
            }
        }
    }
}
