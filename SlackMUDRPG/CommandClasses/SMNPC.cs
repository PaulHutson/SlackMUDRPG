using Newtonsoft.Json;
using SlackMUDRPG.Utility;
using SlackMUDRPG.Utility.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    public class SMNPC : SMCharacter
	{
		[JsonProperty("NPCType")]
		public string NPCType { get; set; }

		[JsonProperty("IsGeneric")]
		public bool IsGeneric { get; set; }

		[JsonProperty("WalkingType")]
		public string WalkingType { get; set; }

		[JsonProperty("FamilyType")]
		public string FamilyType { get; set; }

		[JsonProperty("PronounSingular")]
		public string PronounSingular { get; set; }

		[JsonProperty("PronounMultiple")]
		public string PronounMultiple { get; set; }

		[JsonProperty("DestroyedOutput")]
		public string DestroyedOutput { get; set; }

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

        public void RespondToAction(string actionType, SMCharacter invokingCharacter, SMItem itemIn = null)
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
							if ((invokingCharacter == null) && (this.RoomID != "IsSpawned"))
							{
								// Get a random player (in line with the scope of the additional data)
								invokingCharacter = this.GetRoom().GetRandomCharacter(this, npr.AdditionalData);
							}

							// If the invoking character is not null
							if (invokingCharacter != null)
							{
								// Process the response
								ProcessResponse(npr, invokingCharacter, itemIn);
							}

							// Set that a response has been selected so we can drop out of the loop
							responseSelected = true;
                        }
                    }
                }
            }
        }

        private void ProcessResponse(NPCResponses npr, SMCharacter invokingCharacter, SMItem itemIn)
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
						case "ItemCheck":
							// Get the additional data
							string[] itemType = npr.AdditionalData.Split('.');

							if (itemType[0] == "Family")
							{
								if (itemIn.ItemFamily != itemType[1])
								{
									// Drop the item
									this.GetRoom().AddItem(itemIn);
									this.GetRoom().Announce(ResponseFormatterFactory.Get().Italic($"\"{this.GetFullName()}\" dropped {itemIn.SingularPronoun} {itemIn.ItemName}."));
								}
								else
								{
									ProcessConversation(NPCRS, invokingCharacter);
								}
							}

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
			bool continueToNextStep = true;
            if (npccs != null)
            {
                switch (npccs.Scope.ToLower()) {
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
                        this.GetRoom().ChatEmote(ProcessResponseString(npccs.AdditionalData, invokingCharacter), this, this);
                        break;
                    case "saytoplayer":
                        // Construct the message
                        string sayToPlayerMessage = ResponseFormatterFactory.Get().Italic(this.GetFullName() + " says:", 0) + " \"" + ProcessResponseString(npccs.AdditionalData, invokingCharacter) + "\"";

                        // Send the message
                        invokingCharacter.sendMessageToPlayer(sayToPlayerMessage);
                        break;
					case "emotetoplayer":
						// Construct the message
						string emoteToPlayerMessage = ResponseFormatterFactory.Get().Italic(this.GetFullName() + " " + ProcessResponseString(npccs.AdditionalData, invokingCharacter));

						// Send the message
						invokingCharacter.sendMessageToPlayer(emoteToPlayerMessage);
						break;
					case "attack":
						// Simply attack a target player
						this.Attack(invokingCharacter.GetFullName());
						break;
					case "giveitem":
						// give an item to the player
						string[] additionalDataSplit = npccs.AdditionalData.Split(',');
						string[] itemParts = additionalDataSplit[0].Split('.');

						// Create the item..
						if (itemParts.Count() == 2)
						{
							int numberToCreate = int.Parse(additionalDataSplit[1]);

							// Create the right number of the items.
							while (numberToCreate > 0)
							{
								// Get the item (with a new GUID)
								SMItem itemBeingGiven = SMItemFactory.Get(itemParts[0], itemParts[1]);

								// Pass it to the player
								invokingCharacter.PickUpItem("", itemBeingGiven, true);

								// Reduce the number to create
								numberToCreate--;
							}
						}
						break;
					case "addquest":
						// Load the quest
						SMQuest smq = SMQuestFactory.Get(npccs.AdditionalData);
						if (smq != null)
						{
							invokingCharacter.AddQuest(smq);
						}
						break;
					case "updatequest":
						// Load the quest
						SMQuest qtu = SMQuestFactory.Get(npccs.AdditionalData);
						if (qtu != null)
						{
							invokingCharacter.UpdateQuest(qtu);
						}
						break;
                    case "checkquestinprogress":
                        // Check the quest log isn't null
                        if (invokingCharacter.QuestLog != null)
                        {
                            if (invokingCharacter.QuestLog.Count(questcheck => (questcheck.QuestName.ToLower() == npccs.AdditionalData.ToLower()) && (questcheck.Completed)) > 0)
                            {
                                continueToNextStep = false;
                            }
                        }
                        break;
					case "checkquestcomplete":
						// Check the player has completed the quest
						if (invokingCharacter.QuestLog != null)
						{
							if (invokingCharacter.QuestLog.Count(questcheck => (questcheck.QuestName.ToLower() == npccs.AdditionalData.ToLower()) && (questcheck.Completed)) == 0)
							{
								continueToNextStep = false;
							}
						}
						break;
					case "setplayerattribute":
						// Add a response option
						string s = invokingCharacter.VariableResponse.ToLower();
						switch (npccs.AdditionalData.ToLower())
						{
							case "firstname":
								invokingCharacter.FirstName = char.ToUpper(s[0]) + s.Substring(1);
								break;
							case "lastname":
								invokingCharacter.LastName = char.ToUpper(s[0]) + s.Substring(1);
								break;
							case "sex":
								invokingCharacter.Sex = char.Parse(invokingCharacter.VariableResponse);
								break;
						}
						invokingCharacter.SaveToApplication();
						invokingCharacter.SaveToFile();
						break;
					case "setvariableresponse":
						invokingCharacter.VariableResponse = npccs.AdditionalData.ToLower();
						break;
					case "teachskill":
						// Check if the player already has the skill
						if (invokingCharacter.Skills == null)
						{
							invokingCharacter.Skills = new List<SMSkillHeld>();
						}

						// Get the skill and level to teach to
						string[] skillToTeach = npccs.AdditionalData.Split('.');

						// Check if the character already has the skill
						if (invokingCharacter.Skills.Count(skill => skill.SkillName == skillToTeach[0]) == 0)
						{
							// Create a new skill help object
							SMSkillHeld smsh = new SMSkillHeld();
							smsh.SkillName = skillToTeach[0];
							smsh.SkillLevel = int.Parse(skillToTeach[1]);

							// Finally add it to the player
							invokingCharacter.Skills.Add(smsh);

							// Save the player
							invokingCharacter.SaveToApplication();
							invokingCharacter.SaveToFile();

							// Inform the player they have learnt a new skill
							invokingCharacter.sendMessageToPlayer(ResponseFormatterFactory.Get().Italic($"You learn a new skill: {smsh.SkillName}({smsh.SkillLevel})."));
						}
						
						break;
                    case "shopitem":
                        // Get the scope
                        string scope = npccs.AdditionalData;

                        // Check the scope
                        if (scope == "room") // Items found in a room
                        {
                            // get the shop items from the room
                            List<SMRoom> smrs = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];
                            SMRoom smr = smrs.FirstOrDefault(room => room.RoomID == this.RoomID);
                            if (smr != null)
                            {
                                // Check that the room items aren't null
                                if (smr.NPCShopItems != null)
                                {
                                    // Return the list to the player
                                    invokingCharacter.sendMessageToPlayer(smr.NPCShopItems.GetInventory());
                                }
                            }
                        }
                        else
                        {
                            // TODO: variable implementation later (i.e. for NPCs carrying items to sell)
                        }
                        
                        break;
                    case "shopbuyitem":
                        // Get the variable data
                        string variableResponse = invokingCharacter.VariableResponse;
                        string[] nextSteps = npccs.AdditionalData.Split('|');

                        // Check the variable response is an int.
                        int checkOutput;
                        int.TryParse(variableResponse, out checkOutput);
                        if (checkOutput>0)
                        {
                            // Check the choice is a valid number
                            List<SMRoom> smrs2 = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];
                            SMRoom smr2 = smrs2.FirstOrDefault(room => room.RoomID == this.RoomID);
                            if (smr2 != null)
                            {
                                // Find the shop item
                                SMShopItem shopItemToBuy = smr2.NPCShopItems.ShopInventory.FirstOrDefault(i => i.ItemNumber == int.Parse(variableResponse));
                                if (shopItemToBuy != null)
                                {
                                    // Check the player has enough money for the item
                                    if (invokingCharacter.Currency.CheckCurrency(shopItemToBuy.Cost))
                                    {
                                        // Set the item id to be a generated one.
                                        shopItemToBuy.Item.ItemID = Guid.NewGuid().ToString();

                                        // Buy the item.
                                        invokingCharacter.PickUpItem("", shopItemToBuy.Item, true);

                                        // Remove the money from the player
                                        invokingCharacter.Currency.RemoveCurrency(shopItemToBuy.Cost);

                                        // Save the Character
                                        invokingCharacter.SaveToApplication();
                                        invokingCharacter.SaveToFile();

                                        // TODO: Reduce the number of items available, add the currency to the NPC, etc.
                                        // Need to think on this as it's a bit more complex (we don't save NPCs to the application
                                        // with different states as yet, so it'd involve a bit of work, although not massive).

                                        // Continue the conversation
                                        ProcessConversationStep(npcc, nextSteps[0], invokingCharacter);
                                    }
                                    else
                                    {
                                        // Play the failure conversation
                                        invokingCharacter.sendMessageToPlayer("[i]Not enough money to buy that.[/i]");
                                        ProcessConversationStep(npcc, nextSteps[1], invokingCharacter);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Play the failure conversation.
                            invokingCharacter.sendMessageToPlayer("[i]Please check the item number you've selected...[/i]");
                            ProcessConversationStep(npcc, nextSteps[1], invokingCharacter);
                        }
                        
                        break;
                    case "wait":
                        System.Threading.Thread.Sleep(int.Parse(npccs.AdditionalData) * 1000);
                        break;
                }

				if (continueToNextStep)
				{
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
        }

		/// <summary>
		/// Process the response options for an action.
		/// </summary>
		/// <param name="npcc">The conversation that is taking place</param>
		/// <param name="npccs">The step in the conversation</param>
		/// <param name="invokingCharacter">The invoking character</param>
        private void ProcessResponseOptions(NPCConversations npcc, NPCConversationStep npccs, SMCharacter invokingCharacter)
        {
			// Set the response option variables up
            string responseOptions = ResponseFormatterFactory.Get().Bold(this.GetFullName() + " Responses:") + ResponseFormatterFactory.Get().NewLine;
			bool thereIsAnOption = false;
			List<ShortcutToken> stl = new List<ShortcutToken>();

			// Loop around the options building up the various parts
			foreach (NPCConversationStepResponseOptions npcccsro in npccs.ResponseOptions)
            {
				// Variable to hold whether the response can be added - it may not be allowed due to some prereqs..
				bool canAddResponse = true;

				// Check if there is a prereq...
				if (npcccsro.PreRequisites != null)
				{
					// get the quests for use later
					List<SMQuestStatus> smqs = new List<SMQuestStatus>();
					if (invokingCharacter.QuestLog != null)
					{
						smqs = invokingCharacter.QuestLog;
					}

					// .. if there is, loop around them.
					foreach (NPCConversationStepResponseOptionsPreRequisites prereq in npcccsro.PreRequisites)
					{
						switch (prereq.Type) {
							case "HasDoneQuest":
								if (smqs.Count(quest => (quest.QuestName == prereq.AdditionalData) && (quest.Completed)) == 0)
								{
									canAddResponse = false;
								}
								break;
							case "InProgressQuest":
								if (smqs.Count(quest => (quest.QuestName == prereq.AdditionalData) && (!quest.Completed)) == 0)
								{
									canAddResponse = false;
								}
								break;
							case "HasNotDoneQuest":
								if (smqs.Count(quest => (quest.QuestName == prereq.AdditionalData)) != 0)
								{
									canAddResponse = false;
								}
								break;
							case "IsNotInProgressQuest":
								if (smqs.Count(quest => (quest.QuestName == prereq.AdditionalData) && (!quest.Completed)) != 0)
								{
									canAddResponse = false;
								}
								break;
						}
					}
				}

				// Check that the response can be added
				if (canAddResponse)
				{
					responseOptions += ResponseFormatterFactory.Get().ListItem(ProcessResponseString(npcccsro.ResponseOptionText, invokingCharacter) + " (" + npcccsro.ResponseOptionShortcut + ")");
					ShortcutToken st = new ShortcutToken();
					st.ShortCutToken = npcccsro.ResponseOptionShortcut;
					stl.Add(st);
					thereIsAnOption = true;
				}
			}

			// If an option has been set..
			if (thereIsAnOption)
			{
                AddResponse(npcc, npccs, invokingCharacter, stl, responseOptions);
			}
		}

        public void AddResponse(NPCConversations npcc, NPCConversationStep npccs, SMCharacter invokingCharacter, List<ShortcutToken> stl, string responseOptions)
        {
            // Set up a list to hold them in the character (if there isn't one already)
            if (this.AwaitingCharacterResponses == null)
            {
                this.AwaitingCharacterResponses = new List<SMNPCAwaitingCharacterResponse>();
            }

            // Create the awaiting response token.
            SMNPCAwaitingCharacterResponse acr = new SMNPCAwaitingCharacterResponse();
            acr.ConversationID = npcc.ConversationID;
            acr.ConversationStep = npccs.StepID;
            acr.WaitingForCharacter = invokingCharacter;
            acr.RoomID = this.RoomID;

            // Work out the timeout conversation if there is one.
            string nextStepAfterTimeout = null;
            int timeout = 10000;
            if (npccs.NextStep != null)
            {
                string[] getNextStep = npccs.NextStep.Split('.');
                nextStepAfterTimeout = getNextStep[0];
                timeout = int.Parse(getNextStep[1]);
            }

            // Set the conversation timeout
            acr.ConversationStepAfterTimeout = nextStepAfterTimeout;
            acr.UnixTimeStampTimeout = Utility.Utils.GetUnixTimeOffset(timeout);

            // Add the item to the character, and send a message to the player regarding the available responses.
            this.AwaitingCharacterResponses.Add(acr);
            invokingCharacter.SetAwaitingResponse(this.UserID, stl, timeout, this.RoomID);

            if ((responseOptions != null) && (!responseOptions.Contains("{variable}")))
            {
                invokingCharacter.sendMessageToPlayer(responseOptions);
            }
        }

		public void ProcessCharacterResponse(string responseShortCut, SMCharacter invokingCharacter)
		{
			// Get the current unix time
			int currentUnixTime = Utility.Utils.GetUnixTime();

			// Double check we're not going to get a null exception
			if (this.AwaitingCharacterResponses != null)
			{
				// Delete all responses over that time
				// this.AwaitingCharacterResponses.RemoveAll(awaitingitems => awaitingitems.UnixTimeStampTimeout < currentUnixTime);

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
							
							if ((currentStep != null) && (currentStep.ResponseOptions != null))
							{
								NPCConversationStepResponseOptions nextstep = currentStep.ResponseOptions.FirstOrDefault(ro => ro.ResponseOptionShortcut.ToLower() == responseShortCut.ToLower());
								
								// TODO - Update this location with the character variable info.

								if (nextstep == null)
								{
									nextstep = currentStep.ResponseOptions.FirstOrDefault(ro => ro.ResponseOptionShortcut.ToLower() == "{variable}");
									invokingCharacter.VariableResponse = responseShortCut;
								}

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

										// Remove it from the character too, it's processed now so we don't need it any more!
										invokingCharacter.NPCsWaitingForResponses.RemoveAll(ar => ar.NPCID == this.UserID);
										invokingCharacter.SaveToApplication();
										invokingCharacter.SaveToFile();
										
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
			responseString = responseString.Replace("{response}", invokingCharacter.VariableResponse);

			return responseString;
		}

		/// <summary>
		/// Saves the character to application memory.
		/// </summary>
		public override void SaveToApplication()
		{
			List<SMNPC> smcs = (List<SMNPC>)HttpContext.Current.Application["SMNPCs"];

			SMNPC characterInMem = smcs.FirstOrDefault(smc => smc.UserID == this.UserID);

			if (characterInMem != null)
			{
				smcs.Remove(characterInMem);
			}

			smcs.Add(this);
			HttpContext.Current.Application["SMNPCs"] = smcs;
		}

		/// <summary>
		/// Saves the character to application memory.
		/// </summary>
		public override SMItem ProduceCorpse()
		{
			// Create the corpse
			SMItem corpse = SMItemFactory.Get("Misc", "Corpse");
			corpse.ItemName = this.GetFullName() + " Corpse";

			// If it's an animal or some such, create the destroyed output elements.
			if (this.DestroyedOutput != null)
			{
				corpse.DestroyedOutput = this.DestroyedOutput;
			}

			// Previous item family type
			corpse.PreviousItemFamily = this.FamilyType;

			// Create the "held items" list ready for transferring items to the corpse.
			corpse.HeldItems = new List<SMItem>();
			
			if (this.Slots != null)
			{
				foreach (SMSlot sms in this.Slots)
				{
					if (!sms.isEmpty())
					{
						corpse.HeldItems.Add(sms.EquippedItem);
					}
				}
			}
			
			// TODO Add clothing / armour items to the held items list ready for looting.

			return corpse;
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
	/// - PlayerCharacter.GivesItemToThem
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

		[JsonProperty("PreRequisites")]
		public List<NPCConversationStepResponseOptionsPreRequisites> PreRequisites { get; set; }

		[JsonProperty("ResponseOptionActionSteps")]
        public List<NPCResponseOptionAction> ResponseOptionActionSteps { get; set; }
    }

	/// <summary>
	/// Prerequisites for conversations, sometimes you may not want a response to show up.
	/// 
	/// Types include:
	/// - HasDoneQuest, additionaldata = quest name
	/// - InProgressQuest, additionaldata = quest name
	/// - HasNotDoneQuest, additionaldata = quest name
	/// - IsNotInProgressQuest, additionaldata = quest name
	/// </summary>
	public class NPCConversationStepResponseOptionsPreRequisites
	{
		[JsonProperty("Type")]
		public string Type { get; set; }

		[JsonProperty("AdditionalData")]
		public string AdditionalData { get; set; }
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
	/// - CheckItem
	///     AdditionalData: Item.Type,Number
	/// - TeachSkill
	///		AdditionalData: SkillName.Level
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

	public static class NPCHelper
	{
		public static SMNPC GetNewNPC(string NPCType, bool unique = false)
		{
			SMNPC newNPC = new SMNPC();

			// ... if they're not, spawn them into the room.
			string newNPCPath = FilePathSystem.GetFilePath("NPCs", NPCType);

			// If that NPC Exits
			if (File.Exists(newNPCPath))
			{
				// Read the data in
				using (StreamReader r = new StreamReader(newNPCPath))
				{
					// Read the data in 
					string json = r.ReadToEnd();

					// Deserialise the JSON to an object and add it to the list of NPCS in memory
					newNPC = JsonConvert.DeserializeObject<SMNPC>(json);

					if (!unique)
					{
						newNPC.UserID = Guid.NewGuid().ToString();
					}
				}
			}

			return newNPC;
		}

		public static void StartAnNPCReactionCheck(SMNPC npc, string actionType, SMCharacter invokingCharacter, SMItem itemIn = null)
		{
			HttpContext ctx = HttpContext.Current;

			Thread npcReactionThread = new Thread(new ThreadStart(() =>
			{
				HttpContext.Current = ctx;
				npc.RespondToAction("PlayerCharacter.GivesItemToThem", invokingCharacter, itemIn);
			}));

			npcReactionThread.Start();
		}
	}

    #endregion
}