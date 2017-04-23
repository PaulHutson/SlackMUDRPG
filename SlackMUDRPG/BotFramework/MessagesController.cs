using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using System.Web;
using System.Collections.Generic;

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
		public HttpResponseMessage Post([FromBody]Microsoft.Bot.Connector.Activity activity)
		{
            this.userID = activity.From.Id;

            if (this.userID.Contains(':'))
            {
                this.userID = this.userID.Replace(':', '-');
            }

            List<BotClient> botClients = (List<BotClient>)HttpContext.Current.Application["BotClients"];
            BotClient connectingClient = botClients.FirstOrDefault(bc => ((bc.BotURL == activity.ServiceUrl) && (bc.UserID == this.userID)));
            
            if (activity.Type == ActivityTypes.Message)
			{
				if (connectingClient == null)
				{
                    NewUser(connectingClient, botClients, activity);
                }
				else
				{
					new CommandClasses.SMCommandUtility(this.userID).InitateCommand(activity.Text);
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
            else if (activity.Type == ActivityTypes.ConversationUpdate) {
                if (connectingClient == null)
                {
                    NewUser(connectingClient, botClients, activity);
                }
            }
			else
			{
				HandleSystemMessage(activity);
			}
			var response = Request.CreateResponse(HttpStatusCode.OK);
			return response;
		}

        private void NewUser(BotClient connectingClient, List<BotClient> botClients, [FromBody]Microsoft.Bot.Connector.Activity activity)
        {
            connectingClient = new BotClient();
            connectingClient.UserID = this.userID;
            connectingClient.UserName = activity.From.Name;
            connectingClient.BotURL = activity.ServiceUrl;
            connectingClient.ConversationAccount = activity.Conversation;

            botClients = (List<BotClient>)HttpContext.Current.Application["BotClients"];
            botClients.RemoveAll(c => c.UserID == connectingClient.UserID);
            botClients.Add(connectingClient);
            HttpContext.Current.Application["BotClients"] = botClients;

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
                    string gameUserID = new SlackMUDRPG.CommandClasses.SlackMud().CreateCharacter(this.userID, "New", "Arrival", "m", "20");

                    // Now log them in - note this does a "look" as part of the command.
                    new SlackMUDRPG.CommandClasses.SlackMud().Login(gameUserID, false, null, "BC");
                }
                else
                {
                    BotClientUtility.SendMessage(connectingClient, loginResponse);
                    new CommandClasses.SMCommandUtility(this.userID).InitateCommand("look");
                }
            };
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
			newMessage.From = new ChannelAccount();
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