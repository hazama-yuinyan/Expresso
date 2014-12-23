using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Expresso.Ast;

namespace Expresso.Builtins.Library
{
	/// <summary>
	/// Expressoのスタンダードインプット。
	/// Represents the standard input in Expresso. Because it inherits from the IEnumerable interface,
	/// you can use it just like any other sequence types.
	/// </summary>
	public class StandardIn : IEnumerable<string>, IEnumerable
	{
		static TextReader _in = Console.In;
		static StandardIn inst = null;

        public static StandardIn Instance{
            get{
                if(inst == null)
                    inst = new StandardIn();

                return inst;
            }
        }

		StandardIn(){}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<string> GetEnumerator()
		{
			while(true){
				var line = _in.ReadLine();
				if(line == null)
					break;

				yield return line;
			}
		}
	}

	public interface IClosable
	{
		void Close();
	}
	
	/// <summary>
	/// Expressoのファイルオブジェクト。
	/// This class manages the file object used in Expresso.
	/// </summary>
	public class FileObject : IDisposable, IClosable, IEnumerable<string>, IEnumerable
	{
		StreamReader _reader = null;
		StreamWriter _writer = null;
		BinaryReader _breader = null;
		BinaryWriter _bwriter = null;
		bool binary_mode;
		
		public FileObject(string path, bool binaryMode, FileMode mode, FileAccess access, Encoding encode)
		{
			binary_mode = binaryMode;
			FileStream file_stream = (access.HasFlag(FileAccess.Write) && !File.Exists(path)) ? File.Create(path) :
																								File.Open(path, mode, access);
			if(access.HasFlag(FileAccess.Read)){
				if(binaryMode)
					_breader = new BinaryReader(file_stream);
				else
					_reader = new StreamReader(file_stream, encode);
			}

			if(access.HasFlag(FileAccess.Write)){
				if(binaryMode)
					_bwriter = new BinaryWriter(file_stream);
				else
					_writer = new StreamWriter(file_stream, encode);
			}
		}

		public int Read()
		{
			if(binary_mode)
				return _breader.Read();
			else
				return _reader.Read();
		}

		public string ReadLine()
		{
			if(binary_mode)
				throw new NotSupportedException("Can not read a line while in binary mode!");

			return _reader.ReadLine();
		}

		public string ReadAll()
		{
			if(binary_mode)
				throw new NotSupportedException("Can not read to the end of the stream while in binary mode!");

			return _reader.ReadToEnd();
		}

		public void Write(object val)
		{
			if(binary_mode){
				if(val is int)
					_bwriter.Write((int)val);
				else if(val is double)
					_bwriter.Write((double)val);
				else if(val is string)
					_bwriter.Write((string)val);
			}else{
				if(val is int)
					_writer.Write((int)val);
				else if(val is double)
					_writer.Write((double)val);
				else if(val is string)
					_writer.Write((string)val);
			}
		}
		
		public static FileObject Open(string path, FileMode mode, FileAccess access)
		{
			return new FileObject(path, false, mode, access, Encoding.UTF8);
		}

		public void Close()
		{
			Dispose();
		}
		
		public void Dispose()
		{
			if(binary_mode){
				if(_breader != null)
					_breader.Dispose();

				if(_bwriter != null)
					_bwriter.Dispose();
			}else{
				if(_reader != null)
					_reader.Dispose();

				if(_writer != null)
					_writer.Dispose();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<string> GetEnumerator()
		{
			if(_reader == null)
				throw new InvalidOperationException("Can not read lines from a null stream.");

			while(true){
				var line = ReadLine();
				if(line == null)
					break;

				yield return line;
			}
		}

		public static void ParseOption(string option, out FileMode mode, out FileAccess access, out bool binaryMode)
		{
			mode = FileMode.Open;
			access = FileAccess.Read;
			binaryMode = false;

			var options = Regex.Matches(option, @"[rwbta]");
			for(int i = 0; i < options.Count; ++i){
				switch(options[i].Value){
				case "r":
					access = FileAccess.Read;
					break;

				case "w":
					access = FileAccess.Write;
					mode = FileMode.Truncate;
					break;

				case "a":
					mode = FileMode.Append;
					break;

				case "b":
					binaryMode = true;
					break;

				case "t":
					binaryMode = false;
					break;
				}
			}
		}

		public static FileObject OpenFile(string path, string option, string encoding)
		{
			FileMode mode;
			FileAccess access;
			bool binary_mode;
			FileObject.ParseOption(option, out mode, out access, out binary_mode);

			Encoding actual_encoding = Encoding.GetEncoding(encoding);
			return new FileObject(path, binary_mode, mode, access, actual_encoding);
		}
	}
}

