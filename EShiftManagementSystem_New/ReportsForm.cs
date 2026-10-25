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
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EShiftManagementSystem_New
{
    public partial class ReportsForm : Form
    {
        public ReportsForm()
        {
            InitializeComponent();
        }

        private void ReportsForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
    SELECT
        COUNT(*) AS TotalJobs,
        SUM(CASE
            WHEN JobStatus = 'Pending'
            THEN 1 ELSE 0
        END) AS PendingJobs,
        SUM(CASE
            WHEN JobStatus = 'Approved'
            THEN 1 ELSE 0
        END) AS ApprovedJobs,
        SUM(CASE
            WHEN JobStatus = 'Rejected'
            THEN 1 ELSE 0
        END) AS RejectedJobs
    FROM jobs";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dataGridView1.DataSource = dt;

                        dataGridView1.AutoSizeColumnsMode =
                            DataGridViewAutoSizeColumnsMode.Fill;

                        dataGridView1.ReadOnly = true;
                        dataGridView1.AllowUserToAddRows = false;
                        dataGridView1.AutoGenerateColumns = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error generating Job Summary Report:\n" + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT
                    j.JobID,
                    c.FullName AS CustomerName,
                    j.PickupLocation,
                    j.Destination,
                    j.RequestedDate,
                    j.JobStatus,
                    t.LorryNumber,
                    t.DriverName,
                    t.AssistantName,
                    t.ContainerNumber
                FROM jobs j
                INNER JOIN customers c
                    ON j.CustomerID = c.CustomerID
                LEFT JOIN transport_units t
                    ON j.UnitID = t.UnitID
                ORDER BY j.JobID DESC";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dataGridView1.DataSource = dt;

                        dataGridView1.AutoSizeColumnsMode =
                            DataGridViewAutoSizeColumnsMode.AllCells;

                        dataGridView1.ReadOnly = true;
                        dataGridView1.AllowUserToAddRows = false;
                        dataGridView1.AutoGenerateColumns = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading Transport Job Summary:\n" + ex.Message,
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT
                    UnitID,
                    LorryNumber,
                    DriverName,
                    AssistantName,
                    ContainerNumber,
                    UnitStatus
                FROM transport_units
                ORDER BY UnitID ASC";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dataGridView1.DataSource = dt;

                        dataGridView1.AutoSizeColumnsMode =
                            DataGridViewAutoSizeColumnsMode.Fill;

                        dataGridView1.ReadOnly = true;
                        dataGridView1.AllowUserToAddRows = false;
                        dataGridView1.AutoGenerateColumns = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading Transport Availability Report:\n"
                    + ex.Message,
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0 ||
        (dataGridView1.Rows.Count == 1 &&
         dataGridView1.Rows[0].IsNewRow))
            {
                MessageBox.Show("Please generate a report before exporting.");
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveDialog.Title = "Save Report as PDF";
                saveDialog.FileName = "E-Shift_Report.pdf";

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    using (FileStream stream = new FileStream(
                        saveDialog.FileName, FileMode.Create))
                    {
                        Document document = new Document(
                            PageSize.A4.Rotate(), 20, 20, 20, 20);

                        PdfWriter.GetInstance(document, stream);
                        document.Open();

                        Paragraph title = new Paragraph(
                            "E-Shift Management Report",
                            FontFactory.GetFont(
                                FontFactory.HELVETICA_BOLD, 16));

                        title.Alignment = Element.ALIGN_CENTER;
                        document.Add(title);

                        document.Add(new Paragraph(
                            "Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm")));

                        document.Add(new Paragraph(" "));

                        PdfPTable pdfTable = new PdfPTable(
                            dataGridView1.Columns.Count);

                        pdfTable.WidthPercentage = 100;

                        // Add column headings
                        foreach (DataGridViewColumn column in dataGridView1.Columns)
                        {
                            if (column.Visible)
                            {
                                PdfPCell cell = new PdfPCell(
                                    new Phrase(column.HeaderText));

                                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                                pdfTable.AddCell(cell);
                            }
                        }

                        // Add report data
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                foreach (DataGridViewColumn column in dataGridView1.Columns)
                                {
                                    if (column.Visible)
                                    {
                                        string value = row.Cells[column.Index].Value == null
                                            ? ""
                                            : row.Cells[column.Index].Value.ToString();

                                        pdfTable.AddCell(new Phrase(value));
                                    }
                                }
                            }
                        }

                        document.Add(pdfTable);
                        document.Close();
                    }

                    MessageBox.Show(
                        "Report exported successfully!",
                        "PDF Export",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Error exporting PDF:\n" + ex.Message,
                        "PDF Export Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT
                    CustomerID,
                    FullName,
                    Email,
                    Phone,
                    Address
                FROM customers
                ORDER BY CustomerID ASC";

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
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customer report: " + ex.Message);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT
                    AdminID,
                    FullName,
                    Email
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
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading admin report: " + ex.Message);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseConnection db = new DatabaseConnection();

                using (MySqlConnection con = db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT
                    l.LoadID,
                    l.JobID,
                    c.FullName AS CustomerName,
                    j.PickupLocation,
                    j.Destination,
                    l.LoadDescription,
                    l.Weight,
                    l.LoadStatus
                FROM loads l
                INNER JOIN jobs j
                    ON l.JobID = j.JobID
                INNER JOIN customers c
                    ON j.CustomerID = c.CustomerID
                ORDER BY l.LoadID DESC";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dataGridView1.DataSource = dt;
                        dataGridView1.ReadOnly = true;
                        dataGridView1.AllowUserToAddRows = false;
                        dataGridView1.AutoSizeColumnsMode =
                            DataGridViewAutoSizeColumnsMode.DisplayedCells;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading load report: " + ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
    }
}
