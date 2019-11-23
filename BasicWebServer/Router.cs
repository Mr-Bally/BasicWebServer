using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BasicWebServer
{
    public class Router
    {
        public string WebsitePath { get; set; }

        private Dictionary<string, ExtensionInfo> extFolderMap;

        public Router(string path)
        {
            WebsitePath = path;
            extFolderMap = new Dictionary<string, ExtensionInfo>()
            {
              {"ico", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/ico" }},
              {"png", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/png" }},
              {"jpg", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/jpg" }},
              {"gif", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/gif" }},
              {"bmp", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/bmp" }},
              {"html", new ExtensionInfo() { Loader=PageLoader, ContentType="text/html" }},
              {"css", new ExtensionInfo() { Loader=FileLoader, ContentType="text/css" }},
              {"js", new ExtensionInfo() { Loader=FileLoader, ContentType="text/javascript" }},
              {"", new ExtensionInfo() { Loader=PageLoader, ContentType="text/html" }},
            };
        }

        public ResponsePacket Route(string verb, string path, Dictionary<string, string> kvParams)
        {
            // Need to strip ext info from url 
            var ext = string.Empty;
            ExtensionInfo extInfo;
            ResponsePacket ret = null;

            if (extFolderMap.TryGetValue(ext, out extInfo))
            {
                string fullPath = Path.Combine(WebsitePath, path);  // Get this from config
                ret = extInfo.Loader(fullPath, ext, extInfo);
            }

            return ret;
        }

        private ResponsePacket ImageLoader(string fullPath, string ext, ExtensionInfo extInfo)
        {
            var fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var br = new BinaryReader(fStream);
            var ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
            br.Close();
            fStream.Close();

            return ret;
        }

        private ResponsePacket FileLoader(string fullPath, string ext, ExtensionInfo extInfo)
        {
            var text = File.ReadAllText(fullPath);
            var ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };

            return ret;
        }

        private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
        {
            var ret = new ResponsePacket();

            if (fullPath == WebsitePath) 
            {
                ret = Route("GET", "/index.html", null);
            }
            else
            {
                if (string.IsNullOrEmpty(ext))
                {
                    fullPath = fullPath + ".html";
                }

                //fullPath = alter to include pages
                ret = FileLoader(fullPath, ext, extInfo);
            }

            return ret;
        }

        private static void Respond(HttpListenerResponse response, ResponsePacket resp)
        {
            response.ContentType = resp.ContentType;
            response.ContentLength64 = resp.Data.Length;
            response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
            response.ContentEncoding = resp.Encoding;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Close();
        }
    }
}
