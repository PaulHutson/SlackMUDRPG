﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.CommandsClasses
{
    public class SMCharacter
    {
        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }

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

        [JsonProperty("RoomID")]
        public string RoomID { get; set; }

        [JsonProperty("CharacterItems")]
        public List<SMItem> CharacterItems { get; set; }

        [JsonProperty("Skills")]
        public List<SMSkill> Skills { get; set; }

		/// <summary>
		/// Adds the item to the characters CharacterItems list.
		/// </summary>
		/// <param name="item">Item.</param>
		public void AddItem(SMItem item)
		{
			if (this.CharacterItems == null)
			{
				this.CharacterItems = new List<SMItem>();
			}

			//TODO check that tha play has weight and capacity to add the item

			this.CharacterItems.Add(item);
			this.SaveToApplication();
		}

		/// <summary>
		/// Removes and item by ItemId from the characters CharacterItems list dropping the item to the characters current room.
		/// </summary>
		/// <param name="Id">ItemId</param>
		public void DropItem(string id)
		{
			if (this.CharacterItems == null)
			{
				return;
			}

			SMItem item = this.CharacterItems.Find(obj => obj.ItemId == id);

			if (item != null)
			{
				SMRoom room = this.GetRoom();

				room.AddItem(item);
				this.CharacterItems.Remove(item);
				this.SaveToApplication();
			}
		}

		/// <summary>
		/// Saves the character to the file system.
		/// </summary>
		public void SaveToFile()
		{
			string path = FilePathSystem.GetFilePath("Characters", "Char" + this.UserID);
			string charJSON = JsonConvert.SerializeObject(this, Formatting.Indented);

			using (StreamWriter w = new StreamWriter(path))
			{
				w.WriteLine(charJSON);
			}
		}

		/// <summary>
		/// Saves the character to application memory.
		/// </summary>
		public void SaveToApplication()
		{
			List<SMCharacter> smcs = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];

			if (smcs.FirstOrDefault(smc => smc.UserID == this.UserID) != null)
			{
				SMCharacter charToRemove = smcs.SingleOrDefault(smc => smc.UserID == this.UserID);
				if (charToRemove != null)
				{
					smcs.Remove(charToRemove);
				}
			}

			smcs.Add(this);
			HttpContext.Current.Application["SMCharacters"] = smcs;
		}

		/// <summary>
		/// Gets the characters current room, loads from mem or file as required
		/// </summary>
		public SMRoom GetRoom()
		{
			List<SMRoom> smrs = (List<SlackMUDRPG.CommandsClasses.SMRoom>)HttpContext.Current.Application["SMRooms"];
			SMRoom smr = smrs.FirstOrDefault(obj => obj.RoomID == this.RoomID);

			if (smr != null)
			{
				return smr;
			}
			else
			{
				string path = FilePathSystem.GetFilePath("Locations", "Loc" + this.RoomID);

				if (File.Exists(path))
				{
					using (StreamReader r = new StreamReader(path))
					{
						string json = r.ReadToEnd();
						smr = JsonConvert.DeserializeObject<SMRoom>(json);
						smrs.Add(smr);
						HttpContext.Current.Application["SMRooms"] = smrs;
						return smr;
					}
				}

				return null;
			}
		}
	}
}