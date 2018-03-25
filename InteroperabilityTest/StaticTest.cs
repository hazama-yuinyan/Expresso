using System;
using Expresso.Runtime.Builtins;

namespace InteroperabilityTest
{
    public class StaticTest
    {
        public static void DoSomething()
        {
            Console.WriteLine("Hello from StaticTest.DoSomething");
        }

        public static bool GetSomeBool()
        {
            Console.WriteLine("GetSomeBool called");
            return true;
        }

        public static ExpressoIntegerSequence GetSomeIntSeq()
        {
            Console.WriteLine("GetSomeIntSeq called");
            return new ExpressoIntegerSequence(1, 10, 1, true);
        }
    }
}
