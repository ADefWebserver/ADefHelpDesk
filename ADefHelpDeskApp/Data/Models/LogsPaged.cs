using System;
using System.Collections.Generic;

namespace ADefHelpDeskApp.Data.Models
{
    public partial class LogsPaged
    {
        public List<Logs> Logs { get; set; }
        public int LogCount { get; set; }
    }
}