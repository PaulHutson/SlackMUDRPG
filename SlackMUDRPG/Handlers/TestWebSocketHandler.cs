using SlackMUDRPG.Handlers;

namespace WebSockets

{

	public class TestWebSocketHandler : WebSocketHandler

	{

		private static WebSockets.WebSocketCollection clients = new WebSocketCollection();

		private string name;

		public override void OnOpen()

		{

			this.name = this.WebSocketContext.QueryString["name"];

			clients.Add(this);

			clients.Broadcast(name + " has connected.");

		}

		public override void OnMessage(string message)

		{

			clients.Broadcast(string.Format("{0} said: {1}", name, message));

		}

		public override void OnClose()

		{

			clients.Remove(this);

			clients.Broadcast(string.Format("{0} has gone away.", name));

		}

	}

}