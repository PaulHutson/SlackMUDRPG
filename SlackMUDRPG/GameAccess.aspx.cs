using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using SlackMUDRPG.CommandClasses;
using SlackMUDRPG.Utility;
using SlackMUDRPG.Utility.Formatters;

namespace SlackMUDRPG
{
	public partial class GameAccess : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			// Command text (this works for both form submissions and also query strings)
			string commandText = Utils.GetQueryParam("text");

			// UserID (this works for both form submissions and also query strings)
			string UserID = Utils.GetQueryParam("user_id");

			// Login guard, to ensure users are logged in or logging in before processing commands.
			if (commandText.ToLower() != "login" && !this.IsLoggedIn(UserID))
			{
				lit_output.Text = "You must login before you can play!";
				return;
			}

			// Initiate the command processing.
			lit_output.Text = new SMCommandUtility(UserID).InitateCommand(commandText);
		}

		/// <summary>
		/// Checks if a user is logged in by getting their character from memory by UserID.
		/// </summary>
		/// <param name="UserID"></param>
		/// <returns>True if the user is logged in, otherwise false.</returns>
		private bool IsLoggedIn(string UserID)
		{
			List<SMCharacter> smcs = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
			SMCharacter charInMem = smcs.FirstOrDefault(smc => smc.UserID == UserID);

			if (charInMem != null)
			{
				return true;
			}

			return false;
		}
	}
}