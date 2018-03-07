module main;


import "System.IO.File" as File;
import "System.IO.FileStream" as FileStream;
import "System.Text.UTF8Encoding" as UTF8Encoding;


def main()
{
	//let writer (- FileStream;
    var writer (- FileStream;
    try{
		writer = File.openWrite("./some_text.txt");
		let bytes = UTF8Encoding{encoderShouldEmitUTF8Identifier: true}.getBytes("This is to test writing a file");
		writer.write(bytes, 0, bytes.Length);
	}
	finally{
	    if writer != null {
			writer.dispose();
        }
	}
}