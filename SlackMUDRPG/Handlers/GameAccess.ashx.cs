using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Web.WebSockets;

namespace SlackMUDRPG.Handlers
{
	public class GameAccess : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			if (context.IsWebSocketRequest)
				context.AcceptWebSocketRequest(new GameAccessWebSocketHandler());
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}