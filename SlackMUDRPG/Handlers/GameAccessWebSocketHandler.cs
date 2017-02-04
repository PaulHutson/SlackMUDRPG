using Microsoft.Web.WebSockets;
using SlackMUDRPG.CommandClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Handlers
{
	public class GameAccessWebSocketHandler : WebSocketHandler
	{
		public string userID;

		public override void OnOpen()
		{
			this.userID = this.WebSocketContext.QueryString["userID"];
			SlackMUDRPG.Global.wsClients.Add(this);
			if (!new SlackMUDRPG.CommandClasses.SlackMud().Login(this.userID, false, null, "WS"))
			{
				this.Send("Character not found?");
			};
		}

		public override void OnMessage(string commandText)
		{
			// Process the character command sent in
			// SlackMUDRPG.Global.wsClients.Broadcast(string.Format("{0} said: {1}", name, commandText));

			string userID = commandText.Substring(0, commandText.IndexOf("|"));
			string actualCommandtext = commandText.Substring(commandText.IndexOf("|")+1);

			new SPCommandUtility(userID).InitateCommand(actualCommandtext);
		}

		public override void OnClose()
		{
			SlackMUDRPG.Global.wsClients.Remove(this);
		}
	}
}