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
        [JsonProperty("CharacterName")]
        public string CharacterName { get; set; }

        [JsonProperty("AccountReference")]
        public string AccountReference { get; set; }

        [JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("UserName")]
        public string UserName { get; set; }

        [JsonProperty("HashedPassword")]
        public string HashedPassword { get; set; }

        [JsonProperty("OtherReference")]
        public List<SMAccountOtherReference> OtherReference { get; set; }
    }

    public class SMAccountOtherReference
    {
        [JsonProperty("OtherReference")]
        public string OtherReference { get; set; }
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
            List<SMAccount> allCharacters = new List<SMAccount>();

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    allCharacters = JsonConvert.DeserializeObject<List<SMAccount>>(json);
                }
            }

            // Select the name from the list
            SMAccount smcn = allCharacters.FirstOrDefault(c => c.CharacterName.ToLower() == fullCharName.ToLower());

            // if the name doesn't exist...
            if (smcn == null)
            {
                // Load the char names list into memory
                path = FilePathSystem.GetFilePath("NPCs", "NPCNamesList");
                List<SMAccount> npcCharacters = new List<SMAccount>();

                if (File.Exists(path))
                {
                    using (StreamReader r = new StreamReader(path))
                    {
                        string json = r.ReadToEnd();
                        npcCharacters = JsonConvert.DeserializeObject<List<SMAccount>>(json);
                    }
                }

                // Select the name from the list
                SMAccount npccn = npcCharacters.FirstOrDefault(c => c.CharacterName.ToLower() == fullCharName.ToLower());

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
                return false; // .. the name can't be used
            }
        }

        /// <summary>
        /// Checks that a username / password combination is valid
        /// </summary>
        /// <param name="username">The username being checked</param>
        /// <param name="password">The password being checked</param>
        /// <returns></returns>
        public bool CheckUserName(string username)
        {
            // Load the char names list into memory
            string path = FilePathSystem.GetFilePath("Characters", "CharNamesList");
            List<SMAccount> allCharacters = new List<SMAccount>();

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    allCharacters = JsonConvert.DeserializeObject<List<SMAccount>>(json);
                }
            }

            // Select the name from the list
            SMAccount smcn = allCharacters.FirstOrDefault(c => (c.UserName == username));

            // if the name doesn't exist...
            if (smcn == null)
            {
                return true; // .. the name can be used
            }
            else
            {
                return false; // .. the name can't be used
            }
        }

        /// <summary>
        /// Get the account reference
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <param name="password">The password to check</param>
        /// <returns></returns>
        public string GetAccountReference(string username, string password)
        {
            // Load the char names list into memory
            string path = FilePathSystem.GetFilePath("Characters", "CharNamesList");
            List<SMAccount> allCharacters = new List<SMAccount>();

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    allCharacters = JsonConvert.DeserializeObject<List<SMAccount>>(json);
                }
            }

            // Select the name from the list
            SMAccount smcn = allCharacters.FirstOrDefault(c => ((c.UserName == username) && (Utility.Crypto.DecryptStringAES(c.HashedPassword) == password)));

            // if the username / password is correct
            if (smcn != null)
            {
                return smcn.AccountReference; // .. return the account reference.
            }
            else
            {
                return ""; // .. return nothing
            }
        }

        /// <summary>
        /// Adds the name to the CharNamesList
        /// </summary>
        /// <param name="fullCharName">The full name of the character being added</param>
        /// <returns></returns>
        public void AddNameToList(string fullCharName, string accountReference, SMCharacter smc = null)
        {
            // Load the char names list into memory
            string path = FilePathSystem.GetFilePath("Characters", "CharNamesList");
            List<SMAccount> allCharacters = new List<SMAccount>();
            
            // Check if the file exists at the path.
            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    allCharacters = JsonConvert.DeserializeObject<List<SMAccount>>(json);
                }
            }

            // Create a new character reference
            SMAccount smcn = new SMAccount();
            smcn.CharacterName = fullCharName;
            smcn.AccountReference = accountReference;
            // If the character is passed in...
            if (smc != null)
            {
                // ... check if there is a username and password associated
                if ((smc.Username != null) && (smc.Password != null))
                {
                    // Assigned the username to the email address
                    smcn.UserName = smc.Username;
                    smcn.HashedPassword = smc.Password;
                }
            }

            // Check if the account reference is already in the list
            SMAccount sma = allCharacters.FirstOrDefault(c => c.AccountReference == accountReference);

            // If it does exist...
            if (sma != null)
            {
                // ... remove the old version
                allCharacters.Remove(sma);
            } 

            // Add the character to the list
            allCharacters.Add(smcn);
            
            // Save the list to disk.
            string listJSON = JsonConvert.SerializeObject(allCharacters, Formatting.Indented);

            using (StreamWriter w = new StreamWriter(path))
            {
                w.WriteLine(listJSON);
            }
        }

        /// <summary>
        /// Update the user details of the character to the character names file
        /// This provides easy reference to the username / password when logging in
        /// on other platforms.
        /// </summary>
        /// <param name="smc">The character being used</param>
        public void UpdateUserDetails(SMCharacter smc)
        {
            // Load the char names list into memory
            string path = FilePathSystem.GetFilePath("Characters", "CharNamesList");
            List<SMAccount> allCharacters = new List<SMAccount>();

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    allCharacters = JsonConvert.DeserializeObject<List<SMAccount>>(json);
                }
            }

            // Find the relevant one
            SMAccount smcn = allCharacters.FirstOrDefault(c => c.CharacterName.ToLower() == smc.GetFullName().ToLower());

            // remove the existing one from the memory
            allCharacters.Remove(smcn);

            // Check if we found something, if we didn't we might have an issue..
            if (smcn != null)
            {
                smcn.EmailAddress = smc.Username;
                smcn.HashedPassword = smc.Password;
            }

            // Now save that back out.
            allCharacters.Add(smcn);

            string listJSON = JsonConvert.SerializeObject(allCharacters, Formatting.Indented);

            using (StreamWriter w = new StreamWriter(path))
            {
                w.WriteLine(listJSON);
            }
        }

        public string LogIn(string usernameIn, string passwordIn, string connectionAddress)
        {
            // Load the char names list into memory
            string path = FilePathSystem.GetFilePath("Characters", "CharNamesList");
            List<SMAccount> allCharacters = new List<SMAccount>();

            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    allCharacters = JsonConvert.DeserializeObject<List<SMAccount>>(json);
                }
            }

            // Find the relevant one
            SMAccount smcn = allCharacters.FirstOrDefault(c => ((c.EmailAddress == usernameIn) && (c.HashedPassword == Utility.Crypto.DecryptStringAES(passwordIn))));

            // Check if we've found an account
            if (smcn != null)
            {
                // Check if the current connection address is the account reference,
                // if not add it to the list so we can check when connecting so we 
                // can load the correct account in without the username / password
                // being needed each time.
                if (connectionAddress != smcn.AccountReference)
                {
                    // remove the current char reference from the file
                    allCharacters.Remove(smcn);

                    // create a new char reference element.
                    SMAccountOtherReference smaor = new SMAccountOtherReference();
                    smaor.OtherReference = smcn.AccountReference;

                    // Add the character back to the list with the new reference
                    allCharacters.Add(smcn);

                    // Save the file back to the disk
                    string listJSON = JsonConvert.SerializeObject(allCharacters, Formatting.Indented);

                    using (StreamWriter w = new StreamWriter(path))
                    {
                        w.WriteLine(listJSON);
                    }
                }

                // return the main account reference
                return smcn.AccountReference;
            }
            else 
            {
                // return nothing
                return "";
            }

            
        }
    }
}