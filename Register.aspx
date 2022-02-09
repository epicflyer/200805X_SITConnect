<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="_200805X_SITConnect.Register" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Register Page</title>
</head>
<style>

    .content{
        max-width: 500px;
  margin: auto;
    }

</style>

<body class="content">
    <h1>Register Page</h1>

    <a href="/Login.aspx">To Login Page</a>

    <form id="form1" runat="server" method="post">

        <div asp-validation-summary="All" class="text-danger"></div>

    <div>
        <label class="col-sm-2 col-form-label">First Name:</label>&nbsp;
        <asp:TextBox ID="tb_fname" runat="server"></asp:TextBox>
        <asp:Label ID="lbl_fname" runat="server"></asp:Label>
        <span class="text-danger"></span>
    </div>

    <div>
        <label class="col-sm-2 col-form-label">Last Name:</label>&nbsp;
        <span class="text-danger">
        <asp:TextBox ID="tb_lname" runat="server"></asp:TextBox>
        <asp:Label ID="lbl_lname" runat="server"></asp:Label>
        </span>

    </div>
    <div>
        <label class="col-sm-2 col-form-label">Credit Card Info:</label>&nbsp;
        <span class="text-danger">
        <asp:TextBox ID="tb_creditcard" runat="server" onkeyup="javascript:creditvalidation()"></asp:TextBox>
        <asp:Label ID="lbl_credit" runat="server"></asp:Label>
        <br />
        <asp:Button ID="btn_checkcredit" runat="server" OnClick="btn_checkcredit_Click" Text="Check Credit Card" />
        <asp:Label ID="lbl_checkcredit" runat="server"></asp:Label>
        </span>
    </div>
    <div>
        <label class="col-sm-2 col-form-label">E-Mail:</label>&nbsp;
        <span class="text-danger">
        <asp:TextBox ID="tb_email" runat="server" TextMode="Email"></asp:TextBox>
        <asp:Label ID="lbl_email" runat="server"></asp:Label>
        </span>
    </div>
    <div>
        <label class="col-sm-2 col-form-label">Password:</label>&nbsp;
        <span class="text-danger">
        <asp:TextBox ID="tb_pass" runat="server" onkeyup="javascript:passvalidation()"></asp:TextBox>
        <asp:Label ID="lbl_pass" runat="server"></asp:Label>
        <br />
        <asp:Button ID="btn_checkpass" runat="server" OnClick="btn_checkpass_Click" Text="Check Password" />
        <asp:Label ID="lbl_checkpass" runat="server"></asp:Label>
        </span>
    </div>
    
    <div>
        <label class="col-sm-2 col-form-label">Birth Date:</label>&nbsp;
        <span class="text-danger">
        <asp:Calendar ID="cal_birthdate" runat="server"></asp:Calendar>
        <asp:Label ID="lbl_dob" runat="server"></asp:Label>
        </span>
    </div>

     <div>
        <label class="col-sm-2 col-form-label">Photo:</label>&nbsp;
         <asp:FileUpload ID="fu_file" runat="server" />
         <br />
         <asp:Label ID="lbl_picformat" runat="server"></asp:Label>
    </div>


    <asp:Button ID="createbtn" runat="server" OnClick="RegisterMe" Text="Create" />
    


    

    </form>
</body>

<script type ="text/javascript">
    function passvalidation() {


        var passStr = document.getElementById('<%=tb_pass.ClientID%>').value;

        if (passStr.length < 12) {
            document.getElementById('lbl_pass').innerHTML = "Password length must be at least 12 Characters.";
            document.getElementById('lbl_pass').style.color = "Red";
            return ("too_short");
        }
        else if (passStr.search(/[0-9]/) == -1) {
            document.getElementById('lbl_pass').innerHTML = "Password requires at least 1 number.";
            document.getElementById('lbl_pass').style.color = "Red";
            return ("no_number");
        }
        else if (passStr.search(/[A-Z]/) == -1) {
            document.getElementById('lbl_pass').innerHTML = "Password requires at least 1 Capitalized character.";
            document.getElementById('lbl_pass').style.color = "Red";
            return ("no_capitalchar");
        }
        else if (passStr.search(/[^a-zA-Z0-9]/) == -1) {
            document.getElementById('lbl_pass').innerHTML = "Password requires at least 1 special character.";
            document.getElementById('lbl_pass').style.color = "Red";
            return ("no_specialchar");
        }
        document.getElementById('lbl_pass').innerHTML = "Strong Password!";
        document.getElementById('lbl_pass').style.color = "Blue";
    }

    function creditvalidation() {


        var creditStr = document.getElementById('<%=tb_creditcard.ClientID%>').value;

        if (creditStr.length != 16) {
            document.getElementById('lbl_credit').innerHTML = "Credit Card Information should contain at least 16 numbers.";
            document.getElementById('lbl_credit').style.color = "Red";
            return ("too_short");
        }
        else if (creditStr.search(/[a-zA-Z]/) >= 0) {
            document.getElementById('lbl_credit').innerHTML = "Invalid Credit Card Information.";
            document.getElementById('lbl_credit').style.color = "Red";
            return ("no_capitalchar");
        }
        else if (creditStr.search(/[^0-9]/) >= 0) {
            document.getElementById('lbl_credit').innerHTML = "Credit Card should only contains numbers.";
            document.getElementById('lbl_credit').style.color = "Red";
            return ("no_specialchar");
        }
        document.getElementById('lbl_credit').innerHTML = "Valid Credit Card!";
        document.getElementById('lbl_credit').style.color = "Blue";
    }
</script>

</html>
