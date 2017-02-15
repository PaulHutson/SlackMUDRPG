using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.EnterpriseServices;
using SlackMUDRPG.CommandClasses;

namespace SlackMUDRPG.BotFramework
{
	[BotAuthentication]
	public class MessagesController : ApiController
	{
		public string userID;

		/// <summary>
		/// POST: api/Messages
		/// Receive a message from a user and reply to it
		/// </summary>
		public async Task<HttpResponseMessage> Post([FromBody]Microsoft.Bot.Connector.Activity activity)
		{
			if (activity.Type == ActivityTypes.Message)
			{
				BotClient connectingClient = Global.botClients.FirstOrDefault(bc => ((bc.BotURL == activity.ServiceUrl) && (bc.UserID == activity.From.Id)));
				this.userID = activity.From.Id;

				if (this.userID.Contains(':'))
				{
					this.userID = this.userID.Replace(':', '-');
				}

				if (connectingClient == null)
				{
					connectingClient = new BotClient();
					connectingClient.UserID = this.userID;
					connectingClient.UserName = activity.From.Name;
					connectingClient.BotURL = activity.ServiceUrl;
					connectingClient.ConversationAccount = activity.Conversation;
					SlackMUDRPG.Global.botClients.Add(connectingClient);

					string loginResponse = new SlackMUDRPG.CommandClasses.SlackMud().Login(this.userID, false, null, "BC");
					if (loginResponse == null)
					{
						BotClientUtility.SendMessage(connectingClient, "Character not found?");
					}
					else if (loginResponse != "")
					{
						if (loginResponse.Substring(0, 27) == "You must create a character")
						{
							// Create the new character
							new SlackMUDRPG.CommandClasses.SlackMud().CreateCharacter(this.userID, "New", "Arrival", "m", "20");

							// Now log them in
							new SlackMUDRPG.CommandClasses.SlackMud().Login(this.userID, false, null, "BC");

							// Issue the look command
							new SMCommandUtility(this.userID).InitateCommand("look");
						}
						else
						{
							BotClientUtility.SendMessage(connectingClient, loginResponse);
							new SMCommandUtility(this.userID).InitateCommand("look");
						}
					};
				}
				else
				{
					new SMCommandUtility(this.userID).InitateCommand(activity.Text);
				}
				
				//this.userID = activity.From.Id;

				//ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

				//string length = "1";

				//// return our reply to the user
				//Microsoft.Bot.Connector.Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
				//await connector.Conversations.ReplyToActivityAsync(reply);


				//ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
				//// calculate something for us to return
				//int length = (activity.Text ?? string.Empty).Length;

				//// return our reply to the user
				//Microsoft.Bot.Connector.Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
				//await connector.Conversations.ReplyToActivityAsync(reply);
			}
			else
			{
				HandleSystemMessage(activity);
			}
			var response = Request.CreateResponse(HttpStatusCode.OK);
			return response;
		}
		
		private Microsoft.Bot.Connector.Activity HandleSystemMessage(Microsoft.Bot.Connector.Activity message)
		{
			if (message.Type == ActivityTypes.DeleteUserData)
			{
				// Implement user deletion here
				// If we handle user deletion, return a real message
			}
			else if (message.Type == ActivityTypes.ConversationUpdate)
			{
				// Handle conversation state changes, like members being added and removed
				// Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
				// Not available in all channels
			}
			else if (message.Type == ActivityTypes.ContactRelationUpdate)
			{
				// Handle add/remove from contact lists
				// Activity.From + Activity.Action represent what happened
			}
			else if (message.Type == ActivityTypes.Typing)
			{
				// Handle knowing tha the user is typing
			}
			else if (message.Type == ActivityTypes.Ping)
			{
			}

			return null;
		}
	}

	public static class BotClientUtility
	{
		public static async void SendMessage(BotClient bc, string message)
		{
			ConnectorClient connector = new ConnectorClient(new Uri(bc.BotURL));
			IMessageActivity newMessage = Microsoft.Bot.Connector.Activity.CreateMessageActivity();
			newMessage.Type = ActivityTypes.Message;
			newMessage.From = new ChannelAccount("<BotId>", "<BotName>");
			newMessage.Conversation = bc.ConversationAccount;
			newMessage.Recipient = new ChannelAccount(bc.UserID);
			newMessage.Text = message;
			await connector.Conversations.SendToConversationAsync((Microsoft.Bot.Connector.Activity)newMessage);
		}
	}

	public class BotClient
	{
		public string UserID { get; set; }
		public string UserName { get; set; }
		public ConversationAccount ConversationAccount { get; set; }
		public string BotURL { get; set; }
	}
}