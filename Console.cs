using System;
using System.Collections.Generic;

using Expresso.Ast;
using Expresso.Runtime.Exceptions;

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

			try{
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
