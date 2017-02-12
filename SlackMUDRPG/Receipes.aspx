<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Site.Master" AutoEventWireup="true" CodeBehind="Receipes.aspx.cs" Inherits="SlackMUDRPG.Receipes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ph_Title" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ph_Header" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ph_MainContent" runat="server">
	<asp:Literal runat="server" ID="lit_ReceipeList"></asp:Literal>
</asp:Content>
