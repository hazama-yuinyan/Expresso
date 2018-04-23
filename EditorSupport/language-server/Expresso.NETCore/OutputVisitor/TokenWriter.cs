using System;
using System.IO;
using ICSharpCode.NRefactory;


namespace Expresso.Ast
{
    public abstract class TokenWriter
    {
        public abstract void StartNode(AstNode node);
        public abstract void EndNode(AstNode node);

        /// <summary>
        /// Writes an identifier.
        /// </summary>
        public abstract void WriteIdentifier(Identifier ident);

        /// <summary>
        /// Writes a keyword to output.
        /// </summary>
        public abstract void WriteKeyword(Role role, string keyword);

        /// <summary>
        /// Writes a token to output.
        /// </summary>
        public abstract void WriteToken(Role role, string token);

        /// <summary>
        /// Writes a primitive/literal value.
        /// </summary>
        public abstract void WritePrimitiveValue(object value, string literalValue = null);

        public abstract void WritePrimitiveType(string type);

        public abstract void Space();
        public abstract void Indent();
        public abstract void Unindent();
        public abstract void NewLine();

        public abstract void WriteComment(CommentType commentType, string content);

        public static TokenWriter Create(TextWriter writer, string indentation = "\t")
        {
            return new InsertSpecialsDecorator(
                new InsertRequiredSpaceDecorator(
                    new TextWriterTokenWriter(writer){Indentation = indentation}
                )
            );
        }
    }

    public interface ILocatable
    {
        TextLocation Location{get;}
    }

    public abstract class DecoratingTokenWriter : TokenWriter
    {
        TokenWriter inner_writer;

        protected DecoratingTokenWriter(TokenWriter innerWriter)
        {
            if(innerWriter == null)
                throw new ArgumentNullException("innerWriter");

            inner_writer = innerWriter;
        }

        public override void StartNode(AstNode node)
        {
            inner_writer.StartNode(node);
        }

        public override void EndNode(AstNode node)
        {
            inner_writer.EndNode(node);
        }

        public override void WriteIdentifier(Identifier ident)
        {
            inner_writer.WriteIdentifier(ident);
        }

        public override void WriteKeyword(Role role, string keyword)
        {
            inner_writer.WriteKeyword(role, keyword);
        }

        public override void WriteToken(Role role, string token)
        {
            inner_writer.WriteToken(role, token);
        }

        public override void WritePrimitiveValue(object value, string literalValue)
        {
            inner_writer.WritePrimitiveValue(value, literalValue);
        }

        public override void WritePrimitiveType(string type)
        {
            inner_writer.WritePrimitiveType(type);
        }

        public override void Space()
        {
            inner_writer.Space();
        }

        public override void Indent()
        {
            inner_writer.Indent();
        }

        public override void Unindent()
        {
            inner_writer.Unindent();
        }

        public override void NewLine()
        {
            inner_writer.NewLine();
        }

        public override void WriteComment(CommentType commentType, string content)
        {
            inner_writer.WriteComment(commentType, content);
        }
    }
}

