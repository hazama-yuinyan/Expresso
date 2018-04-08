namespace InteroperabilityTest
{
    public class EnumTest
    {
        public TestEnum TestProperty{
            get; set;
        }

        public static bool TestEnumeration(TestEnum target)
        {
            return target == TestEnum.SomeField;
        }

        public bool TestEnumerationOnInstance()
        {
            return TestProperty == TestEnum.YetAnotherField;
        }
    }
}
