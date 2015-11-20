using System;
using System.Collections.Generic;

using Expresso.Ast;
using Expresso.Runtime.Exceptions;
using System.Reflection;
using System.Linq;

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
Usage: expresso file_name"
                );
				return;
			}
			
			var file_name = args[0];

            if(file_name.EndsWith(".exe") || file_name.EndsWith(".dll")){
                var asm = Assembly.LoadFile(file_name);
                var mod = asm.GetModule("main");
                if(mod == null){
                    Console.Error.WriteLine("No main module found! Can't execute the file!");
                    return;
                }
                var entry_type = mod.GetType("ExsMain");
                if(entry_type == null){
                    Console.Error.WriteLine("No entry point!");
                    return;
                }

                var main_func = entry_type.GetMethod("main");
                if(main_func == null){
                    Console.Error.WriteLine("No entry function! Can't execute the file.");
                }
                try{
                    main_func.Invoke(null, args.Skip(1).ToArray());
                    return;
                }
                catch(Exception e){
                    Console.Error.WriteLine(e.Message);
                }
            }
			try{
                var parser = new Parser(new Scanner(file_name));
                parser.DoPostParseProcessing = true;
                parser.Parse();
			}
            catch(PanickedException panicked){
				Console.WriteLine(panicked.Message);
			}
			catch(Exception e){
				Console.WriteLine(e.Message);
			}
		}
	}
}
