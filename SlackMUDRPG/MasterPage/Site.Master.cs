using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Web.WebSockets;
using SlackMUDRPG.Handlers;

namespace SlackMUDRPG.MasterPage
{
	public partial class Site : System.Web.UI.MasterPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			// force https
			string host = Request.Url.Host;
			if (host != "localhost")
			{
				if (!Request.IsSecureConnection)
				{
					Response.Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));
				}
			}

			// only show logout button if logged in
			logoutBtn.Visible = false;

			if (Request.Cookies["ProvinceUserID"] != null)
			{
				logoutBtn.Visible = true;
			}
		}

		protected void logoutBtnClick(object sender, EventArgs e)
		{
			// Clear ProvinceUserID cookie
			if (Request.Cookies["ProvinceUserID"] != null)
			{
				HttpCookie cookie = Request.Cookies["ProvinceUserID"];

                HttpCookie myCookie = new HttpCookie("ProvinceUserID");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);

                this.closeWsConnections(cookie.Value);
			}

			this.hideLogoutBtn();
		}

		/// <summary>
		/// Sets the visibility of the logout buttom to true
		/// </summary>
		public void showLogoutBtn()
		{
			logoutBtn.Visible = true;
		}

		/// <summary>
		/// Sets the visibility of the logout button to false
		/// </summary>
		public void hideLogoutBtn()
		{
			logoutBtn.Visible = false;
		}

		/// <summary>
		/// Closes any WebSocket connections for a give userID
		/// </summary>
		/// <param name="userID">The userID to close connections for</param>
		private void closeWsConnections(string userID)
		{
			WebSocketCollection wsClients = (WebSocketCollection)HttpContext.Current.Application["WSClients"];

			List<WebSocketHandler> filtered = wsClients.Where(r => ((GameAccessWebSocketHandler)r).userID == userID).ToList();

			foreach (WebSocketHandler h in filtered)
			{
				h.Close();
			}
		}
	}
}