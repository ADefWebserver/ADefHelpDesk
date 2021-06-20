using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ADefHelpDeskApp.Models
{
    public class CategoryNode
    {
        [Key]
        public int Id { get; set; }
        public string NodeName { get; set; }
        public int? ParentId { get; set; }
        public bool Selectable { get; set; }
        public bool RequestorVisible { get; set; }
    }
}
