using System;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;


namespace Expresso.Ast
{
    /// <summary>
    /// A type reference in Expresso AST.
    /// </summary>
    public abstract class AstType : AstNode
    {
        #region Null
        public static readonly AstType NullType = new NullAstType();
        sealed class NullAstType : AstType
        {

        }
        #endregion

        #region PatternPlaceholder
        public static implicit operator AstType(Pattern pattern)
        {
            return (pattern != null) ? new PatternPlaceholder(pattern) : null;
        }

        sealed class PatternPlaceholder : AstType, INode
        {
            readonly Pattern child;

            public PatternPlaceholder(Pattern child)
            {
                this.child = child;
            }

            #region INode implementation

            public bool DoMatch(INode other, Match match)
            {
                throw new NotImplementedException();
            }

            public bool DoMatchCollection(ICSharpCode.NRefactory.Role role, INode pos, Match match, BacktrackingInfo backtrackingInfo)
            {
                throw new NotImplementedException();
            }

            public ICSharpCode.NRefactory.Role Role{
                get{
                    throw new NotImplementedException();
                }
            }

            INode INode.FirstChild{
                get{
                    throw new NotImplementedException();
                }
            }

            INode INode.NextSibling{
                get{
                    throw new NotImplementedException();
                }
            }

            public bool IsNull{
                get{
                    throw new NotImplementedException();
                }
            }

            #endregion


        }
        #endregion

        public override NodeType Type{
            get{

            }
        }

        public AstType()
        {
        }

        public new AstType Clone()
        {
            return (AstType)base.MemberwiseClone();
        }

        #region implemented abstract members of AstNode
        public override void AcceptWalker(AstWalker walker)
        {
            throw new NotImplementedException();
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            throw new NotImplementedException();
        }

        public override string GetText()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

