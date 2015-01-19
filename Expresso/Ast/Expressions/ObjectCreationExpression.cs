﻿using System;
using System.Collections.Generic;
using System.Linq;

using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    /// <summary>
    /// オブジェクト生成式。
    /// Represents an object creation expression.
    /// An object creation expression appears in places that rvalues are expected
    /// and it creates a new object of type `Type` on the stack(if used without the "new" keyword)
    /// or on the heap(if the "new" keyword is preceded).
    /// Path '{' { ident ':' Expression } '}' ;
    /// </summary>
    public class ObjectCreationExpression : Expression
    {
        /// <summary>
        /// The path expression to create an instance out of.
        /// </summary>
        /// <value>The path.</value>
        public PathExpression Path{
            get{return GetChildByRole(Roles.Path);}
            set{SetChildByRole(Roles.Path, value);}
        }

        /// <summary>
        /// Gets all the field names.
        /// </summary>
        /// <value>The field names.</value>
        public AstNodeCollection<KeyValueLikeExpression> Items{
            get{return GetChildrenByRole(Roles.KeyValue);}
        }

        public ObjectCreationExpression(PathExpression path, IEnumerable<PathExpression> fields, IEnumerable<Expression> values)
        {
            Path = path;
            foreach(var item in fields.Zip(values, (field, value) => new KeyValueLikeExpression(field, value)))
                AddChild(item, Roles.KeyValue);
        }

        #region implemented abstract members of AstNode

        public override void AcceptWalker(IAstWalker walker)
        {
            walker.VisitObjectCreationExpression(this);
        }

        public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
        {
            return walker.VisitObjectCreationExpression(this);
        }

        public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
        {
            return walker.VisitObjectCreationExpression(this, data);
        }

        protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
        {
            var o = other as ObjectCreationExpression;
            return o != null && Items.DoMatch(o.Items, match);
        }

        #endregion
    }
}
