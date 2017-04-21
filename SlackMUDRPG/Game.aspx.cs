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
			if (!IsPostBack)
			{
				if (Request.Cookies["ProvinceEverLoggedIn"] != null && Request.Cookies["ProvinceEverLoggedIn"].Value == "Y")
				{
					this.showLoginForm();
				}
				else
				{
					this.showCreateForm();
				}
			}
		}

		protected void loginBtnClick(object sender, EventArgs e)
		{
			// Fail if either username of password are empty
			if (tb_username.Text == "" || tb_password.Text == "")
			{
				pnl_LoginError.Visible = true;
				lit_LoginError.Text = this.getAlertHtml("Please enter a Username and Password.");
				pnl_CreateError.Visible = false;
				this.showLoginForm();
			}
			else
			{
				string userID;
				SlackMud sm = new SlackMud();

				if (sm.WebLogin(tb_username.Text, tb_password.Text, out userID))
				{
					// Login
					Response.Cookies["ProvinceUserID"].Value = userID;

					// Set ever looked in cookie so we know the show the login rather than create screen
					Response.Cookies["ProvinceEverLoggedIn"].Value = "Y";

					this.clearLoginForm();
				}
				else
				{
					// Show the correct error.
					pnl_LoginError.Visible = true;
					lit_LoginError.Text = this.getAlertHtml("Invalid Username or Password.");
					pnl_CreateError.Visible = false;
					this.showLoginForm();
				}
			}
		}

		protected void createBtnClick(object sender, EventArgs e)
		{
			bool error = false;

			if ((newUsername.Text != "") && (email.Text != "") && (newPassword.Text != "") && (repeatPassword.Text != ""))
			{
				// TODO need some other checks here for errors i.e. the passwords not being the same!

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

					Response.Cookies["ProvinceEverLoggedIn"].Value = "Y";

					this.clearCreateForm();
				}
				else
				{
					// Set the error text
					lit_CreateError.Text = this.getAlertHtml("Username already taken, please try another.", "warning");
					error = true;
				}
			}
			else
			{
				// Set the error text
				lit_CreateError.Text = this.getAlertHtml("Please enter details in all fields.");
				error = true;
			}

			if (error)
			{
				// Have to display an error
				pnl_LoginError.Visible = false;
				pnl_CreateError.Visible = true;

				this.showCreateForm();
			}
		}

		private string getAlertHtml(string msg, string type = "danger")
		{
			return $"<p class=\"alert alert-{type}\">{msg}</p>";
		}

		private void clearLoginForm()
		{
			tb_username.Text = "";
			tb_password.Text = "";
		}

		private void clearCreateForm()
		{
			newUsername.Text = "";
			email.Text = "";
			newPassword.Text = "";
			repeatPassword.Text = "";
		}

		private void showLoginForm()
		{
			tab_Login.Attributes["class"] += " active";
			tab_Create.Attributes["class"] = tab_Create.Attributes["class"].Replace("active", "");
			form_Login.Style.Add("display", "block");
			form_Create.Style.Add("display", "none");
		}

		private void showCreateForm()
		{
			tab_Create.Attributes["class"] += " active";
			tab_Login.Attributes["class"] = tab_Login.Attributes["class"].Replace("active", "");
			form_Create.Style.Add("display", "block");
			form_Login.Style.Add("display", "none");
		}
	}
}