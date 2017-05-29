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

        [JsonProperty("RaceID")]
        public string RaceID { get; set; }

        [JsonProperty("IgnoreActions")]
        public List<string> IgnoreActions { get; set; } = new List<string>();

        // Not a property from JSON, but is returns a value based on RaceID
        public NPCRace NPCRace
        {
            get
            {
				if (this.RaceID != null)
				{
					return new SlackMud().GetNPCRace(this.RaceID);
				}

				return null;
            }
        }

        // Used for in memory storing of responses requested from a character
        public List<SMNPCAwaitingCharacterResponse> AwaitingCharacterResponses { get; set; }

        /// <summary>
        /// Responds to an action based on the action identifier passed in.
        /// </summary>
        /// <param name="actionType">A string representing the unique action you wish the NPC to respond to.</param>
        /// <param name="invokingCharacter">The character who invoked this action for performing actions against/to</param>
        /// <param name="itemIn"></param>
        /// <returns>Returns a boolean value representing whether or not the NPC handled the action or not.</returns>
        public bool RespondToAction(string actionType, SMCharacter invokingCharacter, SMItem itemIn = null)
        {
			// check to see if this action is ignored
			if (this.IgnoreActions.Count(a => a.ToLower() == actionType.ToLower()) > 0)
			{
				return false;
			}

            // Get a list of characters that respond to this action type in the room
            List<NPCResponses> listToChooseFrom = NPCResponses.FindAll(npcr => npcr.ResponseType.ToLower() == actionType.ToLower());

            // if the NPC doesn't define responds to the event we'll look for defaults on the race.
            if (listToChooseFrom.Count == 0)
            {
                listToChooseFrom = this.NPCRace?.NPCResponses.FindAll(npcr => npcr.ResponseType.ToLower() == actionType.ToLower()) ?? listToChooseFrom;
            }

            // Cull any prerequisites.
            if ((listToChooseFrom != null) && (invokingCharacter != null))
            {
                // .. if there is, loop around them.
                foreach (NPCResponses npr in listToChooseFrom)
                {
                    if (npr.PreRequisites != null)
                    {
						// Check the response pre requisites
						bool canUseResponse = this.CheckResponcePreRequisites(npr.PreRequisites, invokingCharacter);

                        // Remove any items from the list that can't be used.
                        if (!canUseResponse)
                        {
                            npr.RemoveItemFromResponses = true;
                        }
                    }
                }

                listToChooseFrom.RemoveAll(resp => resp.RemoveItemFromResponses == true);
            }

			// If there are some responses for this character for the actionType
			if ((listToChooseFrom != null) && (listToChooseFrom.Count > 0))
			{
				// If there is more than one of the item randomise the list
				if (listToChooseFrom.Count > 1) {
					listToChooseFrom = listToChooseFrom.OrderBy(item => new Random().Next()).ToList();
				}

				// Loop around until a response is selected
				foreach (NPCResponses npr in listToChooseFrom)
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
						return true;
					}
				}
			}
			else
			{
				if (itemIn != null)
				{
					this.GetRoom().AddItem(itemIn);
					this.GetRoom().Announce("[i]" + this.GetFullName() + " drops " + itemIn.SingularPronoun + " " + itemIn.ItemName + "[/i]");
				}
			}

			return false;
        }

        private void ProcessResponse(NPCResponses npr, SMCharacter invokingCharacter, SMItem itemIn)
        {
            // Process each of the response steps
            foreach (NPCResponseStep NPCRS in npr.ResponseSteps)
            {
                // Get the invoking character
                invokingCharacter = new SlackMud().GetCharacter(invokingCharacter.UserID);

                // Check the character is still in the same room
                if (invokingCharacter != null)
                {
                    if (invokingCharacter.RoomID == this.RoomID)
                    {
                        switch (NPCRS.ResponseStepType)
                        {
                            case "Conversation":
                                ProcessConversation(NPCRS, invokingCharacter);
                                break;
                            case "LeaveDeSpawn":
                                // Announce that they're leaving...
                                if (this.IsGeneric)
                                {
                                    this.GetRoom().Announce("[i]" + this.PronounSingular.ToUpper() + " " + this.GetFullName() + " walks out.[/i]", this, true);
                                }
                                else
                                {
                                    this.GetRoom().Announce("[i]" + this.GetFullName() + " walks out.[/i]", this, true);
                                }

                                // .. remove them from the world.
                                List<SMNPC> npcListToRemoveFrom = (List<SMNPC>)HttpContext.Current.Application["SMNPCs"];
                                npcListToRemoveFrom.Remove(this);
                                HttpContext.Current.Application["SMNPCs"] = npcListToRemoveFrom;

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
                                else if (itemType[0] == "ItemName")
                                {
                                    string[] itemName = itemType[1].Split(',');

                                    if (itemIn.ItemName != itemName[0])
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
        }

        private void ProcessConversation(NPCResponseStep NPCRS, SMCharacter invokingCharacter)
        {
            // First get the conversation
            string[] rsd = NPCRS.ResponseStepData.Split('.');

            // Get the conversation
            NPCConversations npcc = this.NPCConversationStructures.FirstOrDefault(constructure => constructure.ConversationID == rsd[0]);
            if (npcc == null)
            {
                // check race for conversation
                npcc = this.NPCRace?.NPCConversationStructures.FirstOrDefault(cs => cs.ConversationID == rsd[0]);
            }

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
            if ((npccs != null) && (this.RoomID == invokingCharacter.RoomID))
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
						string emoteToPlayerMessage = ResponseFormatterFactory.Get().Italic(this.GetFullName() + " " + ProcessResponseString(npccs.AdditionalData, invokingCharacter),0);

						// Send the message
						invokingCharacter.sendMessageToPlayer(emoteToPlayerMessage);
						break;
                    case "generalemote":
                        // Construct the message
                        string emoteSelfToPlayerMessage = ResponseFormatterFactory.Get().Italic(ProcessResponseString(npccs.AdditionalData, invokingCharacter), 0);

                        // Send the message
                        invokingCharacter.sendMessageToPlayer(emoteSelfToPlayerMessage);
                        break;
                    case "attack":
						// Simply attack a target player
						this.Attack(invokingCharacter.GetFullName());
						break;

					case "takeitems":
						// take items from the player
						string[] items = npccs.AdditionalData.Split(',');

						foreach (string item in items)
						{
							string[] itemDetails = item.Split('.');

							int qtyToTake = Int32.Parse(itemDetails[1]);

							while (qtyToTake > 0)
							{
								invokingCharacter.RemoveItem(itemDetails[0], false);
								qtyToTake--;
							}
						}

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
								invokingCharacter.ReceiveItem(itemBeingGiven);

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
                    case "checkplayername":
                        // Gather the info
                        string[] nextStepsCheckPlayerName = npccs.AdditionalData.Split('|');
                        string name = invokingCharacter.GetFullName();
                        bool nameCanBeUsed = new SMAccountHelper().CheckCharNameCanBeUsed(name);
                        
                        if (nameCanBeUsed)
                        {
                            // Add the name to the list so no one else can use it
                            new SMAccountHelper().AddNameToList(name, invokingCharacter.UserID, invokingCharacter);

                            // Play the succcess conversation step
                            ProcessConversationStep(npcc, nextStepsCheckPlayerName[0], invokingCharacter);
                        }
                        else
                        {
                            // Reset the player name
                            invokingCharacter.FirstName = "New";
                            invokingCharacter.LastName = "Arrival";
                            invokingCharacter.SaveToApplication();
                            invokingCharacter.SaveToFile();

                            // Play the failure conversaton steps (i.e. go back through the same thing again).
                            ProcessConversationStep(npcc, nextStepsCheckPlayerName[1], invokingCharacter);
                        }
                        
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
					case "teachrecipe":
						invokingCharacter.LearnRecipe(npccs.AdditionalData);
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
										// Flag indicating if the item should be charged for
										bool chargeForItem = true;

										// Index of the nextStep to take (0 = success, 1 = failed to buy)
										Int32 nextStep = 0;

										// Process the transaction, giveing the character the item or learning something
										// as defined by the ShopItems type
										switch (shopItemToBuy.ItemType)
										{
											case "recipe":
												// Try and learn the recipe, only charge if the character can learn it
												if (!invokingCharacter.LearnRecipe(shopItemToBuy.AdditionalData))
												{
													chargeForItem = false;
													nextStep = 1;
												}
												break;
											default:
												// Set the item id to be a generated one.
												shopItemToBuy.Item.ItemID = Guid.NewGuid().ToString();

												// Buy the item.
												invokingCharacter.ReceiveItem(shopItemToBuy.Item);
												break;
										}

                                        // Remove the money from the player
										if (chargeForItem)
										{
											invokingCharacter.Currency.RemoveCurrency(shopItemToBuy.Cost);
										}

                                        // Save the Character
                                        invokingCharacter.SaveToApplication();
                                        invokingCharacter.SaveToFile();

                                        // TODO: Reduce the number of items available, add the currency to the NPC, etc.
                                        // Need to think on this as it's a bit more complex (we don't save NPCs to the application
                                        // with different states as yet, so it'd involve a bit of work, although not massive).

                                        // Continue the conversation
                                        ProcessConversationStep(npcc, nextSteps[nextStep], invokingCharacter);
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
                    case "checkplayerinroom":
                        // Get the name to check from the variable response
                        string charnameToCheck = invokingCharacter.VariableResponse;

                        // Get the next steps
                        string[] nextStepsCheckPlayerInRoom = npccs.AdditionalData.Split('|');

                        // Get the character
                        SMCharacter checkcharacter = invokingCharacter.GetRoom().GetPeople().FirstOrDefault(pn => pn.GetFullName().ToLower() == charnameToCheck.ToLower());
                        
                        if (checkcharacter != null) // found the character
                        {
                            ProcessConversationStep(npcc, nextStepsCheckPlayerInRoom[0], invokingCharacter);
                        }
                        else // Character not found
                        {
                            ProcessConversationStep(npcc, nextStepsCheckPlayerInRoom[0], invokingCharacter);
                        }

                        break;
                    case "partyinvite":
                        // Leave any parties the player is currently in.
                        if (invokingCharacter.PartyReference != null)
                        {
                            SMParty smp = SMPartyHelper.GetParty(invokingCharacter.PartyReference.PartyID);
                            smp.LeaveParty(invokingCharacter);
                        }

                        // Invite the named character to a party (secretly, suppressing the message).
                        new SMParty().InviteToParty(invokingCharacter, invokingCharacter.VariableResponse, true);

                        break;
                    case "acceptpartyinvite":
                        // Accept any open invites, suppressing the messages
                        invokingCharacter.AcceptPartyInvite(true);

                        break;
                    case "movecharacterparty":
                        // Move a whole party from one location to another
                        // Get the location information and other information
                        // Get the next steps
                        string[] additionalInformationMoveCharacterParty = npccs.AdditionalData.Split('|');

                        // Find the party
                        SMPartyHelper.GetParty(invokingCharacter.PartyReference.PartyID).MoveAllPartyMembersToLocation(additionalInformationMoveCharacterParty[0], additionalInformationMoveCharacterParty[1], additionalInformationMoveCharacterParty[2], true);
                        
                        break;
                    case "startconversation":
                        // Find the name of the person we want to start a conversation with
                        // and the conversation we're going to go to.
                        string[] nextStepsStartConversation = npccs.AdditionalData.Split('|');

                        // Find the character from the name.
                        List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
                        SMCharacter smcStartConversation = smcs.FirstOrDefault(smc => smc.GetFullName().ToLower() == invokingCharacter.VariableResponse.ToLower());

                        // Start the conversation
                        ProcessConversationStep(npcc, nextStepsStartConversation[1], smcStartConversation);

                        break;
                    case "checkskilllevel":
                        // Find the name of the person we want to start a conversation with
                        // and the conversation we're going to go to.
                        string[] nextStepsCheckSkillLevel = npccs.AdditionalData.Split('|');
                        string nextStepCheckSkillLevel = nextStepsCheckSkillLevel[2];
                        
                        // Get the character skill level for the specificed skill.
                        // Check if the player already has the skill
                        if (invokingCharacter.Skills == null)
                        {
                            nextStepCheckSkillLevel = nextStepsCheckSkillLevel[3];
                        }
                        else
                        {
                            if (invokingCharacter.Skills.Count(skill => ((skill.SkillName == nextStepsCheckSkillLevel[0]) && (skill.SkillLevel >= int.Parse(nextStepsCheckSkillLevel[1])))) == 0)
                            {
                                nextStepCheckSkillLevel = nextStepsCheckSkillLevel[3];
                            }
                        }

                        // Process the conversation.
                        ProcessConversationStep(npcc, nextStepCheckSkillLevel, invokingCharacter);

                        break;
                    case "increasefactionlevel":
                        // find which faction you're increasing and by how much
                        string[] nextStepsIncreaseFactionLevel = npccs.AdditionalData.Split('|');

                        // Increase the character faction.
                        SMFactionHelper.IncreaseFactionLevel(invokingCharacter, nextStepsIncreaseFactionLevel[0], int.Parse(nextStepsIncreaseFactionLevel[1]));
                        break;
                    case "decreasefactionlevel":
                        // find which faction you're decreasing and by how much
                        string[] nextStepsDecreaseFactionLevel = npccs.AdditionalData.Split('|');

                        // Decrease the character faction.
                        SMFactionHelper.DecreaseFactionLevel(invokingCharacter, nextStepsDecreaseFactionLevel[0], int.Parse(nextStepsDecreaseFactionLevel[1]));
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
            string responseOptions = ResponseFormatterFactory.Get().NewLine + ResponseFormatterFactory.Get().Bold(this.GetFullName() + " Responses:",0);
			bool thereIsAnOption = false;
			List<ShortcutToken> stl = new List<ShortcutToken>();

			// Loop around the options building up the various parts
			foreach (NPCConversationStepResponseOptions npcccsro in npccs.ResponseOptions)
            {
				// Variable to hold whether the response can be added - it may not be allowed due to some prereqs..
				bool canAddResponse = true;

				// Check if there is a prereq and if there is the pre requisites are met
				if (npcccsro.PreRequisites != null)
				{
					canAddResponse = this.CheckResponcePreRequisites(npcccsro.PreRequisites, invokingCharacter);
				}

				// Check that the response can be added
				if (canAddResponse)
				{
					responseOptions += ResponseFormatterFactory.Get().ListItem(ProcessResponseString(npcccsro.ResponseOptionText, invokingCharacter) + " [" + npcccsro.ResponseOptionShortcut + "]",0);
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
			// NOTE: Perhaps extract this into a text helper somewhere, or expand on it with Regex replacement
			// from a map/data value or use a template engine like Mustache? -- bbuck
			string responseString = responseStringToProcess;
			responseString = responseString.Replace("{playercharacter}", invokingCharacter.GetFullName());
			responseString = responseString.Replace("{response}", invokingCharacter.VariableResponse);
			responseString = responseString.Replace("{hisher}", GetHisHerPronoun());

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
			corpse.PluralName = this.GetFullName() + " Corpses";

			double sizeWeight = this.Attributes.MaxHitPoints / 2;
			corpse.ItemWeight = (int)Math.Ceiling(sizeWeight);
			corpse.ItemSize = (int)Math.Ceiling(sizeWeight);


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

        /// <summary>
        /// This is overiding the save to file function in the character file to stop NPCs arriving in the char folder.
        /// </summary>
        public override void SaveToFile()
        {
            // Do nothing, we don't want to save the file out for NPCs.
        }

		/// <summary>
		/// Checks if the pre requisite conditions for a given list of conversation responce steps are met or not.
		/// </summary>
		/// <param name="Prereqs">A list of conversation response pre requisites.</param>
		/// <param name="invokingCharacter">The character invoking the conversation step.</param>
		/// <returns>True if all pre requisites are met else false.</returns>
		private bool CheckResponcePreRequisites(List<NPCConversationStepResponseOptionsPreRequisites> Prereqs, SMCharacter invokingCharacter)
		{
			// Get the invoking characters quest log
			List<SMQuestStatus> questLog = new List<SMQuestStatus>();

			if (invokingCharacter.QuestLog != null)
			{
				questLog = invokingCharacter.QuestLog;
			}

			// Set a flag it indicate if the prereqs are met
			bool prereqsMet = true;

			// Loop around the prereqs and check if they are met
			foreach (NPCConversationStepResponseOptionsPreRequisites prereq in Prereqs)
			{
				switch (prereq.Type)
				{
					case "HasDoneQuest":
						if (questLog.Count(q => q.QuestName == prereq.AdditionalData && q.Completed) == 0)
						{
							prereqsMet = false;
						}
						break;

					case "InProgressQuest":
						if (questLog.Count(q => q.QuestName == prereq.AdditionalData && !q.Completed) == 0)
						{
							prereqsMet = false;
						}
						break;

					case "InProgressQuestStep":
						string[] data = prereq.AdditionalData.Split('.');
						SMQuestStatus quest = questLog.FirstOrDefault(q => q.QuestName == data[0]);

						// Quest not started || quest complete || quest not on correct step
						if (quest == null || quest.Completed || quest.QuestStep != data[1])
						{
							prereqsMet = false;
						}
						break;

					case "HasNotDoneQuest":
						if (questLog.Count(q => (q.QuestName == prereq.AdditionalData)) != 0)
						{
							prereqsMet = false;
						}
						break;

					case "IsNotInProgressQuest":
						if (questLog.Count(q => (q.QuestName == prereq.AdditionalData) && (!q.Completed)) != 0)
						{
							prereqsMet = false;
						}
						break;

					case "HasItem":
						string[] additionalDataSplit = prereq.AdditionalData.Split('.');

						if (invokingCharacter.CountOwnedItems(additionalDataSplit[0]) < Int32.Parse(additionalDataSplit[1]))
						{
							prereqsMet = false;
						}
						break;
				}
			}

			return prereqsMet;
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

        [JsonProperty("PreRequisites")]
        public List<NPCConversationStepResponseOptionsPreRequisites> PreRequisites { get; set; }

        // Used for when we want to mark items for culling from a collection (like when a prereq fails).
        public bool RemoveItemFromResponses { get; set; }
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

    /// <summary>
    /// NPCRace is a class that wraps some basic general responses (and potentially
    /// other things) that are shared across NPCs of the same "Race," such as 'Human'
    /// or 'Animal' or 'Rat,' etc...
    /// </summary>
    public class NPCRace {
		[JsonProperty("Name")]
		public string Name { get; set; } = "Unnamed Race";

		[JsonProperty("Description")]
		public string Description { get; set; } = "No description provided.";

		[JsonProperty("NPCResponses")]
		public List<NPCResponses> NPCResponses { get; set; } = new List<NPCResponses>();

		[JsonProperty("NPCConversationStructures")]
		public List<NPCConversations> NPCConversationStructures { get; set; } = new List<NPCConversations>();
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