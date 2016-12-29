using Newtonsoft.Json;
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

        [JsonProperty("userid")]
        public string UserID { get; set; }

        [JsonProperty("RoomLocation")]
        public string RoomLocation { get; set; }

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

			this.SaveCharacter();
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
				this.SaveCharacter();
			}
		}

		/// <summary>
		/// Saves the character to the file system.
		/// </summary>
		public void SaveCharacter()
		{
			string path = FilePathSystem.GetFilePath("Characters", "Char" + this.UserID);
			string charJSON = JsonConvert.SerializeObject(this, Formatting.Indented);

			using (StreamWriter w = new StreamWriter(path))
			{
				w.WriteLine(charJSON);
			}
		}

		/// <summary>
		/// Gets an SMRoom object representing the characters current location.
		/// </summary>
		/// <returns>The room.</returns>
		private SMRoom GetRoom()
		{
			SMRoom room = new SMRoom();

			string path = FilePathSystem.GetFilePath("Locations", "Loc" + this.RoomLocation);

			using (StreamReader r = new StreamReader(path))
			{
				string json = r.ReadToEnd();
				room = JsonConvert.DeserializeObject<SMRoom>(json);
			}

			return room;
		}
	}
}