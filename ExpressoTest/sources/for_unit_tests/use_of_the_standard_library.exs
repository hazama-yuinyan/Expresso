module main;


import System.IO.{File, FileStream} as {File, FileStream};
import System.Text.UTF8Encoding as UTF8Encoding;


def main()
{
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