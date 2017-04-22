using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;
using SlackMUDRPG.Utility.Formatters;
using System.Threading;

namespace SlackMUDRPG.CommandClasses
{
    public class SMRoom
    {
        [JsonProperty("RoomID")]
        public string RoomID { get; set; }

        [JsonProperty("RoomLocationX")]
        public int RoomLocationX { get; set; }

        [JsonProperty("RoomLocationY")]
        public int RoomLocationY { get; set; }

        [JsonProperty("RoomLocationZ")]
        public int RoomLocationZ { get; set; }

        [JsonProperty("Outside")]
        public bool Outside { get; set; }

        [JsonProperty("Instanced")]
        public bool Instanced { get; set; }

        [JsonProperty("SafeZone")]
        public bool SafeZone { get; set; }

        [JsonProperty("SafeZoneCharAttributes")]
        public List<SMRoomSafeCharacterAttributes> SafeZoneCharAttributes { get; set; }

        [JsonProperty("InstanceReloadLocation")]
        public string InstanceReloadLocation { get; set; }

        [JsonProperty("RoomDescription")]
        public string RoomDescription { get; set; }

        [JsonProperty("RoomNewbieTips")]
        public string RoomNewbieTips { get; set; }

        [JsonProperty("RoomExits")]
        public List<SMExit> RoomExits { get; set; }

        [JsonProperty("RoomItems")]
        public List<SMItem> RoomItems { get; set; }

        [JsonProperty("NPCSpawns")]
        public List<SMSpawn> NPCSpawns { get; set; }

        [JsonProperty("NPCShopItems")]
        public SMShop NPCShopItems { get; set; }

		/// <summary>
		/// Holds the class instance of the response formater.
		/// </summary>
		private ResponseFormatter Formatter = null;

		/// <summary>
		/// Class constructor
		/// </summary>
		public SMRoom()
		{
			this.Formatter = ResponseFormatterFactory.Get();
		}

		#region "General Room Function"

		/// <summary>
		/// Adds an item to the room, so any user can collect it.
		/// </summary>
		/// <param name="item">Item.</param>
		public void AddItem(SMItem item)
		{
			if (this.RoomItems == null)
			{
				this.RoomItems = new List<SMItem>();
			}

			this.RoomItems.Add(item);
			this.SaveToApplication();
		}

		/// <summary>
		/// Removes an item from the room, recursively looks through containers for the item if required.
		/// </summary>
		/// <param name="item">SMItem to remove.</param>
		public void RemoveItem(SMItem item)
		{
			if (this.RoomItems != null)
			{
				SMItemHelper.RemoveItemFromList(this.RoomItems, item.ItemID);
			}

			this.SaveToApplication();
		}

		/// <summary>
		/// Saves the room to file.
		/// </summary>
		public void SaveToFile()
		{
			string path = FilePathSystem.GetFilePath("Location", "Loc" + this.RoomID);
			string roomJSON = JsonConvert.SerializeObject(this, Formatting.Indented);

			using (StreamWriter w = new StreamWriter(path))
			{
				w.WriteLine(roomJSON);
			}
		}

		/// <summary>
		/// Saves the room to application memory.
		/// </summary>
		public void SaveToApplication()
		{
			List<SMRoom> smrs = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];

			SMRoom roomInMem = smrs.FirstOrDefault(smr => smr.RoomID == this.RoomID);

			if (roomInMem != null)
			{
				smrs.Remove(roomInMem);
			}

			smrs.Add(this);
			HttpContext.Current.Application["SMRooms"] = smrs;
		}

		/// <summary>
		/// Gets the room exit details
		/// </summary>
		public string GetExitDetails()
		{
			string returnString = "";

			if (this.RoomExits.Count == 0)
			{
				returnString = this.Formatter.ListItem("No Exits are found from this room...");
			}
			else
			{
				returnString += this.Formatter.Bold("Room Exits:");

				string[] exits = new string[this.RoomExits.Count];

				for (int i = 0; i < this.RoomExits.Count; i++)
				{
					exits[i] = $"{this.RoomExits[i].Description} ({this.RoomExits[i].Shortcut})";
				}

				returnString += this.Formatter.ListItem(String.Join(", ", exits));
			}

			return returnString;
		}

