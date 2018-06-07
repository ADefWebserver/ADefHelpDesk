using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskRoles
    {
        public AdefHelpDeskRoles()
        {
            AdefHelpDeskUserRoles = new HashSet<AdefHelpDeskUserRoles>();
        }

        public int Id { get; set; }
        public int PortalId { get; set; }
        public string RoleName { get; set; }

        public ICollection<AdefHelpDeskUserRoles> AdefHelpDeskUserRoles { get; set; }
    }
}
