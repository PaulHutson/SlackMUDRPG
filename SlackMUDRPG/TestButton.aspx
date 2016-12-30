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
        </div>
        <div>
            CharID : <asp:TextBox runat="server" ID="tb_CharID">123</asp:TextBox><br /><br />
        </div>
        <div>
            <asp:Button runat="server" ID="btn_Test" Text="Test Login" OnClick="btn_Test_Click" />
            <asp:Button runat="server" ID="btn_GetLocation" Text="Test Location 1" OnClick="btn_TestLoc_Click" />
            <asp:Button runat="server" ID="btn_CreateCharacter" Text="Test Create Character" OnClick="btn_CreateCharacter_Click" />
        </div>
    </form>
</body>
</html>
