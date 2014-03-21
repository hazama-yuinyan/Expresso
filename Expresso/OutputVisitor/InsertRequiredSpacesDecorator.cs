using System;


namespace Expresso.Ast
{
    class InsertRequiredSpacesDecorator : DecoratingTokenWriter
    {
        /// <summary>
        /// Used to insert the minimal amount of spaces so that the lexer recognizes the tokens that were written.
        /// </summary>
        LastWritten last_written;

        enum LastWritten
        {
            Whitespace,
            Other,
            KeywordOrIdentifier,
            Plus,
            Minus,
            Ampersand,
            QuestionMark,
            Division
        }

        public InsertRequiredSpacesDecorator(TokenWriter writer) : base(writer)
        {
        }

        public override void WriteIdentifier(Identifier ident)
        {
            if(ident.IsVerbatim || ExpressoOutputWalker.IsKeyword(ident.Name, ident)){
                if(last_written == LastWritten.KeywordOrIdentifier){
                    // This space is not strictly required, so we delegate to Space()
                    Space();
                }
            }else if(last_written == LastWritten.KeywordOrIdentifier){
                // This space is strictly required, so we directly call the formatter
                base.Space();
            }

            base.WriteIdentifier(ident);
            last_written = LastWritten.KeywordOrIdentifier;
        }
    }
}

