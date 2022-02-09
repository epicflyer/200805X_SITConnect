using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Data;
using System.Drawing;

namespace _200805X_SITConnect
{

    public partial class Login : System.Web.UI.Page
    {

        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        byte[] Key;
        byte[] IV;

        public class MyObject
        {
            public string success { get; set; }
            public List<string> ErrorMessage { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Login Page - Loading Login Page...");
        }

        public bool ValidateCaptcha()
        {
            System.Diagnostics.Debug.WriteLine("Login Page - Running Captcha Validation...");
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=6LergWYeAAAAAE5XqAi4j_dtftdQThWQ_eKfuRn_ &response=" + captchaResponse);

            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);

                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }
                return result;
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
        }

        protected void LoginMe(object sender, EventArgs e)
        {
            
            string pwd = tb_pass.Text.ToString().Trim();
            string email = tb_email.Text.ToString().Trim();

            var failureStatus = getFailureStatus(email);
            var failureCount = getFailureCount(email);
            System.Diagnostics.Debug.WriteLine("Login Page - "+email +" User logging in...");

            if (failureStatus == "True")
            {
                System.Diagnostics.Debug.WriteLine("Login Page - Account is Locked.");
                var LockedoutTime = getLockoutDate(email);
                System.Diagnostics.Debug.WriteLine("Login Page - Account has been Locked out at: " + LockedoutTime);
                DateTime NowTime = DateTime.Now;
                System.Diagnostics.Debug.WriteLine("Login Page - Current Time: " + NowTime);
                DateTime AccountRecoveryTime = LockedoutTime.AddMinutes(2);
                System.Diagnostics.Debug.WriteLine("Login Page - Account Recovery Time will be: " + AccountRecoveryTime);
                if (AccountRecoveryTime > NowTime)
                {
                    System.Diagnostics.Debug.WriteLine("Login Page - Account Recovery Failed.");
                    lbl_message.Text = "This account has been locked out. Due to multiple login failure attempts. Please try again after 2 mins. \nAt: " + AccountRecoveryTime;
                    lbl_message.ForeColor = Color.Red;
                    return;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Login Page - Account Recovery Successful.");
                    failureCount = 0;
                    failureStatus = "False";
                    editFailureCount(email, failureCount);
                    editFailureStatusAndFailureDate(email, failureStatus, DateTime.Now);
                    System.Diagnostics.Debug.WriteLine("Login Page - Successfully Reset FailureCount, FailureStatus for Email: " + email + ", Failure Count: " + failureCount + ", Failure Status: " + failureStatus);
                }

            }


            SHA512Managed hashing = new SHA512Managed();
            string dbHash = getDBHash(email);
            string dbSalt = getDBSalt(email);

            if (ValidateCaptcha())
            {
                System.Diagnostics.Debug.WriteLine("Login Page - Captcha is Succesful.");
                try
                {
                    if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                    {
                        string pwdWithSalt = pwd + dbSalt;
                        byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                        string userHash = Convert.ToBase64String(hashWithSalt);
                        if (userHash.Equals(dbHash))
                        {
                            System.Diagnostics.Debug.WriteLine("Login Page - Successfully Logged in to Account: " + email);
                            failureCount = 0;
                            failureStatus = "False";
                            editFailureCount(email, failureCount);
                            editFailureStatusAndFailureDate(email, failureStatus, DateTime.Now);
                            System.Diagnostics.Debug.WriteLine("Login Page - Upon Successfully Logged in to Account: " + email + ", FailureCount and FailureStatus has been Updated. Failure Count: " + failureCount+ " Failure Status: " + failureStatus);
                            Session["LoggedIn"] = tb_email.Text.Trim();

                            // creates a new GUID and save into the session
                            string guid = Guid.NewGuid().ToString();
                            Session["AuthToken"] = guid;

                            // now create a new cookie with this guid value
                            Response.Cookies.Add(new HttpCookie("AuthToken", guid));
                            Session["useremail"] = HttpUtility.HtmlEncode(email);
                            System.Diagnostics.Debug.WriteLine("Login Page - Redirecting to Home Page...");
                            Response.Redirect("Home.aspx", false);
                        }


                        else
                        {
                            lbl_message.Text = "Invalid Login Details, Please Try again.";
                            lbl_message.ForeColor = Color.Red ;

                            failureCount += 1;
                            System.Diagnostics.Debug.WriteLine("Login Page - Login Failure Counter: " + failureCount);
                            if (failureCount < 3)
                            {
                                editFailureCount(email, failureCount);
                                System.Diagnostics.Debug.WriteLine("Login Page - Updated FailureCount of Email: " + email + ", Failure Count: " + failureCount);
                                return;
                            }
                            else if (failureCount == 3)
                            {
                                System.Diagnostics.Debug.WriteLine("Login Page - "+ email + " Account has been Locked.");
                                failureStatus = "True";
                                DateTime LockedoutTime = DateTime.Now;
                                editFailureStatusAndFailureDate(email, failureStatus, LockedoutTime);
                                System.Diagnostics.Debug.WriteLine("Login Page - Updated FailureCount of Email: " + email + ", Failure Count: " + failureCount + ", LockedOutDateTime: " + LockedoutTime);
                                lbl_message.Text = "Due to multiple Login Failure Attempts \nEmail Account - " + email + " has been Locked Out. \nPlease try again in 2 mins.";
                                lbl_message.ForeColor = Color.Red;
                                return;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                finally { }
            }
        }
        

        protected string getDBHash(string email)
        {
            string h = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PasswordHash FROM [User] WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null)
                        {
                            if (reader["PasswordHash"] != DBNull.Value)
                            {
                                h = reader["PasswordHash"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return h;
        }

        protected string getDBSalt(string email)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select PASSWORDSALT FROM [User] WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PASSWORDSALT"] != null)
                        {
                            if (reader["PASSWORDSALT"] != DBNull.Value)
                            {
                                s = reader["PASSWORDSALT"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }

        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0,
               plainText.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return cipherText;
        }



        protected string getFailureStatus(string userid)
        {
            string s = "";
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select FailureStatus FROM [User] WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((string)reader["FailureStatus"] != null)
                        {
                            if (reader["FailureStatus"] != DBNull.Value)
                            {
                                s = (string)reader["FailureStatus"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }


        protected int getFailureCount(string userid)
        {
            int s = 0;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select FailureCount FROM [User] WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((int)reader["FailureCount"] != 0)
                        {
                            if (reader["FailureCount"] != DBNull.Value)
                            {
                                s = (int)reader["FailureCount"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }

        protected DateTime getLockoutDate(string userid)
        {
            DateTime s = DateTime.Now;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select LockoutDate FROM [User] WHERE Email=@USERID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@USERID", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((DateTime)reader["LockoutDate"] != null)
                        {
                            if (reader["LockoutDate"] != DBNull.Value)
                            {
                                s = (DateTime)reader["LockoutDate"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }
        protected void editFailureCount(string userid, int failurecount)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE [User] SET FailureCount = @FailureCount WHERE Email = @USERID"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@FailureCount", failurecount);
                            cmd.Parameters.AddWithValue("@USERID", userid);

                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            { }
        }
        protected void editFailureStatusAndFailureDate(string userid, string failurestatus, DateTime currentDateTime)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE [User] SET FailureStatus = @FailureStatus, LockoutDate = @LockoutDate WHERE Email = @USERID"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@FailureStatus", failurestatus);
                            cmd.Parameters.AddWithValue("@LockoutDate", currentDateTime);
                            cmd.Parameters.AddWithValue("@USERID", userid);

                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            { }
        }



    }
}