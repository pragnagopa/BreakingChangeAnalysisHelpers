using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp3._1
{
    internal class RuntimeAssembly
    {
        public string Name { get; set; }

        public string ResolutionPolicy { get; set; }

        public int MajorVersion { get; set; }

        public Version FullVersion { get; set; }
    }
}
