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

		[JsonProperty("ItemName")]
		public string ItemName { get; set; }

		[JsonProperty("SingularPronoun")]
		public string SingularPronoun { get; set; }

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

		[JsonProperty("CanHoldOtherItems")]
		public bool CanHoldOtherItems { get; set; }

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

		[JsonProperty("HeldItems")]
		public List<SMItem> HeldItems { get; set; }

		public SMItem GetDestroyedItem()
		{
			string createFileItemName = this.DestroyedOutput;
			if (createFileItemName.Contains(","))
			{
				createFileItemName = createFileItemName.Split(',')[0];
			}
			
			return new SlackMud().CreateItemFromJson(createFileItemName);
		}
	}

	public class SMRequiredSkill
	{
		[JsonProperty("SkillName")]
		public string SkillName { get; set; }

		[JsonProperty("SkillLevel")]
		public string SkillLevel { get; set; }
	}
}