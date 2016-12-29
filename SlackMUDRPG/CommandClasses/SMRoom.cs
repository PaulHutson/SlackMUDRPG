using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public static class Movement
    {
        public static string MoveLocation(string charID, string exitShortcut)
        {
            // Create returnString
            string returnString = "";

            // Get the current character location
            

            // Get the exits from that location

            // Get the specific exit from the location referred to by the shortcut

            // Move the player to the other location

            return returnString;
        }
    }

    public class SMRoom
    {
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
    }

    public class SMExit
    {
        [JsonProperty("Shortcut")]
        public string Shortcut { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
        
        [JsonProperty("LocationID")]
        public string LocationID { get; set; }
    }
}