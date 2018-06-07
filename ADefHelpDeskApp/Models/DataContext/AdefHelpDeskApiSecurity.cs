using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskApiSecurity
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ContactName { get; set; }
        public string ContactCompany { get; set; }
        public string ContactWebsite { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public bool IsActive { get; set; }
    }
}
