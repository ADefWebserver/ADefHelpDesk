using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskUserRoles
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public AdefHelpDeskRoles Role { get; set; }
        public AdefHelpDeskUsers User { get; set; }
    }
}
