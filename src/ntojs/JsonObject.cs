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
	/// Basic object data structure used for Json.
	/// </summary>
	public class JsonObject : IEnumerable<object>
	{
		/// <summary>
		/// The dictionary of [<see cref="JsonArray"/>, a <see cref="JsonObject"/>, a <see cref="String"/> or null] values.
		/// </summary>
		readonly Dictionary<string, object> _v = new Dictionary<string, object>();

		public JsonObject Add(string key, object value)
		{
			_v.Add(key, value);
			return this;
		}

		public ICollection<string> Keys { get { return _v.Keys; } }

		public ICollection<object> Values { get { return _v.Values; } }

		public object this[string key]
		{
			get { return _v[key]; }
			set { _v[key] = value; }
		}

		#region Get

		public T Get<T>(string key, T defaultValue)
			where T : class
		{
			object val;
			if (!_v.TryGetValue(key, out val))
			{
				return defaultValue;
			}
			var tval = val as T;
			if (tval != null)
				return defaultValue;
			else
				return tval;
		}

		public JsonObject GetGraph(string key)
		{
			return Get<JsonObject>(key, null);
		}

		public JsonObject GetGraph(string key, JsonObject defaultValue)
		{
			return Get<JsonObject>(key, defaultValue);
		}

		public JsonArray GetList(string key)
		{
			return Get<JsonArray>(key, null);
		}

		public JsonArray GetList(string key, JsonArray defaultValue)
		{
			return Get<JsonArray>(key, defaultValue);
		}

		public string GetString(string key)
		{
			return Get<string>(key, null);
		}

		public string GetString(string key, string defaultValue)
		{
			return Get<string>(key, defaultValue);
		}

		#endregion

		#region TryGet

		public bool TryGet<T>(string key, out T value)
			where T : class
		{
			object val;
			if (!_v.TryGetValue(key, out val))
			{
				value = null;
				return false;
			}
			value = val as T;
			return value != null;
		}

		public bool TryGetString(string key, out string value)
		{
			return TryGet<string>(key, out value);
		}

		public bool TryGetList(string key, out JsonArray value)
		{
			return TryGet<JsonArray>(key, out value);
		}

		public bool TryGetGraph(string key, out JsonObject value)
		{
			return TryGet<JsonObject>(key, out value);
		}

		public bool TryGetValue(string key, out object value)
		{
			return _v.TryGetValue(key, out value);
		}

		#endregion

		#region IEnumerable<object>

		public IEnumerator<object> GetEnumerator()
		{
			return _v.Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _v.Values.GetEnumerator();
		}

		#endregion
	}
}
