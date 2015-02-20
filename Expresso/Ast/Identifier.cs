using System;


namespace Expresso.Ast
{
    /// <summary>
    /// 識別子。
	/// Reperesents a symbol.
    /// Identifiers should not be associated with AstType nodes if they just refer to types
    /// because doing so creates cyclic reference and thus causes problems.
    /// </summary>
    public class Identifier : AstNode
    {
        #region Null
        public static new Identifier Null = new NullIdentifier();
        sealed class NullIdentifier : Identifier
        {
            public override bool IsNull{
                get{
                    return true;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitNullNode(this, data);
            }

            internal protected override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

		readonly string name;

        /// <summary>
        /// 識別子名。
		/// The name of the identifier.
        /// </summary>
        public string Name{
            get{return name;}
		}

        public bool IsVerbatim{
            get{
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the identifier id. This id represents the identity or uniqueness of that node
        /// within a whole program.
        /// If some 2 Identifier nodes have the same id, it means that the 2 nodes refer to the same value.
        /// This will get set during name binding.
        /// </summary>
        /// <remarks>0 is considered invalid for IdentifierId.</remarks>
        public uint IdentifierId{
            get; internal set;
        }

		/// <summary>
		/// 変数の型。
		/// The type of the variable.
		/// </summary>
        public AstType Type{
            get{return GetChildByRole(Roles.Type);}
            set{SetChildByRole(Roles.Type, value);}
		}

        public override NodeType NodeType{
            get{return NodeType.Unknown;}
        }

        protected Identifier()
        {
            IdentifierId = 0;
        }

        public Identifier(string name)
		{
            this.name = name;
            IdentifierId = 0;
		}

        public Identifier(string name, AstType type)
        {
            this.name = name;
            Type = type;
            IdentifierId = 0;
        }

        public override void AcceptWalker(IAstWalker walker)
		{
            walker.VisitIdentifier(this);
		}

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitIdentifier(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitIdentifier(this, data);
        }

        #region implemented abstract members of AstNode

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as Identifier;
            return o != null && MatchString(Name, o.Name) && Type.DoMatch(o.Type, match);
        }

        #endregion
    }
}
