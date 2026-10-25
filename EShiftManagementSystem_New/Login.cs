using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace EShiftManagementSystem_New
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        public void ClearLoginFields()
        {
            textBox1.Clear();  // Clear email
            textBox2.Clear();  // Clear password
            textBox1.Focus();
        }

        //private bool VerifyPassword(string password, string storedHash)
        //{
        //    try
        //    {
        //        string[] parts = storedHash.Split(':');

        //        if (parts.Length != 3)
        //            return false;

        //        int iterations = int.Parse(parts[0]);
        //        byte[] salt = Convert.FromBase64String(parts[1]);
        //        byte[] originalHash = Convert.FromBase64String(parts[2]);

        //        using (var pbkdf2 = new Rfc2898DeriveBytes(
        //            password, salt, iterations))
        //        {
        //            byte[] enteredHash =
        //                pbkdf2.GetBytes(originalHash.Length);

        //            return CryptographicOperations.FixedTimeEquals(
        //                enteredHash, originalHash);
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        private void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text.Trim();
            string password = textBox2.Text;



            if (email == "" || password == "")
            {
                MessageBox.Show("Please enter email and password.");
                return;
            }


            if (!Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email.");
                return;
            }

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    // Check administrator account
                    string sql = "SELECT AdminID, PasswordHash FROM admins " +
                                 "WHERE Email = @email";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@email", email);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPassword =
                                    reader["PasswordHash"].ToString();

                                // Admin password is compared directly here.
                                // Use this only if admin passwords are stored as plain text.
                                if (password == storedPassword)                             
                                {
                                    reader.Close();

                                    MessageBox.Show("Admin login successful!");

                                    AdminDashboard a_dashboard = new AdminDashboard();

                                    a_dashboard.Show();
                                    this.Hide();
                                    return;
                                }
                            }
                        }
                    }

                    // Check customer account
                    string sql2 = "SELECT CustomerID, PasswordHash FROM customers " +
                                  "WHERE Email = @email";


                    using (MySqlCommand cmd2 = new MySqlCommand(sql2, con))
                    {
                        cmd2.Parameters.AddWithValue("@email", email);

                        using (MySqlDataReader reader = cmd2.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int customerId = Convert.ToInt32(reader["CustomerID"]);
                                string storedHash =
                                    reader["PasswordHash"].ToString();

                                if (VerifyPassword(password, storedHash))
                                {
                                    reader.Close();

                                    MessageBox.Show("Customer login successful!");
                                    
                                    CustomerDashboard c_dashboard = new CustomerDashboard(customerId);
                                    c_dashboard.Show();
                                    this.Hide();
                                    return;
                                }
                            }
                        }
                    }

                    MessageBox.Show("Incorrect email or password.");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "MySQL error " + ex.Number + ": " + ex.Message,
                    "Database Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        




        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                string[] parts = storedHash.Split(':');

                if (parts.Length != 3)
                    return false;

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] originalHash = Convert.FromBase64String(parts[2]);

                using (System.Security.Cryptography.Rfc2898DeriveBytes
                    pbkdf2 =
                    new System.Security.Cryptography.Rfc2898DeriveBytes(
                        password, salt, iterations))
                {
                    byte[] enteredHash = pbkdf2.GetBytes(originalHash.Length);

                    return System.Linq.Enumerable.SequenceEqual(
                        originalHash, enteredHash);
                }
            }catch
            {
                return false;
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm= new RegisterForm();
            registerForm.Show();
            this.Hide();
            return;
        }

       
    }
}
