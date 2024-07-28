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
using AdefHelpDeskBase.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADefHelpDeskWebApp.Controllers.ExternalApi.Classes
{
    public static class ExternalAPIUtility
    {
        #region public static DTOTask MapAPITaskToTask(DTOAPITask objTask, DTOAPITaskDetail objTaskDetail)
        public static DTOTask MapAPITaskToTask(DTOAPITask objTask, DTOAPITaskDetail objTaskDetail)
        {
            DTOTask paramTask = new DTOTask();
            paramTask.assignedRoleId = objTask.assignedRoleId ?? -1;
            paramTask.assignedRoleName = objTask.assignedRoleName ?? "";
            paramTask.createdDate = objTask.createdDate ?? "";
            paramTask.description = objTask.description ?? "";
            paramTask.dueDate = objTask.dueDate ?? "";
            paramTask.estimatedCompletion = objTask.estimatedCompletion ?? "";
            paramTask.estimatedHours = objTask.estimatedHours;
            paramTask.estimatedStart = objTask.estimatedStart ?? "";
            paramTask.portalId = objTask.portalId ?? -1;
            paramTask.priority = objTask.priority ?? "";
            paramTask.requesterEmail = objTask.requesterEmail ?? "";
            paramTask.requesterName = objTask.requesterName ?? "";
            paramTask.requesterPhone = objTask.requesterPhone ?? "";
            paramTask.requesterUserId = objTask.requesterUserId ?? -1;
            paramTask.selectedTreeNodes = objTask.selectedTreeNodes;
            paramTask.sendEmails = objTask.sendEmails ?? true;
            paramTask.status = objTask.status ?? "";
            paramTask.taskId = objTask.taskId ?? -1;
            paramTask.ticketPassword = objTask.ticketPassword ?? "";

            if (objTaskDetail != null)
            {
                DTOTaskDetail paramDTOTaskDetail = new DTOTaskDetail();
                paramDTOTaskDetail.colDTOAttachment = new List<DTOAttachment>();
                paramDTOTaskDetail.contentType = objTaskDetail.contentType ?? "";
                paramDTOTaskDetail.description = objTaskDetail.taskDetailDescription ?? "";
                paramDTOTaskDetail.detailId = objTaskDetail.detailId ?? -1;
                paramDTOTaskDetail.detailType = objTaskDetail.detailType ?? "";
                paramDTOTaskDetail.emailDescription = objTaskDetail.emailDescription ?? "";
                paramDTOTaskDetail.insertDate = objTaskDetail.insertDate ?? "";
                paramDTOTaskDetail.sendEmails = objTaskDetail.sendTaskDetailEmails ?? false;
                paramDTOTaskDetail.startTime = objTaskDetail.startTime ?? "";
                paramDTOTaskDetail.stopTime = objTaskDetail.stopTime ?? "";
                paramDTOTaskDetail.userId = objTaskDetail.userId ?? -1;
                paramDTOTaskDetail.userName = objTaskDetail.userName ?? "";

                paramTask.colDTOTaskDetail = new List<DTOTaskDetail>();
                paramTask.colDTOTaskDetail.Add(paramDTOTaskDetail);
            }

            return paramTask;
        } 
        #endregion
    }
}
