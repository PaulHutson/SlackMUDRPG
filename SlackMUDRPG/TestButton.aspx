<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestButton.aspx.cs" Inherits="SlackMUDRPG.TestButton" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Literal runat="server" ID="lit_OutputExample"></asp:Literal><br /><br />
            <asp:Button runat="server" ID="btn_Test" Text="Test Login" OnClick="btn_Test_Click" />
            <asp:Button runat="server" ID="btn_CreateCharacter" Text="Test Create Character" OnClick="btn_CreateCharacter_Click" />
        </div>
    </form>
</body>
</html>
