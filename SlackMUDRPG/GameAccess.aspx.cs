using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SlackMUDRPG.CommandClasses;

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
			string serviceType = Request.Form["st"] ?? Request.QueryString["st"];   // This will only be Slack for now, but could use something else like Discord

			// Get Slack Group Name
			string serviceName = Request.Form["sn"] ?? Request.QueryString["sn"];   // This will only be Slack for now, but could use something else like Discord

			// Additional text (this works for both form submissions and also query strings depending on how we're accessing the code)
			string additionalText = Request.Form["text"] ?? Request.QueryString["text"];   // Additional text that might be needed from the form..

            // Additional text (this works for both form submissions and also query strings depending on how we're accessing the code)
            string responseURL = Request.Form["response_url"] ?? Request.QueryString["response_url"] ?? "Mmm";   // Additional text that might be needed from the form..
            
            // Test Functionality 
            //if ((additionalText == "PMTest"))
            //{
            //	SendPrivateMessage(serviceType, serviceName, "@hutsonphutty");
            //}

            Commands.SendMessage(serviceType, serviceName, additionalText, botName, replyToChannel, responseURL);
            
            if ((!error) && (replyToChannel != "@error"))
			{
				Commands.SendMessage(serviceType, serviceName, outputText, botName, replyToChannel, responseURL); // Note we need to change SD for a configurable one.
			}

		}

		private void SetOutputText(string outputLit)
		{
			lit_output.Text = outputLit;
		}

		private void SendPrivateMessage(string serviceType, string nameOfHook, string toName)
		{
			//Commands.SendMessage(serviceType, nameOfHook, "This is a bit of text", "SlackMud", toName);
		}
	}
}