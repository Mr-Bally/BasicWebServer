using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace BasicWebServer
{
    public class Router
    {
        public string WebsitePath { get; set; }

        private readonly Dictionary<string, ExtensionInfo> extFolderMap;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private List<Route> Routes = new List<Route>();

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

        public void AddRoute(Route route)
        {
            Routes.Add(route);
        }

        public ResponsePacket Route(string method, string path, Dictionary<string, string> kvParams)
        {
            var httpMethod = method.ToUpper();
            var ext = Path.GetExtension(path);
            ResponsePacket ret;
            Route route = Routes.FirstOrDefault(r => httpMethod == r.Verb.ToUpper() && path == r.Path);

            if (route != null)
            {
                string redirect = route.Action(kvParams);

                if (string.IsNullOrEmpty(redirect))
                {
                    if (extFolderMap.TryGetValue(ext, out ExtensionInfo extInfo))
                    {
                        ret = extInfo.Loader(path, ext, extInfo);
                    }
                    else
                    {
                        ret = new ResponsePacket() { ResponseCode = HttpStatusCode.NotFound };
                    }
                }
                else
                {
                    // Respond with redirect.
                    ret = new ResponsePacket() { Redirect = redirect };
                }
            }
            else 
            {
                if (extFolderMap.TryGetValue(ext, out ExtensionInfo extInfo))
                {
                    ret = extInfo.Loader(path, ext, extInfo);
                }
                else
                {
                    ret = new ResponsePacket() { ResponseCode = HttpStatusCode.NotFound };
                }
            }

            return ret;
        }

        private ResponsePacket ImageLoader(string path, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret;
            try
            {
                var fullPath = (WebsitePath + extInfo.FilePath + Path.GetFileName(path));
                var fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                var br = new BinaryReader(fStream);
                var text = File.ReadAllText(WebsitePath + extInfo.FilePath + Path.GetFileName(path));
                ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
                br.Close();
                fStream.Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in file loader: {ex.Message}");
                ret = new ResponsePacket() { ResponseCode = HttpStatusCode.NotFound };
            }

            return ret;
        }

        private ResponsePacket FileLoader(string path, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret;
            try
            {
                var text = File.ReadAllText(WebsitePath + extInfo.FilePath + Path.GetFileName(path));
                ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in file loader: {ex.Message}");
                ret = new ResponsePacket() { ResponseCode = HttpStatusCode.NotFound };
            }

            return ret;
        }

        private ResponsePacket PageLoader(string path, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret;

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
