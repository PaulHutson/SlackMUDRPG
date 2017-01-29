using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	/// <summary>
	/// Object used to create lists of items by name with a count.
	/// </summary>
	public class ItemCountObject
	{
		public string SingularName { get; set; }
		public string PluralName { get; set; }
		public int Count { get; set; }
	}

	/// <summary>
	/// Items utility class to provide various item functions to the whole application.
	/// </summary>
	public static class SMItemUtils
	{
		/// <summary>
		/// Processes a list of SMItem objects to produce a list of ItemCountObjects
		/// </summary>
		/// <param name="items">SMItem list to process</param>
		/// <returns>List of objects counting items by name.</returns>
		public static List<ItemCountObject> GetItemCountList(List<SMItem> items)
		{
			var query = from item in items
						group item by item.ItemName into grp
						select new ItemCountObject()
						{
							SingularName = grp.Key,
							PluralName = grp.ToList().First().PluralName,
							Count = grp.Count()
						};

			return query.ToList();
		}
	}
}