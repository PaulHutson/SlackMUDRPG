using Microsoft.Bot.Connector;
using Microsoft.Web.WebSockets;
using SlackMUDRPG.BotFramework;
using SlackMUDRPG.CommandClasses;
using SlackMUDRPG.Handlers;
using SlackMUDRPG.Utility.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public static class Commands
	{
		public static void SendMessage(string serviceType, string nameOfHook, string messageContent, string botName, string channelOrPersonTo, string responseURL = null)
		{
			if (serviceType != null)
			{
				if (serviceType.ToLower() == "slack")
				{
					messageContent = GetFormattedMessage("slack", messageContent);

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
					messageContent = GetFormattedMessage("html", messageContent);

					WebSocketCollection wsClients = (WebSocketCollection)HttpContext.Current.Application["WSClients"];

					List<WebSocketHandler> filtered = wsClients.Where(r => ((GameAccessWebSocketHandler)r).userID == channelOrPersonTo).ToList();

					foreach (WebSocketHandler h in filtered)
					{
						h.Send(messageContent);
					}
				}
				else if (serviceType.ToLower() == "bc")
				{
					messageContent = GetFormattedMessage("skype", messageContent);

					List<BotClient> botClients = (List<BotClient>)HttpContext.Current.Application["BotClients"];
					BotClient bc = botClients.FirstOrDefault(bot => bot.UserID == channelOrPersonTo);
					if (bc != null)
					{
						BotClientUtility.SendMessage(bc, messageContent);
					}
				}
			}
		}

		/// <summary>
		/// Formats a given text string for a given output channel, replacing/stripping tags as required.
		/// </summary>
		/// <param name="platform">The target platform where the text will be outputted.</param>
		/// <param name="message">The text output to format.</param>
		/// <returns>The formatted text.</returns>
		private static string GetFormattedMessage(string platform, string message)
		{
			return OutputFormatterFactory.Get(platform).ProcessOutput(message);
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