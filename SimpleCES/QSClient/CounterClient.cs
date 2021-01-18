using INFEventLogger;
using INFQSCommunication;
using INFQueuingCOMEntities;
using System;
using System.Diagnostics;
using System.Reflection;


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
        private CommunicationProxy mCommunicationManager;
        private bool mConnectionStatus;
        private string mCounterId;
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
        #endregion
        #region Public Functions
        public CounterClient(string pServerIpAddress, string pCounterId)
        {
            try
            {
                mClientName = ConstantResources.cCLIENT_NAME_COUNTER_CLIENT;
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
        public int Login(string pLoginName, string pPassword)
        {
            try
            {
                if (IsConnected)
                {
                    //Currently we only have one type of login
                    string pDomainName = Environment.UserDomainName;
                    string tHashedPassword = SecurityModule.clsTripleDESCrypto.Encrypt(pPassword);
                    string tLanguageID = "1";
                    return QSEmployeeNormalLogin(CounterId, pLoginName, tHashedPassword, pDomainName, tLanguageID);
                }
                else
                {
                    INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, ConstantResources.cERROR_QS_CLIENT_DISCONNECTED, new StackTrace(true).ToString());
                    return INFQSCommunication.mdlGeneral.cERROR;
                }

            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return INFQSCommunication.mdlGeneral.cERROR;
            }
        }
        public int Next()
        {
            try
            {
                string tIgnorePath = "0";
                return QSCounterNext(mCounterId, tIgnorePath);
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return INFQSCommunication.mdlGeneral.cERROR;
            }
        }
        #endregion
        #region Private Functions
        private void InitializeCommunicationManager()
        {
            try
            {
                mCommunicationManager = new CommunicationProxy(mServerIpAddress, mClientName);
                mCommunicationManager.QSConnectedEvent += ConnectedHandler;
                mCommunicationManager.QSDisconnectedEvent += DisconnectedHandler;
                mCommunicationManager.MessageReceivedEvent += MessageReceivedHandler;
                mCommunicationManager.Connect();
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }

        }
        private void MessageReceivedHandler(INFQueuingCOMEntities.clsQueuingInfo pMessage, int pResult)
        {
            try
            {
                
                switch (pMessage.Command)
                {
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
        private void ConnectedHandler()
        {
            try
            {
                mConnectionStatus = true;
                if (QSConnectedEvent != null)
                    QSConnectedEvent();
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }

        }
        private void DisconnectedHandler()
        {
            try
            {
                mConnectionStatus = false;
                if (QSDisconnectedEvent != null)
                    QSDisconnectedEvent();
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }

        }
        private int QSEmployeeNormalLogin(string pCounterId, string pLoginName, string pHashedPassword, string pDomain, string mLanguageId)
        {
            try
            {
                string tLoginData = $"{pLoginName}|{pHashedPassword}||{pDomain}|false|{(int)INFQueuingCOMEntities.mdlGeneral.LoginTypes.Normal}";
                INFQueuingCOMEntities.mdlGeneral.AuthenicationLevels tUserLevel = INFQueuingCOMEntities.mdlGeneral.AuthenicationLevels.Employee;


                INFQueuingCOMEntities.clsQueuingInfo tMessage = new INFQueuingCOMEntities.clsQueuingInfo();
                INFQueuingCOMEntities.clsQueuingInfo[] tResponses = new INFQueuingCOMEntities.clsQueuingInfo[0];

                INFQueuingCOMEntities.mdlGeneral.QueuingCommand tCommand = INFQueuingCOMEntities.mdlGeneral.QueuingCommand.EMPLOYEE_LOGIN;
                object[] tParameter = new object[] { pCounterId, tLoginData, "", tUserLevel, mLanguageId };
                tMessage.Command = tCommand;
                tMessage.Parameter = tParameter;

                return mCommunicationManager.SendMessageToQS(ref tMessage, ref tResponses);
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return INFQSCommunication.mdlGeneral.cERROR;
            }
        }
        private int QSCounterNext(string pCounterId, string pIgnorePath)
        {
            try
            {
                string tWindowData = $"{pCounterId}|{pIgnorePath}";


                INFQueuingCOMEntities.clsQueuingInfo tMessage = new INFQueuingCOMEntities.clsQueuingInfo();
                INFQueuingCOMEntities.clsQueuingInfo[] tResponses = new INFQueuingCOMEntities.clsQueuingInfo[0];

                INFQueuingCOMEntities.mdlGeneral.QueuingCommand tCommand = INFQueuingCOMEntities.mdlGeneral.QueuingCommand.WINDOW_NEXT;
                object tParameter = tWindowData;
                tMessage.Command = tCommand;
                tMessage.Parameter = tParameter;

                int tResult = mCommunicationManager.SendMessageToQS(ref tMessage, ref tResponses);
                return tResult;
            }
            catch (Exception pError)
            {
                INFQSCommunication.mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return INFQSCommunication.mdlGeneral.cERROR;
            }

        }
        private void UpdateCounterInfo(INFQueuingCOMEntities.clsPacketMonitor pPacketMonitor)
        {
            try
            {
                if (pPacketMonitor != null)
                {
                    clsWindowMonitor[] tWindowMonitors = pPacketMonitor.WindowsMonitor;
                    if (tWindowMonitors != null && tWindowMonitors.Length > 0)
                    {
                        foreach (clsWindowMonitor tWindow in tWindowMonitors)
                        {
                            if (tWindow.ID == CounterId)
                            {
                                if (UpdateWindowInfoEvent != null)
                                    UpdateWindowInfoEvent(tWindow);
                                break;
                            }
                        }
                    }
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
