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
        public static string Login(string userID)
        {
            string returnString = "";

            string path = HttpContext.Current.Server.MapPath("~/JSON/Characters/Char" + userID + ".json");
            if (!File.Exists(path))
            {
                returnString = "You must create a character, to do so, use the command /sm CreateCharacter FIRSTNAME,LASTNAME,SEX,AGE\n";
                returnString += "i.e. /sm CreateCharacter Paul,Hutson,m,34";
            }
            else
            {
                returnString = GetCharacter(userID);
            }

            return returnString;
        }

        /// <summary>
        /// Gets a character object, and loads it into memory.
        /// </summary>
        /// <param name="userID">userID is based on the id from the slack channel</param>
        /// <returns>String message for usage</returns>
        public static string GetCharacter(string userID)
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
                return "Welcome back " + SMChar.FirstName;
            }
            else
            {
                return "You do not have a character yet, you need to create one...";
            }
        }

        public static string CreateCharacter(string userID, string firstName, string lastName, int age, char sexIn)
        {
            SMCharacter SMChar = new SMCharacter();
            SMChar.FirstName = firstName;
            SMChar.LastName = lastName;
            SMChar.LastLogindate = DateTime.Now;
            SMChar.LastInteractionDate = DateTime.Now;
            SMChar.RoomLocation = "0";
            SMChar.PKFlag = false;
            SMChar.Sex = sexIn;

            var SMCharJSON = JsonConvert.SerializeObject(SMChar);

            string path = HttpContext.Current.Server.MapPath("~/JSON/Characters/Char" + userID + ".json");
            if (!File.Exists(path))
            {

                using (StreamWriter w = new StreamWriter(path, true))
                {
                    w.WriteLine(SMCharJSON); // Write the text
                }

                return "Character Created";
            }
            else if (File.Exists(path))
            {
                return "You already have a character, you can not create another.";
            }

            return "Error";
        }
    }
}