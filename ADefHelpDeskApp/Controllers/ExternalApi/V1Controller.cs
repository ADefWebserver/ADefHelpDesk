//
// ADefHelpDesk.com
// Copyright (c) 2018
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdefHelpDeskBase.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using AdefHelpDeskBase.CustomTokenProvider;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using AdefHelpDeskBase.JwtTokens;
using ADefHelpDeskApp.Classes;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using AdefHelpDeskBase.Models.DataContext;
using Microsoft.EntityFrameworkCore;
using ADefHelpDeskApp.Controllers;
using Microsoft.AspNetCore.Hosting;
using ADefHelpDeskApp.Controllers.ExternalApi.Classes;
using Microsoft.Extensions.Caching.Memory;
using ADefHelpDeskApp.Models;
using System.IO;

namespace AdefHelpDeskBase.Controllers.WebInterface
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class V1Controller : Controller
    {
        static HttpClient client = new HttpClient();
        private IMemoryCache _cache;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private string _SystemFiles;
        private IConfigurationRoot _configRoot { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configRoot"></param>
        /// <param name="hostEnvironment"></param>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="memoryCache"></param>
        public V1Controller(
            IConfigurationRoot configRoot,
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMemoryCache memoryCache
            )
        {
            _configRoot = configRoot;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
            _cache = memoryCache;

            // Set _SystemFiles 
            _SystemFiles =
                System.IO.Path.Combine(
                    hostEnvironment.ContentRootPath,
                    "SystemFiles");

            // Create SystemFiles directory if needed
            if (!Directory.Exists(_SystemFiles))
            {
                DirectoryInfo di =
                    Directory.CreateDirectory(_SystemFiles);
            }
        }

        // Auth Token

        #region public async Task<string> GetAuthToken([FromBody]ApiToken objApiToken)
        /// <summary>
        /// Obtain a security token to use for subsequent calls - copy the output received and then click the Authorize button (above). Paste the contents (between the quotes) into that box and then click Authorize then close. Now the remaining methods will work.
        /// </summary>
        /// <param name="objApiToken"></param>
        /// <response code="200">JWT token created</response>
        [AllowAnonymous]
        [Route("GetAuthToken")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "external")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<string> GetAuthToken([FromBody]ApiToken objApiToken)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("username", objApiToken.UserName);
            dict.Add("password", objApiToken.Password);
            dict.Add("applicationGUID", objApiToken.ApplicationGUID);

            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            HttpResponseMessage encodedJwt =
                await client.PostAsync($@"{CurrentHostLocation}/api/token",
                new FormUrlEncodedContent(dict));

            var jsonString = encodedJwt.Content.ReadAsStringAsync();
            jsonString.Wait();

            accessToken response = JsonConvert.DeserializeObject<accessToken>(jsonString.Result);

            if (response.authorized)
            {
                return $"Bearer {response.access_token}";
            }
            else
            {
                return $"ERROR: Not Authorized";
            }
        }
        #endregion

        // Current User

        #region public IActionResult GetCurrentUser()
        /// <summary>
        /// Get Current User 
        /// </summary>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetCurrentUser")]
        [ApiExplorerSettings(GroupName = "external")]
        public IActionResult GetCurrentUser()
        {
            string CurrentUser = "[Not Logged In]";

            if (this.User.Identity.IsAuthenticated)
            {
                CurrentUser = this.User.Claims.FirstOrDefault().Value;
            }

            return Ok(CurrentUser);
        }
        #endregion

        // Current Version

        #region public string GetCurrentVersion()
        /// <summary>
        /// Get Current Version 
        /// </summary>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetCurrentVersion")]
        [ApiExplorerSettings(GroupName = "external")]
        public string GetCurrentVersion()
        {
            // Version object to return
            string strVersion = "00.00.00";

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                try
                {
                    // There is actually a connection string
                    // Test it by trying to read the Version table
                    var result = (from version in context.AdefHelpDeskVersion
                                  orderby version.VersionNumber descending
                                  select version).FirstOrDefault();

                    // We have to find at least one Version record
                    if (result != null)
                    {
                        // Set Version number
                        strVersion = result.VersionNumber;
                    }
                }
                catch
                {
                    strVersion = "00.00.00";
                }
            }

            return strVersion;
        }
        #endregion

        // Dashboard

        #region public DTODashboard ShowDashboard()
        /// <summary>
        /// Show Dashboard 
        /// </summary>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("ShowDashboard")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTODashboard ShowDashboard()
        {
            string strConnectionString = GetConnectionString();
            return DashboardController.ShowDashboard(strConnectionString);
        }
        #endregion

        // Tasks

        #region public TaskSearchResult SearchTasks([FromBody]SearchParameters searchData)
        /// <summary>
        /// Search Tasks
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("SearchTasks")]
        [ApiExplorerSettings(GroupName = "external")]
        public TaskSearchResult SearchTasks([FromBody]SearchParameters searchData)
        {
            SearchTaskParameters objSearchTaskParameters = new SearchTaskParameters();
            objSearchTaskParameters.assignedRoleId = searchData.assignedRoleId;
            objSearchTaskParameters.createdDate = searchData.createdDate;
            objSearchTaskParameters.dueDate = searchData.dueDate;            
            objSearchTaskParameters.pageNumber = searchData.pageNumber;
            objSearchTaskParameters.priority = searchData.priority;
            objSearchTaskParameters.rowsPerPage = searchData.rowsPerPage;
            objSearchTaskParameters.searchText = searchData.searchText;
            objSearchTaskParameters.selectedTreeNodes = searchData.selectedTreeNodes;
            objSearchTaskParameters.sortField = searchData.sortField;
            objSearchTaskParameters.sortOrder = searchData.sortOrder;
            objSearchTaskParameters.status = searchData.status;
            objSearchTaskParameters.userId = "-1";
            objSearchTaskParameters.id = "-1";

            return TaskController.SearchTasks(objSearchTaskParameters, -1, 1, GetConnectionString());
        }
        #endregion

        #region public DTOStatus CreateTask(DTOTask objTask, IFormFile objFile)
        /// <summary>
        /// Create Task
        /// </summary>
        /// <param name="objTask"></param>
        /// <param name="objTaskDetail"></param>
        /// <param name="objFile"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateTask")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOStatus CreateTask(DTOAPITask objTask, DTOAPITaskDetail objTaskDetail, IFormFile objFile)
        {
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();
            int intUserId = -1;
            bool IsSuperUser = true;
            bool IsAdministrator = true;
            bool IsAuthenticated = true;

            try
            {
                DTOTask paramTask = ExternalAPIUtility.MapAPITaskToTask(objTask, objTaskDetail);

                objDTOStatus = UploadTaskController.CreateTaskMethod(
                    strConnectionString,
                    CurrentHostLocation,
                    ContentRootPath,
                    paramTask,
                    objFile,
                    strCurrentUser,
                    intUserId,
                    IsSuperUser,
                    IsAdministrator,
                    IsAuthenticated);
            }
            catch (Exception ex)
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
            }

            return objDTOStatus;
        }
        #endregion

        #region public DTOStatus UpdateTask(DTOTask objTask)
        /// <summary>
        /// Update Task
        /// </summary>
        /// <param name="objTask"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdateTask")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOStatus UpdateTask(DTOAPITask objTask)
        {
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();
            int intUserId = -1;
            bool IsAuthenticated = true;

            try
            {
                DTOTask paramTask = ExternalAPIUtility.MapAPITaskToTask(objTask, null);

                objDTOStatus = UploadTaskController.UpdateTaskMethod(
                    strConnectionString,
                    CurrentHostLocation,
                    ContentRootPath,
                    paramTask,
                    strCurrentUser,
                    intUserId,
                    IsAuthenticated);
            }
            catch (Exception ex)
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
            }

            return objDTOStatus;
        }
        #endregion

        #region public DTOStatus CreateUpdateTaskDetail(DTOTask objTask, IFormFile objFile)
        /// <summary>
        /// Create Update Task Detail
        /// </summary>
        /// <param name="objTask"></param>
        /// <param name="objTaskDetail"></param>
        /// <param name="objFile"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateUpdateTaskDetail")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOTaskDetailResponse CreateUpdateTaskDetail(DTOAPITask objTask, DTOAPITaskDetail objTaskDetail, IFormFile objFile)
        {
            DTOTaskDetailResponse objDTOStatus = new DTOTaskDetailResponse();
            objDTOStatus.isSuccess = true;
            objDTOStatus.message = "";
            objDTOStatus.taskDetail = new DTOTaskDetail();

            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();
            int intUserId = -1;
            bool IsSuperUser = true;
            bool IsAdministrator = true;
            bool IsAuthenticated = true;

            try
            {
                DTOTask paramTask = ExternalAPIUtility.MapAPITaskToTask(objTask, objTaskDetail);

                objDTOStatus = UploadTaskController.InsertUpdateTaskDetailMethod(
                    strConnectionString,
                    CurrentHostLocation,
                    ContentRootPath,
                    paramTask,
                    objFile,
                    strCurrentUser,
                    intUserId,
                    IsSuperUser,
                    IsAdministrator,
                    strCurrentUser,
                    IsAuthenticated);
            }
            catch (Exception ex)
            {
                objDTOStatus.isSuccess = false;
                objDTOStatus.message = ex.GetBaseException().Message;
            }

            return objDTOStatus;
        }
        #endregion

        #region public DTOTaskStatus GetTask(int TaskId)
        /// <summary>
        /// Get Task
        /// </summary>
        /// <param name="TaskId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetTask")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOTaskStatus GetTask(int TaskId)
        {
            DTOTaskStatus objDTOStatus = new DTOTaskStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();
            int intUserId = -1;
            bool IsAdministrator = true;
            bool IsAuthenticated = true;

            try
            {
                DTOTask obJDTOTask = new DTOTask();
                obJDTOTask.taskId = TaskId;
                obJDTOTask.ticketPassword = "";

                objDTOStatus.Task = TaskController.GetTask(
                    obJDTOTask, 
                    intUserId, 
                    IsAdministrator, 
                    strConnectionString, 
                    strCurrentUser, 
                    IsAuthenticated);
            }
            catch (Exception ex)
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
            }

            return objDTOStatus;
        }
        #endregion

        #region public DTOTaskStatus DeleteTask(int TaskId)
        /// <summary>
        /// Delete Task
        /// </summary>
        /// <param name="TaskId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteTask")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOTaskStatus DeleteTask(int TaskId)
        {
            DTOTaskStatus objDTOStatus = new DTOTaskStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();

            try
            {
                objDTOStatus.StatusMessage = TaskController.DeleteTask(
                    TaskId,
                    strConnectionString,
                    strCurrentUser);
            }
            catch (Exception ex)
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
            }

            return objDTOStatus;
        }
        #endregion

        #region public DTOTaskStatus DeleteTaskDetail(int TaskId)
        /// <summary>
        /// Delete Task Detail
        /// </summary>
        /// <param name="TaskId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteTaskDetail")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOTaskStatus DeleteTaskDetail(int TaskId)
        {
            DTOTaskStatus objDTOStatus = new DTOTaskStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();

            try
            {
                objDTOStatus.StatusMessage = TaskController.DeleteTaskDetail(
                    TaskId,
                    strConnectionString,
                    strCurrentUser);
            }
            catch (Exception ex)
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
            }

            return objDTOStatus;
        }
        #endregion

        // Users

        #region public UserSearchResult SearchUsers([FromBody]SearchParameters searchData)
        /// <summary>
        /// Search Users
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("SearchUsers")]
        [ApiExplorerSettings(GroupName = "external")]
        public UserSearchResult SearchUsers([FromBody]SearchUserParameters searchData)
        {
            AdefHelpDeskBase.Models.SearchParameters objSearchParameters = new Models.SearchParameters();
            objSearchParameters.pageNumber = searchData.pageNumber;
            objSearchParameters.rowsPerPage = searchData.rowsPerPage;
            objSearchParameters.searchString = searchData.searchString;

            return UserManagerController.SearchUsersMethod(objSearchParameters, GetConnectionString());
        }
        #endregion

        #region public DTOUser GetUser(int UserId)
        /// <summary>
        /// Get User
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetUser")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOUser GetUser(int UserId)
        {
            return UserManagerController.GetUserMethod(UserId, GetConnectionString());
        }
        #endregion

        #region public LoginStatus ValidateUser(DTOAuthentication Authentication)
        /// <summary>
        /// Validate User
        /// </summary>
        /// <param name="Authentication"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ValidateUser")]
        [ApiExplorerSettings(GroupName = "external")]
        public LoginStatus ValidateUser(DTOAuthentication Authentication)
        {
            // LoginStatus to return
            LoginStatus objLoginStatus = new LoginStatus();
            objLoginStatus.isLoggedIn = false;

            // Get values passed
            var paramUserName = Authentication.userName;
            var paramPassword = Authentication.password;

            if ((paramUserName != null) && (paramPassword != null))
            {
                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                // Only check the legacy User password if user is not in the main table
                if (_userManager.Users.Where(x => x.UserName == paramUserName).FirstOrDefault() == null)
                {
                    using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                    {
                        // First check the legacy User table
                        var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                                   where AdefHelpDeskUsers.Username == paramUserName
                                                   where AdefHelpDeskUsers.Password != ""
                                                   select AdefHelpDeskUsers).FirstOrDefault();

                        if (objAdefHelpDeskUser != null)
                        {
                            // User is in the Legacy table and the password is not null
                            // Check their password to see if this account can be migrated
                            if (objAdefHelpDeskUser.Password ==
                                ComputeHash.GetSwcMD5(paramUserName.Trim().ToLower() + paramPassword.Trim()))
                            {
                                // Return that this account can be migrated
                                objLoginStatus.status = "Migrate";
                                return objLoginStatus;
                            }
                        }
                    }
                }

                // Check to see if the user needs to Verify their account
                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                               where AdefHelpDeskUsers.Username == paramUserName
                                               select AdefHelpDeskUsers).FirstOrDefault();

                    if (objAdefHelpDeskUser != null)
                    {
                        if (objAdefHelpDeskUser.VerificationCode != null)
                        {
                            objLoginStatus.status = "Verify";
                            return objLoginStatus;
                        }
                    }
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = _signInManager.PasswordSignInAsync(
                    paramUserName,
                    paramPassword, false,
                    lockoutOnFailure: false).Result;

                if (result.Succeeded)
                {
                    objLoginStatus.status = "Success";
                    objLoginStatus.isLoggedIn = true;
                    return objLoginStatus;
                }
                if (result.RequiresTwoFactor)
                {
                    objLoginStatus.status = "RequiresVerification";
                    return objLoginStatus;
                }
                if (result.IsLockedOut)
                {
                    objLoginStatus.status = "IsLockedOut";
                    return objLoginStatus;
                }
            }

            objLoginStatus.status = "Authentication Failure";

            return objLoginStatus;
        }
        #endregion

        #region public LoginStatus MigrateUser(DTOAuthentication Authentication)
        /// <summary>
        /// Migrate User
        /// </summary>
        /// <param name="Migration"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("MigrateUser")]
        [ApiExplorerSettings(GroupName = "external")]
        public LoginStatus MigrateUser(DTOMigration Migration)
        {
            // LoginStatus to return
            LoginStatus objLoginStatus = new LoginStatus();
            objLoginStatus.isLoggedIn = false;

            if ((Migration.userName != null) && (Migration.password != null) && (Migration.passwordNew != null))
            {
                // Get values passed
                var paramUserName = Migration.userName;
                var paramPassword = ComputeHash.GetSwcMD5(paramUserName.Trim().ToLower() + Migration.password.Trim());
                var paramPasswordNew = Migration.passwordNew;

                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Must be in legacy User table
                    var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                               where AdefHelpDeskUsers.Username == paramUserName
                                               where AdefHelpDeskUsers.Password == paramPassword
                                               select AdefHelpDeskUsers).FirstOrDefault();

                    if (objAdefHelpDeskUser != null)
                    {
                        // Email Validation ****************************

                        if (objAdefHelpDeskUser.Email == null)
                        {
                            objLoginStatus.status = "The Email for this account is not valid. It cannot be migrated.";
                            objLoginStatus.isLoggedIn = false;
                            return objLoginStatus;
                        }

                        EmailValidation objEmailValidation = new EmailValidation();
                        if (!objEmailValidation.IsValidEmail(objAdefHelpDeskUser.Email))
                        {
                            objLoginStatus.status = "The Email for this account is not valid. It cannot be migrated.";
                            objLoginStatus.isLoggedIn = false;
                            return objLoginStatus;
                        }

                        // Migrate Account

                        var user = new ApplicationUser { UserName = paramUserName, Email = objAdefHelpDeskUser.Email };
                        var result = _userManager.CreateAsync(user, paramPasswordNew).Result;

                        if (result.Succeeded)
                        {
                            // Sign the User in
                            var SignInResult = _signInManager.PasswordSignInAsync(
                                paramUserName, paramPasswordNew, false, lockoutOnFailure: false).Result;

                            if (!SignInResult.Succeeded)
                            {
                                // Return the error
                                objLoginStatus.status = $"Could not sign user {paramUserName} in.";
                                objLoginStatus.isLoggedIn = false;
                                return objLoginStatus;
                            }
                            else
                            {
                                try
                                {
                                    // Everything worked
                                    // Update the users password in the legacy table
                                    objAdefHelpDeskUser.Password = ComputeHash.GetSwcMD5(paramUserName.Trim().ToLower() + paramPasswordNew.Trim());
                                    context.SaveChanges();
                                }
                                catch
                                {
                                    // Do nothing if this does not work
                                    // This password is only needed if connecting from the 
                                    // Non Angular version of ADefHelpDesk
                                }

                                // Return Success
                                objLoginStatus.status = $"User {paramUserName} signed in.";
                                objLoginStatus.isLoggedIn = true;
                                return objLoginStatus;
                            }
                        }
                        else
                        {
                            // Return the errors from the Memberhip API Creation
                            string strErrors = "";
                            foreach (var Error in result.Errors)
                            {
                                strErrors = strErrors + "\n" + Error.Description;
                            }

                            // Return the error
                            objLoginStatus.status = strErrors;
                            objLoginStatus.isLoggedIn = false;
                            return objLoginStatus;
                        }
                    }
                    else
                    {
                        objLoginStatus.status = "Orginal password does not match.";
                        return objLoginStatus;
                    }
                }
            }

            objLoginStatus.status = "Authentication Failure";

            return objLoginStatus;
        }
        #endregion

        #region public DTOStatus CreateUser([FromBody]DTOUser DTOUser)
        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="DTOUser"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateUser")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOStatus CreateUser([FromBody]DTOUser DTOUser)
        {
            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();

            return UserManagerController.CreateUserMethod(DTOUser, _hostEnvironment, _userManager, _signInManager, strConnectionString, CurrentHostLocation, strCurrentUser);
        }
        #endregion

        #region public DTOStatus UpdateUser([FromBody]DTOUser DTOUser)
        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="DTOUser"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdateUser")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOStatus UpdateUser([FromBody]DTOUser DTOUser)
        {
            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();

            return UserManagerController.UpdateUser(DTOUser.userId, DTOUser, _userManager, strConnectionString, strCurrentUser);
        }
        #endregion

        #region public DTOStatus DeleteUser(int UserId)
        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteUser")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOStatus DeleteUser(int UserId)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();

            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();

            string strResponse = UserManagerController.DeleteUser(UserId, _userManager, strConnectionString, strCurrentUser);

            if(strResponse != "")
            {
                objDTOStatus.StatusMessage = strResponse;
                objDTOStatus.Success = false;
            }
            else
            {
                objDTOStatus.Success = true;
            }

            return objDTOStatus;
        }
        #endregion

        // Categories

        #region public List<CategoryDTO> GetCategoryNodes([FromBody]bool UseCache)
        /// <summary>
        /// Get Category Nodes
        /// </summary>
        /// <param name="UseCache"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetCategoryNodes")]
        [ApiExplorerSettings(GroupName = "external")]
        public List<CategoryDTO> GetCategoryNodes([FromBody]bool UseCache)
        {
            return ADefHelpDeskApp.Controllers.WebApi.CategoryTreeController.GetNodesMethod(UseCache, _cache, GetConnectionString());
        }
        #endregion

        #region public CategoryNode CreateCategory([FromBody]CategoryNode categoryNode)
        /// <summary>
        /// Create Category
        /// </summary>
        /// <param name="categoryNode"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateCategory")]
        [ApiExplorerSettings(GroupName = "external")]
        public CategoryNode CreateCategory([FromBody]CategoryNode categoryNode)
        {
            // Get Settings
            string strConnectionString = GetConnectionString();

            return ADefHelpDeskApp.Controllers.WebApi.CategoryController.CreateCategory(categoryNode, strConnectionString);
        }
        #endregion

        #region public DTOStatus UpdateCategory([FromBody]CategoryNode categoryNode)
        /// <summary>
        /// Update Category
        /// </summary>
        /// <param name="categoryNode"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdateCategory")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOStatus UpdateCategory([FromBody]CategoryNode categoryNode)
        {
            // Get Settings
            string strConnectionString = GetConnectionString();

            return ADefHelpDeskApp.Controllers.WebApi.CategoryController.UpdateCategory(categoryNode.Id, categoryNode, strConnectionString);
        }
        #endregion

        #region public DTOStatus DeleteCategory(int id)
        /// <summary>
        /// Delete Category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteCategory")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOStatus DeleteCategory(int id)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();

            // Get Settings
            string strConnectionString = GetConnectionString();

            return ADefHelpDeskApp.Controllers.WebApi.CategoryController.DeleteCategory(id, strConnectionString);
        }
        #endregion

        //Roles

        #region public List<RoleDTO> GetRoles()
        /// <summary>
        /// Get Roles
        /// </summary>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetRoles")]
        [ApiExplorerSettings(GroupName = "external")]
        public List<RoleDTO> GetRoles()
        {
            return ADefHelpDeskApp.Controllers.WebApi.RoleController.GetRolesMethod(GetConnectionString());
        }
        #endregion

        #region public DTOStatus UpdateRole([FromBody]RoleDTO RoleDTO)
        /// <summary>
        /// Update Role
        /// </summary>
        /// <param name="RoleDTO"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdateRole")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOStatus UpdateRole([FromBody]RoleDTO RoleDTO)
        {
            return ADefHelpDeskApp.Controllers.WebApi.RoleController.UpdateRole(RoleDTO.iD, RoleDTO, GetConnectionString());
        }
        #endregion

        #region public RoleDTO CreateRole([FromBody]RoleDTO RoleDTO)
        /// <summary>
        /// Create Role
        /// </summary>
        /// <param name="RoleDTO"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateRole")]
        [ApiExplorerSettings(GroupName = "external")]
        public RoleDTO CreateRole([FromBody]RoleDTO RoleDTO)
        {
            return ADefHelpDeskApp.Controllers.WebApi.RoleController.CreateRole(RoleDTO, GetConnectionString());
        }
        #endregion

        #region public DTOStatus DeleteRole([FromBody]RoleDTO RoleDTO)
        /// <summary>
        /// Delete Role
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteRole")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOStatus DeleteRole([FromBody]int Id)
        {
            return ADefHelpDeskApp.Controllers.WebApi.RoleController.DeleteRole(Id, GetConnectionString());
        }
        #endregion

        // Files

        #region public DTONode SystemFiles()
        /// <summary>
        /// System Files 
        /// </summary>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("SystemFiles")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTONode SystemFiles()
        {
            return FilesController.SystemFilesMethod(_hostEnvironment,_SystemFiles);
        }
        #endregion

        #region public DTOResponse GetSystemFile([FromBody]DTONode paramDTONode)
        /// <summary>
        /// Get System File
        /// </summary>
        /// <param name="paramDTONode"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetSystemFile")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOResponse GetSystemFile([FromBody]DTONode paramDTONode)
        {
            return FilesController.GetFileContentMethod(paramDTONode, _SystemFiles);
        }
        #endregion

        #region public DTOFile GetFile([FromBody]DTOAPIFile paramDTOAPIFile)
        /// <summary>
        /// Get File
        /// </summary>
        /// <param name="paramDTOAPIFile"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetFile")]
        [ApiExplorerSettings(GroupName = "external")]
        public DTOFile GetFile([FromBody]DTOAPIFile paramDTOAPIFile)
        {
            DTOFileParameter paramDTOFileParameter = new DTOFileParameter();
            paramDTOFileParameter.attachmentID = paramDTOAPIFile.attachmentID;
            paramDTOFileParameter.detailId = paramDTOAPIFile.detailId;
            paramDTOFileParameter.emailFileName = paramDTOAPIFile.emailFileName;
            paramDTOFileParameter.portalId = paramDTOAPIFile.portalId;
            paramDTOFileParameter.taskId = paramDTOAPIFile.taskId;
            paramDTOFileParameter.ticketPassword = paramDTOAPIFile.ticketPassword;

            var fileResult = FilesController.ReturnFileMethod(paramDTOFileParameter, _SystemFiles, GetConnectionString());
            return fileResult;
        }
        #endregion

        // Logs

        #region public SystemLogSearchResult SystemLogs([FromBody]SearchLogParameters objSearchLogParameters)
        /// <summary>
        /// Search Logs
        /// </summary>
        /// <param name="objSearchLogParameters"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("SystemLogs")]
        [ApiExplorerSettings(GroupName = "external")]
        public SystemLogSearchResult SystemLogs([FromBody]SearchLogParameters objSearchLogParameters)
        {
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            AdefHelpDeskBase.Models.SearchParameters SearchParameters = new Models.SearchParameters();
            SearchParameters.searchString = objSearchLogParameters.searchString;
            SearchParameters.pageNumber = objSearchLogParameters.pageNumber;
            SearchParameters.rowsPerPage = objSearchLogParameters.rowsPerPage;

            return SystemLogController.SystemLogsMethod(SearchParameters, strCurrentUser, GetConnectionString());
        }
        #endregion

        // Utility

        #region private string GetConnectionString()
        private string GetConnectionString()
        {
            // Use this method to make sure we get the latest one
            string strConnectionString = "ERRROR:UNSET-CONNECTION-STRING";

            try
            {
                strConnectionString = _configRoot.GetConnectionString("DefaultConnection");
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

