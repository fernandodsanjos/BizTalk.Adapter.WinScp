using System;
using Microsoft.BizTalk.Adapter.Common;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.BizTalk.Component.Interop;
using WinSCP;
using System.Xml;
using Microsoft.BizTalk.Message.Interop;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BizTalk.Adapter.WinScp.VSExtensions;
using System.Runtime.InteropServices;
using System.Timers;

namespace BizTalk.Adapter.WinScp.Runtime
{
    public class WinScpReceiverEndpoint : ReceiverEndpoint
    {
        private static FILE.ReceivedFileName FILE_ReceivedFileNameProp = new FILE.ReceivedFileName();
        private IBTTransportProxy transportProxy;
        private string transportType;
        private string propertyNamespace;
        private ControlledTermination controlledTermination;
        private IBaseMessageFactory messageFactory;
        
       static readonly object lockobject = new object();

        private WinScpConnection connection = null;
        //  error count for comparison with the error threshold
        int errorCount;

        private System.Timers.Timer timer = null;

        public WinScpReceiverProperties Properties { get; set; }
        public WinScpReceiverEndpoint()
        {
           
        }

        
        private void PickupFilesAndSubmit()
        {

            if (this.controlledTermination.TerminateCalled)
                return;

            string tempDirectory = Properties.TemporaryFolder.HasValue() ? Properties.TemporaryFolder : Path.GetTempPath();
            List<RemoteFileInfo> files = null;

            int currentFile = 0;

            try
            {

                string[] subFolders = this.Properties.SubFolders.Split(new string[] { "|" }, StringSplitOptions.None);

                foreach (var folder in subFolders)
                {
                    if (this.controlledTermination.TerminateCalled)
                        break;

                    var remotePath = folder.HasValue() ? FtpUtil.RemotePath(Properties.RemotePath, folder): Properties.RemotePath;

                    files =
                     connection.OpenSession().EnumerateRemoteFiles(
                         remotePath, Properties.FileMask, EnumerationOptions.None)
                            .Where(fileInfo => fileInfo.LastWriteTime < DateTime.Now.AddSeconds(-(Properties.GracePeriod)) || Properties.GracePeriod == 0)
                         .ToList();


                    foreach (RemoteFileInfo file in files)
                    {
                        if (this.controlledTermination.TerminateCalled)
                            break;

                        if (currentFile > Properties.MaximumNumberOfFiles && Properties.MaximumNumberOfFiles > 0)
                            break;

                        if (file.Length == 0)
                            continue;

                        if (false == CheckMaxFileSize(file))
                            continue;

                        if (file.IsDirectory)
                            continue;

                        string fileExtension = Path.GetExtension(file.Name);

                        if (Properties.ExcludeExtension.HasValue() && fileExtension.EndsWith(Properties.ExcludeExtension))
                            continue;

                        tempDirectory = Path.Combine(tempDirectory, folder);

                        Directory.CreateDirectory(tempDirectory);

                        string localFilePath = Path.Combine(tempDirectory, file.Name);

                        if (File.Exists(localFilePath))
                        {
                            //Possible check lastwritedatetime if temp folder is specified
                            continue;
                        }

                        currentFile++;

                        DonwloadAndSubmit(file, localFilePath);

                    }
               
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("BizTalk Server", $"Could not retrieve file(s), Excception {ex.Message}", EventLogEntryType.Warning);
                errorCount++;
                throw ex;
            }
            finally
            {
                if (Properties.TemporaryFolder.IsEmpty())
                    Directory.Delete(tempDirectory,true);

                connection?.CloseSession();

                files = null;
            }


            if (currentFile > 0 && Properties.DeleteAfterDownload)
                PickupFilesAndSubmit();



        }

        private void DonwloadAndSubmit(RemoteFileInfo file,string localFilePath)
        {
            TransferOptions transferOptions = new TransferOptions
            {
                TransferMode = Properties.TransferMode,
                PreserveTimestamp = false
            };
        


            try
            {
                connection.OpenSession().GetFiles(file.FullName, localFilePath, options: transferOptions).Check();


                IBaseMessage msg = CreateMessage(localFilePath);


                if (null == msg)
                    return;

                string remotePath = FtpUtil.RemotePathOnly(file.FullName);


                if (this.Properties.SubFolders.HasValue())
                {
                    msg.Context.Promote("FilePath", "http://schemas.microsoft.com/BizTalk/2003/legacy-properties", (object)remotePath.Trim('/'));
                }

                msg.Context.Write("FileCreationTime", "http://schemas.microsoft.com/BizTalk/2003/file-properties", (object)file.LastWriteTime);

                Sumbit(msg, localFilePath, file.FullName);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("BizTalk Server", $"WinScp Adapter - DonwloadAndSubmit failed {ex.Message}", EventLogEntryType.Error);
                throw ex;
            }
           


            

           
        }

        private void RemoveLocalFile(string localFilePath)
        {
            try
            {
                if (File.Exists(localFilePath))
                    File.Delete(localFilePath);
            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("BizTalk Server", $"WinScp Adapter - Could not remove local file {ex.Message}", EventLogEntryType.Warning);

            }
        }
        private void RemoveFile(string remoteFilePath,string localFilePath)
        {
            
            try
            {
                if(Properties.DeleteAfterDownload)
                    connection.OpenSession().RemoveFile(remoteFilePath);


                RemoveLocalFile(localFilePath);

               

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("BizTalk Server", $"WinScp Adapter - Could not remove remote file {ex.Message}", EventLogEntryType.Error);
            }
  

        }

        private void EmptyFile(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite,FileShare.None))
            {
                try
                {
                    fs.SetLength(0);
                }
                catch{ }
            }
        }
        private void Sumbit(IBaseMessage msg,string localFilePath,string remotePath)
        {

            try
            {
                using (SyncReceiveSubmitBatch batch = new SyncReceiveSubmitBatch(this.transportProxy, this.controlledTermination, 1))
                {
                   
                    batch.SubmitMessage(msg, new StreamAndUserData(msg.BodyPart.Data, (object)localFilePath));

                    batch.Done();

                    if(batch.Wait())
                    {
                        RemoveFile(remotePath, localFilePath);
                    }

                }
            }
            catch (Exception)
            {
                RemoveLocalFile(localFilePath);

            }
           

        }

           
           


        

       
        private bool CheckMaxFileSize(RemoteFileInfo item)
        {
            long fileSizeBytes = item.Length;
            if (this.Properties.MaxFileSize > 0)
            {
                long maxFileSizeBytes = 1024 * 1024 * this.Properties.MaxFileSize;
                return (fileSizeBytes <= maxFileSizeBytes);
            }
            else
                return true;
        }
        private IBaseMessage CreateMessage(string srcFilePath)
        {
            FileStream fs;

            // Open the file
            try
            {
               fs = File.Open(srcFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None | FileShare.Delete);
            }
            catch (Exception)
            {
               return null;
            }

            IBaseMessagePart part = this.messageFactory.CreateMessagePart();

            part.Data = fs;
            IBaseMessage message = this.messageFactory.CreateMessage();
            message.AddPart("body", part, true);

            SystemMessageContext context = new SystemMessageContext(message.Context);
            context.InboundTransportLocation = this.Properties.Uri;
            context.InboundTransportType = this.transportType;
            

            Microsoft.XLANGs.BaseTypes.XmlQName fileProp = new FILE.ReceivedFileName().QName;
            message.Context.Write(fileProp.Name, fileProp.Namespace, (object)Path.GetFileName(srcFilePath));

            return message;
        }

