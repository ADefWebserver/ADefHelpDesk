using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskUsers
    {
        public AdefHelpDeskUsers()
        {
            AdefHelpDeskUserRoles = new HashSet<AdefHelpDeskUserRoles>();
        }

        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsSuperUser { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Riapassword { get; set; }
        public string VerificationCode { get; set; }

        public ICollection<AdefHelpDeskUserRoles> AdefHelpDeskUserRoles { get; set; }
    }
}
