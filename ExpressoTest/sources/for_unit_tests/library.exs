/***
 * Test program for the standard library of Expresso
 */
module main;


import file;

def main()
{
 	let file = file.openFile("./test.txt", "r");
 	let content = file.readAll();
 	//file.write("This is a test blah blah...");
 	print(content);
}