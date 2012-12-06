/***
 * Test program for the standard library of Expresso
 */


def main(){
 	try{
 		let file = File.openFile("./test.txt", "w");
 		file.write("This is a test blah blah...");
 	}
 	finally{
 		file.close();
 	}
}