using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Geo.Data
{
	public class CosmosDB<T> :IDatabase<T> where T : class, IDocument
	{
		private DocumentClient _client;
		private string _endPoint;
		private string _authKey;
		private string _databaseId;
		private string _collectionId;

		protected DocumentClient Client
		{
			get
			{
				if (_client == null)
				{
					_client = new DocumentClient(new Uri(_endPoint), _authKey);
					CreateDatabaseIfNotExistsAsync().Wait();
					CreateCollectionIfNotExistsAsync().Wait();
				}
				return _client;
			}
		}
		
		public CosmosDB(string endPoint, string authKey, string databaseId, string collectionId)
		{
			this._endPoint = endPoint;
			this._authKey = authKey;
			this._databaseId = databaseId;
			this._collectionId = collectionId;
		}

		public void ResetConfig(string endPoint, string authKey, string databaseId, string collectionId)
		{
			this._endPoint = endPoint;
			this._authKey = authKey;

			this._databaseId = databaseId;
			this._collectionId = collectionId;
			//On updating the configuration we need to create new client with the new settings. We'll do that later in the singleton, here we just need to reset it
			_client = null;
		}

		private void ConfigSettings_Updated(object sender, EventArgs e)
        {
        }

        private async Task CreateDatabaseIfNotExistsAsync()
		{
			try
			{
				await Client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					await Client.CreateDatabaseAsync(new Database { Id = _databaseId });
				}
				else
				{
					throw;
				}
			}
		}

		private async Task CreateCollectionIfNotExistsAsync()
		{
			try
			{
				await Client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					await Client.CreateDocumentCollectionAsync(
						UriFactory.CreateDatabaseUri(_databaseId),
						new DocumentCollection { Id = _collectionId },
						new RequestOptions { OfferThroughput = 1000 });
				}
				else
				{
					throw;
				}
			}
		}

		public async Task<List<T>> ReadItemsAsync(Expression<Func<T, bool>> predicate)
		{
			IDocumentQuery<T> query = Client.CreateDocumentQuery<T>(
				UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId))
				.Where(predicate)
				.AsDocumentQuery();

			List<T> results = new List<T>();
			while (query.HasMoreResults)
			{
				results.AddRange(await query.ExecuteNextAsync<T>());
			}

			return results;
		}

		public async Task<T> ReadItemAsync(Expression<Func<T, bool>> predicate)
		{
			IDocumentQuery<T> query = Client.CreateDocumentQuery<T>(
				UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId))
				.Where(predicate)
				.AsDocumentQuery();
			
			if (query.HasMoreResults)
			{
				var results = await query.ExecuteNextAsync<T>();
				return results.FirstOrDefault();
			}

			return null;
		}

		public async Task<string> CreateItemAsync(T newItem)
		{
			Uri collUri = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
			var result = await Client.CreateDocumentAsync(collUri, newItem);
			return result.Resource.Id;
		}

        public async Task<string> UpsertItemAsync(T item)
		{
			Uri collUri = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
			var result = await Client.UpsertDocumentAsync(collUri, item);
			return result.Resource.Id;
		}

        public async Task ReplaceItemAsync(T item) 
		{
			Uri collUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, item.Id);
			await Client.ReplaceDocumentAsync(collUri, item);
		}

		public async Task DeleteItemAsync(string itemId)
		{
			Uri documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, itemId);
			await Client.DeleteDocumentAsync(documentUri);
		}
	}
}