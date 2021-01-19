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
        private const string cERROR_TITLE_GENERAL = "Unexpected error";
        private const string cERROR_MESSAGE_GENERAL = "An unexpected error occurred";
        private CounterClient mCounterClient;
        //indicate wither login is successful or not
        internal bool mSuccessfulLogin;
        /// <summary>
        /// Login form object constructor
        /// </summary>
        /// <param name="pCounterClient"> Counter client object to be used in login</param>
        public Login(CounterClient pCounterClient)
        {
            try
            {
                InitializeComponent();
                mCounterClient = pCounterClient;
            }
            catch (Exception pError)
            {
                INFQueuingCOMEntities.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(mdlGeneral.cERROR_TITLE_GENERAL, mdlGeneral.cERROR_MESSAGE_GENERAL);
            }

        }
        /// <summary>
        /// Login button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLogin_Click(object sender, EventArgs e)
        {
            try
            {
                //get date from text boxes and check validity
                string pLoginName = textBoxLoginName.Text;
                string pPassword = textBoxPassword.Text;

                if (string.IsNullOrWhiteSpace(pLoginName))
                {
                    ShowErrorMessage(mdlGeneral.cERROR_TITLE_REQUIRED_LOGIN_NAME, mdlGeneral.cERROR_MESSAGE_REQUIRED_LOGIN_NAME);
                    return;
                }
                if (string.IsNullOrWhiteSpace(pPassword))
                {
                    ShowErrorMessage(mdlGeneral.cERROR_TITLE_REQUIRED_PASSWORD, mdlGeneral.cERROR_MESSAGE_REQUIRED_PASSWORD);
                    return;
                }

                //try to perform login
                if (mCounterClient.Login(pLoginName, pPassword) == 0)
                {
                    //if successful result returned change login status and submit dialog
                    mSuccessfulLogin = true;
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    //if login failed, show appropriate message
                    ShowErrorMessage(mdlGeneral.cERROR_TITLE_LOGIN_FAILED, mdlGeneral.cERROR_MESSAGE_LOGIN_FAILED);
                }

            }
            catch (Exception pError)
            {
                INFQueuingCOMEntities.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(mdlGeneral.cERROR_TITLE_GENERAL, mdlGeneral.cERROR_MESSAGE_GENERAL);
            }
        }
        /// <summary>
        /// Show error message dialog to user
        /// </summary>
        /// <param name="pTitle">Error dialog caption title</param>
        /// <param name="pMessage">Error dialog message content</param>
        private void ShowErrorMessage(string pTitle, string pMessage)
        {
            try
            {
                MessageBox.Show(pMessage, pTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception pError)
            {
                INFQueuingCOMEntities.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                MessageBox.Show(cERROR_MESSAGE_GENERAL, cERROR_TITLE_GENERAL, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
