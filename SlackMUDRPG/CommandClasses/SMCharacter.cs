using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;
using SlackMUDRPG.Utility.Formatters;
using System.Threading;

namespace SlackMUDRPG.CommandClasses
{
	public class SMCharacter
	{
		[JsonProperty("firstname")]
		public string FirstName { get; set; }

		[JsonProperty("lastname")]
		public string LastName { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("notes")]
		public List<PlayerNote> Notes { get; set; }

		[JsonProperty("lastinteractiondate")]
		public DateTime LastInteractionDate { get; set; }

		[JsonProperty("lastlogindate")]
		public DateTime LastLogindate { get; set; }

		[JsonProperty("age")]
		public int Age { get; set; }

		[JsonProperty("sex")]
		public char Sex { get; set; }

		[JsonProperty("PKFlag")]
		public bool PKFlag { get; set; }

		[JsonProperty("UserID")]
		public string UserID { get; set; }

		[JsonProperty("Username")]
		public string Username { get; set; }

		[JsonProperty("Password")]
		public string Password { get; set; }

		[JsonProperty("RoomID")]
		public string RoomID { get; set; }

        [JsonProperty("PartyReference")]
        public SMPartyReference PartyReference { get; set; }

        [JsonProperty("CurrentActivity")]
		public string CurrentActivity { get; set; }

		[JsonProperty("NewbieTipsDisabled")]
		public bool NewbieTipsDisabled { get; set; }

        [JsonProperty("QuestLog")]
        public List<SMQuestStatus> QuestLog { get; set; }

        [JsonProperty("Factions")]
        public List<SMFaction> Factions { get; set; }

        [JsonProperty("Attributes")]
		public SMAttributes Attributes { get; set; }

		[JsonProperty("Skills")]
		public List<SMSkillHeld> Skills { get; set; }

		[JsonProperty("KnownRecipes")]
		public List<string> KnownRecipes { get; set; }

		[JsonProperty("Slots")]
		public List<SMSlot> Slots { get; set; }

        [JsonProperty("BodyParts")]
        public List<SMBodyPart> BodyParts { get; set; }

        [JsonProperty("Currency")]
        public SMCurrency Currency { get; set; }

        /// <summary>
        /// Dynaimic property holding the weight of the character
        /// </summary>
        public int CharacterWeight
		{
			get
			{
				return 40 + this.Attributes.Strength;
			}
		}

		/// <summary>
		/// Dynamic property holding the size of the character
		/// </summary>
		public int CharacterSize
		{
			get
			{
				return 150 + this.Attributes.Strength;
			}
		}

		public string ResponseURL { get; set; }
		public string ConnectionService { get; set; }
		public string LastUsedCommand { get; set; }
		public string VariableResponse { get; set; }
		public List<AwaitingResponseFromCharacter> NPCsWaitingForResponses { get; set; }

		/// <summary>
		/// Holds the class instance of the response formater.
		/// </summary>
		private ResponseFormatter Formatter = null;

		/// <summary>
		/// Class constructor
		/// </summary>
		public SMCharacter()
		{
			this.Formatter = ResponseFormatterFactory.Get();
		}

		/// <summary>
		/// Dynamic property holding the characters weight limit (for carrying items)
		/// </summary>
		private int WeightLimit
		{
			get
			{
				return this.Attributes.Strength * 5;
			}
		}

		#region "General Player Functions"

		/// <summary>
		/// Get the full name of the character.
		/// </summary>
		public string GetFullName()
		{
			return this.FirstName + " " + this.LastName;
		}

		/// <summary>
		/// Saves the character to the file system.
		/// </summary>
		public virtual void SaveToFile()
		{
            string path = FilePathSystem.GetFilePath("Characters", "Char" + this.UserID);
            string charJSON = JsonConvert.SerializeObject(this, Formatting.Indented);

            try
            {
                using (StreamWriter w = new StreamWriter(path))
                {
                    w.WriteLine(charJSON);
                }
            }
            catch
            {
                // do nothing
            }
		}

		/// <summary>
		/// Saves the character to application memory.
		/// </summary>
		public virtual void SaveToApplication()
		{
			List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];

			SMCharacter characterInMem = smcs.FirstOrDefault(smc => smc.UserID == this.UserID);

			if (characterInMem != null)
			{
				smcs.Remove(characterInMem);
			}

			smcs.Add(this);
			HttpContext.Current.Application["SMCharacters"] = smcs;
		}

