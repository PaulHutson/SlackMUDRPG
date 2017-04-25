<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Site.Master" AutoEventWireup="true" CodeBehind="Commands.aspx.cs" Inherits="SlackMUDRPG.Commands" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ph_Title" runat="server">
	Province - Commands
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ph_Header" runat="server">
	<link rel="stylesheet" href="css/slackmud_page_info.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ph_MainContent" runat="server">
	<section class="page-content">
		<div class="container full-height">
			<div class="row full-height">
				<div class="col-xs-3 full-height">
					<div class="content-block">
						<nav class="side-nav">
							<ul>
								<asp:Literal runat="server" id="commandTypesList"></asp:Literal>
							</ul>
						</nav>
					</div>
				</div>

				<div class="col-xs-9 full-height">
					<div class="content-block">
						<div class="panel-group game-info-accordion" id="accordion">
							<asp:Literal runat="server" id="commandListings"></asp:Literal>
						</div>
					</div>
				</div>
			</div>
		</div>
	</section>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ph_PageJavaScript" runat="server">
	<script type="text/javascript" src="scripts/slackmud_info_page.js"></script>
</asp:Content>