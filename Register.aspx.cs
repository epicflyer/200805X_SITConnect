using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;


namespace _200805X_SITConnect
{
    public partial class Register : System.Web.UI.Page
    {

        string MYDBConnectionString =System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Register Page - Loading Register Page...");
        }

        protected void RegisterMe(object sender, EventArgs e)
        {
            var error = "";
            var fname = tb_fname.Text;
            var lname = tb_lname.Text;
            var creditcard = tb_creditcard.Text;
            var email = tb_email.Text;
            var pass = tb_pass.Text.ToString().Trim();
            var DOB = cal_birthdate.SelectedDate;
            using (Stream fs = fu_file.PostedFile.InputStream)
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] bytes = br.ReadBytes((Int32)fs.Length);
                }
            }
            if (fname == "")
            {
                error = "error";
                lbl_fname.Text = "Please fill up your First Name.";
                lbl_fname.ForeColor = Color.Red;
            }
            if (lname == "")
            {
                error = "error";
                lbl_lname.Text = "Please fill up your Last Name.";
                lbl_lname.ForeColor = Color.Red;
            }
            if (creditcard == "" || creditcard.Length != 16)
            {
                error = "error";
                lbl_credit.Text = "Invalid Credit Card Information.";
                lbl_credit.ForeColor = Color.Red;
            }
            if (email == "")
            {
                error = "error";
                lbl_email.Text = "Please enter your Email.";
                lbl_email.ForeColor = Color.Red;
            }
            if (pass == "")
            {
                error = "error";
                lbl_pass.Text = "Please enter your password.";
                lbl_pass.ForeColor = Color.Red;
            }
            if (DOB.ToString() == "1/1/0001 12:00:00 am")
            {
                error = "error";
                lbl_dob.Text = "Please Select your Date of Birth.";
                lbl_dob.ForeColor = Color.Red;
            }
            if (fu_file.FileName.ToString().ToLower().EndsWith(".png") == false && fu_file.FileName.ToString().ToLower().EndsWith(".jpg") == false)
            {
                error = "error";
                lbl_picformat.Text = "Only allow Image file that ends with .png or .jpg";
                lbl_picformat.ForeColor = Color.Red;
            }
            if(error == "")
            {
                if (findemailexist(email) == "")
                {
                    //Generate random "salt"
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                    byte[] saltByte = new byte[8];
                    //Fills array of bytes with a cryptographically strong sequence of random values.
                    rng.GetBytes(saltByte);
                    salt = Convert.ToBase64String(saltByte);
                    SHA512Managed hashing = new SHA512Managed();
                    string pwdWithSalt = pass + salt;
                    byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(pass));
                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                    finalHash = Convert.ToBase64String(hashWithSalt);
                    RijndaelManaged cipher = new RijndaelManaged();
                    cipher.GenerateKey();
                    Key = cipher.Key;
                    IV = cipher.IV;
                    System.Diagnostics.Debug.WriteLine("Register Page - Account: " + email + " is being added into DB...");
                    registerAccount();
                    System.Diagnostics.Debug.WriteLine("Register Page - Account: " + email + " has been successfully created into DB.");
                    System.Diagnostics.Debug.WriteLine("Register Page - Redirecting to Login Page...");
                    Response.Redirect("Login.aspx", true);
                }
                else
                {

                    lbl_email.Text = "This Email already Exists.";
                    lbl_email.ForeColor = Color.Red;

                }
            }
            
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
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0,plainText.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            System.Diagnostics.Debug.WriteLine("Register Page - Successfully Encrypted Credit Card Info");
            return cipherText;
        }


        protected void registerAccount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    System.Diagnostics.Debug.WriteLine("zane1");
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [User] VALUES(@Fname, @Lname, @CreditCard, @Email, @Password, @PasswordHash, @PasswordSalt, @DOB, @Photo, @FailureCount, @FailureStatus, @LockoutDate)"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Fname", tb_fname.Text.Trim());
                            cmd.Parameters.AddWithValue("@Lname", tb_lname.Text.Trim());
                            cmd.Parameters.AddWithValue("@CreditCard", encryptData(tb_creditcard.Text.Trim()));
                            cmd.Parameters.AddWithValue("@Email", tb_email.Text.Trim()); 
                            cmd.Parameters.AddWithValue("@Password", tb_pass.Text.Trim());
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                            cmd.Parameters.AddWithValue("@DOB", cal_birthdate.SelectedDate);
                            using (Stream pic = fu_file.PostedFile.InputStream)
                            {
                                using (BinaryReader br = new BinaryReader(pic))
                                {
                                    byte[] bytes = br.ReadBytes((Int32)pic.Length);
                                    cmd.Parameters.AddWithValue("@Photo", bytes);
                                }
                            }
                            cmd.Parameters.AddWithValue("@FailureCount", 0);
                            cmd.Parameters.AddWithValue("@FailureStatus", "False");
                            cmd.Parameters.AddWithValue("@LockoutDate", DateTime.Now);
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
        }


        protected string findemailexist(string userinputemail)
        {
            string s = "";
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = "select Email FROM [User]";
            SqlCommand command = new SqlCommand(sql, connection);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((string)reader["Email"] != null)
                        {
                            if (reader["Email"] != DBNull.Value)
                            {
                                s = (string)reader["Email"];
                                if(s == userinputemail)
                                {
                                    s = "User already exists";
                                }
                                else
                                {
                                    s = "";
                                }
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


        protected void btn_checkpass_Click(object sender, EventArgs e)
        {
            int scores = checkPassword(tb_pass.Text);
            string status = "";
            switch (scores)
            {
                case 1:
                    status = "Very Weak";
                    break;
                case 2:
                    status = "Weak";
                    break;
                case 3:
                    status = "Medium";
                    break;
                case 4:
                    status = "Strong";
                    break;
                case 5:
                    status = "Excellent";
                    break;
                default:
                    break;
            }
            lbl_checkpass.Text = "  Password Complexity : " + status;
            if (scores < 4)
            {
                lbl_checkpass.ForeColor = Color.Red;
                return;
            }
            lbl_checkpass.ForeColor = Color.Green;

        }
        private int checkPassword(string password)
        {
            int score = 0;
            if (password.Length < 12)
            {
                return 1;
            }
            else
            {
                score = 1;
            }
            if (Regex.IsMatch(password, "[a-z]"))
            {
                score += 1;
            }
            if (Regex.IsMatch(password, "[A-Z]"))
            {
                score += 1;
            }
            if (Regex.IsMatch(password, "[0-9]"))
            {
                score += 1;
            }
            if (Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            {
                score += 1;
            }
            return score;
        }

        protected void btn_checkcredit_Click(object sender, EventArgs e)
        {
            int scores = checkCreditCard(tb_creditcard.Text);
            string status = "";
            switch (scores)
            {
                case 0:
                    status = "Valid";
                    break;
                case 1:
                    status = "Invalid";
                    break;
                case 2:
                    status = "Invalid";
                    break;
                default:
                    break;
            }
            lbl_checkcredit.Text = "  Credit Card : " + status;
            if (scores >= 1)
            {
                lbl_checkcredit.ForeColor = Color.Red;
                return;
            }
            lbl_checkcredit.ForeColor = Color.Green;

        }

        private int checkCreditCard(string creditcard)
        {
            int score = 0;
            if (creditcard.Length != 16)
            {
                return 1;
            }
            else
            {
                score = 0;
            }
            if (Regex.IsMatch(creditcard, "[^0-9][16]"))
            {
                score += 1;
            }
            if (Regex.IsMatch(creditcard, "[a-zA-Z]"))
            {
                score += 1;
            }
            return score;
        }

        


    }

    

    
}