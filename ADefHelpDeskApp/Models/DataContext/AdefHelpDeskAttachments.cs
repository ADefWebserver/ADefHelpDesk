using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskAttachments
    {
        public int AttachmentId { get; set; }
        public int DetailId { get; set; }
        public string AttachmentPath { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public int UserId { get; set; }

        public AdefHelpDeskTaskDetails Detail { get; set; }
    }
}
