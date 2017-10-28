using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Geo.Data
{
	public class MSSQLDatabase<T> : IDatabase<T> where T:class, IDocument, new()
	{
		SqlConnection _connection;
		private string _tableName;
		private readonly string _connectionString;

		public MSSQLDatabase(string connectionString)
		{
			_connectionString = connectionString;
			_tableName = typeof(T).Name;
			CreateTableAsync().Wait();
		}

		private SqlConnection Connection
		{
			get
			{
				//TODO: Synchronize http://csharpindepth.com/Articles/General/Singleton.aspx
				if (_connection == null || string.IsNullOrEmpty(_connection.ConnectionString))
				{
					_connection = new SqlConnection(_connectionString);
				}

				return _connection;
			}
		}

		private async Task CreateTableAsync()
		{
			string query = @"IF NOT EXISTS(SELECT * from sysobjects WHERE name = '" + _tableName + @"' AND xtype = 'U')
								CREATE TABLE " + _tableName + @"(
									id		   varchar(50) PRIMARY KEY,
									JSONObject varchar(MAX) NOT NULL )";

			SqlCommand createCommand = new SqlCommand(query.ToString(), Connection);
			await ExecuteCommand(createCommand);
		}

		public async Task<string> CreateItemAsync(T item)
		{
			if (item.Id == null)
			{
				item.Id = Guid.NewGuid().ToString();//TODO: Is that unique?
			}

			SqlCommand command = new SqlCommand();

			StringBuilder query = new StringBuilder();
			query.Append("INSERT INTO  ").Append(_tableName).Append(" (id, JSONObject) VALUES (@id,  @JSONObject)");
			
			command.Parameters.Add("id", SqlDbType.VarChar, 50).Value = item.Id;
			command.Parameters.Add("JSONObject", SqlDbType.VarChar).Value = JsonConvert.SerializeObject(item);

			command.CommandText = query.ToString();
			await ExecuteCommand(command);

			return item.Id;
		}

		public async Task DeleteItemAsync(string id)
		{
			SqlCommand command = new SqlCommand("DELETE FROM " + _tableName + @" WHERE id = @id", Connection);
			command.Parameters.Add("id", SqlDbType.VarChar, 50).Value = id;
			await ExecuteCommand(command);
		}

		public async Task ReplaceItemAsync(T item)
		{
			await ExecuteQueryAsync("UPDATE  " + _tableName + @" SET JSONObject = @JSONObject WHERE id = @id", item);
		}

		public async Task<string> UpsertItemAsync(T item)
		{
			if (item.Id == null) {
				return await CreateItemAsync(item);
			}

			//TODO: Create real upsert with updating only the items that are new;
			await ReplaceItemAsync(item);

			return item.Id;
		}

		private async Task ExecuteQueryAsync(string query, T item)
		{
			SqlCommand command = new SqlCommand(query.ToString(), Connection);
			command.Parameters.Add("id", SqlDbType.VarChar, 50).Value = item.Id;
			command.Parameters.Add("JSONObject", SqlDbType.VarChar).Value = JsonConvert.SerializeObject(item);
			await ExecuteCommand(command);
		}

		private Task ExecuteCommand(SqlCommand command)
		{
			return Task.Run(() =>
			{
				var _connection = Connection;
				using (_connection)
				{
					_connection.Open();
					command.Connection = _connection;
					command.ExecuteNonQuery();
				}
			});
		}
		
		public Task<T> ReadItemAsync(Expression<Func<T, bool>> predicate)
		{
			return Task.Run(() =>
			{
				using (Connection)
				{
					T item = null;
					Connection.Open();

					object value;
					string name = GetPropertyName(predicate, out value);

					if (name != "Id" || !(value is string))
					{
						throw new NotSupportedException("The only supported predicate in this version is of the type item => item.Id = itemId");
					}

					string query = "SELECT id, JSONObject FROM " + _tableName + " WHERE " + name + " = @value"; ;
					SqlCommand command = new SqlCommand(query.ToString(), Connection);
					command.Parameters.Add("value", SqlDbType.VarChar, 50).Value = value;
					var reader = command.ExecuteReader();
					if (reader.Read())
					{
						string jsonObject = (string)reader["JSONObject"];
						item = JsonConvert.DeserializeObject<T>(jsonObject);
					}

					return item;
				}
			});
		}

		private object GetValue(MemberExpression member)
		{
			var objectMember = Expression.Convert(member, typeof(object));
			var getterLambda = Expression.Lambda<Func<object>>(objectMember);
			var getter = getterLambda.Compile();
			return getter();
		}
		
		private string GetPropertyName(Expression<Func<T, bool>> predicate, out object value)
		{
			if (predicate.Body.NodeType == ExpressionType.Equal)
			{
				if (predicate.Body is BinaryExpression)
				{
					var binary = predicate.Body as BinaryExpression;
					value = GetValue((MemberExpression)binary.Right);
					return ((MemberExpression)binary.Left).Member.Name;
				}
			}

			throw new NotSupportedException();
		}

	}
}
