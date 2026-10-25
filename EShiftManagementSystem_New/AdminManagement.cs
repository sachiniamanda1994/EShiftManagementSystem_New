using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace EShiftManagementSystem_New
{
    public partial class AdminManagement : Form
    {
        public AdminManagement()
        {
            InitializeComponent();
            LoadAdmins();
        }


        private string HashPassword(string password)
        {
            int iterations = 100000;

            byte[] salt = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, iterations))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return iterations + ":" +
                       Convert.ToBase64String(salt) + ":" +
                       Convert.ToBase64String(hash);
            }
        }
        private void AdminManagement_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        private void LoadAdmins()
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT AdminID, FullName, Email
                FROM admins
                ORDER BY AdminID ASC";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dataGridView1.DataSource = dt;

                        dataGridView1.ReadOnly = true;
                        dataGridView1.AllowUserToAddRows = false;
                        dataGridView1.AutoSizeColumnsMode =
                            DataGridViewAutoSizeColumnsMode.Fill;

                        dataGridView1.SelectionMode =
                            DataGridViewSelectionMode.FullRowSelect;

                        dataGridView1.MultiSelect = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading administrators: " + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            

                if (e.RowIndex < 0)
                    return;

                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                textBox1.Text =
                    row.Cells["AdminID"].Value.ToString();

                textBox2.Text =
                    row.Cells["FullName"].Value.ToString();

                textBox3.Text =
                    row.Cells["Email"].Value.ToString();

                // Do not display or retrieve the existing password hash.
                textBox4.Clear();
            }

        private void button1_Click(object sender, EventArgs e)
        {
            string fullName = textBox2.Text.Trim();
            string email = textBox3.Text.Trim();
            string password = textBox4.Text;

            // Validate required fields
            if (fullName == "" || email == "" || password == "")
            {
                MessageBox.Show(
                    "Please enter the full name, email, and password.");
                return;
            }

            // Validate email format
            if (!Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address.");
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show(
                    "Password must contain at least 8 characters.");
                return;
            }

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    // Check whether the email already exists
                    string checkQuery =
                        "SELECT COUNT(*) FROM admins WHERE Email = @Email";

                    using (MySqlCommand checkCmd =
                        new MySqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", email);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show(
                                "An administrator with this email already exists.");
                            return;
                        }
                    }

                    // Hash the password before storing it
                    string passwordHash = HashPassword(password);

                    string insertQuery = @"
                INSERT INTO admins (FullName, Email, PasswordHash)
                VALUES (@FullName, @Email, @PasswordHash)";

                    using (MySqlCommand cmd =
                        new MySqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Administrator added successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadAdmins();

                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox4.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error adding administrator:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string adminID = textBox1.Text.Trim();
            string fullName = textBox2.Text.Trim();
            string email = textBox3.Text.Trim();
            string password = textBox4.Text;

            // Check that an administrator is selected
            if (adminID == "")
            {
                MessageBox.Show("Please select an administrator from the table.");
                return;
            }

            // Validate required fields
            if (fullName == "" || email == "")
            {
                MessageBox.Show("Full Name and Email are required.");
                return;
            }

            // Validate email format
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address.");
                return;
            }

            // Validate a new password only when one is entered
            if (password != "" && password.Length < 8)
            {
                MessageBox.Show("A new password must contain at least 8 characters.");
                return;
            }

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    // Prevent duplicate email addresses
                    string checkQuery = @"
                SELECT COUNT(*)
                FROM admins
                WHERE Email = @Email
                  AND AdminID <> @AdminID";

                    using (MySqlCommand checkCmd =
                        new MySqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", email);
                        checkCmd.Parameters.AddWithValue("@AdminID", adminID);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show(
                                "Another administrator already uses this email.");
                            return;
                        }
                    }

                    string passwordHash = "";

                    if (password != "")
                    {
                        passwordHash = HashPassword(password);
                    }

                    string query;

                    if (password == "")
                    {
                        // Keep the existing password hash unchanged
                        query = @"
                    UPDATE admins
                    SET FullName = @FullName,
                        Email = @Email
                    WHERE AdminID = @AdminID";
                    }
                    else
                    {
                        // Hash the new password before updating
                        //string passwordHash = HashPassword(password);

                        query = @"
                    UPDATE admins
                    SET FullName = @FullName,
                        Email = @Email,
                        PasswordHash = @PasswordHash
                    WHERE AdminID = @AdminID";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@AdminID", adminID);

                        if (password != "")
                        {
                            cmd.Parameters.AddWithValue(
                                "@PasswordHash", HashPassword(password));
                        }

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Administrator updated successfully!");
                        }
                        else
                        {
                            MessageBox.Show("No changes were made to the administrator.");
                        }
                    }
                }

                LoadAdmins();

                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox4.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error updating administrator:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string adminID = textBox1.Text.Trim();

            if (adminID == "")
            {
                MessageBox.Show("Please select an administrator to delete.");
                return;
            }

            // Prevent accidental deletion of an administrator
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this administrator?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    // Keep at least one administrator account
                    string countQuery = "SELECT COUNT(*) FROM admins";

                    using (MySqlCommand countCmd =
                        new MySqlCommand(countQuery, con))
                    {
                        int adminCount =
                            Convert.ToInt32(countCmd.ExecuteScalar());

                        if (adminCount <= 1)
                        {
                            MessageBox.Show(
                                "You cannot delete the only administrator account.");
                            return;
                        }
                    }

                    string query =
                        "DELETE FROM admins WHERE AdminID = @AdminID";

                    using (MySqlCommand cmd =
                        new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@AdminID", adminID);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show(
                                "Administrator deleted successfully!",
                                "Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Administrator not found.");
                        }
                    }
                }

                LoadAdmins();

                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox4.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error deleting administrator:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();

            dataGridView1.ClearSelection();

            textBox2.Focus();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
