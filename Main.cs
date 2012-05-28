using System;
using Expresso.Interpreter;

namespace ExpressoConsole
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
			var interp = new Interpreter(parser.root);
			interp.Initialize();
			interp.Run();
		}
	}
}
