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
    public partial class RequestTransport : Form
    {
        private readonly int _customerId;
        private DataTable loadTable;
        public RequestTransport(int customerId)

        {
            InitializeComponent();
            _customerId = customerId;
            CreateLoadTable();
        }

        private void RequestTransport_Load(object sender, EventArgs e)
        {
            dateTimePicker1.MinDate = DateTime.Today;
            dateTimePicker1.Value = DateTime.Today;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pickup = textBox1.Text.Trim();
            string destination = textBox2.Text.Trim();

            if (pickup == "" || destination == "")
            {
                MessageBox.Show("Please enter pickup and destination.");
                return;
            }

            if (loadTable == null || loadTable.Rows.Count == 0)
            {
                MessageBox.Show("Please add at least one load.");
                return;
            }

            DatabaseConnection db = new DatabaseConnection();

            using (MySqlConnection con = db.GetConnection())
            {
                con.Open();

                // Use a transaction so the job and its loads
                // are saved together.
                using (MySqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // 1. Save the transport job.
                        string jobSql =
                            @"INSERT INTO jobs
                    (CustomerID, PickupLocation, Destination,
                     RequestedDate, JobStatus, UnitID)
                    VALUES
                    (@CustomerID, @PickupLocation, @Destination,
                     @RequestedDate, @JobStatus, NULL)";

                        int jobId;

                        using (MySqlCommand cmd =
                            new MySqlCommand(jobSql, con, transaction))
                        {
                            cmd.Parameters.AddWithValue(
                                "@CustomerID", _customerId);

                            cmd.Parameters.AddWithValue(
                                "@PickupLocation", pickup);

                            cmd.Parameters.AddWithValue(
                                "@Destination", destination);

                            cmd.Parameters.AddWithValue(
                                "@RequestedDate",
                                dateTimePicker1.Value.Date);

                            cmd.Parameters.AddWithValue(
                                "@JobStatus", "Pending");

                            cmd.ExecuteNonQuery();

                            jobId = Convert.ToInt32(cmd.LastInsertedId);
                        }

                        // 2. Save every load linked to this job.
                        string loadSql =
                            @"INSERT INTO loads
                    (JobID, LoadDescription, Weight, LoadStatus)
                    VALUES
                    (@JobID, @LoadDescription, @Weight, @LoadStatus)";

                        foreach (DataRow row in loadTable.Rows)
                        {
                            using (MySqlCommand cmd =
                                new MySqlCommand(loadSql, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@JobID", jobId);

                                cmd.Parameters.AddWithValue(
                                    "@LoadDescription",
                                    row["LoadDescription"].ToString());

                                cmd.Parameters.AddWithValue(
                                    "@Weight",
                                    Convert.ToDecimal(row["Weight"]));

                                cmd.Parameters.AddWithValue(
                                    "@LoadStatus", "Pending");

                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Save everything only after all inserts succeed.
                        transaction.Commit();

                        MessageBox.Show(
                            "Transport request submitted successfully.\n" +
                            "Job ID: " + jobId +
                            "\nYour request is waiting for admin approval.");

                        loadTable.Clear();
                        textBox1.Clear();
                        textBox2.Clear();
                        dateTimePicker1.Value = DateTime.Today;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        MessageBox.Show(
                            "Could not submit the transport request.\n" +
                            ex.Message);
                    }
                }
            }

        }












        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();

        }
        private void CreateLoadTable()
        {
            loadTable = new DataTable();

            loadTable.Columns.Add("LoadDescription", typeof(string));
            loadTable.Columns.Add("Weight", typeof(decimal));

            dataGridView1.DataSource = loadTable;

            dataGridView1.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
        }


        private void LoadMyRequests()
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string sql = @" SELECT JobID, PickupLocation, Destination,RequestedDate, JobStatus, UnitID
                                FROM jobs
                                WHERE CustomerID = @CustomerID
                                ORDER BY JobID DESC";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@CustomerID", _customerId);

                        using (MySqlDataAdapter adapter =
                            new MySqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);

                            dataGridView1.DataSource = table;

                            if (table.Rows.Count == 0)
                            {
                                MessageBox.Show(
                                    "You have not submitted any transport requests yet.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading transport requests: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadMyRequests();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string description = textBox3.Text.Trim();
            decimal weight;

            if (description == "")
            {
                MessageBox.Show("Please enter the load description.");
                textBox3.Focus();
                return;
            }

            if (textBox4.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the estimated weight.");
                textBox4.Focus();
                return;
            }

            if (!decimal.TryParse(textBox4.Text.Trim(), out weight)
                || weight <= 0)
            {
                MessageBox.Show("Please enter a valid weight greater than zero.");
                textBox4.Focus();
                return;
            }

            loadTable.Rows.Add(description, weight);

            textBox3.Clear();
            textBox4.Clear();
            textBox3.Focus();
        }
    }
    }
