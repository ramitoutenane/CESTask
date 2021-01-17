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
    public partial class Main : Form
    {
        private bool mLogedin;
        private CounterClient mCounterClient;
        public Main()
        {
            try
            {
                InitializeComponent();
                mLogedin = false;
                mCounterClient = new CounterClient("localhost");
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                labelStatus.Text = ConstantResources.cEMPLOYEE_STATUS_NOT_READY;
                labelStatus.Width = Width;
                if (!mLogedin)
                    using (Login tLogin = new Login(mCounterClient))
                    {
                        tLogin.ShowDialog(this);
                        if (tLogin.DialogResult == DialogResult.OK && tLogin.mSuccessfulLogin == true)
                            mLogedin = true;
                        else
                            Application.Exit();
                    }
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
