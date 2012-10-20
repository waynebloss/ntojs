using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
using System.IO;

namespace ntojs
{
	class TestInOut
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
			var cnStr = args[0];
			
			//using (var stdin = Console.OpenStandardInput())
			//using (var stdout = Console.OpenStandardOutput())
			//{
			//    var buffer = new byte[2048];
			//    int bytes = 0;
			//    while ((bytes = stdin.Read(buffer, 0, buffer.Length)) > 0)
			//    {
			//        stdout.Write(buffer, 0, bytes);
			//    }
			//}

			//using (var stdout = Console.OpenStandardOutput())
			//using (var stdin = Console.OpenStandardInput(4))
			//{
			//    var buffer = new byte[4];
			//    int len = 0;

			//    // Read
			//    while ((len = stdin.Read(buffer, 0, 4)) > 0)
			//    {
			//        var txtLen = BitConverter.ToInt32(buffer, 0);

			//        var txtBuffer = new char[txtLen];
			//        Console.In.ReadBlock(txtBuffer, 0, txtLen);

			//        // Write
			//        stdout.Write(BitConverter.GetBytes(txtLen), 0, 4);

			//        Console.Out.Write(txtBuffer);
			//    }
			//}


		}
	}
}
