using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public static class Commands
    {
        static String[] helloList = { "Hello", "Greetings", "Bonjour", "A very good day to you all", "Hey", "Howdy", "Well Hello!", "Yo", "Hi", "Beunas dias", "Good day", "Hi-ya", "How goes it?", "Howdy-do", "Shalom", "Whazzzzuppp?", "G'Day" };
        static Random rnd = new Random();

        public static string HelloSlack(string extraItemsIn = "")
        {
            int r = rnd.Next(helloList.Count());

            
            // Create actual name links
            // String format: <a class="internal_member_link" data-member-name="kerryb" target="/team/kerryb" href="/team/kerryb">@kerryb</a>
            
            string extraItemsList = "";

            if ((extraItemsIn != null) && (extraItemsIn != "")) { 
                String[] splitString = extraItemsIn.Split(' ');
                foreach (string item in splitString)
                {
                    if ((item.Substring(0, 1) == "@") || (item.Substring(0, 1) == "#"))
                    {
                        extraItemsList += "<" + item + "> ";
                    }
                    else
                    {
                        extraItemsList += item + " ";
                    }
                }
            }

            return helloList[r] + " " + extraItemsList;
        }

        public static void SendToChannelMessage(string serviceType, string nameOfHook, string messageContent, string botName, string channelOrPersonTo)
        {
            using (WebClient client = new WebClient())
            {
                string urlWithAccessToken = GetAccessToken(serviceType, nameOfHook);

                SlackClient sclient = new SlackClient(urlWithAccessToken);

                sclient.PostMessage(username: botName,
                           text: messageContent,
                           channel: channelOrPersonTo);
            }
        }

        public static string GetAccessToken(string serviceType, string nameOfHook)
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