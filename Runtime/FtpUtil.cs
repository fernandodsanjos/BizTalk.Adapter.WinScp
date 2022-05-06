using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizTalk.Adapter.WinScp.Runtime
{
    public class FtpUtil
    {
        public static string RemotePath(string folder,string filename)
        {
            folder = folder.Trim('/');

            return $"/{folder}/{filename}".Replace("//", "/");
        }
        
        public static string RemotePathOnly(string remoteFilePath)
        {
            string path = remoteFilePath;

            try
            {
                path = remoteFilePath.Substring(0, remoteFilePath.LastIndexOf('/'));
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("BizTalk Server", $"WinScp Adapter - RemotePathOnly Error={ex.Message} path {remoteFilePath}", EventLogEntryType.Error);
            }

            return path;
        }

        public static string UpdateExtension(string filepath,string newExtension)
        {
            string path = filepath;

            try
            {
                if (newExtension.StartsWith("."))
                {
                    filepath = $"{filepath.Substring(0, filepath.LastIndexOf("."))}{newExtension}";
                }
                else
                    filepath = $"{filepath.Substring(0, filepath.LastIndexOf("."))}.{newExtension}";
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("BizTalk Server", $"WinScp Adapter - UpdateExtension Error={ex.Message} path {filepath} ", EventLogEntryType.Error);
            }
           

            return filepath;

        }

       
    }
}
