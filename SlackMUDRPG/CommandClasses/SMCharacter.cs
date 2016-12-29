using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public class SMCharacter
    {
        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }

        [JsonProperty("lastinteractiondate")]
        public DateTime LastInteractionDate { get; set; }

        [JsonProperty("lastlogindate")]
        public DateTime LastLogindate { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("sex")]
        public char Sex { get; set; }

        [JsonProperty("PKFlag")]
        public bool PKFlag { get; set; }

        [JsonProperty("userid")]
        public string UserID { get; set; }

        [JsonProperty("RoomLocation")]
        public string RoomLocation { get; set; }

        [JsonProperty("CharacterItems")]
        public List<SMItem> CharacterItems { get; set; }

        [JsonProperty("Skills")]
        public List<SMSkill> Skills { get; set; }
    }
}