		/// <summary>
		/// Gets the last command the character used (excludes login).
		/// </summary>
		/// <returns>Command string last used.</returns>
		public string GetLastUsedCommand()
		{
			if (this.LastUsedCommand != null)
			{
				if (!this.LastUsedCommand.ToLower().Contains("login"))
				{
					return this.LastUsedCommand;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the characters current room, loads from mem or file as required
		/// </summary>
		public SMRoom GetRoom()
		{
			return new SlackMud().GetRoom(this.RoomID);
		}

		/// <summary>
		/// Gets the characters current room details,
		/// </summary>
		public void GetRoomDetails()
		{
			this.sendMessageToPlayer(new SlackMud().GetLocationDetails(this.RoomID, this));
		}

        /// <summary>
        /// Invokes a list of all items being sold by NPCs in a room.
        /// </summary>
        public void GetItemsForSaleList()
        {
            this.GetRoom().ProcessNPCReactions("PlayerCharacter.list", this);
        }

        /// <summary>
        /// Gets the total money the characer has available.
        /// </summary>
        public void GetMoney()
        {
            this.sendMessageToPlayer("[i]You currently have [b]" + this.Currency.GetCurrencyAmount() + "[/b] in your account[/i]");
        }

        /// <summary>
        /// Get the exits from the room.
        /// </summary>
        public void GetRoomExits()
		{
			this.sendMessageToPlayer(this.GetRoom().GetExitDetails(this));
		}

		/// <summary>
		/// Move the character.
		/// </summary>
		/// <param name="charID"></param>
		/// <param name="exitShortcut"></param>
		/// <returns></returns>
		public void Move(string exitShortcut)
		{
			// Get the current character location
			List<SMCharacter> smcs = new List<SMCharacter>();
			smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
			SMCharacter charToMove = new SMCharacter();
			bool foundCharacter = false;

			// Check if there are characters actually on the server to move...
			if (smcs != null)
			{
				if (smcs.FirstOrDefault(smc => smc.UserID == this.UserID) != null)
				{
					// Get the id from the object
					charToMove = smcs.FirstOrDefault(smc => smc.UserID == this.UserID);
					foundCharacter = true;
				}
				else
				{
					this.sendMessageToPlayer(this.Formatter.Italic("Character not logged in, please login before trying to move."));
				}
			}
			else
			{
				this.sendMessageToPlayer(this.Formatter.Italic("Character not logged in, please login before trying to move."));
			}

			if (foundCharacter)
			{
				// Get the room for the characters location
				List<SMRoom> smrs = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];
				SMRoom roomInMem = smrs.FirstOrDefault(smrn => smrn.RoomID.ToLower() == charToMove.RoomID.ToLower());

				// Get the specific exit from the location referred to by the shortcut
				SMExit sme = new SMExit();
				sme = roomInMem.RoomExits.FirstOrDefault(smes => smes.Shortcut.ToLower() == exitShortcut.ToLower());

                // Check that the exit isn't null (i.e. they've entered the right text!)
                if (sme != null)
                {
                    // Get the new room (and check that it's loaded in memory).
                    SMRoom smr = new SlackMud().GetRoom(sme.RoomID);

                    if (smr != null)
                    {
                        // Variable for use in a moment
                        bool initiateMove = true;

                        // Check if the room is locked
                        if (sme.Locked)
                        {
                            // Find out if the character has keys for the location
                            if (!this.CheckKey(sme.RoomLockID))
                            {
                                this.sendMessageToPlayer(this.Formatter.Italic("The door is locked and you do not have a key"));
                                initiateMove = false;
                            }
                        }

                        // If the room is not locked or the character has the right key, let them in.
                        if (initiateMove)
                        {
                            int waitTime = 0;

                            // Show echos if any are around
                            if ((sme.Echo != null) && (sme.Echo != ""))
                            {
                                this.sendMessageToPlayer(sme.Echo);
                                waitTime = 4;
                            }

                            if (waitTime > 0)
                            {
                                Thread.Sleep(waitTime*1000);
                            }
                            
                            ActualMove(smr, " walks out.", " walks in.");
                        }
                    }
                }
                else
                {
                    this.sendMessageToPlayer(this.Formatter.Italic("Exit name: " + exitShortcut + " not found, please check and try again"));
                }
			}
		}

        public void ActualMove(SMRoom smr, string leavingMethod, string enteringMethod, bool processResponses = true)
        {
            // Walk out of the room code.
            SMRoom currentRoom = this.GetRoom();

            string nameOfLocation = smr.RoomID;
            if (nameOfLocation.Contains("||"))
            {
                nameOfLocation = nameOfLocation.Substring(0, nameOfLocation.IndexOf("||"));
            }
            string[] locationNameElements = nameOfLocation.Split('.');
            nameOfLocation = " (" + locationNameElements[locationNameElements.Length-1] + ")";

            currentRoom.Announce(this.Formatter.Italic(this.GetFullName() + leavingMethod + nameOfLocation), this, true);

            if (processResponses)
            {
                currentRoom.ProcessNPCReactions("PlayerCharacter.Leave", this);
            }
            
            // Expire any awaiting responses from NPCs (to clean the memory / character file up)
            NPCsWaitingForResponses = null;

            // Check if the player is leaving a safezone
            if (currentRoom.SafeZone)
            {
                // Reload their stats as they were when they were first in the room.
                SMRoomSafeCharacterAttributes smrsca = currentRoom.SafeZoneCharAttributes.FirstOrDefault(a => a.CharacterName == this.GetFullName());

                // See if there were some attributes in memory (there should be).
                if (smrsca != null)
                {
                    this.Attributes = smrsca.SavedAttributes;
                    smr.SafeZoneCharAttributes.Remove(smrsca);
                }
            }
            
            // Move the player to the new location
            this.RoomID = smr.RoomID;
            this.SaveToApplication();
            this.SaveToFile();
            this.sendMessageToPlayer(new SlackMud().GetLocationDetails(this.RoomID, this));

            // If this is a safe zone, copy the character attributes out for use on death.
            if (smr.SafeZone)
            {
                SMRoomSafeCharacterAttributes smrsca = new SMRoomSafeCharacterAttributes();
                smrsca.CharacterName = this.GetFullName();
                smrsca.SavedAttributes = this.Attributes;

                smr.SafeZoneCharAttributes.Add(smrsca);
            }

            // Announce arrival to other players in the same place
            smr.Announce(this.Formatter.Italic(this.GetFullName() + enteringMethod), this, true);
            smr.ProcessNPCReactions("PlayerCharacter.Enter", this);
        }

		/// <summary>
		/// Flee from a location via a random exit.
		/// </summary>
		public void Flee()
		{
			// Get the room for the characters location
			List<SMRoom> smrs = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];
			SMRoom roomInMem = smrs.FirstOrDefault(smrn => smrn.RoomID.ToLower() == this.RoomID.ToLower());

			// Get the specific exit from the location referred to by the shortcut
			List<SMExit> sme = roomInMem.RoomExits.FindAll(exit => exit.Locked == false);

			// Check that the exit isn't null (i.e. they've entered the right text!)
			if (sme != null)
			{
				if (sme.Count > 1)
				{
					sme = sme.OrderBy(item => new Random().Next()).ToList();
				}

				SMRoom smr = new SlackMud().GetRoom(sme.First().RoomID);

				if (smr != null)
				{
					// Walk out of the room code.
					SMRoom currentRoom = this.GetRoom();

					currentRoom.Announce(this.Formatter.Italic(this.GetFullName() + " flees."), this, true);
					currentRoom.ProcessNPCReactions("PlayerCharacter.Leave", this);
				
					// Move the player to the new location
					this.RoomID = smr.RoomID;
					this.SaveToApplication();
					this.SaveToFile();
					this.sendMessageToPlayer(new SlackMud().GetLocationDetails(this.RoomID, this));

					// Announce arrival to other players in the same place
					smr.Announce(this.Formatter.Italic(this.GetFullName() + " arrives in haste."), this, true);
					smr.ProcessNPCReactions("PlayerCharacter.Enter", this);
				}
				else
				{
					this.sendMessageToPlayer(this.Formatter.Italic("Can not find an exit to flee through."));
				}
			}
		}

        /// <summary>
        /// Inspect an thing in the room...
        /// </summary>
        /// <param name="thingToInspect">The thing to inspect</param>
        public void InspectObject(string thingToInspect)
        {
            this.GetRoom().InspectThing(this, thingToInspect);
        }

        /// <summary>
        /// Set the description of the character
        /// </summary>
        /// <param name="newDescription">The description of the character</param>
        public void SetDescription(string newDescription)
        {
            this.Description = newDescription;
            this.sendMessageToPlayer(this.Formatter.Italic("Description Updated to: " + newDescription));
			this.SaveToApplication();
		}

        /// <summary>
        /// Admin Command - move the player back to the landing area
        /// </summary>
        public void Flush()
        {
            this.RoomID = new SlackMud().GetFlushLocation();
            this.GetRoomDetails();
            this.SaveToApplication();
            this.SaveToFile();

			// Remove the character from memory in case the in mem version is invalid
			List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
			smcs.Remove(this);
			HttpContext.Current.Application["SMCharacters"] = smcs;
		}

		#endregion

		#region "CharacterCommands"

		/// <summary>
		/// Get the skills for the player
		/// </summary>
		public void GetSkills()
		{
			// Variables for output
			string messageToSend = this.Formatter.Bold("Skills:");
			string actualSkills = "";

			// Craft all of the output elements.
			if (this.Skills != null)
			{
				foreach (SMSkillHeld smsh in this.Skills)
				{
					actualSkills += this.Formatter.ListItem(smsh.SkillName + " level " + smsh.SkillLevel);
				}
			}
			
			// Check if they actually had any skills...
			if (actualSkills == "")
			{
				actualSkills = this.Formatter.ListItem("You do not have any skills yet, try to use some to learn them.");
			}

			// Tell the player
			this.sendMessageToPlayer(messageToSend + actualSkills);
		}

		/// <summary>
		/// Get the stats for the player
		/// </summary>
		public void GetStats()
		{
			// Set up the output
			string messageToSend = this.Formatter.Bold("Statistics:");

			// Craft all of the output elements.
			messageToSend += this.Formatter.ListItem("Level: " + this.CalculateLevel());
			messageToSend += this.Formatter.ListItem("-----------------------");
			messageToSend += this.Formatter.ListItem("Charisma: " + this.Attributes.GetBaseStatValue("CHR"));
			messageToSend += this.Formatter.ListItem("Dexterity: " + this.Attributes.GetBaseStatValue("DEX"));
			messageToSend += this.Formatter.ListItem("Fortitude: " + this.Attributes.GetBaseStatValue("FT"));
			messageToSend += this.Formatter.ListItem("Hit Points: " + this.Attributes.GetBaseStatValue("HP") + " / " + this.Attributes.GetBaseStatValue("MAXHP"));
			messageToSend += this.Formatter.ListItem("Social Standing: " + this.Attributes.GetBaseStatValue("SS"));
			messageToSend += this.Formatter.ListItem("Strength: " + this.Attributes.GetBaseStatValue("STR"));
			messageToSend += this.Formatter.ListItem("Toughness: " + this.Attributes.GetBaseStatValue("T"));
			messageToSend += this.Formatter.ListItem("WillPower: " + this.Attributes.GetBaseStatValue("WP"));

			// Tell the player
			this.sendMessageToPlayer(messageToSend);
		}

		/// <summary>
		/// Get the level for the player
		/// </summary>
		public void GetLevel()
		{
			// Tell the player
			this.sendMessageToPlayer(this.CalculateLevel());
		}

		/// <summary>
		/// Get the character level
		/// </summary>
		/// <returns></returns>
		public string CalculateLevel()
		{
			int characterLevel = 0;
			if (this.Skills != null)
			{
				foreach (SMSkillHeld smsh in this.Skills)
				{
					characterLevel += smsh.SkillLevel;
				}
			}
			return characterLevel.ToString();
		}

		/// <summary>
		/// Add a new note to the player notes
		/// </summary>
		/// <param name="addition">The note to add</param>
		public void AddToNotes(string addition)
		{
			if (Notes == null)
			{
				this.Notes = new List<PlayerNote>();
			}
			PlayerNote newNote = new PlayerNote();
			newNote.Note = addition;
			this.Notes.Add(newNote);
			this.sendMessageToPlayer(this.Formatter.Italic("Note added to journal"));

			this.SaveToApplication();
			this.SaveToFile();
		}

		/// <summary>
		/// Get the notes
		/// </summary>
		public void GetNotes()
		{
			string notes = this.Formatter.Bold("Your Journal:");
			if ((Notes != null) && (Notes.Count>0))
			{
				int countNotes = 0;
				foreach(PlayerNote pn in this.Notes)
				{
					countNotes++;
					notes += this.Formatter.ListItem(countNotes + ") " + pn.Note);
				}
			}
			else
			{
				notes += this.Formatter.ListItem("You have no notes, to add a note use the command \"add note whateveryouwant\"");
			}
			this.sendMessageToPlayer(notes);
		}

		/// <summary>
		/// Remove a note from player notes
		/// </summary>
		/// <param name="removeitem">The item to remove</param>
		public void RemoveFromNotes(string removeitem)
		{
			if (Notes != null)
			{
				try
				{
					this.Notes.RemoveAt(int.Parse(removeitem)-1);
					this.sendMessageToPlayer(this.Formatter.Italic("Note removed from journal."));
					this.SaveToApplication();
					this.SaveToFile();
				} catch
				{
					this.sendMessageToPlayer(this.Formatter.Italic("Can not remove that note, please check the number and try again."));
				}
			}
		}

        /// <summary>
        /// Allows a player to go to sleep in a location if an item with the itemfamily "Bed" in a room.
        /// </summary>
        public void Sleep()
        {
            // Get the room that the character is in.
            SMRoom smr = this.GetRoom();
            
            // Check if there is a bed in the room
            if ((smr.RoomItems != null) && (smr.RoomItems.Count(roomitem => roomitem.ItemFamily.ToLower() == "bed") > 0)) 
            {
                // If there is a bed send them a message to say they're logging out.
                this.sendMessageToPlayer(this.Formatter.Italic("You feel weary and slip into a pleasant sleep (you have been logged out from the game world)."));

                // .. remove them from the world.
                List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
                smcs.Remove(this);
                HttpContext.Current.Application["SMCharacters"] = smcs;
                smr.Announce(this.Formatter.Italic(this.GetFullName() + " falls into a deep sleep"));

                new SlackMud().SendGlobalMessage(this, "[i][b]Global Message:[/b] " + this.GetFullName() + " has logged out of the game[/i]");
            }
            else // If there isn't a bed tell them that they can't log out here.
            {
                this.sendMessageToPlayer(this.Formatter.Italic("There is no bed in this location so you can not sleep here."));
            }
        }

        /// <summary>
        /// Allows a player to go to wake up
        /// </summary>
        public void Wake()
        {
            // Get the room that the character is in.
            SMRoom smr = this.GetRoom();
            smr.Announce(this.Formatter.Italic(this.GetFullName() + " wakes up"), this, true);

            // Send a message to the player to show they've woken up
            this.sendMessageToPlayer("[i]You wake up and feel well rested after your sleep.[/i]");

            // then send the details to the player for the room.
            this.GetRoomDetails();
        }

        /// <summary>
        /// Enables the character to read a sign
        /// </summary>
        /// <param name="itemIdentifier">The item to be read</param>
        public void Read(string itemIdentifier)
		{
			// Get the item
			SMItem item = this.FindItemInRoom(itemIdentifier);

			// Check the item is readable
			if (item != null)
			{
				if (item.ItemType == "Readable")
				{
					this.sendMessageToPlayer(this.Formatter.Bold("The " + item.ItemName + " reads:"));
					this.sendMessageToPlayer(this.Formatter.ListItem(item.ItemExtraDetail));

                    if (item.Effects != null)
                    {
                        item.InitiateEffects(this);
                    }
                }
				else
				{
					this.sendMessageToPlayer(this.Formatter.Italic("That item can not be read"));
				}
			}
			else
			{
				this.sendMessageToPlayer(this.Formatter.Italic("Can not find item"));
			}
		}

		/// <summary>
		/// Tells the player how many people are currently online
		/// </summary>
		public void Who()
		{
			// Construct the string
			string whoOnlineString = this.Formatter.Bold("People online:",1);

			// Get the list of all online
			List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];

			// Loop around the characters and add them to the who online list
			string whoList = "";
			foreach (SMCharacter smc in smcs)
			{
				whoList += this.Formatter.ListItem(smc.GetFullName() + " (" + smc.CalculateLevel() + ")");
			}

			// Add the list to the output string.
			whoOnlineString += this.Formatter.General(whoList);

			// Quantify the number of people online presently
			whoOnlineString += this.Formatter.Italic(smcs.Count.ToString() + " currently online",1);

			// Send the message back to the player
			this.sendMessageToPlayer(whoOnlineString);
		}

        /// <summary>
        /// Updates the password for the character
        /// </summary>
        /// <param name="newPassword">The new password</param>
        public void SetPassword(string newPassword)
        {
            this.Password = Utility.Crypto.EncryptStringAES(newPassword);
            this.SaveToApplication();
            this.SaveToFile();
            this.sendMessageToPlayer("[i]Password updated[/i]");
        }

        /// <summary>
        /// Sets the username to be different if needed.
        /// </summary>
        /// <param name="newUserName">The new username to use</param>
        public void SetUserName(string newUserName)
        {
            this.Username = newUserName;
            this.sendMessageToPlayer("[i]Username updated[/i]");
        }

        /// <summary>
        /// Drink something...
        /// </summary>
        /// <param name="itemName">The name of the item to drink</param>
        public void Drink(string itemName)
        {
            this.ConsumeItem(itemName, "drinks", "drink", "Drinkable");
        }

        /// <summary>
        /// Eat something...
        /// </summary>
        /// <param name="itemName">The name of the item to eat</param>
        public void Eat(string itemName)
        {
            this.ConsumeItem(itemName, "eats", "eat", "Eatable");
        }

        /// <summary>
        /// Consume something
        /// </summary>
        /// <param name="itemName">The name of the item to consume</param>
        public void ConsumeItem(string itemName, string consumeVerb, string singularConsumeVerb, string objectTrait)
        {
            // Check the item exists in the characters posession.
            SMItem smi = this.GetOwnedItem(itemName);

            // Check that the character has the item about their person...
            if (smi != null) {
                // See if the item can be drunk...
                if (smi.ObjectTrait.ToLower().Contains(objectTrait.ToLower()))
                {
                    // Check that the player has an initialised Effects list on the attributes
                    if (this.Attributes.Effects == null)
                    {
                        this.Attributes.Effects = new List<SMEffect>();
                    }

                    // Check the "on consume" effects for the item...
                    foreach (SMEffect sme in smi.Effects)
                    {
                        if (sme.Action.ToLower() == "onconsume")
                        {
                            // Change the timing on this.
                            sme.EffectLength = Utility.Utils.GetUnixTimeOffset(sme.EffectLength);

                            // Now add the effect to the attributes
                            this.Attributes.Effects.Add(sme);
                        }
                    }

                    // Remove the item.
                    RemoveOwnedItem(smi.ItemName, false);
                    this.SaveToApplication();
                    this.SaveToFile();

                    // Tell everyone that the character has drunk the item.
                    this.GetRoom().Announce("[i]" + this.GetFullName() + " " + consumeVerb + " " + smi.SingularPronoun + " " + smi.ItemName + "[/i]", this);
                }
                else // ... tell the player that it can't.
                {
                    this.sendMessageToPlayer("[i]Can not drink that item.[/i]");
                }
            }
            else // ...they don't have the item.
            {
                this.sendMessageToPlayer("[i]Can not find the item " + itemName + "[/i]");
            }
        }

		#endregion

		#region "Skill Related Items"

