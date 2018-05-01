using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Expresso.Ast;
using Expresso.Resolver;
using Expresso.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;
using JsonRpc.DynamicProxy.Client;
using JsonRpc.Standard.Client;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using LanguageServer.VsCode.Contracts;
using LanguageServer.VsCode.Contracts.Client;
using LanguageServer.VsCode.Server;

namespace ExpressoLanguageServer
{
    public class LanguageServerSession
    {
        public static readonly Lazy<IList<IUnresolvedAssembly>> BuiltinLibs = new Lazy<IList<IUnresolvedAssembly>>(() => {
            var assemblies = new []{
                typeof(object).Assembly,                    // mscorlib
                typeof(Uri).Assembly,                       // System.dll
                typeof(Enumerable).Assembly,                // System.Core.dll
                typeof(IProjectContent).Assembly
            };

            var project_contents = new IUnresolvedAssembly[assemblies.Length];
            var total = Stopwatch.StartNew();
            Parallel.For(0, assemblies.Length, (int i) => {
                var w = Stopwatch.StartNew();
                var loader = new CecilLoader();
                project_contents[i] = loader.LoadAssemblyFile(assemblies[i].Location);
                Debug.WriteLine(Path.GetFileName(assemblies[i].Location) + ": " + w.Elapsed);
            });

            Debug.WriteLine("Total: " + total.Elapsed);
            return project_contents;
        });
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public LanguageServerSession(JsonRpcClient rpcClient, IJsonRpcContractResolver contractResolver)
        {
            RpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
            var builder = new JsonRpcProxyBuilder {ContractResolver = contractResolver};
            Client = new ClientProxy(builder, rpcClient);
            Documents = new ConcurrentDictionary<Uri, SessionDocument>();
            DiagnosticProvider = new DiagnosticProvider();
        }

        public CancellationToken CancellationToken => cts.Token;

        public JsonRpcClient RpcClient { get; }

        public ClientProxy Client { get; }

        public ConcurrentDictionary<Uri, SessionDocument> Documents { get; }

        public DiagnosticProvider DiagnosticProvider { get; }

        public LanguageServerSettings Settings { get; set; } = new LanguageServerSettings();

        public Dictionary<Uri, ExpressoAst> AstDictionary{
            get;
        } = new Dictionary<Uri, ExpressoAst>();
        public Dictionary<Uri, ExpressoUnresolvedFile> FileDictionary{
            get;
        } = new Dictionary<Uri, ExpressoUnresolvedFile>();

        public void StopServer()
        {
            cts.Cancel();
        }

    }

    public class SessionDocument
    {
        /// <summary>
        /// Actually makes the changes to the inner document per this milliseconds.
        /// </summary>
        private const int RenderChangesDelay = 100;

        public SessionDocument(TextDocumentItem doc)
        {
            Document = TextDocument.Load<FullTextDocument>(doc);
        }

        private Task updateChangesDelayTask;

        private readonly object syncLock = new object();

        private List<TextDocumentContentChangeEvent> impendingChanges = new List<TextDocumentContentChangeEvent>();

        public event EventHandler DocumentChanged;

        public TextDocument Document { get; set; }

        public void NotifyChanges(IEnumerable<TextDocumentContentChangeEvent> changes)
        {
            lock(syncLock){
                if (impendingChanges == null)
                    impendingChanges = changes.ToList();
                else
                    impendingChanges.AddRange(changes);
            }
            if(updateChangesDelayTask == null || updateChangesDelayTask.IsCompleted){
                updateChangesDelayTask = Task.Delay(RenderChangesDelay);
                updateChangesDelayTask.ContinueWith(t => Task.Run((Action)MakeChanges));
            }
        }

        private void MakeChanges()
        {
            List<TextDocumentContentChangeEvent> localChanges;
            lock (syncLock){
                localChanges = impendingChanges;
                if(localChanges == null || localChanges.Count == 0)
                    return;
                
                impendingChanges = null;
            }

            Document = Document.ApplyChanges(localChanges);
            if(impendingChanges == null){
                localChanges.Clear();
                lock (syncLock){
                    if(impendingChanges == null)
                        impendingChanges = localChanges;
                }
            }

            OnDocumentChanged();
        }

        protected virtual void OnDocumentChanged()
        {
            DocumentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}