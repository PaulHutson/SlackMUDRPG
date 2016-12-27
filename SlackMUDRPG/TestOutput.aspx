<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestOutput.aspx.cs" Inherits="SlackMUDRPG.TestOutput" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <%= Application["TestOutput"] %>
    </div>
    </form>
</body>
</html>
