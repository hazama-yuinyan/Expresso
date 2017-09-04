﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast
{
    /// <summary>
    /// Represents a try statement.
    /// A try statement encloses a block and captures any exceptions that are thrown within the block
    /// and then handles them.
    /// "try" Block { "catch" PatternConstruct Block } [ "finally" Block ] ;
    /// </summary>
    public class TryStatement : Statement
    {
        public static readonly Role<CatchClause> CatchRole =
            new Role<CatchClause>("CatchClause", CatchClause.Null);
        public static readonly Role<FinallyClause> FinallyRole =
            new Role<FinallyClause>("FinallyClause", FinallyClause.Null);
        
        public BlockStatement EnclosingBlock{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }

        public AstNodeCollection<CatchClause> CatchClauses{
            get{return GetChildrenByRole(CatchRole);}
        }

        public FinallyClause FinallyClause{
            get{return GetChildByRole(FinallyRole);}
            set{SetChildByRole(FinallyRole, value);}
        }

        public TryStatement(BlockStatement block, IEnumerable<CatchClause> catches, FinallyClause @finally, TextLocation loc)
            : base(loc, (@finally != null) ? @finally.EndLocation : catches.Last().EndLocation)
        {
            EnclosingBlock = block;
            FinallyClause = @finally;
            if(catches != null)
                CatchClauses.AddRange(catches);
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitTryStatement(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitTryStatement(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitTryStatement(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as TryStatement;
            return o != null && EnclosingBlock.DoMatch(o.EnclosingBlock, match) && CatchClauses.DoMatch(o.CatchClauses, match)
                                              && FinallyClause.DoMatch(o.FinallyClause, match);
        }
    }

    /// <summary>
    /// Represents a catch clause.
    /// A catch clause is a block that pattern-matches thrown objects and handles them.
    /// </summary>
    public class CatchClause : Expression
    {
        #region Null
        public static new CatchClause Null = new NullCatchClause();

        sealed class NullCatchClause : CatchClause
        {
            public override bool IsNull {
                get {
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

            internal protected override bool DoMatch(AstNode other, Match match)
            {
                return other == null || other.IsNull;
            } 
        }
        #endregion

        /// <summary>
        /// The pattern that is seen if it is matched.
        /// </summary>
        /// <value>The pattern.</value>
        public PatternConstruct Pattern{
            get{return GetChildByRole(Roles.Pattern);}
            set{SetChildByRole(Roles.Pattern, value);}
        }

        /// <summary>
        /// The body block that is executed if the pattern is matched.
        /// </summary>
        /// <value>The body.</value>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }

        public CatchClause()
        {
        }

        public CatchClause(PatternConstruct pattern, BlockStatement block, TextLocation loc)
            : base(loc, block.EndLocation)
        {
            Pattern = pattern;
            Body = block;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitCatchClause(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitCatchClause(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitCatchClause(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as CatchClause;
            return o != null && Pattern.DoMatch(o.Pattern, match) && Body.DoMatch(o.Body, match);
        }
    }

    /// <summary>
    /// Represents a finally clause.
    /// A finally clause is a catch-all clause for a try statement.
    /// The execution will enter the finally clause no matter what happened before.
    /// </summary>
    public class FinallyClause : Expression
    {
        #region Null
        public static new FinallyClause Null = new NullFinallyClause();

        sealed class NullFinallyClause : FinallyClause
        {
            public override bool IsNull {
                get {
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

            internal protected override bool DoMatch (AstNode other, Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        /// <summary>
        /// The body of the finally clause
        /// </summary>
        /// <value>The body.</value>
        public BlockStatement Body{
            get{return GetChildByRole(Roles.Body);}
            set{SetChildByRole(Roles.Body, value);}
        }

        public FinallyClause()
        {
        }

        public FinallyClause(BlockStatement body, TextLocation loc)
            : base(loc, body.EndLocation)
        {
            Body = body;
        }

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitFinallyClause(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitFinallyClause(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitFinallyClause(this, data);
        }

        protected internal override bool DoMatch(AstNode other, Match match)
        {
            var o = other as FinallyClause;
            return o != null && Body.DoMatch(o.Body, match);
        }
    }
}