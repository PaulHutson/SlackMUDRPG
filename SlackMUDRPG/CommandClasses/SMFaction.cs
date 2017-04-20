using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    public class SMFaction
    {
        [JsonProperty("FactionName")]
        public string FactionName { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }

        public SMFaction(string factionName, int level)
        {
            FactionName = factionName;
            Level = level;
        }
    }

    public static class SMFactionHelper
    {
        /// <summary>
        /// Get the faction level for character for a specific faction
        /// </summary>
        /// <param name="smc">The Character</param>
        /// <param name="factionName">The Faction Name</param>
        /// <returns></returns>
        public static int GetFactionAmount(SMCharacter smc, string factionName)
        {
            // Find the faction information from the player
            SMFaction smf = GetFactionFromPlayerList(smc, factionName);

            // Check if it's null..
            if (smf != null)
            {
                // .. if it's not return the level
                return smf.Level;
            }
            else // there isn't a faction yet
            {
                // return 0
                return 0;
            }
        }

        /// <summary>
        /// Increase the faction level for a character
        /// </summary>
        /// <param name="smc">The Character</param>
        /// <param name="factionName">The Faction Name</param>
        /// <param name="amount">The amount to increase</param>
        /// <returns></returns>
        public static void IncreaseFactionLevel(SMCharacter smc, string factionName, int amount)
        {
            // Find the faction information from the player
            SMFaction smf = GetFactionFromPlayerList(smc, factionName);

            // Check if it's null..
            if (smf != null)
            {
                smc.Factions.Remove(smf);
                smf.Level += amount;
            }
            else
            {
                smf = new SMFaction(factionName, amount);
                if (smc.Factions == null)
                {
                    smc.Factions = new List<SMFaction>();
                }
            }

            smc.Factions.Add(smf);

            smc.sendMessageToPlayer("[i]" + smf.FactionName + " standing increased by " + amount + " to " + smf.Level + "[/i]");

            smc.SaveToApplication();
            smc.SaveToFile();
        }

        /// <summary>
        /// Decrease the faction level for a character
        /// </summary>
        /// <param name="smc">The Character</param>
        /// <param name="factionName">The Faction Name</param>
        /// <param name="amount">The amount to decrease</param>
        /// <returns></returns>
        public static void DecreaseFactionLevel(SMCharacter smc, string factionName, int amount)
        {
            // Find the faction information from the player
            SMFaction smf = GetFactionFromPlayerList(smc, factionName);

            // Check if it's null..
            if (smf != null)
            {
                smc.Factions.Remove(smf);
                smf.Level -= amount;
            }
            else
            {
                smf = new SMFaction(factionName, (0 - amount));
                if (smc.Factions == null)
                {
                    smc.Factions = new List<SMFaction>();
                }
            }

            smc.Factions.Add(smf);

            smc.sendMessageToPlayer("[i]" + smf.FactionName + " standing decreased by " + amount + " to " + smf.Level + "[/i]");

            smc.SaveToApplication();
            smc.SaveToFile();
        }

        /// <summary>
        /// Get a faction reference from a Character
        /// </summary>
        /// <param name="smc">The Character</param>
        /// <param name="factionName">The name of the faction to check</param>
        /// <returns></returns>
        public static SMFaction GetFactionFromPlayerList(SMCharacter smc, string factionName)
        {
            // Check if the player has any factions at all
            if (smc.Factions != null)
            {
                // Find the faction
                SMFaction smf = smc.Factions.FirstOrDefault(f => f.FactionName.ToLower() == factionName.ToLower());

                // If there is a faction...
                if (smf != null)
                {
                    // .. return it
                    return smf;
                }
                else // No faction found
                {
                    return null;
                }
            }
            else // No factions at all on the player
            {
                return null;
            }
        }
    }
}