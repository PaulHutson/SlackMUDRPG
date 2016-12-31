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
		/// Removes and item by ItemId from the characters CharacterItems list dropping the item to the characters current room.
		/// </summary>
		/// <param name="Id">ItemId</param>
		public void DropItem(string id)
		{
			if (this.CharacterItems == null)
			{
				return;
			}

			SMItem item = this.CharacterItems.Find(obj => obj.ItemId == id);

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
                    this.RoomID = sme.RoomID;
                    returnString = SlackMud.GetLocationDetails(this.RoomID);

                    // Announce arrival to other players in the same place
                    // TODO room.Announce()
                }
            }

            return returnString;
        }
    }
}