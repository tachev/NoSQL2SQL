using Geo.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Geo.Data.Tests
{
	public class Item : IDocument
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "documentType")]
		public string DocumentType
		{
			get
			{
				return "Item";
			}
		}

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "listOfItems")]
		public List<Item> ListOfItems { get; set; }

		[JsonProperty(PropertyName = "intField")]
		public int IntField { get; set; }

	}
}
