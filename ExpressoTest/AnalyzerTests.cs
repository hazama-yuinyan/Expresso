using System;
using NUnit.Framework;

namespace Expresso.Test
{
    [TestFixture]
    public class AnalyzerTests
    {
        [Test]
        public void SimpleLiterals()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_literals.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;


        }
    }
}

