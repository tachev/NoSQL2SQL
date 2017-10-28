using Geo.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Geo.Data.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void CreateDatabase()
        {
			IDatabase<Item> database = DatabaseFactory<Item>.CreateDatabase();

			Assert.IsNotNull(database);
        }

		[TestMethod]
		public async System.Threading.Tasks.Task CreateItemTestAsync()
		{
			IDatabase<Item> database = DatabaseFactory<Item>.CreateDatabase();

			var id = await database.CreateItemAsync(CreateTestItem());

			Assert.IsNotNull(id);
		}

		
		[TestMethod]
		public async System.Threading.Tasks.Task ReadItemTestAsync()
		{
			IDatabase<Item> database = DatabaseFactory<Item>.CreateDatabase();

			var item = CreateTestItem();

			var id = await database.CreateItemAsync(item);

			var newItem = await database.ReadItemAsync(a => a.Id == id);

			Assert.AreEqual(id, newItem.Id);

			Assert.AreEqual(item.Name, newItem.Name);


			for (int i = 0; i < 10; i++)
			{
				var oldSubItem = item.ListOfItems[i];
				var newSubItem = newItem.ListOfItems[i];
				Assert.AreEqual(oldSubItem.Name, newSubItem.Name);
				Assert.AreEqual(oldSubItem.IntField, newSubItem.IntField);
			}
		}

		[TestMethod]
		public async System.Threading.Tasks.Task UpsertTestAsync()
		{
			IDatabase<Item> database = DatabaseFactory<Item>.CreateDatabase();

			var item = CreateTestItem();

			var id = await database.UpsertItemAsync(item);

			Assert.IsNotNull(id);

			item.Id = id;

			var newId = await database.UpsertItemAsync(item);

			Assert.AreEqual(id, newId);

		}


		[TestMethod]
		public async System.Threading.Tasks.Task DeleteTestAsync()
		{
			IDatabase<Item> database = DatabaseFactory<Item>.CreateDatabase();

			var item = CreateTestItem();

			var id = await database.CreateItemAsync(item);

			item = await database.ReadItemAsync(a => a.Id == id);

			await database.DeleteItemAsync(id);
			
			item = await database.ReadItemAsync(a => a.Id == id);

			Assert.IsNull(item);
		}
		
		private Item CreateTestItem()
		{
			Item item = new Item() { Name = "Item Name" };
			item.ListOfItems = new List<Item>();

			for (int i = 0; i < 10; i++)
			{
				item.ListOfItems.Add(new Item { Name = "Sub item", IntField = i });
			}

			return item;
		}
	}
}
