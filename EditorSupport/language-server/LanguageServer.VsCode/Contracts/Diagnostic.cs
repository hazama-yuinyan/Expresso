using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LanguageServer.VsCode.Contracts
{
    /// <summary>
    /// A value assigned to a <see cref="Diagnostic" /> determining its severity.
    /// </summary>
    public enum DiagnosticSeverity
    {
        /// <summary>
        /// Reports an error.
        /// </summary>
        Error = 1,

        /// <summary>
        /// Reports a warning.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Reports an information.
        /// </summary>
        Information = 3,

        /// <summary>
        /// Reports a hint.
        /// </summary>
        Hint = 4
    }

    /// <summary>
    /// Represents a diagnostic, such as a compiler error or warning.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Diagnostic
    {

        /// <summary>
        /// Represents an empty array of <see cref="Diagnostic"/>
        /// </summary>
        public static readonly Diagnostic[] EmptyDiagnostics = { };

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.Diagnostic"/> class.
        /// </summary>
        [JsonConstructor]
        public Diagnostic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.Diagnostic"/> class.
        /// </summary>
        /// <param name="severity">Severity.</param>
        /// <param name="range">Range.</param>
        /// <param name="source">Source.</param>
        /// <param name="message">Message.</param>
        public Diagnostic(DiagnosticSeverity severity, Range range, string source, string message) : this(severity, range, source, null, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.Diagnostic"/> class.
        /// </summary>
        /// <param name="severity">Severity.</param>
        /// <param name="range">Range.</param>
        /// <param name="source">Source.</param>
        /// <param name="code">Code.</param>
        /// <param name="message">Message.</param>
        public Diagnostic(DiagnosticSeverity severity, Range range, string source, string code, string message)
        {
            Severity = severity;
            Range = range;
            Source = source;
            Code = code;
            Message = message;
        }

        /// <summary>
        /// The diagnostic's code.
        /// </summary>
        [JsonProperty]
        public string Code { get; set; }

        /// <summary>
        /// The diagnostic's message.
        /// </summary>
        [JsonProperty]
        public string Message { get; set; }

        /// <summary>
        /// The range to which the message applies.
        /// </summary>
        [JsonProperty]
        public Range Range { get; set; }

        /// <summary>
        /// The diagnostic's severity.
        /// </summary>
        [JsonProperty]
        public DiagnosticSeverity Severity { get; set; }

        /// <summary>
        /// A human-readable string describing the source of this diagnostic, e.g. 'typescript' or 'super lint'.
        /// </summary>
        [JsonProperty]
        public string Source { get; set; }

        /// <summary>
        /// Signature for further properties.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{Severity}, {Code}[{Range}]{Message}";
    }
}
