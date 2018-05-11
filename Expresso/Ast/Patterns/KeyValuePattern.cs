using System;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// The pattern that contains a key and value. It's mostly like the <see cref="Expresso.Ast.KeyValueLikeExpression"/> class.
    /// </summary>
    public class KeyValuePattern : PatternConstruct
    {
        /// <summary>
        /// Gets or sets the identifier that represents the key.
        /// </summary>
        /// <value>The key identifier.</value>
        public Identifier KeyIdentifier{
            get => GetChildByRole(Roles.Identifier);
            set => SetChildByRole(Roles.Identifier, value);
        }

        /// <summary>
        /// Gets or sets the expression that represents the value.
        /// </summary>
        /// <value>The value expression.</value>
        public PatternConstruct Value{
            get => GetChildByRole(Roles.Pattern);
            set => SetChildByRole(Roles.Pattern, value);
        }

        public KeyValuePattern(Identifier key, PatternConstruct value)
            : base(key.StartLocation, value.EndLocation)
        {
            KeyIdentifier = key;
            Value = value;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitKeyValuePattern(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitKeyValuePattern(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitKeyValuePattern(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as KeyValuePattern;
            return o != null && KeyIdentifier.DoMatch(o.KeyIdentifier, match) && Value.DoMatch(o.Value, match);
        }
    }
}
