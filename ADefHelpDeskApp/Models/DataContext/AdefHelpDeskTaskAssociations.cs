using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskTaskAssociations
    {
        public int TaskRelationId { get; set; }
        public int TaskId { get; set; }
        public int AssociatedId { get; set; }

        public AdefHelpDeskTasks Task { get; set; }
    }
}
