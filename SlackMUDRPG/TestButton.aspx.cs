using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SlackMUDRPG
{
    public partial class TestButton : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_Test_Click(object sender, EventArgs e)
        {
            lit_OutputExample.Text = SlackMUDRPG.CommandsClasses.SlackMud.Login("123");
        }

        protected void btn_CreateCharacter_Click(object sender, EventArgs e)
        {
            SlackMUDRPG.CommandsClasses.SlackMud.CreateCharacter("123", "Paul", "Hutson", 34, 'm');
        }
    }
}