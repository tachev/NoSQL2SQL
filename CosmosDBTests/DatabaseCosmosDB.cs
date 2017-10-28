using Geo.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geo.Data.Tests
{
	public static class DatabaseFactory<T> where T : class, IDocument
	{
		public static IDatabase<T> CreateDatabase() {
			return new CosmosDB<T>(Environment.GetEnvironmentVariable("documentdb:endpoint"), Environment.GetEnvironmentVariable("documentdb:authKey"), "nosql2sql", "items");
		}

	}
}
