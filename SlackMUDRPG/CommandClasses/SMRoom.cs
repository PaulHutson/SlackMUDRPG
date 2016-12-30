using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.CommandsClasses
{
    public class SMRoom
    {
		[JsonProperty("RoomID")]
		public string RoomID { get; set; }

		[JsonProperty("RoomLocationX")]
        public int RoomLocationX { get; set; }

        [JsonProperty("RoomLocationY")]
        public int RoomLocationY { get; set; }

        [JsonProperty("RoomLocationZ")]
        public int RoomLocationZ { get; set; }

        [JsonProperty("RoomDescription")]
        public string RoomDescription { get; set; }

        [JsonProperty("RoomExits")]
        public List<SMExit> RoomExits { get; set; }

        [JsonProperty("RoomItems")]
        public List<SMItem> RoomItems { get; set; }

		/// <summary>
		/// Adds an item to the room, so any user can collect it.
		/// </summary>
		/// <param name="item">Item.</param>
		public void AddItem(SMItem item)
		{
			if (this.RoomItems == null)
			{
				this.RoomItems = new List<SMItem>();
			}

			this.RoomItems.Add(item);
			this.SaveToApplication();
		}

		/// <summary>
		/// Saves the room to file.
		/// </summary>
		public void SaveToFile()
		{
			string path = FilePathSystem.GetFilePath("Location", "Loc" + this.RoomID);
			string roomJSON = JsonConvert.SerializeObject(this, Formatting.Indented);

			using (StreamWriter w = new StreamWriter(path))
			{
				w.WriteLine(roomJSON);
			}
		}

		/// <summary>
		/// Saves the room to application memory.
		/// </summary>
		public void SaveToApplication()
		{
			List<SMRoom> smrs = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];

			SMRoom roomInMem = smrs.FirstOrDefault(smr => smr.RoomID == this.RoomID);

			if (roomInMem != null)
			{
				smrs.Remove(roomInMem);
			}

			smrs.Add(this);
			HttpContext.Current.Application["SMRooms"] = smrs;
		}
    }

    public class SMExit
    {
        [JsonProperty("Shortcut")]
        public string Shortcut { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
        
        [JsonProperty("RoomID")]
        public string RoomID { get; set; }
    }
}