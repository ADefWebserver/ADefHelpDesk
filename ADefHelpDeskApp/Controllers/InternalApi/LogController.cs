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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AdefHelpDeskBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using System.Text;
using ADefHelpDeskApp.Classes;
using System.IO;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;
using Microsoft.AspNetCore.Http;

namespace ADefHelpDeskApp.Controllers
{
    public class LogController
    {
        private IConfiguration _config { get; set; }

        public LogController(IConfiguration config)
        {
            _config = config;
        }

        #region public LogSearchResult Logs(SearchParameters searchData)
        public LogSearchResult Logs(SearchParameters searchData)
        {
            LogSearchResult objLogSearchResult = new LogSearchResult();
            objLogSearchResult.LogList = new List<DTOLog>();

            // Must be a Super Administrator to call this Method

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                int intTaskId = Convert.ToInt32(searchData.searchString);

                var QueryResult = (from Log in context.AdefHelpDeskLog
                                   where Log.TaskId == intTaskId
                                   select Log).OrderByDescending(l => l.LogId)
                                   .Skip(searchData.rowsPerPage * (searchData.pageNumber))
                                   .Take(searchData.rowsPerPage).ToList();

                List<DTOLog> colDTOLog = new List<DTOLog>();

                foreach (var item in QueryResult)
                {
                    // Get user
                    DTOUser objDTOUSer = UtilitySecurity.UserFromUserId(item.UserId, GetConnectionString());

                    // Create Log
                    DTOLog objDTOLog = new DTOLog();

                    objDTOLog.LogID = item.LogId;
                    objDTOLog.TaskID = item.TaskId;
                    objDTOLog.LogDescription = item.LogDescription;
                    objDTOLog.UserName = (objDTOUSer != null) ? objDTOUSer.userName : "[Unauthenticated]";
                    objDTOLog.DateCreated = item.DateCreated.ToShortDateString() + ' ' + item.DateCreated.ToShortTimeString();

                    colDTOLog.Add(objDTOLog);
                }

                objLogSearchResult.LogList = colDTOLog;
                objLogSearchResult.totalRows = context.AdefHelpDeskLog.Where(x => x.TaskId == intTaskId).Count();
                objLogSearchResult.errorMessage = string.Empty;
            }

            return objLogSearchResult;
        }
        #endregion

        // Utility

        #region private string GetConnectionString()
        private string GetConnectionString()
        {
            // Use this method to make sure we get the latest one
            string strConnectionString = "ERRROR:UNSET-CONECTION-STRING";

            try
            {
                strConnectionString = _config.GetConnectionString("DefaultConnection");
            }
            catch
            {
                // Do nothing
            }

            return strConnectionString;
        }
        #endregion
    }
}