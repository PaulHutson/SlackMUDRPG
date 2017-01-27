using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    public class SMNPC : SMCharacter
    {
        [JsonProperty("Responses")]
        public List<NPCResponses> Responses { get; set; }

        [JsonProperty("ConversationStructures")]
        public List<NPCConversations> ConversationStructures { get; set; }

        [JsonProperty("MovementAlgorithms")]
        public List<NPCMovements> MovementAlgorithms { get; set; }

        [JsonProperty("MovementTarget")]
        public NPCMovementTarget MovementTarget { get; set; }
    }

    public class NPCResponses
    {
        [JsonProperty("ResponseType")]
        public string ResponseType { get; set; }
    }

    public class NPCConversations
    {

    }

    public class NPCMovements
    {
        [JsonProperty("TimeOfDay")]
        public string TimeOfDay { get; set; }

        [JsonProperty("MovementTargetID")]
        public string MovementTargetID { get; set; }

        [JsonProperty("MovementSpeed")]
        public string MovementSpeed { get; set; }
    }

    public class NPCMovementTarget
    {
        [JsonProperty("MovementTargetID")]
        public string MovementTargetID { get; set; }

        [JsonProperty("MovementSpeed")]
        public string MovementSpeed { get; set; }

        [JsonProperty("LastMoveUnixTime")]
        public int LastMoveUnixTime { get; set; }
    }
}