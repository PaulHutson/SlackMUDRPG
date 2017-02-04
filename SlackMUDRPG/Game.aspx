<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage/Site.Master" AutoEventWireup="true" CodeBehind="Game.aspx.cs" Inherits="SlackMUDRPG.Game" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ph_Title" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ph_Header" runat="server">
	<script src="https://code.jquery.com/jquery-2.2.4.min.js" integrity="sha256-BbhdlvQf/xTY9gja0Dq3HiwQF8LaCRTXxZKRutelT44=" crossorigin="anonymous"></script>
	<script type="text/javascript">
		$(document).ready(function () {
			var name = prompt('what is your name?:');
			var url = 'ws://localhost:57980/Handlers/GameAccess.ashx?userID=' + name;
			ws = new WebSocket(url);
			ws.onopen = function () {
				$('#messages').prepend('Connected <br/>');
				$('#cmdSend').click(function () {
					var thingToSend = name + "|" + $('#txtMessage').val();
					ws.send(thingToSend);
					$('#txtMessage').val('');
				});
			};
			ws.onmessage = function (e) {
				$('#chatMessages').append(e.data + '<br/>');
			};
			$('#cmdLeave').click(function () {
				ws.close();
			});
			ws.onclose = function () {
				$('#chatMessages').append('Closed <br/>');
			};
			ws.onerror = function (e) {
				$('#chatMessages').append('Oops something went wront <br/>');
			};
		});
	</script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ph_MainContent" runat="server">
	<div>
		<div id="chatMessages" />
	</div>
	<div>
		<input id="txtMessage"  /> 
		<input id="cmdSend" type="button" value="Send" />
	</div>
</asp:Content>
