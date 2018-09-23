using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskLog
    {
        public int LogId { get; set; }
        public int TaskId { get; set; }
        public string LogDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public int UserId { get; set; }

        public AdefHelpDeskTasks Task { get; set; }
    }
}
