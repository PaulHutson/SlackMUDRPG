using SlackMUDRPG.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public static class Commands
	{
		#region "Example Code"

		//static String[] helloList = { "Hello", "Greetings", "Bonjour", "A very good day to you all", "Hey", "Howdy", "Well Hello!", "Yo", "Hi", "Beunas dias", "Good day", "Hi-ya", "How goes it?", "Howdy-do", "Shalom", "Whazzzzuppp?", "G'Day" };
		//static Random rnd = new Random();
		//public static string HelloSlack(string extraItemsIn = "")
		//{
		//	int r = rnd.Next(helloList.Count());


		//	// Create actual name links
		//	// String format: <a class="internal_member_link" data-member-name="kerryb" target="/team/kerryb" href="/team/kerryb">@kerryb</a>

		//	string extraItemsList = "";

		//	if ((extraItemsIn != null) && (extraItemsIn != ""))
		//	{
		//		String[] splitString = extraItemsIn.Split(' ');
		//		foreach (string item in splitString)
		//		{
		//			if ((item.Substring(0, 1) == "@") || (item.Substring(0, 1) == "#"))
		//			{
		//				extraItemsList += "<" + item + "> ";
		//			}
		//			else
		//			{
		//				extraItemsList += item + " ";
		//			}
		//		}
		//	}

		//	return helloList[r] + " " + extraItemsList;
		//}

		#endregion

		public static void SendMessage(string serviceType, string nameOfHook, string messageContent, string botName, string channelOrPersonTo, string responseURL = null)
		{
			if (serviceType.ToLower() == "slack")
			{
				using (WebClient client = new WebClient())
				{
					SlackClient sclient;

					if ((responseURL != null) && (responseURL != ""))
					{
						sclient = new SlackClient(responseURL);
					}
					else
					{
						string urlWithAccessToken = GetAccessToken(serviceType, nameOfHook);
						sclient = new SlackClient(urlWithAccessToken);
					}

					sclient.PostMessage(username: botName,
							   text: messageContent,
							   channel: channelOrPersonTo);
				}
			}
			else if (serviceType.ToLower() == "ws")
			{
				SlackMUDRPG.Global.wsClients.SingleOrDefault(r => ((GameAccessWebSocketHandler)r).userID == channelOrPersonTo).Send(messageContent);
			}
			
		}

		public static string GetAccessToken(string serviceType, string nameOfHook)
		{
			string accessToken = "";
			if ((serviceType.ToLower() == "slack") || (serviceType == ""))
			{
				// Note the services tokens below needs to be updated based on the registered slack service...
		        accessToken = "https://hooks.slack.com/services/" + Utility.SlackWebHooks.GetWebHookToken(nameOfHook);

            } // implement more for other things like Discord (anything with a webhook really, or for a custom page).
			return accessToken;
		}
	}
}