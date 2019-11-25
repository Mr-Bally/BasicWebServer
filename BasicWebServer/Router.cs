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
              {".ico", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/ico", FilePath="/Content/" }},
              {".png", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/png", FilePath="/Content/" }},
              {".jpg", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/jpg", FilePath="/Content/" }},
              {".gif", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/gif", FilePath="/Content/" }},
              {".bmp", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/bmp", FilePath="/Content/" }},
              {".html", new ExtensionInfo() { Loader=PageLoader, ContentType="text/html", FilePath="/HTML/" }},
              {".css", new ExtensionInfo() { Loader=FileLoader, ContentType="text/css", FilePath="/CSS/" }},
              {".js", new ExtensionInfo() { Loader=FileLoader, ContentType="text/javascript", FilePath="/Javascript/" }},
              {"", new ExtensionInfo() { Loader=PageLoader, ContentType="text/html", FilePath="/HTML/" }},
            };
        }

        public ResponsePacket Route(string method, string path, Dictionary<string, string> kvParams)
        {
            var ext = Path.GetExtension(path);
            ExtensionInfo extInfo;
            ResponsePacket ret = null;

            if (extFolderMap.TryGetValue(ext, out extInfo))
            {
                ret = extInfo.Loader(path, ext, extInfo);
            }
            else
            {
                ret = new ResponsePacket() { ResponseCode = HttpStatusCode.NotFound  };
            }

            return ret;
        }

        private ResponsePacket ImageLoader(string path, string ext, ExtensionInfo extInfo)
        {
            var fullPath = (WebsitePath + extInfo.FilePath + Path.GetFileName(path));
            var fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var br = new BinaryReader(fStream);
            var ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
            br.Close();
            fStream.Close();

            return ret;
        }

        private ResponsePacket FileLoader(string path, string ext, ExtensionInfo extInfo)
        {
            var text = File.ReadAllText(WebsitePath + extInfo.FilePath + Path.GetFileName(path));
            var ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };

            return ret;
        }

        private ResponsePacket PageLoader(string path, string ext, ExtensionInfo extInfo)
        {
            var ret = new ResponsePacket();

            if (path == "/")
            {
                ret = Route("GET", "/index.html", null);
            }
            else
            {
                ret = FileLoader(path, ext, extInfo);
            }

            return ret;
        }

        private void Respond(HttpListenerResponse response, ResponsePacket resp)
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
