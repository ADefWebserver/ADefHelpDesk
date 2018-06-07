using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskTasks
    {
        public AdefHelpDeskTasks()
        {
            AdefHelpDeskLog = new HashSet<AdefHelpDeskLog>();
            AdefHelpDeskTaskAssociations = new HashSet<AdefHelpDeskTaskAssociations>();
            AdefHelpDeskTaskCategories = new HashSet<AdefHelpDeskTaskCategories>();
            AdefHelpDeskTaskDetails = new HashSet<AdefHelpDeskTaskDetails>();
        }

        public int TaskId { get; set; }
        public int PortalId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EstimatedStart { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
        public DateTime? DueDate { get; set; }
        public int AssignedRoleId { get; set; }
        public string TicketPassword { get; set; }
        public int RequesterUserId { get; set; }
        public string RequesterName { get; set; }
        public string RequesterEmail { get; set; }
        public string RequesterPhone { get; set; }
        public int? EstimatedHours { get; set; }

        public ICollection<AdefHelpDeskLog> AdefHelpDeskLog { get; set; }
        public ICollection<AdefHelpDeskTaskAssociations> AdefHelpDeskTaskAssociations { get; set; }
        public ICollection<AdefHelpDeskTaskCategories> AdefHelpDeskTaskCategories { get; set; }
        public ICollection<AdefHelpDeskTaskDetails> AdefHelpDeskTaskDetails { get; set; }
    }
}
