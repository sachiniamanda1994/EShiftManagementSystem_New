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


namespace EShiftManagementSystem_New
{
    public partial class TransportJobForm : Form
    {
        private DatabaseConnection db = new DatabaseConnection();
        public TransportJobForm()
        {
            InitializeComponent();
        }

        private void TransportJobForm_Load(object sender, EventArgs e)
        {
            LoadJobs();
            LoadAvailableUnits();

        }


        private void LoadJobs()
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT JobID, CustomerID, PickupLocation,
                       Destination, RequestedDate, JobStatus, UnitID
                FROM jobs
                ORDER BY JobID DESC";

                    using (MySqlDataAdapter adapter =
                           new MySqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading transport jobs: " + ex.Message);
            }
        }

        private void LoadAvailableUnits()
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT UnitID, LorryNumber
                FROM transport_units
                WHERE UnitStatus = 'Available'
                ORDER BY UnitID";

                    using (MySqlDataAdapter adapter =
                           new MySqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        comboBox1.DataSource = null;
                        comboBox1.DisplayMember = "LorryNumber";
                        comboBox1.ValueMember = "UnitID";
                        comboBox1.DataSource = dt;
                        comboBox1.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading transport units: " + ex.Message);
            }
        }

        private void DisplayTransportDetails()
        {
            if (dataGridView1.CurrentRow == null)
                return;

            object value =
                dataGridView1.CurrentRow.Cells["UnitID"].Value;

            if (value == null || value == DBNull.Value)
            {
                label2.Text = "Not assigned";
                label3.Text = "Not assigned";
                label4.Text = "Not assigned";
                label5.Text = "Not assigned";
                return;
            }

            int unitId = Convert.ToInt32(value);

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT LorryNumber, DriverName,
                       AssistantName, ContainerNumber
                FROM transport_units
                WHERE UnitID = @UnitID";

                    using (MySqlCommand cmd =
                        new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UnitID", unitId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                label2.Text =
                                    "Vehicle Number: " +
                                    reader["LorryNumber"].ToString();

                                label3.Text =
                                    "Driver: " +
                                    reader["DriverName"].ToString();

                                label4.Text =
                                    "Assistant: " +
                                    reader["AssistantName"].ToString();

                                label5.Text =
                                    "Container: " +
                                    reader["ContainerNumber"].ToString();
                            }
                            else
                            {
                                MessageBox.Show(
                                    "No transport unit found for UnitID: " +
                                    unitId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error displaying transport details: " + ex.Message);
            }
        }











        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a transport job.");
                return;
            }

            int jobId = Convert.ToInt32(
                dataGridView1.CurrentRow.Cells["JobID"].Value
            );

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT LoadID, JobID, LoadDescription,
                       Weight, LoadStatus
                FROM loads
                WHERE JobID = @JobID
                ORDER BY LoadID";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@JobID", jobId);

                        using (MySqlDataAdapter adapter =
                               new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            dataGridView1.DataSource = dt;

                            if (dt.Rows.Count == 0)
                            {
                                MessageBox.Show(
                                    "No load details found for Job ID " + jobId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading load details: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a transport job.");
                return;
            }

            int jobId = Convert.ToInt32(
                dataGridView1.CurrentRow.Cells["JobID"].Value);

            string currentStatus = Convert.ToString(
                dataGridView1.CurrentRow.Cells["JobStatus"].Value);

            if (currentStatus != "Pending")
            {
                MessageBox.Show("Only pending requests can be approved.");
                return;
            }

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query =
                        "UPDATE jobs SET JobStatus = @status " +
                        "WHERE JobID = @jobId AND JobStatus = @currentStatus";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@status", "Approved");
                        cmd.Parameters.AddWithValue("@jobId", jobId);
                        cmd.Parameters.AddWithValue("@currentStatus", "Pending");

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Transport request approved successfully.");
                            LoadJobs();
                        }
                        else
                        {
                            MessageBox.Show(
                                "No request was updated. The job may have already been processed.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Approval SQL error:\n" + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a transport job.");
                return;
            }

            int jobId = Convert.ToInt32(
                dataGridView1.CurrentRow.Cells["JobID"].Value
            );

            string currentStatus =
                Convert.ToString(
                    dataGridView1.CurrentRow.Cells["JobStatus"].Value
                );

            if (currentStatus != "Pending")
            {
                MessageBox.Show(
                    "Only pending requests can be rejected.");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to reject Job ID " + jobId + "?",
                "Confirm Rejection",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                UPDATE jobs
                SET JobStatus = @JobStatus
                WHERE JobID = @JobID
                  AND JobStatus = 'Pending'";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@JobStatus", "Rejected");

                        cmd.Parameters.AddWithValue("@JobID", jobId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Transport request rejected.");
                            LoadJobs();
                        }
                        else
                        {
                            MessageBox.Show(
                                "The request was already processed. Please refresh the job list.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rejection failed: " + ex.Message);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null ||
        dataGridView1.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Please select a transport job.");
                return;
            }

            // Step 2: Read the selected job details
            int jobId = Convert.ToInt32(
                dataGridView1.CurrentRow.Cells["JobID"].Value);

            string jobStatus = Convert.ToString(
                dataGridView1.CurrentRow.Cells["JobStatus"].Value);

            // Step 3: Only approved jobs can be assigned
            if (jobStatus != "Approved")
            {
                MessageBox.Show(
                    "Please select an approved job before assigning a transport unit.");
                return;
            }

            // Step 4: Check whether a transport unit is selected
            if (comboBox1.SelectedIndex == -1 ||
                comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Please select an available transport unit.");
                return;
            }

            int unitId = Convert.ToInt32(
                comboBox1.SelectedValue);

            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    using (MySqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Step 5: Mark the selected unit as Assigned
                            string unitQuery = @"
                        UPDATE transport_units
                        SET UnitStatus = 'Assigned'
                        WHERE UnitID = @UnitID
                        AND UnitStatus = 'Available'";

                            int unitRows;

                            using (MySqlCommand cmd =
                                new MySqlCommand(unitQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UnitID", unitId);
                                unitRows = cmd.ExecuteNonQuery();
                            }

                            if (unitRows != 1)
                            {
                                throw new Exception(
                                    "This transport unit is no longer available. Refresh the form and try again.");
                            }

                            // Step 6: Assign the unit to the selected job
                            string jobQuery = @"
                        UPDATE jobs
                        SET UnitID = @UnitID
                        WHERE JobID = @JobID
                        AND JobStatus = 'Approved'
                        AND UnitID IS NULL";

                            int jobRows;

                            using (MySqlCommand cmd =
                                new MySqlCommand(jobQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UnitID", unitId);
                                cmd.Parameters.AddWithValue("@JobID", jobId);

                                jobRows = cmd.ExecuteNonQuery();
                            }

                            if (jobRows != 1)
                            {
                                throw new Exception(
                                    "The job could not be assigned. It may already have a transport unit.");
                            }

                            // Step 7: Save both updates
                            transaction.Commit();

                            MessageBox.Show(
                                "Transport unit assigned successfully!");

                            // Step 8: Refresh the job list and available units
                            LoadJobs();
                            LoadAvailableUnits();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error assigning transport unit:\n" + ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DisplayTransportDetails();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadJobs();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();

            foreach (Form form in Application.OpenForms)
            {
                if (form is AdminDashboard)
                {
                    form.Show();
                    break;
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a job first.");
                return;
            }

            int jobId = Convert.ToInt32(
                dataGridView1.CurrentRow.Cells["JobID"].Value);

            string jobStatus = Convert.ToString(
                dataGridView1.CurrentRow.Cells["JobStatus"].Value);

            if (jobStatus != "Approved")
            {
                MessageBox.Show(
                    "Only approved jobs can be marked as completed.");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Mark this job as completed and make its transport unit available?",
                "Complete Job",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            using (MySqlConnection conn = db.GetConnection())
            {
                conn.Open();

                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int unitId;

                        // Find the transport unit assigned to this job.
                        string getUnitQuery =
                            "SELECT UnitID FROM jobs " +
                            "WHERE JobID = @JobID FOR UPDATE";

                        using (MySqlCommand cmd = new MySqlCommand(
                            getUnitQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JobID", jobId);

                            object value = cmd.ExecuteScalar();

                            if (value == null || value == DBNull.Value)
                            {
                                throw new Exception(
                                    "No transport unit is assigned to this job.");
                            }

                            unitId = Convert.ToInt32(value);
                        }

                        // Mark the job as completed.
                        string updateJobQuery =
                            "UPDATE jobs SET JobStatus = 'Completed' " +
                            "WHERE JobID = @JobID AND JobStatus = 'Approved'";

                        using (MySqlCommand cmd = new MySqlCommand(
                            updateJobQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@JobID", jobId);

                            if (cmd.ExecuteNonQuery() != 1)
                            {
                                throw new Exception(
                                    "The job could not be marked as completed.");
                            }
                        }

                        // Make the assigned transport unit available again.
                        string updateUnitQuery =
                            "UPDATE transport_units " +
                            "SET UnitStatus = 'Available' " +
                            "WHERE UnitID = @UnitID AND UnitStatus = 'Assigned'";

                        using (MySqlCommand cmd = new MySqlCommand(
                            updateUnitQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UnitID", unitId);

                            if (cmd.ExecuteNonQuery() != 1)
                            {
                                throw new Exception(
                                    "The assigned transport unit was not found " +
                                    "or is not marked as Assigned.");
                            }
                        }

                        transaction.Commit();

                        MessageBox.Show(
                            "Job completed successfully. The transport unit " +
                            "is now available.",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        LoadJobs();
                        LoadAvailableUnits();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            // The transaction may already have ended.
                        }

                        MessageBox.Show(
                            "Could not complete the job: " + ex.Message,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
