//
// ADefHelpDesk.com
// Copyright (c) 2021
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
using AdefHelpDeskBase.Models.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADefHelpDeskApp.Classes
{
    class Log
    {
        public Log() { }

        #region InsertLog
        public static void InsertLog(string DefaultConnection, int TaskID, int UserID, string LogDescription)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                AdefHelpDeskLog objAdefHelpDeskLog = new AdefHelpDeskLog();
                objAdefHelpDeskLog.DateCreated = DateTime.Now;
                objAdefHelpDeskLog.LogDescription = Extensions.Left(LogDescription, 499);
                objAdefHelpDeskLog.TaskId = TaskID;
                objAdefHelpDeskLog.UserId = UserID;

                context.AdefHelpDeskLog.Add(objAdefHelpDeskLog);
                context.SaveChanges();
            }
        }
        #endregion

        #region InsertSystemLog
        public static void InsertSystemLog(string DefaultConnection, string LogType, string UserName, string LogMessage)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                AdefHelpDeskSystemLog objAdefHelpDeskSystemLog = new AdefHelpDeskSystemLog();
                objAdefHelpDeskSystemLog.CreatedDate = DateTime.Now;
                objAdefHelpDeskSystemLog.LogMessage = Extensions.Left(LogMessage, 4000);
                objAdefHelpDeskSystemLog.LogType = LogType;
                objAdefHelpDeskSystemLog.UserName = UserName;

                context.AdefHelpDeskSystemLog.Add(objAdefHelpDeskSystemLog);
                context.SaveChanges();
            }
        }
        #endregion
    }
}
