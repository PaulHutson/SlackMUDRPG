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

        // Used for in memory storing of responses requested from a character
        public List<SMNPCAwaitingCharacterResponse> AwaitingCharacterResponses { get; set; }

        public void RespondToAction(string actionType, SMCharacter invokingCharacter)
        {
            // Get a list of characters that respond to this action type in the room
            List<NPCResponses> listToChooseFrom = NPCResponses.FindAll(npcr => npcr.ResponseType == actionType);

            // If there are some responses for this character for the actionType
            if (listToChooseFrom != null)
            {
                // If there is more than one of the item randomise the list
                if (listToChooseFrom.Count > 1) {
					listToChooseFrom = listToChooseFrom.OrderBy(item => new Random().Next()).ToList();
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
							// If the invoking character is null
							if (invokingCharacter == null)
							{
								// Get a random player (in line with the scope of the additional data)
								invokingCharacter = this.GetRoom().GetRandomCharacter(this, npr.AdditionalData);
							}

							// If the invoking character is not null
							if (invokingCharacter != null)
							{
								// Process the response
								ProcessResponse(npr, invokingCharacter);
							}

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
                            this.Attack(invokingCharacter.GetFullName());
                            break;
                        case "UseSkill":
							string[] dataSplit = null;
							if (NPCRS.ResponseStepData.Contains('.'))
							{
								dataSplit = NPCRS.ResponseStepData.Split('.');
							}
							else
							{
								dataSplit[0] = NPCRS.ResponseStepData;
								dataSplit[1] = null;
							}
							
							this.UseSkill(dataSplit[0], dataSplit[1]);
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
					case "attack":
						// Simply attack a target player
						this.Attack(invokingCharacter.GetFullName());
						break;
					case "wait":
                        System.Threading.Thread.Sleep(int.Parse(npccs.AdditionalData) * 1000);
                        break;
                }

				if (npccs.ResponseOptions != null)
				{
					if (npccs.ResponseOptions.Count > 0)
					{
						ProcessResponseOptions(npcc, npccs, invokingCharacter);
					}
				}

				if (npccs.NextStep != null)
				{
					string[] splitNextStep = npccs.NextStep.Split('.');
					if (splitNextStep[1] != "0")
					{
						System.Threading.Thread.Sleep(int.Parse(splitNextStep[1]) * 1000);
					}
					ProcessConversationStep(npcc, splitNextStep[0], invokingCharacter);
				}
            }
        }

        private void ProcessResponseOptions(NPCConversations npcc, NPCConversationStep npccs, SMCharacter invokingCharacter)
        {
            string responseOptions = OutputFormatterFactory.Get().Bold(this.GetFullName() + " Responses:") + OutputFormatterFactory.Get().NewLine;
			List<ShortcutToken> stl = new List<ShortcutToken>();

			foreach (NPCConversationStepResponseOptions npcccsro in npccs.ResponseOptions)
            {
                responseOptions += OutputFormatterFactory.Get().ListItem(ProcessResponseString(npcccsro.ResponseOptionText, invokingCharacter) + " (" + npcccsro.ResponseOptionShortcut + ")");
				ShortcutToken st = new ShortcutToken();
				st.ShortCutToken = npcccsro.ResponseOptionShortcut;
				stl.Add(st);
			}

            if (this.AwaitingCharacterResponses == null)
            {
                this.AwaitingCharacterResponses = new List<SMNPCAwaitingCharacterResponse>();
            }
			
            SMNPCAwaitingCharacterResponse acr = new SMNPCAwaitingCharacterResponse();
			acr.ConversationID = npcc.ConversationID;
            acr.ConversationStep = npccs.StepID;
            acr.WaitingForCharacter = invokingCharacter;
			acr.RoomID = this.RoomID;

            string nextStepAfterTimeout = null;
            int timeout = 1000;
            if (npccs.NextStep != null)
            {
                string[] getNextStep = npccs.NextStep.Split('.');
                nextStepAfterTimeout = getNextStep[0];
                timeout = int.Parse(getNextStep[1]);
            }
            
            acr.ConversationStepAfterTimeout = nextStepAfterTimeout;
            acr.UnixTimeStampTimeout = Utility.Utils.GetUnixTimeOffset(timeout);

            this.AwaitingCharacterResponses.Add(acr);

			invokingCharacter.SetAwaitingResponse(this.UserID, stl, timeout, this.RoomID);
			invokingCharacter.sendMessageToPlayer(responseOptions);
		}

		public void ProcessCharacterResponse(string responseShortCut, SMCharacter invokingCharacter)
		{
			// Get the current unix time
			int currentUnixTime = Utility.Utils.GetUnixTime();

			// Double check we're not going to get a null exception
			if (this.AwaitingCharacterResponses != null)
			{
				// Delete all responses over that time
				this.AwaitingCharacterResponses.RemoveAll(awaitingitems => awaitingitems.UnixTimeStampTimeout < currentUnixTime);

				if (this.AwaitingCharacterResponses.Count > 0)
				{
					// Get the Character Response.
					SMNPCAwaitingCharacterResponse acr = this.AwaitingCharacterResponses.FirstOrDefault(rw => rw.WaitingForCharacter.UserID == invokingCharacter.UserID);

					// Make sure we've returned something
					if (acr != null)
					{
						// Load the conversation
						NPCConversations npcc = this.NPCConversationStructures.FirstOrDefault(nc => nc.ConversationID == acr.ConversationID);
						if (npcc != null)
						{
							// Get the relevant part of the conversation to go to
							NPCConversationStep currentStep = npcc.ConversationSteps.FirstOrDefault(step => step.StepID == acr.ConversationStep);
							
							if (currentStep != null)
							{
								NPCConversationStepResponseOptions nextstep = currentStep.ResponseOptions.FirstOrDefault(ro => ro.ResponseOptionShortcut.ToLower() == responseShortCut.ToLower());
								
								if (nextstep != null)
								{
									NPCResponseOptionAction nroa = nextstep.ResponseOptionActionSteps.FirstOrDefault();

									if (nroa != null)
									{
										// Get the conversation / step to go to.
										string[] convostep = nroa.AdditionalData.Split('.');

										// check whether the conversation is the same as the original if not get the new one
										if (convostep[0] != npcc.ConversationID)
										{
											npcc = this.NPCConversationStructures.FirstOrDefault(nc => nc.ConversationID == convostep[0]);
										}

										// Remove the item from the awaiting items.
										AwaitingCharacterResponses.Remove(acr);

										// process it
										ProcessConversationStep(npcc, convostep[1], invokingCharacter);
									}
								}
							}
						}
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

	#region "NPC Structures"

	/// <summary>
	/// Response types can be of the following types:
	/// - PlayerCharacter.Enter (Faction = FactionName.Threshold, AdditionalData = Player.Known)
	/// - PlayerCharacter.Leave (Faction = FactionName.Threshold, AdditionalData = Player.Known)
	/// - PlayerCharacter.Attack
	/// - PlayerCharacter.AttacksThem
	/// - PlayerCharacter.SayNPCName (Faction = FactionName.Threshold, AdditionalData = Player.Known)
	/// - PlayerCharacter.SayKeyWord (Faction = FactionName.Threshold, AdditionalData = Player.Known)
	/// - PlayerCharacter.UseSkillOnThem (AdditionalData = the skill used)
	/// - PlayerCharacter.UseSkillOnAnotherCharacter (AdditionalData = the skill used)
	/// - PlayerCharacter.UseSkill (AdditionalData = the skill used)
	/// - PlayerCharacter.ExaminesThem
	/// - PlayerCharacter.InRoom (Faction = FactionName.Threshold, frequency should be lower on this)
	/// - NPC.Enter
	/// - NPC.Leave
	/// - NPC.ExaminesThem
	/// - NPC.Attack
	/// - Pulse (Faction = FactionName.Threshold, AdditionalData = Player.Known)
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

    /// <summary>
    /// In memory store of awaiting responses and their timeout (if any)
    /// </summary>
    public class SMNPCAwaitingCharacterResponse
    {
        public SMCharacter WaitingForCharacter { get; set; }
		public string RoomID { get; set; }
		public string ConversationID { get; set; }
		public string ConversationStep { get; set; }
        public int UnixTimeStampTimeout { get; set; }
        public string ConversationStepAfterTimeout { get; set; }
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

    #endregion
}