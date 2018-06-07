//
// ADefHelpDesk.com
// Copyright (c) 2017
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
using System.ComponentModel.DataAnnotations;

namespace AdefHelpDeskBase.Models
{
    public static class Constants
    {
        // General
        public const string NONPassword = "[PASSWORD]";
        public const string EmailError = "Email-Error";
        public const string EmailSent = "Email-Sent";
        public const string TaskDetailDeletion = "TaskDetail-Deletion";
        public const string FileDeletion = "File-Deletion";

        // Errors
        public const string FileWriteError = "File-Write-Error";
        public const string FileReadError = "File-Read-Error";
        public const string FileDeleteError = "File-Delete-Error";
        public const string TaskCreationError = "Task-Creation-Error";
        public const string TaskReadError = "Task-Read-Error";
        public const string TaskUpdateError = "Task-Update-Error";
        public const string TaskDeleteError = "Task-Delete-Error";

        // Notification Types
        public const string NotifyNewTask = "NotifyNewTask";
        public const string NotifyUpdateTaskDetail = "NotifyUpdateTaskDetail";

        // WebAPI
        public const string WebAPIAccountCreated = "WebAPI Account Created";
        public const string WebAPIAccountUpdated = "WebAPI Account Updated";
        public const string WebAPIAccountDeleted = "WebAPI Account Deleted";

        // Content Types
        public const string TXT = ".TXT";
        public const string EML = ".EML";
        public const string HTML = ".HTML";
    }
}
