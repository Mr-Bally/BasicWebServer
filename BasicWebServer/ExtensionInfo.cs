using System;

namespace BasicWebServer
{
    public class ExtensionInfo
    {
        public string ContentType { get; set; }
        public Func<string, string, ExtensionInfo, ResponsePacket> Loader { get; set; }
        public string FilePath { get; set; }
    }
}
