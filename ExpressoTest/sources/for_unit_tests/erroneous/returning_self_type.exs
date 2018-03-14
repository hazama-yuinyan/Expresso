/**
 * Test program for reutrning the self class type.
 */

module main;

class TestClass
{
	def returnSelf() -> TestClass
	{
	    return self;
	}
}
