using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Geo.Data
{
	public interface IDatabase<T> where T : class, IDocument, new()
	{
		Task<string> CreateItemAsync(T item);

		Task<T> ReadItemAsync(Expression<Func<T, bool>> predicate);

		Task<T> ReadItemByIdAsync(string id);

		Task<string> UpsertItemAsync(T item);

		Task ReplaceItemAsync(T item);

		Task DeleteItemAsync(string id);

		Task<List<T>> SearchAsync(string searchQuery, string orderBy);
	}
}
