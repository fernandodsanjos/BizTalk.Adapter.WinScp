using Microsoft.BizTalk.Adapter.Common;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using System;
using System.Xml;

namespace BizTalk.Adapter.WinScp.Runtime
{
  public class WinScpTransmitter : AsyncTransmitter
  {
    
    private XmlDocument handlerConfigDom;

    public WinScpTransmitter()
      : base(
            "WinScp Transmitter", 
            "1.0", 
            "Send files form BizTalk through WinScp", 
            "WinScp", 
            new Guid("BA2D86DF-D4E1-442A-9809-4DDF9CC05CB7"), 
            "http://BizTalk/adapters/2022/winscp-properties",
            typeof (WinScpTransmitterEndpoint), 50)
    {
      
     
      
    }

    protected override IBTTransmitterBatch CreateAsyncTransmitterBatch() => (IBTTransmitterBatch) new AsyncTransmitterBatch(this.MaxBatchSize, this.EndpointType, this.PropertyNamespace, this.HandlerPropertyBag, this.TransportProxy, (AsyncTransmitter) this);

     
    protected override EndpointParameters CreateEndpointParameters(
      IBaseMessage message)
    {
      IBaseMessageContext context = message.Context;
            WinScpEndpointParameters endpointParameters = new WinScpEndpointParameters(new SystemMessageContext(context).OutboundTransportLocation);
      endpointParameters.RefreshSessionKey(this.handlerConfigDom, message, this.PropertyNamespace, context);
      return (EndpointParameters) endpointParameters;
    }
      
  }
}
