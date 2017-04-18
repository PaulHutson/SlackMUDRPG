using SlackMUDRPG.CommandClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SlackMUDRPG
{
	public partial class Game : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{

		}

        protected void loginBtnClick(object sender, EventArgs e)
        {
            // Check that both the boxes have something in them...
            if ((tb_username.Text != "") && (tb_password.Text != ""))
            {
                string userID;
                SlackMud sm = new SlackMud();
                if (sm.WebLogin(tb_username.Text, tb_password.Text, out userID))
                {
                    // Login
                    Response.Cookies["ProvinceUserID"].Value = userID;
                }
                else
                {
                    // Failed to login - need to post an error.
                    // TODO: Make sure we go to the right tab... and display an error
                }
            }
        }
        
        protected void createBtnClick(object sender, EventArgs e)
		{
			if ((newUsername.Text != "") && (newPassword.Text != ""))
			{
                if (new SMAccountHelper().CheckUserName(newUsername.Text)) {
                    Response.Cookies["ProvinceUserID"].Value = new SlackMUDRPG.CommandClasses.SlackMud().CreateCharacter(
                        Guid.NewGuid().ToString(),
                        "New",
                        "Arrival",
                        "m",
                        "18",
                        "BaseCharacter",
                        null,
                        newUsername.Text,
                        newPassword.Text,
                        false
                    );
                }
                else
                {
                    // Have to display an error
                    // TODO: Make sure we go to the right tab... and display an error
                }
            }
		}
	}
}