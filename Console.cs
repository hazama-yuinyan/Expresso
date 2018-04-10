using System;
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
I can read Expresso source files and compile them into assembly files that mono can execute.
But to run the executable, currently you have to first copy the runtime assemblies(Expresso.dll and ExpressoRuntime.dll). 
Usage: mono exsc.exe file_name -o target_path -e executable_name
To execute the resulting binary: mono the_name_of_the_executable"
                );
				return;
			}
			
			var file_name = args[0];
            var output_path = args[2];
            var executable_name = args[4];

            try{
                var parser = new Parser(new Scanner(file_name));
                parser.DoPostParseProcessing = true;
                parser.Parse();

                var ast = parser.TopmostAst;

                var options = new ExpressoCompilerOptions{
                    OutputPath = output_path,
                    BuildType = BuildType.Debug | BuildType.Executable,
                    ExecutableName = executable_name
                };
                var emitter = new CSharpEmitter(parser, options);
                ast.AcceptWalker(emitter, null);
            }
            catch(ParserException e){
                Console.Error.WriteLine(e);
            }
            catch(Exception e){
                Console.Error.WriteLine(e.Message);
            }
		}
	}
}
