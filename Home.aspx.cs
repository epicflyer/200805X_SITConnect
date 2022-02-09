using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace _200805X_SITConnect
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (!Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    System.Diagnostics.Debug.WriteLine("Home Page - Home page Loading Failed, redirecting back to Login Page...");
                    Response.Redirect("Login.aspx", false);
                }
                System.Diagnostics.Debug.WriteLine("Home Page - Loading Home Page...");
                lbl_displayinput.Text = Session["useremail"].ToString();

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Home Page - Home page Loading Failed, redirecting back to Login Page...");
                Response.Redirect("Login.aspx", false);
            }
        }

        protected void signOut(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies["AuthToken"].Value = string.Empty;
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
            }
            System.Diagnostics.Debug.WriteLine("Home Page - User has been Logged Out.");
            System.Diagnostics.Debug.WriteLine("Home Page - Redirecting user back to Login Page...");
            Response.Redirect("Login.aspx", false);
        }

    }
}