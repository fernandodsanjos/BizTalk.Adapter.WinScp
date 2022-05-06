using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizTalk.Adapter.WinScp.Runtime
{
    public class Result
    {
        private Exception error = null;
        public Exception Error
        {
            get
            {
                return error;
            }
            set
            {
                error = value;
                Success = false;
            }
        }

        public bool Success { get; set; }

        public object Data { get; set; }
    }
}
