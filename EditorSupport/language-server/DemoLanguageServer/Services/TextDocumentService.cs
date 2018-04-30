using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Expresso;
using Expresso.Resolver;
using ExpressoLanguageServer.Generators;
using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode;
using LanguageServer.VsCode.Contracts;

namespace ExpressoLanguageServer.Services
{
    [JsonRpcScope(MethodPrefix = "textDocument/")]
    public class TextDocumentService : ExpressoLanguageServiceBase
    {
        UTF8Encoding encoding = new UTF8Encoding();

        [JsonRpcMethod]
        public async Task<Hover> Hover(TextDocumentIdentifier textDocument, Position position, CancellationToken ct)
        {
            // Note that Hover is cancellable.
            await Task.Delay(1000, ct);
            var ast = Session.AstDictionary[textDocument.Uri];
            var file = Session.FileDictionary[textDocument.Uri];
            var ast_resolver = Session.AstResolver;
            return HoverGenerator.GenerateHover(ast, file, ast_resolver, position);
        }

        [JsonRpcMethod]
        public SignatureHelp SignatureHelp(TextDocumentIdentifier textDocument, Position position)
        {
            return new SignatureHelp(new List<SignatureInformation>
            {
                new SignatureInformation("**Function1**", "Documentation1"),
                new SignatureInformation("**Function2** <strong>test</strong>", "Documentation2"),
            });
        }

        [JsonRpcMethod(IsNotification = true)]
        public /*async Task*/ void DidOpen(TextDocumentItem textDocument)
        {
            var doc = new SessionDocument(textDocument);
            var parser = new Parser(new Scanner(new MemoryStream(encoding.GetBytes(textDocument.Text)))){
                DoPostParseProcessing = true
            };
            parser.Parse();
            var ast = parser.TopmostAst;
            Session.AstDictionary.Add(textDocument.Uri, ast);

            var file = ast.ToTypeSystem();
            Session.FileDictionary.Add(textDocument.Uri, file);

            if(Session.AstResolver == null)
                Session.AstResolver = new ExpressoAstResolver(new ExpressoResolver(file), ast, file);
            /*var session = Session;
            doc.DocumentChanged += async (sender, args) => {
                // Lint the document when it's changed.
                var doc1 = ((SessionDocument) sender).Document;
                var diag1 = session.DiagnosticProvider.LintDocument(doc1, session.Settings.MaxNumberOfProblems);
                if(session.Documents.ContainsKey(doc1.Uri)){
                    // In case the document has been closed when we were linting…
                    await session.Client.Document.PublishDiagnostics(doc1.Uri, diag1);
                }
            };
            Session.Documents.TryAdd(textDocument.Uri, doc);
            var diag = Session.DiagnosticProvider.LintDocument(doc.Document, Session.Settings.MaxNumberOfProblems);
            await Client.Document.PublishDiagnostics(textDocument.Uri, diag);*/
        }

        [JsonRpcMethod(IsNotification = true)]
        public void DidChange(TextDocumentIdentifier textDocument,
            ICollection<TextDocumentContentChangeEvent> contentChanges)
        {
            var session_doc = Session.Documents[textDocument.Uri];
            session_doc.NotifyChanges(contentChanges);
            var new_parser = new Parser(new Scanner(new MemoryStream(encoding.GetBytes(session_doc.Document.Content)))){
                DoPostParseProcessing = true
            };
            new_parser.Parse();
            var ast = new_parser.TopmostAst;
            Session.AstDictionary[textDocument.Uri] = ast;

            var file = ast.ToTypeSystem();
            Session.FileDictionary[textDocument.Uri] = file;
        }

        [JsonRpcMethod(IsNotification = true)]
        public void WillSave(TextDocumentIdentifier textDocument, TextDocumentSaveReason reason)
        {
            //Client.Window.LogMessage(MessageType.Log, "-----------");
            //Client.Window.LogMessage(MessageType.Log, Documents[textDocument].Content);
        }

        [JsonRpcMethod(IsNotification = true)]
        public async Task DidClose(TextDocumentIdentifier textDocument)
        {
            if(textDocument.Uri.IsUntitled())
                await Client.Document.PublishDiagnostics(textDocument.Uri, new Diagnostic[0]);

            Session.AstDictionary.Remove(textDocument.Uri);
            Session.FileDictionary.Remove(textDocument.Uri);
            Session.Documents.TryRemove(textDocument.Uri, out _);
        }

        private static readonly CompletionItem[] PredefinedCompletionItems =
        {
            new CompletionItem(".NET", CompletionItemKind.Keyword,
                "Keyword1",
                "Short for **.NET Framework**, a software framework by Microsoft (possibly its subsets) or later open source .NET Core.",
                null),
            new CompletionItem(".NET Standard", CompletionItemKind.Keyword,
                "Keyword2",
                "The .NET Standard is a formal specification of .NET APIs that are intended to be available on all .NET runtimes.",
                null),
            new CompletionItem(".NET Framework", CompletionItemKind.Keyword,
                "Keyword3",
                ".NET Framework (pronounced dot net) is a software framework developed by Microsoft that runs primarily on Microsoft Windows.", null),
        };

        [JsonRpcMethod]
        public CompletionList Completion(TextDocumentIdentifier textDocument, Position position)
        {
            return new CompletionList(PredefinedCompletionItems);
        }

    }
}
