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
		public static string Login(string userID, bool newCharacter = false, string responseURL = null, string connectionService = "slack")
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
				List<SMCharacter> smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];
				SMCharacter character = smcs.FirstOrDefault(smc => smc.UserID == userID);

				if ((character != null) && (!newCharacter))
				{
					return "You're already logged in!";
				}
				else
				{
					// Get the character
					character = GetCharacter(userID);

                    // Set the response URL of the character
                    if (responseURL != null)
                    {
                        character.ResponseURL = responseURL;
                    }

					SMRoom room = character.GetRoom();
					if (room != null)
					{
						// Announce someone has walked into the room.
						room.Announce("_" + character.GetFullName() + " walks in._");
					}

					if (!newCharacter)
					{
						returnString = "Welcome back " + character.FirstName + " " + character.LastName + "\n";
					}
					else
					{
						returnString = "Welcome to SlackMud!\n";
						returnString += "We've created your character in the magical world of Arrelvia!\n"; // TODO, use a welcome script!
					}
					returnString += GetLocationDetails(character.RoomID, character.UserID);
				}
			}

			// Return the text output
			return returnString;
		}

		/// <summary>
		/// Gets a character and also loads the character to memory if it isn't already there.
		/// </summary>
		/// <param name="userID">The id of the character you want to load</param>
		/// <returns>A character</returns>
		public static SMCharacter GetCharacter(string userID)
		{
			// Get the room file if it exists
			List<SMCharacter> smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];
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

						// Add the character to the application memory 
						smcs.Add(charInMem);
						HttpContext.Current.Application["SMCharacters"] = smcs;
					}
				}
			}

			return charInMem;
		}

		/// <summary>
		/// Gets a character object, and loads it into memory.
		/// </summary>
		/// <param name="userID">userID is based on the id from the slack channel</param>
		/// <param name="newCharacter">newCharacter to change the output of the text based on whether the character is new or not</param>
		/// <returns>String message for usage</returns>
		public static string GetCharacterOLD(string userID, bool newCharacter = false)
		{
			List<SMCharacter> smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];
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
		public static string CreateCharacter(string userID, string firstName, string lastName, int age, char sexIn, string characterType = "BaseCharacter")
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
				SMChar.Sex = sexIn;

				// Add default attributes to the character
				SMChar.Attributes = CreateBaseAttributesFromJson("Attribute." + characterType);

				// Set default character slots before adding items to them
				SMChar.CharacterSlots = CreateCharacterSlotsFromJSON("Slots." + characterType);

				// Add default items to the character
				SMCharacterSlot rightHand = SMChar.GetSlotByName("RightHand");
				rightHand.EquippedItem = CreateItemFromJson("Weapons.WoodenSword");

				SMCharacterSlot back = SMChar.GetSlotByName("Back");
				back.EquippedItem = CreateItemFromJson("Containers.SmallBackpack");

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

				// log the newly created character into the game
				return Login(userID, true);
			}
			else
			{
				// If they already have a character tell them they do and that they need to login.
				return "You already have a character, you cannot create another.\nTry to login instead i.e. /sm Login";
			}
		}

		#endregion

		#region "Location Methods"

		/// <summary>
		/// Gets a room and also loads the room to memory if it isn't already there.
		/// </summary>
		/// <param name="roomID">The id of the location you want to load</param>
		/// <returns>A room</returns>
		public static SMRoom GetRoom(string roomID)
		{
			// Get the room file if it exists
			List<SMRoom> smrs = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];
			SMRoom roomInMem = smrs.FirstOrDefault(smr => smr.RoomID == roomID);

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
					}
				}
			}

			return roomInMem;
		}

		public static string GetLocationDetails(string roomID, string userID = "0")
		{
			// Variable for the return string
			string returnString = "";

			// Get the room from memory
			SMRoom smr = GetRoom(roomID);

			// Check if the character exists..
			if (smr == null)
			{
				// If they don't exist inform the person as to how to create a new user
				returnString = "Location does not exist?  Please report this as an error to hutsonphutty+SlackMud@gmail.com";
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

		private static List<SMCharacterSlot> CreateCharacterSlotsFromJSON(string filename)
		{
			string path = FilePathSystem.GetFilePath("Misc", filename);

			List<SMCharacterSlot> slots = new List<SMCharacterSlot>();

			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					string json = r.ReadToEnd();
					slots = JsonConvert.DeserializeObject<List<SMCharacterSlot>>(json);
				}
			}

			return slots;
		}

		#endregion

		#region "Item Methods"

		/// <summary>
		/// Internal Method to create a new item with data from an Objects json file
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="fileName">File name of the objects json file.</param>
		public static SMItem CreateItemFromJson(string fileName)
		{
			string path = FilePathSystem.GetFilePath("Objects", fileName);

			SMItem item = new SMItem();

			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					string json = r.ReadToEnd();
					item = JsonConvert.DeserializeObject<SMItem>(json);
					item.ItemID = Guid.NewGuid().ToString();
				}
			}

			return item;
		}

		#endregion

		#region "Attribute Methods"

		/// <summary>
		/// Internal Method to get base attributes from a file
		/// </summary>
		/// <returns>An Attribues list</returns>
		/// <param name="fileName">File name of the objects json file.</param>
		private static SMAttributes CreateBaseAttributesFromJson(string fileName)
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

		public static void GlobalAnnounce(string msg)
		{
			// Get all the characters currently logged in
			List<SMCharacter> smcl = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];

			// Send a message to each of them.
			foreach (SMCharacter smc in smcl)
			{
				smc.Announce("*GLOBAL MESSAGE: " + msg + "*");
			}
		}

		#endregion
	}
}