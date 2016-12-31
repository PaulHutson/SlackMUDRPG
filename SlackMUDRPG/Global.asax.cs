using SlackMUDRPG.CommandsClasses;
using System;
using System.Collections.Generic;
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

            List<SMCharacter> smc = new List<SMCharacter>();
            Application["SMCharacters"] = smc;

			List<SMRoom> smr = new List<SMRoom>();
			Application["SMRooms"] = smr;
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