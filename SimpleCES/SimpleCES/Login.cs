using INFEventLogger;
using INFQueuingCOMEntities;
using QSClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleCES
{

    public partial class Login : Form
    {
        private static string cERROR_TITLE_GENERAL = "Unexpected error";
        private static string cERROR_MESSAGE_GENERAL = "An unexpected error occurred";
        private CounterClient mCounterClient;
        internal bool mSuccessfulLogin;
        public Login(CounterClient pCounterClient)
        {
            try
            {
                InitializeComponent();
                mCounterClient = pCounterClient;
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(ConstantResources.cERROR_TITLE_GENERAL, ConstantResources.cERROR_MESSAGE_GENERAL);
            }

        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string pLoginName = textBoxLoginName.Text;
                string pPassword = textBoxPassword.Text;

                if (string.IsNullOrWhiteSpace(pLoginName))
                {
                    ShowErrorMessage(ConstantResources.cERROR_TITLE_REQUIRED_LOGIN_NAME, ConstantResources.cERROR_MESSAGE_REQUIRED_LOGIN_NAME);
                    return;
                }
                if (string.IsNullOrWhiteSpace(pPassword))
                {
                    ShowErrorMessage(ConstantResources.cERROR_TITLE_REQUIRED_PASSWORD, ConstantResources.cERROR_MESSAGE_REQUIRED_PASSWORD);
                    return;
                }

                if (mCounterClient.Login(pLoginName, pPassword) == 0)
                {
                    mSuccessfulLogin = true;
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    ShowErrorMessage(ConstantResources.cERROR_TITLE_LOGIN_FAILED, ConstantResources.cERROR_MESSAGE_LOGIN_FAILED);
                }

            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(ConstantResources.cERROR_TITLE_GENERAL, ConstantResources.cERROR_MESSAGE_GENERAL);
            }
        }

        private void ShowErrorMessage(string pTitle, string pMessage)
        {
            try
            {
                MessageBox.Show(pMessage, pTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                MessageBox.Show(cERROR_MESSAGE_GENERAL, cERROR_TITLE_GENERAL, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
