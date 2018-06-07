using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskTaskCategories
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int CategoryId { get; set; }

        public AdefHelpDeskCategories Category { get; set; }
        public AdefHelpDeskTasks Task { get; set; }
    }
}
