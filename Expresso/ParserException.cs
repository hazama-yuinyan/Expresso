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
        readonly AstNode node, node2;
        readonly string error_code;

        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>The error code.</value>
        public string ErrorCode{
            get{return error_code;}
        }

        /// <summary>
        /// It delivers to help messages additional information.
        /// </summary>
        /// <value>The help object.</value>
        public object HelpObject{
            get; set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Expresso.Ast.Analysis.ParserException"/> class with `message`, `errorCode` and `node`.
        /// Use this overload when you don't have a format string.
        /// </summary>
        /// <param name="message">The message string.</param>
        /// <param name="errorCode">The error code. It must contain the string "ES" at the head.</param>
        /// <param name="node">The node at which the error happened. The <see cref="AstNode.StartLocation"/> property for this node will be used.</param>
        public ParserException(string message, string errorCode, AstNode node) : base(string.Format("Error {0}: {1}", errorCode, message))
        {
            this.node = node;
            error_code = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Expresso.Ast.Analysis.ParserException"/> class with `formatMessage`, `errorCode`, `node` and `values`.
        /// Use this overload when you need to format some string.
        /// </summary>
        /// <param name="formatMessage">The format string according to which the values are formatted.</param>
        /// <param name="errorCode">The rrror code. It must contain the string "ES" at the head.</param>
        /// <param name="node">The node at which the error happened. The <see cref="AstNode.StartLocation"/> property for this node will be used.</param>
        /// <param name="values">Values which are formatted into `formatMessage`.</param>
        public ParserException(string formatMessage, string errorCode, AstNode node, params object[] values)
            : base(string.Format("Error {0}: {1}", errorCode, string.Format(formatMessage, values)))
        {
            this.node = node;
            error_code = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Expresso.Ast.Analysis.ParserException"/> class with `format`, `errorCode`, `node`, `node2` and `values`.
        /// Use this overload when the error represents a range.
        /// </summary>
        /// <param name="format">The format string according to which the values are formatted.</param>
        /// <param name="errorCode">The error code. It must contain the string "ES" at the head.</param>
        /// <param name="node">The node which is the start of the error range. The <see cref="AstNode.StartLocation"/> property for this node will be used.</param>
        /// <param name="node2">The node which is the end of the error range. The <see cref="AstNode.StartLocation"/> property for this node will be used.</param>
        /// <param name="values">Values which are formatted into `format`.</param>
        public ParserException(string format, string errorCode, AstNode node, AstNode node2, params object[] values)
            : base(string.Format("Error {0}: {1}", errorCode, string.Format(format, values)))
        {
            this.node = node;
            this.node2 = node2;
            error_code = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Expresso.Ast.Analysis.ParserException"/> class with `message`, `errorCode`, `node` and `excpetion`.
        /// Use this overload when there is an inner exception.
        /// </summary>
        /// <param name="message">The message string.</param>
        /// <param name="errorCode">The error code. It must contain the string "ES" at the head.</param>
        /// <param name="node">The node at which this exception happened. The <see cref="AstNode.StartLocation"/> property for this node will be used.</param>
        /// <param name="exception">The inner exception.</param>
        public ParserException(string message, string errorCode, AstNode node, Exception exception)
            : base(string.Format("Error {0}: {1}", errorCode, message), exception)
        {
            this.node = node;
            error_code = errorCode;
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

