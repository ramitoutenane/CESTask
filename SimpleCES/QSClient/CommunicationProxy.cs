using System;
using System.Diagnostics;
using System.Reflection;
using INFEventLogger;
using INFQSCommunication;

namespace QSClient
{
    public class CommunicationProxy
    {
        #region Delegates and Events
        public delegate void MessageReceivedDelegate(INFQueuingCOMEntities.clsQueuingInfo pMessage);
        public event MessageReceivedDelegate MessageReceivedEvent;

        public delegate void QSConnectedDelegate();
        public event QSConnectedDelegate QSConnectedEvent;

        public delegate void QSDisconnectedDelegate();
        public event QSDisconnectedDelegate QSDisconnectedEvent;
        #endregion
        #region Declarations
        private string mServerIpAddress;
        private string mClientName;
        private clsQSClientObject mCommunicationManager;
        #endregion
        #region Public Functions
        /// <summary>
        /// Communication Proxy Constructor 
        /// </summary>
        /// <param name="pServerIpAddress">QS IP address</param>
        /// <param name="pClientName">QS client name</param>
        public CommunicationProxy(string pServerIpAddress, string pClientName)
        {
            try
            {
                mServerIpAddress = pServerIpAddress;
                mClientName = pClientName;
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }

        }
        /// <summary>
        /// connect to QS
        /// </summary>
        /// <returns>QS connection status</returns>
        public mdlGeneral.eConnectionStatus Connect()
        {
            try
            {
                if (!mdlGeneral.RegisterRemotingChannel())
                {
                    mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, ConstantResources.cERROR_CHANEL_REGISTER, new StackTrace(true).ToString());
                    return mdlGeneral.eConnectionStatus.GeneralError;
                }

                //create communication manager and register events to it
                if (mCommunicationManager == null)
                {
                    mCommunicationManager = new clsQSClientObject(mClientName);
                    mCommunicationManager.QSConnected += HandleQSConnectedEvent;
                    mCommunicationManager.QSDisconnected += HandleQSDisconnectedEvent;
                    mCommunicationManager.MessageReceived += HandleQSMessageReceivedEvent;
                }
                //connect to QS
                mdlGeneral.eConnectionStatus tConnectionStatus = mCommunicationManager.Connect(mServerIpAddress);
                if (tConnectionStatus != mdlGeneral.eConnectionStatus.Success)
                {
                    return tConnectionStatus;
                }
                //invoke connected event
                HandleQSConnectedEvent();
                return mdlGeneral.eConnectionStatus.Success;
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return mdlGeneral.eConnectionStatus.GeneralError;
            }

        }
        /// <summary>
        /// Disconnect from QS
        /// </summary>
        public void Disconnect()
        {
            try
            {
                //disconnect from QS then invoke disconnected event
                if (mCommunicationManager != null)
                {
                    mCommunicationManager.Disconnect();
                    HandleQSDisconnectedEvent();
                }
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }
        }
        /// <summary>
        /// Send queuing info message to QS
        /// </summary>
        /// <param name="pMessage">Queuing info message to send to QS</param>
        /// <param name="pResponses">Queuing info array to get responses from QS</param>
        /// <returns>sent message result code</returns>
        public int SendMessageToQS(ref INFQueuingCOMEntities.clsQueuingInfo pMessage, ref INFQueuingCOMEntities.clsQueuingInfo[] pResponses)

        {
            try
            {
                if (mCommunicationManager == null)
                {
                    mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, ConstantResources.cERROR_NULL_QS_CLIENT, new StackTrace(true).ToString());
                    return mdlGeneral.cERROR;
                }
                //send message to QS using communication manager
                return mCommunicationManager.Send(ref pMessage, ref pResponses);
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return mdlGeneral.cERROR;
            }

        }
        #endregion
        #region Private Functions
        /// <summary>
        /// Handle QS received  message
        /// </summary>
        /// <param name="pMessage">Queuing info message to send to QS</param>
        /// <param name="pResult">Received message action result code</param>
        private void HandleQSMessageReceivedEvent(ref INFQueuingCOMEntities.clsQueuingInfo pMessage, ref int pResult)
        {
            try
            {
                //invoke message received event
                if (MessageReceivedEvent != null)
                {
                    MessageReceivedEvent(pMessage);
                }
                pResult = mdlGeneral.cSUCCESS;
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }
        }
        /// <summary>
        /// Handle QS connected
        /// </summary>
        private void HandleQSConnectedEvent()
        {
            try
            {
                //invoke QS connected event
                if (QSConnectedEvent != null)
                {
                    QSConnectedEvent();
                }
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }
        }
        /// <summary>
        /// Handle QS disconnected
        /// </summary>
        private void HandleQSDisconnectedEvent()
        {
            try
            {
                //invoke QS disconnected event
                if (QSDisconnectedEvent != null)
                {
                    QSDisconnectedEvent();
                }
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }
        }
        #endregion
    }
}
