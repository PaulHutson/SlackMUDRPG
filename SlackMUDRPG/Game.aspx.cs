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

		protected void btn_CreateCharacter_Click(object sender, EventArgs e)
		{
			if ((tb_CreateUserName.Text != "") && (tb_CreatePassword.Text != ""))
			{
				Response.Cookies["ProvinceUserID"].Value = new SlackMUDRPG.CommandClasses.SlackMud().CreateCharacter(Guid.NewGuid().ToString(), "New", "Arrival", "m", "18", "BaseCharacter", null, tb_CreateUserName.Text, tb_CreatePassword.Text, false);
			}
		}
	}
}