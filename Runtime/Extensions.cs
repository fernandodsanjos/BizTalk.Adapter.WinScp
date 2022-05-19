
using Microsoft.BizTalk.Adapter.Common;
using Microsoft.BizTalk.Component.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BizTalk.Adapter.WinScp.VSExtensions
{
    public static class Extensions
    {
       
      

        public static bool HasValue(this string value,string defaultValue = null)
        {
            if (String.IsNullOrEmpty(value?.Trim()))
            {
                return false;
            }
            else if (defaultValue != null && value.Trim() == defaultValue)
            {
                return false;
            }
            else
                return true;


        }

        public static bool HasValue(this XmlNode node, string defaultValue = null)
        {

            if (String.IsNullOrEmpty(node?.InnerText.Trim()))
            {
                return false;
            }
            else if(defaultValue != null && node.InnerText.Trim() == defaultValue)
            {
                return false;
            }
            else
                return true;


        }

        public static bool IsEmpty(this string value)
        {
            if (String.IsNullOrEmpty(value?.Trim()))
            {
                return true;
            }
            else
                return false;


        }

        public static bool IsEmpty(this XmlNode node)
        {

            if (String.IsNullOrEmpty(node?.InnerText.Trim()))
            {
                return true;
            }
            else
                return false;


        }

       
        
    }
}
