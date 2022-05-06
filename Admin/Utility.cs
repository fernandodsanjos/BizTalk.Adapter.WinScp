using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BizTalk.Adapter.WinScp.Admin
{
    public static class Utility
    {
        public static string XMLSerialize<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }
        
        public static T XMLDeserialize<T>(string xml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            return  (T)xmlSerializer.Deserialize(XmlReader.Create(xml));
        }
    }
}
