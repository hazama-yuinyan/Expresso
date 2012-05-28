using System;
using System.IO;
using NUnit.Framework;
using Expresso.Ast;
using Expresso.Interpreter;

namespace Expresso.Test
{
	[TestFixture]
	public class ParserTests
	{
		private Parser parser;
		
		public ParserTests ()
		{
		}
		
		static string[] sources = {
			"def main(){" +
			"	let $x (- int;" +
			"	$x = 1 + 4;" +
			"	$x = 1 - 4;" +
			"	$x = 1 * 4;" +
			"	$x = 4 / 2;" +
			"	$x = 4 % 2;" +
			"	$x = 4 ** 2;" +
			"}",
			"def main(){" +
			"	let $x = 1;" +
			"	let $"
		};
		
		[Test]
		public void SimpleExpressions(string source, Node[] expects)
		{
			var parser = new Parser(new Scanner(new StringReader(source)));
			
			
		}
		
		[Test]
		public void SimpleStatements(string source, Node[] expects)
		{
			
		}
	}
}

