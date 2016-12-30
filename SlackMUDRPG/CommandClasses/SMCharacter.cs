using Newtonsoft.Json;
using SlackMUDRPG.CommandClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        [JsonProperty("userid")]
        public string UserID { get; set; }

        [JsonProperty("RoomLocation")]
        public string RoomLocation { get; set; }

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

			this.CharacterItems.Add(item);
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
            SMCharacter charToMove;
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
                // Get the exits from that location
                // TODO get room exits from memory

                // Get the specific exit from the location referred to by the shortcut
                // TODO check the exits include one for the location

                // Check new room is loaded
                // TODO Implement

                // Move the player to the new location
                // TODO change the player location

                // Announce arrival to other players in the same place
                // Get all players in the location

                // Send a message to those players
                // TODO 
            }

            return returnString;
        }
    }
}