<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Site.Master" AutoEventWireup="true" CodeBehind="JsonBuilder.aspx.cs" Inherits="SlackMUDRPG.JsonBuilder.JsonBuilder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ph_Title" runat="server">
	JSON Object Builder
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ph_Header" runat="server">

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ph_MainContent" runat="server">

	<div class="form-group">
		<label class="control-label col-xs-3">Object Type:</label>
		<div class="col-xs-8">
			<asp:DropDownList ID="objectType" AutoPostBack="true" runat="server" OnSelectedIndexChanged="ObjectTypeChanged" CssClass="form-control"></asp:DropDownList>
		</div>
	</div>

	<div>
		<asp:PlaceHolder ID="mainFormPlaceholder" runat="server"></asp:PlaceHolder>
	</div>
</asp:Content>
