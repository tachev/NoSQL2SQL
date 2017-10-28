using Geo.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geo.Data.Tests
{
	public static class DatabaseFactory<T> where T : class, IDocument, new()
	{
		public static IDatabase<T> CreateDatabase() {
			return new MSSQLDatabase<T>(Environment.GetEnvironmentVariable("sqldb"));
		}

	}
}
