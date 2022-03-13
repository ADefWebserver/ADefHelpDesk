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
using System.Linq.Dynamic.Core;

namespace ADefHelpDeskApp.Controllers
{
    public class SystemLogController
    {
        private IConfiguration _config { get; set; }

        public SystemLogController(IConfiguration config)
        {
            _config = config;
        }

        #region public SystemLogSearchResult SystemLogs(SearchParameters searchData)
        public SystemLogSearchResult SystemLogs(SearchParameters searchData)
        {
            SystemLogSearchResult objSystemLogSearchResult = new SystemLogSearchResult();

            // Must be a Super Administrator to call this Method

            return SystemLogsMethod(searchData, GetConnectionString());
        }
        #endregion

        // Methods

        #region public static SystemLogSearchResult SystemLogsMethod(SearchParameters searchData, string ConnectionString)
        public static SystemLogSearchResult SystemLogsMethod(SearchParameters searchData, string ConnectionString)
        {
            SystemLogSearchResult objSystemLogSearchResult = new SystemLogSearchResult();
            objSystemLogSearchResult.SystemLogList = new List<DTOSystemLog>();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var Query = from systemLog in context.AdefHelpDeskSystemLog
                            select systemLog;

                // filter
                if (!string.IsNullOrEmpty(searchData.searchString))
                {
                    Query = Query.Where(searchData.searchString.Replace(";", "").Replace("--", ""));
                }

                // sorting
                if (!string.IsNullOrEmpty(searchData.orderBy))
                {
                    Query = Query.OrderBy(searchData.orderBy.Replace(";", "").Replace("--", ""));
                }

                var QueryResult = (from systemLog in Query
                                   select systemLog)
                                   .Skip(searchData.rowsPerPage * (searchData.pageNumber))
                                   .Take(searchData.rowsPerPage)
                                   .ToList();

                List<DTOSystemLog> colDTOSystemLog = new List<DTOSystemLog>();

                foreach (var item in QueryResult)
                {
                    DTOSystemLog objDTOSystemLog = new DTOSystemLog();

                    objDTOSystemLog.LogID = item.LogId;
                    objDTOSystemLog.LogType = item.LogType;
                    objDTOSystemLog.LogMessage = item.LogMessage;
                    objDTOSystemLog.UserName = item.UserName ?? "";
                    objDTOSystemLog.CreatedDate = item.CreatedDate;

                    colDTOSystemLog.Add(objDTOSystemLog);
                }

                objSystemLogSearchResult.SystemLogList = colDTOSystemLog;
                objSystemLogSearchResult.totalRows = Query.Count();
                objSystemLogSearchResult.errorMessage = string.Empty;
            }

            return objSystemLogSearchResult;
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