        public void ControlledEndpointTask(object sender, ElapsedEventArgs e)
        {
            ControlledEndpointTask();
        }
        public void ControlledEndpointTask()
        {

            if (this.controlledTermination.Enter())
            {

                try
                {

                    this.EndpointTask();
                   
                    GC.Collect();
                }
                catch (COMException ex)
                {
                    EventLog.WriteEntry("BizTalk Server", $"WinScp ControlledEndpointTask - e.ErrorCode = {ex.ErrorCode}", EventLogEntryType.Error);
                    
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("BizTalk Server", $"WinScp ControlledEndpointTask - e.Message = {ex.Message}", EventLogEntryType.Error);
                  
                }
                finally
                {
                    this.controlledTermination.Leave();

                    
                }
            }
     

         
        }

        /// <summary>
        /// Handle the work to be performed each polling interval
        /// </summary>
        private void EndpointTask()
        {
            try
            {
              

                PickupFilesAndSubmit();

                //Success, reset the error count
                errorCount = 0;
            }
            catch (Exception e)
            {
                //WinScp - EndpointTask Failed Undantag från HRESULT: 0xC0C0163C
                EventLog.WriteEntry("BizTalk Server", $"WinScp Adapter - EndpointTask Failed {e.Message}", EventLogEntryType.Error);
                transportProxy.SetErrorInfo(e);
                //Track number of failures
                errorCount++;

            }
            finally
            {
                if(this.controlledTermination.TerminateCalled == false)
                    StopOrContinue();
            }


              
        }

        private void StopOrContinue()
        {
            if ((this.Properties.ErrorThreshold != 0) && (this.errorCount >= this.Properties.ErrorThreshold))
            {

                this.transportProxy.ReceiverShuttingdown(this.Properties.Uri, new ErrorThresholdExceeded());

            }
            else
            {
                StartTimer();

            }


        }

        public override void Open(
            string uri,
            IPropertyBag config,
            IPropertyBag bizTalkConfig,
            IPropertyBag handlerPropertyBag,
            IBTTransportProxy transportProxy,
            string transportType,
            string propertyNamespace, ControlledTermination control)
        {

            //config
            this.transportProxy = transportProxy;
            this.transportType = transportType;
            this.propertyNamespace = propertyNamespace;
            this.controlledTermination = control;
            this.messageFactory = this.transportProxy.GetMessageFactory();

            Properties = new WinScpReceiverProperties();
            Properties.LoadConfig(ConfigProperties.ExtractConfigDom(config));

            connection = new WinScpConnection(Properties);

            StartTimer();
            

        }

        private void StopTimer()
        {
            lock (lockobject)
            {

                if (timer == null)
                    return;

                this.timer.Stop();
            }
        }

        private void StartTimer()
        {
            lock (lockobject)
            {
                if (this.timer == null)
                {
                    this.timer = new System.Timers.Timer(Properties.PollingInterval * 1000);
                    this.timer.AutoReset = false;
                    this.timer.Elapsed += new ElapsedEventHandler(this.ControlledEndpointTask);
                }

                this.timer.Start();
            }
        }
        public override void Update(
            IPropertyBag config, 
            IPropertyBag bizTalkConfig, 
            IPropertyBag handlerPropertyBag)
        {
            StopTimer();
            //bizTalkConfig
            Properties.LoadConfig(ConfigProperties.ExtractConfigDom(config));

            connection = new WinScpConnection(Properties);

            StartTimer();
        }

        public override void Dispose()
        {

           this.controlledTermination.Terminate();


            try
            {
                if (timer != null)
                    timer.Dispose();

                if (connection != null)
                    connection.Dispose();
            }
            finally
            {
                //  disposed = true;
                timer = null;
                connection = null;

                base.Dispose();

                // GC.SuppressFinalize(this);
            }
           

          

        }

      


    }
}
