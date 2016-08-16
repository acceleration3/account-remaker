using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AccountRemaker
{
    public partial class Main : Form
    {
        public List<ChatangoAccount> accountList = new List<ChatangoAccount>();
        private bool started = false;

        public Main()
        {
            InitializeComponent();
        }

        private void btnShowMore_Click(object sender, EventArgs e)
        {
            if(btnShowMore.Text == ">")
            {
                btnShowMore.Text = "<";
                this.Width = 742;
            }
            else
            {
                btnShowMore.Text = ">";
                this.Width = 477;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            accountGrid.Columns[0].DataPropertyName = "username";
            accountGrid.Columns[1].DataPropertyName = "password";
            accountGrid.Columns[2].DataPropertyName = "email";
            accountGrid.Columns[3].DataPropertyName = "status";
        }

        public void UpdateList()
        {
            this.Invoke(new MethodInvoker(() =>
            {
                accountGrid.DataSource = null;
                accountGrid.DataSource = accountList;
            }));
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            if(tbUsername.Text == "" || tbPassword.Text == "" || tbEmail.Text == "")
            {
                MessageBox.Show("Please fill in all the account details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ChatangoAccount newAccount = new ChatangoAccount(tbUsername.Text, tbPassword.Text, tbEmail.Text);
            accountList.Add(newAccount);

            accountGrid.DataSource = null;
            accountGrid.DataSource = accountList;

            if (started)
                newAccount.startChecking(this);
            
            tbUsername.Text = tbPassword.Text = tbEmail.Text = "";

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if(!started)
            {
                btnStart.Text = "Stop";

                foreach (ChatangoAccount acc in accountList)
                    acc.startChecking(this);

                started = true;
            }
            else
            {
                btnStart.Text = "Start";

                foreach (ChatangoAccount acc in accountList)
                    acc.stopChecking();

                started = false;
            }
        }

        private void accountGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex != -1)
            { 
                e.Graphics.FillRectangle(Brushes.Black, e.CellBounds);
                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
            else if(e.ColumnIndex == 3 && e.FormattedValue.ToString() == "Success!")
            {
                e.Graphics.FillRectangle(Brushes.DarkGreen, e.CellBounds);
                e.PaintContent(e.CellBounds);
                e.CellStyle.ForeColor = Color.White;
                e.Handled = true;
            }
        }
    }
}
