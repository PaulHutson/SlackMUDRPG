using SlackMUDRPG.CommandsClasses;
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
            SMCharacter smc = SlackMud.GetCharacter(tb_CharID.Text);
            OutputUpdate(SlackMUDRPG.CommandsClasses.SlackMud.GetLocationDetails(smc.RoomID, smc.UserID));
        }

        protected void btn_MoveRoom_Click(object sender, EventArgs e)
        {
            // Get char from memory for the move
            SMCharacter smc = SlackMud.GetCharacter(tb_CharID.Text);

            // Move the char to a new location
            OutputUpdate(smc.Move(tb_RoomShortcutText.Text));
        }

        protected void btn_Say_Click(object sender, EventArgs e)
        {
            SMCharacter smc = SlackMud.GetCharacter(tb_CharID.Text);
            smc.Say(tb_ChatText.Text);
        }
        
        protected void btn_Shout_Click(object sender, EventArgs e)
        {
            SMCharacter smc = SlackMud.GetCharacter(tb_CharID.Text);
            smc.Shout(tb_ChatText.Text);
        }

		protected void btn_Clear_Click(object sender, EventArgs e)
		{
			tb_TextAreaOutput.Text = "";
		}

        private void OutputUpdate(string s)
        {
            tb_TextAreaOutput.Text += s + "\n\n";
        }
        
    }
}