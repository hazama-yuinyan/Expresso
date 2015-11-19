﻿using System;
using NUnit.Framework;

namespace Expresso.Test
{
    [TestFixture]
    public class ErroneouesTests
    {
        [Test]
        public void Literals()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/literals.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(6, parser.errors.count);
        }
    }
}

