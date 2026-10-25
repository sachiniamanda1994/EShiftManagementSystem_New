using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EShiftManagementSystem_New
{
    public partial class AdminCustomerManagement : Form
    {
        public AdminCustomerManagement()
        {
            InitializeComponent();
        }

        private void AdminCustomerManagement_Load(object sender, EventArgs e)
        {
            LoadAllCustomers();
        }

        private void LoadAllCustomers()
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string sql =
                        "SELECT CustomerID, FullName, Address, Phone, Email " +
                        "FROM customers";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(sql, con))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        dataGridView1.DataSource = table;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message);
            }
        }

        // SEARCH CUSTOMERS
        private void button1_Click(object sender, EventArgs e)
        {
            string keyword = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Enter a customer ID, name, or email.");
                textBox1.Focus();
                return;
            }

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string sql =
                        "SELECT CustomerID, FullName, Address, Phone, Email " +
                        "FROM customers " +
                        "WHERE CAST(CustomerID AS CHAR) LIKE @search " +
                        "OR FullName LIKE @search " +
                        "OR Email LIKE @search";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@search", "%" + keyword + "%");

                        using (MySqlDataAdapter adapter =
                            new MySqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            dataGridView1.DataSource = table;

                            if (table.Rows.Count == 0)
                                MessageBox.Show("No matching customers found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message);
            }
        }

        // VIEW ALL CUSTOMERS
        private void button2_Click(object sender, EventArgs e)
        {
            LoadAllCustomers();
            textBox1.Clear();
        }

        // SELECT A CUSTOMER FROM THE GRID
        private void dataGridView1_CellContentClick(
            object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            textBox2.Text =
                Convert.ToString(row.Cells["CustomerID"].Value);
            textBox3.Text =
                Convert.ToString(row.Cells["FullName"].Value);
            textBox4.Text =
                Convert.ToString(row.Cells["Address"].Value);
            textBox5.Text =
                Convert.ToString(row.Cells["Phone"].Value);
            textBox6.Text =
                Convert.ToString(row.Cells["Email"].Value);
        }

        // UPDATE SELECTED CUSTOMER
        private void button3_Click(object sender, EventArgs e)
        {
            int customerId;

            if (!int.TryParse(textBox2.Text, out customerId))
            {
                MessageBox.Show("Please select a customer first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox3.Text) ||
                string.IsNullOrWhiteSpace(textBox4.Text) ||
                string.IsNullOrWhiteSpace(textBox5.Text) ||
                string.IsNullOrWhiteSpace(textBox6.Text))
            {
                MessageBox.Show("Please fill in all customer details.");
                return;
            }

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string sql =
                        "UPDATE customers SET FullName=@name, " +
                        "Address=@address, Phone=@phone, Email=@email " +
                        "WHERE CustomerID=@id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@name", textBox3.Text.Trim());
                        cmd.Parameters.AddWithValue(
                            "@address", textBox4.Text.Trim());
                        cmd.Parameters.AddWithValue(
                            "@phone", textBox5.Text.Trim());
                        cmd.Parameters.AddWithValue(
                            "@email", textBox6.Text.Trim());
                        cmd.Parameters.AddWithValue("@id", customerId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Customer updated successfully!");
                            LoadAllCustomers();
                        }
                        else
                        {
                            MessageBox.Show(
                                "No details changed, or the customer was not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed: " + ex.Message);
            }
        }

        // DELETE SELECTED CUSTOMER
        private void button4_Click(object sender, EventArgs e)
        {
            int customerId;

            if (!int.TryParse(textBox2.Text, out customerId))
            {
                MessageBox.Show("Please select a customer first.");
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Are you sure you want to delete this customer?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
                return;

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string sql =
                        "DELETE FROM customers WHERE CustomerID=@id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@id", customerId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Customer deleted successfully!");
                            ClearCustomerFields();
                            LoadAllCustomers();
                        }
                        else
                        {
                            MessageBox.Show("Customer record was not found.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    MessageBox.Show(
                        "This customer has related records. " +
                        "Deletion is restricted to protect job history.");
                }
                else
                {
                    MessageBox.Show("Database error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete error: " + ex.Message);
            }
        }

        // CLEAR FIELDS
        private void ClearCustomerFields()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            dataGridView1.ClearSelection();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ClearCustomerFields();
        }

        // CLOSE FORM
        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
            AdminDashboard ad_dashboard = new AdminDashboard();
            ad_dashboard.Show();
        }
    }
}
    

