using Microsoft.BizTalk.Adapter.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WinSCP;

namespace BizTalk.Adapter.WinScp.Runtime
{
    public abstract class WinScpCommonProperties
    {
      
        public enum ProxyMethod
        {
            None = 0,
            SOCKS4 = 1,
            SOCKS5 = 2,
            HTTP = 3,
            Telnet = 4
        }
        //WebDAV and S3, not implemented/tested

        //Session => https://winscp.net/eng/docs/library_session#properties
        //string SessionLogPath	Path to store session log file to. Default null means, no session log file is created. See also DebugLogPath. The property has to be set before calling Open.


        //https://winscp.net/eng/docs/rawsettings

        //bool Opened	Is session opened yet? true, when Open was successfully called already. Read-only.

        //public int ReceiveDataTimeout { get; set; } = 90000;

        //https://winscp.net/eng/docs/library_transferoptions , https://winscp.net/eng/docs/library_transferoptions_addrawsettings

        public TransferMode TransferMode { get; set; } = TransferMode.Automatic;

        //https://winscp.net/eng/docs/library_sessionoptions#properties
        public FtpMode FtpMode { get; set; } = FtpMode.Active;

        /// <summary>
        /// SSL
        /// </summary>
        public FtpSecure FtpSecure { get; set; }

        
        /// <summary>
        /// GiveUpSecurityAndAcceptAnyTlsHostCertificate  FTPS/TLS or set TlsHostCertificateFingerprint. 
        /// SshHostKeyPolicy. SshHostKeyPolicy.Check to verify the host key against SshHostKeyFingerprint OR use SshHostKeyPolicy.AcceptNew to automatically accept host key of new hosts.
        /// </summary>
        public bool AcceptAnyHostCertificate { 
            get; 
            set; 
        }

        /// <summary>
        /// TlsHostCertificateFingerprint (FTPS) or SshHostKeyFingerprint (SFTP)
        /// </summary>
        public string HostKeyFingerprint { get; set; }

        /// <summary>
        /// SshPrivateKeyPath or TlsClientCertificatePath
        /// </summary>
        public string PrivateKeyPath { get; set; }
        public uint PortNumber { get; set; } = 0;

        public string Password { get; set; }

      
        /// <summary>
        /// SSH or SLL
        /// </summary>
        public string PrivateKeyPassphrase { get; set; }

        public string UserName { get; set; }

        public string TemporaryFolder { get; set; }
        /// <summary>
        /// Server response timeout. Defaults to 15 seconds.
        /// </summary>
        public uint Timeout { get; set; }
        public Protocol Protocol { get; set; }

        public string HostName { get; set; }

        /// <summary>
        /// Use WebDAVS (WebDAV over TLS/SSL), instead of WebDAV.
        /// </summary>
        public bool WebdavSecure { get; set; }
 
        public string Uri { get; set; }//Unique port Uri used by BizTalk
        public string Url { get; set; }//Use Url to set default Protocol,HostName,Port and Username

       
        /// <summary>
        /// Handler
        /// </summary>
        public uint ErrorThreshold
        {
            get; set;
        }

        /// <summary>
        /// Must start with /
        /// </summary>
        public string RemotePath { get; set; }

        public string SessionLogPath { get; set; }

        public string DebugLogPath { get; set; }

        public int DebugLogLevel { get; set; }

        public ProxyMethod FirewallType { get; set; } = ProxyMethod.None;

        public string FirewallAddress { get; set; }

        public string FirewallUserName { get; set; }
        public string FirewallPassword { get; set; }

        public int FirewallPort { get; set; }


        public WinScpCommonProperties()
        {
         
        }

        /// <summary>
        /// Load handler config
        /// </summary>
        public void LoadHandler(XmlDocument config)
        {
            //https://winscp.net/eng/docs/rawsettings

            int firewallType = ConfigProperties.ExtractInt(config, "/Config/firewallType");

            if(firewallType > 0)
            {
                FirewallType = (ProxyMethod)firewallType;
                FirewallAddress = ConfigProperties.IfExistsExtract(config, "/Config/firewallAddress", FirewallAddress);
                FirewallUserName = ConfigProperties.IfExistsExtract(config, "/Config/firewallUserName", FirewallUserName);
                FirewallPassword = ConfigProperties.IfExistsExtract(config, "/Config/firewallPassword", FirewallPassword);
                FirewallPort = ConfigProperties.ExtractInt(config, "/Config/firewallPort");
         
            }

        }