		public List<SMCharacter> GetPeople()
		{
			// Return element.
			List<SMCharacter> charsInLocation = new List<SMCharacter>();

			// Search through logged in users to see which are in this location
			List<SMCharacter> smcs = new List<SMCharacter>();
			smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];

			// Check if the character already exists or not.
			if (smcs != null)
			{
				if (smcs.Count(smc => smc.RoomID == this.RoomID) > 0)
				{
					charsInLocation = smcs.FindAll(s => s.RoomID == this.RoomID);
				}
			}

			return charsInLocation;
		}

        public List<SMNPC> GetNPCs()
        {
            // Return element.
            List<SMNPC> npcsInLocation = new List<SMNPC>();

            // Search through logged in users to see which are in this location
            List<SMNPC> lNPCs = new List<SMNPC>();
            lNPCs = (List<SMNPC>)HttpContext.Current.Application["SMNPCs"];

            // Get the NPCs.
            if (lNPCs != null)
            {
                if (lNPCs.Count(smc => smc.RoomID == this.RoomID) > 0)
                {
                    npcsInLocation = lNPCs.FindAll(s => s.RoomID == this.RoomID);
                }
            }

            return npcsInLocation;
        }

        public List<SMCharacter> GetAllPeople()
        {
            List<SMCharacter> lsmc = this.GetPeople();
            List<SMNPC> lnpcs = this.GetNPCs();

            if (lnpcs != null)
            {
                foreach(SMNPC npc in lnpcs)
                {
                    lsmc.Add(npc);
                }
            }

            return lsmc;
        }

		/// <summary>
		/// Randomly select a single character from within a scope
		/// </summary>
		/// <param name="scope">PlayerCharacters, NPCs or AllPeople</param>
		/// <returns></returns>
		public SMCharacter GetRandomCharacter(SMCharacter ignoreChar, string scope = null)
		{
			switch (scope)
			{
				case "PlayerCharacters":
					List<SMCharacter> pc = this.GetPeople();
					pc.Remove(ignoreChar);
					pc.Shuffle();
					return pc.FirstOrDefault();
				case "NPCs":
					List<SMCharacter> npcs = new List<SMCharacter>();
					List<SMNPC> npcl = this.GetNPCs();
					if (npcl != null)
					{
						foreach (SMNPC npc in npcl)
						{
							npcs.Add(npc);
						}
					}
					npcs.Remove(ignoreChar);
					npcs.Shuffle();
					return npcs.FirstOrDefault();
				default:
					List<SMCharacter> ap = GetAllPeople();
					ap.Remove(ignoreChar);
					ap.Shuffle();
					return ap.FirstOrDefault();
			}
		}

        /// <summary>
        /// Gets a list of all the people in the room.
        /// </summary>
        public string GetPeopleDetails(SMCharacter smc = null)
		{
			string returnString = this.Formatter.Bold("People:");

			// Get the people within the location
			List<SMCharacter> smcs = this.GetPeople();

			// Check if the character already exists or not.
			if (smcs != null)
			{
				string[] people = new string[smcs.Count];

				for (int i = 0; i < smcs.Count; i++)
				{
					if (smcs[i].UserID == smc.UserID)
					{
						people[i] = "You";
					}
					//else
					//{
					//	people[i] = smcs[i].GetFullName();
					//}
				}

				returnString += this.Formatter.ListItem(String.Join(", ", people));
			}
			else
			{
				returnString += this.Formatter.ListItem("There's noone here.");
			}

			return returnString;
		}

        public string GetNPCDetails()
        {
            string returnString = "";

            List<SMNPC> SMNPCs = this.GetNPCs();

            // Check if the character already exists or not.
            if (SMNPCs != null && SMNPCs.Count > 0)
            {
				string[] smnpcs = new string[SMNPCs.Count];

				for (int i = 0; i < SMNPCs.Count; i++)
				{
					smnpcs[i] = SMNPCs[i].GetFullName();
				}

				returnString += this.Formatter.ListItem(String.Join(", ", smnpcs));
            }

            return returnString;
        }

        /// <summary>
		/// Gets a list of all the items in the room.
		/// </summary>
		public string GetItemDetails()
        {
            string returnString = this.Formatter.Bold("Objects:");

            // Check if the character already exists or not.
            if ((this.RoomItems != null) && (this.RoomItems.Count > 0))
            {
				List<ItemCountObject> items = SMItemUtils.GetItemCountList(this.RoomItems);

				foreach (ItemCountObject item in items)
				{
					string itemDetails = $"{item.Count} x ";

					if (item.Count > 1)
					{
						itemDetails += item.PluralName;
					}
					else
					{
						itemDetails += item.SingularName;
					}

					returnString += this.Formatter.ListItem(itemDetails);
				}
            }
            else
            {
                returnString += this.Formatter.ListItem("Nothing");
            }

            return returnString;
        }

        /// <summary>
        /// Internal Method to create a room decription, created as it's going to be used over and over...
        /// </summary>
        /// <param name="smr">An SMRoom</param>
        /// <returns>String including a full location string</returns>
        public string GetLocationInformation(SMCharacter smc = null)
		{
			// Construct the room string.
			string returnString = "";

			if (smc != null)
			{
				if ((!smc.NewbieTipsDisabled) && (this.RoomNewbieTips != null) && (this.RoomNewbieTips != ""))
				{
					returnString += this.Formatter.Bold("~~~~~ NEWBIE TIPS ~~~~~");
					returnString += this.Formatter.General(this.RoomNewbieTips);
					returnString += this.Formatter.Bold("~~~~~     END     ~~~~~");
				}
			}

            // Add the location details for the room
            string nameOfLocation = this.RoomID;
            if (nameOfLocation.Contains("||"))
            {
                nameOfLocation = nameOfLocation.Substring(0, nameOfLocation.IndexOf("||"));
            }
			returnString += this.Formatter.Bold($"Location Details - {nameOfLocation.Replace(".",", ")}:");
			returnString += this.Formatter.Italic(this.RoomDescription, 2);

			// Add the people within the location
			returnString += this.GetPeopleDetails(smc);

            // Add the NPCs within the location
            returnString += this.GetNPCDetails();

            // Add the exits to the room so that someone can leave.
            returnString += this.GetExitDetails();

			// Show all the items within the room that can be returned.
			returnString += this.GetItemDetails();

			// Return the string to the calling method.
			return returnString;
		}

        /// <summary>
        /// Inspect a person, object, etc.
        /// </summary>
        /// <param name="smc">The character doing the inspecting</param>
        /// <param name="thingToInspect">The thing that the person wants to inspect</param>
        public void InspectThing(SMCharacter smc, string thingToInspect)
        {
            // Check if it's a character first
            SMCharacter targetCharacter = this.GetPeople().FirstOrDefault(checkChar => checkChar.GetFullName().ToLower() == thingToInspect.ToLower());
            
			if (targetCharacter != null)
            {
                smc.sendMessageToPlayer(this.Formatter.Bold("Description of " + targetCharacter.GetFullName() + " (Level " + targetCharacter.CalculateLevel() + "):"));
                if ((targetCharacter.Description != null) || (targetCharacter.Description != ""))
                {
                    smc.sendMessageToPlayer(this.Formatter.Italic(targetCharacter.Description));
                }
                else
                {
                    smc.sendMessageToPlayer(this.Formatter.Italic("No description set..."));
                }
                smc.sendMessageToPlayer(this.Formatter.CodeBlock(targetCharacter.GetInventoryList()));
				targetCharacter.sendMessageToPlayer(this.Formatter.Italic(smc.GetFullName() + " looks at you"));
				return;
            }

			// If not a character, check if it is an NPC...
			SMNPC targetNPC = this.GetNPCs().FirstOrDefault(checkChar => (checkChar.GetFullName().ToLower() == thingToInspect.ToLower()) || (checkChar.NPCType.ToLower() == thingToInspect.ToLower()));

			if (targetNPC != null)
			{
				smc.sendMessageToPlayer(this.Formatter.Bold("Description of " + targetNPC.GetFullName() + " (Level " + targetNPC.CalculateLevel() + "):"));
				if ((targetNPC.Description != null) || (targetNPC.Description != ""))
				{
					smc.sendMessageToPlayer(this.Formatter.Italic(targetNPC.Description));
				}
				else
				{
					smc.sendMessageToPlayer(this.Formatter.Italic("No description set..."));
				}
				smc.sendMessageToPlayer(this.Formatter.CodeBlock(targetNPC.GetInventoryList()));
				targetNPC.GetRoom().ProcessNPCReactions("PlayerCharacter.ExaminesThem", smc, targetNPC.UserID);
				targetNPC.GetRoom().ProcessNPCReactions("PlayerCharacter.Examines", smc);
				return;
			}

			// If not an NPC, check the players equipped items and the items in the room...
			SMItem smi = smc.GetEquippedItem(thingToInspect);

			if (smi == null)
			{
				smi = SMItemHelper.GetItemFromList(this.RoomItems, thingToInspect);
			}

			if (smi != null)
			{
				string itemDeatils = this.Formatter.Bold("Description of \"" + smi.ItemName + "\":");
				itemDeatils += this.Formatter.ListItem(smi.ItemDescription);

				if (smi.CanHoldOtherItems())
				{
					itemDeatils += this.Formatter.Italic($"This \"{smi.ItemName}\" contains the following items:");
					itemDeatils += SMItemHelper.GetContainerContents(smi);
				}

				smc.sendMessageToPlayer(itemDeatils);
				return;
			}

			// Otherwise nothing found
			smc.sendMessageToPlayer(this.Formatter.Italic("Can not inspect that item."));
		}

		#endregion

		#region "Inventory Function"

		/// <summary>
		/// Gets an item in the room by its ItemName (case insensitive).
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="name">ItemName.</param>
		public SMItem GetItemByName(string name)
		{
			if (this.RoomItems == null)
			{
				return null;
			}

			return this.RoomItems.FirstOrDefault(smi => smi.ItemName.ToLower() == name.ToLower());
		}

		/// <summary>
		/// Gets an item in the room by its ItemName.
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="name">ItemName.</param>
		public SMItem GetItemByFamilyName(string familyName)
		{
			if (this.RoomItems == null)
			{
				return null;
			}

			return this.RoomItems.FirstOrDefault(smi => smi.ItemFamily.ToLower() == familyName.ToLower());
		}

		/// <summary>
		/// Gets the id of an item in the room by its name
		/// </summary>
		/// <param name="itemName"></param>
		/// <returns>The ItemID</returns>
		public string GetRoomItemID(string itemName)
		{
			SMItem item = this.GetItemByName(itemName);

			if (item != null)
			{
				return item.ItemID;
			}

			return null;
		}

		/// <summary>
		/// Update an item
		/// </summary>
		/// <param name="itemID">The ID of the item</param>
		/// <param name="attributeType">The attribute to update</param>
		/// <param name="newValue">The new value of the attribue</param>
		public void UpdateItem(string itemID, string attributeType, int newValue)
		{
			if (this.RoomItems != null)
			{
				switch (attributeType)
				{
					case "HP":
						this.RoomItems.FirstOrDefault(smitem => smitem.ItemID == itemID).HitPoints = newValue;
						break;
				}
				this.SaveToApplication();
			}
		}

		#endregion

		#region "Room Chat Functions"

		/// <summary>
		/// Character "SAY" method
		/// </summary>
		/// <param name="speech">What the character is "Saying"</param>
		/// <param name="charSpeaking">The character who is speaking</param>
		public void ChatSay(string speech, SMCharacter charSpeaking)
		{
			// Construct the message
			string message = this.Formatter.Italic(charSpeaking.GetFullName() + " says:", 0) + " \"" + speech + "\"";

			// Send the message to all people connected to the room
			foreach (SMCharacter smc in this.GetPeople())
			{
				this.ChatSendMessage(smc, message);
			}
		}

		/// <summary>
		/// Character "Whisper" method
		/// </summary>
		/// <param name="speech">What the character is "Whispering"</param>
		/// <param name="charSpeaking">The character who is speaking</param>
		/// /// <param name="whisperToName">The character who is being whispered to</param>
		public void ChatWhisper(string speech, SMCharacter charSpeaking, string whisperToName)
		{
			// Construct the message
			string message = this.Formatter.Italic(charSpeaking.GetFullName() + " whispers:", 0) + " \"" + speech + "\"";

			// See if the person being whispered to is in the room
			SMCharacter smc = this.GetPeople().FirstOrDefault(charWhisperedto => charWhisperedto.GetFullName() == whisperToName);
			if (smc != null)
			{
				this.ChatSendMessage(smc, message);
			}
			else
			{
				message = "That person doesn't appear to be here?";
				// TODO Send message to player to say they can't whisper to that person.
			}
		}

		/// <summary>
		/// Character "Shout" method
		/// </summary>
		/// <param name="speech">What the character is "Shouting"</param>
		/// <param name="charSpeaking">The character who is speaking</param>
		public void ChatShout(string speech, SMCharacter charSpeaking)
		{
			// variable for the message sending used later.
			string message;

			// Send the message to all people connected to the room
			foreach (SMCharacter smc in this.GetPeople())
			{
				// construct the local message to be sent.
				message = this.Formatter.Bold(charSpeaking.GetFullName() + " shouts:", 0) + " \"" + speech + "\"";
				this.ChatSendMessage(smc, message);
			}

			// Send a message to people connected to rooms around this room
			foreach (SMExit sme in this.RoomExits)
			{
				// Get the room details from the exit id
				SMRoom otherRooms = new SMRoom();

				otherRooms = new SlackMud().GetRoom(sme.RoomID);

				// Get the "from" location
				SMExit smre = otherRooms.RoomExits.FirstOrDefault(smef => smef.RoomID == this.RoomID);

				// Construct the message
				message = this.Formatter.Italic("Someone shouts from " + smre.Description + " (" + smre.Shortcut + "):",0) + " \"" + speech + "\"";

				// Send the message to all people connected to the room
				foreach (SMCharacter smcInOtherRoom in otherRooms.GetPeople())
				{
					otherRooms.ChatSendMessage(smcInOtherRoom, message);
				}
			}
		}

		/// <summary>
		/// Character "EMOTE" method
		/// </summary>
		/// <param name="emoting">What the character is "Emoting"</param>
		/// <param name="charSpeaking">The character who is emoting</param>
		public void ChatEmote(string speech, SMCharacter charSpeaking, SMNPC charNPCSpeak = null)
		{
			// Construct the message
			// Precursor items for generic NPCs
			string precursor = "";
			if ((charNPCSpeak != null) && (charNPCSpeak.IsGeneric))
			{
				precursor = charNPCSpeak.PronounSingular.ToUpper() + " ";
			}

			// Output the message
			string message = this.Formatter.Italic(precursor + charSpeaking.GetFullName() + " " + speech,0);

			// Send the message to all people connected to the room
			foreach (SMCharacter smc in this.GetPeople())
			{
				this.ChatSendMessage(smc, message);
			}
		}

		public void Announce(string msg, SMCharacter sender = null, bool suppressMessageToSender = false)
		{
			// Send the message to all people connected to the room
			foreach (SMCharacter smc in this.GetPeople())
			{
				bool sendMessage = true;

				if ((suppressMessageToSender) && (sender != null) && (sender.UserID == smc.UserID))
				{
					sendMessage = false;
				}

				if (sendMessage)
				{
					this.ChatSendMessage(smc, msg);
				}
			}
		}

		public void ChatSendMessage(SMCharacter smc, string msg)
		{
			smc.sendMessageToPlayer(msg);
		}

        #endregion

        #region "NPC Functions"

        public void ProcessNPCReactions(string actionType, SMCharacter invokingCharacter, string extraData = null)
        {
            List<SMNPC> lNPCs = new List<SMNPC>();
            lNPCs = ((List<SMNPC>)HttpContext.Current.Application["SMNPCs"]).FindAll(npc => ((npc.RoomID == invokingCharacter.RoomID) && (npc.GetFullName() != invokingCharacter.GetFullName())));

            // Check if the character already exists or not.
            if (lNPCs != null)
            {
                // Get the NPCs who have a response of the right type
				if (extraData != null)
				{
					switch (actionType) {
						case "PlayerCharacter.ExaminesThem":
							lNPCs = lNPCs.FindAll(npc => ((npc.NPCResponses.Count(npcr => npcr.ResponseType.ToLower() == actionType.ToLower()) > 0) && (npc.UserID == extraData)));
							break;
						default:
							lNPCs = lNPCs.FindAll(npc => npc.NPCResponses.Count(npcr => npcr.ResponseType.ToLower() == actionType.ToLower()) > 0);
							break;
					}
				}
				else
				{
					lNPCs = lNPCs.FindAll(npc => npc.NPCResponses.Count(npcr => npcr.ResponseType.ToLower() == actionType.ToLower()) > 0);
				}
				
                if (lNPCs != null)
                {
                    foreach (SMNPC reactingNPC in lNPCs)
                    {
						HttpContext ctx = HttpContext.Current;

						Thread npcReactionThread = new Thread(new ThreadStart(() =>
						{
							HttpContext.Current = ctx;
							reactingNPC.RespondToAction(actionType, invokingCharacter);
						}));

						npcReactionThread.Start();
                    }
                }
            }
        }

		public void Spawn()
		{
            if (this.NPCSpawns != null)
            {
                // Only one thing at a time can spawn at the moment.
                bool spawnedThisRound = false;
                List<SMNPC> smnpcl = new List<SMNPC>();

                // loop around the spawns
                foreach (SMSpawn sms in this.NPCSpawns)
                {
                    // random number between 1 and 100
                    int randomChance = (new Random().Next(1, 100));

                    if (!spawnedThisRound)
                    {
                        // Check if we should try spawning
                        if (randomChance < sms.SpawnFrequency)
                        {
                            // Check if it's a unique person
                            if (sms.Unique)
                            {
                                // Check if the unique NPC is already somewhere in the world...
                                smnpcl = (List<SMNPC>)HttpContext.Current.Application["SMNPCs"];
                                if (smnpcl.Count(npc => (npc.FirstName + npc.LastName) == sms.TypeOfNPC) == 0)
                                {
                                    // ... if they're not, spawn them into the room.
                                    SMNPC newNPC = NPCHelper.GetNewNPC(sms.TypeOfNPC, true);
                                    newNPC.RoomID = this.RoomID;
                                    smnpcl.Add(newNPC);
                                    HttpContext.Current.Application["SMNPCs"] = smnpcl;

                                    this.Announce(this.Formatter.Italic(newNPC.GetFullName() + " walks in"));
                                }
                            }

                            if (!spawnedThisRound)
                            {
                                // Check how many there are of this type in the room already
                                int numberOfNPCsOfType = this.GetNPCs().Count(npc => npc.NPCType == sms.TypeOfNPC);

                                // If there are less NPCs than the max number of the type...
                                if (numberOfNPCsOfType < sms.MaxNumber)
                                {
                                    // .. add one
                                    SMNPC newNPC = NPCHelper.GetNewNPC(sms.TypeOfNPC);
                                    newNPC.RoomID = this.RoomID;
									smnpcl = (List<SMNPC>)HttpContext.Current.Application["SMNPCs"];
									smnpcl.Add(newNPC);
                                    HttpContext.Current.Application["SMNPCs"] = smnpcl;

                                    this.Announce(this.Formatter.Italic(newNPC.PronounSingular.ToUpper() + " " + newNPC.GetFullName() + " " + newNPC.WalkingType + "s in"));
                                }
                            }
                        }
                    }
                }
            }
		}

        #endregion
    }

    public class SMExit
	{
		[JsonProperty("Shortcut")]
		public string Shortcut { get; set; }

		[JsonProperty("Description")]
		public string Description { get; set; }

        [JsonProperty("RoomID")]
        public string RoomID { get; set; }

        [JsonProperty("RoomLockID")]
        public string RoomLockID { get; set; }

        [JsonProperty("Lockable")]
        public bool Lockable { get; set; }

        [JsonProperty("Locked")]
        public bool Locked { get; set; }
    }

	public class SMSpawn
	{
		[JsonProperty("TypeOfNPC")]
		public string TypeOfNPC { get; set; }

		[JsonProperty("MaxNumber")]
		public int MaxNumber { get; set; }

		[JsonProperty("SpawnFrequency")]
		public int SpawnFrequency { get; set; }

		[JsonProperty("Unique")]
		public bool Unique { get; set; }
	}

    public class SMRoomSafeCharacterAttributes
    {
        [JsonProperty("CharacterName")]
        public string CharacterName { get; set; }

        [JsonProperty("CharacterName")]
        public SMAttributes SavedAttributes { get; set; }
    }
}
