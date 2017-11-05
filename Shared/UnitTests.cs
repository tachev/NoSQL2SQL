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
		public async System.Threading.Tasks.Task UpsertSubItemTestAsync()
		{
			IDatabase<Item> database = DatabaseFactory<Item>.CreateDatabase();

			var item = CreateTestItem();

			var id = await database.UpsertItemAsync(item);

			Assert.IsNotNull(id);

			//Insert a sub item and changing the name
			var updateItem = new Item
			{
				Id = id,
				Name = "New Name",
				ListOfItems = new List<Item> {
					new Item {Id = "1", Name = "Updated subitem", IntField = 1 },
					new Item {Id = "10", Name = "New subitem", IntField = 10 }
				}
			};

			var newId = await database.UpsertItemAsync(updateItem);

			var readItem = await database.ReadItemAsync(a => a.Id == id);

			//Check if the name has changed
			Assert.AreEqual("New Name", readItem.Name);

			//Check if the number of items grew
			Assert.AreEqual(11, readItem.ListOfItems.Count);

			//Check if the "0" item exists and didn't change
			Assert.AreEqual("Subitem", readItem.ListOfItems.Find(a => a.Id == "0").Name);

			//Check if the "1" item exists and changed his title, but didn't change the int value
			Assert.AreEqual("Updated subitem", readItem.ListOfItems.Find(a => a.Id == "1").Name);
			Assert.AreEqual(1, readItem.ListOfItems.Find(a => a.Id == "1").IntField);

			//Check if the "10" item is saved correctly
			Assert.AreEqual("New subitem", readItem.ListOfItems.Find(a => a.Id == "10").Name);
			Assert.AreEqual(10, readItem.ListOfItems.Find(a => a.Id == "10").IntField);
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
				item.ListOfItems.Add(new Item { Id = i.ToString(), Name = "Subitem", IntField = i });
			}

			return item;
		}
	}
}
