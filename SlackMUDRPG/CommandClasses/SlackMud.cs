using Newtonsoft.Json;
using SlackMUDRPG.CommandClasses;
using SlackMUDRPG.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public static class SlackMud
    {

        #region "Login and Character Methods"

        /// <summary>
        /// Logs someone in with the Slack UserID
        /// </summary>
        /// <param name="userID">Slack UserID</param>
        /// <returns>A string response</returns>
        public static string Login(string userID)
        {
            // Variable for the return string
            string returnString = "";

            // Get the right path, and work out if the file exists.
            string path = FilePathSystem.GetFilePath("Characters", "Char" + userID);

            // Check if the character exists..
            if (!File.Exists(path))
            {
                // If they don't exist inform the person as to how to create a new user
                returnString = "You must create a character, to do so, use the command /sm CreateCharacter FIRSTNAME,LASTNAME,SEX,AGE\n";
                returnString += "i.e. /sm CreateCharacter Paul,Hutson,m,34";
            }
            else
            {
                // If the userid already has a user then get the character details.
                returnString = GetCharacter(userID);
            }

            // Return the text output
            return returnString;
        }

        /// <summary>
        /// Gets a character object, and loads it into memory.
        /// </summary>
        /// <param name="userID">userID is based on the id from the slack channel</param>
        /// <param name="newCharacter">newCharacter to change the output of the text based on whether the character is new or not</param>
        /// <returns>String message for usage</returns>
        public static string GetCharacter(string userID, bool newCharacter = false)
		{
			List<SMCharacter> smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];
			SMCharacter character = smcs.FirstOrDefault(smc => smc.UserID == userID);

			if (character != null)
			{
				return "Your are already logged in!";
			}
			else
			{
				string path = FilePathSystem.GetFilePath("Characters", "Char" + userID);

				if (File.Exists(path))
				{
					SMCharacter smc = new SMCharacter();

					using (StreamReader r = new StreamReader(path))
					{
						string json = r.ReadToEnd();
						smc = JsonConvert.DeserializeObject<SMCharacter>(json);
					}

					SMRoom room = smc.GetRoom();
					if (room != null)
					{
						//TODO room.Announce() someone has entered the room or new player ect...
					}

					smcs.Add(smc);
					HttpContext.Current.Application["SMCharacters"] = smcs;

					if (!newCharacter)
					{
						return "Welcome back " + smc.FirstName;
					}

					string returnString = "Welcome to SlackMud!\n";
					returnString += "We've created your character in the magical world of Arrelvia!"; // TODO, use a welcome script!
																									  // TODO, get room details

					return returnString;
				}
				else
				{
					// If the UserID doesn't have a character already, inform them that they need to create one.
					return "You do not have a character yet, you need to create one...";
				}
			}
        }

        /// <summary>
        /// Create new character method
        /// </summary>
        /// <param name="userID">UserID - from the Slack UserID</param>
        /// <param name="firstName">The first name of the character</param>
        /// <param name="lastName">The last name of the character</param>
        /// <param name="age">The age of the character</param>
        /// <param name="sexIn">M or F for the male / Female character</param>
        /// <returns>A string with the character information</returns>
        public static string CreateCharacter(string userID, string firstName, string lastName, int age, char sexIn)
        {
            // Create the character options
            SMCharacter SMChar = new SMCharacter();
			SMChar.UserID = userID;
            SMChar.FirstName = firstName;
            SMChar.LastName = lastName;
            SMChar.LastLogindate = DateTime.Now;
            SMChar.LastInteractionDate = DateTime.Now;
            SMChar.PKFlag = false;
            SMChar.Sex = sexIn;

            // Add default items to the character
            SMChar.AddItem(CreateItemFromJson("Containers.SmallBackpack"));
            SMChar.AddItem(CreateItemFromJson("Weapons.WoodenSword"));

            // Set the start location
            SMChar.RoomID = "1";
            string defaultRoomPath = FilePathSystem.GetFilePath("Scripts", "EnterWorldProcess-FirstLocation");
            if (File.Exists(defaultRoomPath))
            {
                // Use a stream reader to read the file in (based on the path)
                using (StreamReader r = new StreamReader(defaultRoomPath))
                {
                    // Create a new JSON string to be used...
                    string json = r.ReadToEnd();

                    // ... get the information from the the start location token..
                    SMStartLocation sl = JsonConvert.DeserializeObject<SMStartLocation>(json);

                    // Set the start location.
                    SMChar.RoomID = sl.StartLocation;
                }
            }

            // Get the path for the character
            string path = FilePathSystem.GetFilePath("Characters", "Char" + userID);

            // If the file doesn't exist i.e. the character doesn't exist
            if (!File.Exists(path))
            {
                // Write the character to the stream
                SMChar.SaveToFile();

                // log the newly created character into the game
                return GetCharacter(userID, true);
            }
            else
            {
                // If they already have a character tell them they do and that they need to login.
                return "You already have a character, you cannot create another.\nTry to login instead i.e. /sm Login";
            }
        }

        #endregion

        #region "Location Methods"

        public static string GetLocationDetails(string locationID)
        {
            // Variable for the return string
            string returnString = "";

            // Get the right path, and work out if the file exists.
            string path = FilePathSystem.GetFilePath("Locations", "Loc" + locationID);

            // Check if the character exists..
            if (!File.Exists(path))
            {
                // If they don't exist inform the person as to how to create a new user
                returnString = "Location does not exist?  Please report this as an error to hutsonphutty+SlackMud@gmail.com";
            }
            else
            {
                // Load the room details
                SMRoom smr = new SMRoom();

                // Use a stream reader to read the file in (based on the path)
                using (StreamReader r = new StreamReader(path))
                {
                    // Create a new JSON string to be used...
                    string json = r.ReadToEnd();

                    // ... get the informaiton from the the room information.
                    smr = JsonConvert.DeserializeObject<SMRoom>(json);

                    // Return the room description, exits, people and objects 
                    returnString = ConstructLocationInformation(smr, locationID);
                }
            }

            // Return the text output
            return returnString;
        }

        /// <summary>
        /// Internal Method to create a room decription, created as it's going to be used over and over...
        /// </summary>
        /// <param name="smr">An SMRoom</param>
        /// <returns>String including a full location string</returns>
        private static string ConstructLocationInformation(SMRoom smr, String locationID)
        {
            // Construct the room string.
            // Create the string and add the basic room description.
            string returnString = smr.RoomDescription;

            // Add the people within the location
            // Search through logged in users to see which are in this location
            List<SMCharacter> smcs = new List<SMCharacter>();
            smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];

            // Check if the character already exists or not.
            if (smcs != null)
            {
                string smcsNames = "";
                if (smcs.Count(smc => smc.RoomID == locationID) > 0)
                {
                    returnString += "\n\nPeople: ";
                    List<SMCharacter> charsInLocation = new List<SMCharacter>();
                    charsInLocation = smcs.FindAll(s => s.RoomID == locationID);
                    var counted = 0;
                    foreach (SMCharacter sma in charsInLocation)
                    {
                        if (counted == 0)
                        {
                            returnString += sma.FirstName + " " + sma.LastName;
                        }
                        else
                        {
                            returnString += ", " + sma.FirstName + " " + sma.LastName;
                        }
                    }
                }
            }

            // Add the exits to the room so that someone can leave.
            returnString += "\n\nExits: " + smr.RoomExits;

            // Show all the items within the room that can be returned.
            returnString += "\n\nItems: TODO";

            // Return the string to the calling method.
            return returnString;
        }

        #endregion

        #region "Item Methods"

        /// <summary>
        /// Internal Method to create a new item with data from an Objects json file
        /// </summary>
        /// <returns>The item.</returns>
        /// <param name="fileName">File name of the objects json file.</param>
        private static SMItem CreateItemFromJson(string fileName)
        {
            string path = FilePathSystem.GetFilePath("Objects", fileName);

            SMItem item = new SMItem();

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    item = JsonConvert.DeserializeObject<SMItem>(json);
                    item.ItemId = Guid.NewGuid().ToString();
                }
            }

            return item;
        }

        #endregion

    }
}