module main;


import System.IO.{File, FileStream} as {File, FileStream};
import System.Text.UTF8Encoding as UTF8Encoding;


def main()
{
    var writer (- FileStream;
    try{
		writer = File.OpenWrite("./some_text.txt");
		let bytes = UTF8Encoding{encoderShouldEmitUTF8Identifier: true}.GetBytes("This is to test writing a file");
		writer.Write(bytes, 0, bytes.Length);
	}
	finally{
	    if writer != null {
			writer.Dispose();
        }
	}
}