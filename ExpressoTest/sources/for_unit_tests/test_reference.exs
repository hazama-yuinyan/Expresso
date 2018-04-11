module main;


def changeValue(val (- &int)
{
	val = 1000;
}

def main()
{
	let a = 100;
	println("Before: ${a}");
	changeValue(&a);
	println("After: ${a}");
}