		/// <summary>
		/// Use a skill
		/// </summary>
		/// <param name="skillName">The name of the skill to use</param>
		/// <param name="targetName">The name of a (the) target to use the skill on (optional)</param>
		public void UseSkill(string skillName, string targetName = null, bool isCombat = false, string extraData = null)
		{
			// Create a new instance of the skill.
			SMSkill smc = ((List<SMSkill>)HttpContext.Current.Application["SMSkills"]).FirstOrDefault(sms => sms.SkillName.ToLower() == skillName.ToLower());

            if (this.CurrentActivity != smc.ActivityType)
            {
                SMSkillHeld smcs = null;

			    // Find out if the character has the skill.
			    if (this.Skills != null)
			    {
				    smcs = this.Skills.FirstOrDefault(charskill => charskill.SkillName.ToLower() == skillName.ToLower());
			    }

			    // If the character has the skill
			    if ((isCombat) || ((smcs != null)||(smc.CanUseWithoutLearning)))
			    {
				    // Variables for use later
				    string targetType = null, targetID = null;
				    bool useSkill = true;

				    // If there's a target we need to look at...
				    if (targetName != null) {
					    // .. get the room
					    SMRoom currentRoom = this.GetRoom();

                        // find any players with that target name first
                        SMCharacter targetCharacter = currentRoom.GetAllPeople().FirstOrDefault(tC => tC.GetFullName().ToLower() == targetName.ToLower());
                        
                        // If it's not null set the target details
                        if (targetCharacter != null)
					    {
						    // Set the target as a character and set the target id
						    targetType = "Character";
						    targetID = targetCharacter.UserID;
					    }
					    else // We need to see if there's an object with the name
					    {
						    // get a target item with the target name
						    SMItem targetItem = currentRoom.GetItemByName(targetName);

						    // if we find one...
						    if (targetItem != null)
						    {
							    // .. set the target type to be an item and set the target id
							    targetType = "Item";
							    targetID = targetItem.ItemID;
						    }
						    else
						    {
							    // get a target item with the target name
							    targetItem = currentRoom.GetItemByFamilyName(targetName);

							    // if we find one...
							    if (targetItem != null)
							    {
								    // .. set the target type to be an item and set the target id
								    targetType = "Item";
								    targetID = targetItem.ItemID;
							    }
							    else
							    {
								    // Not found the target with the name.. so send a message...
								    this.sendMessageToPlayer(this.Formatter.General("The target you've specified is not valid"));

								    // And we can't use the skill because we can't find the target.
								    useSkill = false;
							    }
						    }
					    }
				    }

				    // Check if we're able to use the skill...
				    if (useSkill) {
					    // Output variables we don't need
					    string messageOut;
					    float floatOut;

					    // Execute the skill
					    smc.UseSkill(this, out messageOut, out floatOut, extraData, 0, true, targetType, targetID);
				    }
			    }
			    else
			    {
				    // Can't use the skill so let the player know!
				    this.sendMessageToPlayer(this.Formatter.Italic($"You need to learn the \"{skillName}\" skill before you can use it."));
			    }
            }
            else
            {
                this.sendMessageToPlayer(this.Formatter.Italic($"You are already {this.CurrentActivity}"));
            }
        }

        /// <summary>
        /// Cast a spell
        /// </summary>
        /// <param name="spellName">The name of the spell being cast</param>
        /// <param name="targetName">The target name if any</param>
        public void Cast(string spellName, string targetName = null)
        {
            UseSkill(spellName, targetName);
        }

		/// <summary>
		/// Check that a player has the required skill level (by name)
		/// </summary>
		/// <param name="skillName">Name of the skill</param>
		/// <param name="skillLevel">The skill level the player has</param>
		/// <returns></returns>
		public bool HasRequiredSkill(string skillName, string skillLevel)
		{
			if (this.Skills != null)
			{
				return this.Skills.FirstOrDefault(skill => skill.SkillName == skillName && skill.SkillLevel >= int.Parse(skillLevel)) != null;
			}
			return false;
		}

        public void CraftItem(string nameOfReceipe)
        {
            List<SMReceipe> smrl = (List<SMReceipe>)HttpContext.Current.Application["SMReceipes"];
            SMReceipe smr = smrl.FirstOrDefault(receipe => receipe.Name.ToLower() == nameOfReceipe.ToLower());

            if (smr != null)
            {
                this.UseSkill(smr.RequiredSkills.First().SkillName, null, false, nameOfReceipe);
            }
            else
            {
                this.sendMessageToPlayer(this.Formatter.Italic($"The receipe for the item {nameOfReceipe} does not exist"));
            }
        }

        /// <summary>
        /// Stops the current activity happening.
        /// </summary>
        public void StopActivity()
        {
			this.sendMessageToPlayer(this.Formatter.Italic($"Stopped {this.CurrentActivity}"));
			this.CurrentActivity = null;
		}

		#endregion

		#region "Combat Related Functions"

		public void Attack(string targetName)
		{
			// If there's a target we need to look at...
			if (targetName != null)
			{
				// .. get the room
				SMRoom currentRoom = this.GetRoom();

				// find any players with that target name first
				List<SMCharacter> characterlist = currentRoom.GetAllPeople();

				SMCharacter targetCharacter = characterlist.FirstOrDefault(tC => tC.GetFullName().ToLower() == targetName.ToLower());

				// First check if they're just using a firstname or surname.
				if (targetCharacter == null)
				{
					targetCharacter = characterlist.FirstOrDefault(tC => (tC.FirstName.ToLower() == targetName.ToLower()) || (tC.LastName.ToLower() == targetName.ToLower()));
				}

				// Check if it's an NPC they're targeting by "family" type name i.e. Goose for a Larger Goose.
				if (targetCharacter == null)
				{
					targetCharacter = currentRoom.GetNPCs().FirstOrDefault(tc => tc.FamilyType.ToLower() == targetName.ToLower());
				}

				// If it's not null set the target details
				if (targetCharacter != null)
				{
					// Attack the character
					SMCombat.Attack(this, targetCharacter);
				}
				else // We need to see if there's an object with the name
				{
					// get a target item with the target name
					SMItem targetItem = currentRoom.GetItemByName(targetName);

					// if we find one...
					if (targetItem != null)
					{
						// Attack the item
						SMCombat.Attack(this, targetItem);
					}
					else
					{
						// get a target item with the target name
						targetItem = currentRoom.GetItemByFamilyName(targetName);

						// if we find one...
						if (targetItem != null)
						{
							// Attack the item
							SMCombat.Attack(this, targetItem);
						}
						else
						{
                            // Not found the target with the name.. so send a message...
                            this.sendMessageToPlayer(this.Formatter.General("The target you've specified is not valid"));
						}
					}
				}
			}
		}

        /// <summary>
        /// Kill the character, at present they'll just respawn in the "hospital"
        /// Later we need to extend this to have a limit to the number of lives!
        /// </summary>
        public void Die()
        {
            // Get the room the char is currently in
            SMRoom smr = this.GetRoom();

            // Check whether they're in a safe zone or not...
            if (smr.SafeZone)
            {
                // Check if it is an NPC... 
                if (this.GetType().Name != "SMNPC")
                {
                    // Get the saved attribues
                    SMRoomSafeCharacterAttributes smsca = smr.SafeZoneCharAttributes.FirstOrDefault(ca => ca.CharacterName == this.GetFullName());
                    
                    if (smsca != null)
                    {
                        // Load the attributes back into memory for the character
                        this.Attributes = smsca.SavedAttributes;
                    }

                    // Move the player to the reload location (if they're not already in the location).
                    if (this.RoomID != smr.InstanceReloadLocation)
                    {
                        // Set the char room id to be the new id
                        this.RoomID = smr.InstanceReloadLocation;

                        // Tell the players in the room the person has been moved.
                        smr.Announce(this.GetFullName() + " has been moved to " + smr.InstanceReloadLocation.Replace(".", ", "), this, true);

                        // Tell the player they've been moved to the other location...
                        this.sendMessageToPlayer(this.Formatter.Italic("You would have died..."));
                        this.GetRoomDetails();
                    }
                    
                    // Save the player
                    this.SaveToApplication();
                    this.SaveToFile();
                }
                else // it is an NPC
                {
                    // First create a corpse where they are, with all the associated items attached!
                    // Drop all the items the character is holding
                    string droppedItemsAnnouncement = "";
                    bool isFirstDroppedItem = true;
                    if (this.Slots != null)
                    {
                        foreach (SMSlot smcs in this.Slots)
                        {
                            if (((smcs.Name == "RightHand") || (smcs.Name == "LeftHand")) && (!smcs.isEmpty()))
                            {
                                SMItem droppedItem = smcs.EquippedItem;
                                this.GetRoom().AddItem(droppedItem);
                                if (!isFirstDroppedItem)
                                {
                                    droppedItemsAnnouncement += ", ";
                                }
                                else
                                {
                                    isFirstDroppedItem = false;
                                }
                                droppedItemsAnnouncement += droppedItem.SingularPronoun + " " + droppedItem.ItemName;
                                smcs.EquippedItem = null;
                            }
                        }
                    }

                    SMItem corpse = ProduceCorpse();
                    SMRoom currentRoom = this.GetRoom();

                    currentRoom.AddItem(corpse);

                    // Remove the character from memory (they really do die here).
                    new SlackMud().RemoveNPCFromMemory(this.UserID);
                }
            }
            else
            {
                // First create a corpse where they are, with all the associated items attached!
                // Drop all the items the character is holding
                string droppedItemsAnnouncement = "";
                bool isFirstDroppedItem = true;
                if (this.Slots != null)
                {
                    foreach (SMSlot smcs in this.Slots)
                    {
                        if (((smcs.Name == "RightHand") || (smcs.Name == "LeftHand")) && (!smcs.isEmpty()))
                        {
                            SMItem droppedItem = smcs.EquippedItem;
                            this.GetRoom().AddItem(droppedItem);
                            if (!isFirstDroppedItem)
                            {
                                droppedItemsAnnouncement += ", ";
                            }
                            else
                            {
                                isFirstDroppedItem = false;
                            }
                            droppedItemsAnnouncement += droppedItem.SingularPronoun + " " + droppedItem.ItemName;
                            smcs.EquippedItem = null;
                        }
                    }
                }

                SMItem corpse = ProduceCorpse();
                SMRoom currentRoom = this.GetRoom();

                currentRoom.AddItem(corpse);

                // Check whether it's an NPC
                if (this.GetType().Name != "SMNPC")
                {
                    // Then move the player back to the hospital
                    this.RoomID = "Hospital";
                    this.Attributes.HitPoints = this.Attributes.MaxHitPoints / 2;

                    // Tell the player they've died and announce their new location
                    this.sendMessageToPlayer(this.Formatter.General("You have died and have awoken feeling groggy - you won't be at full health yet, you'll need to recharge yourself!"));
                    this.GetRoomDetails();

                    // TODO reduce the number of rerolls they have
                    // If they get to 0 rerolls the character is permenant dead.

                    // Announce the items the player dropped.
                    if (droppedItemsAnnouncement != "")
                    {
                        currentRoom.Announce(this.Formatter.General($"While dying {this.GetFullName()} dropped the following items: {droppedItemsAnnouncement}"));
                    }
                    
                    // Reset the character activity
                    this.CurrentActivity = null;

                    // Save the player
                    this.SaveToApplication();
                    this.SaveToFile();
                }
                else
                {
                    new SlackMud().RemoveNPCFromMemory(this.UserID);
                }
            }
        }

		public virtual SMItem ProduceCorpse()
		{
			// Create the corpse
			SMItem corpse = SMItemFactory.Get("Misc", "Corpse");
			corpse.ItemName = "Corpse of " + this.GetFullName();
			
			// Create the "held items" list ready for transferring items to the corpse.
			corpse.HeldItems = new List<SMItem>();

			if (this.Slots != null)
			{
				foreach (SMSlot sms in this.Slots)
				{
					if (!sms.isEmpty())
					{
						corpse.HeldItems.Add(sms.EquippedItem);
					}
				}
			}

			// TODO Add clothing / armour items to the held items list ready for looting.

			return corpse;
		}

		/// <summary>
		/// Check if the player dodges or parry's the attack.
		/// </summary>
		/// <returns></returns>
		public bool CheckDodgeParry()
        {
            // Ensure that the character has skills...
           
            // Check Dodge
            int rndChance = new Random().Next(1, 100);

			// Set the base dodge as the dexterity of the character
			int dodgeChance = this.Attributes.Dexterity;

			if (this.Skills != null)
			{
				// Does the character have the dodge skill?
				SMSkillHeld smsh = this.Skills.FirstOrDefault(skill => skill.SkillName == "Dodge");
				if (smsh != null)
				{
					dodgeChance += (int)(smsh.SkillLevel * 2);
				}
			}

			if (rndChance <= dodgeChance)
			{
				// Send the message to the player that they've dodged...
				this.sendMessageToPlayer(this.Formatter.Italic("You have dodged an attack..."));

				// Check whether the player should get better at the skill
				SMSkill smc = ((List<SMSkill>)HttpContext.Current.Application["SMSkills"]).FirstOrDefault(sms => sms.SkillName.ToLower() == "Dodge".ToLower());
				smc.SkillIncrease(this);

				return true;
			}

			// Does the character have the parry skill and something equipped?
			if (this.Skills != null)
			{
				SMSkillHeld smsh = this.Skills.FirstOrDefault(skill => skill.SkillName == "Parry");
				if ((!this.AreHandsEmpty()) && (smsh != null) && (this.HasItemTypeEquipped("Weapon")))
				{
					// Weapon check
					SMItem weapon = this.GetEquippedItem();
					int bonusValue = 0;
					if (this.Skills != null)
					{
						SMSkillHeld playerWeaponSkill = this.Skills.FirstOrDefault(sms => sms.SkillName.Contains(weapon.ItemFamily));
						bonusValue = playerWeaponSkill.SkillLevel;
					}

					int parryChance = (int)(smsh.SkillLevel * 2) + bonusValue;
					if (rndChance <= parryChance)
					{
						// Send the message to the player that they've parried
						this.sendMessageToPlayer(this.Formatter.Italic("You have parried an attack..."));

						// Check whether the player should get better at the skill
						SMSkill smc = ((List<SMSkill>)HttpContext.Current.Application["SMSkills"]).FirstOrDefault(sms => sms.SkillName.ToLower() == "Parry".ToLower());
						smc.SkillIncrease(this);

						return true;
					}
				}
			}

            return false;
        }