        /// <summary>
        /// Load adapter or runtime config
        /// </summary>
        public virtual void LoadConfig(XmlDocument configDOM)
        {
            SessionOptions options = new SessionOptions();

            this.Url = ConfigProperties.Extract(configDOM, "/Config/url", string.Empty);

            options.ParseUrl(this.Url);
            
            this.HostName = options.HostName;

            if (String.IsNullOrEmpty(options.UserName))
            {
                this.UserName = ConfigProperties.IfExistsExtract(configDOM, "/Config/userName", String.Empty);
            }
            else
                this.UserName = options.UserName;

            if (options.PortNumber  == 0)
                this.PortNumber = ConfigProperties.IfExistsExtractUInt(configDOM, "/Config/serverPort", 0);

            this.FtpMode = ExtractFtpMode(configDOM);
            this.FtpSecure = ExtractFtpsConnMode(configDOM);
            this.TransferMode = ExtractTransferMode(configDOM);
            this.RemotePath = ConfigProperties.IfExistsExtract(configDOM, "/Config/remotePath", @"/");

            this.AcceptAnyHostCertificate = ConfigProperties.IfExistsExtractBool(configDOM, "/Config/acceptAnyHostCertificate", false);
            this.HostKeyFingerprint = ConfigProperties.IfExistsExtract(configDOM, "/Config/hostKeyFingerprint", String.Empty);
            this.PrivateKeyPath = ConfigProperties.IfExistsExtract(configDOM, "/Config/privateKeyPath", String.Empty);

            this.Password =  ConfigProperties.IfExistsExtract(configDOM, "/Config/password", String.Empty);
            this.PrivateKeyPassphrase = ConfigProperties.IfExistsExtract(configDOM, "/Config/privateKeyPassphrase", String.Empty);
           
            this.Timeout = ConfigProperties.IfExistsExtractUInt(configDOM, "/Config/timeout", 90000);
            this.SessionLogPath = ConfigProperties.IfExistsExtract(configDOM, "/Config/sessionLogPath", String.Empty);
            this.DebugLogPath = ConfigProperties.IfExistsExtract(configDOM, "/Config/debugLogPath", String.Empty);
            this.DebugLogLevel = ConfigProperties.IfExistsExtractInt(configDOM, "/Config/debugLogLevel", 0);
            this.ErrorThreshold = ConfigProperties.IfExistsExtractUInt(configDOM, "/Config/errorThreshold", 10);

            
            /* TO-DO
            string affiliateApplication = ConfigProperties.IfExistsExtract(configDOM, "/Config/ssoAffiliateApplication", (string)null);
            if (!string.IsNullOrEmpty(affiliateApplication))
            {
                SSOResult ssoResult = new SSOResult(message, affiliateApplication);
                this.userName = ssoResult.UserName;
                this.password = ssoResult.Result[0];
            }
            */

            this.Uri = ConfigProperties.Extract(configDOM, "/Config/uri", String.Empty);
            this.TemporaryFolder = ConfigProperties.IfExistsExtract(configDOM, "/Config/temporaryFolder", String.Empty);
        }

        /// <summary>
        /// Used to initial load Session on Open
        /// </summary>
        public void Load(XmlDocument adapterConfig, XmlDocument portConfig, XmlDocument handlerConfig)
        {
            LoadConfig(adapterConfig);
        }

        /// <summary>
        /// Runtime update
        /// </summary>
        public void Update(XmlDocument adapterConfig, XmlDocument portConfig, XmlDocument handlerConfig)
        {
            LoadConfig(adapterConfig);
        }
        public bool Validate()
        {
            return true;
        }

        private FtpMode ExtractFtpMode(XmlDocument configDOM)
        {

            string psv = ConfigProperties.Extract(configDOM, "/Config/passiveMode", "Passive");

            FtpMode ftpMode = (FtpMode)Enum.Parse(typeof(FtpMode), psv);


            return ftpMode;
        }

        private FtpSecure ExtractFtpsConnMode(XmlDocument configDOM)
        {
            
            string conn = ConfigProperties.Extract(configDOM, "/Config/ftpsConnMode", "None");
            
            FtpSecure ftpSecure = (FtpSecure)Enum.Parse(typeof(FtpSecure), conn);


            return ftpSecure;
        }

        private TransferMode ExtractTransferMode(XmlDocument configDOM)
        {

            string tran = ConfigProperties.Extract(configDOM, "/Config/transferMode", "Automatic");

            TransferMode transferMode = (TransferMode)Enum.Parse(typeof(TransferMode), tran);


            return transferMode;
        }
    }
}
