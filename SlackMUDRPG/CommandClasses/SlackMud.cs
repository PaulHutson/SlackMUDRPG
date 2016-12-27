using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public static class SlackMud
    {
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
            string path = HttpContext.Current.Server.MapPath("~/JSON/Characters/Char" + userID + ".json");
            
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
            // Set the path to look for the character information.
            string path = HttpContext.Current.Server.MapPath("~/JSON/Characters/Char" + userID + ".json");
            
            // Check if the file exists.
            if (File.Exists(path))
            {
                // Get the Character
                SMCharacter SMChar = new SMCharacter();

                // Use a stream reader to read the file in (based on the path)
                using (StreamReader r = new StreamReader(path))
                {
                    // Create a new JSON string to be used...
                    string json = r.ReadToEnd();

                    // ... use the json string we've just gotten to create a new character
                    SMChar = JsonConvert.DeserializeObject<SMCharacter>(json);
                }

                // Add the character to the application memory (so it's accessible to everyone sending commands, etc).
                // Get the list of existing characters
                List<SMCharacter> smcs = new List<SMCharacter>();
                smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];
                
                // Check if the character already exists or not.
                if (smcs != null) { 
                    if (smcs.FirstOrDefault(smc => smc.FirstName == SMChar.FirstName) == null)
                    {
                        // If it doesn't, add it to the character list.
                        smcs.Add(SMChar);
                        HttpContext.Current.Application["SMCharacters"] = smcs;
                    }
                }

                // return a welcome!
                string returnString = "";
                if (!newCharacter)
                {
                    returnString = "Welcome back " + SMChar.FirstName;
                } else
                {
                    returnString = "Welcome to SlackMud!\n";
                    returnString += "We've created your character in the magical world of Arrelvia!"; // TO DO, use a welcome script!
                    // TO DO, get room details
                }
                return returnString;
            }
            else
            {
                // If the UserID doesn't have a character already, inform them that they need to create one.
                return "You do not have a character yet, you need to create one...";
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
            SMChar.FirstName = firstName;
            SMChar.LastName = lastName;
            SMChar.LastLogindate = DateTime.Now;
            SMChar.LastInteractionDate = DateTime.Now;
            SMChar.RoomLocation = "0";  // NEED TO ADD THE LOCATION FROM THE SCRIPTS
            SMChar.PKFlag = false;
            SMChar.Sex = sexIn;

            // Create the JSON object from the new SMCharacter object
            var SMCharJSON = JsonConvert.SerializeObject(SMChar);

            // Get the path for the character
            string path = HttpContext.Current.Server.MapPath("~/JSON/Characters/Char" + userID + ".json");

            // If the file doesn't exist i.e. the character doesn't exist
            if (!File.Exists(path))
            {
                // Write the character to the stream
                using (StreamWriter w = new StreamWriter(path, true))
                {
                    w.WriteLine(SMCharJSON); // Write the text
                }

                // log the newly created character into the game
                return GetCharacter(userID, true);
            }
            else
            {
                // If they already have a character tell them they do and that they need to login.
                return "You already have a character, you can not create another.\nTry to login instead i.e. /sm Login";
            }
        }
    }
}