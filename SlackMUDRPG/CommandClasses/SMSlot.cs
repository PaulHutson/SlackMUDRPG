using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace SlackMUDRPG.CommandClasses
{
	public class SMSlot
	{
		[JsonProperty("Name")]
		public string Name { get; set; }

		[JsonProperty("AllowedTypes")]
		public List<string> AllowedTypes { get; set; }

		[JsonProperty("EquippedItem")]
		public SMItem EquippedItem { get; set; }

		/// <summary>
		/// Get a human readable string representing the slots name by adding spaces before uppercase characters.
		/// </summary>
		/// <returns>Readable verion of the slot name.</returns>
		public string GetReadableName()
		{
			Regex exp = new Regex(@"(?!^)(?=[A-Z])");
			return exp.Replace(this.Name, " ");
		}

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

		/// <summary>
		/// Gets the name of the item equiped in this slot.
		/// </summary>
		/// <returns>Name of the equipped item or Empty</returns>
		public string GetEquippedItemName()
		{
			if (this.isEmpty())
			{
				return "Empty";
			}

			return this.EquippedItem.ItemName;
		}
	}
}