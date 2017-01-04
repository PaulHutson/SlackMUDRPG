using SlackMUDRPG.CommandsClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SlackMUDRPG
{
	public partial class GameAccess : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			// Default text to output if everything goes Pete-Tong
			string outputText = "Command not found... try '/sdcommand help' to return a list of possible commands";

			// Other variables needed
			bool error = true;                              // Set the error bool to true because.. actually why do that?
			string botName = "SlackMud";                // Error bot name
			string replyToChannel = "@error";               // Default ReplyToChannel
			string replyToError = outputText;               // Default reply text

			// Get Slack / Other webhook locations
			string serviceType = Request.Form["st"];   // This will only be Slack for now, but could use something else like Discord
			if (serviceType == null)
			{
				serviceType = Request.QueryString["st"];
			}

			// Get Slack Group Name
			string serviceName = Request.Form["sn"];   // This will only be Slack for now, but could use something else like Discord
			if (serviceName == null)
			{
				serviceName = Request.QueryString["sn"];
			}

			// Additional text (this works for both form submissions and also query strings depending on how we're accessing the code)
			string additionalText = Request.Form["text"];   // Additional text that might be needed from the form..
			if (additionalText == null)
			{
				additionalText = Request.QueryString["text"];
			}

			// Test Functionality 
			if ((additionalText == "PMTest"))
			{
				SendPrivateMessage(serviceType, serviceName, "@hutsonphutty");
				SetOutputText("Output On");
			}

			if ((!error) && (replyToChannel != "@error"))
			{
				Commands.SendToChannelMessage(serviceType, serviceName, outputText, botName, replyToChannel); // Note we need to change SD for a configurable one.
			}

		}

		private void SetOutputText(string outputLit)
		{
			lit_output.Text = outputLit;
		}

		private void SendPrivateMessage(string serviceType, string nameOfHook, string toName)
		{
			Commands.SendToChannelMessage(serviceType, nameOfHook, "This is a bit of text", "SlackMud", toName);
		}
	}
}