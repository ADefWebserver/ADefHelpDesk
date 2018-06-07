using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ADefHelpDeskApp.Models
{
    public class CategoryDTO
    {
        [Key]
        public string categoryId { get; set; }
        public string label { get; set; }
        public string expandedIcon { get; set; }
        public string collapsedIcon { get; set; }
        public List<CategoryDTO> children { get; set; }
        public int parentId { get; set; }
        public CategoryDTO parent { get; set; }
        public string type { get; set; }
        public bool selectable { get; set; }
        public NodeDetailDTO data { get; set; }
    }

    public class NodeDetailDTO
    {
        [Key]
        public string categoryId { get; set; }
        public Boolean CheckboxChecked { get; set; }
        public bool selectable { get; set; }
        public bool requestorVisible { get; set; }
    }
}
