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
                    // TODO: make this work.
                    lit_VariableJavascript.Text = "<script type=\"text/javascript\">$(function () {showLogin();});</script>";

                    // Show the correct error.
                    pnl_LoginError.Visible = true;
                    pnl_CreateError.Visible = false;
                }
            }
        }
        
        protected void createBtnClick(object sender, EventArgs e)
		{
            bool error = false;
            string errorText = "";

			if ((newUsername.Text != "") && (email.Text != "") && (newPassword.Text != "") && (repeatPassword.Text != "")) // need some other checks here for errors i.e. the passwords not being the same!
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
                    // Set the error text
                    errorText = "Username is not valid.";
                    error = true;
                }
            }
            else
            {
                // Set the error text
                errorText = "Please enter details in all fields.";
                error = true;
            }

            if (error)
            {
                // Change the error text
                lit_CreateError.Text = errorText;

                // Have to display an error
                pnl_LoginError.Visible = false;
                pnl_CreateError.Visible = true;
            }
		}
	}
}