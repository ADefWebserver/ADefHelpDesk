using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models
{
    public class DTONode
    {
        [Key]
        public string data { get; set; }
        public string label { get; set; }
        public string expandedIcon { get; set; }
        public string collapsedIcon { get; set; }
        public List<DTONode> children { get; set; }
        public int parentId { get; set; }
        public string type { get; set; }
    }
}