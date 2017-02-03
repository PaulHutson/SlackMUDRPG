using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Handlers
{
	/// <summary>
	/// Summary description for WebSocketHandler
	/// </summary>
	public class WebSocketHandler : IHttpHandler
	{

		public void ProcessRequest(HttpContext context)
		{
			if (context.IsWebSocketRequest)
			{
				context.AcceptWebSocketRequest(new TestWebSocketHandler());
			}
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