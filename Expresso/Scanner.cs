
using System;
using System.IO;
using System.Collections;

namespace Expresso {

public class Token {
	public int kind;    // token kind
	public int pos;     // token position in bytes in the source text (starting at 0)
	public int charPos;  // token position in characters in the source text (starting at 0)
	public int col;     // token column (starting at 1)
	public int line;    // token line (starting at 1)
	public string val;  // token value
	public Token next;  // ML 2005-03-11 Tokens are kept in linked list
}

//-----------------------------------------------------------------------------------
// Buffer
//-----------------------------------------------------------------------------------
public class Buffer {
	// This Buffer supports the following cases:
	// 1) seekable stream (file)
	//    a) whole stream in buffer
	//    b) part of stream in buffer
	// 2) non seekable stream (network, console)

	public const int EOF = char.MaxValue + 1;
	const int MIN_BUFFER_LENGTH = 1024; // 1KB
	const int MAX_BUFFER_LENGTH = MIN_BUFFER_LENGTH * 64; // 64KB
	byte[] buf;         // input buffer
	int bufStart;       // position of first byte in buffer relative to input stream
	int bufLen;         // length of buffer
	int fileLen;        // length of input stream (may change if the stream is no file)
	int bufPos;         // current position in buffer
	Stream stream;      // input stream (seekable)
	bool isUserStream;  // was the stream opened by the user?
	
	public Buffer (Stream s, bool isUserStream) {
		stream = s; this.isUserStream = isUserStream;
		
		if (stream.CanSeek) {
			fileLen = (int) stream.Length;
			bufLen = Math.Min(fileLen, MAX_BUFFER_LENGTH);
			bufStart = Int32.MaxValue; // nothing in the buffer so far
		} else {
			fileLen = bufLen = bufStart = 0;
		}

		buf = new byte[(bufLen>0) ? bufLen : MIN_BUFFER_LENGTH];
		if (fileLen > 0) Pos = 0; // setup buffer to position 0 (start)
		else bufPos = 0; // index 0 is already after the file, thus Pos = 0 is invalid
		if (bufLen == fileLen && stream.CanSeek) Close();
	}
	
	protected Buffer(Buffer b) { // called in UTF8Buffer constructor
		buf = b.buf;
		bufStart = b.bufStart;
		bufLen = b.bufLen;
		fileLen = b.fileLen;
		bufPos = b.bufPos;
		stream = b.stream;
		// keep destructor from closing the stream
		b.stream = null;
		isUserStream = b.isUserStream;
	}

	~Buffer() { Close(); }
	
	protected void Close() {
		if (!isUserStream && stream != null) {
			stream.Close();
			stream = null;
		}
	}
	
	public virtual int Read () {
		if (bufPos < bufLen) {
			return buf[bufPos++];
		} else if (Pos < fileLen) {
			Pos = Pos; // shift buffer start to Pos
			return buf[bufPos++];
		} else if (stream != null && !stream.CanSeek && ReadNextStreamChunk() > 0) {
			return buf[bufPos++];
		} else {
			return EOF;
		}
	}

	public int Peek () {
		int curPos = Pos;
		int ch = Read();
		Pos = curPos;
		return ch;
	}
	
	// beg .. begin, zero-based, inclusive, in byte
	// end .. end, zero-based, exclusive, in byte
	public string GetString (int beg, int end) {
		int len = 0;
		char[] buf = new char[end - beg];
		int oldPos = Pos;
		Pos = beg;
		while (Pos < end) buf[len++] = (char) Read();
		Pos = oldPos;
		return new String(buf, 0, len);
	}

