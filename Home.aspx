<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="_200805X_SITConnect.Home" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>

<style>

    .content{
        max-width: 500px;
  margin: auto;
    }

</style>

<body class="content">

    <h1>Home Page</h1>

    <form id="form1" runat="server">

    <div>
        You have Logged In with,
        <asp:Label ID="lbl_displayinput" runat="server"></asp:Label>
        .<br />
        <br />
        <a href="/Login.aspx">To Login Page</a>
    </div>

    <div>
        <a href="/Register.aspx">To Register Page</a>
    </div>

        <div>
            <asp:Button ID="btn_signout" Onclick="signOut" runat="server" Text="Logout " />
        </div>
    </form>
</body>
</html>
