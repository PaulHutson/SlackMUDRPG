<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Site.Master" AutoEventWireup="true" CodeBehind="Game.aspx.cs" Inherits="SlackMUDRPG.Game" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ph_Title" runat="server">
	TODO
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ph_Header" runat="server">
	<link rel="stylesheet" href="css/slackmud_page_game.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ph_MainContent" runat="server">

		<!-- Game play section -->
		<section class="page-content" id="gameplay-section">
			<div class="container full-height">
				<div class="row full-height">
					<div class="col-xs-12 full-height">
						<div class="content-block">
							<div class="game-output" id="game-output"></div>
							<div class="game-input">
								<div class="input-group">
									<input type="text" class="form-control" placeholder="Enter commands here..." id="game-input">
									<span class="input-group-btn">
										<button class="btn btn-default" type="button" id="submit-cmd">Send</button>
									</span>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</section>

		<!-- Login/Create character form -->
		<!--
		<section class="page-content" id="login-create-section">
			LOGIN/CREATE
		</section>
		-->

	<div id="login-create-section">
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
				<div class="InformationRow">
					<div class="Strong">Note:</div> Your character details will be set through gameplay...
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

<asp:Content ID="Content4" ContentPlaceHolderID="ph_PageJavaScript" runat="server">
	<script type="text/javascript" src="scripts/slackmud_gamepage_websockets.js"></script>
</asp:Content>

