using System;
using System.Collections.Generic;

using Expresso.Interpreter;
using Expresso.Runtime.Exceptions;
using Expresso.Runtime.Operations;

namespace Expresso.Terminal
{
	class ExpressoMain
	{
		public static void Main(string[] args)
		{
			if(args.Length == 0){
				Console.WriteLine(
@"Welcome to the Expresso Console!
Usage: expresso file_name");
				return;
			}
			
			var file_name = args[0];
			var ast = ExpressoOps.ParseAndBind(file_name);
			var interp = new Expresso.Interpreter.Interpreter();

			Expresso.Interpreter.Interpreter.MainModule = ast;
			interp.CurOpenedSourceFileName = file_name;
			try{
				interp.Run(new List<object>(args));
			}
			catch(RuntimeException eval_ex){
				Console.WriteLine(eval_ex.Message);
			}
			catch(Exception e){
				Console.WriteLine(e.Message);
			}
		}
	}
}
