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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdefHelpDeskBase.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
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
using ADefHelpDeskApp.Controllers.InternalApi;
using ADefHelpDeskApp.Jwt;

namespace AdefHelpDeskBase.Controllers.WebInterface
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class V1Controller : Controller
    {
        static HttpClient client = new HttpClient();
        private IMemoryCache _cache;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private string _SystemFiles;
        private IConfiguration _configuration { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JWTAuthenticationService _authenticationService;
            
        private readonly UploadTaskController _uploadTaskController;
        private readonly CategoryTreeController _categoryTreeController;

        /// <summary>
        /// External Controller
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="hostEnvironment"></param>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="memoryCache"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="authenticationService"></param>
        /// <param name="uploadTaskController"></param>
        /// <param name="categoryTreeController"></param>
        public V1Controller
            (
            IConfiguration configuration,
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMemoryCache memoryCache,
            IHttpContextAccessor httpContextAccessor,
            JWTAuthenticationService authenticationService,
            UploadTaskController uploadTaskController,
            CategoryTreeController categoryTreeController
            )
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
            _cache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            _authenticationService = authenticationService;
            _uploadTaskController = uploadTaskController;
            _categoryTreeController = categoryTreeController;
            
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

        #region public async Task<string> GetAuthToken([FromQuery] ApiToken objApiToken)
        /// <summary>
        /// Obtain a security token to use for subsequent calls - copy the output received and then click the Authorize button (above). Paste the contents (between the quotes) into that box and then click Authorize then close. Now the remaining methods will work.
        /// </summary>
        /// <param name="objApiToken"></param>
        /// <response code="200">JWT token created</response>
        [AllowAnonymous]
        [HttpGet("GetAuthToken")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<string> GetAuthToken([FromQuery] ApiToken objApiToken)
        {
            ApiToken AuthToken = new ApiToken();
            AuthToken.UserName = objApiToken.UserName;
            AuthToken.Password = objApiToken.Password;
            AuthToken.ApplicationGUID = objApiToken.ApplicationGUID;

            string access_token = await _authenticationService.Authenticate(AuthToken);

            bool authorized = (access_token.Length > 0);

            if (authorized)
            {
                return $"{access_token}";
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
        public IActionResult GetCurrentUser()
        {
            string CurrentUser = "[** Error **]";

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var UserName = this.User.Claims.Where(x => x.Type == "Username").FirstOrDefault();

                if (UserName != null)
                {
                    CurrentUser = UserName.Value;
                }
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
        public DTODashboard ShowDashboard()
        {
            string strConnectionString = GetConnectionString();
            return DashboardController.ShowDashboard(strConnectionString);
        }
        #endregion

        //Tasks

        #region public TaskSearchResult SearchTasks([FromBody] SearchParameters searchData)
        /// <summary>
        /// Search Tasks
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("SearchTasks")]
        public TaskSearchResult SearchTasks([FromBody] SearchParameters searchData)
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

        #region public DTOStatus CreateTask([FromForm] DTOAPITask objTask)
        /// <summary>
        /// Create Task
        /// </summary>
        /// <param name="objTask">Task</param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateTask")]
        public DTOStatus CreateTask([FromForm] DTOAPITask objTask)
        {
            var task = objTask;

            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.Where(x => x.Type == "Username").FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();
            int intUserId = -1;
            bool IsSuperUser = true;
            bool IsAdministrator = true;
            bool IsAuthenticated = true;

            try
            {
                DTOTask paramTask = ExternalAPIUtility.MapAPITaskToTask(objTask, new DTOAPITaskDetail());

                // Get file data (if any)      
                IFormFile objFile = null;
                if (objTask.fileattachment != null)
                {
                    // Note: We only allow one file   
                    objFile = objTask.fileattachment;
                }

                objDTOStatus = _uploadTaskController.CreateTaskMethod(strConnectionString, CurrentHostLocation, ContentRootPath, paramTask, objFile, strCurrentUser, intUserId, IsSuperUser, IsAdministrator, IsAuthenticated);
            }
            catch (Exception ex)
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
            }

            return objDTOStatus;
        }
        #endregion

        #region public DTOStatus UpdateTask([FromForm] DTOAPITask objTask)
        /// <summary>
        /// Update Task
        /// </summary>
        /// <param name="objTask"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdateTask")]
        public DTOStatus UpdateTask([FromForm] DTOAPITask objTask)
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

                objDTOStatus = _uploadTaskController.UpdateTaskMethod(
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

        #region public DTOTaskDetailResponse CreateUpdateTaskDetail([FromForm] DTOAPITaskDetail objDTOAPITaskDetail)
        /// <summary>
        /// Create Update Task Detail
        /// </summary>
        /// <param name="objDTOAPITaskDetail"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateUpdateTaskDetail")]
        public DTOTaskDetailResponse CreateUpdateTaskDetail([FromForm] DTOAPITaskDetail objDTOAPITaskDetail)
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
                // Set the TaskId and TicketPassword
                DTOAPITask objTask = new DTOAPITask() { taskId = objDTOAPITaskDetail.taskId, ticketPassword = objDTOAPITaskDetail.ticketPassword };

                DTOTask paramTask = ExternalAPIUtility.MapAPITaskToTask(objTask, objDTOAPITaskDetail);

                // Get file data (if any)      
                IFormFile objFile = null;
                if (objDTOAPITaskDetail.fileattachment != null)
                {
                    // Note: We only allow one file   
                    objFile = objDTOAPITaskDetail.fileattachment;
                }

                objDTOStatus = _uploadTaskController.InsertUpdateTaskDetailMethod(
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

        #region public DTOTaskStatus GetTask([FromQuery] int TaskId)
        /// <summary>
        /// Get Task
        /// </summary>
        /// <param name="TaskId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetTask")]
        public DTOTaskStatus GetTask([FromQuery] int TaskId)
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

        #region public DTOTaskStatus DeleteTask([FromQuery] int TaskId)
        /// <summary>
        /// Delete Task
        /// </summary>
        /// <param name="TaskId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteTask")]
        public DTOTaskStatus DeleteTask([FromQuery] int TaskId)
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

        #region public DTOTaskStatus DeleteTaskDetail([FromQuery] int TaskDetailId)
        /// <summary>
        /// Delete Task Detail
        /// </summary>
        /// <param name="TaskDetailId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteTaskDetail")]
        public DTOTaskStatus DeleteTaskDetail([FromQuery] int TaskDetailId)
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
                    TaskDetailId,
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

        #region public UserSearchResult SearchUsers([FromForm] SearchParameters searchData)
        /// <summary>
        /// Search Users
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("SearchUsers")]
        public UserSearchResult SearchUsers([FromForm] SearchUserParameters searchData)
        {
            AdefHelpDeskBase.Models.SearchParameters objSearchParameters = new Models.SearchParameters();
            objSearchParameters.pageNumber = searchData.pageNumber;
            objSearchParameters.rowsPerPage = searchData.rowsPerPage;
            objSearchParameters.searchString = searchData.searchString;

            return UserManagerController.SearchUsersMethod(objSearchParameters, GetConnectionString());
        }
        #endregion

        #region public DTOUser GetUser([FromQuery] int UserId)
        /// <summary>
        /// Get User
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetUser")]
        public DTOUser GetUser([FromQuery] int UserId)
        {
            return UserManagerController.GetUserMethod(UserId, GetConnectionString());
        }
        #endregion

        #region public LoginStatus ValidateUser([FromForm] DTOAuthentication Authentication)
        /// <summary>
        /// Validate User
        /// </summary>
        /// <param name="Authentication"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ValidateUser")]
        public LoginStatus ValidateUser([FromForm] DTOAuthentication Authentication)
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

        #region public LoginStatus MigrateUser([FromForm] DTOAuthentication Authentication)
        /// <summary>
        /// Migrate User
        /// </summary>
        /// <param name="Migration"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("MigrateUser")]
        public LoginStatus MigrateUser([FromForm] DTOMigration Migration)
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

        #region public DTOStatus CreateUser([FromForm] DTOUser DTOUser)
        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="DTOUser"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateUser")]
        public DTOStatus CreateUser([FromForm] DTOUser DTOUser)
        {
            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();

            UserManagerController objUserManagerController = new UserManagerController(_configuration, _hostEnvironment, _userManager, _signInManager);

            // Cannot Create a SuperUser
            DTOUser.isSuperUser = false;

            return objUserManagerController.CreateUserMethod(DTOUser, _hostEnvironment, _userManager, _signInManager, strConnectionString, CurrentHostLocation).Result;
        }
        #endregion

        #region public DTOStatus UpdateUser([FromForm] DTOUser DTOUser)
        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="DTOUser"></param>
        /// <returns>DTOStatus</returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdateUser")]
        public DTOStatus UpdateUser([FromForm] DTOUser DTOUser)
        {
            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();

            // Cannot Create a SuperUser
            DTOUser.isSuperUser = false;

            return UserManagerController.UpdateUser(DTOUser.userId, DTOUser, _userManager, strConnectionString, strCurrentUser);
        }
        #endregion

        #region public DTOStatus DeleteUser([FromQuery] int UserId)
        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteUser")]
        public DTOStatus DeleteUser([FromQuery] int UserId)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();

            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            string strConnectionString = GetConnectionString();

            string strResponse = UserManagerController.DeleteUser(UserId, _userManager, strConnectionString, strCurrentUser);

            if (strResponse != "")
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

        #region public List<CategoryDTO> GetCategoryNodes([FromQuery] bool RequestorVisibleOnly)
        /// <summary>
        /// Get Category Nodes
        /// </summary>
        /// <param name="RequestorVisibleOnly"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetCategoryNodes")]
        public List<CategoryDTO> GetCategoryNodes([FromQuery] bool RequestorVisibleOnly)
        {
            // Get Settings
            string strConnectionString = GetConnectionString();
            
            return _categoryTreeController.GetNodesMethod(RequestorVisibleOnly, false, _cache, new List<int>(), strConnectionString);
        }
        #endregion

        #region public CategoryNode CreateCategory([FromForm]CategoryNode categoryNode)
        /// <summary>
        /// Create Category
        /// </summary>
        /// <param name="categoryNode"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateCategory")]
        public CategoryNode CreateCategory([FromForm] CategoryNode categoryNode)
        {
            // Get Settings
            string strConnectionString = GetConnectionString();

            return ADefHelpDeskApp.Controllers.InternalApi.CategoryController.CreateCategory(categoryNode, strConnectionString);
        }
        #endregion

        #region public DTOStatus UpdateCategory([FromForm] CategoryNode categoryNode)
        /// <summary>
        /// Update Category
        /// </summary>
        /// <param name="categoryNode"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdateCategory")]
        public DTOStatus UpdateCategory([FromForm] CategoryNode categoryNode)
        {
            // Get Settings
            string strConnectionString = GetConnectionString();

            return ADefHelpDeskApp.Controllers.InternalApi.CategoryController.UpdateCategory(categoryNode.Id, categoryNode, strConnectionString);
        }
        #endregion

        #region public DTOStatus DeleteCategory([FromQuery] int id)
        /// <summary>
        /// Delete Category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteCategory")]
        public DTOStatus DeleteCategory([FromQuery] int id)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();

            // Get Settings
            string strConnectionString = GetConnectionString();

            return ADefHelpDeskApp.Controllers.InternalApi.CategoryController.DeleteCategory(id, strConnectionString);
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
        public List<RoleDTO> GetRoles()
        {
            return ADefHelpDeskApp.Controllers.InternalApi.RoleController.GetRolesMethod(GetConnectionString());
        }
        #endregion

        #region public DTOStatus UpdateRole([FromForm] RoleDTO RoleDTO)
        /// <summary>
        /// Update Role
        /// </summary>
        /// <param name="RoleDTO"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdateRole")]

        public DTOStatus UpdateRole([FromForm] RoleDTO RoleDTO)
        {
            return ADefHelpDeskApp.Controllers.InternalApi.RoleController.UpdateRole(RoleDTO.iD, RoleDTO, GetConnectionString());
        }
        #endregion

        #region public RoleDTO CreateRole([FromForm] RoleDTO RoleDTO)
        /// <summary>
        /// Create Role
        /// </summary>
        /// <param name="RoleDTO"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("CreateRole")]

        public RoleDTO CreateRole([FromForm] RoleDTO RoleDTO)
        {
            return ADefHelpDeskApp.Controllers.InternalApi.RoleController.CreateRole(RoleDTO, GetConnectionString());
        }
        #endregion

        #region public DTOStatus DeleteRole([FromForm] RoleDTO RoleDTO)
        /// <summary>
        /// Delete Role
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("DeleteRole")]
        public DTOStatus DeleteRole([FromForm] int Id)
        {
            return ADefHelpDeskApp.Controllers.InternalApi.RoleController.DeleteRole(Id, GetConnectionString());
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
        public DTONode SystemFiles()
        {
            //return FilesController.SystemFilesMethod(_hostEnvironment,_SystemFiles);
            return new DTONode();
        }
        #endregion

        #region public DTOResponse GetSystemFile([FromQuery] DTONode paramDTONode)
        /// <summary>
        /// Get System File
        /// </summary>
        /// <param name="paramDTONode"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetSystemFile")]
        public DTOResponse GetSystemFile([FromQuery] DTONode paramDTONode)
        {
            // return FilesController.GetFileContentMethod(paramDTONode, _SystemFiles);
            return new DTOResponse();
        }
        #endregion

        #region public DTOFile GetFile([FromForm] DTOAPIFile paramDTOAPIFile)
        /// <summary>
        /// Get File
        /// </summary>
        /// <param name="paramDTOAPIFile"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetFile")]

        public DTOFile GetFile([FromForm] DTOAPIFile paramDTOAPIFile)
        {
            DTOFileParameter paramDTOFileParameter = new DTOFileParameter();
            paramDTOFileParameter.attachmentID = paramDTOAPIFile.attachmentID;
            paramDTOFileParameter.detailId = paramDTOAPIFile.detailId;
            paramDTOFileParameter.emailFileName = paramDTOAPIFile.emailFileName;
            paramDTOFileParameter.portalId = paramDTOAPIFile.portalId;
            paramDTOFileParameter.taskId = paramDTOAPIFile.taskId;
            paramDTOFileParameter.ticketPassword = paramDTOAPIFile.ticketPassword;

            //var fileResult = FilesController.ReturnFileMethod(paramDTOFileParameter, _SystemFiles, GetConnectionString());
            //return fileResult;

            return new DTOFile();
        }
        #endregion

        // Logs

        #region public SystemLogSearchResult SystemLogs([FromForm] SearchLogParameters objSearchLogParameters)
        /// <summary>
        /// Search Logs
        /// </summary>
        /// <param name="objSearchLogParameters"></param>
        /// <returns></returns>
        // JwtBearerDefaults means this method will only work if a Jwt is being passed
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("SystemLogs")]
        public SystemLogSearchResult SystemLogs([FromForm] SearchLogParameters objSearchLogParameters)
        {
            //string strCurrentUser = this.User.Claims.FirstOrDefault().Value;
            AdefHelpDeskBase.Models.SearchParameters SearchParameters = new Models.SearchParameters();
            SearchParameters.searchString = objSearchLogParameters.searchString;
            SearchParameters.pageNumber = objSearchLogParameters.pageNumber;
            SearchParameters.rowsPerPage = objSearchLogParameters.rowsPerPage;

            return SystemLogController.SystemLogsMethod(SearchParameters, GetConnectionString());
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
                strConnectionString = _configuration.GetConnectionString("DefaultConnection");
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

