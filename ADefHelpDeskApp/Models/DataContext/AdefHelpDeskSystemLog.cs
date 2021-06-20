using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskSystemLog
    {
        public int LogId { get; set; }
        public string LogType { get; set; }
        public string LogMessage { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
