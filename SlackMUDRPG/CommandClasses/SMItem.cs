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

		[JsonProperty("PreviousItemFamily")]
		public string PreviousItemFamily { get; set; }

		[JsonProperty("PluralFamilyName")]
		private string PluralFamilyName { get; set; } = null;

		[JsonProperty("ItemDescription")]
		public string ItemDescription { get; set; }

		[JsonProperty("ItemExtraDetail")]
		public string ItemExtraDetail { get; set; }

		[JsonProperty("ItemWeight")]
		public int ItemWeight { get; set; }

		[JsonProperty("ItemCapacity")]
		public int ItemCapacity { get; set; }

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

		[JsonProperty("HeldItems")]
		public List<SMItem> HeldItems { get; set; }

		[JsonProperty("CanHoldFamilies")]
		public List<string> CanHoldFamilies { get; set; }

		[JsonProperty("Effects")]
		public List<SMEffect> Effects { get; set; }

		/// <summary>
		/// Determines if the item can hold other items.
		/// </summary>
		/// <returns>Bool indicating if the item can hold other items.</returns>
		public bool CanHoldOtherItems()
		{
			return this.ItemType == "Container";
		}

		/// <summary>
		/// Determines if the item can hold a given itme based on the properties of both items.
		/// </summary>
		/// <param name="item">The item that is to be put in this item.</param>
		/// <returns>Bool indicating if the item can be put in this item.</returns>
		public bool CanHoldItemByFamily(SMItem item)
		{
			if (!this.CanHoldOtherItems())
			{
				return false;
			}

			if (this.CanHoldFamilies != null && this.CanHoldFamilies.Any())
			{
				if (this.CanHoldFamilies.Contains("any"))
				{
					return true;
				}

				if (CanHoldFamilies.FirstOrDefault(s => s.ToLower() == item.ItemFamily.ToLower()) != null)
				{
					return true;
				}

				return false;
			}

			return true;
		}

		/// <summary>
		/// Determines if the item can hold a given other item by checking the properties of both items and the available
		/// capacity of this item.
		/// </summary>
		/// <param name="item">The item to be put inside this.</param>
		/// <returns>Bool indicating if the item can be put inside this.</returns>
		public bool CanHoldItem(SMItem item)
		{
			if (this.CanHoldOtherItems() && this.CanHoldItemByFamily(item))
			{
				if (SMItemHelper.GetItemAvailbleCapacity(this) >= item.ItemSize)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if this item contains a specified other item.
		/// </summary>
		/// <param name="item">The item to test for inside this.</param>
		/// <returns>Bool indicating of the item is inside this.</returns>
		public bool Contains(SMItem item)
		{
			if (this.HeldItems != null && this.HeldItems.Any())
			{
				SMItem foundItem = SMItemHelper.GetItemFromList(this.HeldItems, item.ItemID);

				return foundItem == null ? false : true;
			}

			return false;
		}

		/// <summary>
		/// Gets a new instance of the items DestroyedOutput.
		/// </summary>
		/// <returns>New item or null.</returns>
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

		/// <summary>
		/// Gets a new instance of the items DestroyedOutput.
		/// </summary>
		/// <returns>New items list or null.</returns>
		public List<SMItem> GetDestroyedItems()
		{
			// Create the return list.
			List<SMItem> smil = new List<SMItem>();

			if (this.DestroyedOutput != null)
			{
				string[] splitDestroyedObjects = this.DestroyedOutput.Split('|');

				foreach (string destroyedObject in splitDestroyedObjects)
				{
					// get "xxx.yyy.zzz" from "xxx.yyy.zzz,n"
					string item = destroyedObject.Split(',')[0];

					// get list of parts x, y, z from x.y.z
					List<string> parts = item.Split('.').ToList();

					// get and remove x from the list
					string category = parts[0];
					parts.RemoveAt(0);

					// get y.z by joining the remaining elements
					string name = string.Join(".", parts);

					smil.Add(SMItemFactory.Get(category, name));
				}
			}

			return smil;
		}

        /// <summary>
        /// Initate an effect from an item.
        /// </summary>
        /// <param name="smc">The character the effect will take place on</param>
        public void InitiateEffects(SMCharacter smc)
        {
            if (this.Effects != null)
            {
                foreach (SMEffect sme in this.Effects)
                {
                    switch (sme.Action)
                    {
                        case "OnExamine":
                            if (sme.EffectType == "AddQuest")
                            {
                                SMQuest smq = SMQuestFactory.Get(sme.AdditionalData);
                                if (smq != null)
                                {
                                    smc.AddQuest(smq);
                                }
                            }
                            break;
                    }
                }
            }
        }

        public string GetSingularItemName()
        {
            return this.SingularPronoun + " " + this.ItemName;
        }

		/// <summary>
		/// Get the plural family name of the item. If this is null then just add an 's' to the family name.
		/// </summary>
		/// <returns>The plural family name.</returns>
		public string GetPluralFamilyName()
		{
			if (this.PluralFamilyName == null)
			{
				return $"{this.ItemFamily}s";
			}

			return this.PluralFamilyName;
		}
	}
}