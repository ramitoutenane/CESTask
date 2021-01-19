using INFEventLogger;
using INFQSCommunication;
using INFQueuingCOMEntities;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace QSClient
{
    public class CounterClient
    {
        #region Delegates and Events
        public delegate void QSConnectedDelegate();
        public event QSConnectedDelegate QSConnectedEvent;

        public delegate void QSDisconnectedDelegate();
        public event QSDisconnectedDelegate QSDisconnectedEvent;

        public delegate void UpdateWindowInfoDelegate(clsWindowMonitor tWindow);
        public event UpdateWindowInfoDelegate UpdateWindowInfoEvent;

        #endregion
        #region Declarations
        private string mServerIpAddress;
        private readonly string mClientName;
        /// <summary>
        /// to be used to communicate with QS
        /// </summary>
        private CommunicationProxy mCommunicationManager;
        private bool mConnectionStatus;
        private string mCounterId;
        /// <summary>
        /// to be used to save a copy of latest counter state
        /// </summary>
        private clsWindowMonitor mWindowMonitor;
        #endregion
        #region Properties
        public bool IsConnected
        {
            get
            {
                return mConnectionStatus;
            }
        }
        public string CounterId
        {
            get
            {
                return mCounterId;
            }
        }
        public clsWindowMonitor WindowMonitor
        {
            get
            {
                return mWindowMonitor;
            }
        }
        #endregion
        #region Public Functions
        /// <summary>
        /// Counter Client Constructor 
        /// </summary>
        /// <param name="pServerIpAddress">QS IP address</param>
        /// <param name="pCounterId">Current counter client id </param>
        public CounterClient(string pServerIpAddress, string pCounterId)
        {
            try
            {
                mClientName = mdlGeneral.cCLIENT_NAME_COUNTER_CLIENT;
                mServerIpAddress = pServerIpAddress;
                mCounterId = pCounterId;
                mConnectionStatus = false;
                InitializeCommunicationManager();
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);

            }
        }
        /// <summary>
        /// Login to system with given credentials
        /// </summary>
        /// <param name="pLoginName">User login name</param>
        /// <param name="pPassword">User password</param>
        /// <returns>Login result code</returns>
        public int Login(string pLoginName, string pPassword)
        {
            try
            {
                if (IsConnected)
                {
                    //check user type and prepare required login data
                    //currently we only have one type of login
                    string pDomainName = Environment.UserDomainName;
                    string tHashedPassword = SecurityModule.clsTripleDESCrypto.Encrypt(pPassword);
                    string tLanguageID = mdlGeneral.cCOUNTER_DEFAULT_LANGUAGE_ID;

                    //login as normal employee
                    return QSEmployeeNormalLogin(pLoginName, tHashedPassword, pDomainName, tLanguageID);
                }
                else
                {
                    INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, mdlGeneral.cERROR_QS_CLIENT_DISCONNECTED, new StackTrace(true).ToString());
                    return INFQSCommunication.mdlGeneral.cERROR;
                }

            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return INFQSCommunication.mdlGeneral.cERROR;
            }
        }
        /// <summary>
        /// Perform next action on QS
        /// </summary>
        /// <returns>Next action result code</returns>
        public int Next()
        {
            try
            {
                string tIgnorePath = mdlGeneral.cCOUNTER_DEFAULT_IGNORE_PATH_VALUE;
                return QSCounterNext(tIgnorePath);
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return INFQSCommunication.mdlGeneral.cERROR;
            }
        }
        #endregion
        #region Private Functions
        /// <summary>
        /// Initialize communication manager and connect to it
        /// </summary>
        private void InitializeCommunicationManager()
        {
            try
            {
                //create new communication manager object and register events to it
                if (mCommunicationManager == null)
                {
                    mCommunicationManager = new CommunicationProxy(mServerIpAddress, mClientName);
                    mCommunicationManager.QSConnectedEvent += ConnectedHandler;
                    mCommunicationManager.QSDisconnectedEvent += DisconnectedHandler;
                    mCommunicationManager.MessageReceivedEvent += MessageReceivedHandler;
                }

                //connect to QS 
                INFQSCommunication.mdlGeneral.eConnectionStatus tConnectionStatus = mCommunicationManager.Connect();

                //if Connection to QS failed
                if (tConnectionStatus != INFQSCommunication.mdlGeneral.eConnectionStatus.Success)
                {
                    INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, mdlGeneral.cERROR_QS_CONNECTION_FAILED + tConnectionStatus.ToString(), new StackTrace(true).ToString()); ;
                }
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }

        }
        /// <summary>
        /// Handle messages received from QS
        /// </summary>
        /// <param name="pMessage">Queuing info message received from QS</param>
        private void MessageReceivedHandler(clsQueuingInfo pMessage)
        {
            try
            {
                switch (pMessage.Command)
                {
                    //handle monitor changed message 
                    case INFQueuingCOMEntities.mdlGeneral.QueuingCommand.MONITOR_CHANGED:
                        clsPacketMonitor tPacketMonitor = (clsPacketMonitor)pMessage.Parameter;
                        UpdateCounterInfo(tPacketMonitor);
                        break;
                }

            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }
        }
        /// <summary>
        /// Handle QS connected
        /// </summary>
        private void ConnectedHandler()
        {
            try
            {
                //change connection status 
                mConnectionStatus = true;
                //invoke QS connected event
                if (QSConnectedEvent != null)
                    QSConnectedEvent();
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }

        }
        /// <summary>
        /// Handle QS disconnected
        /// </summary>
        private void DisconnectedHandler()
        {
            try
            {
                //change connection status 
                mConnectionStatus = false;
                //invoke QS disconnected event
                if (QSDisconnectedEvent != null)
                    QSDisconnectedEvent();
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }

        }
        /// <summary>
        /// Login to system as normal employee
        /// </summary>
        /// <param name="pLoginName">User login name</param>
        /// <param name="pHashedPassword">User Hashed password</param>
        /// <param name="pDomain">Login domain</param>
        /// <param name="mLanguageId">User language</param>
        /// <returns>Login result code</returns>
        private int QSEmployeeNormalLogin(string pLoginName, string pHashedPassword, string pDomain, string mLanguageId)
        {
            try
            {
                //construct login data string
                string tLoginData = $"{pLoginName}|{pHashedPassword}||{pDomain}|false|{(int)INFQueuingCOMEntities.mdlGeneral.LoginTypes.Normal}";

                //set user authentication level as normal employee
                INFQueuingCOMEntities.mdlGeneral.AuthenicationLevels tUserLevel = INFQueuingCOMEntities.mdlGeneral.AuthenicationLevels.Employee;

                //initialize message and response objects 
                clsQueuingInfo tMessage = new clsQueuingInfo();
                clsQueuingInfo[] tResponses = new clsQueuingInfo[0];

                //add required parameters to message
                INFQueuingCOMEntities.mdlGeneral.QueuingCommand tCommand = INFQueuingCOMEntities.mdlGeneral.QueuingCommand.EMPLOYEE_LOGIN;
                object[] tParameter = new object[] { CounterId, tLoginData, "", tUserLevel, mLanguageId };
                tMessage.Command = tCommand;
                tMessage.Parameter = tParameter;

                //send login message to QS and return result
                int tResult = mCommunicationManager.SendMessageToQS(ref tMessage, ref tResponses);
                return tResult;
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return mdlGeneral.cERROR;
            }
        }
        /// <summary>
        /// Perform counter next action on QS
        /// </summary>
        /// <param name="pIgnorePath">Ignore path flag</param>
        /// <returns>Next action result</returns>
        private int QSCounterNext(string pIgnorePath)
        {
            try
            {
                //construct window data string
                string tWindowData = $"{CounterId}|{pIgnorePath}";

                //initialize message and response objects 
                INFQueuingCOMEntities.clsQueuingInfo tMessage = new INFQueuingCOMEntities.clsQueuingInfo();
                INFQueuingCOMEntities.clsQueuingInfo[] tResponses = new INFQueuingCOMEntities.clsQueuingInfo[0];

                //add required parameters to message
                INFQueuingCOMEntities.mdlGeneral.QueuingCommand tCommand = INFQueuingCOMEntities.mdlGeneral.QueuingCommand.WINDOW_NEXT;
                object tParameter = tWindowData;
                tMessage.Command = tCommand;
                tMessage.Parameter = tParameter;

                //send next action message to QS and return result
                int tResult = mCommunicationManager.SendMessageToQS(ref tMessage, ref tResponses);
                return tResult;
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return INFQSCommunication.mdlGeneral.cERROR;
            }

        }
        /// <summary>
        /// Update current counter info
        /// </summary>
        /// <param name="pPacketMonitor">Packet monitor object containing info</param>
        private void UpdateCounterInfo(clsPacketMonitor pPacketMonitor)
        {
            try
            {
                //get current counter window monitor from given packet monitor
                clsWindowMonitor tWindow = pPacketMonitor?.WindowsMonitor?.FirstOrDefault(Window => Window.ID == CounterId);
                //update counter window monitor and invoke updated window event
                if (tWindow != null)
                {
                    mWindowMonitor = tWindow;
                    if (UpdateWindowInfoEvent != null)
                        UpdateWindowInfoEvent(mWindowMonitor);
                }

            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }
        }
        #endregion
    }
}
