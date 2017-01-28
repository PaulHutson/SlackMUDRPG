using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SlackMUDRPG.CommandClasses;

namespace SlackMUDRPG
{
	public partial class TestButton : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected void btn_Test_Click(object sender, EventArgs e)
		{
			new SlackMud().Login(tb_CharID.Text);
		}

		protected void btn_CreateCharacter_Click(object sender, EventArgs e)
		{
			new SlackMud().CreateCharacter(tb_CharID.Text, "Testy", "McTestface", "f", "8");
		}

		protected void btn_TestLoc_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			new SlackMud().GetLocationDetails(smc.RoomID, smc.UserID);
		}

		protected void btn_MoveRoom_Click(object sender, EventArgs e)
		{
			// Get char from memory for the move
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);

			// Move the char to a new location
			smc.Move(tb_RoomShortcutText.Text);
		}

		protected void btn_Say_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.Say(tb_ChatText.Text);
		}

		protected void btn_Shout_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.Shout(tb_ChatText.Text);
		}

		protected void btn_Clear_Click(object sender, EventArgs e)
		{
			tb_TextAreaOutput.Text = "";
		}

		protected void btn_Get_Guid(object sender, EventArgs e)
		{
			OutputUpdate(Guid.NewGuid().ToString());
		}

		protected void btn_PickUpStick_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.PickUpItem("Wooden Stick");
		}

		protected void btn_DropStick_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.DropItem("Wooden Stick");
		}

		protected void btn_ListInventory_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			string invInput = InvInput.Text == "" ? null : InvInput.Text;
			smc.ListInventory(invInput);
		}

		private void OutputUpdate(string s)
		{
			tb_TextAreaOutput.Text += s + "\n\n";
		}

		protected void btn_Look_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.GetRoomDetails();
		}

		protected void btn_ChopTree_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.UseSkill("Chop", "Tree");
		}

		protected void btn_ChopLog_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.UseSkill("Chop", "Log");
		}

		protected void btn_ChopTreeTrunk_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.UseSkill("Chop", "Trunk");
		}

		protected void btn_AttackRob_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.Attack("Robert Curran");
		}

		protected void btn_AttackPell_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.Attack("Pell");
		}

		protected void btn_Stop_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.StopActivity();
		}

		protected void btn_Inspect_Rob_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.InspectObject("Rob Curran2");
		}

		protected void Button1_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.InspectObject("Wooden Pell");
		}

		protected void btn_Mining_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.UseSkill("Mine", "Gold");
		}

		protected void btn_CraftSword_Click(object sender, EventArgs e)
		{
			SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
			smc.UseSkill("Carpentry", null, false, "Wooden Sword");
		}

        protected void btn_AttackPaul_Click(object sender, EventArgs e)
        {
            SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
            smc.Attack("Paul Hutson");
        }

        protected void btn_OOC_Click(object sender, EventArgs e)
        {
            SMCharacter smc = new SlackMud().GetCharacter(tb_CharID.Text);
            smc.SendOOC(tb_ChatText.Text);
        }
    }
}
