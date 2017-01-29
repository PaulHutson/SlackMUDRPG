using Newtonsoft.Json;
using SlackMUDRPG.Utility.Formatters;
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

        public void RespondToAction(string actionType, SMCharacter invokingCharacter)
        {
            // Get a list of characters that respond to this action type in the room
            List<NPCResponses> listToChooseFrom = NPCResponses.FindAll(npcr => npcr.ResponseType == actionType);

            // If there are some responses for this character for the actionType
            if (listToChooseFrom != null)
            {
                // If there is more than one of the item randomise the list
                if (listToChooseFrom.Count > 1) { 
                    listToChooseFrom.OrderBy(item => new Random().Next());
                }

                // Loop around until a response is selected
                bool responseSelected = false;
                foreach (NPCResponses npr in listToChooseFrom)
                {
                    // If we're still looking for a response try the next one (if there is one)
                    if (!responseSelected)
                    {
                        // randomly select whether this happens or not
                        int rndChance = new Random().Next(1, 100);
                        if (rndChance <= npr.Frequency)
                        {
                            // Process the response
                            ProcessResponse(npr, invokingCharacter);

                            // Set that a response has been selected so we can drop out of the loop
                            responseSelected = true;
                        }
                    }
                }
            }
        }

        private void ProcessResponse(NPCResponses npr, SMCharacter invokingCharacter)
        {
            // Process each of the response steps
            foreach (NPCResponseStep NPCRS in npr.ResponseSteps)
            {
                // Get the invoking character
                invokingCharacter = new SlackMud().GetCharacter(invokingCharacter.UserID);

                // Check the character is still in the same room
                if (invokingCharacter.RoomID == this.RoomID)
                {
                    switch (NPCRS.ResponseStepType)
                    {
                        case "Conversation":
                            ProcessConversation(NPCRS, invokingCharacter);
                            break;
                        case "Attack":
                            // TODO
                            break;
                        case "UseSkill":
                            // TODO
                            break;
                    }
                }
            }
        }

        private void ProcessConversation(NPCResponseStep NPCRS, SMCharacter invokingCharacter)
        {
            // First get the conversation
            string[] rsd = NPCRS.ResponseStepData.Split('.');

            // Get the conversation
            NPCConversations npcc = this.NPCConversationStructures.FirstOrDefault(constructure => constructure.ConversationID == rsd[0]);

            // Check we definitely found a structure to use
            if (npcc != null)
            {
                ProcessConversationStep(npcc, rsd[1], invokingCharacter);
            }
        }

        private void ProcessConversationStep(NPCConversations npcc, string stepID, SMCharacter invokingCharacter)
        {
            NPCConversationStep npccs = npcc.ConversationSteps.FirstOrDefault(cs => cs.StepID == stepID);
            if (npccs != null)
            {
                switch (npccs.Scope) {
                    case "choice":
                        string[] choices = npccs.AdditionalData.Split(',');
                        int choicesNumber = choices.Count();
                        int randomChoice = (new Random().Next(1, choicesNumber+1))-1;
                        if (randomChoice > choicesNumber)
                        {
                            randomChoice = 0;
                        }
                        ProcessConversationStep(npcc, choices[randomChoice], invokingCharacter);
                        break;
                    case "say":
                        this.Say(ProcessResponseString(npccs.AdditionalData, invokingCharacter));
                        break;
                    case "shout":
                        this.Shout(ProcessResponseString(npccs.AdditionalData, invokingCharacter));
                        break;
                    case "whisper":
                        this.Whisper(ProcessResponseString(npccs.AdditionalData, invokingCharacter), invokingCharacter.GetFullName());
                        break;
                    case "emote":
                        this.Emote(ProcessResponseString(npccs.AdditionalData, invokingCharacter));
                        break;
                    case "saytoplayer":
                        // Construct the message
                        string sayToPlayerMessage = OutputFormatterFactory.Get().Italic(this.GetFullName() + " says:", 0) + " \"" + ProcessResponseString(npccs.AdditionalData, invokingCharacter) + "\"";

                        // Send the message
                        invokingCharacter.sendMessageToPlayer(sayToPlayerMessage);
                        break;
                    case "emotetoplayer":
                        // Construct the message
                        string emoteToPlayerMessage = OutputFormatterFactory.Get().Italic(this.GetFullName() + " " + ProcessResponseString(npccs.AdditionalData, invokingCharacter));

                        // Send the message
                        invokingCharacter.sendMessageToPlayer(emoteToPlayerMessage);
                        break;
                    case "wait":
                        System.Threading.Thread.Sleep(int.Parse(npccs.AdditionalData) * 1000);
                        break;
                }

                if (npccs.NextStep != null)
                {
                    string[] splitNextStep = npccs.NextStep.Split('.');
                    if (splitNextStep[1] != "0")
                    {
                        System.Threading.Thread.Sleep(int.Parse(splitNextStep[1]) * 1000);
                        ProcessConversationStep(npcc, splitNextStep[0], invokingCharacter);
                    }
                }
            }
        }

        private string ProcessResponseString(string responseStringToProcess, SMCharacter invokingCharacter)
        {
            string responseString = responseStringToProcess;
            responseString = responseString.Replace("{playercharacter}", invokingCharacter.GetFullName());

            return responseString;
        }
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
    /// - Conversation
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