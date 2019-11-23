﻿using System.Text;

namespace BasicWebServer
{
    public class ResponsePacket
    {
        public string Redirect { get; set; }
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
        public Encoding Encoding { get; set; }
    }
}
