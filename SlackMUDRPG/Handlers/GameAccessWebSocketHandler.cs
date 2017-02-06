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
			string loginResponse = new SlackMUDRPG.CommandClasses.SlackMud().Login(this.userID, false, null, "WS");
			if (loginResponse == null)
			{
				this.Send("Character not found?");
			}
			else if (loginResponse != "") 
			{
				this.Send(loginResponse);
				new SMCommandUtility(this.userID).InitateCommand("look");
			};
		}

		public override void OnMessage(string commandText)
		{
			// Process the character command sent in
			// SlackMUDRPG.Global.wsClients.Broadcast(string.Format("{0} said: {1}", name, commandText));
			this.userID = this.WebSocketContext.QueryString["userID"];
			new SMCommandUtility(this.userID).InitateCommand(commandText);
		}

		public override void OnClose()
		{
			this.userID = this.WebSocketContext.QueryString["userID"];
			SlackMUDRPG.Global.wsClients.Remove(this);
			
		}
	}
}