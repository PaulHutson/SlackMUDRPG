using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public class SMSkill
    {
        [JsonProperty("SkillType")]
        public string SkillType { get; set; }

        [JsonProperty("SkillName")]
        public string SkillName { get; set; }

        [JsonProperty("SkillLevel")]
        public int SkillLevel { get; set; }

        [JsonProperty("SkillXP")]
        public int SkillXP { get; set; }
    }
}