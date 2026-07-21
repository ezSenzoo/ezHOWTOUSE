using System;
using System.Collections.Generic;
using System.Text;

namespace HOWTOUSE
{
    public static class SessionContext
    {
        public static string STF_NO { get; private set; }

        public static string STF_NM { get; private set; }

        public static string IP_ADDRESS { get; private set; }


        public static void SetUser(string employeeNo, string userName, string ipAdress)
        {
            STF_NO = employeeNo;
            STF_NM = userName;
            IP_ADDRESS = ipAdress;
        }


        public static void Clear()
        {
            STF_NO = null;
            STF_NM = null;
            IP_ADDRESS = null;
        }
    }
}
