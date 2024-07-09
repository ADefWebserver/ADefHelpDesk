using System;
using System.Collections.Generic;
using System.Text;

namespace AdefHelpDeskBase.Models
{
    public class DTOLog
    {
        public int LogID { get; set; }
        public int TaskID { get; set; }
        public string LogDescription { get; set; }
        public string DateCreated { get; set; }
        public string UserName { get; set; }
    }
}