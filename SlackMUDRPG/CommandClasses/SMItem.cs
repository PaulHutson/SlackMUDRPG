using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public class SMItem
    {
        [JsonProperty("ItemName")]
        public string ItemName { get; set; }

        [JsonProperty("ItemType")]
        public string ItemType { get; set; }

        [JsonProperty("ItemDescription")]
        public string ItemDescription { get; set; }

        [JsonProperty("ItemWeight")]
        public int ItemWeight { get; set; }

        [JsonProperty("ItemSize")]
        public int ItemSize { get; set; }

        [JsonProperty("CanHoldOtherItems")]
        public bool CanHoldOtherItems { get; set; }

        [JsonProperty("HeldItems")]
        public SMItem[] HeldItems { get; set; }
    }
}