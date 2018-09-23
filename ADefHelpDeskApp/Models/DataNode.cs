using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AdefHelpDeskBase.Models
{
    public class DataNode
    {
        [Key]
        public string Data { get; set; }
        public string NameName { get; set; }
        public string NodeParentData { get; set; }
    }
}