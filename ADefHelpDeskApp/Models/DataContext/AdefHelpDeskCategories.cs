using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskCategories
    {
        public AdefHelpDeskCategories()
        {
            AdefHelpDeskTaskCategories = new HashSet<AdefHelpDeskTaskCategories>();
        }

        public int CategoryId { get; set; }
        public int PortalId { get; set; }
        public int? ParentCategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Level { get; set; }
        public bool RequestorVisible { get; set; }
        public bool Selectable { get; set; }

        public ICollection<AdefHelpDeskTaskCategories> AdefHelpDeskTaskCategories { get; set; }
    }
}
