using System;
using System.Reflection;
using System.Linq;
using Expresso.Ast.Analysis;
using Expresso.CodeGen;

namespace Expresso.Terminal
{
	class ExpressoMain
	{
		public static void Main(string[] args)
		{
			if(args.Length == 0){
				Console.WriteLine(
@"Welcome to the Expresso Console!
I can read both assembly files or source files.
Usage: mono exsc file_name -o target_path"
                );
				return;
			}
			
			var file_name = args[0];

            if(file_name.EndsWith(".exs", StringComparison.CurrentCulture)){
                var output_path = args[2];

                try{
                    var parser = new Parser(new Scanner(file_name));
                    parser.DoPostParseProcessing = true;
                    parser.Parse();

                    var ast = parser.TopmostAst;

                    var options = new ExpressoCompilerOptions{
                        OutputPath = output_path,
                        BuildType = BuildType.Debug | BuildType.Executable
                    };
                    var emitter = new CSharpEmitter(parser, options);
                    ast.AcceptWalker(emitter, null);
                }
                catch(ParserException e){
                    Console.WriteLine(e.Message);
                }
            }else{
                try{
                    var asm = Assembly.LoadFile(file_name);
                    var mod = asm.GetModule("main.exe");
                    if(mod == null){
                        Console.Error.WriteLine("No main module found! Can't execute the file!");
                        return;
                    }
                    var entry_type = mod.GetType("Main");
                    if(entry_type == null){
                        Console.Error.WriteLine("No entry point!");
                        return;
                    }

                    var main_func = entry_type.GetMethod("Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if(main_func == null){
                        Console.Error.WriteLine("No entry function! Can't execute the file.");
                        return;
                    }
                    main_func.Invoke(null, args.Skip(1).ToArray());
                }
                catch(Exception e){
                    Console.WriteLine(e.Message);
                }
            }
		}
	}
}
