using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.CommandClasses
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

		[JsonProperty("CurrentActivity")]
		public string CurrentActivity { get; set; }

		[JsonProperty("Attributes")]
		public SMAttributes Attributes { get; set; }

		[JsonProperty("Skills")]
		public List<SMCharacterSkill> Skills { get; set; }

		[JsonProperty("CharacterSlots")]
		public List<SMCharacterSlot> CharacterSlots { get; set; }

        public string ResponseURL { get; set; }

		#region "General Player Functions"

		/// <summary>
		/// Get the full name of the character.
		/// </summary>
		public string GetFullName()
		{
			return this.FirstName + " " + this.LastName;
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
			List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];

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
			return new SlackMud().GetRoom(this.RoomID);
		}

		/// <summary>
		/// Gets the characters current room details,
		/// </summary>
		public void GetRoomDetails()
		{
			this.sendMessageToPlayer(new SlackMud().GetLocationDetails(this.RoomID, this.UserID));
		}

		/// <summary>
		/// Move the character.
		/// </summary>
		/// <param name="charID"></param>
		/// <param name="exitShortcut"></param>
		/// <returns></returns>
		public void Move(string exitShortcut)
		{
			// Get the current character location
			List<SMCharacter> smcs = new List<SMCharacter>();
			smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
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
					this.sendMessageToPlayer("Character not logged in, please login before trying to move.");
				}
			}
			else
			{
				this.sendMessageToPlayer("Character not logged in, please login before trying to move.");
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
				SMRoom smr = new SlackMud().GetRoom(sme.RoomID);

				if (smr != null)
				{
					// Move the player to the new location
					this.RoomID = smr.RoomID;
					this.SaveToFile();
					this.sendMessageToPlayer(new SlackMud().GetLocationDetails(this.RoomID));

					// Announce arrival to other players in the same place
					smr.Announce("_" + this.GetFullName() + " walks in._", this, true);

				}
			}
		}

		#endregion

		#region "Skill Related Items"
		
		/// <summary>
		/// Use a skill
		/// </summary>
		/// <param name="skillName">The name of the skill to use</param>
		/// <param name="targetName">The name of a (the) target to use the skill on (optional)</param>
		public void UseSkill(string skillName, string targetName = null, bool isCombat = false)
		{
			// Find out if the character has the skill.
			SMCharacterSkill smcs = this.Skills.FirstOrDefault(charskill => charskill.SkillName == skillName);

			// If the character has the skill
			if ((isCombat) || (smcs != null))
			{
				// Variables for use later
				string targetType = null, targetID = null;
				bool useSkill = true;

				// If there's a target we need to look at...
				if (targetName != null) {
					// .. get the room
					SMRoom currentRoom = this.GetRoom();

					// find any players with that target name first
					SMCharacter targetCharacter = currentRoom.GetPeople().FirstOrDefault(tC => tC.GetFullName() == targetName);

					// If it's not null set the target details
					if (targetCharacter != null)
					{
						// Set the target as a character and set the target id
						targetType = "Character";
						targetID = targetCharacter.UserID;
					}
					else // We need to see if there's an object with the name
					{
						// get a target item with the target name
						SMItem targetItem = currentRoom.GetItemByName(targetName);

						// if we find one...
						if (targetItem != null)
						{
							// .. set the target type to be an item and set the target id
							targetType = "Item";
							targetID = targetItem.ItemID;
						}
						else
						{
							// get a target item with the target name
							targetItem = currentRoom.GetItemByFamilyName(targetName);

							// if we find one...
							if (targetItem != null)
							{
								// .. set the target type to be an item and set the target id
								targetType = "Item";
								targetID = targetItem.ItemID;
							}
							else
							{
								// Not found the target with the name.. so send a message...
								this.sendMessageToPlayer("The target you've specified is not valid");

								// And we can't use the skill because we can't find the target.
								useSkill = false;
							}
						}
					}
				}

				// Check if we're able to use the skill...
				if (useSkill) { 
					// Output variables we don't need
					string messageOut;
					float floatOut;

					// Create a new instance of the skill.
					SMSkill smc = ((List<SMSkill>)HttpContext.Current.Application["SMSkills"]).FirstOrDefault(sms => sms.SkillName == skillName);

					// Execute the skill
					smc.UseSkill(this, out messageOut, out floatOut, true, targetType, targetID);
				}
			}
			else
			{
				// Can't use the skill so let the player know!
				this.sendMessageToPlayer("You need to learn the \"" + skillName + "\" skill before you can use it.");
			}
		}

		/// <summary>
		/// Check that a player has the required skill level (by name)
		/// </summary>
		/// <param name="skillName">Name of the skill</param>
		/// <param name="skillLevel">The skill level the player has</param>
		/// <returns></returns>
		public bool HasRequiredSkill(string skillName, string skillLevel)
		{
			return this.Skills.FirstOrDefault(skill => skill.SkillName == skillName && skill.SkillLevel >= int.Parse(skillLevel)) != null;
		}

		#endregion

		#region "Combat Related Functions"

		public void Attack(string targetName)
		{
			// If there's a target we need to look at...
			if (targetName != null)
			{
				// .. get the room
				SMRoom currentRoom = this.GetRoom();

				// find any players with that target name first
				SMCharacter targetCharacter = currentRoom.GetPeople().FirstOrDefault(tC => tC.GetFullName() == targetName);

				// If it's not null set the target details
				if (targetCharacter != null)
				{
					// Attack the character
					SMCombat.Attack(this, targetCharacter);
				}
				else // We need to see if there's an object with the name
				{
					// get a target item with the target name
					SMItem targetItem = currentRoom.GetItemByName(targetName);

					// if we find one...
					if (targetItem != null)
					{
						// Attack the item
						SMCombat.Attack(this, targetItem);
					}
					else
					{
						// get a target item with the target name
						targetItem = currentRoom.GetItemByFamilyName(targetName);

						// if we find one...
						if (targetItem != null)
						{
							// Attack the item
							SMCombat.Attack(this, targetItem);
						}
						else
						{
							// Not found the target with the name.. so send a message...
							this.sendMessageToPlayer("The target you've specified is not valid");
						}
					}
				}
			}
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
		/// Picks up an item from the characters current room, by the items name.
		/// </summary>
		/// <param itemName="The name of the item."></param>
		public void PickUpItem(string itemName)
		{
			string itemID = this.GetRoom().GetRoomItemID(itemName);

			if (itemID == null)
			{
				this.sendMessageToPlayer($"Cannot find that item in this room");
				return;
			}

			this.PickUpItemByID(itemID);
		}

		/// <summary>
		/// Picks up an item from the characters current room, by the items ItemID.
		/// </summary>
		/// <param name="id">The ItemID if the item to pick up.</param>
		public void PickUpItemByID(string id)
		{
			SMRoom room = this.GetRoom();

			SMItem item = room.RoomItems.FirstOrDefault(smi => smi.ItemID == id);

			if (item == null)
			{
				this.sendMessageToPlayer($"Cannot find that item in this room");
				return;
			}

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

			// remove item from the room
			room.RemoveItem(item);
			room.Announce($"\"{GetFullName()}\" picked up \"{item.ItemName}\"");

			// add item to slot
			hand.EquippedItem = item;
			this.sendMessageToPlayer($"You equipped \"{item.ItemName}\"");

			this.SaveToApplication();
		}

		/// <summary>
		/// Drops an item the character is holding by its name.
		/// </summary>
		/// <param name="itemName">Name of the item.</param>
		public void DropItem(string itemName)
		{
			// check item is in a hand
			SMCharacterSlot hand = GetHandWithItemEquipped(itemName);

			if (hand == null)
			{
				sendMessageToPlayer("You must be holding the item to drop it");
				return;
			}

			SMItem item = this.GetEquippedItem(itemName);

			// remove item from slot
			hand.EquippedItem = null;
			SaveToApplication();

			// add item to room
			SMRoom room = GetRoom();
			room.AddItem(item);

			// announce to the room
			room.Announce($"\"{this.GetFullName()}\" dropped \"{item.ItemName}\"");
		}

		//TODO equip item

		//TODO equip item to slot

		//TODO list slot

		//TODO list slots

		public string GetOwnedItemIDByName(string name)
		{
			foreach (SMCharacterSlot slot in CharacterSlots)
			{
				if (!slot.isEmpty())
				{
					if (slot.EquippedItem.ItemName == name)
					{
						return slot.EquippedItem.ItemID;
					}

					if (slot.EquippedItem.ItemType == "container")
					{
						SMItem item = this.FindItemInContainerByName(name, slot.EquippedItem);
						if (item != null)
						{
							return item.ItemID;
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Has the item equipped.
		/// </summary>
		/// <returns><c>true</c>, if item equipped was hased, <c>false</c> otherwise.</returns>
		/// <param name="id">ItemID.</param>
		public bool HasItemEquipped(string id)
		{
			foreach (SMCharacterSlot slot in CharacterSlots)
			{
				if (!slot.isEmpty() && slot.EquippedItem.ItemID == id)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Has an item of a given type equipped.
		/// </summary>
		/// <returns><c>true</c>, if item of given type equipped was hased, <c>false</c> otherwise.</returns>
		/// <param name="type">ItemType.</param>
		public bool HasItemTypeEquipped(string type)
		{
			foreach (SMCharacterSlot slot in CharacterSlots)
			{
				if (!slot.isEmpty() && slot.EquippedItem.ItemType == type)
				{
					return true;
				}
			}

			return false;
		}

        /// <summary>
		/// Has an item of a given family type equipped.
		/// </summary>
		/// <returns><c>true</c>, if item of given type equipped was hased, <c>false</c> otherwise.</returns>
		/// <param name="familyType">Item Type Family.</param>
		public bool HasItemFamilyTypeEquipped(string familyType)
        {
            foreach (SMCharacterSlot slot in CharacterSlots)
            {
                if (!slot.isEmpty() && slot.EquippedItem.ItemFamily == familyType)
                {
                    return true;
                }
            }

            return false;
        }

		/// <summary>
		/// Gets an item by name / family name
		/// </summary>
		/// <param name="itemName">The name / family name of the item</param>
		/// <returns>The equipped item</returns>
		public SMItem GetEquippedItem(string itemName)
		{
			foreach (SMCharacterSlot slot in CharacterSlots)
			{
				if (!slot.isEmpty())
				{
					if (slot.EquippedItem.ItemName == itemName)
					{
						return slot.EquippedItem;
					}

					if (slot.EquippedItem.ItemFamily == itemName)
					{
						return slot.EquippedItem;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Counts the number of a named item the character owns.
		/// </summary>
		/// <returns>The count.</returns>
		/// <param name="name">ItemName.</param>
		public int CountOwnedItemsByName(string name)
		{
			int count = 0;

			foreach (SMCharacterSlot slot in CharacterSlots)
			{
				if (!slot.isEmpty())
				{
					if (slot.EquippedItem.ItemName == name)
					{
						count++;
					}
					else if (slot.EquippedItem.ItemFamily == name)
					{
						count++;
					}

					if (slot.EquippedItem.ItemType == "container")
					{
						SMItem item = this.FindItemInContainerByName(name, slot.EquippedItem);
						if (item != null && item.ItemName == name)
						{
							count++;
						}
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Are the characters hands empty.
		/// </summary>
		/// <returns><c>true</c>, if hands are empty, <c>false</c> otherwise.</returns>
		public bool AreHandsEmpty()
		{
			SMCharacterSlot rightHand = this.GetSlotByName("RightHand");
			SMCharacterSlot leftHand = this.GetSlotByName("LeftHand");

			return rightHand.isEmpty() && leftHand.isEmpty();
		}

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
					if (slot.EquippedItem.ItemType == "container")
					{
						weight += this.GetContainerWeight(slot.EquippedItem);
					}
					else
					{
						weight += slot.EquippedItem.ItemWeight;
					}
				}
			}

			return weight;
		}

		/// <summary>
		/// Gets the weight of items in a container, this will recursivley call itself to resolve the weights of
		/// containers inside other containers that hold items.
		/// </summary>
		/// <returns>The weight.</returns>
		private int GetContainerWeight(SMItem container)
		{
			int weight = container.ItemWeight;

			if (container.HeldItems != null)
			{
				foreach (SMItem item in container.HeldItems)
				{
					if (item.ItemType == "container")
					{
						weight += this.GetContainerWeight(item);
					}
					else
					{
						weight += item.ItemWeight;
					}
				}
			}

			return weight;
		}

		/// <summary>
		/// Gets an equipped item by its ItemID.
		/// </summary>
		/// <returns>The equipped item.</returns>
		/// <param name="id">ItemID.</param>
		private SMItem GetEquippedItemByID(string id)
		{
			foreach (SMCharacterSlot slot in this.CharacterSlots)
			{
				if (slot.EquippedItem != null)
				{
					if (slot.EquippedItem.ItemID == id)
					{
						return slot.EquippedItem;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the hand (SMCharacterSlot) which has the item equipped.
		/// </summary>
		/// <returns>The hand (SMCharacterSlot) with item equipped, or null.</returns>
		/// <param name="itemName">The name of the item.</param>
		private SMCharacterSlot GetHandWithItemEquipped(string itemName)
		{
			SMCharacterSlot rightHand = this.GetSlotByName("RightHand");
			SMCharacterSlot leftHand = this.GetSlotByName("LeftHand");

			if (!rightHand.isEmpty() && rightHand.EquippedItem.ItemName == itemName)
			{
				return rightHand;
			}
			else if (!leftHand.isEmpty() && leftHand.EquippedItem.ItemName == itemName)
			{
				return leftHand;
			}

			return null;
		}

		/// <summary>
		/// Get the first item a player is holding
		/// </summary>
		/// <returns>The equipped item</returns>
		public SMItem GetEquippedItem()
		{
			foreach (SMCharacterSlot slot in CharacterSlots)
			{
				if (!slot.isEmpty())
				{
					return slot.EquippedItem;
				}
			}

			return null;
		}

		/// <summary>
		/// Finds an item by name in a container by recursivly searching through the container
		/// and any containers it contains.
		/// </summary>
		/// <returns>The item in a container.</returns>
		/// <param name="name">ItemName.</param>
		/// <param name="container">Container to look in.</param>
		private SMItem FindItemInContainerByName(string name, SMItem container)
		{
			if (container.HeldItems != null)
			{
				foreach (SMItem item in container.HeldItems)
				{
					if (item.ItemName == name)
					{
						return item;
					}

					if (item.ItemType == "container")
					{
						SMItem smi = this.FindItemInContainerByName(name, item);
						if (smi != null)
						{
							return smi;
						}
					}
				}
			}

			return null;
		}

		#endregion

		#region "Chat Functions"

		/// <summary>
		/// Make the character say something
		/// </summary>
		/// <param name="speech">What the character is saying</param>
		public void Say(string speech)
		{
            new SlackMud().GetRoom(this.RoomID).ChatSay(speech, this);
		}

		/// <summary>
		/// Make the character Whisper something
		/// </summary>
		/// <param name="speech">What the character is whispering</param>
		/// /// <param name="whisperToName">Who the character is whispering to (name)</param>
		public void Whisper(string speech, string whisperToName)
		{
            new SlackMud().GetRoom(this.RoomID).ChatWhisper(speech, this, whisperToName);
		}

		/// <summary>
		/// Make the character Shout something
		/// </summary>
		/// <param name="speech">What the character is shouting</param>
		public void Shout(string speech)
		{
            new SlackMud().GetRoom(this.RoomID).ChatShout(speech, this);
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
			Commands.SendMessage("", "SlackMud", message, "SlackMud", this.UserID, this.ResponseURL);
		}

		#endregion
	}
}