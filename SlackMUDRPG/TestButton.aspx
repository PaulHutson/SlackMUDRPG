﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestButton.aspx.cs" Inherits="SlackMUDRPG.TestButton" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type='text/javascript' src='https://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js'></script>
    <script type="text/javascript">
        $(document).ready(function () {
            scrollBoxToBottom();
        });

        function scrollBoxToBottom() {
            $('#tb_TextAreaOutput').scrollTop($('#tb_TextAreaOutput')[0].scrollHeight);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox runat="server" ID="tb_TextAreaOutput" ClientIDMode="Static" Height="200px" ReadOnly="True" Rows="5" TextMode="MultiLine" Width="600px"></asp:TextBox>
		</div>
		<div>
			<asp:Button runat="server" ID="btn_clear" Text="Clear" OnClick="btn_Clear_Click" OnClientClick="scrollBoxToTop();" />
			<asp:Button runat="server" ID="btn_guid" Text="Get GUID" OnClick="btn_Get_Guid" OnClientClick="scrollBoxToTop();" />
			<br /><br />
		</div>
        <div>
            CharID : <asp:TextBox runat="server" ID="tb_CharID">123</asp:TextBox><br />
            ShortcutRoomMove : <asp:TextBox runat="server" ID="tb_RoomShortcutText"></asp:TextBox><br />
            Talk :	<asp:TextBox runat="server" ID="tb_ChatText"></asp:TextBox> <asp:Button runat="server" ID="btn_Say" Text="Say" OnClick="btn_Say_Click" OnClientClick="scrollBoxToBottom();" />  <asp:Button runat="server" ID="btn_Shout" Text="Shout" OnClick="btn_Shout_Click" OnClientClick="scrollBoxToBottom();" />
					          <asp:Button runat="server" ID="btn_Resp" Text="Respond" OnClientClick="scrollBoxToBottom();" OnClick="btn_Resp_Click" />  <%--<asp:Button runat="server" ID="btn_Whisper" Text="Whisper" OnClick="btn_Whisper_Click" OnClientClick="scrollBoxToBottom();" />--%><br /><br />
        </div>
        <div>
            <asp:Button runat="server" ID="btn_Test" Text="Test Login" OnClick="btn_Test_Click" OnClientClick="scrollBoxToBottom();" />
            <asp:Button runat="server" ID="btn_GetLocation" Text="Get Location Details" OnClick="btn_TestLoc_Click" OnClientClick="scrollBoxToBottom();" />
            <asp:Button runat="server" ID="btn_CreateCharacter" Text="Test Create Character" OnClick="btn_CreateCharacter_Click" OnClientClick="scrollBoxToBottom();" /><br />
            <asp:Button runat="server" ID="btn_MoveToRoom" Text="Move To Room Shortcut" OnClick="btn_MoveRoom_Click" OnClientClick="scrollBoxToBottom();" />
			<asp:Button runat="server" ID="btn_Look" Text="Look (at current location)" OnClientClick="scrollBoxToBottom();" OnClick="btn_Look_Click" /><br />
            <asp:Button runat="server" ID="btn_Inspect_Rob" Text="Inspect Rob Curren2" OnClientClick="scrollBoxToBottom();" OnClick="btn_Inspect_Rob_Click" />
			<asp:Button runat="server" ID="btn_Inspect_WearyTraveller" Text="Inspect Weary Traveller" OnClientClick="scrollBoxToBottom();" OnClick="btn_Inspect_WearyTraveller_Click" />
            <asp:Button runat="server" ID="Button1" Text="Inspect Wooden Pell" OnClientClick="scrollBoxToBottom();" OnClick="Button1_Click" />
        </div>

		<div>
			Inventory:
				Inventory Input : <asp:TextBox runat="server" ID="InvInput">123</asp:TextBox><br />
				<asp:Button runat="server" ID="btn_PickUpStick" Text="Pick Up Wooden Stick" OnClick="btn_PickUpStick_Click" OnClientClick="scrollBoxToBottom();" />
				<asp:Button runat="server" ID="btn_DropUpStick" Text="Drop Wooden Stick" OnClick="btn_DropStick_Click" OnClientClick="scrollBoxToBottom();" />
				<asp:Button runat="server" ID="btn_ListInventory" Text="List Inventory" OnClick="btn_ListInventory_Click" OnClientClick="scrollBoxToBottom();" />
		</div>
		
		<div>
			Skill:
				<asp:Button runat="server" ID="btn_ChopTree" Text="Chop Tree" OnClientClick="scrollBoxToBottom();" OnClick="btn_ChopTree_Click" />
				<asp:Button runat="server" ID="btn_ChopLog" Text="Chop Log" OnClientClick="scrollBoxToBottom();" OnClick="btn_ChopLog_Click"  />
				<asp:Button runat="server" ID="btn_ChopTreeTrunk" Text="Chop Tree Trunk" OnClientClick="scrollBoxToBottom();" OnClick="btn_ChopTreeTrunk_Click"  />
				<asp:Button runat="server" ID="btn_Stop" Text="STOP Activity" OnClientClick="scrollBoxToBottom();" OnClick="btn_Stop_Click"   />
                <asp:Button runat="server" ID="btn_Mining" Text="Mine Gold" OnClientClick="scrollBoxToBottom();" OnClick="btn_Mining_Click" />
		</div>
		
		<div>
			Crafting:
				<asp:Button runat="server" ID="btn_CraftSword" Text="Craft Sword" OnClientClick="scrollBoxToBottom();" OnClick="btn_CraftSword_Click"/>
		</div>
		
		<div>
			Combat:
				<asp:Button runat="server" ID="btn_AttackRob" Text="Attack Robert Curran" OnClientClick="scrollBoxToBottom();" OnClick="btn_AttackRob_Click" />
                <asp:Button runat="server" ID="btn_AttackPaul" Text="Attack Paul Hutson" OnClientClick="scrollBoxToBottom();" OnClick="btn_AttackPaul_Click"/>
				<asp:Button runat="server" ID="btn_AttackPell" Text="Attack Pell" OnClientClick="scrollBoxToBottom();" OnClick="btn_AttackPell_Click" />
		</div>
		<asp:TextBox runat="server" ID="st" Text="slack" Visible="false"></asp:TextBox>
    </form>
</body>
</html>
