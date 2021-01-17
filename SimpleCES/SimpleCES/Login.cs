using INFQueuingCOMEntities;
using QSClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleCES
{

    public partial class Login : Form
    {
        private CounterClient mCounterClient;
        internal bool mSuccessfulLogin;
        public Login(CounterClient pCounterClient)
        {
            InitializeComponent();
            mCounterClient = pCounterClient;
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string pLoginName = textBoxLoginName.Text;
                string pPassword = textBoxPassword.Text;
                if (String.IsNullOrWhiteSpace(pLoginName) || String.IsNullOrWhiteSpace(pPassword))
                    MessageBox.Show("Empty", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (mCounterClient.Login("119", pLoginName, pPassword) == 0)
                {
                    mSuccessfulLogin = true;
                    DialogResult =DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
