<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="_200805X_SITConnect.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login Page</title>
    <script src="https://www.google.com/recaptcha/api.js?render=6LergWYeAAAAAJcboBuQ1rggAF7oFMSyGe4jbG11"></script>
</head>

    <style>

    .content{
        max-width: 500px;
  margin: auto;
    }

</style>



<body class="content">

    <h1>Login Page</h1>

    <a href="/Register.aspx">To Register Page</a>

    <form id="form1" runat="server" method="post">

        <div asp-validation-summary="All" class="text-danger">
            <asp:Label ID="lbl_message" runat="server"></asp:Label>
            <br />
        </div>

    <div>
        <label class="col-sm-2 col-form-label">E-Mail:</label>&nbsp;
        <asp:TextBox ID="tb_email" runat="server" TextMode="Email"></asp:TextBox>
        <span  class="text-danger"></span>
    </div>
        <br />
    <div>
        <label class="col-sm-2 col-form-label">Password:</label>&nbsp;
        <span class="text-danger">
        <asp:TextBox ID="tb_pass" runat="server" TextMode="Password"></asp:TextBox>
        </span>
    </div>

        <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"/>

        <asp:Button ID="loginbtn" runat="server" OnClick="LoginMe" Text="Login"/>

        

    </form>

    <script>
    grecaptcha.ready(function () {
        grecaptcha.execute('6LergWYeAAAAAJcboBuQ1rggAF7oFMSyGe4jbG11', { action: 'Login' }).then(function (token) {
            document.getElementById("g-recaptcha-response").value = token;
        });
    });
    </script>

</html>
