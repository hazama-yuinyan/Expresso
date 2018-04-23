using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LanguageServer.VsCode.Contracts
{
    /// <summary>
    /// A document link is a range in a text document that links to an internal or external resource, like another
    /// text document or a web site.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DocumentLink
    {
        /// <summary>
        /// The range this link applies to.
        /// </summary>
        [JsonProperty]
        public Range Range { get; set; }

        /// <summary>
        /// The range this link applies to. If missing a resolve request is sent later.
        /// </summary>
        [JsonProperty]
        public Uri Uri { get; set; }

    }

    /// <summary>
    /// Descibe options to be used when registered for document link events.
    /// </summary>
    public class DocumentLinkRegistrationOptions : TextDocumentRegistrationOptions
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.DocumentLinkRegistrationOptions"/> class.
        /// </summary>
        [JsonConstructor]
        public DocumentLinkRegistrationOptions()
            : this(false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.DocumentLinkRegistrationOptions"/> class.
        /// </summary>
        /// <param name="resolveProvider">If set to <c>true</c> resolve provider.</param>
        public DocumentLinkRegistrationOptions(bool resolveProvider)
            : this(resolveProvider, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.DocumentLinkRegistrationOptions"/> class.
        /// </summary>
        /// <param name="resolveProvider">If set to <c>true</c> resolve provider.</param>
        /// <param name="documentSelector">Document selector.</param>
        public DocumentLinkRegistrationOptions(bool resolveProvider, IEnumerable<DocumentFilter> documentSelector)
            : base(documentSelector)
        {
            ResolveProvider = resolveProvider;
        }

        /// <summary>
        /// Document links have a resolve provider as well.
        /// </summary>
        [JsonProperty]
        public bool ResolveProvider { get; set; }
    }
}
