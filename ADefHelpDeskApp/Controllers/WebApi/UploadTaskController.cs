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
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AdefHelpDeskBase.Models.DataContext;
using MimeKit;

namespace ADefHelpDeskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class UploadTaskController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;        
        private IConfigurationRoot _configRoot { get; set; }

        public UploadTaskController(
            IConfigurationRoot configRoot,
            IWebHostEnvironment hostEnvironment)
        {
            _configRoot = configRoot;
            _hostEnvironment = hostEnvironment;

            #region Settings
            // Set FileUploadPath
            GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());

            if (
                objGeneralSettings.FileUploadPath != null
                && objGeneralSettings.FileUploadPath.Trim().Length > 0
                )
            {
                // Create FileUploadPath directory if needed
                if (!Directory.Exists(objGeneralSettings.FileUploadPath))
                {
                    try
                    {
                        DirectoryInfo di =
                            Directory.CreateDirectory(objGeneralSettings.FileUploadPath);
                    }
                    catch (Exception)
                    {
                        // Do nothing
                    }
                }
            }
            #endregion
        }

        // ************* TASK

        // POST: api/UploadTask/CreateTask
        [HttpPost("[action]")]
        #region public IActionResult CreateTask([FromForm] ICollection<IFormFile> files)
        public IActionResult CreateTask([FromForm] ICollection<IFormFile> files)
        {
            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Identity.Name;
            string strConnectionString = GetConnectionString();
            int intUserId = UtilitySecurity.UserIdFromUserName(strCurrentUser, strConnectionString);
            bool IsSuperUser = UtilitySecurity.IsSuperUser(strCurrentUser, strConnectionString);
            bool IsAdministrator = UtilitySecurity.IsAdministrator(strCurrentUser, strConnectionString);
            bool IsAuthenticated = this.User.Identity.IsAuthenticated;

            // Get file data (if any)      
            IFormFile objFile = null;
            if (Request.Form.Files.Count > 0)
            {
                // Note: We only allow one file   
                objFile = Request.Form.Files[0];
            }

            // Retrieve data from FormData
            var objTaskForm = Request.Form["task"].First();
            DTOTask objTask = JsonConvert.DeserializeObject<DTOTask>(objTaskForm);

            return Ok(CreateTaskMethod(strConnectionString, CurrentHostLocation, ContentRootPath, objTask, objFile, strCurrentUser, intUserId, IsSuperUser, IsAdministrator, IsAuthenticated));
        }
        #endregion

        // POST: api/UploadTask/UpdateTask
        [HttpPost("[action]")]
        [Authorize]
        #region public IActionResult UpdateTask([FromBody]DTOTask objTask)
        public IActionResult UpdateTask([FromBody]DTOTask objTask)
        {
            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Identity.Name;
            string strConnectionString = GetConnectionString();
            int intUserId = UtilitySecurity.UserIdFromUserName(strCurrentUser, strConnectionString);
            bool IsSuperUser = UtilitySecurity.IsSuperUser(strCurrentUser, strConnectionString);
            bool IsAdministrator = UtilitySecurity.IsAdministrator(strCurrentUser, strConnectionString);
            bool IsAuthenticated = this.User.Identity.IsAuthenticated;

            // Must be a Administrator to call this Method
            if (!UtilitySecurity.IsAdministrator(strCurrentUser, strConnectionString))
            {
                DTOStatus objDTOStatus = new DTOStatus();
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = "Must be an Administrator to call this Method";
                return Ok(objDTOStatus);
            }

            return Ok(UpdateTaskMethod(strConnectionString, CurrentHostLocation, ContentRootPath, objTask, strCurrentUser, intUserId, IsAuthenticated));
        }
        #endregion

        // ************* TASK DETAIL

        // POST: api/UploadTask/InsertUpdateTaskDetail
        [HttpPost("[action]")]
        #region public IActionResult InsertUpdateTaskDetail([FromForm] ICollection<IFormFile> files)
        public IActionResult InsertUpdateTaskDetail([FromForm] ICollection<IFormFile> files)
        {
            // Get Settings
            string CurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string ContentRootPath = _hostEnvironment.ContentRootPath;
            string strCurrentUser = this.User.Identity.Name;
            string strConnectionString = GetConnectionString();
            int intUserId = UtilitySecurity.UserIdFromUserName(strCurrentUser, strConnectionString);
            bool IsSuperUser = UtilitySecurity.IsSuperUser(strCurrentUser, strConnectionString);
            bool IsAdministrator = UtilitySecurity.IsAdministrator(strCurrentUser, strConnectionString);
            bool IsAuthenticated = this.User.Identity.IsAuthenticated;
            string strLogUserName = (IsAuthenticated) ? strCurrentUser : "[Unauthenticated]";
            
            // Get file data (if any)      
            IFormFile objFile = null;
            if (Request.Form.Files.Count > 0)
            {
                // Note: We only allow one file   
                objFile = Request.Form.Files[0];
            }

            // Retrieve data from FormData            
            var objTaskForm = Request.Form["task"].First();
            DTOTask objTask = JsonConvert.DeserializeObject<DTOTask>(objTaskForm);

            return Ok(InsertUpdateTaskDetailMethod(strConnectionString, CurrentHostLocation, ContentRootPath, objTask, objFile, strCurrentUser, intUserId, IsSuperUser, IsAdministrator, strLogUserName, IsAuthenticated));
        }
        #endregion

        // Emails

        #region private static void NotifyAssignedGroup(DTOTask objTask, string NotificationType, string strCurrentUser, string CurrentHostLocation, string ConnectionString, string ContentRootPath)
        private static void NotifyAssignedGroup(DTOTask objTask, string NotificationType, string strCurrentUser, string CurrentHostLocation, string ConnectionString, string ContentRootPath)
        {
            // Uses objTask.assignedRoleId.Value to send emails to all Administrator's in that role
            GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);
            DTOUser objCurrentDTOUser = UtilitySecurity.UserFromUserName(strCurrentUser, ConnectionString);
            string strAssignedRole = UtilitySecurity.RoleNameForRoleId(objTask.assignedRoleId.Value, ConnectionString);

            // Set Subject
            string strSubject = "";
            if (NotificationType == Constants.NotifyNewTask)
            {
                strSubject = $"A Help Desk ticket has been assigned to your Role (#{objTask.taskId})";
            }

            if (NotificationType == Constants.NotifyUpdateTaskDetail)
            {
                strSubject = $"A Help Desk ticket assigned to your Role has been updated (#{objTask.taskId})";
            }

            List<DTOUser> colUsers = new List<DTOUser>();
            // Get all users in the AssignedRole Role
            colUsers = UtilitySecurity.UsersForRoleId(objTask.assignedRoleId.Value, ConnectionString);

            // If there are no assigned users, notify the Superusers
            if (colUsers.Count == 0)
            {
                // Get all SuperUsers
                colUsers = UtilitySecurity.SuperUsers(ConnectionString);

                // Set the Assigned Role
                strAssignedRole = "SuperUsers";
            }

            foreach (DTOUser objDTOUser in colUsers)
            {
                // Do not send email to the user triggering this email
                if (objDTOUser.userName == strCurrentUser)
                {
                    continue;
                }

                // Send user the Email
                string strFullName = $"{objDTOUser.firstName} {objDTOUser.lastName}";
                string strEmailContents = "";

                if (NotificationType == Constants.NotifyNewTask)
                {
                    strEmailContents = System.IO.File.ReadAllText(ContentRootPath + $@"\SystemFiles\Email-Admin-NewTask.txt");
                    strEmailContents = strEmailContents.Replace("[strFullName]", strFullName);
                    strEmailContents = strEmailContents.Replace("[strAssignedRole]", strAssignedRole);
                    strEmailContents = strEmailContents.Replace("[objTask.taskId.Value]", objTask.taskId.Value.ToString());
                    strEmailContents = strEmailContents.Replace("[objTask.description]", objTask.description);
                    strEmailContents = strEmailContents.Replace("[objTask.status]", objTask.status);
                    strEmailContents = strEmailContents.Replace("[objTask.priority]", objTask.priority);
                    strEmailContents = strEmailContents.Replace("[CurrentHostLocation]", CurrentHostLocation);
                    strEmailContents = strEmailContents.Replace("[objTask.taskId]", objTask.taskId.ToString());
                }

                if (NotificationType == Constants.NotifyUpdateTaskDetail)
                {
                    strEmailContents = System.IO.File.ReadAllText(ContentRootPath + $@"\SystemFiles\Email-Admin-UpdateTask.txt");
                    strEmailContents = strEmailContents.Replace("[strFullName]", strFullName);
                    strEmailContents = strEmailContents.Replace("[strAssignedRole]", strAssignedRole);
                    strEmailContents = strEmailContents.Replace("[objTask.taskId.Value]", objTask.taskId.Value.ToString());
                    strEmailContents = strEmailContents.Replace("[objTask.colDTOTaskDetail.FirstOrDefault().detailId]", objTask.colDTOTaskDetail.FirstOrDefault().detailId.ToString());
                    strEmailContents = strEmailContents.Replace("[objTask.description]", objTask.description);
                    strEmailContents = strEmailContents.Replace("[objTask.status]", objTask.status);
                    strEmailContents = strEmailContents.Replace("[objTask.priority]", objTask.priority);
                    strEmailContents = strEmailContents.Replace("[objTask.colDTOTaskDetail.FirstOrDefault().insertDate]", objTask.colDTOTaskDetail.FirstOrDefault().insertDate);
                    strEmailContents = strEmailContents.Replace("[objTask.colDTOTaskDetail.FirstOrDefault().description]", objTask.colDTOTaskDetail.FirstOrDefault().description);
                    strEmailContents = strEmailContents.Replace("[CurrentHostLocation]", CurrentHostLocation);
                    strEmailContents = strEmailContents.Replace("[objTask.taskId]", objTask.taskId.Value.ToString());
                }

                // Send Email                
                string smtpStatus = Email.SendMail(
                    true, // Send Async
                    ConnectionString,
                    objDTOUser.email,
                    strFullName,
                    "", "",
                    objGeneralSettings.SMTPFromEmail,
                    GetHeader(objTask.taskId, objTask.ticketPassword),
                    strSubject,
                    $"{strEmailContents} <br><br> This Email was sent from: {CurrentHostLocation}.");

                if (smtpStatus != "")
                {
                    // There was some sort of error - log it
                    string strErrorMessage =
                        $"Error: Cannot send NotifyAssignedGroupOfAssignment Email: {smtpStatus}";

                    // Log to the System Log
                    Log.InsertSystemLog(
                        ConnectionString,
                        Constants.EmailError,
                        strCurrentUser,
                        strErrorMessage);
                }
                else
                {
                    string strMessage = "";
                    if (NotificationType == Constants.NotifyNewTask)
                    {
                        strMessage = $"Help Desk ticket has been assigned to {strFullName} ({objDTOUser.userName}) ";
                    }
                    if (NotificationType == Constants.NotifyUpdateTaskDetail)
                    {
                        strMessage = $"Help Desk ticket updated notification for {strFullName} ({objDTOUser.userName}) ";
                    }

                    strMessage = strMessage + $"for Role ({strAssignedRole}) by {objCurrentDTOUser.firstName} {objCurrentDTOUser.lastName} ({objCurrentDTOUser.userName}).";

                    Log.InsertLog(
                        ConnectionString,
                        objTask.taskId.Value,
                        UtilitySecurity.UserIdFromUserName(strCurrentUser, ConnectionString),
                        strMessage
                        );
                }
            }
        }
        #endregion

        #region private static void NotifySuperUsers(DTOTask objTask, string strCurrentUser, string CurrentHostLocation, string ConnectionString, string ContentRootPath)
        private static void NotifySuperUsers(DTOTask objTask, string strCurrentUser, string CurrentHostLocation, string ConnectionString, string ContentRootPath)
        {
            GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);
            DTOUser objCurrentDTOUser = UtilitySecurity.UserFromUserName(strCurrentUser, ConnectionString);

            // Get all SuperUsers
            List<DTOUser> colUsers = UtilitySecurity.SuperUsers(ConnectionString);

            foreach (DTOUser objDTOUser in colUsers)
            {
                // Do not send email to the user triggering this email
                if (objDTOUser.userName == strCurrentUser)
                {
                    continue;
                }

                // Send user the Email
                string strFullName = $"{objDTOUser.firstName} {objDTOUser.lastName}";

                string strEmailContents = System.IO.File.ReadAllText(ContentRootPath + $@"\SystemFiles\Email-SuperUser-NewTask.txt");
                strEmailContents = strEmailContents.Replace("[strFullName]", strFullName);
                strEmailContents = strEmailContents.Replace("[objTask.taskId.Value]", objTask.taskId.Value.ToString());
                strEmailContents = strEmailContents.Replace("[objTask.description]", objTask.description);
                strEmailContents = strEmailContents.Replace("[objTask.status]", objTask.status);
                strEmailContents = strEmailContents.Replace("[objTask.priority]", objTask.priority);
                strEmailContents = strEmailContents.Replace("[CurrentHostLocation]", CurrentHostLocation);
                strEmailContents = strEmailContents.Replace("[objTask.taskId]", objTask.taskId.Value.ToString());

                // Send Email                
                string smtpStatus = Email.SendMail(
                    true, // Send Async
                    ConnectionString,
                    objDTOUser.email,
                    strFullName,
                    "", "",
                    objGeneralSettings.SMTPFromEmail,
                    GetHeader(objTask.taskId, objTask.ticketPassword),
                    $"A New Help Desk ticket has been created (#{objTask.taskId})",
                    $"{strEmailContents} <br><br> This Email was sent from: {CurrentHostLocation}.");

                if (smtpStatus != "")
                {
                    // There was some sort of error - log it
                    string strErrorMessage =
                        $"Error: Cannot send NotifySuperUsers Email: {smtpStatus}";

                    // Log to the System Log
                    Log.InsertSystemLog(
                        ConnectionString,
                        Constants.EmailError,
                        strCurrentUser,
                        strErrorMessage);
                }
            }
        }
        #endregion

        #region private static void NotifyUser(DTOTask objTask, string NotificationType, string strCurrentUser, string CurrentHostLocation, string ConnectionString, string ContentRootPath)
        private static void NotifyUser(DTOTask objTask, string NotificationType, string strCurrentUser, string CurrentHostLocation, string ConnectionString, string ContentRootPath)
        {
            GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);
            string strFullName = "";

            DTOUser objDTOUser = new DTOUser();
            // If ticket is assigned to a user, get the user information
            if (objTask.requesterUserId.HasValue)
            {
                if (objTask.requesterUserId.Value > 0)
                {
                    objDTOUser = UtilitySecurity.UserFromUserId(objTask.requesterUserId.Value, ConnectionString);
                    if (objDTOUser.userName == null)
                    {
                        // User not found - was deleted
                        Log.InsertSystemLog(
                            ConnectionString,
                            Constants.EmailError,
                            strCurrentUser,
                            $"Requester #{objTask.requesterUserId.Value} for Task #{objTask.taskId} not found.");

                        return;
                    }
                    else
                    {
                        strFullName = $"{objDTOUser.firstName} {objDTOUser.lastName}";
                    }
                }
                else
                {
                    // Build from Ticket details
                    strFullName = objTask.requesterName;
                    objDTOUser.email = objTask.requesterEmail;
                }
            }
            else
            {
                // Build from Ticket details
                strFullName = objTask.requesterName;
                objDTOUser.email = objTask.requesterEmail;
            }

            // Set Subject
            string strSubject = "";
            if (NotificationType == Constants.NotifyNewTask)
            {
                strSubject = $"A New Help Desk ticket has been created (#{objTask.taskId})";
            }

            if (NotificationType == Constants.NotifyUpdateTaskDetail)
            {
                strSubject = $"A Help Desk ticket has been updated (#{objTask.taskId})";
            }

            // Send user the Email
            string strEmailContents = "";
            if (NotificationType == Constants.NotifyNewTask)
            {
                strEmailContents = System.IO.File.ReadAllText(ContentRootPath + $@"\SystemFiles\Email-User-NewTask.txt");
                strEmailContents = strEmailContents.Replace("[strFullName]", strFullName);
                strEmailContents = strEmailContents.Replace("[objTask.taskId.Value]", objTask.taskId.Value.ToString());
                strEmailContents = strEmailContents.Replace("[objTask.description]", objTask.description);
                strEmailContents = strEmailContents.Replace("[objTask.status]", objTask.status);
                strEmailContents = strEmailContents.Replace("[objTask.priority]", objTask.priority);
                strEmailContents = strEmailContents.Replace("[CurrentHostLocation]", CurrentHostLocation);
                strEmailContents = strEmailContents.Replace("[objTask.taskId]", objTask.taskId.Value.ToString());
                strEmailContents = strEmailContents.Replace("[objTask.ticketPassword]", objTask.ticketPassword);
            }

            if (NotificationType == Constants.NotifyUpdateTaskDetail)
            {
                strEmailContents = System.IO.File.ReadAllText(ContentRootPath + $@"\SystemFiles\Email-User-UpdateTask.txt");
                strEmailContents = strEmailContents.Replace("[strFullName]", strFullName);
                strEmailContents = strEmailContents.Replace("[objTask.taskId.Value]", objTask.taskId.Value.ToString());
                strEmailContents = strEmailContents.Replace("[objTask.colDTOTaskDetail.FirstOrDefault().detailId]", objTask.colDTOTaskDetail.FirstOrDefault().detailId.ToString());
                strEmailContents = strEmailContents.Replace("[objTask.description]", objTask.description);
                strEmailContents = strEmailContents.Replace("[objTask.status]", objTask.status);
                strEmailContents = strEmailContents.Replace("[objTask.priority]", objTask.priority);
                strEmailContents = strEmailContents.Replace("[objTask.colDTOTaskDetail.FirstOrDefault().insertDate]", objTask.colDTOTaskDetail.FirstOrDefault().insertDate);
                strEmailContents = strEmailContents.Replace("[objTask.colDTOTaskDetail.FirstOrDefault().description]", objTask.colDTOTaskDetail.FirstOrDefault().description);
                strEmailContents = strEmailContents.Replace("[CurrentHostLocation]", CurrentHostLocation);
                strEmailContents = strEmailContents.Replace("[objTask.taskId]", objTask.taskId.Value.ToString());
                strEmailContents = strEmailContents.Replace("[objTask.ticketPassword]", objTask.ticketPassword);
            }

            // Send Email                
            string smtpStatus = Email.SendMail(
                true, // Send Async
                ConnectionString,
                objDTOUser.email,
                strFullName,
                "", "",
                objGeneralSettings.SMTPFromEmail,
                GetHeader(objTask.taskId, objTask.ticketPassword),
                strSubject,
                $"{strEmailContents} <br><br> This Email was sent from: {CurrentHostLocation}.");

            if (smtpStatus != "")
            {
                // There was some sort of error - log it
                string strErrorMessage =
                    $"Error: Cannot send NotifyUser Email: {smtpStatus}";

                // Log to the System Log
                Log.InsertSystemLog(
                    ConnectionString,
                    Constants.EmailError,
                    strCurrentUser,
                    strErrorMessage);
            }
        }
        #endregion

        // Methods

        #region public static DTOStatus CreateTaskMethod(string ConnectionString, string CurrentHostLocation, string ContentRootPath, DTOTask objTask, IFormFile objFile, string strCurrentUser, int intUserId, bool IsSuperUser, bool IsAdministrator, bool IsAuthenticated)
        public static DTOStatus CreateTaskMethod(string ConnectionString, string CurrentHostLocation, string ContentRootPath, DTOTask objTask, IFormFile objFile, string strCurrentUser, int intUserId, bool IsSuperUser, bool IsAdministrator, bool IsAuthenticated)
        {
            GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);

            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";
            string strUploadedFileName = "";
            bool boolSendEmails = true;
            bool CanUpload = Directory.Exists(objGeneralSettings.FileUploadPath);

            #region **** FORM VALIDATION ****
            // If the user is not logged in they must have a name and an email address
            if (!IsAuthenticated)
            {
                if (objTask.requesterName == null || objTask.requesterName.Trim().Length == 0)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = "Name is required";
                    return objDTOStatus;
                }

                EmailValidation objEmailValidation = new EmailValidation();
                if (objTask.requesterEmail == null ||
                    objTask.requesterEmail.Trim().Length == 0 ||
                    !objEmailValidation.IsValidEmail(objTask.requesterEmail))
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = "Valid Email is required";
                    return objDTOStatus;
                }
            }

            // Description is always required
            if (objTask.description == null || objTask.description.Trim().Length == 0)
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = "Description is required";
                return objDTOStatus;
            }
            #endregion

            // Set Send Emails
            if (objTask.sendEmails.HasValue)
            {
                boolSendEmails = objTask.sendEmails.Value;
            }

            #region **** FILE UPLOAD VALIDATION **** (Only allow user to Upload if Settings allow)
            // Only check if user is attempting to upload a file
            if (objFile != null)
            {
                #region Is User able to upload a file?
                if (
                    ((objGeneralSettings.UploadPermission == @"Administrator/Registered Users") && (IsAuthenticated == false))
                    ||
                    ((objGeneralSettings.UploadPermission == @"Administrator") && (!(IsSuperUser || IsAdministrator)))
                    )
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = $"Only users in the role(s): {objGeneralSettings.UploadPermission} can upload files.";
                    return objDTOStatus;
                }
                #endregion

                #region Validate file type
                if (!Utility.ValidateFileExtension(Path.GetExtension(objFile.FileName)))
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = $"Only .png, .gif, .jpg, .jpeg, .doc, .docx, .xls, .xlsx, .pdf, .zip, .txt, .mp3, .htm, .eml, .html files may be used.";
                    return objDTOStatus;
                }
                #endregion

                #region Can files be physically uploaded?
                if (!CanUpload) // This is set when the class is instantiated
                {
                    string strErrorMessage =
                        $"Error: Cannot upload to file path at: {objGeneralSettings.FileUploadPath}";

                    // Log to the System Log
                    Log.InsertSystemLog(
                        ConnectionString,
                        Constants.FileWriteError,
                        strCurrentUser,
                        strErrorMessage);

                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = strErrorMessage;
                    return objDTOStatus;
                }
                #endregion
            }
            #endregion

            // ***********  SAVE DATA

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                try
                {
                    AdefHelpDeskTasks objAdefHelpDeskTasks = new AdefHelpDeskTasks();
                    AdefHelpDeskLog objAdefHelpDeskLog = new AdefHelpDeskLog();

                    try
                    {
                        #region **** Create Task record 
                        objAdefHelpDeskTasks.CreatedDate = DateTime.Now;
                        objAdefHelpDeskTasks.Description = objTask.description;
                        objAdefHelpDeskTasks.DueDate = Utility.CastToDate(objTask.dueDate);
                        objAdefHelpDeskTasks.EstimatedCompletion = Utility.CastToDate(objTask.estimatedCompletion);
                        objAdefHelpDeskTasks.EstimatedHours = objTask.estimatedHours;
                        objAdefHelpDeskTasks.EstimatedStart = Utility.CastToDate(objTask.estimatedStart);
                        objAdefHelpDeskTasks.PortalId = Utility.CastNullableIntegerToPossibleNegativeOneInteger(objTask.portalId);
                        objAdefHelpDeskTasks.Priority = objTask.priority;
                        objAdefHelpDeskTasks.RequesterUserId = Utility.CastNullableIntegerToPossibleNegativeOneInteger(objTask.requesterUserId);
                        objAdefHelpDeskTasks.RequesterEmail = objTask.requesterEmail;
                        objAdefHelpDeskTasks.RequesterName = objTask.requesterName;
                        objAdefHelpDeskTasks.RequesterPhone = objTask.requesterPhone;
                        objAdefHelpDeskTasks.Status = objTask.status; // Active, Cancelled, New, On Hold, Resolved
                        objAdefHelpDeskTasks.TicketPassword = Utility.CreateRandomKey(10);

                        // Can only be set if the user is an Administrator
                        if (IsAdministrator)
                        {
                            objAdefHelpDeskTasks.AssignedRoleId = Utility.CastNullableIntegerToPossibleNegativeOneInteger(objTask.assignedRoleId);                            
                        }
                        else
                        {
                            objAdefHelpDeskTasks.AssignedRoleId = -1;
                        }

                        context.AdefHelpDeskTasks.Add(objAdefHelpDeskTasks);
                        context.SaveChanges();

                        // Update objTask 
                        objTask.taskId = objAdefHelpDeskTasks.TaskId;
                        objTask.ticketPassword = objAdefHelpDeskTasks.TicketPassword;

                        if (boolSendEmails)
                        {
                            // If Assigned role has been set send emails
                            if (objTask.assignedRoleId > -1)
                            {
                                NotifyAssignedGroup(objTask, Constants.NotifyNewTask, strCurrentUser, CurrentHostLocation, ConnectionString, ContentRootPath);
                            }
                            else
                            {
                                // A SuperUser will need to assign this ticket
                                NotifySuperUsers(objTask, strCurrentUser, CurrentHostLocation, ConnectionString, ContentRootPath);
                            }

                            // Notify the user
                            NotifyUser(objTask, Constants.NotifyNewTask, strCurrentUser, CurrentHostLocation, ConnectionString, ContentRootPath);
                        }
                        #endregion

                        #region **** Save to the Log
                        string strLogUserName = (IsAuthenticated) ? strCurrentUser : "[Unauthenticated]";
                        Log.InsertLog(ConnectionString, objAdefHelpDeskTasks.TaskId, intUserId, $"{strLogUserName} created ticket.");
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        #region **** Handle Exception
                        string strErrorMessage =
                            $"Error: Cannot create task '{objTask.description}': {ex.GetBaseException().Message}";

                        // Log to the System Log
                        Log.InsertSystemLog(
                            ConnectionString,
                            Constants.TaskCreationError,
                            strCurrentUser,
                            strErrorMessage);

                        objDTOStatus.Success = false;
                        objDTOStatus.StatusMessage = strErrorMessage;
                        return objDTOStatus;
                        #endregion
                    }

                    // **** Create Task Details record

                    // Only create if there is a file or Task Details
                    if ((objFile != null) || TaskDetailsRequired(objTask))
                    {
                        #region ***** Upload File
                        if (objFile != null) // Only if we have a file to upload
                        {
                            try
                            {
                                // Construct unique file name
                                strUploadedFileName = $@"\{objAdefHelpDeskTasks.TaskId}_{DateTime.Now.ToShortDateString()}-{DateTime.Now.ToLongTimeString()}";
                                strUploadedFileName = strUploadedFileName + $"-{Utility.CreateRandomKey(10)}";
                                strUploadedFileName = strUploadedFileName + $"{Path.GetExtension(objFile.FileName)}";
                                strUploadedFileName = strUploadedFileName.Replace(",", "").ToString();
                                strUploadedFileName = strUploadedFileName.Replace(" ", "").ToString();
                                strUploadedFileName = strUploadedFileName.Replace(":", "").ToString();
                                strUploadedFileName = strUploadedFileName.Replace(@"\", "").ToString();
                                strUploadedFileName = strUploadedFileName.Replace(@"/", "").ToString();

                                FileUtility.UploadFile(objFile, strUploadedFileName, ConnectionString);
                            }
                            catch (Exception ex)
                            {
                                string strErrorMessage =
                                    $"Error: Cannot upload file '{strUploadedFileName}': {ex.GetBaseException().Message}";

                                // Log to the System Log
                                Log.InsertSystemLog(
                                    ConnectionString,
                                    Constants.FileWriteError,
                                    strCurrentUser,
                                    strErrorMessage);

                                try
                                {
                                    // Try to delete the Task
                                    context.AdefHelpDeskLog.Remove(objAdefHelpDeskLog);
                                    context.SaveChanges();
                                    context.AdefHelpDeskTasks.Remove(objAdefHelpDeskTasks);
                                    context.SaveChanges();
                                }
                                catch (Exception ex2)
                                {
                                    // Log to the System Log
                                    Log.InsertSystemLog(
                                        ConnectionString,
                                        Constants.FileWriteError,
                                        strCurrentUser,
                                        ex2.Message);
                                }

                                objDTOStatus.Success = false;
                                objDTOStatus.StatusMessage = strErrorMessage;
                                return objDTOStatus;
                            }
                        }
                        #endregion
                    }

                    #region ******** Create Task Detail record
                    if ((objFile != null) || TaskDetailsRequired(objTask))
                    {
                        if ((!TaskWorkDetailsRequired(objTask)) || (objFile != null))
                        {
                            // Comment - Visible
                            AdefHelpDeskTaskDetails objAdefHelpDeskTaskDetails = new AdefHelpDeskTaskDetails();

                            objAdefHelpDeskTaskDetails.Task = objAdefHelpDeskTasks; // Associate with the parent Task
                            objAdefHelpDeskTaskDetails.Description = objTask.colDTOTaskDetail[0].description;
                            objAdefHelpDeskTaskDetails.DetailType = "Comment - Visible"; // Work, Comment, Comment - Visible
                            objAdefHelpDeskTaskDetails.InsertDate = DateTime.Now;
                            objAdefHelpDeskTaskDetails.UserId = intUserId;

                            context.AdefHelpDeskTaskDetails.Add(objAdefHelpDeskTaskDetails);
                            context.SaveChanges();

                            // Associate file (if there is one)
                            if (objFile != null)
                            {
                                AdefHelpDeskAttachments objAdefHelpDeskAttachments = new AdefHelpDeskAttachments();

                                objAdefHelpDeskAttachments.Detail = objAdefHelpDeskTaskDetails; // Associate with the parent TaskDetails
                                objAdefHelpDeskAttachments.FileName = strUploadedFileName;
                                objAdefHelpDeskAttachments.OriginalFileName = objFile.FileName;
                                objAdefHelpDeskAttachments.AttachmentPath = $@"{objGeneralSettings.FileUploadPath}\";
                                objAdefHelpDeskAttachments.UserId = intUserId;

                                context.AdefHelpDeskAttachments.Add(objAdefHelpDeskAttachments);
                                context.SaveChanges();
                            }
                        }

                        if (TaskWorkDetailsRequired(objTask))
                        {
                            // Work
                            AdefHelpDeskTaskDetails objAdefHelpDeskTaskDetailsWork = new AdefHelpDeskTaskDetails();

                            objAdefHelpDeskTaskDetailsWork.Task = objAdefHelpDeskTasks; // Associate with the parent Task
                            objAdefHelpDeskTaskDetailsWork.Description = objTask.colDTOTaskDetail[0].description;
                            objAdefHelpDeskTaskDetailsWork.DetailType = "Work"; // Work, Comment, Comment - Visible
                            objAdefHelpDeskTaskDetailsWork.InsertDate = DateTime.Now;
                            objAdefHelpDeskTaskDetailsWork.StartTime = Utility.CastToDate(objTask.colDTOTaskDetail[0].startTime);
                            objAdefHelpDeskTaskDetailsWork.StopTime = Utility.CastToDate(objTask.colDTOTaskDetail[0].stopTime);
                            objAdefHelpDeskTaskDetailsWork.UserId = intUserId;

                            context.AdefHelpDeskTaskDetails.Add(objAdefHelpDeskTaskDetailsWork);
                            context.SaveChanges();
                        }
                    }
                    #endregion

                    #region **** Save Categories (if any)
                    if (objTask.selectedTreeNodes != null)
                    {
                        foreach (var intTreeNode in objTask.selectedTreeNodes)
                        {
                            AdefHelpDeskTaskCategories objAdefHelpDeskTaskCategories = new AdefHelpDeskTaskCategories();
                            objAdefHelpDeskTaskCategories.Task = objAdefHelpDeskTasks;
                            objAdefHelpDeskTaskCategories.CategoryId = intTreeNode;

                            context.AdefHelpDeskTaskCategories.Add(objAdefHelpDeskTaskCategories);
                        }

                        context.SaveChanges();
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                }
            }

            // Return the status
            return objDTOStatus;
        }        
        #endregion

        #region public static DTOStatus UpdateTaskMethod(string ConnectionString, string CurrentHostLocation, string ContentRootPath, DTOTask objTask, string strCurrentUser, int intUserId, bool IsAuthenticated)
        public static DTOStatus UpdateTaskMethod(string ConnectionString, string CurrentHostLocation, string ContentRootPath, DTOTask objTask, string strCurrentUser, int intUserId, bool IsAuthenticated)
        {
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);
            int intOrginalAssignmentRole = -1;
            bool boolSendEmails = true;
            bool CanUpload = Directory.Exists(objGeneralSettings.FileUploadPath);

            // Set Send Emails
            if (objTask.sendEmails.HasValue)
            {
                boolSendEmails = objTask.sendEmails.Value;
            }

            // ***********  SAVE DATA

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                try
                {
                    AdefHelpDeskTasks objAdefHelpDeskTasks = new AdefHelpDeskTasks();
                    AdefHelpDeskLog objAdefHelpDeskLog = new AdefHelpDeskLog();

                    if (objTask.taskId != null) // New Record (Insert)
                    {
                        try
                        {
                            #region **** Update Task record 
                            objAdefHelpDeskTasks = (from task in context.AdefHelpDeskTasks
                                                    where task.TaskId == objTask.taskId
                                                    select task).FirstOrDefault();

                            if (objAdefHelpDeskTasks != null)
                            {
                                // Get intOrginalAssignmentRole to determine if Assigned users will need to be notified
                                intOrginalAssignmentRole = objAdefHelpDeskTasks.AssignedRoleId;

                                // Set values
                                objAdefHelpDeskTasks.Description = objTask.description;
                                objAdefHelpDeskTasks.DueDate = Utility.CastToDate(objTask.dueDate);
                                objAdefHelpDeskTasks.EstimatedCompletion = Utility.CastToDate(objTask.estimatedCompletion);
                                objAdefHelpDeskTasks.EstimatedHours = objTask.estimatedHours;
                                objAdefHelpDeskTasks.EstimatedStart = Utility.CastToDate(objTask.estimatedStart);
                                objAdefHelpDeskTasks.PortalId = Utility.CastNullableIntegerToPossibleNegativeOneInteger(objTask.portalId);
                                objAdefHelpDeskTasks.Priority = objTask.priority;
                                objAdefHelpDeskTasks.RequesterEmail = objTask.requesterEmail;
                                objAdefHelpDeskTasks.RequesterName = objTask.requesterName;
                                objAdefHelpDeskTasks.RequesterPhone = objTask.requesterPhone;
                                objAdefHelpDeskTasks.Status = objTask.status; // Active, Cancelled, New, On Hold, Resolved
                                objAdefHelpDeskTasks.AssignedRoleId = Utility.CastNullableIntegerToPossibleNegativeOneInteger(objTask.assignedRoleId);
                                objAdefHelpDeskTasks.RequesterUserId = Utility.CastNullableIntegerToPossibleNegativeOneInteger(objTask.requesterUserId);

                                context.SaveChanges();
                                #endregion

                                // Notify Assigned users of ticket being assigned to them
                                if (intOrginalAssignmentRole != Utility.CastNullableIntegerToPossibleNegativeOneInteger(objTask.assignedRoleId))
                                {
                                    if (boolSendEmails)
                                    {
                                        NotifyAssignedGroup(objTask, Constants.NotifyNewTask, strCurrentUser, CurrentHostLocation, ConnectionString, ContentRootPath);
                                    }
                                }

                                #region **** Save to the Log
                                string strLogUserName = (IsAuthenticated) ? strCurrentUser : "[Unauthenticated]";
                                Log.InsertLog(ConnectionString, objAdefHelpDeskTasks.TaskId, intUserId, $"{strLogUserName} updated ticket.");
                                #endregion
                            }
                            else
                            {
                                objDTOStatus.Success = false;
                                objDTOStatus.StatusMessage = $"Task #{objTask.taskId} Not Found";
                                return objDTOStatus;
                            }
                        }
                        catch (Exception ex)
                        {
                            #region **** Handle Exception
                            string strErrorMessage =
                                $"Error: Cannot create task '{objTask.description}': {ex.GetBaseException().Message}";

                            // Log to the System Log
                            Log.InsertSystemLog(
                                ConnectionString,
                                Constants.TaskCreationError,
                                strCurrentUser,
                                strErrorMessage);

                            objDTOStatus.Success = false;
                            objDTOStatus.StatusMessage = strErrorMessage;
                            return objDTOStatus;
                            #endregion
                        }
                    }

                    #region **** Save Categories (if any)
                    if (objTask.selectedTreeNodes != null)
                    {
                        // First delete any TreeNodes
                        var TaskCategories = (from TaskCategory in context.AdefHelpDeskTaskCategories
                                              where TaskCategory.TaskId == objTask.taskId
                                              select TaskCategory);

                        foreach (var item in TaskCategories)
                        {
                            context.AdefHelpDeskTaskCategories.Remove(item);
                        }

                        context.SaveChanges();

                        // Add new TreeNodes
                        objAdefHelpDeskTasks = (from task in context.AdefHelpDeskTasks
                                                .Include(categories => categories.AdefHelpDeskTaskCategories)
                                                where task.TaskId == objTask.taskId
                                                select task).FirstOrDefault();

                        foreach (var intTreeNode in objTask.selectedTreeNodes)
                        {
                            AdefHelpDeskTaskCategories objAdefHelpDeskTaskCategories = new AdefHelpDeskTaskCategories();
                            objAdefHelpDeskTaskCategories.Task = objAdefHelpDeskTasks;
                            objAdefHelpDeskTaskCategories.CategoryId = intTreeNode;

                            context.AdefHelpDeskTaskCategories.Add(objAdefHelpDeskTaskCategories);
                        }

                        context.SaveChanges();
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                }
            }

            // Return the status
            return objDTOStatus;
        }
        #endregion

        #region public static DTOTaskDetailResponse InsertUpdateTaskDetailMethod(string ConnectionString, string CurrentHostLocation, string ContentRootPath, DTOTask objTask, IFormFile objFile, string strCurrentUser, int intUserId, bool IsSuperUser, bool IsAdministrator, string strLogUserName, bool IsAuthenticated)
        public static DTOTaskDetailResponse InsertUpdateTaskDetailMethod(string ConnectionString, string CurrentHostLocation, string ContentRootPath, DTOTask objTask, IFormFile objFile, string strCurrentUser, int intUserId, bool IsSuperUser, bool IsAdministrator, string strLogUserName, bool IsAuthenticated)
        {
            DTOTaskDetailResponse objDTOTaskDetailResponse = new DTOTaskDetailResponse();
            objDTOTaskDetailResponse.isSuccess = true;
            objDTOTaskDetailResponse.message = "";
            objDTOTaskDetailResponse.taskDetail = new DTOTaskDetail();

            GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);
            string strUploadedFileName = "";
            bool boolSendTaskEmails = true;
            bool boolSendTaskDetailEmail = true;
            bool CanUpload = Directory.Exists(objGeneralSettings.FileUploadPath); 

            // Set Send Task Emails
            if (objTask.sendEmails.HasValue)
            {
                boolSendTaskEmails = objTask.sendEmails.Value;
            }

            #region **** FILE UPLOAD VALIDATION **** (Only allow user to Upload if Settings allow)
            // Only check if user is attempting to upload a file
            if (objFile != null)
            {
                #region Is User able to upload a file?
                if (
                    ((objGeneralSettings.UploadPermission == @"Administrator/Registered Users") && (IsAuthenticated == false))
                    ||
                    ((objGeneralSettings.UploadPermission == @"Administrator") && (!(IsSuperUser || IsAdministrator)))
                    )
                {
                    objDTOTaskDetailResponse.isSuccess = false;
                    objDTOTaskDetailResponse.message = $"Only users in the role(s): {objGeneralSettings.UploadPermission} can upload files.";
                    return objDTOTaskDetailResponse;
                }
                #endregion

                #region Validate file type
                if (!Utility.ValidateFileExtension(Path.GetExtension(objFile.FileName)))
                {
                    objDTOTaskDetailResponse.isSuccess = false;
                    objDTOTaskDetailResponse.message = $"Only .png, .gif, .jpg, .jpeg, .doc, .docx, .xls, .xlsx, .pdf, .zip, .txt, .mp3, .htm, .eml, .html files may be used.";
                    return objDTOTaskDetailResponse;
                }
                #endregion

                #region Can files be physically uploaded?
                if (!CanUpload) 
                {
                    string strErrorMessage =
                        $"Error: Cannot upload to file path at: {objGeneralSettings.FileUploadPath}";

                    // Log to the System Log
                    Log.InsertSystemLog(
                        ConnectionString,
                        Constants.FileWriteError,
                        strCurrentUser,
                        strErrorMessage);

                    objDTOTaskDetailResponse.isSuccess = false;
                    objDTOTaskDetailResponse.message = strErrorMessage;
                    return objDTOTaskDetailResponse;
                }
                #endregion
            }
            #endregion

            // ***********  SAVE DATA

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                try
                {
                    AdefHelpDeskTasks objAdefHelpDeskTask = new AdefHelpDeskTasks();
                    AdefHelpDeskTaskDetails objAdefHelpDeskTaskDetails = new AdefHelpDeskTaskDetails();
                    AdefHelpDeskAttachments objNewAdefHelpDeskAttachments = new AdefHelpDeskAttachments();
                    AdefHelpDeskLog objAdefHelpDeskLog = new AdefHelpDeskLog();

                    try
                    {
                        // Get using ticketPassword because user may not be logged in
                        objAdefHelpDeskTask = (from task in context.AdefHelpDeskTasks
                                               .Include(details => details.AdefHelpDeskTaskDetails)
                                               where task.TaskId == objTask.taskId
                                               where task.TicketPassword == objTask.ticketPassword
                                               select task).FirstOrDefault();

                        if (objAdefHelpDeskTask != null)
                        {
                            // Must pass at least one AdefHelpDeskTaskDetail record
                            var paramAdefHelpDeskTaskDetails = objTask.colDTOTaskDetail.FirstOrDefault();
                            if (paramAdefHelpDeskTaskDetails == null)
                            {
                                objDTOTaskDetailResponse.isSuccess = false;
                                objDTOTaskDetailResponse.message = "There is no AdefHelpDeskTaskDetail record to be updated.";
                                return objDTOTaskDetailResponse;
                            }

                            // Set Email Flag
                            if (paramAdefHelpDeskTaskDetails.sendEmails.HasValue)
                            {
                                boolSendTaskDetailEmail = paramAdefHelpDeskTaskDetails.sendEmails.Value;
                            }

                            // Add vaules to Task needed later for emails
                            objTask.assignedRoleId = objAdefHelpDeskTask.AssignedRoleId;
                            objTask.requesterName = objAdefHelpDeskTask.RequesterName;
                            objTask.requesterEmail = objAdefHelpDeskTask.RequesterEmail;
                            objTask.requesterUserId = objAdefHelpDeskTask.RequesterUserId;
                            objTask.description = objAdefHelpDeskTask.Description;
                            objTask.status = objAdefHelpDeskTask.Status;
                            objTask.priority = objAdefHelpDeskTask.Priority;

                            #region ***** Upload File
                            if (objFile != null) // Only if we have a file to upload
                            {
                                try
                                {
                                    // Process file
                                    // Construct unique file name
                                    strUploadedFileName = $@"\{objAdefHelpDeskTask.TaskId}_{DateTime.Now.ToShortDateString()}-{DateTime.Now.ToLongTimeString()}";
                                    strUploadedFileName = strUploadedFileName + $"-{Utility.CreateRandomKey(10)}";
                                    strUploadedFileName = strUploadedFileName + $"{Path.GetExtension(objFile.FileName)}";
                                    strUploadedFileName = strUploadedFileName.Replace(",", "").ToString();
                                    strUploadedFileName = strUploadedFileName.Replace(" ", "").ToString();
                                    strUploadedFileName = strUploadedFileName.Replace(":", "").ToString();
                                    strUploadedFileName = strUploadedFileName.Replace(@"\", "").ToString();
                                    strUploadedFileName = strUploadedFileName.Replace(@"/", "").ToString();

                                    FileUtility.UploadFile(objFile, strUploadedFileName, ConnectionString);
                                }
                                catch (Exception ex)
                                {
                                    string strErrorMessage =
                                        $"Error: Cannot upload file '{strUploadedFileName}': {ex.GetBaseException().Message}";

                                    // Log to the System Log
                                    Log.InsertSystemLog(
                                        ConnectionString,
                                        Constants.FileWriteError,
                                        strCurrentUser,
                                        strErrorMessage);

                                    objDTOTaskDetailResponse.isSuccess = false;
                                    objDTOTaskDetailResponse.message = strErrorMessage;
                                    return objDTOTaskDetailResponse;
                                }
                            }
                            #endregion

                            if (paramAdefHelpDeskTaskDetails.detailId == -1)
                            {
                                #region **** Insert Task Detail record 
                                // Only one TaskDetail can be passed to be inserted

                                objAdefHelpDeskTaskDetails.Task = objAdefHelpDeskTask;
                                objAdefHelpDeskTaskDetails.Description = paramAdefHelpDeskTaskDetails.description;
                                objAdefHelpDeskTaskDetails.UserId = intUserId;
                                objAdefHelpDeskTaskDetails.InsertDate = DateTime.Now;

                                #region Attach File
                                if (objFile != null)
                                {
                                    objNewAdefHelpDeskAttachments.Detail = objAdefHelpDeskTaskDetails;
                                    objNewAdefHelpDeskAttachments.AttachmentPath = objGeneralSettings.FileUploadPath + @"\";
                                    objNewAdefHelpDeskAttachments.FileName = strUploadedFileName;
                                    objNewAdefHelpDeskAttachments.OriginalFileName = objFile.FileName;
                                    objNewAdefHelpDeskAttachments.UserId = UtilitySecurity.UserIdFromUserName(strCurrentUser, ConnectionString);
                                    objAdefHelpDeskTaskDetails.AdefHelpDeskAttachments.Add(objNewAdefHelpDeskAttachments);

                                    // If file type is .EML set the TaskType to EML
                                    if (Path.GetExtension(objFile.FileName).ToUpper() == Constants.EML)
                                    {
                                        objAdefHelpDeskTaskDetails.ContentType = Constants.EML.Replace(".", "");
                                        objAdefHelpDeskTaskDetails.Description = SetEmailContents(objFile);
                                    }
                                }
                                #endregion

                                // Only an Admin can insert the following fields
                                if (IsAdministrator)
                                {
                                    objAdefHelpDeskTaskDetails.DetailType = paramAdefHelpDeskTaskDetails.detailType;

                                    if (paramAdefHelpDeskTaskDetails.startTime != "")
                                    {
                                        objAdefHelpDeskTaskDetails.StartTime = Utility.CastToDate(paramAdefHelpDeskTaskDetails.startTime);
                                    }

                                    if (paramAdefHelpDeskTaskDetails.stopTime != "")
                                    {
                                        objAdefHelpDeskTaskDetails.StopTime = Utility.CastToDate(paramAdefHelpDeskTaskDetails.stopTime);
                                    }
                                }
                                else
                                {
                                    // A non-admin can only create a customer visible comment
                                    objAdefHelpDeskTaskDetails.DetailType = "Comment - Visible";
                                }

                                context.AdefHelpDeskTaskDetails.Add(objAdefHelpDeskTaskDetails);
                                context.SaveChanges();

                                // Set return values

                                // If file type is .EML send back email conetnts
                                if (objFile != null)
                                {
                                    if (Path.GetExtension(objFile.FileName).ToUpper() == Constants.EML)
                                    {
                                        paramAdefHelpDeskTaskDetails.description = objAdefHelpDeskTaskDetails.Description;
                                    }
                                }
                                paramAdefHelpDeskTaskDetails.detailId = objAdefHelpDeskTaskDetails.DetailId;
                                paramAdefHelpDeskTaskDetails.insertDate = objAdefHelpDeskTaskDetails.InsertDate.ToLongDateString() + " " + objAdefHelpDeskTaskDetails.InsertDate.ToLongTimeString();
                                paramAdefHelpDeskTaskDetails.detailType = objAdefHelpDeskTaskDetails.DetailType;
                                paramAdefHelpDeskTaskDetails.contentType = objAdefHelpDeskTaskDetails.ContentType;

                                if (objAdefHelpDeskTaskDetails.StartTime.HasValue)
                                {
                                    paramAdefHelpDeskTaskDetails.startTime = objAdefHelpDeskTaskDetails.StartTime.Value.ToShortDateString() + " " + objAdefHelpDeskTaskDetails.StartTime.Value.ToLongTimeString();
                                }
                                if (objAdefHelpDeskTaskDetails.StopTime.HasValue)
                                {
                                    paramAdefHelpDeskTaskDetails.stopTime = objAdefHelpDeskTaskDetails.StopTime.Value.ToShortDateString() + " " + objAdefHelpDeskTaskDetails.StopTime.Value.ToLongTimeString();
                                }

                                #region Return File Attachement
                                if (objFile != null)
                                {
                                    DTOAttachment objNewDTOAttachment = new DTOAttachment();
                                    objNewDTOAttachment.attachmentID = objNewAdefHelpDeskAttachments.AttachmentId;
                                    objNewDTOAttachment.fileName = objFile.FileName;
                                    objNewDTOAttachment.originalFileName = objFile.FileName;
                                    paramAdefHelpDeskTaskDetails.colDTOAttachment.Add(objNewDTOAttachment);
                                }
                                #endregion

                                objDTOTaskDetailResponse.taskDetail = paramAdefHelpDeskTaskDetails;

                                #region **** Save to the Log
                                Log.InsertLog(ConnectionString, objAdefHelpDeskTask.TaskId, intUserId, $"{strLogUserName} inserted ticket detail.");
                                #endregion

                                #endregion
                            }
                            else
                            {
                                #region **** Update Task Detail record 

                                // Must find the AdefHelpDeskTaskDetail record to be updated
                                objAdefHelpDeskTaskDetails = objAdefHelpDeskTask.AdefHelpDeskTaskDetails
                                    .Where(x => x.DetailId == paramAdefHelpDeskTaskDetails.detailId)
                                    .FirstOrDefault();

                                if (objAdefHelpDeskTaskDetails == null)
                                {
                                    objDTOTaskDetailResponse.isSuccess = false;
                                    objDTOTaskDetailResponse.message = $"AdefHelpDeskTaskDetail detailId #{paramAdefHelpDeskTaskDetails.detailId} was not found.";
                                    return objDTOTaskDetailResponse;
                                }

                                // A Non-Admin can only update their own ticket
                                if (!IsAdministrator)
                                {
                                    if (objAdefHelpDeskTaskDetails.UserId != intUserId)
                                    {
                                        objDTOTaskDetailResponse.isSuccess = false;
                                        objDTOTaskDetailResponse.message = "A Non-Admin can only update their own ticket.";
                                        return objDTOTaskDetailResponse;
                                    }
                                }

                                objAdefHelpDeskTaskDetails.Description = paramAdefHelpDeskTaskDetails.description;

                                // Only an Admin can update the following fields
                                if (IsAdministrator)
                                {
                                    if (paramAdefHelpDeskTaskDetails.startTime != "")
                                    {
                                        objAdefHelpDeskTaskDetails.StartTime = Utility.CastToDate(paramAdefHelpDeskTaskDetails.startTime);
                                    }

                                    if (paramAdefHelpDeskTaskDetails.stopTime != "")
                                    {
                                        objAdefHelpDeskTaskDetails.StopTime = Utility.CastToDate(paramAdefHelpDeskTaskDetails.stopTime);
                                    }

                                    objAdefHelpDeskTaskDetails.DetailType = paramAdefHelpDeskTaskDetails.detailType;

                                    #region File Update *******************
                                    var objAttachment = (from Attachments in context.AdefHelpDeskAttachments
                                                         where Attachments.DetailId == objAdefHelpDeskTaskDetails.DetailId
                                                         select Attachments).FirstOrDefault();

                                    if (objAttachment != null)
                                    {
                                        // Is there an existing file and a file to be updated?
                                        if (paramAdefHelpDeskTaskDetails.colDTOAttachment.FirstOrDefault() != null)
                                        {
                                            // Is the current file the same file?
                                            if (objAttachment.AttachmentId
                                                != paramAdefHelpDeskTaskDetails.colDTOAttachment.FirstOrDefault().attachmentID)
                                            {
                                                // Delete the existing file
                                                DeleteExistingFile(objAttachment, strCurrentUser, ConnectionString);

                                                // Remove from database
                                                context.AdefHelpDeskAttachments.Remove(objAttachment);

                                                // Log it
                                                Log.InsertLog(ConnectionString,
                                                    objAdefHelpDeskTaskDetails.TaskId,
                                                    UtilitySecurity.UserIdFromUserName(strCurrentUser, ConnectionString),
                                                    $"File {objAttachment.OriginalFileName} was deleted by {strCurrentUser}.");
                                            }
                                        }

                                        // Is there an existing file but no file to be updated?
                                        if (paramAdefHelpDeskTaskDetails.colDTOAttachment.FirstOrDefault() == null)
                                        {
                                            // Delete the existing file
                                            DeleteExistingFile(objAttachment, strCurrentUser, ConnectionString);

                                            // Remove from database
                                            context.AdefHelpDeskAttachments.Remove(objAttachment);

                                            // Log it
                                            Log.InsertLog(ConnectionString,
                                                objAdefHelpDeskTaskDetails.TaskId,
                                                UtilitySecurity.UserIdFromUserName(strCurrentUser, ConnectionString),
                                                $"File {objAttachment.OriginalFileName} was deleted by {strCurrentUser}.");
                                        }
                                    }
                                    #endregion

                                    #region Attach a new file **************
                                    if (objFile != null)
                                    {
                                        objNewAdefHelpDeskAttachments.Detail = objAdefHelpDeskTaskDetails;
                                        objNewAdefHelpDeskAttachments.AttachmentPath = objGeneralSettings.FileUploadPath + @"\";
                                        objNewAdefHelpDeskAttachments.FileName = strUploadedFileName;
                                        objNewAdefHelpDeskAttachments.OriginalFileName = objFile.FileName;
                                        objNewAdefHelpDeskAttachments.UserId = UtilitySecurity.UserIdFromUserName(strCurrentUser, ConnectionString);
                                        objAdefHelpDeskTaskDetails.AdefHelpDeskAttachments.Add(objNewAdefHelpDeskAttachments);

                                        // If file type is .EML set the TaskType to EML
                                        if (Path.GetExtension(objFile.FileName).ToUpper() == Constants.EML)
                                        {
                                            objAdefHelpDeskTaskDetails.ContentType = Constants.EML.Replace(".", "");
                                            objAdefHelpDeskTaskDetails.Description = SetEmailContents(objFile);
                                        }
                                    }
                                    #endregion
                                }

                                context.SaveChanges();

                                // Set return values

                                // If file type is .EML send back email conetnts
                                if (objFile != null)
                                {
                                    if (Path.GetExtension(objFile.FileName).ToUpper() == Constants.EML)
                                    {
                                        paramAdefHelpDeskTaskDetails.description = objAdefHelpDeskTaskDetails.Description;
                                    }
                                }

                                paramAdefHelpDeskTaskDetails.detailId = objAdefHelpDeskTaskDetails.DetailId;
                                paramAdefHelpDeskTaskDetails.insertDate = objAdefHelpDeskTaskDetails.InsertDate.ToLongDateString() + " " + objAdefHelpDeskTaskDetails.InsertDate.ToLongTimeString();
                                paramAdefHelpDeskTaskDetails.detailType = objAdefHelpDeskTaskDetails.DetailType;
                                paramAdefHelpDeskTaskDetails.contentType = objAdefHelpDeskTaskDetails.ContentType;

                                if (objAdefHelpDeskTaskDetails.StartTime.HasValue)
                                {
                                    paramAdefHelpDeskTaskDetails.startTime = objAdefHelpDeskTaskDetails.StartTime.Value.ToShortDateString() + " " + objAdefHelpDeskTaskDetails.StartTime.Value.ToLongTimeString();
                                }
                                if (objAdefHelpDeskTaskDetails.StopTime.HasValue)
                                {
                                    paramAdefHelpDeskTaskDetails.stopTime = objAdefHelpDeskTaskDetails.StopTime.Value.ToShortDateString() + " " + objAdefHelpDeskTaskDetails.StopTime.Value.ToLongTimeString();
                                }

                                #region Return File Attachement
                                if (objFile != null)
                                {
                                    DTOAttachment objNewDTOAttachment = new DTOAttachment();
                                    objNewDTOAttachment.attachmentID = objNewAdefHelpDeskAttachments.AttachmentId;
                                    objNewDTOAttachment.fileName = objFile.FileName;
                                    objNewDTOAttachment.originalFileName = objFile.FileName;
                                    paramAdefHelpDeskTaskDetails.colDTOAttachment.Add(objNewDTOAttachment);
                                }
                                #endregion

                                objDTOTaskDetailResponse.taskDetail = paramAdefHelpDeskTaskDetails;

                                #region **** Save to the Log
                                Log.InsertLog(ConnectionString, objAdefHelpDeskTask.TaskId, intUserId, $"{strLogUserName} updated ticket detail.");
                                #endregion

                                #endregion
                            }

                            // Notify user
                            if (boolSendTaskDetailEmail)
                            {
                                NotifyUser(objTask, Constants.NotifyUpdateTaskDetail, strCurrentUser, CurrentHostLocation, ConnectionString, ContentRootPath);
                            }

                            if (boolSendTaskEmails)
                            {
                                // Notify AssignedGroup
                                // Only if the Requester updated or inserted the ticket
                                // or the current user is Anonymous
                                if ((objTask.requesterUserId == intUserId) || (intUserId == -1))
                                {
                                    NotifyAssignedGroup(objTask, Constants.NotifyUpdateTaskDetail, strCurrentUser, CurrentHostLocation, ConnectionString, ContentRootPath);
                                }
                            }
                        }
                        else
                        {
                            objDTOTaskDetailResponse.isSuccess = false;
                            objDTOTaskDetailResponse.message = $"Task #{objTask.taskId} Not Found";
                            return objDTOTaskDetailResponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        #region **** Handle Exception
                        string strErrorMessage =
                            $"Error: Cannot update task detail '{objTask.description}': {ex.GetBaseException().Message}";

                        // Log to the System Log
                        Log.InsertSystemLog(
                            ConnectionString,
                            Constants.TaskCreationError,
                            strCurrentUser,
                            strErrorMessage);

                        objDTOTaskDetailResponse.isSuccess = false;
                        objDTOTaskDetailResponse.message = strErrorMessage;
                        return objDTOTaskDetailResponse;
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    objDTOTaskDetailResponse.isSuccess = false;
                    objDTOTaskDetailResponse.message = ex.GetBaseException().Message;
                }
            }

            // Return the status            
            return objDTOTaskDetailResponse;
        }
        #endregion

        // Utility

        #region private static bool TaskDetailsRequired(DTOTask objTask)
        private static bool TaskDetailsRequired(DTOTask objTask)
        {
            bool boolTaskDetailsRequired = false;
            if (
                (objTask.colDTOTaskDetail[0].description.Length > 0) ||
                (objTask.colDTOTaskDetail[0].startTime.Length > 0) ||
                (objTask.colDTOTaskDetail[0].stopTime.Length > 0)
                )
            {
                boolTaskDetailsRequired = true;
            }

            return boolTaskDetailsRequired;
        }
        #endregion

        #region private static bool TaskWorkDetailsRequired(DTOTask objTask)
        private static bool TaskWorkDetailsRequired(DTOTask objTask)
        {
            bool boolTaskWorkDetailsRequired = false;
            if (
                (objTask.colDTOTaskDetail[0].startTime != null) &&
                (objTask.colDTOTaskDetail[0].stopTime != null)
                )
            {
                if (
                    (objTask.colDTOTaskDetail[0].startTime.Length > 0) ||
                    (objTask.colDTOTaskDetail[0].stopTime.Length > 0)
                    )
                {
                    boolTaskWorkDetailsRequired = true;
                }
            }

            return boolTaskWorkDetailsRequired;
        }
        #endregion

        #region private static void DeleteExistingFile(AdefHelpDeskAttachments objAttachment, string strCurrentUser, string ConnectionString)
        private static void DeleteExistingFile(AdefHelpDeskAttachments objAttachment, string strCurrentUser, string ConnectionString)
        {
            // Construct path
            string FullPath = Path.Combine(objAttachment.AttachmentPath, objAttachment.FileName);

            // Delete file if it exists
            if (System.IO.File.Exists(FullPath))
            {
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception ex)
                {
                    Log.InsertSystemLog(ConnectionString,
                        Constants.FileDeleteError,
                        strCurrentUser,
                        $"Attempting to delete file {FullPath} resulted in error {ex.GetBaseException().ToString()}.");
                }
            }
            else
            {
                Log.InsertSystemLog(ConnectionString,
                    Constants.FileReadError,
                    strCurrentUser,
                    $"Attempting to delete file {FullPath} resulted in 'file not found' error.");
            }

        }
        #endregion

        #region private static string GetHeader(int? taskId, string ticketPassword)
        private static string GetHeader(int? taskId, string ticketPassword)
        {
            string strResponse = "";

            try
            {
                strResponse = $"Task:{taskId}-code:{ticketPassword}";
            }
            catch
            {

            }

            return strResponse;

        }
        #endregion

        #region private static string SetEmailContents(IFormFile objFile)
        private static string SetEmailContents(IFormFile objFile)
        {
            MimeMessage message = new MimeMessage();
            using (var stream = objFile.OpenReadStream())
            {
                message = MimeMessage.Load(objFile.OpenReadStream());
            }

            return message.TextBody;
        }
        #endregion

        #region private string GetConnectionString()
        private string GetConnectionString()
        {
            // Use this method to make sure we get the latest one
            string strConnectionString = "ERRROR:UNSET-CONECTION-STRING";

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
