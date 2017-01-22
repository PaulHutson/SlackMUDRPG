using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMItem
	{
		[JsonProperty("ItemID")]
		public string ItemID { get; set; }

		[JsonProperty("SingularPronoun")]
		public string SingularPronoun { get; set; }

		[JsonProperty("ItemName")]
		public string ItemName { get; set; }

		[JsonProperty("PluralName")]
		public string PluralName { get; set; }

		[JsonProperty("PluralPronoun")]
		public string PluralPronoun { get; set; }

		[JsonProperty("ItemType")]
		public string ItemType { get; set; }

		[JsonProperty("ItemFamily")]
		public string ItemFamily { get; set; }

		[JsonProperty("ItemDescription")]
		public string ItemDescription { get; set; }

		[JsonProperty("ItemWeight")]
		public int ItemWeight { get; set; }

		[JsonProperty("ItemSize")]
		public int ItemSize { get; set; }

		[JsonProperty("HitPoints")]
		public int HitPoints { get; set; }

		[JsonProperty("MaxHitPoints")]
		public int MaxHitPoints { get; set; }

		[JsonProperty("BaseDamage")]
		public float BaseDamage { get; set; }

		[JsonProperty("Toughness")]
		public int Toughness { get; set; }

		[JsonProperty("DestroyedOutput")]
		public string DestroyedOutput { get; set; }

		[JsonProperty("RequiredSkills")]
		public List<SMRequiredSkill> RequiredSkills { get; set; }

        [JsonProperty("AdditionalData")]
        public string AdditionalData { get; set; }

		[JsonProperty("ObjectTrait")]
		public string ObjectTrait { get; set; }

		/// <summary>
		/// Determines if the item can hold other items.
		/// Decalred virtual so this can be overridden in sub classes
		/// </summary>
		/// <returns>Bool indicating if the item can hold other items.</returns>
		public virtual bool canHoldOtherItems()
		{
			return false;
		}

		/// <summary>
		/// Gets a new instance of the items DestroyedOutput
		/// </summary>
		/// <returns>New item or null</returns>
		public SMItem GetDestroyedItem()
		{
			if (this.DestroyedOutput != null)
			{
				// get "xxx.yyy.zzz" from "xxx.yyy.zzz,n"
				string item = this.DestroyedOutput.Split(',')[0];

				// get list of parts x, y, z from x.y.z
				List<string> parts = item.Split('.').ToList();

				// get and remove x from the list
				string category = parts[0];
				parts.RemoveAt(0);

				// get y.z by joining the remaining elements
				string name = string.Join(".", parts);

				// Return a newly factoried item
				return SMItemFactory.Get(category, name);
			}

			return null;
		}
	}
}