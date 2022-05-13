
using Microsoft.BizTalk.Adapter.Common;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using WinSCP;
using BizTalk.Adapter.WinScp.VSExtensions;
using System.Diagnostics;
using System.Xml;

namespace BizTalk.Adapter.WinScp.Runtime
{
    internal class WinScpTransmitterEndpoint : AsyncTransmitterEndpoint
    {

        private string PropertyNamespace { get; set; }

        public Session transmitSession { get; set; }
        private bool reuseEndpoint = true;
        private WinScpConnection connection = null;
        static readonly object lockobject = new object();

        [DllImport("BTSMsgCore.dll", CharSet = CharSet.Unicode)]
        private static extern long BuildFilename(
        IBaseMessage message,
        string maskFilename,
        StringBuilder realFilename,
        ref bool hasWildcard,
        ref int outputLen);
        public WinScpTransmitterEndpoint(AsyncTransmitter container)
        : base(container)
        {
          
        }

        public WinScpTransmitterProperties Properties { get; set; }
        public override bool ReuseEndpoint => this.reuseEndpoint;

        public override void Open(
          EndpointParameters endpointParameters,
          IPropertyBag handlerPropertyBag,
          string propertyNamespace)
        {

            

            this.PropertyNamespace = propertyNamespace;
            this.Properties = ((WinScpEndpointParameters)endpointParameters).Properties;

            try
            {
                if(handlerPropertyBag != null)
                {
                    XmlDocument doc = ConfigProperties.ExtractConfigDom(handlerPropertyBag);
                    this.Properties.LoadHandler(doc);
                }
 
            }
            catch (NoAdapterConfig)
            {

            }

        
            connection = new WinScpConnection(this.Properties);

        }

        public override IBaseMessage ProcessMessage(IBaseMessage message)
        {

            lock (lockobject)
            {
              
                string temporaryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(temporaryPath);

                string remoteFilepath = Properties.RemotePath;

                try
                {

                    string temporaryFilename = Path.Combine(temporaryPath, Guid.NewGuid().ToString());


                    using (FileStream destination = new FileStream(temporaryFilename, FileMode.Create, FileAccess.Write))
                        message.BodyPart.Data.CopyTo((Stream)destination);

                    remoteFilepath = FtpUtil.RemotePath(Properties.RemotePath, CreateFileName(message, Properties.TargetFileName));


                    if (Properties.TemporaryFileExtension.HasValue())
                    {
                        remoteFilepath = FtpUtil.UpdateExtension(remoteFilepath, Properties.TemporaryFileExtension);
                    }

                    connection.OpenSession().PutFiles(temporaryFilename, remoteFilepath, true, connection.GetTransferOptions()).Check();


                    if (Properties.TemporaryFileExtension.HasValue())
                    {
                        string extension = Path.GetExtension(temporaryFilename);

                        string newRemoteFilepath = FtpUtil.UpdateExtension(remoteFilepath, extension);

                        connection.OpenSession().MoveFile(remoteFilepath, newRemoteFilepath);
                    }



                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("BizTalk Server", $"Could not transmit file {remoteFilepath}, Exception {ex.Message}", EventLogEntryType.Error);
                    throw ex;
                }
                finally
                {
                    Directory.Delete(temporaryPath,true);
                    connection.Done();
                }
            }

            return (IBaseMessage)null;
        }

        private static string CreateFileName(IBaseMessage message, string pattern)
        {
            int outputLen = 260;
            StringBuilder realFilename = new StringBuilder(outputLen);
            bool hasWildcard = false;
            NativeMethods.BuildFilename(message, pattern, realFilename, ref hasWildcard, ref outputLen);
            return realFilename.ToString();
        }

        public override void Dispose()
        {
            try
            {
                ShutdownRequest();

                connection.Dispose();
            }
            finally
            {
                base.Dispose();
            }
        }

        private void ShutdownRequest()
        {
            lock (lockobject)
            {
                return;
            }
        }
  
    }
}
