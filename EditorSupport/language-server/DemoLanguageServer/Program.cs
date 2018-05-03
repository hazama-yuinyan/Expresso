#define WAIT_FOR_DEBUGGER

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Expresso.Ast.Analysis;
using JsonRpc.Standard.Client;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;
using LanguageServer.VsCode;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace ExpressoLanguageServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            var debug_mode = args.Any(a => a.Equals("--debug", StringComparison.OrdinalIgnoreCase));

            #if WAIT_FOR_DEBUGGER
            while (!Debugger.IsAttached) Thread.Sleep(1000);
            //Debugger.Break();
            #endif

            StreamWriter log_writer = null;
            if (debug_mode){
                log_writer = File.CreateText("messages-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log");
                log_writer.AutoFlush = true;
            }

            using(log_writer)
            using(var cin = Console.OpenStandardInput())
            using(var bcin = new BufferedStream(cin))
            using(var cout = Console.OpenStandardOutput())
            using(var reader = new PartwiseStreamMessageReader(bcin))
            using(var writer = new PartwiseStreamMessageWriter(cout)){
                var contract_resolver = new JsonRpcContractResolver{
                    NamingStrategy = new CamelCaseJsonRpcNamingStrategy(),
                    ParameterValueConverter = new CamelCaseJsonValueConverter(),
                };
                var client_handler = new StreamRpcClientHandler();
                var client = new JsonRpcClient(client_handler);

                if(debug_mode){
                    // We want to capture log all the LSP server-to-client calls as well
                    client_handler.MessageSending += (_, e) => {
                        lock (log_writer) log_writer.WriteLine("<C{0}", e.Message);
                    };
                    client_handler.MessageReceiving += (_, e) => {
                        lock (log_writer) log_writer.WriteLine(">C{0}", e.Message);
                    };
                }
                // Configure & build service host
                var session = new LanguageServerSession(client, contract_resolver);
                session.LogStream = log_writer;

                var host = BuildServiceHost(log_writer, contract_resolver, debug_mode);
                var server_handler = new StreamRpcServerHandler(host,
                    StreamRpcServerHandlerOptions.ConsistentResponseSequence |
                    StreamRpcServerHandlerOptions.SupportsRequestCancellation);
                server_handler.DefaultFeatures.Set(session);

                // If we want server to stop, just stop the "source"
                using(server_handler.Attach(reader, writer))
                using(client_handler.Attach(reader, writer)){
                    // Wait for the "stop" request.
                    session.CancellationToken.WaitHandle.WaitOne();
                }
                log_writer?.WriteLine("Exited");
            }
        }

        private static IJsonRpcServiceHost BuildServiceHost(TextWriter logWriter,
            IJsonRpcContractResolver contractResolver, bool debugMode)
        {
            var logger_factory = new LoggerFactory();
            logger_factory.AddProvider(new DebugLoggerProvider(null));
          
            var builder = new JsonRpcServiceHostBuilder{
                ContractResolver = contractResolver,
                LoggerFactory = logger_factory
            };
            builder.UseCancellationHandling();
            builder.Register(typeof(Program).GetTypeInfo().Assembly);

            if(debugMode){
                // Log all the client-to-server calls.
                builder.Intercept(async (context, next) => {
                    lock(logWriter)
                        logWriter.WriteLine("> {0}", context.Request);
                    
                    try{
                        await next();
                    }
                    catch(ParserException e){
                        Console.Error.WriteLine(e);
                    }
                    catch(Exception e){
                        Console.Error.WriteLine(e.Message);
                    }

                    lock(logWriter)
                        logWriter.WriteLine("< {0}", context.Response);
                });
            }

            return builder.Build();
        }

    }
}