	public int Pos {
		get { return bufPos + bufStart; }
		set {
			if (value >= fileLen && stream != null && !stream.CanSeek) {
				// Wanted position is after buffer and the stream
				// is not seek-able e.g. network or console,
				// thus we have to read the stream manually till
				// the wanted position is in sight.
				while (value >= fileLen && ReadNextStreamChunk() > 0);
			}

			if (value < 0 || value > fileLen) {
				throw new FatalError("buffer out of bounds access, position: " + value);
			}

			if (value >= bufStart && value < bufStart + bufLen) { // already in buffer
				bufPos = value - bufStart;
			} else if (stream != null) { // must be swapped in
				stream.Seek(value, SeekOrigin.Begin);
				bufLen = stream.Read(buf, 0, buf.Length);
				bufStart = value; bufPos = 0;
			} else {
				// set the position to the end of the file, Pos will return fileLen.
				bufPos = fileLen - bufStart;
			}
		}
	}
	
	// Read the next chunk of bytes from the stream, increases the buffer
	// if needed and updates the fields fileLen and bufLen.
	// Returns the number of bytes read.
	private int ReadNextStreamChunk() {
		int free = buf.Length - bufLen;
		if (free == 0) {
			// in the case of a growing input stream
			// we can neither seek in the stream, nor can we
			// foresee the maximum length, thus we must adapt
			// the buffer size on demand.
			byte[] newBuf = new byte[bufLen * 2];
			Array.Copy(buf, newBuf, bufLen);
			buf = newBuf;
			free = bufLen;
		}
		int read = stream.Read(buf, bufLen, free);
		if (read > 0) {
			fileLen = bufLen = (bufLen + read);
			return read;
		}
		// end of stream reached
		return 0;
	}
}

//-----------------------------------------------------------------------------------
// UTF8Buffer
//-----------------------------------------------------------------------------------
public class UTF8Buffer: Buffer {
	public UTF8Buffer(Buffer b): base(b) {}

	public override int Read() {
		int ch;
		do {
			ch = base.Read();
			// until we find a utf8 start (0xxxxxxx or 11xxxxxx)
		} while ((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
		if (ch < 128 || ch == EOF) {
			// nothing to do, first 127 chars are the same in ascii and utf8
			// 0xxxxxxx or end of file character
		} else if ((ch & 0xF0) == 0xF0) {
			// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x07; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F; ch = base.Read();
			int c4 = ch & 0x3F;
			ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
		} else if ((ch & 0xE0) == 0xE0) {
			// 1110xxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x0F; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F;
			ch = (((c1 << 6) | c2) << 6) | c3;
		} else if ((ch & 0xC0) == 0xC0) {
			// 110xxxxx 10xxxxxx
			int c1 = ch & 0x1F; ch = base.Read();
			int c2 = ch & 0x3F;
			ch = (c1 << 6) | c2;
		}
		return ch;
	}
}

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
public class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int maxT = 116;
	const int noSym = 116;


	public Buffer buffer; // scanner buffer
	
	string directoryName;  // directory name that we've opened
	Token t;          // current token
	int ch;           // current input character
	int pos;          // byte position of current character
	int charPos;      // position by unicode characters starting with 0
	int col;          // column number of current character
	int line;         // line number of current character
	int oldEols;      // EOLs that appeared in a comment;
	static readonly Hashtable start; // maps first token character to start state

	Token tokens;     // list of tokens already peeked (first token is a dummy)
	Token pt;         // current peek token
	
	char[] tval = new char[128]; // text of current token
	int tlen;         // length of current token

	/// <summary>
	/// The file path that we've opened
	/// </summary>
	public string FilePath{
		get; private set;
	}
	
	static Scanner() {
		start = new Hashtable(128);
		for (int i = 65; i <= 90; ++i) start[i] = 10;
		for (int i = 95; i <= 95; ++i) start[i] = 10;
		for (int i = 97; i <= 113; ++i) start[i] = 10;
		for (int i = 115; i <= 122; ++i) start[i] = 10;
		for (int i = 49; i <= 57; ++i) start[i] = 53;
		for (int i = 46; i <= 46; ++i) start[i] = 54;
		for (int i = 92; i <= 92; ++i) start[i] = 23;
		for (int i = 114; i <= 114; ++i) start[i] = 55;
		start[58] = 56; 
		start[59] = 3; 
		start[123] = 4; 
		start[40] = 87; 
		start[60] = 88; 
		start[91] = 5; 
		start[41] = 6; 
		start[125] = 7; 
		start[62] = 89; 
		start[93] = 8; 
		start[44] = 9; 
		start[48] = 57; 
		start[39] = 31; 
		start[34] = 41; 
		start[35] = 63; 
		start[45] = 90; 
		start[61] = 91; 
		start[38] = 92; 
		start[124] = 93; 
		start[43] = 94; 
		start[42] = 95; 
		start[47] = 96; 
		start[37] = 97; 
		start[64] = 78; 
		start[63] = 79; 
		start[33] = 98; 
		start[94] = 86; 
		start[Buffer.EOF] = -1;

	}
	
	public Scanner (string fileName) {
		this.directoryName = Path.GetDirectoryName(fileName);
		FilePath = fileName;
		try {
			Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			buffer = new UTF8Buffer(new Buffer(stream, false));
			Init();
		} catch (IOException) {
			throw new FatalError("Cannot open file " + fileName);
		}
	}
	
	public Scanner (Stream s) {
		buffer = new Buffer(s, true);
		Init();
	}
	
	void Init() {
		pos = -1; line = 1; col = 0; charPos = -1;
		oldEols = 0;
		NextCh();
		/*if (ch == 0xEF) { // check optional byte order mark for UTF-8
			NextCh(); int ch1 = ch;
			NextCh(); int ch2 = ch;
			if (ch1 != 0xBB || ch2 != 0xBF) {
				throw new FatalError(String.Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
			}
			buffer = new UTF8Buffer(buffer); col = 0; charPos = -1;
			NextCh();
		}*/
		pt = tokens = new Token();  // first token is a dummy
	}
	
	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			// buffer reads unicode chars, if UTF8 has been detected
			ch = buffer.Read(); col++; charPos++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; col = 0; }
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		if (ch != Buffer.EOF) {
			tval[tlen++] = (char) ch;
			NextCh();
		}
	}



