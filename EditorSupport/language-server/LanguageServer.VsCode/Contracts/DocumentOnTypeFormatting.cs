using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LanguageServer.VsCode.Contracts
{
    /// <summary>
    /// Descibe options to be used when registered for document link events.
    /// </summary>
    public class DocumentOnTypeFormattingRegistrationOptions : TextDocumentRegistrationOptions
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.DocumentOnTypeFormattingRegistrationOptions"/> class.
        /// </summary>
        [JsonConstructor]
        public DocumentOnTypeFormattingRegistrationOptions()
            : this(default(char), null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.DocumentOnTypeFormattingRegistrationOptions"/> class.
        /// </summary>
        /// <param name="firstTriggerCharacter">First trigger character.</param>
        public DocumentOnTypeFormattingRegistrationOptions(char firstTriggerCharacter)
            : this(firstTriggerCharacter, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.DocumentOnTypeFormattingRegistrationOptions"/> class.
        /// </summary>
        /// <param name="firstTriggerCharacter">First trigger character.</param>
        /// <param name="moreTriggerCharacter">More trigger character.</param>
        public DocumentOnTypeFormattingRegistrationOptions(char firstTriggerCharacter,
            ICollection<char> moreTriggerCharacter)
            : this(firstTriggerCharacter, moreTriggerCharacter, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.DocumentOnTypeFormattingRegistrationOptions"/> class.
        /// </summary>
        /// <param name="firstTriggerCharacter">First trigger character.</param>
        /// <param name="moreTriggerCharacter">More trigger character.</param>
        /// <param name="documentSelector">Document selector.</param>
        public DocumentOnTypeFormattingRegistrationOptions(char firstTriggerCharacter,
            ICollection<char> moreTriggerCharacter, IEnumerable<DocumentFilter> documentSelector)
            : base(documentSelector)
        {
            FirstTriggerCharacter = firstTriggerCharacter;
            MoreTriggerCharacter = moreTriggerCharacter;
        }

        /// <summary>
        /// A character on which formatting should be triggered.
        /// </summary>
        [JsonProperty]
        public char FirstTriggerCharacter { get; set; }

        /// <summary>
        /// More trigger characters.
        /// </summary>
        [JsonProperty]
        public ICollection<char> MoreTriggerCharacter { get; set; }
    }
}
