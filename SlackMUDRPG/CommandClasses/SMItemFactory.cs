using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.CommandClasses
{
	public static class SMItemFactory
	{
		/// <summary>
		///  Builds and returns a new item based on the provided type and name.
		/// </summary>
		/// <param name="itemType">The type of the new item, determines the SMItem subclass to use.</param>
		/// <param name="itemName">The name of the new item, determines the file to get the spec from.</param>
		/// <returns>A new item of null.</returns>
		public static SMItem Get(string itemType, string itemName)
		{
			string itemSpec = GetItemSpecJson(itemType, itemName);

			if (itemSpec != "")
			{
				string guid = Guid.NewGuid().ToString();

				return GetItem(guid, itemSpec);
			}

			return null;
		}

		/// <summary>
		/// Gets a string representing the json spec for an item.
		/// </summary>
		/// <param name="itemType">The items type.</param>
		/// <param name="itemName">THe items name.</param>
		/// <returns>Json spec for the item or an empty string.</returns>
		private static string GetItemSpecJson(string itemType, string itemName)
		{
			string filename = $"{itemType}.{itemName}";
			string filePath = FilePathSystem.GetFilePath("Objects", filename);

			string objectSpecJson = "";

			if (File.Exists(filePath))
			{
				using (StreamReader reader = new StreamReader(filePath))
				{
					objectSpecJson = reader.ReadToEnd();
				}
			}

			return objectSpecJson;
		}

		/// <summary>
		/// Gets a new general item based on the spec provided.
		/// </summary>
		/// <param name="guid">A guid for the new item.</param>
		/// <param name="jsonSpec">The spec of the item as a json string.</param>
		/// <returns>An SMItem instance.</returns>
		private static SMItem GetItem(string guid, string jsonSpec)
		{
			SMItem item = JsonConvert.DeserializeObject<SMItem>(jsonSpec);
			item.ItemID = guid;

			return item;
		}
	}
}