using System;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// Represents a fatal error in parsing phase.
    /// Because throwing a ParserException causes the program to immediately halt
    /// use it when you absolutely need to do so.
    /// </summary>
    public class ParserException : Exception
    {
        AstNode node, node2;

        public ParserException()
        {
        }

        public ParserException(string message, AstNode node) : base(message)
        {
            this.node = node;
        }

        public ParserException(string formatMessage, AstNode node, params object[] values)
            : base(string.Format(formatMessage, values))
        {
            this.node = node;
        }

        public ParserException(string format, AstNode node, AstNode node2, params object[] values)
            : base(string.Format(format, values))
        {
            this.node = node;
            this.node2 = node2;
        }

        public ParserException(string message, AstNode node, Exception exception) : base(message, exception)
        {
            this.node = node;
        }

        public override string ToString()
        {
            if(node2 != null)
                return string.Format("{0} ~ {1} -- {2}", node.StartLocation, node2.StartLocation, Message);
            else
                return string.Format("{0} -- {1}", node.StartLocation, Message);
        }
    }
}

