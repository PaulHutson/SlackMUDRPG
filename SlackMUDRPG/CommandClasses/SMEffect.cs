using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    public class SMEffect
    {
        [JsonProperty("Action")]
        public string Action { get; set; }

        [JsonProperty("EffectType")]
        public string EffectType { get; set; }

        [JsonProperty("AdditionalData")]
        public string AdditionalData { get; set; }
    }
}