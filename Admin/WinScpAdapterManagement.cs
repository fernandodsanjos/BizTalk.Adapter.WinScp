using Microsoft.BizTalk.Adapter.Common;
using Microsoft.BizTalk.Adapter.Framework;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using WinSCP;
using BizTalk.Adapter.WinScp.VSExtensions;

namespace BizTalk.Adapter.WinScp.Admin
{
    public class WinScpAdapterManagement :
      AdapterManagementBase,
      IAdapterConfig,
      IAdapterConfigValidation
    {

        public string GetConfigSchema(ConfigType type)
        {
            switch (type)
            {
                case ConfigType.ReceiveLocation:
                    return this.GetSchemaFromResource("BizTalk.Adapter.WinScp.Admin.ReceiveLocation.xsd");
                case ConfigType.TransmitLocation:
                    return this.GetSchemaFromResource("BizTalk.Adapter.WinScp.Admin.TransmitLocation.xsd");
                case ConfigType.ReceiveHandler:
                    return this.GetSchemaFromResource("BizTalk.Adapter.WinScp.Admin.ReceiveHandler.xsd");
                case ConfigType.TransmitHandler:
                    return this.GetSchemaFromResource("BizTalk.Adapter.WinScp.Admin.TransmitHandler.xsd");
                default:
                    return string.Empty;
            }
        }

        public Result GetSchema(string xsdLocation, string xsdNamespace, out string xsdFileName)
        {
            xsdFileName = string.Empty;
            return Result.Continue;
        }

        public string ValidateConfiguration(ConfigType type, string config)
        {
            switch (type)
            {
                case ConfigType.ReceiveLocation:
                    return this.ValidateReceiveLocation(config);
                case ConfigType.TransmitLocation:
                    return this.ValidateTransmitLocation(config);
                default:
                    return config;
            }
        }

        private string ValidateTransmitLocation(string config)
        {
            XmlDocument configDOM = new XmlDocument();
            configDOM.LoadXml(config);

            XmlNode url = configDOM.SelectSingleNode("Config/url");

            if (url.IsEmpty())
                throw new ArgumentNullException("ValidateTransmitLocation", "Url is missing");

            XmlNode targetFileName = configDOM.SelectSingleNode("Config/targetFileName");

            if (targetFileName.IsEmpty())
            {
                if (targetFileName == null)
                {
                    targetFileName = configDOM.CreateNode(XmlNodeType.Element, "targetFileName", string.Empty);
                    configDOM.SelectSingleNode("Config").AppendChild(targetFileName);

                }

                targetFileName.InnerText = "%MessageID%.xml";
            }

            AddUriNode(configDOM, url.InnerText, targetFileName.InnerText);

            return configDOM.OuterXml;

        }

        private string ValidateReceiveLocation(string config)
        {

            XmlDocument configDOM = new XmlDocument();
            configDOM.LoadXml(config);

            XmlNode temporaryFolder = configDOM.SelectSingleNode("Config/temporaryFolder");

            //if (temporaryFolder.IsEmpty())
            //    throw new ArgumentNullException("ValidateReceiveLocation", "Temporary folder setting is mandatory");

            XmlNode url = configDOM.SelectSingleNode("Config/url");


            XmlNode fileMask = configDOM.SelectSingleNode("Config/fileMask");

            if (url.IsEmpty())
                throw new ArgumentNullException("ValidateReceiveLocation", "Url is missing");

            if (fileMask.IsEmpty())
            {
                if (fileMask == null)
                {
                    fileMask = configDOM.CreateNode(XmlNodeType.Element, "fileMask", string.Empty);
                    configDOM.SelectSingleNode("Config").AppendChild(fileMask);

                }

                fileMask.InnerText = "*.*";
            }

            AddUriNode(configDOM, url.InnerText, fileMask.InnerText);

            return configDOM.OuterXml;
        }

        private void AddUriNode(XmlDocument configDOM, string url,string fileMaskOrName)
        {

   
            XmlNode port = configDOM.SelectSingleNode("Config/serverPort");
            XmlNode remotePath = configDOM.SelectSingleNode("Config/remotePath");
            XmlNode username = configDOM.SelectSingleNode("Config/userName");

            XmlNode subFolders = configDOM.SelectSingleNode("/Config/subFolders");

            SessionOptions sessionOptions = new SessionOptions();

            sessionOptions.ParseUrl(url);

            if(username.HasValue())
            {
                sessionOptions.UserName = username.InnerText;
            }

            if (port.HasValue("0"))
            {
                sessionOptions.PortNumber = int.Parse(port.InnerText);
            }

            StringBuilder urlBuilder = new StringBuilder();

            if (sessionOptions.UserName.HasValue())
            {
                urlBuilder.Append($"{sessionOptions.UserName}@");
            }

            urlBuilder.Append(sessionOptions.HostName);

            if (sessionOptions.PortNumber > 0)
            {
                urlBuilder.Append($":{sessionOptions.PortNumber}");
            }

            if (remotePath.HasValue())
            {
                urlBuilder.Append($@"/{remotePath.InnerText}");
            }

            if (subFolders.HasValue())
            {
                if(subFolders.InnerText.Length > 20)
                {
                    urlBuilder.Append($@"/*");
                }
                else
                    urlBuilder.Append($@"/{subFolders.InnerText}");
            }
           
            urlBuilder.Append($@"/{fileMaskOrName}");

            string uri = urlBuilder.ToString().Replace("//","/");

            uri = $@"{sessionOptions.Protocol.ToString().ToLower()}://{uri}";

            XmlNode newChild = configDOM.SelectSingleNode("Config/uri");

            if (newChild == null)
            {
                newChild = configDOM.CreateNode(XmlNodeType.Element, "uri", string.Empty);
                configDOM.SelectSingleNode("Config").AppendChild(newChild);
            }

            newChild.InnerText = uri;

        }




    }
}
