using Geo.Data;
using System.Collections.Generic;
using System.Linq;

namespace Geo.Data
{
	internal static class UpdateHelper
	{
		internal static void UpdateItem<T>(T oldItem, T item) where T : class, IDocument, new()
		{
			var properties = oldItem.GetType().GetProperties();

			foreach (var property in properties)
			{
				if (property.PropertyType.GetInterfaces().Contains(typeof(ICollection<T>)))
				{
					UpdateListOfTheSameType((ICollection<T>)property.GetValue(oldItem), (ICollection<T>)property.GetValue(item));
				}
				else if (property.SetMethod != null)
				{
					var newValue = property.GetValue(item);
					if (newValue != null)
					{
						property.SetValue(oldItem, newValue);
					}
				}
			}
		}

		private static void UpdateListOfTheSameType<T>(ICollection<T> oldItems, ICollection<T> newItems) where T : class, IDocument, new()
		{
			if (newItems == null)
			{
				return;
			}

			foreach (var item in newItems)
			{
				var oldItem = oldItems.FirstOrDefault(a => a.Id == item.Id);
				if (oldItem != null)
				{
					UpdateItem(oldItem, item);
				}
				else
				{
					oldItems.Add(item);
				}
			}
		}
	}
}
