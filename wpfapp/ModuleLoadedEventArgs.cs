using System;
using System.Collections.Generic;
using System.Text;

namespace wpfapp
{
    public class ModuleLoadedEventArgs : EventArgs
    {
        public bool Error { get; set; }
        // public bool Success { get; set; }
    }
}
