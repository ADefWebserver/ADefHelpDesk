using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskTaskDetails
    {
        public AdefHelpDeskTaskDetails()
        {
            AdefHelpDeskAttachments = new HashSet<AdefHelpDeskAttachments>();
        }

        public int DetailId { get; set; }
        public int TaskId { get; set; }
        public string DetailType { get; set; }
        public DateTime InsertDate { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public string ContentType { get; set; }

        public AdefHelpDeskTasks Task { get; set; }
        public ICollection<AdefHelpDeskAttachments> AdefHelpDeskAttachments { get; set; }
    }
}
