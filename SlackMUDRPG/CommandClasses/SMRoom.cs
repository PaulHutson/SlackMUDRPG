using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;
using SlackMUDRPG.Utility.Formatters;

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

        [JsonProperty("RoomDescription")]
		public string RoomDescription { get; set; }

		[JsonProperty("RoomExits")]
		public List<SMExit> RoomExits { get; set; }

		[JsonProperty("RoomItems")]
		public List<SMItem> RoomItems { get; set; }

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
				returnString = "> No Exits are found from this room...";
			}
			else
			{
				returnString += "\n> \n> Room Exits:\n";
				bool isFirst = true;

				foreach (SMExit sme in this.RoomExits)
				{
					if (!isFirst)
					{
						returnString += ", ";
					}
					else
					{
						isFirst = false;
						returnString += "> ";
					}
					returnString += sme.Description + " (" + sme.Shortcut + ")";
				}
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

        /// <summary>
        /// Gets a list of all the people in the room.
        /// </summary>
        public string GetPeopleDetails(string userID = "0")
		{
			string returnString = "\n> \n> People:\n";

			// Get the people within the location
			List<SMCharacter> smcs = this.GetPeople();

			// Check if the character already exists or not.
			if (smcs != null)
			{
				bool isFirst = true;
				foreach (SMCharacter smc in smcs)
				{
					if (!isFirst)
					{
						returnString += ", ";
					}
					else
					{
						isFirst = false;
						returnString += "> ";
					}

					if (smc.UserID == userID)
					{
						returnString += "You";
					}
					else
					{
						returnString += smc.GetFullName();
					}
				}
			}
            else
			{
				returnString += "> There's noone here.";
			}

			return returnString;
		}

        public string GetNPCDetails()
        {
            string returnString = "";

            List<SMNPC> SMNPCs = this.GetNPCs();

            // Check if the character already exists or not.
            if (SMNPCs != null)
            {
                bool isFirst = true;
                foreach (SMNPC smNPC in SMNPCs)
                {
                    if (!isFirst)
                    {
                        returnString += ", ";
                    }
                    else
                    {
                        isFirst = false;
                        returnString += "\n> \n> NPCs:\n> ";
                    }

                    returnString += smNPC.GetFullName();
                }
            }

            return returnString;
        }

        /// <summary>
		/// Gets a list of all the items in the room.
		/// </summary>
		public string GetItemDetails()
        {
            string returnString = "\n> \n> Objects:\n";

            // Check if the character already exists or not.
            if ((this.RoomItems != null) && (this.RoomItems.Count > 0))
            {
                bool isFirst = true;
                foreach (SMItem smi in RoomItems)
                {
                    if (!isFirst)
                    {
                        returnString += ", ";
                    }
                    else
                    {
                        isFirst = false;
						returnString += "> ";
					}

                    returnString += smi.ItemName;
                }
            }
            else
            {
                returnString += "> Nothing";
            }

            return returnString;
        }

        /// <summary>
        /// Internal Method to create a room decription, created as it's going to be used over and over...
        /// </summary>
        /// <param name="smr">An SMRoom</param>
        /// <returns>String including a full location string</returns>
        public string GetLocationInformation(string userID = "0")
		{
			// Construct the room string.
			string returnString = "*Location Details - " + this.RoomID.Replace(".",", ")  + ":*\n";

			// Create the string and add the basic room description.
			returnString += "> " + this.RoomDescription;

			// Add the people within the location
			returnString += this.GetPeopleDetails(userID);

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
            SMCharacter targetCharacter = this.GetPeople().FirstOrDefault(checkChar => checkChar.GetFullName() == thingToInspect);

			if (targetCharacter != null)
            {
                smc.sendMessageToPlayer(OutputFormatterFactory.Get().Bold("Description of " + targetCharacter.GetFullName() + " (Level " + targetCharacter.CalculateLevel() + "):"));
                if ((targetCharacter.Description != null) || (targetCharacter.Description != ""))
                {
                    smc.sendMessageToPlayer(OutputFormatterFactory.Get().Italic(targetCharacter.Description));
                }
                else
                {
                    smc.sendMessageToPlayer(OutputFormatterFactory.Get().Italic("No description set..."));
                }
                smc.sendMessageToPlayer(OutputFormatterFactory.Get().CodeBlock(targetCharacter.GetInventoryList()));
				targetCharacter.sendMessageToPlayer(OutputFormatterFactory.Get().Italic(smc.GetFullName() + " looks at you"));
            }
            else // If not a character, check if it is an NPC...
            {
				SMNPC targetNPC = this.GetNPCs().FirstOrDefault(checkChar => checkChar.GetFullName() == thingToInspect);

				if (targetNPC != null)
				{
					smc.sendMessageToPlayer(OutputFormatterFactory.Get().Bold("Description of " + targetNPC.GetFullName() + " (Level " + targetNPC.CalculateLevel() + "):"));
					if ((targetNPC.Description != null) || (targetNPC.Description != ""))
					{
						smc.sendMessageToPlayer(OutputFormatterFactory.Get().Italic(targetNPC.Description));
					}
					else
					{
						smc.sendMessageToPlayer(OutputFormatterFactory.Get().Italic("No description set..."));
					}
					smc.sendMessageToPlayer(OutputFormatterFactory.Get().CodeBlock(targetNPC.GetInventoryList()));
					targetNPC.GetRoom().ProcessNPCReactions("PlayerCharacter.ExaminesThem", smc);
				}
				else // If not an NPC, check the objects in the room
				{

					SMItem smi = SMItemHelper.GetItemFromList(this.RoomItems, thingToInspect);

					if (smi != null)
					{
						string itemDeatils = OutputFormatterFactory.Get().Bold("Description of \"" + smi.ItemName + "\":");
						itemDeatils += OutputFormatterFactory.Get().ListItem(smi.ItemDescription);

						if (smi.CanHoldOtherItems())
						{
							itemDeatils += OutputFormatterFactory.Get().Italic($"This \"{smi.ItemName}\" contains the following items:");
							itemDeatils += SMItemHelper.GetContainerContents(smi);
						}

						smc.sendMessageToPlayer(itemDeatils);
	                }
	                else
	                {
	                    smc.sendMessageToPlayer(OutputFormatterFactory.Get().Italic("Can not inspect that item."));
	                }
				}
            }
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

			return this.RoomItems.FirstOrDefault(smi => smi.ItemFamily == familyName);
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
			string message = OutputFormatterFactory.Get().Italic(charSpeaking.GetFullName() + " says:", 0) + " \"" + speech + "\"";

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
			string message = OutputFormatterFactory.Get().Italic(charSpeaking.GetFullName() + " whispers:", 0) + " \"" + speech + "\"";

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
				message = OutputFormatterFactory.Get().Bold(charSpeaking.GetFullName() + " shouts:", 0) + " \"" + speech + "\"";
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
				message = OutputFormatterFactory.Get().Italic("Someone shouts from " + smre.Description + " (" + smre.Shortcut + "):",0) + " \"" + speech + "\"";

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
		public void ChatEmote(string speech, SMCharacter charSpeaking)
		{
			// Construct the message
			string message = OutputFormatterFactory.Get().Italic(charSpeaking.GetFullName() + " " + speech);

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

        public void ProcessNPCReactions(string actionType, SMCharacter invokingCharacter)
        {
            List<SMNPC> lNPCs = new List<SMNPC>();
            lNPCs = (List<SMNPC>)HttpContext.Current.Application["SMNPCs"];

            // Check if the character already exists or not.
            if (lNPCs != null)
            {
                // Get the NPCs who have a response of the right type
                lNPCs = lNPCs.FindAll(npc => npc.NPCResponses.Count(npcr => npcr.ResponseType == actionType) > 0);
                if (lNPCs != null)
                {
                    foreach (SMNPC reactingNPC in lNPCs)
                    {
                        reactingNPC.RespondToAction(actionType, invokingCharacter);
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
}
