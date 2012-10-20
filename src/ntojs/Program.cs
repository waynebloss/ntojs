using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
using System.IO;

namespace ntojs
{
	class Program
	{
		static void Main(string[] args)
		{
			TestQuery.Go(args);
			//TestInOut.Go(args);
		}
	}
}
