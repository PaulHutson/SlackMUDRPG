using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.CommandsClasses
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
            } else
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
                    returnString += sme.Description + "(" + sme.Shortcut + ")";
                }
            }
            
            return returnString;
        }
        
        /// <summary>
        /// Gets a list of all the people in the room.
        /// </summary>
        public string GetPeopleDetails(string userID = "0")
        {
            string returnString = "\n\nPeople:\n";

            // Add the people within the location
            // Search through logged in users to see which are in this location
            List<SMCharacter> smcs = new List<SMCharacter>();
            smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];

            // Check if the character already exists or not.
            if (smcs != null)
            {
                if (smcs.Count(smc => smc.RoomID == this.RoomID) > 0)
                {
                    List<SMCharacter> charsInLocation = new List<SMCharacter>();
                    charsInLocation = smcs.FindAll(s => s.RoomID == this.RoomID);
                    bool isFirst = true;
                    foreach (SMCharacter sma in charsInLocation)
                    {
                        if (!isFirst)
                        {
                            returnString += ", ";
                        }
                        else
                        {
                            isFirst = false;
                        }

                        if (sma.UserID == userID)
                        {
                            returnString += "You";
                        }
                        else
                        {
                            returnString += sma.FirstName + " " + sma.LastName;
                        }

                    }
                }
                else
                {
                    returnString += "There's noone here.";
                }
            }
            else
            {
                returnString += "There's noone here.";
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
            returnString += "\n\nItems: TODO";

            // Return the string to the calling method.
            return returnString;
        }
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