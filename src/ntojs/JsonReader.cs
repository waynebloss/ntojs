using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrayTypeBase = ntojs.JsonArray;
using ArrayTypeDefault = ntojs.JsonArray;
using ObjectTypeBase = ntojs.JsonObject;
using ObjectTypeDefault = ntojs.JsonObject;

namespace ntojs
{
	/// <summary>
	/// Parses JSON strings into a <see cref="TArray"/>, <see cref="TObject"/>,
	/// <see cref="string"/> or null.
	/// </summary>
	/// <remarks>
	/// JSON Arrays are parsed into the type specified by <see cref="TArray"/>.
	/// JSON Objects are parsed into the type specified by <see cref="TObject"/>.
	/// 
	/// JSON Numbers are basically validated to be well formed numbers and nothing
	/// more.
	/// 
	/// JSON Booleans are returned as the string "True" or "False" with an upper-
	/// case first letter, which matches the .Net convention.
	/// 
	/// JSON Number and Boolean value types remain as strings after parsing.
	/// These are not converted to a strongly typed bool or int/double/long variable
	/// here in order to keep things simple and to avoid early boxing.
	/// </remarks>
	/// <seealso cref="http://www.json.org/"/>
	public class JsonReader<TArray, TObject>
		where TArray : ArrayTypeBase, new()
		where TObject : ObjectTypeBase, new()
	{
		/// <summary>
		/// Current index.
		/// </summary>
		int _idx;
		/// <summary>
		/// JSON string.
		/// </summary>
		readonly string _json;

		#region Constructor

		public JsonReader(string json) : this(json, 0) { }

		public JsonReader(string json, int startIndex)
		{
			_errorIdx = NonErrorIndex;
			_idx = startIndex;
			_json = json;
			if (!String.IsNullOrEmpty(json))
				_sb = new StringBuilder();
		}

		#endregion

		#region Character Resources (CR)

		/// <summary>
		/// Character Resources
		/// </summary>
		static class CR
		{
			public const char Backslash = '\\';
			public const char Backspace = '\b';
			public const char BraceClose = '}';
			public const char BraceOpen = '{';
			public const char BracketClose = ']';
			public const char BracketOpen = '[';
			public const char Colon = ':';
			public const char Comma = ',';
			public const char Dot = '.';
			public const char Dquote = '"';
			public const char Feed = '\f';
			public const char i0 = '0';
			public const char i1 = '1';
			public const char i2 = '2';
			public const char i3 = '3';
			public const char i4 = '4';
			public const char i5 = '5';
			public const char i6 = '6';
			public const char i7 = '7';
			public const char i8 = '8';
			public const char i9 = '9';
			public const char Minus = '-';
			public const char Newline = '\n';
			public const char Nbsp = '\u00a0';
			public const char Plus = '+';
			public const char Return = '\r';
			public const char Slash = '/';
			public const char Space = ' ';
			public const char Tab = '\t';
		}

		#endregion

		#region TokenType

		enum TokenType
		{
			None,

			String,
			Number,
			True,
			False,
			Null,

			KeyDelim,
			ValueDelim,

			ArrayBegin,
			ArrayEnd,

			ObjectBegin,
			ObjectEnd,

			End,
			Unknown
		}

		#endregion

		#region Tokenize

		/// <summary>
		/// <see cref="TokenType"/> that was buffered by a call to <see cref="Peek"/>.
		/// </summary>
		TokenType _readAhead;

		/// <summary>
		/// Removes the read ahead buffer that was created by <see cref="Peek"/>.
		/// </summary>
		void Flush()
		{
			_readAhead = TokenType.None;
		}
		/// <summary>
		/// Buffers the next <see cref="TokenType"/> from the source and returns
		/// the buffered value until <see cref="Flush"/> or <see cref="ReadToken"/>
		/// are called.
		/// </summary>
		/// <returns></returns>
		TokenType Peek()
		{
			if (_readAhead != TokenType.None)
				return _readAhead;

			_readAhead = ReadTokenCore();

			return _readAhead;
		}
		/// <summary>
		/// Reads a <see cref="TokenType"/> from the source.
		/// </summary>
		/// <returns></returns>
		TokenType ReadToken()
		{
			if (_readAhead != TokenType.None)
			{
				var n = _readAhead;
				_readAhead = TokenType.None;
				return n;
			}
			return ReadTokenCore();
		}
		/// <summary>
		/// Returns a <see cref="TokenType"/> from the source.
		/// </summary>
		/// <returns></returns>
		TokenType ReadTokenCore()
		{
			_idx = SkipWhiteSpace(_json, _idx);

			if (_idx == _json.Length)
				return TokenType.End;

			char c = _json[_idx++];
			switch (c)
			{
			case CR.Dquote:
				return TokenType.String;

			case CR.Plus:
				return TokenType.Number;

			case CR.Comma:
				return TokenType.ValueDelim;

			case CR.Minus:
			case CR.Dot:
			case CR.i0:
			case CR.i1:
			case CR.i2:
			case CR.i3:
			case CR.i4:
			case CR.i5:
			case CR.i6:
			case CR.i7:
			case CR.i8:
			case CR.i9:
				return TokenType.Number;

			case CR.Colon:
				return TokenType.KeyDelim;

			case CR.BracketOpen:
				return TokenType.ArrayBegin;

			case CR.BracketClose:
				return TokenType.ArrayEnd;

			case 'f':
				if (QueueCount() >= 4 &&
					_json[_idx] == 'a' &&
					_json[_idx + 1] == 'l' &&
					_json[_idx + 2] == 's' &&
					_json[_idx + 3] == 'e')
				{
					_idx += 4;
					return TokenType.False;
				}
				break;

			case 'n':
				if (QueueCount() >= 3 &&
					_json[_idx] == 'u' &&
					_json[_idx + 1] == 'l' &&
					_json[_idx + 2] == 'l')
				{
					_idx += 3;
					return TokenType.Null;
				}
				break;

			case 't':
				if (QueueCount() >= 3 &&
					_json[_idx + 0] == 'r' &&
					_json[_idx + 1] == 'u' &&
					_json[_idx + 2] == 'e')
				{
					_idx += 3;
					return TokenType.True;
				}
				break;

			case CR.BraceOpen:
				return TokenType.ObjectBegin;

			case CR.BraceClose:
				return TokenType.ObjectEnd;
			}
			_idx--;
			return TokenType.Unknown;
		}
		/// <summary>
		/// Returns next non-whitespace character index from the given json beginning with the
		/// specified start position.
		/// </summary>
		/// <param name="json"></param>
		/// <param name="startIndex"></param>
		/// <returns></returns>
		/// <remarks>
		/// WhiteSpace characters considered are:
		///	<list type="table">
		///		<listheader>
		///			<term>Character Name</term>
		///			<description>Description</description>
		///		</listheader>
		///		<item>
		///			<term>Space</term>
		///			<description>ASCII: 0x20</description>
		///		</item>
		///		<item>
		///			<term>Tab</term>
		///			<description>ASCII: 0x09</description>
		///		</item>
		///		<item>
		///			<term>Line Feed</term>
		///			<description>ASCII: 0x0A</description>
		///		</item>
		///		<item>
		///			<term>Vertical Tab</term>
		///			<description>ASCII: 0x0B</description>
		///		</item>
		///		<item>
		///			<term>Form Feed</term>
		///			<description>ASCII: 0x0C</description>
		///		</item>
		///		<item>
		///			<term>Carriage Return</term>
		///			<description>ASCII: 0x0D </description>
		///		</item>
		///		<item>
		///			<term>Non-breaking Space</term>
		///			<description>Extended ASCII: 0x00A0 </description>
		///		</item>
		/// </list>
		/// </remarks>
		static int SkipWhiteSpace(string json, int startIndex)
		{
			while (startIndex < json.Length)
			{
				char c = json[startIndex];

				if (!(c == CR.Space || (c >= CR.Tab && c <= CR.Return) || c == CR.Nbsp))
					return startIndex;

				startIndex++;
			}
			return startIndex;
		}

		#endregion

		#region Parse/Read

		static readonly uint[] HexDigits = new[] { 10u, 11u, 12u, 13u, 14u, 15u };
		const int UnicodeEscapeLen = 4;
		/// <summary>
		/// Buffer to hold string parts as they are read.
		/// </summary>
		readonly StringBuilder _sb;

		int QueueCount()
		{
			return _json.Length - _idx;
		}

		public object Read()
		{
			if (String.IsNullOrEmpty(_json))
				return null;

			switch (Peek())
			{
			case TokenType.None:
			case TokenType.End:
			case TokenType.Unknown:
				return null;
			}

			return ReadValue();
		}

		TArray ReadArray()
		{
			var array = new TArray();

			while (true)
			{
				switch (Peek())
				{
				case TokenType.ValueDelim:
					Flush();
					break;

				case TokenType.ArrayEnd:
					Flush();
					return array;

				default:
					/// 
					/// WARNING: Do not call <see cref="Flush"/> (here or above)
					/// before <see cref="ReadValue"/>! It needs the token in the
					/// read ahead buffer!
					/// 
					array.Add(ReadValue());
					break;
				}
			}
		}

		string ReadNumber()
		{
			/// We will return the substring starting from the previous number character (token).
			/// 
			/// The previous character does not have to be checked again for number-ness here
			/// because it has already been done once by the tokenizer (<see cref="ReadTokenCore"/>).
			/// 
			var start = _idx - 1;
			do
			{
				var c = _json[_idx];

				if ((c >= CR.i0 && c <= CR.i9) || c == CR.Dot || c == CR.Minus || c == CR.Plus || c == 'e' || c == 'E')
				{
					if (++_idx == _json.Length)
					{
						OnError("Unexpected EOF.");
						break;
					}
					continue;
				}
				break;
			} while (true);

			return _json.Substring(start, _idx - start);
		}

		TObject ReadObject()
		{
			var newObj = new TObject();

			while (true)
			{
				var t = Peek();
				Flush();

				switch (t)
				{
				case TokenType.ValueDelim:
					break;

				case TokenType.ObjectEnd:
					return newObj;

				case TokenType.String:
					string key = ReadString();

					if (ReadToken() != TokenType.KeyDelim)
					{
						OnError("Unexpected token.");	// (expected colon.)
						return newObj;
					}

					object value = ReadValue();

					newObj.Add(key, value);

					break;

				default:
					OnError("Unexpected token.");
					return newObj;
				}
			}
		}

		string ReadString()
		{
			_sb.Length = 0;
			int startIdx = -1;

			while (_idx < _json.Length)
			{
				var c = _json[_idx++];
				/// 
				/// Scan for Dquote and Backslash.
				/// 
				if (c == CR.Dquote)
					return ReadStringEnd(startIdx);

				if (c != CR.Backslash)
				{
					/// Not a Dquote, not a Backslash.
					/// Start copying from here.
					if (startIdx == -1)
						startIdx = _idx - 1;

					continue;
				}
				/// Found a Backslash.
				/// 
				/// Append string that has been found so far to the string buffer.
				/// 
				if (startIdx != -1)
				{
					_sb.Append(_json, startIdx, _idx - startIdx - 1);
					startIdx = -1;
				}
				/// Check for more characters.
				if (_idx == _json.Length)
					break;
				/// 
				/// Read the Backslash escaped char and handle it.
				/// 
				c = _json[_idx++];
				switch (c)
				{
				case CR.Dquote:
				case CR.Slash:
				case CR.Backslash:
					_sb.Append(c);
					break;

				case 'b':
					_sb.Append(CR.Backspace);
					break;

				case 'f':
					_sb.Append(CR.Feed);
					break;

				case 'n':
					_sb.Append(CR.Newline);
					break;

				case 'r':
					_sb.Append(CR.Return);
					break;

				case 't':
					_sb.Append(CR.Tab);
					break;

				case 'u':
					if (QueueCount() < UnicodeEscapeLen)
						break;

					var c2 = UnicodePointToChar(_json, _idx, UnicodeEscapeLen);
					_sb.Append(c2);
					_idx += UnicodeEscapeLen;

					break;
				}
			}
			OnError("Unexpected EOF.");
			return ReadStringEnd(startIdx);
		}

		string ReadStringEnd(int startIdx)
		{
			if (startIdx != -1)
			{
				if (_sb.Length == 0)
					return _json.Substring(startIdx, _idx - startIdx - 1);

				_sb.Append(_json, startIdx, _idx - startIdx - 1);
			}
			return _sb.ToString();
		}

		object ReadValue()
		{
			var t = Peek();

			Flush();

			switch (t)
			{
			case TokenType.String:
				return ReadString();

			case TokenType.Number:
				return ReadNumber();

			case TokenType.True:
				return "True";

			case TokenType.False:
				return "False";

			case TokenType.Null:
				return null;

			case TokenType.ArrayBegin:
				return ReadArray();

			case TokenType.ObjectBegin:
				return ReadObject();
			}
			OnError("Unexpected token.");
			return null;
		}

		static char UnicodePointToChar(string source, int startIndex, int length)
		{
			uint n = 0;

			int end = startIndex + length;
			for (int i = startIndex; i < end; i++)
			{
				char c = source[i];

				if (c >= '0' && c <= '9')
					n = n * 16 + (uint)(c - '0');
				else if (c >= 'a' && c <= 'f')
					n = n * 16 + HexDigits[(c - 'a')];
				else if (c >= 'A' && c <= 'F')
					n = n * 16 + HexDigits[(c - 'A')];
			}
			return (char)n;
		}

		#endregion

		#region Error Handling

		const int NonErrorIndex = -1;

		int _errorIdx;

		/// <summary>
		/// Gets the index into the original JSON data string at which point an error occurred.
		/// </summary>
		public int ErrorIndex { get { return _errorIdx; } }
		/// <summary>
		/// Gets a value indicating if an error occurred.
		/// </summary>
		public bool HasError { get { return _json == null || _errorIdx > NonErrorIndex; } }
		/// <summary>
		/// Gets a value indicating if an unexpected End Of File (EOF) was reached.
		/// <para>There are only two types of errors while reading JSON - EOF and Unexpected token.</para>
		/// </summary>
		public bool IsErrorEOF { get { return _json == null || _errorIdx == _json.Length; } }

		void OnError(string description)
		{
			_errorIdx = _idx;
			System.Diagnostics.Debug.Print("JsonReader Error at index {0}: {1}", _errorIdx, description);
		}

		#endregion
	}
}
