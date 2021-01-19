using INFEventLogger;
using INFQueuingCOMEntities;
using QSClient;
using System;
using System.Configuration;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace SimpleCES
{
    public partial class Main : Form
    {
        private const string cERROR_TITLE_GENERAL = "Unexpected error";
        private const string cERROR_MESSAGE_GENERAL = "An unexpected error occurred";
        private delegate void ThreadSafeChangeStatus(string pStatusText);
        private bool mLoggedIn;
        private CounterClient mCounterClient;
        /// <summary>
        /// Main form object constructor
        /// </summary>
        public Main()
        {
            try
            {
                InitializeComponent();
                //get application settings from configuration file
                string tBranchIP = ConfigurationManager.AppSettings[mdlGeneral.cBRANCH_IP];
                string tCounterId = ConfigurationManager.AppSettings[mdlGeneral.cCOUNTER_ID];

                mLoggedIn = false;
                mCounterClient = new CounterClient(tBranchIP, tCounterId);
                mCounterClient.UpdateWindowInfoEvent += UpdateWindowInfoEvent;
            }
            catch (Exception pError)
            {
                INFQueuingCOMEntities.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(mdlGeneral.cERROR_TITLE_GENERAL, mdlGeneral.cERROR_MESSAGE_GENERAL);
            }
        }
        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                labelStatus.Text = mdlGeneral.cEMPLOYEE_STATUS_NOT_READY;
                //show login form dialog to user
                if (!mLoggedIn)
                    using (Login tLogin = new Login(mCounterClient))
                    {
                        tLogin.ShowDialog(this);
                        //if logged in successfully allow to use system, exit otherwise
                        if (tLogin.DialogResult == DialogResult.OK && tLogin.mSuccessfulLogin == true)
                            mLoggedIn = true;
                        else
                            Application.Exit();
                    }
            }
            catch (Exception pError)
            {
                INFQueuingCOMEntities.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(mdlGeneral.cERROR_TITLE_GENERAL, mdlGeneral.cERROR_MESSAGE_GENERAL);
            }

        }
        /// <summary>
        /// Updated window monitor object event handler
        /// </summary>
        /// <param name="tWindow">Window monitor object containing current window info</param>
        private void UpdateWindowInfoEvent(clsWindowMonitor tWindow)
        {
            try
            {
                if (tWindow != null)
                {
                    //show counter status according to window status
                    StringBuilder tStatusTextBuilder = new StringBuilder();
                    switch (tWindow.State)
                    {
                        case INFQueuingCOMEntities.mdlGeneral.EmployeeActiontypes.NotReady:
                            tStatusTextBuilder.AppendLine(mdlGeneral.cEMPLOYEE_STATUS_NOT_READY);
                            break;
                        case INFQueuingCOMEntities.mdlGeneral.EmployeeActiontypes.Ready:
                            tStatusTextBuilder.AppendLine(mdlGeneral.cEMPLOYEE_STATUS_READY);
                            break;
                        case INFQueuingCOMEntities.mdlGeneral.EmployeeActiontypes.Serving:
                            tStatusTextBuilder.AppendLine(mdlGeneral.cEMPLOYEE_STATUS_SERVING);
                            if (tWindow.CustomerTransaction != null)
                            {
                                tStatusTextBuilder.AppendLine(mdlGeneral.cTICKET_NUMBER);
                                tStatusTextBuilder.AppendLine(tWindow.CustomerTransaction.DisplayTicketNumber);
                            }
                            break;
                    }
                    ChangeStatus(tStatusTextBuilder.ToString());
                }
            }
            catch (Exception pError)
            {

                INFQueuingCOMEntities.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(mdlGeneral.cERROR_TITLE_GENERAL, mdlGeneral.cERROR_MESSAGE_GENERAL);
            }
        }
        /// <summary>
        /// Next button click event handler
        /// </summary>
        private void buttonNext_Click(object sender, EventArgs e)
        {
            try
            {
                mCounterClient.Next();
            }
            catch (Exception pError)
            {
                INFQueuingCOMEntities.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(mdlGeneral.cERROR_TITLE_GENERAL, mdlGeneral.cERROR_MESSAGE_GENERAL);
            }
        }
        /// <summary>
        /// Change counter status label to given  text
        /// </summary>
        /// <param name="pStatusText">Text to change status label into</param>
        private void ChangeStatus(string pStatusText)
        {
            try
            {
                //check if method is called from thread that require invoking delegate
                if (labelStatus.InvokeRequired)
                {
                    //call method using thread safe delegate to change status
                    labelStatus.Invoke(new ThreadSafeChangeStatus(ChangeStatus), pStatusText);
                }
                else
                {
                    labelStatus.Text = pStatusText;
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
