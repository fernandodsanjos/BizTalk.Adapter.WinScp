using Microsoft.BizTalk.SSOClient.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace BizTalk.Adapter.WinScp.Runtime
{
    public class SSOApplication
    {
       

        public NetworkCredential Credentials(string SSOAffiliate)
        {
            NetworkCredential credential = new NetworkCredential();
            string externalUserName;
            string[] credentials = ((ISSOLookup1)new SSOLookup()).GetCredentials(SSOAffiliate, 0, out externalUserName);
            if (!string.IsNullOrEmpty(externalUserName))
            {

                credential.UserName = externalUserName;

                if (credentials.Length > 0)
                {
                    credential.Password = credentials[0];
                }

            }

            return credential;
        }
    }
}
