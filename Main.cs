using System;
using System.Collections.Generic;
using Expresso.Interpreter;

namespace Expresso.Terminal
{
	class ExpressoMain
	{
		public static void Main(string[] args)
		{
			if(args.Length == 0){
				Console.WriteLine("Welcome to the Expresso Console!");
				Console.WriteLine("Usage: Expresso file_name");
				return;
			}
			
			var file_name = args[0];
			var parser = new Parser(new Scanner(file_name));
			parser.ParsingFileName = file_name;
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter();
			Expresso.Interpreter.Interpreter.MainModule = parser.ParsingModule;
			interp.CurOpenedSourceFileName = file_name;
			try{
				interp.Initialize();
				interp.Run(new List<object>(args));
			}
			catch(EvalException eval_ex){
				Console.WriteLine(eval_ex.Message);
			}
			catch(Exception e){
				Console.WriteLine(e.Message);
			}
		}
	}
}
