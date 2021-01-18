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
        public Main()
        {
            try
            {
                InitializeComponent();
                string tBranchIP = ConfigurationManager.AppSettings[ConstantResources.cBRANCH_IP];
                string tCounterId = ConfigurationManager.AppSettings[ConstantResources.cCOUNTER_ID];
                mLoggedIn = false;
                mCounterClient = new CounterClient(tBranchIP, tCounterId);
                mCounterClient.UpdateWindowInfoEvent += mCounterClient_UpdateWindowInfoEvent;
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(ConstantResources.cERROR_TITLE_GENERAL, ConstantResources.cERROR_MESSAGE_GENERAL);
            }
        }
        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                labelStatus.Text = ConstantResources.cEMPLOYEE_STATUS_NOT_READY;
                if (!mLoggedIn)
                    using (Login tLogin = new Login(mCounterClient))
                    {
                        tLogin.ShowDialog(this);
                        if (tLogin.DialogResult == DialogResult.OK && tLogin.mSuccessfulLogin == true)
                            mLoggedIn = true;
                        else
                            Application.Exit();
                    }
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(ConstantResources.cERROR_TITLE_GENERAL, ConstantResources.cERROR_MESSAGE_GENERAL);
            }

        }
        private void mCounterClient_UpdateWindowInfoEvent(clsWindowMonitor tWindow)
        {
            try
            {
                if (tWindow != null)
                {
                    StringBuilder tStatusTextBuilder = new StringBuilder();
                    switch (tWindow.State)
                    {
                        case mdlGeneral.EmployeeActiontypes.NotReady:
                            tStatusTextBuilder.AppendLine(ConstantResources.cEMPLOYEE_STATUS_NOT_READY);
                            break;
                        case mdlGeneral.EmployeeActiontypes.Ready:
                            tStatusTextBuilder.AppendLine(ConstantResources.cEMPLOYEE_STATUS_READY);
                            break;
                        case mdlGeneral.EmployeeActiontypes.Serving:
                            tStatusTextBuilder.AppendLine(ConstantResources.cEMPLOYEE_STATUS_SERVING);
                            if (tWindow.CustomerTransaction != null)
                            {
                                tStatusTextBuilder.AppendLine(ConstantResources.cTICKET_NUMBER);
                                tStatusTextBuilder.AppendLine(tWindow.CustomerTransaction.DisplayTicketNumber);
                            }
                            break;
                    }
                    ChangeStatus(tStatusTextBuilder.ToString());
                }
            }
            catch (Exception pError)
            {

                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(ConstantResources.cERROR_TITLE_GENERAL, ConstantResources.cERROR_MESSAGE_GENERAL);
            }
        }
        private void buttonNext_Click(object sender, EventArgs e)
        {
            try
            {
                mCounterClient.Next();
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                ShowErrorMessage(ConstantResources.cERROR_TITLE_GENERAL, ConstantResources.cERROR_MESSAGE_GENERAL);
            }
        }
        private void ChangeStatus(string pStatusText)
        {
            try
            {
                if (labelStatus.InvokeRequired)
                {
                    labelStatus.Invoke(new ThreadSafeChangeStatus(ChangeStatus), pStatusText);
                }
                else
                {
                    labelStatus.Text = pStatusText;
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
