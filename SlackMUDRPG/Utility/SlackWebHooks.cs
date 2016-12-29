using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility
{
    public static class SlackWebHooks
    {
        public static string GetWebHookToken(string serviceName)
        {
            // Variable for the return string
            string returnString = "";

            // Get the right path, and work out if the file exists.
            string path = FilePathSystem.GetFilePath("SlackWebHooks", "SlackWebHook_" + serviceName);

            // Check if the file exists
            if (!File.Exists(path))
            {
                // If they don't exist inform the requestee as to how to create a new integration
                returnString = "Can't find custom integration file, please create one."; // TODO Implement a way for people to create these.
            }
            else
            {
                // Load the Token Details
                SlackWebHook swh = new SlackWebHook();

                // Use a stream reader to read the file in (based on the path)
                using (StreamReader r = new StreamReader(path))
                {
                    // Create a new JSON string to be used...
                    string json = r.ReadToEnd();

                    // ... get the informaiton from the the room information.
                    swh = JsonConvert.DeserializeObject<SlackWebHook>(json);

                    // Return the room description, exits, people and objects 
                    returnString = swh.Token;
                }
            }

            // Return the text output
            return returnString;
        }
    }

    public class SlackWebHook
    {
        [JsonProperty("Token")]
        public string Token { get; set; }
    }
}