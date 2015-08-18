using System;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// Represents a fatal error in parsing phase.
    /// Because throwing a ParserException causes the process to immediately halt
    /// use it when you absolutely need to do so.
    /// </summary>
    public class ParserException : Exception
    {
        public ParserException()
        {
        }

        public ParserException(string message) : base(message)
        {
        }

        public ParserException(string formatMessage, params object[] values)
            : base(string.Format(formatMessage, values))
        {
        }

        public ParserException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}

