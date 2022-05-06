using Microsoft.BizTalk.Message.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinSCP;
using BizTalk.Adapter.WinScp.VSExtensions;
using System.Timers;
using Microsoft.BizTalk.Adapter.Common;

namespace BizTalk.Adapter.WinScp.Runtime
{


    public class WinScpConnection : IDisposable
    {


        private static readonly object lockObject = new object();
        private WinScpCommonProperties Properties { get; set; }
        private Session session = null;
        private SessionOptions options = null;

        private bool Busy { get; set; }
        private DateTime LastUsed { get; set; }

        private System.Timers.Timer timer;
        //ConnectionReuseTime
        public WinScpConnection(WinScpCommonProperties properties)
        {
            Properties = properties;

            if (properties is WinScpTransmitterProperties)
            {
                this.timer = new System.Timers.Timer(((WinScpTransmitterProperties)Properties).ConnectionReuseTime * 1000);
                this.timer.AutoReset = true;
                this.timer.Elapsed += new ElapsedEventHandler(this.Release);
                this.timer.Enabled = false;
            }
        }

        private void Release(object sender, ElapsedEventArgs e)
        {

            lock (lockObject)
            {
                this.timer.Close();

                if (Busy)
                {
                    this.timer.Start();
                    return;
                }


                if (SessionOpened() == false)
                {
                    this.timer.Start();
                    return;
                }


                if (LastUsed.AddSeconds(((WinScpTransmitterProperties)Properties).ConnectionReuseTime) < DateTime.Now)
                {
                    CloseSession();
                }
                else
                    this.timer.Start();


            }
        }

        /// <summary>
        /// Signal that work is done for now with this session
        /// </summary>
        public void Done()
        {
            lock (lockObject)
            {
                LastUsed = DateTime.Now;
                Busy = false;
                timer.Start();
            }
        }


        public Session OpenSession()
        {
            lock (lockObject)
            {
                Busy = true;

                LastUsed = DateTime.Now;

                if (SessionOpened() == false)
                {
                    #region TODO
                    //Before
                    //After

                    #endregion
                    session = new Session
                    {
                        SessionLogPath = Properties.SessionLogPath.HasValue() ? Properties.SessionLogPath:null,
                        DebugLogPath = Properties.DebugLogPath.HasValue() ? Properties.DebugLogPath : null,
                        DebugLogLevel = Properties.DebugLogLevel


                    };

                    session.Open(GetSessionOptions());

                }

                return session;
            }

        }




        private SessionOptions GetSessionOptions()
        {

            options = new SessionOptions();

            options.ParseUrl(Properties.Url);

            options.PortNumber = Properties.PortNumber > 0 ? (int)Properties.PortNumber : options.PortNumber;
            options.UserName = Properties.UserName.HasValue() ? Properties.UserName : options.UserName;
            options.Password = Properties.Password.HasValue() ? Properties.Password : options.Password;
            options.FtpMode = Properties.FtpMode;


            //options.AddRawSettings("ProxyMethod", "3");
            //options.AddRawSettings("ProxyHost", "********");
            //options.AddRawSettings("ProxyPort", "8082");

            //options.AddRawSettings("KEX", ..KexPolicy);

            /*
             *  private const string DefaultEncryptionCipher = "aes,3des,blowfish,WARN,arcfour,des";
                private const string Cipher = "Cipher";

               AddEncryptionCipher =>   if (encryptionCipher.Equals((object) EncryptionCipher.Auto))
                .AddRawSettings("Cipher", "aes,3des,blowfish,WARN,arcfour,des");
            else if (encryptionCipher.Equals((object) EncryptionCipher.TripleDES))
                .AddRawSettings("Cipher", "3des,WARN");
            else
                .AddRawSettings("Cipher", encryptionCipher.ToString().ToLower() + ",WARN"); 

             */

            if (options.Protocol == Protocol.Sftp)
            {
                options.SshPrivateKeyPath = Properties.PrivateKeyPath.HasValue() ? Properties.PrivateKeyPath : null;
                options.SshHostKeyPolicy = Properties.AcceptAnyHostCertificate ? SshHostKeyPolicy.GiveUpSecurityAndAcceptAny : SshHostKeyPolicy.Check;
                options.SshHostKeyFingerprint = Properties.AcceptAnyHostCertificate ? "ssh-rsa 2048 10:10:10:10:10:10:10:10:10:10:10:10:10:10:10:10" : Properties.HostKeyFingerprint;
            }

            if (options.Protocol == Protocol.Ftp)
            {
                if (Properties.FtpSecure != FtpSecure.None)
                {
                    options.TlsClientCertificatePath = Properties.PrivateKeyPath.HasValue() ? Properties.PrivateKeyPath : options.TlsClientCertificatePath;
                    options.GiveUpSecurityAndAcceptAnyTlsHostCertificate = Properties.AcceptAnyHostCertificate;
                    options.TlsHostCertificateFingerprint = Properties.AcceptAnyHostCertificate ? "00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00" : Properties.HostKeyFingerprint;
                    options.FtpSecure = Properties.FtpSecure;
                }
            }

            if (options.TlsClientCertificatePath.HasValue() || options.SshPrivateKeyPath.HasValue())
            {
                options.PrivateKeyPassphrase = Properties.PrivateKeyPassphrase.IsEmpty() ? null : Properties.PrivateKeyPassphrase;
            }

            return options;
        }

        public bool SessionOpened()
        {
            if (session == null)
                return false;

            try
            {
                return session.Opened;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public void CloseSession()
        {
            try
            {
                session?.Close();
            }
            catch (Exception)
            {

            }
        }

        public void Dispose()
        {
            lock (lockObject)
            {
                try
                {
                    if (timer != null)
                    {
                        timer.Stop();
                        timer.Dispose();
                    }

                    if (session != null)
                    {
                        if (Marshal.IsComObject(session))
                        {
                            while (0 < Marshal.ReleaseComObject(session))
                                GC.SuppressFinalize(session);
                        }

                        session.Dispose();
                    }

                }
                finally
                {

                    timer = null;
                    session = null;

                }
            }



        }
    }
}
