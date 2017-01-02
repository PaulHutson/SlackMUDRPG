using Newtonsoft.Json;
using SlackMUDRPG.CommandClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

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

        [JsonProperty("UserID")]
        public string UserID { get; set; }

        [JsonProperty("RoomID")]
        public string RoomID { get; set; }

        [JsonProperty("Attributes")]
        public SMAttributes Attributes { get; set; }

        [JsonProperty("CharacterItems")]
        public List<SMItem> CharacterItems { get; set; }
        
        [JsonProperty("Skills")]
        public List<SMSkill> Skills { get; set; }

		[JsonProperty("CharacterSlots")]
		public List<SMCharacterSlot> CharacterSlots { get; set; }

        #region "General Player Functions"

        /// <summary>
        /// Get the full name of the character.
        /// </summary>
        public string GetFullName()
        {
            return this.FirstName + " " + this.LastName;
        }

        /// <summary>
        /// Adds the item to the characters CharacterItems list.
        /// </summary>
        /// <param name="item">Item.</param>
        public void AddItem(SMItem item)
		{
			if (this.CharacterItems == null)
			{
				this.CharacterItems = new List<SMItem>();
			}

			//TODO check that tha play has weight and capacity to add the item

			this.CharacterItems.Add(item);
			this.SaveToApplication();
		}

		/// <summary>
		/// Removes and item by ItemID from the characters CharacterItems list dropping the item to the characters current room.
		/// </summary>
		/// <param name="id">ItemID</param>
		public void DropItem(string id)
		{
			if (this.CharacterItems == null)
			{
				return;
			}

			SMItem item = this.CharacterItems.Find(obj => obj.ItemID == id);

			if (item != null)
			{
				SMRoom room = this.GetRoom();

				room.AddItem(item);
				this.CharacterItems.Remove(item);
				this.SaveToApplication();
			}
		}

		/// <summary>
		/// Saves the character to the file system.
		/// </summary>
		public void SaveToFile()
		{
			string path = FilePathSystem.GetFilePath("Characters", "Char" + this.UserID);
			string charJSON = JsonConvert.SerializeObject(this, Formatting.Indented);

			using (StreamWriter w = new StreamWriter(path))
			{
				w.WriteLine(charJSON);
			}
		}

		/// <summary>
		/// Saves the character to application memory.
		/// </summary>
		public void SaveToApplication()
		{
			List<SMCharacter> smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];

			SMCharacter characterInMem = smcs.FirstOrDefault(smc => smc.UserID == this.UserID);

			if (characterInMem != null)
			{
				smcs.Remove(characterInMem);
			}

			smcs.Add(this);
			HttpContext.Current.Application["SMCharacters"] = smcs;
		}

		/// <summary>
		/// Gets the characters current room, loads from mem or file as required
		/// </summary>
		public SMRoom GetRoom()
		{
            return SlackMud.GetRoom(this.RoomID);
		}

        /// <summary>
        /// Move the character.
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="exitShortcut"></param>
        /// <returns></returns>
        public string Move(string exitShortcut)
        {
            // Create returnString
            string returnString = "";

            // Get the current character location
            List<SMCharacter> smcs = new List<SMCharacter>();
            smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];
            SMCharacter charToMove = new SMCharacter();
            bool foundCharacter = false;

            // Check if there are characters actually on the server to move...
            if (smcs != null)
            {
                if (smcs.FirstOrDefault(smc => smc.UserID == this.UserID) != null)
                {
                    // Get the id from the object
                    charToMove = smcs.FirstOrDefault(smc => smc.UserID == this.UserID);
                    foundCharacter = true;
                }
                else
                {
                    returnString = "Character not logged in, please login before trying to move.";
                }
            }
            else
            {
                returnString = "Character not logged in, please login before trying to move.";
            }

            if (foundCharacter)
            {
                // Get the room for the characters location
                List<SMRoom> smrs = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];
                SMRoom roomInMem = smrs.FirstOrDefault(smrn => smrn.RoomID == charToMove.RoomID);

                // Get the specific exit from the location referred to by the shortcut
                SMExit sme = new SMExit();
                sme = roomInMem.RoomExits.FirstOrDefault(smes => smes.Shortcut == exitShortcut);

                // Get the new room (and check that it's loaded in memory).
                SMRoom smr = SlackMud.GetRoom(sme.RoomID);

                if (smr != null)
                {
                    // Move the player to the new location
                    this.RoomID = smr.RoomID;
                    this.SaveToFile();
                    returnString = SlackMud.GetLocationDetails(this.RoomID);

                    // Announce arrival to other players in the same place
                    smr.Announce("_" + this.GetFullName() + " walks in._");

                }
            }

            return returnString;
        }

        #endregion

		#region "Inventory Functions"

		/// <summary>
		/// Gets an SMCharacterSlot by name.
		/// </summary>
		/// <returns>The slot.</returns>
		/// <param name="name">Name of the slot.</param>
		public SMCharacterSlot GetSlotByName(string name)
		{
			if (this.CharacterSlots != null)
			{
				return this.CharacterSlots.FirstOrDefault(slot => slot.Name == name);
			}

			return null;
		}

		/// <summary>
		/// Picks up an item from the characters current room.
		/// </summary>
		/// <param name="item">The item to pick up.</param>
		public void PickUpItem(SMItem item)
		{
			// check for an empty hand to pick up the item
			SMCharacterSlot hand = this.GetEmptyHand();

			if (hand == null)
			{
				this.sendMessageToPlayer($"You need an empty hand to pick up \"{item.ItemName}\"");
				return;
			}

			// check weight constraints
			int weightLimit = this.GetWeightLimit();
			int currentWeight = this.GetCurrentWeight();

			if ((currentWeight + item.ItemWeight) > weightLimit)
			{
				this.sendMessageToPlayer($"Unable to pick up \"{item.ItemName}\" this would exceed your weight limit of \"{weightLimit}\"");
				return;
			}


			// check item is in the current room and remove it
			SMRoom room = this.GetRoom();
			if (room.RoomItems.FirstOrDefault(i => i.ItemID == item.ItemID) == null)
			{
				this.sendMessageToPlayer($"Cannot find \"{item.ItemName}\" in this room");
				return;
			}

			room.RemoveItem(item);
			room.Announce($"\"{GetFullName()}\" picked up \"{item.ItemName}\"");

			// add item to slot
			hand.EquippedItem = item;
			this.sendMessageToPlayer($"You equipped \"{item.ItemName}\"");

			this.SaveToApplication();
		}

		//TODO drop item

		//TODO equip item

		//TODO equip item to slot

		//TODO list slot

		//TODO list slots

		/// <summary>
		/// Gets an emptySMCharacterSlot that is a hand.
		/// </summary>
		/// <returns>The empty hand SMCharacterSlot or null.</returns>
		private SMCharacterSlot GetEmptyHand()
		{
			SMCharacterSlot rightHand = this.GetSlotByName("RightHand");
			SMCharacterSlot leftHand = this.GetSlotByName("LeftHand");

			if (rightHand.isEmpty())
			{
				return rightHand;
			}
			else if (leftHand.isEmpty())
			{
				return leftHand;
			}

			return null;
		}

		/// <summary>
		/// Gets the weight limit for the character based on STR attribute.
		/// </summary>
		/// <returns>The weight limit.</returns>
		private int GetWeightLimit()
		{
			return this.Attributes.Strength * 5;
		}

		/// <summary>
		/// Gets the current weight being carried by the character.
		/// </summary>
		/// <returns>The weight.</returns>
		private int GetCurrentWeight()
		{
			int weight = 0;

			foreach (SMCharacterSlot slot in this.CharacterSlots)
			{
				if (!slot.isEmpty())
				{
					weight += slot.EquippedItem.ItemWeight;

					if (slot.EquippedItem.ItemType == "container")
					{
						weight += this.GetContainerWeight(slot.EquippedItem);
					}
				}
			}

			return weight;
		}

		/// <summary>
		/// Gets the weight of items in a container.
		/// </summary>
		/// <returns>The weight.</returns>
		private int GetContainerWeight(SMItem container)
		{
			//TODO implement this properly
			return 0;
		}

		#endregion

        #region "Chat Functions"

        /// <summary>
        /// Make the character say something
        /// </summary>
        /// <param name="speech">What the character is saying</param>
        public void Say(string speech)
        {
            SlackMud.GetRoom(this.RoomID).ChatSay(speech, this);
        }

        /// <summary>
        /// Make the character Whisper something
        /// </summary>
        /// <param name="speech">What the character is whispering</param>
        /// /// <param name="whisperToName">Who the character is whispering to (name)</param>
        public void Whisper(string speech, string whisperToName)
        {
            SlackMud.GetRoom(this.RoomID).ChatWhisper(speech, this, whisperToName);
        }

        /// <summary>
        /// Make the character Shout something
        /// </summary>
        /// <param name="speech">What the character is shouting</param>
        public void Shout(string speech)
        {
            SlackMud.GetRoom(this.RoomID).ChatShout(speech, this);
        }

        /// <summary>
        /// Announce something to the player
        /// </summary>
        /// <param name="announcement">What is being announced to the player</param>
        public void Announce(string announcement)
        {
            sendMessageToPlayer(announcement);
        }

        /// <summary>
        /// Send the message to the player
        /// </summary>
        /// <param name="message">the message being sent to the player</param>
        public void sendMessageToPlayer(string message)
        {
            // TODO Change the name of the service based on the one used to send the information!
            Commands.SendToChannelMessage("", "SlackMud", message, "SlackMud", this.UserID);
        }

        #endregion
    }
}