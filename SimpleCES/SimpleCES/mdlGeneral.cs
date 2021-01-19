using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCES
{
    /// <summary>
    /// Constant and static values to be used across module classes
    /// </summary>
    public static class mdlGeneral
    {
        public const string cEMPLOYEE_STATUS_NOT_READY = "Not Ready To Serve";
        public const string cEMPLOYEE_STATUS_READY = "Ready To Serve";
        public const string cEMPLOYEE_STATUS_SERVING = "SERVING";
        public const string cTICKET_NUMBER = "Ticket Number : ";
        public const string cERROR_TITLE_GENERAL = "Unexpected error";
        public const string cERROR_MESSAGE_GENERAL = "An unexpected error occurred";
        public const string cERROR_TITLE_REQUIRED_LOGIN_NAME = "Login name required";
        public const string cERROR_MESSAGE_REQUIRED_LOGIN_NAME = "You must enter a valid login name";
        public const string cERROR_TITLE_REQUIRED_PASSWORD = "Password required";
        public const string cERROR_MESSAGE_REQUIRED_PASSWORD = "You must enter a valid password";
        public const string cERROR_TITLE_LOGIN_FAILED = "Login failed";
        public const string cERROR_MESSAGE_LOGIN_FAILED = "Login failed, please check your credentials and try again";
        public const string cBRANCH_IP = "BranchIP";
        public const string cCOUNTER_ID = "CounterId";
    }
}
