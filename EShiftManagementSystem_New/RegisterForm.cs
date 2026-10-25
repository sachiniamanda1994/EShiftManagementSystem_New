using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EShiftManagementSystem_New
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fullName = textBox1.Text.Trim();
            string email = textBox2.Text.Trim();
            string phone = textBox3.Text.Trim();
            string address = textBox4.Text.Trim();
            string password = textBox5.Text;
            string confirmPassword = textBox6.Text;

            if (fullName == "" || email == "" ||phone == "" || password == "" || confirmPassword == "")
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }

            if (!Regex.IsMatch(email,@"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address.");
                textBox2.Focus();
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show(
                    "Password must contain at least 8 characters.");
                textBox2.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                textBox6.Clear();
                textBox6.Focus();
                return;
            }

            try
            {
                // Hash the password before storing it
                string passwordHash = HashPassword(password);

                DatabaseConnection db = new DatabaseConnection();
                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string sql = @"INSERT INTO customers (FullName, Email, Phone, Address, PasswordHash) VALUES (@FullName, @Email, @Phone, @Address, @PasswordHash)";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Address", address);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Registration successful! You can now log in.");

                ClearFields();

                Login login = new Login();
                login.Show();
                this.Hide();
            }catch(MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    MessageBox.Show("This email address is already registered.");
                }
                else
                {
                    MessageBox.Show("Database error: " + ex.Message);
                }
            }
        }


        private string HashPassword(string password)
        {
            int iterations = 100000;
            byte[] salt = new byte[16];

            using (System.Security.Cryptography.RandomNumberGenerator rng =
                System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (System.Security.Cryptography.Rfc2898DeriveBytes pbkdf2 =
                new System.Security.Cryptography.Rfc2898DeriveBytes(
                    password, salt, iterations))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return iterations + ":" +
                    Convert.ToBase64String(salt) + ":" +
                    Convert.ToBase64String(hash);
            }
        }

        private void ClearFields()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();

            textBox1.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
        }
    }
}
