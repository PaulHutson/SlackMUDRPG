using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMCreature
	{
        [JsonProperty("CreatureName")]
        public string CreatureName { get; set; }

        [JsonProperty("CreatureType")]
        public string CreatureType { get; set; }
        
        [JsonProperty("Age")]
        public int Age { get; set; }

        [JsonProperty("Sex")]
        public char Sex { get; set; }
        
        [JsonProperty("CreatureID")]
        public string CreatureID { get; set; }

        [JsonProperty("RoomID")]
        public string RoomID { get; set; }

        [JsonProperty("CurrentActivity")]
        public string CurrentActivity { get; set; }
        
        [JsonProperty("Skills")]
        public List<SMSkillHeld> Skills { get; set; }

        [JsonProperty("Attributes")]
        public SMAttributes Attributes { get; set; }
    }
}