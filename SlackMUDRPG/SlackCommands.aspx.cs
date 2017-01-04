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
	public partial class SlackCommands : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			int newAmount = int.Parse(Application["TestOutput"].ToString()) + 1;
			Application["TestOutput"] = newAmount;

			string outputText = "Command not found... try '/sdcommand help' to return a list of possible commands";
			bool error = true;
			bool respondDirectToUser = true;
			string botName = "T43 3RR0R |307";
			string channelName = "#";
			string channelID = "x";
			string replyToChannel = "@error";
			string replyToError = outputText;
			string userName = "Slackbot";
			string additionalText = Request.Form["text"] ?? Request.QueryString["text"];
			if (additionalText == null)
			{
				additionalText = Request.QueryString["text"];
			}

			if ((Request.QueryString["op"] == "randomgreeting") || (additionalText == "randomgreeting"))
			{
				if (additionalText == "randomgreeting")
				{
					additionalText = "";
				}
				outputText = Commands.HelloSlack(additionalText);
				botName = Request.Form["user_name"] ?? "TestBot";
				channelName = Request.Form["channel_name"] ?? "General";
				channelID = Request.Form["channel_id"] ?? "C3L9HN59V";
				replyToChannel = SetChannelReply(channelName, channelID);
				if (replyToChannel == "@error")
				{
					SetOutputText("Can not say hello to a private channel");
				}
				error = false;
				respondDirectToUser = false;
			}
			else if ((additionalText == "PMTest"))
			{
				SendPrivateMessage("U3KLW47QC"); // this is Pauls userid (note, not name!)
				SetOutputText("Output On");
			}
			else if ((Request.QueryString["op"] == "help") || (additionalText == "help"))
			{
				SetOutputText(HelpString());
				error = false;
			}

			if ((!error) && (replyToChannel != "@error"))
			{
				SendToChannelMessage(outputText, botName, replyToChannel);
			}
		}

		private void SetOutputText(string outputLit)
		{
			lit_output.Text = outputLit;
		}

		private string HelpString()
		{
			return "SD Commands Help:\n" +
					"  /randomgreeting will return a random greeting message\n\n" +
					"Additionally, all commands can be run with /sdcommand [commandname] i.e. /sdcommand randomgreeting will return a random greeting.";
		}

		private string SetChannelReply(string channelName, string channelID)
		{
			string outputInformation = "@error";

			if (channelID.Substring(0, 1) == "C")
			{
				outputInformation = "#" + channelName;
			}

			return outputInformation;
		}

		private void SendPrivateMessage(string toName)
		{
			SendToChannelMessage("This is a bit of text", "Test Bot!", toName);
		}

		private void SendToChannelMessage(string messageContent, string botName, string channelTo)
		{
			using (WebClient client = new WebClient())
			{
				string urlWithAccessToken = Commands.GetAccessToken("", "SlackMud");

				SlackClient sclient = new SlackClient(urlWithAccessToken);

				sclient.PostMessage(username: botName,
						   text: messageContent,
						   channel: channelTo);
			}
		}
	}
}