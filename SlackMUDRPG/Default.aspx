<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SlackMUDRPG.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ph_Title" runat="server">
	Province - Welcome To Ravensmere
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ph_Header" runat="server">
	<link rel="stylesheet" href="css/slackmud_page_home.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ph_MainContent" runat="server">
	<section class="page-content" id="gameplay-section">
			<div class="container full-height">
				<div class="row full-height">
					<div class="col-xs-9 full-height text-center">
						<div class="content-block">
							<img class="img-responsive main-logo" src="css/images/province_main_logo.png" alt="Province">

							<div class="home-page-blurb">
								<p>Province is a text base multi-player-rpg in which players are encouraged to explore and
									create in a truly sandbox fantasy environment.</p>

								<p>Journey across the sea to Ravensmere, a newly founded city on the doorstep of an
									unexplored continent. </p>

								<p>Work with other travellers to grow the settlement, hone your skills and venture forth to
									seek new lands, cultures and experiences.</p>

								<p class="tagline"><span>Create your character today</span></p>

								<a class="btn btn-start-playing" href="Game.aspx">start playing</a>
							</div>

						</div>
					</div>

					<div class="col-xs-3 full-height">
						<div class="content-block">

							<div class="list-group slackmud-list-group text-center">
								<a href="#" id="webPlayLink" runat="server" class="list-group-item">
									<img src="css/images/province_p_200.png" alt="Web Play">
									<p class="list-group-item-text">Web Play</p>
								</a>
								<a href="#" id="addSlackLink" runat="server" class="list-group-item">
									<img src="css/images/slack_200.png" alt="Add To Slack">
									<p class="list-group-item-text">Add to Slack</p>
								</a>
								<a href="#" id="addSkypeLink" runat="server" class="list-group-item">
									<img src="css/images/skype_200.png" alt="Add To Skype">
									<p class="list-group-item-text">Add to Skype</p>
								</a>
							</div>
						</div>
					</div>
				</div>
			</div>
		</section>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ph_PageJavaScript" runat="server">
</asp:Content>