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

			// Initiate the command processing.
			new SMCommandUtility(Utils.GetQueryParam("user_id")).InitateCommand(commandText);
		}
	}
}