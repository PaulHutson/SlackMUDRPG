using Newtonsoft.Json;
using SlackMUDRPG.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    public class SMAccount
    {
        [JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("HashedPassword")]
        public string HashedPassword { get; set; }

        [JsonProperty("AccountReference")]
        public string AccountReference { get; set; }
    }

    public class SMCharacterName
    {
        [JsonProperty("CharacterName")]
        public string CharacterName { get; set; }

        [JsonProperty("AccountReference")]
        public string AccountReference { get; set; }
    }

    public class SMAccountHelper
    {
        /// <summary>
        /// Checks that a character name can be used (i.e. hasn't already been used).
        /// </summary>
        /// <param name="fullCharName">The full name of the character being checked</param>
        /// <returns></returns>
        public bool CheckCharNameCanBeUsed(string fullCharName)
        {
            // Load the char names list into memory
            string path = FilePathSystem.GetFilePath("Characters", "CharNamesList");
            List<SMCharacterName> allCharacters = new List<SMCharacterName>();

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    allCharacters = JsonConvert.DeserializeObject<List<SMCharacterName>>(json);
                }
            }

            // Select the name from the list
            SMCharacterName smcn = allCharacters.FirstOrDefault(c => c.CharacterName.ToLower() == fullCharName.ToLower());

            // if the name doesn't exist...
            if (smcn == null)
            {
                // Load the char names list into memory
                path = FilePathSystem.GetFilePath("NPCs", "NPCNamesList");
                List<SMCharacterName> npcCharacters = new List<SMCharacterName>();

                if (File.Exists(path))
                {
                    using (StreamReader r = new StreamReader(path))
                    {
                        string json = r.ReadToEnd();
                        npcCharacters = JsonConvert.DeserializeObject<List<SMCharacterName>>(json);
                    }
                }

                // Select the name from the list
                SMCharacterName npccn = npcCharacters.FirstOrDefault(c => c.CharacterName.ToLower() == fullCharName.ToLower());

                if (npccn == null)
                {
                    return true; // .. the name can be used
                }
                else
                {
                    return false; // can't use the name as an NPC has it already.
                }
            }
            else 
            {
                return false; // .. the name can't be used as a player has used it.
            }
        }

        /// <summary>
        /// Adds the name to the CharNamesList
        /// </summary>
        /// <param name="fullCharName">The full name of the character being added</param>
        /// <returns></returns>
        public void AddNameToList(string fullCharName, string accountReference)
        {
            // Load the char names list into memory
            string path = FilePathSystem.GetFilePath("Characters", "CharNamesList");
            List<SMCharacterName> allCharacters = new List<SMCharacterName>();

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    allCharacters = JsonConvert.DeserializeObject<List<SMCharacterName>>(json);
                }
            }

            // Create a new character reference
            SMCharacterName smcn = new SMCharacterName();
            smcn.CharacterName = fullCharName;
            smcn.AccountReference = accountReference;

            // Add the character to the list
            allCharacters.Add(smcn);

            // Save the list to disk.
            string listJSON = JsonConvert.SerializeObject(allCharacters, Formatting.Indented);

            using (StreamWriter w = new StreamWriter(path))
            {
                w.WriteLine(listJSON);
            }
        }
    }
}