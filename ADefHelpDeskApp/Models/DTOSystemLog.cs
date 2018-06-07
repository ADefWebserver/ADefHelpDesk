using System;
using System.Collections.Generic;
using System.Text;

namespace AdefHelpDeskBase.Models
{
    public class DTOSystemLog
    {
        public int LogID { get; set; }
        public string LogType { get; set; }
        public string LogMessage { get; set; }
        public string UserName { get; set; }
        public string CreatedDate { get; set; }
    }
}