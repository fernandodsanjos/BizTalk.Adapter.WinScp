using System;
using Microsoft.BizTalk.Adapter.Common;

namespace BizTalk.Adapter.WinScp.Runtime
{
    public class WinScpReceiver:Receiver
    {
        public WinScpReceiver():base(
            "WinScp Adapter",
            "1.0", 
            "Fetch remote files into BizTalk using WinScp",
            "WinScp", 
            new Guid("3CD91E6D-DDF6-4ED8-B48E-735086468564"),
             "http://BizTalk/adapters/2022/winscp-properties",
             typeof(WinScpReceiverEndpoint)
            )
        {

        }
    }
}
