using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public class SMItem
    {
		[JsonProperty("ItemID")]
		public string ItemID { get; set; }

        [JsonProperty("ItemName")]
        public string ItemName { get; set; }

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

        [JsonProperty("HeldItems")]
        public SMItem[] HeldItems { get; set; }

        public SMItem GetDestroyedItem()
        {
            return SlackMud.CreateItemFromJson(this.DestroyedOutput);
        }
    }
}