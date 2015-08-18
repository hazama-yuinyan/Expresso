using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;


namespace Expresso.CodeGen
{
    /// <summary>
    /// Collects all the variables declared in one scope.
    /// Note that it isn't going to look through any nested scopes.
    /// </summary>
    public class ParameterCollector : ExpressionVisitor
    {
        List<ParameterExpression> parameters;

        public IEnumerable<ParameterExpression> Parameters{
            get{
                return parameters.Distinct();
            }
        }

        void AddParameter(Expression expr)
        {
            var param = expr as ParameterExpression;
            if(param != null)
                parameters.Add(param);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var lhs = Visit(node.Left);
            AddParameter(lhs);
            var rhs = Visit(node.Right);
            AddParameter(rhs);
            return null;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            var cond = Visit(node.Test);
            AddParameter(cond);
            var lhs = Visit(node.IfTrue);
            AddParameter(lhs);
            var rhs = Visit(node.IfFalse);
            AddParameter(rhs);
            return null;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            var args = Visit(node.Arguments);
            foreach(var arg in args)
                AddParameter(arg);

            return null;
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            var val = Visit(node.Value);
            AddParameter(val);
            return null;
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            var args = Visit(node.Arguments);
            foreach(var arg in args)
                AddParameter(arg);

            var obj = Visit(node.Object);
            AddParameter(obj);
            return null;
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            var args = Visit(node.Arguments);
            foreach(var arg in args)
                AddParameter(arg);

            var expr = Visit(node.Expression);
            AddParameter(expr);
            return null;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            foreach(var init in node.Initializers)
                Visit(init);

            Visit(node.NewExpression);
            return null;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var expr = Visit(node.Expression);
            AddParameter(expr);
            return null;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var expr = Visit(node.Expression);
            AddParameter(expr);
            return null;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Visit(node.NewExpression);
            return null;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            Visit(node.Initializers);
            return null;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var args = Visit(node.Arguments);
            foreach(var arg in args)
                AddParameter(arg);

            var obj = Visit(node.Object);
            AddParameter(obj);
            return null;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var args = Visit(node.Arguments);
            foreach(var arg in args)
                AddParameter(arg);

            return null;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var exprs = Visit(node.Expressions);
            foreach(var expr in exprs)
                AddParameter(expr);

            return null;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            var expr = Visit(node.Expression);
            AddParameter(expr);
            return null;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var expr = Visit(node.Operand);
            AddParameter(expr);
            return null;
        }
    }
}

