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
            string botName = "T43 3RR0R |307";              // Error bot name
            string channelName = "#";                       // Channel the request came from
            string channelID = "x";                         // Default channel id to reply to
            string replyToChannel = "@error";               // Default ReplyToChannel
            string replyToError = outputText;               // Default reply text

            // Get Slack / Other webhook locations
            string serviceType = Request.Form["st"];   // This will only be Slack for now, but could use something else like Discord
            if (serviceType == null)
            {
                serviceType = Request.QueryString["st"];
            }

            // Get Slack / Other webhook locations
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
                SendToChannelMessage(serviceType, serviceName, outputText, botName, replyToChannel); // Note we need to change SD for a configurable one.
            }

        }

        private void SetOutputText(string outputLit)
        {
            lit_output.Text = outputLit;
        }

        private void SendPrivateMessage(string serviceType, string nameOfHook, string toName)
        {
            SendToChannelMessage(serviceType, nameOfHook, "This is a bit of text", "Test Bot!", toName);
        }

        private void SendToChannelMessage(string serviceType, string nameOfHook, string messageContent, string botName, string channelTo)
        {
            using (WebClient client = new WebClient())
            {
                string urlWithAccessToken = GetAccessToken(serviceType, nameOfHook);

                SlackClient sclient = new SlackClient(urlWithAccessToken);

                sclient.PostMessage(username: botName,
                           text: messageContent,
                           channel: channelTo);
            }
        }

        private string GetAccessToken(string serviceType, string nameOfHook)
        {
            string accessToken = "";
            if ((serviceType == "Slack") || (serviceType == ""))
            {
                // Note the services tokens below needs to be updated based on the registered slack service...
                accessToken = "https://hooks.slack.com/services/" + Utility.SlackWebHooks.GetWebHookToken(nameOfHook);
            } // implement more for other things like Discord (anything with a webhook really, or for a custom page).
            return accessToken;
        }
    }
}