//
// ADefHelpDesk.com
// Copyright (c) 2024
// by Michael Washington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
//
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdefHelpDeskBase.Models
{
    /// <summary>
    /// Task Detail object
    /// </summary>
    public class DTOAPITaskDetail
    {
        /// <summary>
        /// Required
        /// </summary>
        public int taskId { get; set; }
        /// <summary>
        /// Required
        /// </summary>
        public string ticketPassword { get; set; }
        [DefaultValue(-1)]
        public int? detailId { get; set; }
        /// <summary>
        /// Comment - Visible / Comment / Work
        /// </summary>
        [DefaultValue("Comment - Visible")]
        public string detailType { get; set; }
        /// <summary>
        /// Null / EML
        /// </summary>
        public string contentType { get; set; }
        public string insertDate { get; set; }
        [DefaultValue(-1)]
        public int? userId { get; set; }
        public string userName { get; set; }
        public string taskDetailDescription { get; set; }
        public string emailDescription { get; set; }
        public string startTime { get; set; }
        public string stopTime { get; set; }
        [DefaultValue(false)]
        public bool? sendTaskDetailEmails { get; set; }
        public IFormFile fileattachment { get; set; }
    }
}