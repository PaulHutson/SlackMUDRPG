using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMCharacterSlot
	{
		[JsonProperty("Name")]
		public string Name { get; set; }

		[JsonProperty("AllowedTypes")]
		public List<string> AllowedTypes { get; set; }

		[JsonProperty("EquippedItem")]
		public SMItem EquippedItem { get; set; }

		/// <summary>
		/// Checks if the slot is empty
		/// </summary>
		/// <returns><c>true</c>, if slot is empty, <c>false</c> otherwise.</returns>
		public bool isEmpty()
		{
			return this.EquippedItem == null;
		}

		/// <summary>
		/// Checks if a given item can be equipped, based on its type and the slots status
		/// </summary>
		/// <returns><c>true</c>, if the iten can be equipped, <c>false</c> otherwise.</returns>
		/// <param name="item">Item.</param>
		public bool canEquipItem(SMItem item)
		{
			if (this.isEmpty())
			{
				if (this.AllowedTypes.Contains("any"))
				{
					return true;
				}

				return this.AllowedTypes.Contains(item.ItemType);
			}

			return false;
		}
	}
}