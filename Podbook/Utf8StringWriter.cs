﻿using System.IO;
using System.Text;

namespace Hiale.Podbook
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
