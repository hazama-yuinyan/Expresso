using System;
using Expresso.Interpreter;

namespace Expresso.Terminal
{
	class ExpressoMain
	{
		public static void Main (string[] args)
		{
			if(args.Length == 0){
				Console.WriteLine("Welcome to the Expresso Console!");
				Console.WriteLine("Usage: Expresso file_name");
				return;
			}
			
			var file_name = args[0];
			var parser = new Parser(new Scanner(file_name));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			try{
				interp.Initialize();
				interp.Run();
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
