<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Site.Master" AutoEventWireup="true" CodeBehind="Game.aspx.cs" Inherits="SlackMUDRPG.Game" %>
<%@ MasterType VirtualPath="~/MasterPage/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ph_Title" runat="server">
	Province - Web Play
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

	<!-- Login/create character forms -->
	<section class="login-create-section" id="login-create-section">
		<div class="container">
			<div class="row">
				<div class="col-xs-8 col-xs-offset-2">
					<div class="panel panel-login">
						<div class="panel-heading">
							<div class="row">
								<div class="col-xs-6 tabs tab-login" ID="tab_Login" runat="server">
									Login
								</div>
								<div class="col-xs-6 tabs tab-create" ID="tab_Create" runat="server">
									Create Character
								</div>
							</div>
						</div>
						<div class="panel-body">
							<div class="row">
								<div class="col-xs-12">
									<!-- Login Form -->
									<div class="login-form form-wrapper" ID="form_Login" runat="server">
										<h2>Login To Play</h2>

										<asp:Panel runat="server" ID="pnl_LoginError" Visible="false">
											<asp:Literal runat="server" ID="lit_LoginError"></asp:Literal>
										</asp:Panel>

										<div class="form-group">
											<asp:TextBox runat="server" id="tb_username" class="form-control" placeholder="Username"></asp:TextBox>
										</div>

										<div class="form-group">
											<asp:TextBox runat="server" id="tb_password" TextMode="Password" class="form-control" placeholder="Password"></asp:TextBox>
										</div>

										<div class="form-group">
											<div class="row">
												<div class="col-sm-6 col-sm-offset-3">
													<asp:Button runat="server" id="loginBtn" class="form-control btn btn-submit" Text="Login" OnClick="loginBtnClick" />
												</div>
											</div>
										</div>

										<p class="text-center">Don't have a character? <a href="#" class="create-link">Create</a> one to play.</p>
									</div>

									<!-- Create character form -->
									<div class="create-form form-wrapper" ID="form_Create" runat="server">
										<h2>Create A Character</h2>
										<p>You can create a new account and character for the game via the below form.</p>

										<asp:Panel runat="server" ID="pnl_CreateError" Visible="false">
											<asp:Literal runat="server" ID="lit_CreateError"></asp:Literal>
										</asp:Panel>

										<div class="form-group">
											<asp:TextBox runat="server" id="newUsername" class="form-control" placeholder="Username"></asp:TextBox>
										</div>

										<div class="form-group">
											<asp:TextBox runat="server" id="email" TextMode="Email" class="form-control" placeholder="Email Address"></asp:TextBox>
										</div>

										<div class="form-group">
											<asp:TextBox runat="server" id="newPassword" TextMode="Password" class="form-control" placeholder="Password"></asp:TextBox>
										</div>

										<div class="form-group">
											<asp:TextBox runat="server" id="repeatPassword" TextMode="Password" class="form-control" placeholder="Repeat Password"></asp:TextBox>
										</div>

										<p class="text-center"><strong>Note: </strong> Your character details will be set through gameplay.</p>

										<div class="form-group">
											<div class="row">
												<div class="col-sm-6 col-sm-offset-3">
													<asp:Button runat="server" id="createBtn" class="form-control btn btn-submit" Text="Create Character" OnClick="createBtnClick" />
												</div>
											</div>
										</div>

										<p class="text-center">Already registered? <a href="#" class="login-link">Login</a> to play.</p>
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	</section>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ph_PageJavaScript" runat="server">
	<script type="text/javascript" src="scripts/slackmud_gamepage_websockets.js"></script>
	<script type="text/javascript" src="scripts/slackmud_login_create.js"></script>
</asp:Content>