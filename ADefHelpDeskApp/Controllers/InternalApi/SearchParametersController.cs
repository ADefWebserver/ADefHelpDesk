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
    public class SearchParametersController
    {
        private IConfiguration _config { get; set; }

        public SearchParametersController(IConfiguration config)
        {
            _config = config;
        }

        #region public SearchTaskParameters Index(string CurrentUserName)
        public SearchTaskParameters Index(string CurrentUserName)
        {
            // SearchTaskParameters to return
            // Set important defaults
            SearchTaskParameters objSearchTaskParameters = new SearchTaskParameters();
            objSearchTaskParameters.priority = "";
            objSearchTaskParameters.searchText = "";
            objSearchTaskParameters.status = "";
            objSearchTaskParameters.pageNumber = 1;
            objSearchTaskParameters.rowsPerPage = 10;

            // Get UserId
            int UserId = UtilitySecurity.UserIdFromUserName(CurrentUserName, GetConnectionString());

            // Get the LastSearch
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var objLastSearch = context.AdefHelpDeskLastSearch
                    .Where(x => x.UserId == UserId)
                    .FirstOrDefault();

                if (objLastSearch != null)
                {
                    objSearchTaskParameters.id = objLastSearch.Id.ToString();
                    objSearchTaskParameters.userId = objLastSearch.UserId.ToString();
                    objSearchTaskParameters.assignedRoleId = objLastSearch.AssignedRoleId != null ? objLastSearch.AssignedRoleId.ToString() : "";
                    objSearchTaskParameters.searchText = objLastSearch.SearchText != null ? objLastSearch.SearchText.ToString() : "";
                    objSearchTaskParameters.status = objLastSearch.Status != null ? objLastSearch.Status.ToString() : "";
                    objSearchTaskParameters.createdDate = objLastSearch.CreatedDate != null ? objLastSearch.CreatedDate.Value.ToShortDateString() : "";
                    objSearchTaskParameters.dueDate = objLastSearch.DueDate != null ? objLastSearch.DueDate.Value.ToShortDateString() : "";
                    objSearchTaskParameters.priority = objLastSearch.Priority != null ? objLastSearch.Priority.ToString() : "";
                    objSearchTaskParameters.pageNumber = objLastSearch.CurrentPage ?? -1;
                    objSearchTaskParameters.rowsPerPage = objLastSearch.PageSize ?? -1;

                    // Categories (selectedTreeNodes)
                    objSearchTaskParameters.selectedTreeNodes = new List<int>();

                    if (objLastSearch.Categories != null)
                    {
                        string[] Categories = objLastSearch.Categories.Split(",");
                        foreach (var Category in Categories)
                        {
                            if (Category.Trim().Length > 0)
                            {
                                objSearchTaskParameters.selectedTreeNodes.Add(Convert.ToInt32(Category));
                            }
                        }
                    }
                }
            }

            // Return the result
            return objSearchTaskParameters;
        }
        #endregion

        #region public DTOStatus SaveSearchParameters(SearchTaskParameters paramSearchTaskParameters,string CurrentUserName)
        public DTOStatus SaveSearchParameters(SearchTaskParameters paramSearchTaskParameters, string CurrentUserName)
        {
            try
            {
                // Get UserId
                int UserId = UtilitySecurity.UserIdFromUserName(CurrentUserName, GetConnectionString());

                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // First remove any existing records for the user
                    context.AdefHelpDeskLastSearch.RemoveRange(context.AdefHelpDeskLastSearch.Where(x => x.UserId == UserId));
                    context.SaveChanges();

                    AdefHelpDeskLastSearch objNewLastSearch = new AdefHelpDeskLastSearch();

                    objNewLastSearch.UserId = UserId;
                    objNewLastSearch.SearchText = (paramSearchTaskParameters.searchText != null) ? paramSearchTaskParameters.searchText.Trim() : "";
                    objNewLastSearch.Status = (paramSearchTaskParameters.status != null) ? paramSearchTaskParameters.status.Trim() : "";
                    objNewLastSearch.Priority = (paramSearchTaskParameters.priority != null) ? paramSearchTaskParameters.priority.Trim() : "";
                    objNewLastSearch.CurrentPage = paramSearchTaskParameters.pageNumber;
                    objNewLastSearch.PageSize = paramSearchTaskParameters.rowsPerPage;

                    if (paramSearchTaskParameters.userId != null)
                    {
                        objNewLastSearch.UserId = Convert.ToInt32(paramSearchTaskParameters.userId);
                    }

                    if (paramSearchTaskParameters.assignedRoleId != null)
                    {
                        objNewLastSearch.AssignedRoleId = Convert.ToInt32(paramSearchTaskParameters.assignedRoleId);
                    }

                    if (paramSearchTaskParameters.createdDate != null)
                    {
                        objNewLastSearch.CreatedDate = Utility.CastToDate(paramSearchTaskParameters.createdDate);
                    }

                    if (paramSearchTaskParameters.dueDate != null)
                    {
                        objNewLastSearch.DueDate = Utility.CastToDate(paramSearchTaskParameters.dueDate);
                    }

                    objNewLastSearch.Categories = string.Join(",", paramSearchTaskParameters.selectedTreeNodes);

                    // Save changes
                    context.AdefHelpDeskLastSearch.Add(objNewLastSearch);
                    context.SaveChanges();
                }
            }
            catch
            {
                // Do nothing if search paramaters cannot be saved
            }

            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = $"No Content";
            objDTOStatus.Success = false;
            return objDTOStatus;
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
