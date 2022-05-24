using Microsoft.BizTalk.Adapter.Common;
using Microsoft.BizTalk.Message.Interop;
using BizTalk.Adapter.WinScp.Runtime;
using System;
using System.Text;
using System.Xml;

namespace BizTalk.Adapter.WinScp.Runtime
{
  public class WinScpTransmitterProperties : WinScpCommonProperties
  {

    public string SessionKey { get; set; }
    public string OutboundLocation { get; set; }

    public string TargetFileName { get; set; }

    public string TemporaryFileExtension { get; set; }
        /// <summary>
        /// Connection reuse time in seconds
        /// </summary>
    public uint ConnectionReuseTime { get; set; } = 120;

    public void RefreshSessionKey()
    {
            //whats the usage?****
            SessionKey = this.OutboundLocation;
    }


    public WinScpTransmitterProperties(string outboundLocation)
    {
      this.OutboundLocation = outboundLocation;
      this.TargetFileName = "%MessageID%.xml";
    }

    public override void LoadConfig(XmlDocument configDOM)
    {
        base.LoadConfig(configDOM);

        this.TargetFileName = ConfigProperties.IfExistsExtract(configDOM, "/Config/targetFileName", this.TargetFileName);
        this.ConnectionReuseTime = ConfigProperties.IfExistsExtractUInt(configDOM, "/Config/maxConnectionReuseTime", 120);
        this.TemporaryFileExtension = ConfigProperties.IfExistsExtract(configDOM, "/Config/temporaryFileExtension", String.Empty);
  
    }

   
  }
}
