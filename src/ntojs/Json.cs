using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ArrayTypeBase = ntojs.JsonArray;
using ArrayTypeDefault = ntojs.JsonArray;
using ObjectTypeBase = ntojs.JsonObject;
using ObjectTypeDefault = ntojs.JsonObject;

namespace ntojs
{
	/// <summary>
	/// Helper for working with JSON data.
	/// </summary>
	public static class Json
	{
		#region Read

		/// <summary>
		/// Reads the given JSON data into a <see cref="JsonArray"/>, a <see cref="JsonObject"/>, a <see cref="String"/> or null value.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static object Read(string json)
		{
			return Read<ArrayTypeDefault, ObjectTypeDefault>(json);
		}
		/// <summary>
		/// Reads the given JSON data into a <see cref="JsonArray"/>, a <see cref="JsonObject"/>, a <see cref="String"/> or null value.
		/// </summary>
		/// <typeparam name="TArray"></typeparam>
		/// <typeparam name="TObject"></typeparam>
		/// <param name="json"></param>
		/// <returns></returns>
		public static object Read<TArray, TObject>(string json)
			where TArray : ArrayTypeBase, new()
			where TObject : ObjectTypeBase, new()
		{
			return new JsonReader<TArray, TObject>(json).Read();
		}

		#endregion

		#region Write

		public static string Write<T>(T root)
			where T : class
		{
			return Write(typeof(T), root);
		}

		public static string Write(Type type, object root)
		{
			return "null";
		}

		#endregion

		#region DebugDump

		/// <summary>
		/// Reads the given JSON data and dumps the resulting graph to the debugger.
		/// </summary>
		/// <param name="json"></param>
		public static void DebugDump(string json)
		{
			if (String.IsNullOrEmpty(json))
			{
				Debug.Print("DebugDumping JSON: [null or empty]{0}DebugDump OK{0}", Environment.NewLine);
				return;
			}
			else if (json.Length > 80)
				Debug.Print("DebugDumping JSON: {1} ...{0}", Environment.NewLine, json.Substring(0, 80).Replace('\t', ' ').Replace("\r", "").Replace('\n', ' '));
			else
				Debug.Print("DebugDumping JSON: {1}{0}", Environment.NewLine, json.Replace('\t', ' ').Replace("\r", "").Replace('\n', ' '));

			var val = Json.Read(json);
			DebugDump(0, val, "");

			Debug.Print("{0}DebugDump OK{0}", Environment.NewLine);
		}

		static void DebugDump(int indent, object val, string key)
		{
			var indentStr = new String('\t', indent);

			if (val == null)
			{
				Debug.Print(indentStr + "null");
				return;
			}
			var str = val as string;
			if (str != null)
			{
				if (String.IsNullOrEmpty(key))
					Debug.Print(indentStr + val.ToString());
				else
					Debug.Print(indentStr + "{0}: {1}", key, val);
			}
			else
			{
				var arr = val as ArrayTypeBase;
				if (arr != null)
				{
					if (!String.IsNullOrEmpty(key))
						Debug.Print(indentStr + "{0}: [", key);
					else
						Debug.Print(indentStr + "[");

					foreach (var item in arr)
						DebugDump(indent + 1, item, "");

					Debug.Print(indentStr + "]");
				}
				else
				{
					var obj = val as ObjectTypeBase;
					if (obj != null)
					{
						if (!String.IsNullOrEmpty(key))
							Debug.Print(indentStr + "{0}: {{", key);
						else
							Debug.Print(indentStr + "{");

						foreach (var key2 in obj.Keys)
							DebugDump(indent, obj[key2], key2);

						Debug.Print(indentStr + "}");
					}
					else
					{
						Debug.Print(indentStr + "ERROR: Invalid type {0}.", val.GetType().FullName);
					}
				}
			}
		}

		#endregion
	}
}
