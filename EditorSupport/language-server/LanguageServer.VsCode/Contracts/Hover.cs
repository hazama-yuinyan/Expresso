
using Newtonsoft.Json;

namespace LanguageServer.VsCode.Contracts
{
    /// <summary>
    /// Represents the result of a hover request - a formatted tooltip.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Hover
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.Hover"/> class.
        /// </summary>
        [JsonConstructor]
        public Hover()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.Hover"/> class.
        /// </summary>
        /// <param name="contents">Contents.</param>
        public Hover(string contents) : this(contents, new Range())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.Hover"/> class.
        /// </summary>
        /// <param name="contents">Contents.</param>
        /// <param name="range">Range.</param>
        public Hover(string contents, Range range)
        {
            Contents = contents;
            Range = range;
        }

        /// <summary>
        /// A Markdown string to display in the Hover.
        /// </summary>
        [JsonProperty]
        public string Contents { get; set; }

        /// <summary>
        /// The range to which this Hover applies.
        /// </summary>
        [JsonProperty]
        public Range Range { get; set; }
    }
}
