//
// ADefHelpDesk.com
// Copyright (c) 2022
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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdefHelpDeskBase.Models
{
    public class DTOTask
    {
        [Key]
        public int? taskId { get; set; }
        public int? portalId { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public string priority { get; set; }
        public string createdDate { get; set; }
        public string estimatedStart { get; set; }
        public string estimatedCompletion { get; set; }
        public string dueDate { get; set; }
        public int? assignedRoleId { get; set; }
        public string assignedRoleName { get; set; }
        public string ticketPassword { get; set; }
        public int? requesterUserId { get; set; }
        public string requesterName { get; set; }
        public string requesterEmail { get; set; }
        public string requesterPhone { get; set; }
        public int? estimatedHours { get; set; }
        public bool? sendEmails { get; set; }
        public List<int> selectedTreeNodes { get; set; }
        public List<DTOTaskDetail> colDTOTaskDetail { get; set; }
    }
}