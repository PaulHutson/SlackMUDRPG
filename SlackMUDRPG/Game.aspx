<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Site.Master" AutoEventWireup="true" CodeBehind="Game.aspx.cs" Inherits="SlackMUDRPG.Game" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ph_Title" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ph_Header" runat="server">
	<script src="https://code.jquery.com/jquery-2.2.4.min.js" integrity="sha256-BbhdlvQf/xTY9gja0Dq3HiwQF8LaCRTXxZKRutelT44=" crossorigin="anonymous"></script>
	<script type="text/javascript">
		var ws;
		var userid;
		$(document).ready(function () {
			if (Cookies.get('ProvinceUserID') != null) {
				console.log("Cookie:" + Cookies.get('ProvinceUserID'));
				$("#PlayArea").show();
				$("#LoginCreateCharacterForm").hide();

				var url = 'ws://localhost:57980/Handlers/GameAccess.ashx?userID=' + Cookies.get('ProvinceUserID');
				ws = new WebSocket(url);
				ws.onopen = function () {
					$('#messages').prepend('Connected <br/>');
					$('#cmdSend').click(function () {
						SendMessage();
					});
				};
				ws.onmessage = function (e) {
					$('#OutputWindow').append(e.data + '<br/>');
				};
				$('#cmdLeave').click(function () {
					ws.close();
				});
				ws.onclose = function () {
					$('#OutputWindow').append('Closed <br/>');
				};
				ws.onerror = function (e) {
					$('#OutputWindow').append('Oops something went wront <br/>');
				};
			} else {
				// Show the login / char creation form
				$("#LoginCreateCharacterForm").show();
			};
		});

		function SendMessage() {
			ws.send($('#txtMessage').val());
			$('#txtMessage').val('');
		};

		$(window).keydown(function (event) {
			if (event.keyCode == 13) {
				event.preventDefault();
				SendMessage();
				return false;
			};
		});
	</script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ph_MainContent" runat="server">
	<div id="PlayArea">
		<div>
			<div id="OutputWindow"></div>
		</div>
		<div>
			<input type="text" id="txtMessage" />
			<input type="button" value="Send" id="cmdSend" />
		</div>
	</div>
	

	<div id="LoginCreateCharacterForm">
		<div id="LoginForm">
			<div class="FormRow">
				<div class="FormRomLabel">
					Username:
				</div>
				<div>
					<asp:TextBox runat="server" ID="tb_UserName"></asp:TextBox>
				</div>
			</div>
			<div class="FormRow">
				<div class="FormRomLabel">
					Password:
				</div>
				<div>
					<asp:TextBox runat="server" ID="tb_Password" TextMode="Password"></asp:TextBox>
				</div>
			</div>
			<div class="FormRow">
				<div>
					<asp:Button runat="server" ID="btn_Login" Text="Login" />
				</div>
			</div>
			<div>
				Create Character
			</div>
		</div>
		<div id="CreateForm">
			<div class="FormRow">
				<div class="InformationRow">
					You can create a new character for the game via the below form.
				</div>
			</div>
			<div class="FormRow">
				<div class="FormRomLabel">
					Username:
				</div>
				<div>
					<asp:TextBox runat="server" ID="tb_CreateUserName"></asp:TextBox>
				</div>
			</div>
			<div class="FormRow">
				<div class="FormRomLabel">
					Password:
				</div>
				<div>
					<asp:TextBox runat="server" ID="tb_CreatePassword"></asp:TextBox>
				</div>
			</div>
			<div class="FormRow">
				<div class="FormRomLabel">
					Character Firstname:
				</div>
				<div>
					<asp:TextBox runat="server" ID="tb_FirstName"></asp:TextBox>
				</div>
			</div>
			<div class="FormRow">
				<div class="FormRomLabel">
					Character Lastname:
				</div>
				<div>
					<asp:TextBox runat="server" ID="tb_LastName"></asp:TextBox>
				</div>
			</div>
			<div class="FormRow">
				<div class="FormRomLabel">
					Male or Female
				</div>
				<div>
					<asp:DropDownList runat="server" ID="ddl_Sex">
						<asp:ListItem Text="Male" Value="M"></asp:ListItem>
						<asp:ListItem Text="Female" Value="F"></asp:ListItem>
					</asp:DropDownList>
				</div>
			</div>
			<div class="FormRow">
				<div class="InformationRow">
					<div class="Strong">Note:</div> Your class will be set through gameplay...
				</div>
			</div>
			<div class="FormRow">
				<div class="FormRomLabel">
				</div>
				<div>
					<asp:Button runat="server" ID="btn_CreateCharacter" Text="Create Character" OnClick="btn_CreateCharacter_Click" />
				</div>
			</div>
			<div class="FormRow">
				I already have a character, let me log in
			</div>
		</div>
	</div>

</asp:Content>
