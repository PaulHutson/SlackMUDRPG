using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SlackMUDRPG
{
	public partial class Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			webPlayLink.HRef = "Game.aspx";

			//addSlackLink.HRef = "https://slack.com/oauth/authorize?scope=bot&client_id=121710143684.174002597126&redirect_uri=https%3a%2f%2fslack.botframework.com%2fHome%2fauth&state=SlackMud";

			//addSkypeLink.HRef = "https://join.skype.com/bot/8f6e3171-4a43-4121-a479-8c0dce48f9e6";
		}
	}
}