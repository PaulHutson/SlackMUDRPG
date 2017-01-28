using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    public class SMNPC : SMCharacter
    {
        [JsonProperty("NPCResponses")]
        public List<NPCResponses> NPCResponses { get; set; }

        [JsonProperty("NPCConversationStructures")]
        public List<NPCConversations> NPCConversationStructures { get; set; }

        [JsonProperty("NPCMovementAlgorithms")]
        public List<NPCMovements> NPCMovementAlgorithms { get; set; }

        [JsonProperty("NPCMovementTarget")]
        public NPCMovementTarget NPCMovementTarget { get; set; }
    }

    /// <summary>
    /// Response types can be of the following types:
    /// - PlayerCharacter.Enter (Faction = FactionName.Threshold, AdditionalData = Player.Known)
    /// - PlayerCharacter.Leave (Faction = FactionName.Threshold, AdditionalData = Player.Known)
    /// - PlayerCharacter.Attack
    /// - PlayerCharacter.SayNPCName (Faction = FactionName.Threshold, AdditionalData = Player.Known)
    /// - PlayerCharacter.SayKeyWord (Faction = FactionName.Threshold, AdditionalData = Player.Known)
    /// - PlayerCharacter.UseSkillOnThem (AdditionalData = the skill used)
    /// - PlayerCharacter.UseSkillNotOnThem (AdditionalData = the skill used)
    /// - PlayerCharacter.ExaminesThem
    /// - PlayerCharacter.InRoom (Faction = FactionName.Threshold, frequency should be lower on this)
    /// - NPC.Enter
    /// - NPC.Leave
    /// - NPC.ExaminesThem
    /// - NPC.Attack
    /// 
    /// Frequency is set to how often a character will do something
    /// this is automatically set to be 100 by default (i.e. they will
    /// do it 100% of the time).
    /// </summary>
    public class NPCResponses
    {
        [JsonProperty("ResponseType")]
        public string ResponseType { get; set; }

        [JsonProperty("ResponseTimeOfDay")]
        public string ResponseTimeOfDay { get; set; } // all, morning, afternoon, night, day

        [JsonProperty("AdditionalData")]
        public string AdditionalData { get; set; } // Could be not wanted

        [JsonProperty("Faction")]
        public string Faction { get; set; }

        [JsonProperty("Frequency")]
        public int Frequency { get; set; }

        [JsonProperty("ResponseSteps")]
        public List<NPCResponseStep> ResponseSteps { get; set; }
    }

    /// <summary>
    /// Response Steps execute on something happening.
    /// 
    /// The following response steps are available:
    /// - StartConversation
    ///     Data: ConversationID.StepID
    /// - Attack
    ///     Data: TheThingToAttack
    /// - UseSkill
    ///     Data: The skill to be used
    /// </summary>
    public class NPCResponseStep
    {
        [JsonProperty("ResponseStepType")]
        public string ResponseStepType { get; set; }

        [JsonProperty("ResponseStepData")]
        public string ResponseStepData { get; set; }
    }

    /// <summary>
    /// Groups conversation steps together
    /// </summary>
    public class NPCConversations
    {
        [JsonProperty("ConversationID")]
        public string ConversationID { get; set; }

        [JsonProperty("ConversationStep")]
        public List<NPCConversationStep> ConversationSteps { get; set; }
    }

    /// <summary>
    /// NPCConversation Steps, allow for multiple conversations to happen 
    /// at once (both privately and globally).  Characters will also be able
    /// to dip in and out of conversations (based on actions performed).
    /// </summary>
    public class NPCConversationStep
    {
        [JsonProperty("StepID")]
        public string StepID { get; set; }

        [JsonProperty("Scope")]
        public string Scope { get; set; } // ((Data:Number Range), say, shout, whisper, saytoplayer, emote, emotetoplayer, wait)

        [JsonProperty("AdditionalData")]
        public string AdditionalData { get; set; } // what is said

        [JsonProperty("NextStep")]
        public string NextStep { get; set; } // NextStepID.WaitTime : What the next conversation step is

        [JsonProperty("ResponseOptions")]
        public List<NPCConversationStepResponseOptions> ResponseOptions { get; set; }
    }

    /// <summary>
    /// Response options are groups of responses that a player (or, later and
    /// NPC can respond with).
    /// </summary>
    public class NPCConversationStepResponseOptions
    {
        [JsonProperty("ResponseOptionShortcut")]
        public string ResponseOptionShortcut { get; set; }

        [JsonProperty("ResponseOptionText")]
        public string ResponseOptionText { get; set; }

        [JsonProperty("ResponseOptionActionSteps")]
        public List<NPCResponseOptionAction> ResponseOptionActionSteps { get; set; }
    }

    /// <summary>
    /// Response action steps govern what happen when a certain response is 
    /// made by a character.
    /// 
    /// Response Option Types include:
    /// - Conversation
    ///     AdditionalData: The conversation ID and step ID
    /// - Emote
    ///     AdditionalData: The thing they emote
    /// - GiveItem
    ///     AdditionalData: ItemType.Amount
    /// - DropItem
    ///     AdditionalData: ItemType.Amount
    /// - AddPlayerQuest
    ///     AdditionalData: QuestID
    /// - PlayerQuestUpdate
    ///     AdditionalData: QuestIDStep
    /// - UseSkill
    ///     AdditionalData: SkillToUse
    /// - TakeItem
    ///     AdditionalData: ItemType.Number.Target
    /// </summary>
    public class NPCResponseOptionAction
    {
        [JsonProperty("NPCResponseOptionActionType")]
        public string NPCResponseOptionActionType { get; set; }

        [JsonProperty("AdditionalData")]
        public string AdditionalData { get; set; }
    }

    public class NPCMovements
    {
        [JsonProperty("TimeOfDay")]
        public string TimeOfDay { get; set; }  // Can be "any", Night, day, or specific hours

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