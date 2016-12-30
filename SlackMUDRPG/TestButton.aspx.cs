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
            OutputUpdate(SlackMUDRPG.CommandsClasses.SlackMud.Login(tb_CharID.Text));
        }

        protected void btn_CreateCharacter_Click(object sender, EventArgs e)
        {
            OutputUpdate(SlackMUDRPG.CommandsClasses.SlackMud.CreateCharacter(tb_CharID.Text, "Paul", "Hutson", 34, 'm'));
        }

        protected void btn_TestLoc_Click(object sender, EventArgs e)
        {
            OutputUpdate(SlackMUDRPG.CommandsClasses.SlackMud.GetLocationDetails("1"));
        }

        private void OutputUpdate(string s)
        {
            lit_OutputExample.Text = s;
        }
        
    }
}