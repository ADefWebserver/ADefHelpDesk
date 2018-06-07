using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskRiausers
    {
        public int UserId { get; set; }
        public string Riapassword { get; set; }
        public string Ipaddress { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
