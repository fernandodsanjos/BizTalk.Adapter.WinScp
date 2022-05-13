
using Microsoft.BizTalk.Adapter.Common;
using Microsoft.BizTalk.Message.Interop;
using System;
using System.Xml;

namespace BizTalk.Adapter.WinScp.Runtime
{
  public class WinScpEndpointParameters : EndpointParameters
  {
    
    public WinScpEndpointParameters(string outboundLocation)
      : base(outboundLocation)
    {
    }

    public override string SessionKey => this.Properties.SessionKey;

    public WinScpTransmitterProperties Properties { get; set; }

    public void RefreshSessionKey(
      XmlDocument handlerConfigDOM,
      IBaseMessage message,
      string propNamespace,
      IBaseMessageContext context)
    {
      this.Properties = new WinScpTransmitterProperties(this.OutboundLocation);
      if (handlerConfigDOM != null)
      {
        this.Properties.LoadConfig(handlerConfigDOM);
      }
      string xml = (string) context.Read("AdapterConfig", propNamespace);
      if (xml != null)
      {
       
        XmlDocument configDOM = new XmlDocument();
        try
        {
          configDOM.LoadXml(xml);
          this.Properties.LoadConfig(configDOM);
        }
        catch (Exception)
        {
         
          throw new ErrorLoadingConfigXmlDom();
        }
       
      }

      this.Properties.RefreshSessionKey();
    }

    
    }
}
