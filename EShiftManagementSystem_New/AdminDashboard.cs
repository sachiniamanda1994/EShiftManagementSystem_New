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
    public partial class AdminDashboard : Form
    {
        public AdminDashboard()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AdminCustomerManagement acform = new AdminCustomerManagement();
            acform.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AdminManagement adform = new AdminManagement();
            adform.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ProductForm pform = new ProductForm();
            pform.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TransportJobForm tform = new TransportJobForm();
            tform.Show();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ReportsForm rform = new ReportsForm();
            rform.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?","Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Login login = Application.OpenForms["Form1"] as Login;

                if (login != null)
                {
                    login.ClearLoginFields();
                    login.Show();
                }

                this.Close();
            }
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            LoadManagement loadForm = new LoadManagement();
            loadForm.Show();
        }

        private void AdminDashboard_Load(object sender, EventArgs e)
        {

        }
    }
    
}
