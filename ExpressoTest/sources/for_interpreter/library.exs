/***
 * Test program for the standard library of Expresso
 */


def main(){
 	try{
 		let file = File.openFile("./test.txt", "r");
 		let content = file.readAll();
 		//file.write("This is a test blah blah...");
 		print content;
 	}
 	finally{
 		file.close();
 	}
}