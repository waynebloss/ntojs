using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;

namespace ntojs
{
	static class TestQuery
	{
		/// <summary>
		/// Transforms SQL Query Results into Javascript.
		/// </summary>
		/// <param name="args"></param>
		/// <remarks>
		/// ## Overview
		///	- Args: SQL Server Connection Details.
		/// - StdIn: TSQL Queries.
		/// - StdOut: Javascript Object/Array Literals.
		/// </remarks>
		public static void Go(string[] args)
		{
			var cnStr = GetConnectionString(args);
			var query = GetQuery(args);

			//Console.WriteLine("['query result']");
			//Console.Out.WriteLine();
			
			RunQuery(cnStr, query);

			//Console.WriteLine("[");
			//Console.Write("\t[");
			////Console.Write(", ");
			//Console.Write("\"c\"]");
			//Console.WriteLine(",");
			//Console.Write("\t[");
			//Console.Write(10000.ToString());
			//Console.Write("]");
			//Console.WriteLine();
			//Console.Write("]");
			//Console.WriteLine();
		}

		private static string GetConnectionString(string[] args)
		{
			var cnStr = "";
			if (args.Length > 0) cnStr = args[0];
			else cnStr = "Data Source=localhost;Initial Catalog=Scratch;Integrated Security=True";

			return cnStr;
		}

		private static void RunQuery(string cnStr, string query)
		{
			using (var cn = new SqlConnection(cnStr))
			{
				cn.Open();
				using (var cmd = cn.CreateCommand())
				{
					cmd.CommandText = query;

					using (var rdr = cmd.ExecuteReader())
					{
						Console.WriteLine("[");

						Console.Write("\t[");
						for (int i = 0; i < rdr.FieldCount; i++)
						{
							if (i > 0)
								Console.Write(", ");
							Console.Write("\"" + rdr.GetName(i) + "\"");
						}
						Console.Write("]");

						while (rdr.Read())
						{
							Console.WriteLine(",");
							Console.Write("\t[");
							for (int i = 0; i < rdr.FieldCount; i++)
							{
								if (i > 0)
									Console.Write(", ");

								if (rdr.IsDBNull(i))
								{
									Console.WriteLine("null");
									continue;
								}

								var tn = rdr.GetFieldType(i).Name;
								if (tn == "String")
									Console.Write("\"" + rdr.GetString(i) + "\"");
								else if (tn == "Int32")
									Console.Write(rdr.GetInt32(i).ToString());
								else
									Console.WriteLine("null");
							}
							Console.Write("]");
						}
						Console.WriteLine();
						Console.Write("]");
						Console.Out.Close();
						Environment.Exit(0);
						return;
					}
				}
			}
			//Console.WriteLine();
			//Console.Out.Close();
		}

		private static string GetQuery(string[] args)
		{
			var query = "";
			if (args.Length > 1)
			{
				query = args[1];
			}
			else //if (Console.KeyAvailable)
			{
				query = Console.In.ReadToEnd();
			}
			//else
			//{
			//    throw new ArgumentException("query missing.");
			//}
			return query;
		}
	}
}
