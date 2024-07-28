using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AdefHelpDeskBase.Models
{
    public class DTODataNode
    {
        [Key]
        public int Id { get; set; }
        public string NodeName { get; set; }
        public int ParentId { get; set; }
    }
}