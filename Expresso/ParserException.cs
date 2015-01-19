using System;


namespace Expresso.Ast.Analysis
{
    public class ParserException : Exception
    {
        public object[] Objects{get; private set;}

        public ParserException()
        {
        }

        public ParserException(string message) : base(message)
        {
        }

        public ParserException(string formatMessage, params object[] values) : base(formatMessage)
        {
            Objects = values;
        }

        public ParserException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}

