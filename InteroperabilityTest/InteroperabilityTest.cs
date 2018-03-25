using System;
using System.Collections.Generic;

namespace InteroperabilityTest
{
    public class InteroperabilityTest : TestInterface
    {
        public void DoSomething()
        {
            Console.WriteLine("Hello from 'DoSomething'");
        }

        public List<int> GetIntList()
        {
            Console.WriteLine("GetIntList called");
            return new List<int>{1, 2, 3, 4, 5};
        }

        public int GetSomeInt()
        {
            Console.WriteLine("GetSomeInt called");
            return 100;
        }
    }
}
