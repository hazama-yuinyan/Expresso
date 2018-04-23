using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LanguageServer.VsCode.Contracts
{

    /// <summary>
    /// The kind of a completion entry.
    /// </summary>
    public enum CompletionItemKind
    {
        /// <summary>
        /// Represents a plain text.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Represents a method.
        /// </summary>
        Method = 2,

        /// <summary>
        /// Represents a function.
        /// </summary>
        Function = 3,

        /// <summary>
        /// Represents a constructor.
        /// </summary>
        Constructor = 4,

        /// <summary>
        /// Represents a field.
        /// </summary>
        Field = 5,

        /// <summary>
        /// Represents a variable.
        /// </summary>
        Variable = 6,

        /// <summary>
        /// Represents a class.
        /// </summary>
        Class = 7,

        /// <summary>
        /// Represents an interface.
        /// </summary>
        Interface = 8,

        /// <summary>
        /// Represents a module.
        /// </summary>
        Module = 9,

        /// <summary>
        /// Represents a property.
        /// </summary>
        Property = 10,

        /// <summary>
        /// Represents a unit.
        /// </summary>
        Unit = 11,

        /// <summary>
        /// Represents a value.
        /// </summary>
        Value = 12,

        /// <summary>
        /// Represents an enum.
        /// </summary>
        Enum = 13,

        /// <summary>
        /// Represents a keyword.
        /// </summary>
        Keyword = 14,

        /// <summary>
        /// Represents a snippet.
        /// </summary>
        Snippet = 15,

        /// <summary>
        /// Rerepsents a color.
        /// </summary>
        Color = 16,

        /// <summary>
        /// Rerepsents a file.
        /// </summary>
        File = 17,

        /// <summary>
        /// Represents a reference.
        /// </summary>
        Reference = 18,
    }

    /// <summary>
    /// Defines whether the insert text in a completion item should be interpreted as
    /// plain text or a snippet.
    /// </summary>
    public enum InsertTextFormat
    {
        /// <summary>
        /// The primary text to be inserted is treated as a plain string.
        /// </summary>
        PlainText = 1,
        /// <summary>
        ///  The primary text to be inserted is treated as a snippet.
        /// </summary>
        /// <remarks>
        /// <para>A snippet can define tab stops and placeholders with <c>$1</c>, <c>$2</c>
        /// and <c>${3:foo}</c>. <c>$0</c> defines the final tab stop, it defaults to
        /// the end of the snippet.Placeholders with equal identifiers are linked,
        /// that is typing in one will update others too.</para>
        /// <para>
        /// See also: https://github.com/Microsoft/vscode/blob/master/src/vs/editor/contrib/snippet/common/snippet.md
        /// </para>
        /// </remarks>
        Snippet = 2,
    }

    /// <summary>
    /// Represents a collection of <see cref="CompletionItem"/> to be presented in the editor。
    /// </summary>
    public class CompletionList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.CompletionList"/> class.
        /// </summary>
        [JsonConstructor]
        public CompletionList()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.CompletionList"/> class.
        /// </summary>
        /// <param name="items">Items.</param>
        public CompletionList(IEnumerable<CompletionItem> items) : this(items, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.CompletionList"/> class.
        /// </summary>
        /// <param name="items">Items.</param>
        /// <param name="isIncomplete">If set to <c>true</c> is incomplete.</param>
        public CompletionList(IEnumerable<CompletionItem> items, bool isIncomplete)
        {
            IsIncomplete = isIncomplete;
            Items = items;
        }

        /// <summary>
        /// This list it not complete. Further typing should result in recomputing this list.
        /// </summary>
        [JsonProperty]
        public bool IsIncomplete { get; set; }

        /// <summary>
        /// The completion items.
        /// </summary>
        [JsonProperty]
        public IEnumerable<CompletionItem> Items { get; set; }

    }

    /// <summary>
    /// A completion item.
    /// </summary>
    public class CompletionItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.CompletionItem"/> class.
        /// </summary>
        [JsonConstructor]
        public CompletionItem()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.CompletionItem"/> class.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="kind">Kind.</param>
        /// <param name="data">Data.</param>
        public CompletionItem(string label, CompletionItemKind kind, JToken data) : this(label, kind, null, null, data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.CompletionItem"/> class.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="kind">Kind.</param>
        /// <param name="detail">Detail.</param>
        /// <param name="data">Data.</param>
        public CompletionItem(string label, CompletionItemKind kind, string detail, JToken data) : this(label, kind, detail, null, data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LanguageServer.VsCode.Contracts.CompletionItem"/> class.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="kind">Kind.</param>
        /// <param name="detail">Detail.</param>
        /// <param name="documentation">Documentation.</param>
        /// <param name="data">Data.</param>
        public CompletionItem(string label, CompletionItemKind kind, string detail, string documentation, JToken data)
        {
            Label = label;
            Kind = kind;
            Detail = detail;
            Documentation = documentation;
            Data = data;
        }

        /// <summary>
        /// The label of this completion item. By default
        /// also the text that is inserted when selecting
        /// this completion.
        /// </summary>
        [JsonProperty]
        public string Label { get; set; }

        /// <summary>
        /// The kind of this completion item. Based of the kind
        /// an icon is chosen by the editor.
        /// </summary>
        [JsonProperty]
        public CompletionItemKind Kind { get; set; } = CompletionItemKind.Text;

        /// <summary>
        /// A human-readable (short) string with additional information
        /// about this item, like type or symbol information.
        /// </summary>
        [JsonProperty]
        public string Detail { get; set; }

        /// <summary>
        /// A human-readable string that represents a doc-comment.
        /// </summary>
        [JsonProperty]
        public string Documentation { get; set; }

        /// <summary>
        /// A string that shoud be used when comparing this item
        /// with other items. When `falsy` the label is used.
        /// </summary>
        [JsonProperty]
        public string SortText { get; set; }

        /// <summary>
        /// A string that should be used when filtering a set of
        /// completion items. When `falsy` the label is used.
        /// </summary>
        [JsonProperty]
        public string FilterText { get; set; }

        /// <summary>
        /// A string that should be inserted a document when selecting
        /// this completion. When `falsy` the label is used.
        /// </summary>
        [JsonProperty]
        public string InsertText { get; set; }

        /// <summary>
        /// The format of the insert text. The format applies to both the `insertText` property
        /// and the `newText` property of a provided `textEdit`.
        /// </summary>
        [JsonProperty]
        public InsertTextFormat InsertTextFormat { get; set; } = InsertTextFormat.PlainText;

        /// <summary>
        /// An edit which is applied to a document when selecting this completion. When an edit is provided the value of
        /// `insertText` is ignored.
        ///
        /// *Note:* The range of the edit must be a single line range and it must contain the position at which completion
        /// has been requested.
        /// </summary>
        [JsonProperty]
        public TextEdit TextEdit { get; set; }

        /// <summary>
        /// An optional array of additional text edits that are applied when
        /// selecting this completion. Edits must not overlap with the main edit
        /// nor with themselves.
        /// </summary>
        [JsonProperty]
        public IEnumerable<TextEdit> AdditionalTextEdits { get; set; }

        /// <summary>
        /// An optional set of characters that when pressed while this completion is active will accept it first and
        /// then type that character.
        /// </summary>
        [JsonProperty]
        public IEnumerable<char> CommitCharacters { get; set; }

        /// <summary>
        /// An optional command that is executed *after* inserting this completion. *Note* that
        /// additional modifications to the current document should be described with the
        /// <see cref="AdditionalTextEdits"/> property.
        /// </summary>
        [JsonProperty]
        public EditorCommand Command { get; set; }

        /// <summary>
        /// An data entry field that is preserved on a completion item between
        /// a completion and a completion resolve request.
        /// </summary>
        [JsonProperty]
        public JToken Data { get; set; }
    }

    /// <summary>
    /// Descibe options to be used when registered for code completion events.
    /// </summary>
    public class CompletionRegistrationOptions : TextDocumentRegistrationOptions
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.CompletionRegistrationOptions"/> class.
        /// </summary>
        [JsonConstructor]
        public CompletionRegistrationOptions()
            : this(false, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.CompletionRegistrationOptions"/> class.
        /// </summary>
        /// <param name="resolveProvider">If set to <c>true</c> resolve provider.</param>
        public CompletionRegistrationOptions(bool resolveProvider)
            : this(resolveProvider, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.CompletionRegistrationOptions"/> class.
        /// </summary>
        /// <param name="resolveProvider">If set to <c>true</c> resolve provider.</param>
        /// <param name="triggerCharacters">Trigger characters.</param>
        public CompletionRegistrationOptions(bool resolveProvider, IEnumerable<char> triggerCharacters)
            : this(resolveProvider, triggerCharacters, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LanguageServer.VsCode.Contracts.CompletionRegistrationOptions"/> class.
        /// </summary>
        /// <param name="resolveProvider">If set to <c>true</c> resolve provider.</param>
        /// <param name="triggerCharacters">Trigger characters.</param>
        /// <param name="documentSelector">Document selector.</param>
        public CompletionRegistrationOptions(bool resolveProvider, IEnumerable<char> triggerCharacters, IEnumerable<DocumentFilter> documentSelector)
            : base(documentSelector)
        {
            ResolveProvider = resolveProvider;
            TriggerCharacters = triggerCharacters;
        }

        /// <summary>
        /// The server provides support to resolve additional
        /// information for a completion item. (i.e. supports <c>completionItem/resolve</c>.)
        /// </summary>
        [JsonProperty]
        public bool ResolveProvider { get; set; }

        /// <summary>
        /// The characters that trigger completion automatically.
        /// </summary>
        [JsonProperty]
        public IEnumerable<char> TriggerCharacters { get; set; }
    }

}
