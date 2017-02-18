using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
		}
	}
}