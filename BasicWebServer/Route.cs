﻿using System;
using System.Collections.Generic;

namespace BasicWebServer
{
    public class Route
    {
        public string Verb { get; set; }
        public string Path { get; set; }
        public Func<Dictionary<string, string>, string> Action { get; set; }
    }
}