		#endregion

		#region "Inventory Functions"

		/// <summary>
		/// Picks up an item from the current room (identified by a given string) and places it in an equipped container or empty hand.
		/// </summary>
		/// <param name="itemIdentifier">Items identifier (id, name or family).</param>
		public void PickUpItem(string itemIdentifier)
		{
			// Find the item in the characters room
			SMItem itemToPickup = this.FindItemInRoom(itemIdentifier);

			// If the item cannot be found inform the player and return
			if (itemToPickup == null)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find \"{Utils.SanitiseString(itemIdentifier)}\"!"));
				return;
			}

			// Weight Check
			if (!this.CheckWeightWithNewItem(itemToPickup))
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to pick up {itemToPickup.ItemName}({itemToPickup.ItemWeight}), this would exceed your weight limit of \"{this.WeightLimit}\"."));
				return;
			}

			bool putInContainer = false;
			bool putInHand = false;

            if (this.AreHandsEmpty())
            {
                putInHand = this.PutItemInEmptyHand(itemToPickup);
            }
            
            if (!putInHand)
            {
                // Try to put the item in an equipped container
                putInContainer = this.PutItemInEquippedContainer(itemToPickup);

                // If unable to put in a container try empty hands
                if (!putInContainer)
                {
                    putInHand = this.PutItemInEmptyHand(itemToPickup);
                }
            }
			
			// Remove item from room and send messages on success
			if (putInContainer || putInHand)
			{
				this.GetRoom().RemoveItem(itemToPickup);

				this.SaveToFile();

				this.sendMessageToPlayer(this.Formatter.Italic($"You picked up {itemToPickup.SingularPronoun} {itemToPickup.ItemName}."));

				this.GetRoom().Announce(this.Formatter.Italic($"{this.GetFullName()} picked up {itemToPickup.SingularPronoun} {itemToPickup.ItemName}."), this, true);

				return;
			}

			// Otherwise send failure message
			this.sendMessageToPlayer(this.Formatter.Italic($"Unable to pick up {itemToPickup.ItemName}({itemToPickup.ItemWeight}), you don't have space for it."));
		}

		/// <summary>
		/// Receives a given item and places it in an equipped container or empty hand, ignoring weight limits.
		/// </summary>
		/// <param name="item">The item the character is receiving.</param>
		/// <param name="supressOutput">Bool indicating of the playour output shoudl be suppressed.</param>
		public void ReceiveItem(SMItem item, bool supressOutput = false)
		{
			bool putInContainer = false;
			bool putInHand = false;

			// Try to put the item in an equipped container
			putInContainer = this.PutItemInEquippedContainer(item);

			// If unable to put in a container try empty hands
			if (!putInContainer)
			{
				putInHand = this.PutItemInEmptyHand(item);
			}

			// Save character and send messages on success
			if (putInContainer || putInHand)
			{
				this.SaveToFile();

				if (!supressOutput)
				{
					this.sendMessageToPlayer(this.Formatter.Italic($"You received {item.SingularPronoun} {item.ItemName}."));

					this.GetRoom().Announce(this.Formatter.Italic($"{this.GetFullName()} received {item.SingularPronoun} {item.ItemName}."), this, true);
				}

				return;
			}

			// Otherwise send failure message
			if (!supressOutput)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to receive {item.ItemName}({item.ItemWeight}), you don't have space for it."));
			}
		}

		/// <summary>
		/// Empties all items from an equipped container identified by a given string. Items are places in the characters current location.
		/// </summary>
		/// <param name="containerIdentifier">String identifying the equipped container to empty.</param>
		public void EmptyContainer(string containerIdentifier)
		{
			// Find the equipped container to empty
			SMItem container = this.GetEquippedContainer(containerIdentifier);

			if (containerIdentifier != null)
			{
				if (container.HeldItems != null && container.HeldItems.Any())
				{
					List<SMItem> toRemove = new List<SMItem>();

					// Look through all items in the container adding them to the room
					foreach (SMItem item in container.HeldItems.ToList())
					{
						// Add item to the room
						this.GetRoom().AddItem(item);

						// Remove item from the container
						container.HeldItems.Remove(item);

						// Report to player
						this.sendMessageToPlayer(this.Formatter.Italic($"You dropped {item.SingularPronoun} {item.ItemName}."));

						// Announce to room
						this.GetRoom().Announce(this.Formatter.Italic($"\"{this.GetFullName()}\" dropped {item.SingularPronoun} {item.ItemName}."), this, true);
					}

					this.SaveToApplication();
					return;
				}

				this.sendMessageToPlayer(this.Formatter.Italic($"\"{container.ItemName}\" is already empty!"));
				return;
			}

			this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find \"{Utils.SanitiseString(containerIdentifier)}\"!"));
			return;
		}

		/// <summary>
		/// Stows a single item matching a given identifer that the character is holding, placing the item in an equipped container.
		/// </summary>
		/// <param name="itemIdentifier">Identifer of the item to stow.</param>
		public void Stow(string itemIdentifier)
		{
			// Handle "stow all"
			if (itemIdentifier.ToLower() == "all")
			{
				this.StowAll();
				return;
			}

			SMSlot slot = null;

			SMItem item = this.GetHeldItem(itemIdentifier, out slot);

			// Unable to find item
			if (item == null)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find \"{Utils.SanitiseString(itemIdentifier)}\" to stow, are you holding it?"));
				return;
			}

			// Failed to put item in an equipped container
			if (!this.PutItemInEquippedContainer(item))
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to stow \"{item.ItemName}\"!"));
				return;
			}

			// Success
			slot.EquippedItem = null;
			this.sendMessageToPlayer(this.Formatter.Italic($"You to stowed \"{item.ItemName}\"."));
			this.SaveToApplication();
		}

		/// <summary>
		/// Stow all items the character is holding, please them in an equipped container.
		/// </summary>
		public void StowAll()
		{
			// Get a list of slots to stow equipped items for
			List<SMSlot> hands = new List<SMSlot>();
			hands.Add(this.GetSlotByName("RightHand"));
			hands.Add(this.GetSlotByName("LeftHand"));

			// Check if there are ay items to stow
			if (hands.All(s => s.isEmpty()))
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Nothing to stow."));
				return;
			}

			// Try and stow each item one by one
			foreach (SMSlot hand in hands)
			{
				if (!hand.isEmpty())
				{
					this.Stow(hand.EquippedItem.ItemID);
				}
			}
		}

		/// <summary>
		/// Fill a container with as many of an item as are available or can fit in the container.
		/// </summary>
		/// <param name="containerIdentifier">String identifying the container.</param>
		/// <param name="itemIdentifier">String identifying the item.</param>
		public void FillContainer(string containerIdentifier, string itemIdentifier)
		{
			// Find the target container, check equipped items then the room
			SMItem container = this.GetEquippedContainer(containerIdentifier);
			container = container == null ? this.GetRoom().GetRoomContainer(containerIdentifier) : container;

			if (container == null)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find a container identified by \"{Utils.SanitiseString(containerIdentifier)}\""));
				return;
			}

			// Setup counter for items successfully put in the container
			int itemCount = 0;

			// Store a copy of the item to use in the player responces
			SMItem itemForResponcesDetails = null;

			// Setup a variable to hold a reason for stopping the fill operation
			string stopReason = String.Empty;

			while (true)
			{
				// Get item to put in container
				string itemLocation = String.Empty;

				SMItem itemToFillWith = this.GetItemForFill(itemIdentifier, out itemLocation);

				if (itemToFillWith == null)
				{
					stopReason = "no-item";
					break;
				}

				itemForResponcesDetails = itemToFillWith;

				// Check character weight limits
				if (itemLocation == "room" && !this.CheckWeightWithNewItem(itemToFillWith))
				{
					stopReason = "weight-limit";
					break;
				}

				// Check container can hold items of that type
				if (!container.CanHoldItemByFamily(itemToFillWith))
				{
					stopReason = "item-family-restriction";
					break;
				}

				// Check container has capacity for the item
				if (SMItemHelper.GetItemAvailbleCapacity(container) < itemToFillWith.ItemSize)
				{
					stopReason = "capacity-limit";
					break;
				}

				// Move item to container
				if (itemLocation == "room")
				{
					this.GetRoom().RemoveItem(itemToFillWith);
				}
				else
				{
					this.RemoveEquippedItem(itemToFillWith.ItemID);
				}

				SMItemHelper.PutItemInContainer(itemToFillWith, container);

				// incrament count
				itemCount++;

				// Check capacity to see if the container has been filled
				if (SMItemHelper.GetItemUsedCapacity(container) == container.ItemCapacity)
				{
					stopReason = "capacity-limit";
					break;
				}
			}

			// Save updates
			this.SaveToApplication();

			// Respond to player with reason for stopping the fill operation
			string msg = String.Empty;

			switch (stopReason)
			{
				case "no-item":
					if (itemCount < 1)
					{
						msg = $"Unable to find \"{Utils.SanitiseString(itemIdentifier)}\"!";
					}
					else
					{
						msg = $"Stopped filling {container.ItemName} because no more \"{itemForResponcesDetails.PluralName}\" are avalable in this location!";
					}
					break;

				case "weight-limit":
					if (itemCount < 1)
					{
						msg = $"Unable to fill {container.ItemName}, this would exceed your weight limit of {this.WeightLimit}.";
					}
					else
					{
						msg = $"Stopped filling {container.ItemName}, adding more items would exceed your weight limit of {this.WeightLimit}.";
					}
					break;

				case "item-family-restriction":
					msg = $"Unable to fill {container.ItemName} with items of the family \"{itemForResponcesDetails.ItemFamily}\"";
					break;

				case "capacity-limit":
					if (itemCount < 1)
					{
						msg = $"Unable to fill {container.ItemName}, there is no space available in it!";
					}
					else
					{
						msg = $"Stopped filling {container.ItemName}, capacity limit reached.";
					}
					break;
			}

			this.sendMessageToPlayer(this.Formatter.Italic(msg));

			// Respond to player with result of the fill operation
			if (itemCount > 0)
			{
				string result = $"You filled {container.ItemName} with {itemCount} ";
				if (itemCount > 1)
				{
					result += $"{itemForResponcesDetails.PluralName}";
				}
				else
				{
					result += $"{itemForResponcesDetails.ItemName}";
				}

				this.sendMessageToPlayer(this.Formatter.Italic(result));
			}
		}

		/// <summary>
		/// Gets the current weight of all items the character is holding or wearing.
		/// </summary>
		/// <returns>The current weight.</returns>
		private int GetCurrentWeight()
		{
			int weight = 0;

			foreach (SMSlot slot in this.Slots)
			{
				if (!slot.isEmpty())
				{
					weight += SMItemHelper.GetItemWeight(slot.EquippedItem);
				}
			}

			// TODO account for clothing (including item help in pockets ect...)

			return weight;
		}

		/// <summary>
		/// Checks in the character would be overweight if a given item was added to their inventory.
		/// </summary>
		/// <param name="item">The item bening added.</param>
		/// <returns>Bool indicating if adding the item would put the character overweight.</returns>
		private bool CheckWeightWithNewItem(SMItem item)
		{
			if (this.WeightLimit - this.GetCurrentWeight() < item.ItemWeight)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Attempts to put a give item in a container the character has equipped.
		/// </summary>
		/// <param name="item">The item to put in an equipped container.</param>
		/// <returns>Bool indicating the success of the operation.</returns>
		private bool PutItemInEquippedContainer(SMItem item)
		{
			SMItem container = this.GetEquippedContainerForItem(item);

			if (container != null)
			{
				SMItemHelper.PutItemInContainer(item, container);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to put a give item in an empty hand if the character has one.
		/// </summary>
		/// <param name="item">The item to put in an empty hand.</param>
		/// <returns>Bool indicating the suucess of the operation.</returns>
		private bool PutItemInEmptyHand(SMItem item)
		{
			SMSlot hand = this.GetEmptyHand();

			if (hand != null)
			{
				hand.EquippedItem = item;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Searches the character for a equipped container that can hold a given item.
		/// </summary>
		/// <param name="item">The item to find a container for.</param>
		/// <return>A container the item can go in or null if not container is found.</return></returns>
		private SMItem GetEquippedContainerForItem(SMItem item)
		{
			foreach (SMSlot slot in this.Slots)
			{
				// If there is an item equipped and it can hold other items
				if (!slot.isEmpty() && slot.EquippedItem.CanHoldOtherItems())
				{
					return SMItemHelper.FindContainerForItemRecursive(item, slot.EquippedItem);
				}
			}

			return null;
		}

		/// <summary>
		/// Gets an SMSlot representing an empty hand on the character, optionally weighted to the right hand first.
		/// </summary>
		/// <returns>The empty hand SMSlot.</returns>
		/// <param name="rightFirst">If set to <c>true</c> tries the right hand first.</param>
		private SMSlot GetEmptyHand(bool rightFirst = true)
		{
			SMSlot rightHand = this.GetSlotByName("RightHand");
			SMSlot leftHand = this.GetSlotByName("LeftHand");

			if (rightFirst)
			{
				if (rightHand.isEmpty())
				{
					return rightHand;
				}

				return leftHand.isEmpty() ? leftHand : null;
			}

			if (leftHand.isEmpty())
			{
				return leftHand;
			}

			return rightHand.isEmpty() ? rightHand : null;
		}

		/// <summary>
		/// Gets an equipped container by searching all slots for a container matching given identifier.
		/// </summary>
		/// <param name="itemIdentifier"></param>
		/// <returns>The equipped container or null.</returns>
		private SMItem GetEquippedContainer(string identifier)
		{
			SMItem container = null;

			foreach (SMSlot slot in this.Slots)
			{
				if (!slot.isEmpty() && slot.EquippedItem.CanHoldOtherItems())
				{
					container = SMItemHelper.FindContainerInContainerRecursive(identifier, slot.EquippedItem);

					if (container != null)
					{
						break;
					}
				}
			}

			return container;
		}

		/// <summary>
		/// Gets an item, identified by a give string, that the character is holding.
		/// </summary>
		/// <param name="itemIdentifier">String identifying the item.</param>
		/// <param name="slot">Variable to hold the slot the item is found it.</param>
		/// <returns>The item if found otherwise null.</returns>
		private SMItem GetHeldItem(string itemIdentifier, out SMSlot slot)
		{
			slot = null;

			// Get a list of slots to stow equipped items for
			List<SMSlot> hands = new List<SMSlot>();
			hands.Add(this.GetSlotByName("RightHand"));
			hands.Add(this.GetSlotByName("LeftHand"));

			foreach (SMSlot hand in hands)
			{
				if (this.SlotContainsItem(hand, itemIdentifier))
				{
					slot = hand;
					return hand.EquippedItem;
				}
			}

			return null;
		}

		/// <summary>
		/// Checks in a give slot contains an item idetified by a given string.
		/// </summary>
		/// <param name="slot">The SMSlot object to check.</param>
		/// <param name="itemIdentifier">String identifying the item.</param>
		/// <returns>Bool idicating if there was a match.</returns>
		private bool SlotContainsItem(SMSlot slot, string itemIdentifier)
		{
			if (!slot.isEmpty() && SMItemHelper.ItemMatches(slot.EquippedItem, itemIdentifier))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets an item for the FillContainer method to put in a container.
		/// </summary>
		/// <param name="itemIdentifier">String identifying the item.</param>
		/// <param name="itemLocation">Variable to hold the location of the item if found.</param>
		/// <returns>An item found that matched the given identifier.</returns>
		private SMItem GetItemForFill(string itemIdentifier, out string itemLocation)
		{
			itemLocation = null;
			SMItem item = null;

			item = this.GetRoom().GetRoomItem(itemIdentifier);

			if (item != null)
			{
				itemLocation = "room";
				return item;
			}

			item = this.GetEquippedItem(itemIdentifier);

			if (item != null)
			{
				itemLocation = "character";
				return item;
			}

			return item;
		}

		/// <summary>
		/// Drops the item specified by a given identifier (id, name, family).
		/// </summary>
		/// <param name="itemIdentifier">Item identifier.</param>
		public void DropItem(string itemIdentifier)
		{
			// TODO account for containers in clothing, e.g. pockets

			SMItem itemToDrop = null;

			// Loop through slots and check if the target item is directly equipped
			foreach (SMSlot slot in this.Slots)
			{
				if (slot.EquippedItem != null)
				{
					if (SMItemHelper.ItemMatches(slot.EquippedItem, itemIdentifier))
					{
						itemToDrop = slot.EquippedItem;

						// If the target item matched remove it
						slot.EquippedItem = null;

						this.SaveToApplication();
					}
				}

				if (itemToDrop != null)
				{
					break;
				}
			}

			// If we have not found the target item at this point look in equipped containers
			if (itemToDrop == null)
			{
				foreach (SMSlot slot in this.Slots)
				{
					if (slot != null)
					{
						if (slot.EquippedItem.CanHoldOtherItems() && slot.EquippedItem.HeldItems != null)
						{
							itemToDrop = SMItemHelper.GetItemFromList(slot.EquippedItem.HeldItems, itemIdentifier);

							// If the target item is found in the container remove it
							if (itemToDrop != null)
							{
								SMItemHelper.RemoveItemFromList(slot.EquippedItem.HeldItems, itemIdentifier);

								this.SaveToApplication();
							}
						}
					}
				}
			}

			// If we have found the target item add it too the room and announce this
			if (itemToDrop != null)
			{
				this.GetRoom().AddItem(itemToDrop);
				this.GetRoom().Announce(this.Formatter.Italic($"\"{this.GetFullName()}\" dropped {itemToDrop.SingularPronoun} {itemToDrop.ItemName}."));
				return;
			}

			this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find \"{Utils.SanitiseString(itemIdentifier)}\" to drop!"));
		}

		/// <summary>
		/// Trys to equip an item to a slot, which can be optionally specified.
		/// Look in the character inventory, then the room for the item, specified by an identifier (id, name, family).
		/// If slot is not specifed trys to find a suitable availble one.
		/// </summary>
		/// <param name="itemIdentifier">Identifier of the item to equip.</param>
		/// <param name="targetSlotName">Optional, name of the slot to equip the item to.</param>
		public void EquipItem(string itemIdentifier, string targetSlotName = null)
		{
			// Find the item to be equipped, look on the character, then in the current room

			// TODO account for items contained within clothing

			SMSlot slotContainingItem;
			SMItem itemToEquip = this.GetEquippableItem(itemIdentifier, out slotContainingItem);
			bool ownedItem = true;

			if (itemToEquip == null)
			{
				itemToEquip = this.FindItemInRoom(itemIdentifier);
				ownedItem = false;
			}

			if (itemToEquip == null)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find the item \"{Utils.SanitiseString(itemIdentifier)}\" to equip!"));
				return;
			}

			// Determine the slot the item should be equipped to
			SMSlot targetSlot = null;

			// Try the slot specified in the user input
			if (targetSlotName != null)
			{
				targetSlot = this.GetSlotByName(targetSlotName);

				if (targetSlot == null)
				{
					this.sendMessageToPlayer(this.Formatter.Italic($"The specified slot \"{Utils.SanitiseString(targetSlotName)}\" could not be found!"));
					return;
				}

				if (!targetSlot.isEmpty())
				{
					this.sendMessageToPlayer(this.Formatter.Italic($"Cannot equip an item to \"{targetSlot.GetReadableName()}\", that slot is not empty!"));
					return;
				}

				if (!targetSlot.canEquipItem(itemToEquip))
				{
					this.sendMessageToPlayer(this.Formatter.Italic($"Cannot equip an item of type \"{itemToEquip.ItemType}\" to \"{targetSlot.GetReadableName()}\"!"));
					return;
				}
			}

			// Try and find a suitable empty slot if slot name is not given
			if (targetSlot == null)
			{
				foreach (SMSlot slot in this.Slots)
				{
					if (slot.isEmpty() && slot.canEquipItem(itemToEquip))
					{
						targetSlot = slot;
						break;
					}
				}
			}

			if (targetSlot == null)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find a suitable empty slot to equip \"{itemToEquip.ItemName}\"!"));
				return;
			}

			// If the character does not already own the item check weight and remove item from room
			if (!ownedItem)
			{
				if (this.GetCurrentWeight() + SMItemHelper.GetItemWeight(itemToEquip) > this.WeightLimit)
				{
					this.sendMessageToPlayer(this.Formatter.Italic($"Unable to equip up \"{itemToEquip.ItemName}\", this would exceed your weight limit of \"{this.WeightLimit}\"!"));
					return;
				}

				this.GetRoom().RemoveItem(itemToEquip);
				this.GetRoom().Announce(this.Formatter.Italic($"\"{this.GetFullName()}\" picked up \"{itemToEquip.ItemName}\"!"));

			}
			// Else just remove the item from its container
			else
			{
				SMItemHelper.RemoveItemFromList(slotContainingItem.EquippedItem.HeldItems, itemToEquip.ItemID);
			}

			// Equip the item to the target slot and inform the palyer
			targetSlot.EquippedItem = itemToEquip;
			this.sendMessageToPlayer(this.Formatter.Italic($"You equipped {itemToEquip.SingularPronoun} \"{itemToEquip.ItemName}\"!"));
			this.SaveToApplication();
		}

		/// <summary>
		/// Puts an item identifed by id, name or family into a container identified by id, name or family.
		/// </summary>
		/// <param name="itemIdentifier"></param>
		/// <param name="containerIdentifier"></param>
		public void PutItem(string itemIdentifier, string containerIdentifier)
		{
			bool ownedItem;
			string output;

			// Find item, look in slots, clothing, then room
			SMItem itemToPut = null;

			// Look in slots
			itemToPut = this.GetEquippedItem(itemIdentifier);
			ownedItem = true;

			// TODO look in clothing

			// Look in the room if the item has not been found
			if (itemToPut == null)
			{
				itemToPut = this.FindItemInRoom(itemIdentifier);
				ownedItem = false;
			}

			// Check the item has been found
			if (itemToPut == null)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find the item \"{Utils.SanitiseString(itemIdentifier)}\"!"));
				return;
			}

			// Find container to put item in, look in slots, clothing, then room
			SMItem targetContainer = null;

			// Look in slots
			targetContainer = this.GetEquippedContainer(containerIdentifier);

			// TODO look in clothing e.g. pockets

			// Look in the room if the container has not been found
			if (targetContainer == null)
			{
				targetContainer = this.FindContainerInRoom(containerIdentifier);
			}

			// Check the container has been found
			if (targetContainer == null)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find the target container \"{Utils.SanitiseString(containerIdentifier)}\"!"));
				return;
			}

			// Check the item and container are not the same thing
			if (itemToPut.ItemID == targetContainer.ItemID)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"You cannot put an item inside it's self!"));
				return;
			}

			// Check the container can hold the family corresponding to the item
			if (!targetContainer.CanHoldItemByFamily(itemToPut))
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"You cannot put an item of the family \"{itemToPut.ItemFamily}\" in \"{targetContainer.ItemName}\"!"));
				return;
			}

			// Check weight limit is item not already owned
			if (!ownedItem)
			{
				if (this.GetCurrentWeight() + SMItemHelper.GetItemWeight(itemToPut) > this.WeightLimit)
				{
					output = $"Unable to put {itemToPut.SingularPronoun} \"{itemToPut.ItemName}\" directly from the room into ";
					output += $"{targetContainer.SingularPronoun} \"{targetContainer.ItemName}\", this would exceed your weight limit of \"{this.WeightLimit}\"!";

					this.sendMessageToPlayer(this.Formatter.Italic(output));
					return;
				}
			}

			// Check capacity limits in the target container
			if (SMItemHelper.GetItemAvailbleCapacity(targetContainer) < itemToPut.ItemSize)
			{
				output = $"Unable to put {itemToPut.SingularPronoun} \"{itemToPut.ItemName}\" in {targetContainer.SingularPronoun} \"{targetContainer.ItemName}\", ";
				output += $"as it would exceed the \"{targetContainer.ItemName}'s\" capacity of \"{targetContainer.ItemCapacity}\"!";

				this.sendMessageToPlayer(this.Formatter.Italic(output));
				return;
			}

			// Remove item from source
			if (ownedItem)
			{
				// TODO removal from clothing slots (e.g. pockets)
				this.RemoveEquippedItem(itemToPut.ItemID);
			}
			else
			{
				// Remove item from room
				this.GetRoom().RemoveItem(itemToPut);
				this.GetRoom().Announce(this.Formatter.Italic($"\"{this.GetFullName()}\" picked up {itemToPut.SingularPronoun} \"{itemToPut.ItemName}\"!"));
			}

			// Add item to container
			output = $"You put {itemToPut.SingularPronoun} \"{itemToPut.ItemName}\" ";
			output += $"in {targetContainer.SingularPronoun} {targetContainer.ItemName}.";

			SMItemHelper.PutItemInContainer(itemToPut, targetContainer);
			this.sendMessageToPlayer(this.Formatter.Italic(output));

			this.GetRoom().SaveToApplication();
			this.SaveToApplication();
            this.SaveToFile();
		}

		/// <summary>
		/// Gives an item identified by itemIdentifier to a charcter identified by playerName.
		/// </summary>
		/// <param name="itemIdentifier">Identifier of the item to give.</param>
		/// <param name="playerName">Name of the character to give the item to.</param>
		public void GiveItem(string itemIdentifier, string playerName)
		{
			// Get item from current players inventory
			SMItem itemToGive = this.GetOwnedItem(itemIdentifier);

			if (itemToGive == null)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"You must own an item to give it, \"{Utils.SanitiseString(itemIdentifier)}\" was not found in your inventory!"));
				return;
			}

			// Get player to give item to
			SMCharacter playerToGiveTo = this.GetRoom().GetAllPeople().FirstOrDefault(smc => (smc.GetFullName().ToLower() == playerName.ToLower()) || (smc.FirstName.ToLower() == playerName.ToLower()) || (smc.LastName.ToLower() == playerName.ToLower()));

			if (playerToGiveTo == null)
			{
				this.sendMessageToPlayer(this.Formatter.Italic($"Unable to find a player called, \"{Utils.SanitiseString(playerName)}\" in this room!"));
				return;
			}

			if (playerToGiveTo.GetType().Name != "SMNPC")
			{
				// Check player can recieve item (needs an empyt hand available weigth capacity to hold carry the item
				SMSlot receivingHand = playerToGiveTo.GetEmptyHand();

				if (receivingHand == null)
				{
					this.sendMessageToPlayer(this.Formatter.Italic($"Unable to give \"{itemToGive.ItemName}\" to \"{playerToGiveTo.GetFullName()}\", they dont have an emoty hand to take it!"));
					playerToGiveTo.sendMessageToPlayer(this.Formatter.Italic($"{this.GetFullName()} tried to give you {itemToGive.SingularPronoun} \"{itemToGive.ItemName}\" but you dont have an empty had to take it!"));
					return;
				}

				if (playerToGiveTo.WeightLimit - playerToGiveTo.GetCurrentWeight() < SMItemHelper.GetItemWeight(itemToGive))
				{
					this.sendMessageToPlayer(this.Formatter.Italic($"Unable to give \"{itemToGive.ItemName}\" to \"{playerToGiveTo.GetFullName()}\", they can't carry that much weight!"));
					playerToGiveTo.sendMessageToPlayer(this.Formatter.Italic($"{this.GetFullName()} tried to give you {itemToGive.SingularPronoun} \"{itemToGive.ItemName}\" but your strong enough to carry it!"));
					return;
				}

				// Remove item from current player
				this.RemoveOwnedItem(itemToGive.ItemID);

				// Add item to receiving player
				receivingHand.EquippedItem = itemToGive;

				// Send responces and save both players
				this.sendMessageToPlayer(this.Formatter.Italic($"You gave {playerToGiveTo.GetFullName()} {itemToGive.SingularPronoun} \"{itemToGive.ItemName}\""));
				playerToGiveTo.sendMessageToPlayer(this.Formatter.Italic($"{this.GetFullName()} gave you {itemToGive.SingularPronoun} \"{itemToGive.ItemName}\""));

				// Make sure the player now knows what he has.
				this.SaveToApplication();
				playerToGiveTo.SaveToApplication();
			}
			else
			{
				// Tell the player he passed the item over
				this.sendMessageToPlayer(this.Formatter.Italic($"You gave {playerToGiveTo.GetFullName()} {itemToGive.SingularPronoun} \"{itemToGive.ItemName}\""));

				// Remove item from current player
				this.RemoveOwnedItem(itemToGive.ItemID);

				// Make sure the player now knows what he has.
				this.SaveToApplication();
				playerToGiveTo.SaveToApplication();

				// Respond to the action
				SMNPC smn = this.GetRoom().GetNPCs().FirstOrDefault(smc => smc.GetFullName().ToLower() == playerToGiveTo.GetFullName().ToLower());
				if (smn != null)
				{
					NPCHelper.StartAnNPCReactionCheck(smn, "PlayerCharacter.GivesItemToThem", this, itemToGive);
				}
			}
		}

		/// <summary>
		/// Lists the characters inventory slot by slot. If a slot is specified by name only details of that slot are listed, but it is done recursivly.
		/// </summary>
		/// <param name="slotName">Optional name of the slot to list.</param>
		public void ListInventory(string slotName = null)
		{
			string inventory = this.Formatter.Bold("Your Inventory:", 1);

			// Weight Indicator
			inventory += this.Formatter.Italic($"Weight: {this.GetCurrentWeight()} / {this.WeightLimit}", 2);

			// Append details of the characters actual inventory
			if (slotName == null)
			{
				foreach (SMSlot slot in this.Slots)
				{
					inventory += this.ListSlotDetails(slot.Name);
				}
			}
			else
			{
				inventory += this.ListSlotDetails(slotName, true);
			}

			// TODO include details of items within clothing

			this.sendMessageToPlayer(inventory);
		}

		/// <summary>
		/// Gets a list of the contents of each slot, optionally listing containet contents.
		/// </summary>
		/// <param name="listContainerContents">Optionally list containet contents.</param>
		/// <returns></returns>
		public List<string> GetInventoryList(bool listContainerContents = false)
		{
			List<string> inventory = new List<string>();

			if (this.Slots != null && this.Slots.Any())
			{
				foreach (SMSlot slot in this.Slots)
				{
					inventory.Add(this.ListSlotDetails(slot.Name, listContainerContents));
				}
			}

			return inventory;
		}

		/// <summary>
		/// Gets an SMSlot by name, case insensitive.
		/// </summary>
		/// <returns>The SMSlot by name.</returns>
		/// <param name="slotName">Name of the slot.</param>
		public SMSlot GetSlotByName(string slotName)
		{
			if (this.Slots != null)
			{
				return this.Slots.FirstOrDefault(sms => sms.Name.ToLower() == slotName.ToLower());
			}

			return null;
		}

		/// <summary>
		/// Gets an equipped item by searching all slots for a given identifier. If not identifier is given the item in the first non-empty slot is returned.
		/// </summary>
		/// <param name="itemIdentifier"></param>
		/// <returns>The equipped item or null.</returns>
		public SMItem GetEquippedItem(string itemIdentifier = null)
		{
			SMItem item = null;

			if (this.Slots != null)
			{
				foreach (SMSlot slot in this.Slots)
				{
					if (!slot.isEmpty())
					{
						if (itemIdentifier == null)
						{
							return slot.EquippedItem;
						}
						else if (SMItemHelper.ItemMatches(slot.EquippedItem, itemIdentifier))
						{
							item = slot.EquippedItem;
							break;
						}
					}
				}
			}

			return item;
		}

		/// <summary>
		/// Counts the number of items owned by the character that match a given id, name or family.
		/// </summary>
		/// <param name="itemIdentifier">Id, name or family to match.</param>
		/// <returns>Coutn of matching items.</returns>
		public int CountOwnedItems(string itemIdentifier)
		{
			int count = 0;

			foreach (SMSlot slot in this.Slots)
			{
				if (!slot.isEmpty())
				{
					if (SMItemHelper.ItemMatches(slot.EquippedItem, itemIdentifier))
					{
						count++;
					}

					if (slot.EquippedItem.CanHoldOtherItems())
					{
						count += SMItemHelper.CountItemsInContainer(slot.EquippedItem, itemIdentifier);
					}
				}
			}

			// TODO account for slot in clothing e.g. pockets

			return count;
		}

		/// <summary>
		/// Finds an item in the current room with a matching identifier.
		/// </summary>
		/// <returns>The item in room.</returns>
		/// <param name="itemIdentifier">Items identifier (id, name, or family).</param>
		public SMItem FindItemInRoom(string itemIdentifier)
		{
			return SMItemHelper.GetItemFromList(this.GetRoom().RoomItems, itemIdentifier);
		}

		/// <summary>
		/// Finds a container in the current room with a matching identifier.
		/// </summary>
		/// <returns>The container in room.</returns>
		/// <param name="itemIdentifier">Items identifier (id, name, or family).</param>
		private SMItem FindContainerInRoom(string itemIdentifier)
		{
			SMItem container = SMItemHelper.GetItemFromList(this.GetRoom().RoomItems, itemIdentifier);

			if (container != null && container.CanHoldOtherItems())
			{
				return container;
			}

			return null;
		}

		/// <summary>
		/// Gets an item owned by the character (from any slot, container or clothing)
		/// </summary>
		/// <returns>The owned item.</returns>
		/// <param name="itemIdentifier">Item identifier.</param>
		public SMItem GetOwnedItem(string itemIdentifier)
		{
			// Check in each slot (not recursive)
			foreach (SMSlot slot in this.Slots)
			{
				if (!slot.isEmpty())
				{
					if (SMItemHelper.ItemMatches(slot.EquippedItem, itemIdentifier))
					{
						return slot.EquippedItem;
					}
				}
			}

			// Check in any equipped containers (recursive)
			SMItem item = null;

			foreach (SMSlot slot in this.Slots)
			{
				if (!slot.isEmpty() && slot.EquippedItem.CanHoldOtherItems())
				{
                    if (slot.EquippedItem.HeldItems != null)
                    {
                        if (slot.EquippedItem.HeldItems.Any())
                        {
                            item = SMItemHelper.GetItemFromList(slot.EquippedItem.HeldItems, itemIdentifier);

                            if (item != null)
                            {
                                return item;
                            }
                        }
                    }
				}
			}

			// TODO check clothing and in clothing (e.g. pockets)

			return null;
		}

		/// <summary>
		/// Gets an equippable item owned by the character.
		/// </summary>
		/// <returns>The equippable item.</returns>
		/// <param name="itemIdentifier">Item identifier.</param>
		private SMItem GetEquippableItem(string itemIdentifier, out SMSlot slotContainingItem)
		{
			// TODO check slots in clothing, e.g. pockets

			foreach (SMSlot slot in this.Slots)
			{
				if (!slot.isEmpty() && slot.EquippedItem.CanHoldOtherItems() && slot.EquippedItem.HeldItems != null)
				{
					SMItem item = SMItemHelper.GetItemFromList(slot.EquippedItem.HeldItems, itemIdentifier);

					if (item != null)
					{
						slotContainingItem = slot;
						return item;
					}
				}
			}

			slotContainingItem = null;
			return null;
		}

		/// <summary>
		/// Lists the contents of a given slot, optionally including container contents.
		/// </summary>
		/// <param name="slotName">The name of the slot to list.</param>
		/// <param name="listContainerContents"></param>
		/// <returns></returns>
		private string ListSlotDetails(string slotName, bool listContainerContents = false)
		{
			SMSlot slot = this.GetSlotByName(slotName);

			if (slot == null)
			{
				return this.Formatter.Italic($"Sorry unable to find a slot called \"{Utils.SanitiseString(slotName)}\"!", 1);
			}

			string listing = string.Empty;

			listing += this.Formatter.Bold($"{slot.GetReadableName()}: ", 0);
			listing += this.Formatter.General($" {slot.GetEquippedItemName()}", 1);

			// If the item in the slot is a container list its contents if required
			if (listContainerContents && slot.EquippedItem != null && slot.EquippedItem.CanHoldOtherItems())
			{
				if (slot.EquippedItem.HeldItems != null && slot.EquippedItem.HeldItems.Any())
				{
					listing += this.Formatter.Italic($"Capacity: {SMItemHelper.GetItemUsedCapacity(slot.EquippedItem)} / {slot.EquippedItem.ItemCapacity}", 1);
					listing += this.Formatter.Italic($"This \"{slot.EquippedItem.ItemName}\" contains the following items:", 1);
					listing += SMItemHelper.GetContainerContents(slot.EquippedItem);
				}
				else
				{
					listing += this.Formatter.Italic($"This \"{slot.EquippedItem.ItemName}\" is empty.", 1);
				}
			}

			return listing;
		}

		/// <summary>
		/// Removes an equipped item by searching all slots for a given identifier.
		/// </summary>
		/// <param name="itemIdentifier"></param>
		private void RemoveEquippedItem(string itemIdentifier)
		{
			foreach (SMSlot slot in this.Slots)
			{
				if (!slot.isEmpty())
				{
					if (SMItemHelper.ItemMatches(slot.EquippedItem, itemIdentifier))
					{
						slot.EquippedItem = null;
						this.SaveToApplication();
						break;
					}
				}
			}
		}

		/// <summary>
		/// Removes an owned item by searching all slots and their contents for a given identifier.
		/// </summary>
		/// <param name="itemIdentifier"></param>
		public void RemoveOwnedItem(string itemIdentifier, bool tellPlayer = false)
		{
            bool haveRemovedItem = false;

			// Check in each slot (not recursive)
			foreach (SMSlot slot in this.Slots)
			{
				if (!slot.isEmpty())
				{
					if (SMItemHelper.ItemMatches(slot.EquippedItem, itemIdentifier))
					{
						slot.EquippedItem = null;
                        haveRemovedItem = true;
                        this.SaveToApplication();
						break;
					}
				}
			}

			// Check in any equipped containers (recursive)
			foreach (SMSlot slot in this.Slots)
			{
				if (!slot.isEmpty())
				{
					// Look inside the equipped item if it is a container
					if (slot.EquippedItem.CanHoldOtherItems() && slot.EquippedItem.HeldItems != null)
					{
                        if (slot.EquippedItem.HeldItems.Any())
                        {
                            if (SMItemHelper.GetItemFromList(slot.EquippedItem.HeldItems, itemIdentifier) != null)
                            {
                                SMItemHelper.RemoveItemFromList(slot.EquippedItem.HeldItems, itemIdentifier);
                                haveRemovedItem = true;
                                this.SaveToApplication();
                                break;
                            }
                        }
					}
				}
			}

			// TODO check in clothing slots e.g. pockets

            // Announce it to the player if needed
            if (tellPlayer && haveRemovedItem)
            {
                // Send the message
                this.sendMessageToPlayer("[i]Removed " + itemIdentifier + " from inventory.[/i]");
            }
		}

		/// <summary>
		/// Checks if the characters has an item of a given type equipped.
		/// </summary>
		/// <returns><c>true</c>, if item of given type is equipped, <c>false</c> otherwise.</returns>
		/// <param name="type">ItemType.</param>
		public bool HasItemTypeEquipped(string type)
		{
			foreach (SMSlot slot in Slots)
			{
				if (!slot.isEmpty() && slot.EquippedItem.ItemType.ToLower() == type.ToLower())
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks in the character has an item equipped matching a given identifier (id, name, family)
		/// </summary>
		/// <returns><c>true</c>, if a matching item is equipped, <c>false</c> otherwise.</returns>
		/// <param name="familyType">Item identifier.</param>
		public bool HasItemEquipped(string itemIdentifier)
		{
			foreach (SMSlot slot in Slots)
			{
				if (!slot.isEmpty() && SMItemHelper.ItemMatches(slot.EquippedItem, itemIdentifier))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Are the characters hands empty.
		/// </summary>
		/// <returns><c>true</c>, if hands are empty, <c>false</c> otherwise.</returns>
		public bool AreHandsEmpty()
		{
			SMSlot rightHand = this.GetSlotByName("RightHand");
			SMSlot leftHand = this.GetSlotByName("LeftHand");

			if ((rightHand == null) && (leftHand == null))
			{
				return false;
			}

			return rightHand.isEmpty() && leftHand.isEmpty();
		}

        /// <summary>
        /// Destroy an item in your posession, so we don't have to continually drop quivers everywhere
        /// </summary>
        /// <param name="itemIdentifier">The identified of the item</param>
        public void DestroyItem(string itemIdentifier)
        {
            // Remove the owned item
            RemoveOwnedItem(itemIdentifier, true);
            this.SaveToFile();
        }

		#endregion

		#region "Inventory Functions Old"

		// TODO will refactor when addressing locks and keys on containers
		/// <summary>
		/// Finds an item by AdditionalData in a container by recursivly searching through the container
		/// and any containers it contains.
		/// </summary>
		/// <returns>The item in a container.</returns>
		/// <param name="name">additionalData.</param>
		/// <param name="container">Container to look in.</param>
		private SMItem FindItemInContainerByAdditionalData(string additionalData, SMItem container)
		{
			if (container.HeldItems != null)
			{
				foreach (SMItem item in container.HeldItems)
				{
					if (item.AdditionalData != null)
					{
						if (item.AdditionalData.ToLower() == additionalData.ToLower())
						{
							return item;
						}
					}
					
					if (item.ItemType == "container")
					{
						SMItem smi = this.FindItemInContainerByAdditionalData(additionalData, item);
						if (smi != null)
						{
							return smi;
						}
					}
				}
			}

			return null;
		}

		// TODO will refactor when addressing locks and keys on containers
		/// <summary>
		/// Tests if the character has a key for a location
		/// </summary>
		/// <param name="LocationName">The location name for the key</param>
		/// <returns>True/False result of ownership test.</returns>
		private bool CheckKey(string lockedKeyCode)
        {
            foreach (SMSlot slot in Slots)
            {
                if (!slot.isEmpty())
                {
                    if (slot.EquippedItem.AdditionalData == lockedKeyCode)
                    {
                        return true;
                    }

                    if (slot.EquippedItem.ItemType.ToLower() == "container")
                    {
                        SMItem item = this.FindItemInContainerByAdditionalData(lockedKeyCode, slot.EquippedItem);
                        if (item != null)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // TODO will refactor when addressing useable items
        /// <summary>
        /// Remove an item by it's name.
        /// </summary>
        /// <param name="name">Name of the item that is going to be removed</param>
        /// <param name="includeRoom">Check the items in the room as well as the character</param>
        /// <returns></returns>
        public bool RemoveItem(string name, bool includeRoom)
        {
            // Scroll around the character slots and remove an item.
            foreach (SMSlot slot in Slots)
            {
                if (!slot.isEmpty())
                {
                    if (slot.EquippedItem.ItemName.ToLower() == name.ToLower())
                    {
                        slot.EquippedItem = null;
                        this.SaveToApplication();
                        this.SaveToFile();
                        return true;
                    }
                    else if (slot.EquippedItem.ItemFamily.ToLower() == name.ToLower())
                    {
                        slot.EquippedItem = null;
                        this.SaveToApplication();
                        this.SaveToFile();
                        return true;
                    }

                    if (slot.EquippedItem.ItemType.ToLower() == "container")
                    {
                        foreach (SMItem item in slot.EquippedItem.HeldItems)
                        {
                            if ((item.ItemName.ToLower() == name.ToLower()) || (item.ItemFamily.ToLower() == name.ToLower()))
                            {
                                slot.EquippedItem.HeldItems.Remove(item);
                                this.SaveToApplication();
                                this.SaveToFile();
                                return true;
                            }
                        }
                    }
                }
            }

            // Remove items from the room.
            if (includeRoom)
            {
                // Scroll around all the items in the room
                foreach (SMItem item in this.GetRoom().RoomItems)
                {
                    // Remove items that are just in the room by themselves...
                    if ((item.ItemName.ToLower() == name.ToLower()) || (item.ItemFamily.ToLower() == name.ToLower()))
                    {
                        this.GetRoom().RoomItems.Remove(item);
                        return true;
                    }

                    // If there is a container in the room then you can look in that too
                    // TODO : Add a check to ensure that they're not locked, if they are the character needs the key on their chain to access it.
                    if (item.ItemType.ToLower() == "container")
                    {
                        // Scroll around the inner items remove any that are there..
                        foreach (SMItem innerItem in item.HeldItems)
                        {
                            // Check the name / family name and remove it if it's the right type.
                            if ((innerItem.ItemName.ToLower() == name.ToLower()) || (innerItem.ItemFamily.ToLower() == name.ToLower()))
                            {
                                item.HeldItems.Remove(innerItem);
                                return true;
                            }
                        }
                    }
                }
            }

            // Finally return false if the item isn't found.
            return false;
        }

        #endregion

        #region "Chat Functions"

        /// <summary>
        /// Make the character say something
        /// </summary>
        /// <param name="speech">What the character is saying</param>
        public void Say(string speech)
		{
            new SlackMud().GetRoom(this.RoomID).ChatSay(speech, this);
		}

		/// <summary>
		/// Make the character Whisper something
		/// </summary>
		/// <param name="speech">What the character is whispering</param>
		/// /// <param name="whisperToName">Who the character is whispering to (name)</param>
		public void Whisper(string speech, string whisperToName)
		{
			new SlackMud().GetRoom(this.RoomID).ChatWhisper(speech, this, whisperToName);
		}

		/// <summary>
		/// Make the character Shout something
		/// </summary>
		/// <param name="speech">What the character is shouting</param>
		public void Shout(string speech)
		{
			new SlackMud().GetRoom(this.RoomID).ChatShout(speech, this);
		}

		/// <summary>
		/// Make the character emote something
		/// </summary>
		/// <param name="thingtoEmote">What the character is emoting</param>
		public void Emote(string thingtoEmote)
		{
			new SlackMud().GetRoom(this.RoomID).ChatEmote(thingtoEmote, this);
		}

		/// <summary>
		/// Announce something to the player
		/// </summary>
		/// <param name="announcement">What is being announced to the player</param>
		public void Announce(string announcement)
		{
			sendMessageToPlayer(announcement);
		}

		/// <summary>
		/// Send the message to the player
		/// </summary>
		/// <param name="message">the message being sent to the player</param>
		public void sendMessageToPlayer(string message)
		{
			// TODO Change the name of the service based on the one used to send the information!
			string userIDToSendTo = this.UserID;

			Commands.SendMessage(this.ConnectionService, "SlackMud", message, "SlackMud", userIDToSendTo, this.ResponseURL);
		}

		/// <summary>
		/// Enable or disable the tips
		/// </summary>
		/// <param name="enabledisable">Expected: on or off</param>
		public void NewbieTips(string enabledisable)
		{
			switch (enabledisable.ToLower())
			{
				case "on":
					this.NewbieTipsOn();
					break;
				case "off":
					this.NewbieTipsOff();
					break;
				default:
					this.sendMessageToPlayer(this.Formatter.Italic("Please specify \"on\" or \"off\" when using the newbietips command."));
					break;
			}
		}

		/// <summary>
		/// Enable newbie tips if they have been disabled.
		/// </summary>
		private void NewbieTipsOn()
		{
			this.NewbieTipsDisabled = false;
			this.sendMessageToPlayer(this.Formatter.Italic("Newbie tips enabled [to disable them again type \"newbietips off\"]"));
		}

		/// <summary>
		/// Disable newbie tips.
		/// </summary>
		private void NewbieTipsOff()
		{
			this.NewbieTipsDisabled = true;
			this.sendMessageToPlayer(this.Formatter.Italic("Newbie tips disabled [to enable them again type \"newbietips on\"]"));
		}

		/// <summary>
		/// Sends an ooc message to the zone you're in ...
		/// </summary>
		/// <param name="message">the message being sent to the player</param>
		public void SendOOC(string message)
		{
			List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
			string region = "GLOBAL";
			string currentRoomName = this.GetRoom().RoomID;
            string currentArea = currentRoomName.Substring(0, currentRoomName.IndexOf('.'));
			region = currentArea;
			smcs = smcs.FindAll(smc => smc.RoomID.Substring(0, smc.RoomID.IndexOf('.')) == currentArea);

			foreach (SMCharacter smc in smcs)
			{
				smc.sendMessageToPlayer(this.Formatter.Italic(this.GetFullName() + " OOC[" + currentRoomName + "]: " + message));
			}
		}

        /// <summary>
		/// Sends an ooc message globally
		/// </summary>
		/// <param name="message">the message being sent to the player</param>
        public void SendOOCGlobal(string message)
        {
            List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
            string currentRoomName = this.GetRoom().RoomID;
            foreach (SMCharacter smc in smcs)
            {
                smc.sendMessageToPlayer(this.Formatter.Italic(this.GetFullName() + " OOC[" + currentRoomName + "]: " + message));
            }
        }

        #endregion

        #region "NPC Interaction"

        /// <summary>
        /// Add an awaiting response item
        /// </summary>
        /// <param name="NPCID">The id of the NPC awaiting the response</param>
        /// <param name="timeOut">The timeout for the response (unix time) response</param>
        public void SetAwaitingResponse(string NPCID, List<ShortcutToken> shortCutTokens, int timeOut, string roomID = null)
		{
			if (this.NPCsWaitingForResponses == null)
			{
				this.NPCsWaitingForResponses = new List<AwaitingResponseFromCharacter>();
			}

			AwaitingResponseFromCharacter arfc = new AwaitingResponseFromCharacter();
			arfc.NPCID = NPCID;
			arfc.ShortCutTokens = shortCutTokens;
			arfc.TimeOut = Utility.Utils.GetUnixTimeOffset(timeOut);
			arfc.RoomID = roomID;

			// Remove any responses that are waiting from the character with these options already
			this.NPCsWaitingForResponses.RemoveAll(responseToCheck => ((responseToCheck.NPCID == NPCID) && (responseToCheck.ShortCutTokens == shortCutTokens)));

			// Add the new set in.
			this.NPCsWaitingForResponses.Add(arfc);
			this.SaveToApplication();
			this.SaveToFile();
		}

		/// <summary>
		/// Expire responses that are no longer needed
		/// </summary>
		private void ExpireResponse(bool clearRoomResponses = false)
		{
			// Get the current unix time
			int currentUnixTime = Utility.Utils.GetUnixTime();

			// Delete all awaiting responses after time
			if (this.NPCsWaitingForResponses != null)
			{
				this.NPCsWaitingForResponses.RemoveAll(awaitingitems => awaitingitems.TimeOut < currentUnixTime);

				if (clearRoomResponses)
				{
					this.NPCsWaitingForResponses.RemoveAll(awaitingitems => awaitingitems.RoomID == this.RoomID);
				}
			}
		}

		/// <summary>
		/// Process a response to a question
		/// </summary>
		/// <param name="responseShortcut">The shortcut for the response</param>
		public void ProcessResponse(string responseShortcut)
		{
			// Responded to player
			bool respondedToPlayer = false;
			
			ExpireResponse();

            if (this.NPCsWaitingForResponses == null)
            {
                this.NPCsWaitingForResponses = new List<AwaitingResponseFromCharacter>();
            }

			// If there are still characters awaiting responses
			if (this.NPCsWaitingForResponses.Count() > 0)
			{
				// First check if we are awaiting any responses with that shortcut token (and aren't timed out)
				List<AwaitingResponseFromCharacter> NPCIDs = this.NPCsWaitingForResponses.FindAll(awaitingitems => awaitingitems.ShortCutTokens.Count(sct => sct.ShortCutToken.ToLower() == responseShortcut.ToLower()) > 0);

				// If there is something waiting
				if (NPCIDs.Count > 0)
				{
					// Get the NPCs
					List<SMNPC> lNPCs = new List<SMNPC>();
					lNPCs = (List<SMNPC>)HttpContext.Current.Application["SMNPCs"];

					foreach (AwaitingResponseFromCharacter NPC in NPCIDs)
					{
						// Find out if the NPC is in the same room as the character
						SMNPC targetNPC = lNPCs.FirstOrDefault(npc => ((npc.UserID == NPC.NPCID) && (npc.RoomID == this.RoomID)));
						if (targetNPC != null)
						{
							// Process the response.
							targetNPC.ProcessCharacterResponse(responseShortcut, this);
							respondedToPlayer = true;
						}
					}

				}
			}
            
            if (!respondedToPlayer)
			{
				this.sendMessageToPlayer(this.Formatter.Italic("Can not find a waiting response with that shortcut... did you wait too long to respond?"));
			}
		}

		/// <summary>
		/// Gets a list of NPC awaiting responses relavent to the characters current room
		/// </summary>
		/// <returns></returns>
		public List<AwaitingResponseFromCharacter> GetAwaitingResponsesForRoom()
		{
			string roomId = this.GetRoom().RoomID;

			if (this.NPCsWaitingForResponses != null)
			{
				List<AwaitingResponseFromCharacter> awaitingResponses = this.NPCsWaitingForResponses.FindAll(resp => resp.RoomID == roomId);

				if (awaitingResponses != null && awaitingResponses.Any())
				{
					return awaitingResponses;
				}
			}

			return null;
		}

		public void ClearResponses()
		{
			int currentTime = Utility.Utils.GetUnixTime();
			if (NPCsWaitingForResponses != null)
			{
				this.NPCsWaitingForResponses.RemoveAll(r => r.TimeOut < currentTime);
				this.SaveToApplication();
				this.SaveToFile();
			}
		}

        public void HailNPC(string hailTarget)
        {
            // First get all the NPCs in the room with that name
            SMNPC npc = this.GetRoom().GetNPCs().FirstOrDefault(n => (n.FirstName.ToLower() == hailTarget.ToLower()) || (n.LastName.ToLower() == hailTarget.ToLower()) || (n.GetFullName().ToLower() == hailTarget.ToLower()) || (n.FamilyType.ToLower() == hailTarget.ToLower()));

            // Spoken to someone
            bool spokenToSomeone = false;

            // If there is someone in the list
            if (npc != null)
            {
				// NOTE: Potentially extract into a global strings JSON
				this.sendMessageToPlayer("[i]" + this.GetFullName() + " says:[/i] \"Hail " + npc.GetFullName() + "\"");
				bool responded = npc.RespondToAction("PlayerCharacter.Hail", this);
				spokenToSomeone = true;
				if (!responded)
				{
					// NOTE: Potentially extract into a global strings JSON
					this.sendMessageToPlayer(string.Format("[i]{0} doesn't seem to respond to your hail.[/i]", npc.GetFullName()));
				}
            }
            else // Check if there is a player of the name in the room.
            {
				SMCharacter smc = this.GetRoom().GetPeople().FirstOrDefault(p => (p.FirstName.ToLower() == hailTarget.ToLower()) || (p.LastName.ToLower() == hailTarget.ToLower()) || (p.GetFullName().ToLower() == hailTarget.ToLower()));

				if (smc != null)
				{
					this.Say("Hail " + smc.GetFullName());
					spokenToSomeone = true;
				}
            }

			if (!spokenToSomeone)
			{
				this.sendMessageToPlayer("[i]Can not hail \"" + hailTarget + "\"[/i]");
			}
        }

		#endregion

		#region "Quests"

		public void AddQuest(SMQuest smq)
		{
			// Add the item to the quest log
			if (this.QuestLog == null)
			{
				this.QuestLog = new List<SMQuestStatus>();
			}

			// Check the quest log doesn't already include a quest with the name already..
			// .. i.e. you can't have it twice at the same time!
			if (this.QuestLog.Count(q => q.QuestName == smq.QuestName) == 0)
			{
				// Construct a new status based on the quest
				SMQuestStatus smqs = new SMQuestStatus();
				smqs.Completed = false;
				smqs.Expires = 0;
				smqs.LastDateUpdated = Utility.Utils.GetUnixTime();
				smqs.QuestName = smq.QuestName;
				smqs.QuestStep = smq.QuestSteps.First().Name;
				smqs.QuestStepInstructions = smq.QuestSteps.First().Instructions;
                smqs.Daily = smq.Daily;
                
				this.QuestLog.Add(smqs);

				// Tell the player the new quest has been added
				this.sendMessageToPlayer(this.Formatter.Italic($"Quest \"{smqs.QuestName}\" added to quest log."));

				this.SaveToApplication();
				this.SaveToFile();
			}
		}

		public void UpdateQuest(SMQuest smq)
		{
			// Get the quest status from the log
			if (this.QuestLog == null)
			{
				this.QuestLog = new List<SMQuestStatus>();
			}
			SMQuestStatus smqs = this.QuestLog.FirstOrDefault(quest => quest.QuestName == smq.QuestName);

			if (!smqs.Completed)
			{
				// Move the quest steps along by one.
				SMQuestStep currentStep = null;
				SMQuestStep nextStep = null;
				foreach (SMQuestStep step in smq.QuestSteps)
				{
					if (currentStep != null)
					{
						nextStep = step;
						break;
					}

					if (step.Name == smqs.QuestStep)
					{
						currentStep = step;
					}
				}

				// Move the step along to the next one if there is a new one..
				if (nextStep != null)
				{
					smqs.QuestStep = nextStep.Name;
					smqs.QuestStepInstructions = nextStep.Instructions;
					this.sendMessageToPlayer(this.Formatter.Italic($"Quest \"{smq.QuestName}\" updated."));
					this.sendMessageToPlayer(this.Formatter.Italic($"Next step \"{nextStep.Instructions}\"."));
				}
				else // Finish the quest!
				{
					this.sendMessageToPlayer(this.Formatter.Italic($"Quest \"{smq.QuestName}\" completed!"));
					smqs.Completed = true;

					// Get the rewards
					if (smq.Rewards != null)
					{
						foreach (SMQuestReward reward in smq.Rewards)
						{
							switch (reward.Type.ToLower())
							{
								case "item":
									string[] itemInfo = reward.AdditionalData.Split(',');
									int numberToGive = int.Parse(itemInfo[1]);
									string[] itemParts = itemInfo[0].Split('.');

									while (numberToGive > 0)
									{
										// Get the item (with a new GUID)
										SMItem itemBeingGiven = SMItemFactory.Get(itemParts[0], itemParts[1]);

										// Pass it to the player
										this.ReceiveItem(itemBeingGiven);

										numberToGive--;
									}
									
									break;
                                case "key":
                                    // Get the item (with a new GUID)
                                    SMItem keyBeingGiven = SMItemFactory.Get("Misc", "Key");
                                    string[] keyInfo = reward.AdditionalData.Split('.');
                                    keyBeingGiven.ItemDescription = keyInfo[0];
                                    keyBeingGiven.ItemName = keyInfo[0];
                                    keyBeingGiven.AdditionalData = keyInfo[1];

                                    // Pass the key to the player
									this.ReceiveItem(keyBeingGiven);

                                    break;
                                case "money":
                                    // Get the item (with a new GUID)
                                    this.Currency.AddCurrency(int.Parse(reward.AdditionalData));
                                    this.sendMessageToPlayer("[i]Received " + reward.AdditionalData + " marks[/i]");

                                    break;
                            }
						}
					}
				}
			}
			
			this.SaveToApplication();
			this.SaveToFile();
		}

		public void ClearQuests()
		{
			int currentTime = Utility.Utils.GetUnixTime();
			if (this.QuestLog != null)
			{
				this.QuestLog.RemoveAll(quest => (quest.Expires < currentTime) && (quest.Expires != 0));
				this.SaveToApplication();
				this.SaveToFile();
			}
		}

		public void GetQuestLog(bool getCompleted = false)
		{
			// Prepare the quest log string:
			string questLogResponse = "";

			if (this.QuestLog != null)
			{
				List<SMQuestStatus> questStatusList = this.QuestLog;
				IOrderedEnumerable<SMQuestStatus> statusList = questStatusList.OrderBy(qs => qs.Completed);
				
				foreach (SMQuestStatus smq in statusList)
				{
					if ((smq.Completed != true) || (getCompleted))
					{
						questLogResponse += this.Formatter.ListItem(this.Formatter.Italic($"{smq.QuestName} - {smq.QuestStep} ({smq.QuestStepInstructions})"), 0);
					}
				}

				if (questLogResponse == "")
				{
					questLogResponse = this.Formatter.Italic("You do not have any quests in progress.");
				}

				questLogResponse = this.Formatter.Bold("Quest log:") + $" {questLogResponse}";
			}
			else
			{
				if (!getCompleted)
				{
					questLogResponse = this.Formatter.Italic("You do not have any quests in progress.");
				}
				else
				{
					questLogResponse = this.Formatter.Italic("You have not completed any quests yet.");
				}
			}

			this.sendMessageToPlayer(questLogResponse);
		}

		public void GetQuestLogHistory()
		{
			GetQuestLog(true);
		}

        #endregion

        #region "Parties"

        public void InviteToParty(string characterName, bool suppressMessages = false)
        {
            new SMParty().InviteToParty(this, characterName, suppressMessages);
        }

        public void AcceptPartyInvite(bool suppressMessages = false)
        {
            new SMParty().JoinParty(this, suppressMessages);
        }

        public void LeaveParty(bool suppressMessages = false)
        {
            new SMParty().LeaveParty(this, suppressMessages);
        }

        public void CreateParty(bool suppressMessages = false)
        {
            new SMParty().CreateParty(this, suppressMessages);
        }

        public void WhoIsInParty()
        {
            new SMParty().FindAllPartyMembers(this);
        }

        #endregion

        #region "Text Helper Functions"

        public string GetHisHerPronoun()
        {
            switch (Char.ToLower(Sex))
            {
                case 'm':
                    return "his";
                case 'f':
                    return "her";
                default:
                    return "their";
            }
        }

        #endregion "Text Helper Functions"
    }

    #region "Other Class Structures"

    public class AwaitingResponseFromCharacter
	{
		public string NPCID { get; set; }
		public List<ShortcutToken> ShortCutTokens { get; set; }
		public string RoomID { get; set; }
		public int TimeOut { get; set; }
	}

	public class ShortcutToken
	{
		public string ShortCutToken;
	}

	public class PlayerNote
	{
		public string Note;
	}

	#endregion
}
