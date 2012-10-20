using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ntojs
{
	/// <summary>
	/// Basic array data structure used for Json.
	/// </summary>
	public class JsonArray : IEnumerable<object>
	{
		/// <summary>
		/// The list of [<see cref="JsonArray"/>, a <see cref="JsonObject"/>, a <see cref="String"/> or null] values.
		/// </summary>
		readonly List<object> _v = new List<object>();

		public JsonArray Add(object value)
		{
			_v.Add(value);
			return this;
		}

		#region IEnumerable<object>

		public IEnumerator<object> GetEnumerator()
		{
			return _v.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _v.GetEnumerator();
		}

		#endregion
	}
}
