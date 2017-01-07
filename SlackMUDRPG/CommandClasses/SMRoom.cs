using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

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
		/// Removes an item from the room.
		/// </summary>
		/// <param name="item">Item.</param>
		public void RemoveItem(SMItem item)
		{
			if (this.RoomItems != null)
			{
				this.RoomItems.Remove(item);
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
				returnString = "No Exits are found from this room...";
			}
			else
			{
				returnString += "\n\nRoom Exits:\n";
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

		/// <summary>
		/// Gets a list of all the people in the room.
		/// </summary>
		public string GetPeopleDetails(string userID = "0")
		{
			string returnString = "\n\nPeople:\n";

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
					}

					if (smc.UserID == userID)
					{
						returnString += "You";
					}
					else
					{
						returnString += smc.FirstName + " " + smc.LastName;
					}

				}
			}
			else
			{
				returnString += "There's noone here.";
			}

			return returnString;
		}

        /// <summary>
		/// Gets a list of all the items in the room.
		/// </summary>
		public string GetItemDetails()
        {
            string returnString = "\n\nObjects:\n";

            // Check if the character already exists or not.
            if (this.RoomItems.Count > 0)
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
                    }

                    returnString += smi.ItemName;

                }
            }
            else
            {
                returnString += "Nothing";
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
			// Create the string and add the basic room description.
			string returnString = this.RoomDescription;

			// Add the people within the location
			returnString += this.GetPeopleDetails(userID);

			// Add the exits to the room so that someone can leave.
			returnString += this.GetExitDetails();

			// Show all the items within the room that can be returned.
			returnString += this.GetItemDetails();

			// Return the string to the calling method.
			return returnString;
		}

		#endregion

		#region "Inventory Function"

		/// <summary>
		/// Gets an item in the room by its ItemName.
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="name">ItemName.</param>
		public SMItem GetItemByName(string name)
		{
			if (this.RoomItems == null)
			{
				return null;
			}

			return this.RoomItems.FirstOrDefault(smi => smi.ItemName == name);
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
			string message = "_" + charSpeaking.GetFullName() + " says:_ \"" + speech + "\"";

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
			string message = "_" + charSpeaking.GetFullName() + " whispers:_ \"" + speech + "\"";

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
				message = "*" + charSpeaking.GetFullName() + " shouts:* \"" + speech + "\"";
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
				message = "_Someone shouts from " + smre.Description + " (" + smre.Shortcut + "):_ \"" + speech + "\"";

				// Send the message to all people connected to the room
				foreach (SMCharacter smcInOtherRoom in otherRooms.GetPeople())
				{
					otherRooms.ChatSendMessage(smcInOtherRoom, message);
				}
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
	}

	public class SMExit
	{
		[JsonProperty("Shortcut")]
		public string Shortcut { get; set; }

		[JsonProperty("Description")]
		public string Description { get; set; }

		[JsonProperty("RoomID")]
		public string RoomID { get; set; }
	}
}