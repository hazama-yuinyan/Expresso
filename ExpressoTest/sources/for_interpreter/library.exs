/***
 * Test program for the standard library of Expresso
 */
module main;


def main()
{
 	let file = File.openFile("./test.txt", "r");
 	let content = file.readAll();
 	//file.write("This is a test blah blah...");
 	print(content);
}