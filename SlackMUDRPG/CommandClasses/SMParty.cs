using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    /// <summary>
    /// Party functionality
    /// </summary>
    public class SMParty
    {
        public string PartyID { get; set; }
        public List<SMPartyMember> PartyMembers { get; set; }

        /// <summary>
        /// A new party creation
        /// </summary>
        /// <param name="invokingCharacter">The character creating the party.</param>
        public void CreateParty(SMCharacter invokingCharacter, bool suppressMessage = false)
        {
            if ((invokingCharacter.PartyReference == null) || (invokingCharacter.PartyReference.Status == "Invited"))
            {
                // Create a new id
                PartyID = Guid.NewGuid().ToString();

                // Add the invoking character to the party
                SMPartyMember smpm = new SMPartyMember();
                smpm.CharacterName = invokingCharacter.GetFullName();
                smpm.UserID = invokingCharacter.UserID;
                PartyMembers.Add(smpm);

                // Add the party to the global memory
                List<SMParty> smp = (List<SMParty>)HttpContext.Current.Application["Parties"];
                smp.Add(this);

                // Save the party to the memory.
                HttpContext.Current.Application["Parties"] = smp;

                // Reference the new party against the character
                invokingCharacter.PartyReference = new SMPartyReference(this.PartyID, "Leader");

                if (!suppressMessage)
                {
                    invokingCharacter.sendMessageToPlayer("[i]Party created.[/i]");
                }
            }
            else // They are already in a party
            {
                if (!suppressMessage)
                {
                    invokingCharacter.sendMessageToPlayer("[i]Can not create a party as you're already in one.[/i]");
                }
            }
        }

        /// <summary>
        /// Invite someone to a party.
        /// </summary>
        /// <param name="invokingCharacter"></param>
        /// <param name="characterName"></param>
        /// <param name="suppressMessages"></param>
        public void InviteToParty(SMCharacter invokingCharacter, string characterName, bool suppressMessages = false)
        {
            // See if the person being invited exists in memory
            SMCharacter smc = ((List<SMCharacter>)HttpContext.Current.Application["SMCharacters"]).FirstOrDefault(ch => ch.GetFullName() == characterName);

            // check that the player exists...
            if (smc != null)
            {
                // Check that they're not already in a party
                if ((smc.PartyReference == null))
                {
                    // For use later
                    bool canJoinParty = true;

                    // Check the invoking player is in a party... and they're the leader
                    if (invokingCharacter.PartyReference != null)
                    {
                        if (invokingCharacter.PartyReference.Status != "Leader")
                        {
                            canJoinParty = false;
                            if (!suppressMessages)
                            {
                                invokingCharacter.sendMessageToPlayer("[i]You are not the party leader so can not invite someone[/i]");
                            }
                        }
                    }
                    else // The player inviting does't have a party yet.. 
                    {
                        // .. so lets create it.
                        new SMParty().CreateParty(invokingCharacter, true);
                    }

                    // If the player can join the party...
                    if (canJoinParty)
                    {
                        if (!suppressMessages)
                        {
                            // Send a message to the player being invited...
                            smc.sendMessageToPlayer("[i]You have been invited to a party by " + invokingCharacter.GetFullName() + " - to accept type \"AcceptInvite\"[/i]");
                        }

                        // Add the SMPartyReference to the character
                        smc.PartyReference = new SMPartyReference(invokingCharacter.PartyReference.PartyID, "invited");
                    }
                }
                else
                {
                    if (!suppressMessages)
                    {
                        invokingCharacter.sendMessageToPlayer("[i]Can not invite that character to a party as they are already in a party.[/i]");
                    }
                }
            }
            else
            {
                if (!suppressMessages)
                {
                    invokingCharacter.sendMessageToPlayer("[i]Can not find a character named + \"" + characterName + "\"[/i]");
                }
            }
        }

        public void JoinParty(SMCharacter invokingCharacter, bool suppressMessages = false)
        {
            // Check the character has an open invite
            if ((invokingCharacter.PartyReference != null) && (invokingCharacter.PartyReference.Status == "Invited"))
            {
                // Find the party in the list of parties
                List<SMParty> smp = (List<SMParty>)HttpContext.Current.Application["Parties"];

                // Find the relevant party
                SMParty sp = smp.FirstOrDefault(p => p.PartyID == invokingCharacter.PartyReference.PartyID);

                // If it exists...
                if (sp != null)
                {
                    // Remove the party from the global list for a mo..
                    smp.Remove(sp);

                    // ... add the character to the party.
                    SMPartyMember smpm = new SMPartyMember();
                    smpm.CharacterName = invokingCharacter.GetFullName();
                    smpm.UserID = invokingCharacter.UserID;
                    sp.PartyMembers.Add(smpm);

                    // Add the party to the list again..
                    smp.Add(sp);

                    // .. and save the list out
                    HttpContext.Current.Application["Parties"] = smp;

                    // ... send a message to all the people in the party.
                    sp.SendMessageToAllPartyMembers(sp, "[i]{playercharacter} joined the party[/i]");

                    // ... change the status of the party element on the character to "joined"
                    invokingCharacter.PartyReference.Status = "joined";
                }
                else // .. the party no longer exists, so can't be joined
                {
                    // Tell the player
                    invokingCharacter.sendMessageToPlayer("[i]That party no longer exists.[/i]");

                    // Remove the reference from their party invite list.
                    invokingCharacter.PartyReference = null;
                }
            }
            else // No party
            {
                invokingCharacter.sendMessageToPlayer("[i]You have no open party invites.[/i]");
            }
        }

        public void LeaveParty(SMCharacter invokingCharacter, bool suppressMessages = false)
        {
            // Find the current party if they have one (and it's not just at "invited" stage).
            if ((invokingCharacter.PartyReference == null) || (invokingCharacter.PartyReference.Status == "Invited"))
            {
                // Remove them from the party reference.
                // Get the list of parties
                List<SMParty> smp = (List<SMParty>)HttpContext.Current.Application["Parties"];
                
                // Find the relevant party
                SMParty sp = smp.FirstOrDefault(p => p.PartyID == invokingCharacter.PartyReference.PartyID);

                if (!suppressMessages)
                {
                    sp.SendMessageToAllPartyMembers(sp, "[i]{playercharacter} left the party[/i]");
                }
                
                // Remove the party from the global list element.
                smp.Remove(sp);

                // Find the relevant member
                SMPartyMember pm = sp.PartyMembers.FirstOrDefault(p => p.CharacterName == invokingCharacter.GetFullName());
                sp.PartyMembers.Remove(pm);

                // Check there are still people in the party
                if (sp.PartyMembers.Count > 0) 
                {
                    // Add the member back to the list
                    smp.Add(sp);
                }
                
                // Save it out to the global list again
                HttpContext.Current.Application["Parties"] = smp;

                // Remove the party reference from the character file
                invokingCharacter.PartyReference = null;
            }
            else
            {
                invokingCharacter.sendMessageToPlayer("[i]You are not in a party so can't leave one[/i]");
            }
        }

        public void SendMessageToAllPartyMembers(SMParty sp, string messageToSend)
        {
            // Send a message to all the party members to let them know that the person has left.
            foreach (SMPartyMember smpm in sp.PartyMembers)
            {
                // Find the character by their id
                SMCharacter smc = new SlackMud().GetCharacter(smpm.UserID);

                // If the character exists...
                if (smc != null)
                {
                    // Replace the player character element with the actual character name
                    messageToSend = messageToSend.Replace("{playercharacter", smpm.CharacterName);

                    // ... Send the message to the player
                    smc.sendMessageToPlayer(messageToSend);
                }
            }
        }

        public void FindAllPartyMembers(SMCharacter invokingCharacter)
        {
            string stringToSend = "[b]Party Members:[/b] ";
            bool first = true;

            foreach(SMPartyMember pm in this.PartyMembers)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    stringToSend += ", ";
                }
                stringToSend += pm.CharacterName;
            }

            invokingCharacter.sendMessageToPlayer(stringToSend);
        }
    }

    /// <summary>
    /// Helper methods for parties
    /// </summary>
    public static class SMPartyHelper
    {
        /// <summary>
        /// Get a party for use
        /// </summary>
        /// <param name="partyID">The party id that you want to find the party of.</param>
        /// <returns></returns>
        public static SMParty GetParty(string partyID)
        {
            List<SMParty> smp = (List<SMParty>)HttpContext.Current.Application["Parties"];
            SMParty sp = smp.FirstOrDefault(p => p.PartyID == partyID);

            return sp;
        }
    }

    /// <summary>
    /// Characters within a party
    /// </summary>
    public class SMPartyMember
    {
        public string CharacterName { get; set; }
        public string UserID { get; set; }
    }

    /// <summary>
    /// Used for invites and attaching to a character
    /// </summary>
    public class SMPartyReference
    {
        public string PartyID { get; set; }
        public string Status { get; set; }

        public SMPartyReference(string partyID, string status)
        {
            this.PartyID = partyID;
            this.Status = status;
        }
    }
}