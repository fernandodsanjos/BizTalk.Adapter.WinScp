using Microsoft.BizTalk.Adapter.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WinSCP;

namespace BizTalk.Adapter.WinScp.Runtime
{
    public class WinScpReceiverProperties: WinScpCommonProperties
    {


        public WinScpReceiverProperties()
        {

        }

        public string FileMask { get; set; } = "*.*";

        public uint MaxFileSize { get; set; } = 100;

        public uint MaximumNumberOfFiles { get; set; }

        public string SubFolders { get; set; }

        public long PollingInterval { get; set; }

        public uint GracePeriod { get; set; } = 0;

        public string ExcludeExtension { get; set; } = ".BTS-WIP";

        public bool DeleteAfterDownload { get; set; } = true;
        public override void LoadConfig(XmlDocument configDOM)
        {
            base.LoadConfig(configDOM);

            this.FileMask = ConfigProperties.IfExistsExtract(configDOM, "/Config/fileMask", "*.*");

            this.ExcludeExtension = ConfigProperties.IfExistsExtract(configDOM, "/Config/excludeExtension", ".BTS-WIP");

            this.SubFolders = ConfigProperties.IfExistsExtract(configDOM, "/Config/subFolders", String.Empty);

            
            this.PollingInterval = ConfigProperties.ExtractPollingInterval(configDOM);
            this.GracePeriod = ConfigProperties.IfExistsExtractUInt(configDOM, "/Config/gracePeriod", 0);

            this.MaxFileSize = ConfigProperties.IfExistsExtractUInt(configDOM, "/Config/maxFileSize", 100);
            this.MaximumNumberOfFiles = ConfigProperties.IfExistsExtractUInt(configDOM, "/Config/maximumNumberOfFiles", 0);

            this.DeleteAfterDownload = ConfigProperties.IfExistsExtractBool(configDOM, "/Config/deleteAfterDownload", true);

        }


    }
}
