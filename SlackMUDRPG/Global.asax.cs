using Newtonsoft.Json;
using SlackMUDRPG.CommandClasses;
using SlackMUDRPG.CommandsClasses;
using SlackMUDRPG.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace SlackMUDRPG
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            Application["TestOutput"] = 0;

            // Character List
            List<SMCharacter> smc = new List<SMCharacter>();
            Application["SMCharacters"] = smc;

            // Room List
			List<SMRoom> smr = new List<SMRoom>();
			Application["SMRooms"] = smr;

            // Load the Commands into memory for usage later
            // Used for both parsing commands sent in and also for help output
            SMCommands lsmc = new SMCommands();
            string path = FilePathSystem.GetFilePath("Misc", "Help");
            // Check if the character exists..
            if (File.Exists(path))
            {
                // Use a stream reader to read the file in (based on the path)
                using (StreamReader r = new StreamReader(path))
                {
                    // Create a new JSON string to be used...
                    string json = r.ReadToEnd();

                    // ... get the information from the help file
                    lsmc = JsonConvert.DeserializeObject<SMCommands>(json);
                }
            }
            Application["SMCommands"] = lsmc;
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            // If the application closes flush everything to disk.
            List<SMCharacter> smcl = (List<SlackMUDRPG.CommandsClasses.SMCharacter>)HttpContext.Current.Application["SMCharacters"];

            foreach (SMCharacter smc in smcl)
            {
                smc.SaveToFile();
            }

            // TODO Save Rooms
            List<SMRoom> smrl = (List<SlackMUDRPG.CommandsClasses.SMRoom>)HttpContext.Current.Application["SMRooms"];

            foreach (SMRoom smr in smrl)
            {
                smr.SaveToFile();
            }
        }
    }
}