using INFEventLogger;
using INFQSCommunication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QSClient
{
    public class CommunicationProxy
    {
        public delegate void MessageReceivedDelegate(INFQueuingCOMEntities.clsQueuingInfo pMessage, int pResult);
        public event MessageReceivedDelegate MessageReceivedEvent;

        public delegate void QSConnectedDelegate();
        public event QSConnectedDelegate QSConnectedEvent;

        public delegate void QSDisconnectedDelegate();
        public event QSDisconnectedDelegate QSDisconnectedEvent;

        private string mServerIpAddress;
        private string mClientName;
        private clsQSClientObject mCommunicationManager;
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
        public mdlGeneral.eConnectionStatus Connect()
        {
            try
            {
                if (!mdlGeneral.RegisterRemotingChannel())
                {
                    mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, ConstantResources.cERROR_CHANEL_REGISTER, new StackTrace(true).ToString());
                    return mdlGeneral.eConnectionStatus.GeneralError;
                }

                //register events to QSClientObject
                if (mCommunicationManager == null)
                {
                    mCommunicationManager = new clsQSClientObject(mClientName);
                    mCommunicationManager.QSConnected += HandleQSConnectedEvent;
                    mCommunicationManager.QSDisconnected += HandleQSDisconnectedEvent;
                    mCommunicationManager.MessageReceived += HandleQSMessageReceivedEvent;
                }
                mdlGeneral.eConnectionStatus tConnectionStatus = mCommunicationManager.Connect(mServerIpAddress);
                if (tConnectionStatus != mdlGeneral.eConnectionStatus.Success)
                {
                    return tConnectionStatus;
                }

                HandleQSConnectedEvent();
                return mdlGeneral.eConnectionStatus.Success;
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return mdlGeneral.eConnectionStatus.GeneralError;
            }

        }
        public void Disconnect()
        {
            try
            {
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
        private void HandleQSMessageReceivedEvent(ref INFQueuingCOMEntities.clsQueuingInfo pMessage, ref int pResult)
        {
            try
            {
                if (MessageReceivedEvent != null)
                {
                    MessageReceivedEvent(pMessage, pResult);
                }
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
            }
        }
        private void HandleQSConnectedEvent()
        {
            try
            {
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
        private void HandleQSDisconnectedEvent()
        {
            try
            {
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
        public int SendMessageToQS(ref INFQueuingCOMEntities.clsQueuingInfo pMessage, ref INFQueuingCOMEntities.clsQueuingInfo[] pResponses)

        {
            try
            {
                if (mCommunicationManager == null)
                {
                    mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, ConstantResources.cERROR_NULL_QS_CLIENT, new StackTrace(true).ToString());
                    return mdlGeneral.cERROR;
                }

                return mCommunicationManager.Send(ref pMessage, ref pResponses);
            }
            catch (Exception pError)
            {
                mdlGeneral.LogEvent(mdlEnumerations.INFEventTypes.Error, GetType().ToString(), MethodBase.GetCurrentMethod().Name, pError.Message, pError.StackTrace);
                return mdlGeneral.cERROR;
            }

        }

    }
}
