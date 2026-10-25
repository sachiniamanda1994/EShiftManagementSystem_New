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
    public partial class CustomerDashboard : Form
    {
        private readonly int _customerId;
        public CustomerDashboard(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            button4.Enabled = false;
        }

        private void CustomerDashboard_Load(object sender, EventArgs e)
        {
            LoadCustomerDetails(); 
        }

        private void LoadCustomerDetails()
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string sql =
                        "SELECT CustomerID, FullName, Address, Phone, Email " +
                        "FROM customers WHERE CustomerID = @id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@id", _customerId);

                        using (MySqlDataAdapter adapter =
                            new MySqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);

                            dataGridView1.DataSource = table;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading your details: " + ex.Message);
            }
        }







        private void button1_Click(object sender, EventArgs e)
        {

        }







        //..................................VIEW ALL............................
        private void button2_Click(object sender, EventArgs e)
        {
            LoadCustomerDetails();
            textBox1.Clear();
        }
     /*       try
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

                textBox1.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message);
            }
        } */






        //......................SEARCH..........................................

        private void button1_Click_1(object sender, EventArgs e)
        {
            string keyword = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadCustomerDetails();
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
                        "WHERE CustomerID = @id " +
                        "AND (FullName LIKE @search OR Email LIKE @search)";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@id", _customerId);
                        cmd.Parameters.AddWithValue(
                            "@search", "%" + keyword + "%");

                        using (MySqlDataAdapter adapter =
                            new MySqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);

                            dataGridView1.DataSource = table;

                            if (table.Rows.Count == 0)
                            {
                                MessageBox.Show(
                                    "No matching details found in your account.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message);
            }
        }


        //...........................ALL VIEW IN GRID.....................................
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            {
                if (e.RowIndex < 0)
                    return;

                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                textBox2.Text = Convert.ToString(row.Cells["CustomerID"].Value);
                //row.Cells["CustomerID"].Value.ToString();

                textBox3.Text = Convert.ToString(row.Cells["FullName"].Value);
                //row.Cells["FullName"].Value.ToString();

                textBox4.Text = Convert.ToString(row.Cells["Address"].Value);
                //row.Cells["Address"].Value.ToString();

                textBox5.Text = Convert.ToString(row.Cells["Phone"].Value);
                //row.Cells["Phone"].Value.ToString();

                textBox6.Text = Convert.ToString(row.Cells["Email"].Value);
                //row.Cells["Email"].Value.ToString();
            }
        }







        //..........................UPDATE....................................

        private void button3_Click(object sender, EventArgs e)
        {
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

                        cmd.Parameters.AddWithValue("@id", _customerId);


                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Customer updated successfully!");

                            // Reload the customer list
                            string selectSql =
                                "SELECT CustomerID, FullName, Address, Phone, Email " +
                                "FROM customers WHERE CustomerID = @id\"";

                            using (MySqlCommand selectCmd =
                        new MySqlCommand(selectSql, con))
                            {
                                selectCmd.Parameters.AddWithValue(
                                    "@id", _customerId);

                                using (MySqlDataAdapter adapter =
                                   new MySqlDataAdapter(selectCmd))
                                {
                                    DataTable table = new DataTable();
                                    adapter.Fill(table);
                                    dataGridView1.DataSource = table;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                "No rows were changed. Check that the customer ID exists " +
                                "and that you changed at least one value.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed: " + ex.Message);
            }
        }






        //.......................CLEAR......................................
        private void ClearCustomerFields()
        {
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();

            dataGridView1.ClearSelection();
        }





        //.....................DELEAT.......................................
        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ClearCustomerFields();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                RequestTransport requestForm = new RequestTransport(_customerId);
                requestForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to open the transport request form.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }









        //private void button1_Click(object sender, EventArgs e)


    }
    
}
