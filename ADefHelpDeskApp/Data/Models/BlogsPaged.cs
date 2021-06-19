using System;
using System.Collections.Generic;

namespace ADefHelpDeskApp.Data.Models
{
    public partial class BlogsPaged
    {
        public List<Blogs> Blogs { get; set; }
        public int BlogCount { get; set; }
    }
}