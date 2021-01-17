﻿using INFEventLogger;
using INFQSCommunication;
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
        #endregion
        #region Declarations
        private string mServerIpAddress;
        private readonly string mClientName;
        private CommunicationProxy mCommunicationManager;
        private bool mConnectionStatus;
        #endregion
        #region Properties
        public bool IsConnected
        {
            get
            {
                return mConnectionStatus;
            }
        }
        #endregion
        #region Public Functions
        public CounterClient(string pServerIpAddress)
        {
            try
            {
                mClientName = ConstantResources.cCLIENT_NAME_COUNTER_CLIENT;
                mServerIpAddress = pServerIpAddress;
                mConnectionStatus = false;
                InitializeCommunicationManager();
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);

            }
        }
        public int Login(string pCounterId, string pLoginName, string pPassword)
        {
            try
            {
                if (IsConnected)
                {
                    //Currently we only have one type of login
                    string pDomainName = Environment.UserDomainName;
                    string tHashedPassword = SecurityModule.clsTripleDESCrypto.Encrypt(pPassword);
                    string tLanguageID = "1";
                    return EmployeeNormalLogin(pCounterId, pLoginName, tHashedPassword, pDomainName, tLanguageID);
                }
                else
                {
                    mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, ConstantResources.cERROR_QS_CLIENT_DISCONNECTED, new StackTrace(true).ToString());
                    return mdlGeneral.cERROR;
                }

            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return mdlGeneral.cERROR;
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
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }

        }
        private void MessageReceivedHandler(INFQueuingCOMEntities.clsQueuingInfo pMessage, int pResult)
        {
            try
            {

            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
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
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
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
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }

        }
        private int EmployeeNormalLogin(string pCounterId, string pLoginName, string pHashedPassword, string pDomain, string mLanguageId)
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
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return mdlGeneral.cERROR;
            }
        }

        #endregion

    }
}