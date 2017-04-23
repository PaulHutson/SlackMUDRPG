<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Site.Master" AutoEventWireup="true" CodeBehind="Skills.aspx.cs" Inherits="SlackMUDRPG.Skills" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ph_Title" runat="server">
	Province - Skills
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ph_Header" runat="server">
	<link rel="stylesheet" href="css/slackmud_page_info.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ph_MainContent" runat="server">
	<section class="page-content">
		<div class="container full-height">
			<div class="row full-height">
				<div class="col-xs-12 full-height">
					<div class="content-block">
						<div class="panel-group game-info-accordion" id="accordion">
							<asp:Literal runat="server" id="skillListings"></asp:Literal>
						</div>
					</div>
				</div>
			</div>
		</div>
	</section>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ph_PageJavaScript" runat="server">
</asp:Content>
