using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SlackMUDRPG
{
	public partial class TestPost : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			using (WebClient client = new WebClient())
			{
				string urlWithAccessToken = "https://hooks.slack.com/services/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";

				SlackMUDRPG.CommandsClasses.SlackClient sclient = new SlackMUDRPG.CommandsClasses.SlackClient(urlWithAccessToken);

				sclient.PostMessage(username: "Mr. Torgue",
						   text: "THIS IS A TEST MESSAGE! SQUEEDLYBAMBLYFEEDLYMEEDLYMOWWWWWWWW!",
						   channel: "#general");

			}
		}
	}
}