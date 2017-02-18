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
	public class SlackMud
	{
		#region "Login and Character Methods"

		/// <summary>
		/// Logs someone in with the Slack UserID
		/// </summary>
		/// <param name="userID">Slack UserID</param>
		/// <returns>A string response</returns>
		public string Login(string userID, bool newCharacter = false, string responseURL = null, string connectionService = "slack", string password = null)
		{
			// Variables for the return string
			string returnString = "";

            // Get all current characters
            List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
            SMCharacter character = smcs.FirstOrDefault(smc => smc.UserID == userID);

            // Get the right path, and work out if the file exists.
            string path = FilePathSystem.GetFilePath("Characters", "Char" + userID);

			// Check if the character exists..
			if (!File.Exists(path))
			{
				// If they don't exist inform the person as to how to create a new user
				returnString = "You must create a character, to do so, use the command /sm CreateCharacter FIRSTNAME,LASTNAME,SEX,AGE\n";
				returnString += "i.e. /sm CreateCharacter Paul,Hutson,m,34";

				// return information [need to return this without a player!]
				return returnString;
            }
			else
			{
				if ((character != null) && (!newCharacter))
				{
					returnString += "You're already logged in!";
					return returnString;
				}
				else
				{
					// Get the character
					character = GetCharacter(userID, password);

                    // Reset the character activity, just in case!
                    character.CurrentActivity = null;

                    // Set the response URL of the character
                    if (responseURL != null)
                    {
                        character.ResponseURL = responseURL;
                    }

					// Set the connection service
					character.ConnectionService = connectionService;

					// Set the last login datetime
					character.LastLogindate = DateTime.Now;
                    
					if (!newCharacter)
					{
						returnString = OutputFormatterFactory.Get().Bold("Welcome back " + character.FirstName + " " + character.LastName + " (you are level " + character.CalculateLevel() + ")");
					}
					else
					{
						returnString = OutputFormatterFactory.Get().Bold("Welcome to SlackMud!");
						returnString += OutputFormatterFactory.Get().General("We've created your character in the magical world of Arrelvia!"); // TODO, use a welcome script!
					}
					returnString += GetLocationDetails(character.RoomID, character.UserID);

					// Clear old responses an quests from the character
					character.ClearQuests();
					character.ClearResponses();

					// Return the text output
					character.sendMessageToPlayer(returnString);
					
                    // Walk the character in
                    SMRoom room = character.GetRoom();
                    if (room != null)
                    {
                        // Announce someone has walked into the room.
                        room.Announce(OutputFormatterFactory.Get().Italic(character.GetFullName() + " walks in."));
                        room.ProcessNPCReactions("PlayerCharacter.Enter", character);
                    }

					return "";
                }
			}
		}

		/// <summary>
		/// Gets a character and also loads the character to memory if it isn't already there.
		/// </summary>
		/// <param name="userID">The id of the character you want to load</param>
		/// <returns>A character</returns>
		public SMCharacter GetCharacter(string userID, string userName = null, string password = null)
		{
			// Get the room file if it exists
			List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
			SMCharacter charInMem = smcs.FirstOrDefault(smc => smc.UserID == userID);

			if (charInMem == null)
			{
				// Get the right path, and work out if the file exists.
				string path = FilePathSystem.GetFilePath("Characters", "Char" + userID);

				// Check if the character exists..
				if (File.Exists(path))
				{
					using (StreamReader r = new StreamReader(path))
					{
						// Get the character from the json string
						string json = r.ReadToEnd();
						charInMem = JsonConvert.DeserializeObject<SMCharacter>(json);

						bool canLogin = true;
						if (password != null)
						{
							canLogin = (Crypto.DecryptStringAES(charInMem.Password, "ProvinceMUD") == password);
						}
						if ((canLogin) && (password != null))
						{
							canLogin = (Crypto.DecryptStringAES(charInMem.Password, "ProvinceMUD") == password);
						}

						if (canLogin)
						{
							// Add the character to the application memory 
							smcs.Add(charInMem);
							HttpContext.Current.Application["SMCharacters"] = smcs;
						}
					}
				}
			}
			return charInMem;
		}

		/// <summary>
		/// Gets a character and also loads the character to memory if it isn't already there.
		/// </summary>
		/// <param name="userID">The id of the character you want to load</param>
		/// <returns>A character</returns>
		public SMNPC GetNPC(string userID)
		{
			// Get the room file if it exists
			return ((List<SMNPC>)HttpContext.Current.Application["SMNPCs"]).FirstOrDefault(smc => smc.UserID == userID);
		}

        /// <summary>
        /// Gets any type of character and also loads the character to memory if it isn't already there.
        /// Note: This returns NPCs as well as Player Characters
        /// </summary>
        /// <param name="userID">The id of the character you want to load</param>
        /// <returns>A character</returns>
        public SMCharacter GetAllCharacters(string userID)
        {
            // Get the room file if it exists
            SMCharacter returnCharacter = ((List<SMNPC>)HttpContext.Current.Application["SMNPCs"]).FirstOrDefault(smc => smc.UserID == userID);

            if (returnCharacter == null)
            {
                returnCharacter = GetCharacter(userID);
            }

            return returnCharacter;
        }

		/// <summary>
		/// Remove an NPC from the memory
		/// </summary>
		/// <param name="charToRemoveID">The id of the NPC to remove</param>
		public void RemoveNPCFromMemory(string charToRemoveID)
		{
			SMNPC npcchar = new SlackMud().GetNPC(charToRemoveID);
			List<SMNPC> npcl = (List<SMNPC>)HttpContext.Current.Application["SMNPCs"];
			npcl.Remove(npcchar);
			HttpContext.Current.Application["SMNPCs"] = npcl;
		}

        /// <summary>
        /// Gets a character object, and loads it into memory.
        /// </summary>
        /// <param name="userID">userID is based on the id from the slack channel</param>
        /// <param name="newCharacter">newCharacter to change the output of the text based on whether the character is new or not</param>
        /// <returns>String message for usage</returns>
        public string GetCharacterOLD(string userID, bool newCharacter = false)
		{
			List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
			SMCharacter character = smcs.FirstOrDefault(smc => smc.UserID == userID);

			if ((character != null) && (!newCharacter))
			{
				return "You're already logged in!";
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
		/// <param name="characterType">M or F for the male / Female character</param>
		/// <returns>A string with the character information</returns>
		public string CreateCharacter(string userID, string firstName, string lastName, string sexIn, string age, string characterType = "BaseCharacter", string responseURL = null, string userName = null, string password = null)
		{
			// Get the path for the character
			string path = FilePathSystem.GetFilePath("Characters", "Char" + userID);

			// If the file doesn't exist i.e. the character doesn't exist
			if (!File.Exists(path))
			{
				// Create the character options
				SMCharacter SMChar = new SMCharacter();
				SMChar.UserID = userID;
				SMChar.FirstName = firstName;
				SMChar.LastName = lastName;
				SMChar.LastLogindate = DateTime.Now;
				SMChar.LastInteractionDate = DateTime.Now;
				SMChar.PKFlag = false;
				SMChar.Sex = char.Parse(sexIn);
				SMChar.Age = int.Parse(age);
				if (userName != null)
				{
					SMChar.Username = userName;
				}
				if (password != null)
				{
					SMChar.Password = Utility.Crypto.EncryptStringAES(password, "ProvinceMud");
				}
				
				// Add default attributes to the character
				SMChar.Attributes = CreateBaseAttributesFromJson("Attribute." + characterType);

				// Set default character slots before adding items to them
				SMChar.Slots = CreateSlotsFromJSON("Slots." + characterType);

				// Add default items to the character
				SMSlot back = SMChar.GetSlotByName("Back");
				back.EquippedItem = SMItemFactory.Get("Container", "SmallBackpack");

				// Add default body parts to the new character
				SMChar.BodyParts = CreateBodyPartsFromJSON("BodyParts." + characterType);

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

						// TODO Add room to memory if not already there.

					}
				}

				// Write the character to the stream
				SMChar.SaveToFile();

				// Check if there is a response URL
				if (responseURL != null)
				{
					// Log the newly created character into the game if in something like Slack
					Login(userID, true, responseURL);
					return "";
				}
				else
				{
					// Return the userid ready for logging in
					return userID;
				}
            }
			else
			{
                // If they already have a character tell them they do and that they need to login.
                // log the newly created character into the game
                // Login(userID, true, responseURL);

                // Get all current characters
                List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
                SMCharacter character = smcs.FirstOrDefault(smc => smc.UserID == userID);

				return "You already have a character, you cannot create another.";
			}   
        }

        public string GetStartingLocation()
        {
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
                    return sl.StartLocation;
                }
            }

            return "1";
        }

		#endregion

		#region "Location Methods"

		/// <summary>
		/// Gets a room and also loads the room to memory if it isn't already there.
		/// </summary>
		/// <param name="roomID">The id of the location you want to load</param>
		/// <returns>A room</returns>
		public SMRoom GetRoom(string roomID)
		{
			// Get the room file if it exists
			List<SMRoom> smrs = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];
			SMRoom roomInMem = smrs.FirstOrDefault(smr => smr.RoomID == roomID);

            // If the room is not already in memory load the room
			if (roomInMem == null)
			{
				// Get the right path, and work out if the file exists.
				string path = FilePathSystem.GetFilePath("Locations", "Loc" + roomID);

				// Check if the character exists..
				if (File.Exists(path))
				{
					// Use a stream reader to read the file in (based on the path)
					using (StreamReader r = new StreamReader(path))
					{
						// Create a new JSON string to be used...
						string json = r.ReadToEnd();

						// ... get the information from the the room information.
						roomInMem = JsonConvert.DeserializeObject<SMRoom>(json);

						// Add the room to the application memory 
						smrs.Add(roomInMem);
						HttpContext.Current.Application["SMRooms"] = smrs;

						// Try to spawn some creatures into the room on first load
						roomInMem.Spawn();
					}
				}
			}

			return roomInMem;
		}

		public string GetLocationDetails(string roomID, string userID = "0")
		{
			// Variable for the return string
			string returnString = "";

			// Get the room from memory
			SMRoom smr = GetRoom(roomID);

			// Check if the character exists..
			if (smr == null)
			{
				// If they don't exist inform the person as to how to create a new user
				returnString = OutputFormatterFactory.Get().Italic("Location does not exist?  Please report this as an error to hutsonphutty+SlackMud@gmail.com");
			}
			else
			{
				// Return the room description, exits, people and objects 
				returnString = smr.GetLocationInformation(userID);
			}

			// Return the text output
			return returnString;
		}

		#endregion

		#region "Slots Methods"

		private List<SMSlot> CreateSlotsFromJSON(string filename)
		{
			string path = FilePathSystem.GetFilePath("Misc", filename);

			List<SMSlot> slots = new List<SMSlot>();

			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					string json = r.ReadToEnd();
					slots = JsonConvert.DeserializeObject<List<SMSlot>>(json);
				}
			}

			return slots;
		}

		#endregion

		#region "BodyPart Methods"

		private List<SMBodyPart> CreateBodyPartsFromJSON(string filename)
		{
			string path = FilePathSystem.GetFilePath("Misc", filename);

			List<SMBodyPart> parts = new List<SMBodyPart>();

			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					string json = r.ReadToEnd();
					parts = JsonConvert.DeserializeObject<List<SMBodyPart>>(json);
				}
			}

			return parts;
		}

		#endregion

		#region "Attribute Methods"

		/// <summary>
		/// Internal Method to get base attributes from a file
		/// </summary>
		/// <returns>An Attribues list</returns>
		/// <param name="fileName">File name of the objects json file.</param>
		private SMAttributes CreateBaseAttributesFromJson(string fileName)
		{
			string path = FilePathSystem.GetFilePath("Misc", fileName);

			SMAttributes attrs = new SMAttributes();

			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					string json = r.ReadToEnd();
					attrs = JsonConvert.DeserializeObject<SMAttributes>(json);
				}
			}

			return attrs;
		}

		#endregion

		#region "ChatMethods"

		public void GlobalAnnounce(string msg)
		{
			BroadcastMessage("*GLOBAL MESSAGE: " + msg + "*");
		}

		public void BroadcastMessage(string msg)
		{
			// Get all the characters currently logged in
			List<SMCharacter> smcl = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];

			// Send a message to each of them.
			foreach (SMCharacter smc in smcl)
			{
				smc.Announce(msg);
			}
		}

		#endregion
	}
}