	bool Comment0() {
		int level = 1, pos0 = pos, line0 = line, col0 = col, charPos0 = charPos;
		NextCh();
		if (ch == '/') {
			NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0; charPos = charPos0;
		}
		return false;
	}

	bool Comment1() {
		int level = 1, pos0 = pos, line0 = line, col0 = col, charPos0 = charPos;
		NextCh();
		if (ch == '*') {
			NextCh();
			for(;;) {
				if (ch == '*') {
					NextCh();
					if (ch == '/') {
						level--;
						if (level == 0) { oldEols = line - line0; NextCh(); return true; }
						NextCh();
					}
				} else if (ch == '/') {
					NextCh();
					if (ch == '*') {
						level++; NextCh();
					}
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0; charPos = charPos0;
		}
		return false;
	}


	void CheckLiteral() {
		switch (t.val) {
			case "for": t.kind = 24; break;
			case "export": t.kind = 25; break;
			case "assembly": t.kind = 26; break;
			case "module": t.kind = 27; break;
			case "type": t.kind = 28; break;
			case "field": t.kind = 29; break;
			case "method": t.kind = 30; break;
			case "param": t.kind = 31; break;
			case "return": t.kind = 32; break;
			case "import": t.kind = 34; break;
			case "from": t.kind = 35; break;
			case "as": t.kind = 36; break;
			case "interface": t.kind = 37; break;
			case "def": t.kind = 38; break;
			case "class": t.kind = 40; break;
			case "public": t.kind = 41; break;
			case "protected": t.kind = 42; break;
			case "private": t.kind = 43; break;
			case "static": t.kind = 44; break;
			case "mutating": t.kind = 45; break;
			case "override": t.kind = 46; break;
			case "let": t.kind = 49; break;
			case "var": t.kind = 50; break;
			case "int": t.kind = 52; break;
			case "uint": t.kind = 53; break;
			case "bool": t.kind = 54; break;
			case "float": t.kind = 55; break;
			case "double": t.kind = 56; break;
			case "bigint": t.kind = 57; break;
			case "string": t.kind = 58; break;
			case "byte": t.kind = 59; break;
			case "char": t.kind = 60; break;
			case "vector": t.kind = 61; break;
			case "dictionary": t.kind = 62; break;
			case "slice": t.kind = 63; break;
			case "intseq": t.kind = 64; break;
			case "void": t.kind = 65; break;
			case "break": t.kind = 67; break;
			case "upto": t.kind = 68; break;
			case "continue": t.kind = 69; break;
			case "yield": t.kind = 70; break;
			case "throw": t.kind = 71; break;
			case "if": t.kind = 82; break;
			case "else": t.kind = 83; break;
			case "while": t.kind = 84; break;
			case "do": t.kind = 85; break;
			case "in": t.kind = 86; break;
			case "match": t.kind = 87; break;
			case "try": t.kind = 89; break;
			case "catch": t.kind = 90; break;
			case "finally": t.kind = 91; break;
			case "_": t.kind = 92; break;
			case "true": t.kind = 111; break;
			case "false": t.kind = 112; break;
			case "null": t.kind = 113; break;
			case "self": t.kind = 114; break;
			case "super": t.kind = 115; break;
			default: break;
		}
	}

	Token NextToken() {
		while (ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13
		) NextCh();
		if (ch == '/' && Comment0() ||ch == '/' && Comment1()) return NextToken();
		int apx = 0;
		int recKind = noSym;
		int recEnd = pos;
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; t.charPos = charPos;
		int state;
		if (start.ContainsKey(ch)) { state = (int) start[ch]; }
		else { state = 0; }
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: {
				if (recKind != noSym) {
					tlen = recEnd - t.pos;
					SetScannerBehindT();
				}
				t.kind = recKind; break;
			} // NextCh already done
			case 1:
				{t.kind = 2; break;}
			case 2:
				{t.kind = 4; break;}
			case 3:
				{t.kind = 5; break;}
			case 4:
				{t.kind = 6; break;}
			case 5:
				{t.kind = 9; break;}
			case 6:
				{t.kind = 10; break;}
			case 7:
				{t.kind = 11; break;}
			case 8:
				{t.kind = 13; break;}
			case 9:
				{t.kind = 14; break;}
			case 10:
				recEnd = pos; recKind = 16;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 10;}
				else {t.kind = 16; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 11:
				{t.kind = 17; break;}
			case 12:
				{
					tlen -= apx;
					SetScannerBehindT();
					t.kind = 17; break;}
			case 13:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 14;}
				else {goto case 0;}
			case 14:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else {goto case 0;}
			case 15:
				recEnd = pos; recKind = 18;
				if (ch >= '0' && ch <= '9' || ch == '_') {AddCh(); goto case 15;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 20;}
				else {t.kind = 18; break;}
			case 16:
				recEnd = pos; recKind = 18;
				if (ch >= '0' && ch <= '9' || ch == '_') {AddCh(); goto case 16;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 20;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 17;}
				else {t.kind = 18; break;}
			case 17:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 19;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 18;}
				else {goto case 0;}
			case 18:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 19;}
				else {goto case 0;}
			case 19:
				recEnd = pos; recKind = 18;
				if (ch >= '0' && ch <= '9' || ch == '_') {AddCh(); goto case 19;}
				else if (ch == 'F' || ch == 'f') {AddCh(); goto case 20;}
				else {t.kind = 18; break;}
			case 20:
				{t.kind = 18; break;}
			case 21:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 22;}
				else {goto case 0;}
			case 22:
				recEnd = pos; recKind = 19;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 22;}
				else {t.kind = 19; break;}
			case 23:
				if (ch == 'U' || ch == 'u') {AddCh(); goto case 24;}
				else {goto case 0;}
			case 24:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 25;}
				else {goto case 0;}
			case 25:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 26;}
				else {goto case 0;}
			case 26:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 27;}
				else {goto case 0;}
			case 27:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 28;}
				else {goto case 0;}
			case 28:
				recEnd = pos; recKind = 20;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 29;}
				else {t.kind = 20; break;}
			case 29:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 30;}
				else {goto case 0;}
			case 30:
				{t.kind = 20; break;}
			case 31:
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 32;}
				else if (ch == 92) {AddCh(); goto case 33;}
				else if (ch == 39) {AddCh(); goto case 40;}
				else {goto case 0;}
			case 32:
				if (ch == 39) {AddCh(); goto case 40;}
				else {goto case 0;}
			case 33:
				if (ch == 92 || ch >= 'a' && ch <= 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't') {AddCh(); goto case 32;}
				else if (ch == 'U' || ch == 'u') {AddCh(); goto case 34;}
				else {goto case 0;}
			case 34:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 35;}
				else {goto case 0;}
			case 35:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 36;}
				else {goto case 0;}
			case 36:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 37;}
				else {goto case 0;}
			case 37:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 38;}
				else {goto case 0;}
			case 38:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 39;}
				else if (ch == 39) {AddCh(); goto case 40;}
				else {goto case 0;}
			case 39:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 32;}
				else {goto case 0;}
			case 40:
				{t.kind = 21; break;}
			case 41:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 41;}
				else if (ch == 92) {AddCh(); goto case 42;}
				else if (ch == '"') {AddCh(); goto case 48;}
				else {goto case 0;}
			case 42:
				if (ch == 92 || ch >= 'a' && ch <= 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't') {AddCh(); goto case 41;}
				else if (ch == 'U' || ch == 'u') {AddCh(); goto case 43;}
				else {goto case 0;}
			case 43:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 44;}
				else {goto case 0;}
			case 44:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 45;}
				else {goto case 0;}
			case 45:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 46;}
				else {goto case 0;}
			case 46:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 47;}
				else {goto case 0;}
			case 47:
				if (ch <= '!' || ch >= '#' && ch <= '/' || ch >= ':' && ch <= '@' || ch >= 'G' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535) {AddCh(); goto case 41;}
				else if (ch == 92) {AddCh(); goto case 42;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 58;}
				else if (ch == '"') {AddCh(); goto case 48;}
				else {goto case 0;}
			case 48:
				{t.kind = 22; break;}
			case 49:
				if (ch <= '!' || ch >= '#' && ch <= 65535) {AddCh(); goto case 49;}
				else if (ch == '"') {AddCh(); goto case 52;}
				else {goto case 0;}
			case 50:
				if (ch == '"') {AddCh(); goto case 51;}
				else if (ch == '#') {AddCh(); goto case 50;}
				else {goto case 0;}
			case 51:
				if (ch <= '!' || ch >= '#' && ch <= 65535) {AddCh(); goto case 51;}
				else if (ch == '"') {AddCh(); goto case 59;}
				else {goto case 0;}
			case 52:
				{t.kind = 23; break;}
			case 53:
				recEnd = pos; recKind = 17;
				if (ch >= '0' && ch <= '9' || ch == '_') {AddCh(); goto case 53;}
				else if (ch == '.') {apx++; AddCh(); goto case 60;}
				else if (ch == 'L' || ch == 'U' || ch == 'l' || ch == 'u') {AddCh(); goto case 11;}
				else {t.kind = 17; break;}
			case 54:
				recEnd = pos; recKind = 15;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 16;}
				else if (ch == '.') {AddCh(); goto case 61;}
				else {t.kind = 15; break;}
			case 55:
				recEnd = pos; recKind = 16;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 10;}
				else if (ch == '"') {AddCh(); goto case 49;}
				else if (ch == '#') {AddCh(); goto case 50;}
				else {t.kind = 16; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 56:
				recEnd = pos; recKind = 3;
				if (ch == ':') {AddCh(); goto case 2;}
				else {t.kind = 3; break;}
			case 57:
				recEnd = pos; recKind = 17;
				if (ch >= '0' && ch <= '9' || ch == '_') {AddCh(); goto case 53;}
				else if (ch == '.') {apx++; AddCh(); goto case 60;}
				else if (ch == 'L' || ch == 'U' || ch == 'l' || ch == 'u') {AddCh(); goto case 11;}
				else if (ch == 'X' || ch == 'x') {AddCh(); goto case 21;}
				else {t.kind = 17; break;}
			case 58:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 41;}
				else if (ch == 92) {AddCh(); goto case 42;}
				else if (ch == '"') {AddCh(); goto case 48;}
				else {goto case 0;}
			case 59:
				if (ch <= '!' || ch >= '$' && ch <= 65535) {AddCh(); goto case 51;}
				else if (ch == '"') {AddCh(); goto case 59;}
				else if (ch == '#') {AddCh(); goto case 62;}
				else {goto case 0;}
			case 60:
				recEnd = pos; recKind = 18;
				if (ch >= '0' && ch <= '9') {apx = 0; AddCh(); goto case 16;}
				else if (ch == '.') {apx++; AddCh(); goto case 12;}
				else if (ch == 'F' || ch == 'f') {apx = 0; AddCh(); goto case 20;}
				else if (ch == 'E' || ch == 'e') {apx = 0; AddCh(); goto case 13;}
				else {t.kind = 18; break;}
			case 61:
				recEnd = pos; recKind = 1;
				if (ch == '.') {AddCh(); goto case 1;}
				else {t.kind = 1; break;}
			case 62:
				recEnd = pos; recKind = 23;
				if (ch <= '!' || ch >= '$' && ch <= 65535) {AddCh(); goto case 51;}
				else if (ch == '"') {AddCh(); goto case 59;}
				else if (ch == '#') {AddCh(); goto case 62;}
				else {t.kind = 23; break;}
			case 63:
				if (ch == '[') {AddCh(); goto case 64;}
				else {goto case 0;}
			case 64:
				{t.kind = 33; break;}
			case 65:
				{t.kind = 39; break;}
			case 66:
				{t.kind = 47; break;}
			case 67:
				{t.kind = 72; break;}
			case 68:
				{t.kind = 73; break;}
			case 69:
				{t.kind = 74; break;}
			case 70:
				{t.kind = 75; break;}
			case 71:
				{t.kind = 76; break;}
			case 72:
				{t.kind = 77; break;}
			case 73:
				{t.kind = 78; break;}
			case 74:
				{t.kind = 79; break;}
			case 75:
				{t.kind = 80; break;}
			case 76:
				{t.kind = 81; break;}
			case 77:
				{t.kind = 88; break;}
			case 78:
				{t.kind = 93; break;}
			case 79:
				{t.kind = 94; break;}
			case 80:
				{t.kind = 95; break;}
			case 81:
				{t.kind = 96; break;}
			case 82:
				{t.kind = 97; break;}
			case 83:
				{t.kind = 98; break;}
			case 84:
				{t.kind = 99; break;}
			case 85:
				{t.kind = 100; break;}
			case 86:
				{t.kind = 101; break;}
			case 87:
				recEnd = pos; recKind = 7;
				if (ch == '-') {AddCh(); goto case 66;}
				else {t.kind = 7; break;}
			case 88:
				recEnd = pos; recKind = 8;
				if (ch == '<') {AddCh(); goto case 99;}
				else if (ch == '=') {AddCh(); goto case 84;}
				else {t.kind = 8; break;}
			case 89:
				recEnd = pos; recKind = 12;
				if (ch == '>') {AddCh(); goto case 100;}
				else if (ch == '=') {AddCh(); goto case 85;}
				else {t.kind = 12; break;}
			case 90:
				recEnd = pos; recKind = 105;
				if (ch == '>') {AddCh(); goto case 65;}
				else if (ch == '=') {AddCh(); goto case 68;}
				else {t.kind = 105; break;}
			case 91:
				recEnd = pos; recKind = 48;
				if (ch == '>') {AddCh(); goto case 77;}
				else if (ch == '=') {AddCh(); goto case 82;}
				else {t.kind = 48; break;}
			case 92:
				recEnd = pos; recKind = 51;
				if (ch == '=') {AddCh(); goto case 73;}
				else if (ch == '&') {AddCh(); goto case 81;}
				else {t.kind = 51; break;}
			case 93:
				recEnd = pos; recKind = 66;
				if (ch == '=') {AddCh(); goto case 74;}
				else if (ch == '|') {AddCh(); goto case 80;}
				else {t.kind = 66; break;}
			case 94:
				recEnd = pos; recKind = 104;
				if (ch == '=') {AddCh(); goto case 67;}
				else {t.kind = 104; break;}
			case 95:
				recEnd = pos; recKind = 106;
				if (ch == '=') {AddCh(); goto case 69;}
				else if (ch == '*') {AddCh(); goto case 101;}
				else {t.kind = 106; break;}
			case 96:
				recEnd = pos; recKind = 107;
				if (ch == '=') {AddCh(); goto case 70;}
				else {t.kind = 107; break;}
			case 97:
				recEnd = pos; recKind = 108;
				if (ch == '=') {AddCh(); goto case 72;}
				else {t.kind = 108; break;}
			case 98:
				recEnd = pos; recKind = 110;
				if (ch == '=') {AddCh(); goto case 83;}
				else {t.kind = 110; break;}
			case 99:
				recEnd = pos; recKind = 102;
				if (ch == '=') {AddCh(); goto case 75;}
				else {t.kind = 102; break;}
			case 100:
				recEnd = pos; recKind = 103;
				if (ch == '=') {AddCh(); goto case 76;}
				else {t.kind = 103; break;}
			case 101:
				recEnd = pos; recKind = 109;
				if (ch == '=') {AddCh(); goto case 71;}
				else {t.kind = 109; break;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
	
	private void SetScannerBehindT() {
		buffer.Pos = t.pos;
		NextCh();
		line = t.line; col = t.col; charPos = t.charPos;
		for (int i = 0; i < tlen; i++) NextCh();
	}
	
	// get the next token (possibly a token already seen during peeking)
	public Token Scan () {
		if (tokens.next == null) {
			return NextToken();
		} else {
			pt = tokens = tokens.next;
			return tokens;
		}
	}

	// peek for the next token, ignore pragmas
	public Token Peek () {
		do {
			if (pt.next == null) {
				pt.next = NextToken();
			}
			pt = pt.next;
		} while (pt.kind > maxT); // skip pragmas
	
		return pt;
	}

	// make sure that peeking starts at the current scan position
	public void ResetPeek () { pt = tokens; }

	/// <summary>
	/// Opens a file relative to the source file that the scanner is currently opening
	/// and creates a new scanner on it.
	/// </summary>
	public Scanner OpenChildFile(string childFileName)
	{
		var combined = Path.GetFullPath(Path.Combine(directoryName, childFileName));
		var child_scanner = new Scanner(combined);
		return child_scanner;
	}

	/// <summary>
	/// Checks whether a file relative to the source file that the scanner is currently opening
	/// exists.
	/// </summary>
	public bool ChildFileExists(string childFileName)
	{
		var combined = Path.GetFullPath(Path.Combine(directoryName, childFileName));
		return File.Exists(combined);
	}

	/// <summary>
	/// Gets a string representing a file path relative to the source file 
	/// that the scanner is currently opening.
	/// </summary>
	public string GetFullPath(string childFileName)
	{
		var combined = Path.GetFullPath(Path.Combine(directoryName, childFileName));
		return combined;
	}

} // end Scanner
}