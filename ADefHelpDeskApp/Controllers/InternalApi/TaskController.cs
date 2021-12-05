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
using ADefHelpDeskApp.Classes;
using AdefHelpDeskBase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using AdefHelpDeskBase.Models.DataContext;
using MessageReader;
using MimeKit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.AspNetCore.Http;

namespace AdefHelpDeskBase.Controllers
{
    public class TaskController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IConfiguration _config { get; set; }

        public TaskController(
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region public DTOTask RetrieveTask(DTOTask paramDTOTask,string CurrentUserName, bool IsAuthenticated)
        public DTOTask RetrieveTask(DTOTask paramDTOTask, string CurrentUserName, bool IsAuthenticated)
        {
            int intUserID = UtilitySecurity.UserIdFromUserName(CurrentUserName, GetConnectionString());
            bool IsAdministrator = UtilitySecurity.IsAdministrator(CurrentUserName, GetConnectionString());
            return GetTask(paramDTOTask, intUserID, IsAdministrator, GetConnectionString(), CurrentUserName, IsAuthenticated);
        }
        #endregion

        #region public TaskSearchResult SearchTasks(SearchTaskParameters searchData,string CurrentUserName)
        public TaskSearchResult SearchTasks(SearchTaskParameters searchData, string CurrentUserName)
        {
            // Get UserID
            int intUserId = UtilitySecurity.UserIdFromUserName(CurrentUserName, GetConnectionString());

            // Determine if user is an Admin 
            int IsAdministrator = (UtilitySecurity.IsAdministrator(CurrentUserName, GetConnectionString())) ? 1 : 0;

            return SearchTasks(searchData, intUserId, IsAdministrator, GetConnectionString());
        }
        #endregion

        #region public DTOStatus Delete(int id,string CurrentUserName)
        public DTOStatus Delete(int id, string CurrentUserName)
        {
            DTOStatus objDTOStatus = new DTOStatus();

            // Must be a Administrator to call this Method
            if (!UtilitySecurity.IsAdministrator(CurrentUserName, GetConnectionString()))
            {
                return objDTOStatus;
            }

            string strResponse = DeleteTask(id, GetConnectionString(), CurrentUserName);

            objDTOStatus.StatusMessage = strResponse;
            objDTOStatus.Success = false;
            return objDTOStatus;           
        }
        #endregion

        #region public DTOStatus DeleteTaskDetail(int id,string CurrentUserName)
        public DTOStatus DeleteTaskDetail(int id, string CurrentUserName)
        {
            DTOStatus objDTOStatus = new DTOStatus();

            // Must be a Administrator to call this Method
            if (!UtilitySecurity.IsAdministrator(CurrentUserName, GetConnectionString()))
            {
                return objDTOStatus;
            }

            string strResponse = DeleteTaskDetail(id, GetConnectionString(), CurrentUserName);

            objDTOStatus.StatusMessage = strResponse;
            objDTOStatus.Success = false;
            return objDTOStatus;
        }
        #endregion

        // Private Methods

        #region public static TaskSearchResult SearchTasks(SearchTaskParameters searchData, int intUserId, int iSAdministrator, string DefaultConnection)
        public static TaskSearchResult SearchTasks(SearchTaskParameters searchData, int intUserId, int iSAdministrator, string DefaultConnection)
        {
            TaskSearchResult objTaskSearchResult = new TaskSearchResult();
            objTaskSearchResult.taskList = new List<DTOTask>();
            List<AdefHelpDeskRoles> AllRoles = new List<AdefHelpDeskRoles>();
            var resultTable = new DataTable();

            //If searchData.rowsPerPage = 0 set it to 1
            if (searchData.rowsPerPage == 0)
            {
                searchData.rowsPerPage = 1;
            }

            //If searchData.pageNumber = 0 set it to 1
            if (searchData.pageNumber == 0)
            {
                searchData.pageNumber = 1;
            }

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get all possible roles to reduce database calls later
                AllRoles = (from role in context.AdefHelpDeskRoles
                            select role).ToList();
            }

            using (var conn = new SqlConnection(DefaultConnection))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandText = "[spSearchTasks]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    cmd.Parameters.AddWithValue("@paramIsAdmin", iSAdministrator);
                    cmd.Parameters.AddWithValue("@paramUserId", intUserId);
                    cmd.Parameters.AddWithValue("@paramSearchText", CleanString(searchData.searchText) ?? "");
                    cmd.Parameters.AddWithValue("@paramStatus", searchData.status ?? "");
                    cmd.Parameters.AddWithValue("@paramPriority", searchData.priority ?? "");
                    cmd.Parameters.AddWithValue("@paramCreatedDate", CleanString(searchData.createdDate) ?? "");
                    cmd.Parameters.AddWithValue("@paramDueDate", CleanString(searchData.dueDate) ?? "");
                    cmd.Parameters.AddWithValue("@paramAssignedRoleId", searchData.assignedRoleId ?? "");
                    cmd.Parameters.AddWithValue("@paramSelectedTreeNodes", String.Join(",", searchData.selectedTreeNodes));
                    cmd.Parameters.AddWithValue("@paramSortOrder", searchData.sortOrder ?? "");
                    cmd.Parameters.AddWithValue("@paramSortField", searchData.sortField ?? "");
                    cmd.Parameters.AddWithValue("@paramRowsPerPage", searchData.rowsPerPage);
                    cmd.Parameters.AddWithValue("@paramPageNumber", searchData.pageNumber);

                    SqlParameter parTotalCount = new SqlParameter("@paramTotalCount", SqlDbType.Int);
                    parTotalCount.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(parTotalCount);

                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(resultTable);
                    }

                    objTaskSearchResult.totalRows = Convert.ToInt32(parTotalCount.Value);
                }
            }

            List<DTOTask> colDTOTasks = new List<DTOTask>();
            foreach (System.Data.DataRow item in resultTable.Rows)
            {
                DTOTask objDTOTask = new DTOTask();

                objDTOTask.taskId = Convert.ToInt32(item.ItemArray[0]);
                objDTOTask.portalId = Convert.ToInt32(item.ItemArray[1]);
                objDTOTask.description = Convert.ToString(item.ItemArray[2]);
                objDTOTask.status = Convert.ToString(item.ItemArray[3]);
                objDTOTask.priority = Convert.ToString(item.ItemArray[4]);
                objDTOTask.createdDate = Convert.ToDateTime(item.ItemArray[5]).ToShortDateString();
                objDTOTask.estimatedStart = (item.ItemArray[6].ToString() != "") ? Convert.ToDateTime(item.ItemArray[6]).ToShortDateString() : "";
                objDTOTask.estimatedCompletion = (item.ItemArray[7].ToString() != "") ? Convert.ToDateTime(item.ItemArray[7]).ToShortDateString() : "";
                objDTOTask.dueDate = (item.ItemArray[8].ToString() != "") ? Convert.ToDateTime(item.ItemArray[8]).ToShortDateString() : "";
                objDTOTask.assignedRoleId = Convert.ToInt32(item.ItemArray[9]);
                objDTOTask.ticketPassword = Convert.ToString(item.ItemArray[10]);
                objDTOTask.requesterUserId = Convert.ToInt32(item.ItemArray[11]);
                objDTOTask.requesterName = Convert.ToString(item.ItemArray[12]);
                objDTOTask.requesterEmail = Convert.ToString(item.ItemArray[13]);
                objDTOTask.requesterPhone = Convert.ToString(item.ItemArray[14]);
                if (item.ItemArray[15].ToString() != "")
                {
                    objDTOTask.estimatedHours = Convert.ToInt32(item.ItemArray[15]);
                }

                // Set Requester Name
                if (objDTOTask.requesterUserId > 0)
                {
                    var User = UtilitySecurity.UserFromUserId(objDTOTask.requesterUserId.Value, DefaultConnection);
                    objDTOTask.requesterName = $"{User.firstName} {User.lastName}";
                }
                else
                {
                    objDTOTask.requesterName = objDTOTask.requesterName;
                }

                // Set AssignedRoleName
                var objUserRole = AllRoles.Where(x => x.Id == objDTOTask.assignedRoleId).FirstOrDefault();
                if (objUserRole != null)
                {
                    objDTOTask.assignedRoleName = objUserRole.RoleName;
                }
                else
                {
                    objDTOTask.assignedRoleName = "[Unassigned]";
                }

                colDTOTasks.Add(objDTOTask);
            }

            objTaskSearchResult.taskList = colDTOTasks;
            objTaskSearchResult.errorMessage = string.Empty;

            return objTaskSearchResult;
        }
        #endregion

        #region public static DTOTask GetTask(DTOTask paramDTOTask, int intUserID, bool IsAdministrator, string DefaultConnection, string strCurrentUser, bool IsAuthenticated)
        public static DTOTask GetTask(DTOTask paramDTOTask, int intUserID, bool IsAdministrator, string DefaultConnection, string strCurrentUser, bool IsAuthenticated)
        {
            DTOTask objTask = new DTOTask();
            objTask.taskId = -1; // Task Not found
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                AdefHelpDeskTasks Result;

                // Perform Search
                if (paramDTOTask.ticketPassword != "")
                {
                    // Using ticketPassword
                    Result = (from task in context.AdefHelpDeskTasks
                              .Include(details => details.AdefHelpDeskTaskDetails)
                              .Include(categories => categories.AdefHelpDeskTaskCategories)
                              where task.TicketPassword == paramDTOTask.ticketPassword
                              where task.TaskId == paramDTOTask.taskId
                              select task).FirstOrDefault();

                    if (Result == null)
                    {
                        return objTask;
                    }
                }
                else
                {
                    // Using TaskId
                    Result = (from task in context.AdefHelpDeskTasks
                              .Include(details => details.AdefHelpDeskTaskDetails)
                              .Include(categories => categories.AdefHelpDeskTaskCategories)
                              where task.TaskId == paramDTOTask.taskId
                              select task).FirstOrDefault();

                    // Must be a Administrator or Requester to only use TaskId
                    if (!IsAdministrator)
                    {                         
                        if (!(Result.RequesterUserId == intUserID))
                        {
                            if (!UtilitySecurity.IsAdministrator(strCurrentUser, DefaultConnection))
                            {
                                return objTask;
                            }
                        }
                    }
                }

                if (Result == null)
                {
                    return objTask;
                }

                objTask.taskId = Result.TaskId;
                objTask.status = Result.Status;
                objTask.assignedRoleId = Result.AssignedRoleId;
                objTask.createdDate = Result.CreatedDate.ToShortDateString();
                objTask.description = Result.Description;
                objTask.dueDate = (Result.DueDate != null) ? Result.DueDate.Value.ToShortDateString() : "";
                objTask.estimatedCompletion = (Result.EstimatedCompletion != null) ? Result.EstimatedCompletion.Value.ToShortDateString() : "";
                objTask.estimatedHours = Result.EstimatedHours;
                objTask.estimatedStart = (Result.EstimatedStart != null) ? Result.EstimatedStart.Value.ToShortDateString() : "";
                objTask.portalId = Result.PortalId;
                objTask.priority = Result.Priority;
                objTask.requesterEmail = Result.RequesterEmail;
                objTask.requesterName = Result.RequesterName;
                objTask.requesterPhone = Result.RequesterPhone;
                objTask.requesterUserId = Result.RequesterUserId;
                objTask.ticketPassword = Result.TicketPassword;

                // Set Requester Name
                if (Result.RequesterUserId > 0)
                {
                    var User = UtilitySecurity.UserFromUserId(Result.RequesterUserId, DefaultConnection);
                    objTask.requesterName = $"{User.firstName} {User.lastName}";
                }
                else
                {
                    objTask.requesterName = Result.RequesterName;
                }

                // Add Task Categories
                objTask.selectedTreeNodes = new List<int>();
                foreach (var itemTaskCategory in Result.AdefHelpDeskTaskCategories)
                {
                    objTask.selectedTreeNodes.Add(itemTaskCategory.CategoryId);
                }

                // Add Task Details
                objTask.colDTOTaskDetail = new List<DTOTaskDetail>();

                // Get all TaskDetails
                var TaskDetails = Result.AdefHelpDeskTaskDetails.OrderByDescending(x => x.DetailId);

                // Non-Admins can only see "Comment - Visible"
                if (!IsAdministrator)
                {
                    TaskDetails = TaskDetails.Where(x => x.DetailType == "Comment - Visible").OrderByDescending(x => x.DetailId);
                }
                else
                {
                    TaskDetails = TaskDetails.OrderByDescending(x => x.DetailId);
                }

                foreach (var itemTaskDetail in TaskDetails)
                {
                    DTOTaskDetail objDTOTaskDetail = new DTOTaskDetail();

                    objDTOTaskDetail.contentType = (itemTaskDetail.ContentType != null) ? itemTaskDetail.ContentType : Constants.TXT;
                    objDTOTaskDetail.description = itemTaskDetail.Description;
                    objDTOTaskDetail.detailId = itemTaskDetail.DetailId;
                    objDTOTaskDetail.detailType = itemTaskDetail.DetailType;
                    objDTOTaskDetail.insertDate = itemTaskDetail.InsertDate.ToLongDateString() + " " + itemTaskDetail.InsertDate.ToLongTimeString();
                    objDTOTaskDetail.startTime = (itemTaskDetail.StartTime != null) ? itemTaskDetail.StartTime.Value.ToShortDateString() + " " + itemTaskDetail.StartTime.Value.ToShortTimeString() : "";
                    objDTOTaskDetail.stopTime = (itemTaskDetail.StopTime != null) ? itemTaskDetail.StopTime.Value.ToShortDateString() + " " + itemTaskDetail.StopTime.Value.ToShortTimeString() : "";
                    objDTOTaskDetail.userId = itemTaskDetail.UserId;
                    objDTOTaskDetail.userName = UtilitySecurity.UserFromUserId(itemTaskDetail.UserId, DefaultConnection).userName;

                    // Add Attachments
                    objDTOTaskDetail.colDTOAttachment = new List<DTOAttachment>();

                    var AttachmentResults = (from attachment in context.AdefHelpDeskAttachments
                                             where attachment.DetailId == objDTOTaskDetail.detailId
                                             select attachment);

                    foreach (var itemAttachmement in AttachmentResults)
                    {
                        DTOAttachment objDTOAttachment = new DTOAttachment();

                        objDTOAttachment.attachmentID = itemAttachmement.AttachmentId;
                        //objDTOAttachment.attachmentPath = itemAttachmement.AttachmentPath; -- Do not send for security reasons
                        //objDTOAttachment.fileName = itemAttachmement.FileName; -- Do not send for security reasons
                        objDTOAttachment.originalFileName = itemAttachmement.OriginalFileName;
                        objDTOAttachment.userId = itemAttachmement.UserId.ToString();

                        objDTOTaskDetail.colDTOAttachment.Add(objDTOAttachment);

                        // If file type is .EML it is a Email 
                        if (Path.GetExtension(itemAttachmement.OriginalFileName).ToUpper() == Constants.EML)
                        {
                            // Construct path
                            string FullFilePath = Path.Combine(itemAttachmement.AttachmentPath, itemAttachmement.FileName);
                            // Set Email Description and ContentType
                            SetEmailContents(itemAttachmement.FileName, itemAttachmement.AttachmentId, FullFilePath, DefaultConnection, ref objDTOTaskDetail);
                            objDTOTaskDetail.contentType = Constants.EML.Replace(".", "");
                        }

                    }

                    objTask.colDTOTaskDetail.Add(objDTOTaskDetail);
                }
            }

            #region **** Save to the Log
            if ((objTask.taskId != null) && (objTask.taskId != -1))
            {
                string strLogUserName = (IsAuthenticated) ? strCurrentUser : "[Unauthenticated]";
                Log.InsertLog(DefaultConnection, Convert.ToInt32(objTask.taskId), intUserID, $"{strLogUserName} viewed ticket.");
            }
            #endregion

            return objTask;
        } 
        #endregion
        
        #region public static string DeleteTask(int TaskId, string DefaultConnection, string strCurrentUser)
        public static string DeleteTask(int TaskId, string DefaultConnection, string strCurrentUser)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var objTask = context.AdefHelpDeskTasks.SingleOrDefault(x => x.TaskId == TaskId);

                #region Validate
                if (objTask == null)
                {
                    return "Task Not found";
                }

                if (context.AdefHelpDeskTaskDetails.FirstOrDefault(x => x.TaskId == objTask.TaskId) != null)
                {
                    return "Must delete all Comments and Work items first.";
                }
                #endregion

                // Delete associated records
                var colTaskAssociations = from TaskAssociations in context.AdefHelpDeskTaskAssociations
                                          where TaskAssociations.TaskId == objTask.TaskId
                                          select TaskAssociations;

                context.AdefHelpDeskTaskAssociations.RemoveRange(colTaskAssociations);

                var colTaskCategories = from TaskCategories in context.AdefHelpDeskTaskCategories
                                        where TaskCategories.TaskId == objTask.TaskId
                                        select TaskCategories;

                context.AdefHelpDeskTaskCategories.RemoveRange(colTaskCategories);

                var colAdefHelpDeskLog = from AdefHelpDeskLog in context.AdefHelpDeskLog
                                         where AdefHelpDeskLog.TaskId == objTask.TaskId
                                         select AdefHelpDeskLog;

                context.AdefHelpDeskLog.RemoveRange(colAdefHelpDeskLog);
                context.SaveChanges();

                // Log it
                Log.InsertSystemLog(DefaultConnection,
                    Constants.TaskDetailDeletion,
                    strCurrentUser,
                    $"({strCurrentUser}) Deleted Task # {objTask.TaskId} ({objTask.Description})");

                // Delete Task
                context.AdefHelpDeskTasks.Remove(objTask);
                context.SaveChanges();
            }

            return "";
        }
        #endregion

        #region public static string DeleteTaskDetail(int TaskId, string DefaultConnection, string strCurrentUser)
        public static string DeleteTaskDetail(int TaskDetailId, string DefaultConnection, string strCurrentUser)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var objTaskDetail = context.AdefHelpDeskTaskDetails.SingleOrDefault(x => x.DetailId == TaskDetailId);
                if (objTaskDetail == null)
                {
                    return "Task Detail Not Found";
                }

                // Get the Attachments of the current item
                var colAttachments = from Attachment in context.AdefHelpDeskAttachments
                                     where Attachment.DetailId == objTaskDetail.DetailId
                                     select Attachment;

                // Loop thru each Attachment 
                foreach (var objAttachment in colAttachments)
                {
                    // Delete the file
                    DeleteExistingFile(objAttachment, DefaultConnection, strCurrentUser);
                }

                context.AdefHelpDeskAttachments.RemoveRange(colAttachments);
                context.SaveChanges();

                // Log it
                Log.InsertSystemLog(DefaultConnection,
                    Constants.TaskDetailDeletion,
                    strCurrentUser,
                    $"({strCurrentUser}) Deleted TaskDetail # {objTaskDetail.DetailId} ({objTaskDetail.Description}) of Task # {objTaskDetail.TaskId}.");

                // Delete TaskDetail
                context.AdefHelpDeskTaskDetails.Remove(objTaskDetail);
                context.SaveChanges();
            }

            return "";
        }
        #endregion

        // Utility

        #region private static string CleanString(string paramValue)
        private static string CleanString(string paramValue)
        {
            if(paramValue == null)
            {
                return null;
            }

            string ReturnValue = paramValue.Replace(";","");
            ReturnValue = ReturnValue.Replace("--", "");

            return ReturnValue;
        }
        #endregion

        #region private string GetConnectionString()
        private string GetConnectionString()
        {
            // Use this method to make sure we get the latest one
            string strConnectionString = "ERRROR:UNSET-CONNECTION-STRING";

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

        #region private void DeleteExistingFile(AdefHelpDeskAttachments objAttachment, string DefaultConnection, string strCurrentUser)
        private static void DeleteExistingFile(AdefHelpDeskAttachments objAttachment, string DefaultConnection, string strCurrentUser)
        {
            GeneralSettings objGeneralSettings = new GeneralSettings(DefaultConnection);

            if (objGeneralSettings.StorageFileType == "AzureStorage")
            {
                CloudStorageAccount storageAccount = null;
                CloudBlobContainer cloudBlobContainer = null;

                // Retrieve the connection string for use with the application. 
                string storageConnectionString = objGeneralSettings.AzureStorageConnection;

                // Check whether the connection string can be parsed.
                if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
                {
                    // Ensure there is a AdefHelpDesk Container
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                    cloudBlobContainer = cloudBlobClient.GetContainerReference("adefhelpdesk-files");
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(objAttachment.FileName);
                    cloudBlockBlob.DeleteIfExistsAsync();
                }
                else
                {
                    throw new Exception("AzureStorage configured but AzureStorageConnection value cannot connect.");
                }
            }
            else
            {
                // Construct path
                string FullPath = Path.Combine(objAttachment.AttachmentPath, objAttachment.FileName);

                // Delete file if it exists
                if (System.IO.File.Exists(FullPath))
                {
                    try
                    {
                        System.IO.File.Delete(FullPath);

                        // Log it
                        Log.InsertSystemLog(DefaultConnection,
                            Constants.FileDeletion,
                            strCurrentUser,
                            $"({strCurrentUser}) Deleted File {objAttachment.OriginalFileName} ({FullPath}) of Task Detail # {objAttachment.DetailId}.");
                    }
                    catch (Exception ex)
                    {
                        Log.InsertSystemLog(DefaultConnection,
                            Constants.FileDeleteError,
                            strCurrentUser,
                            $"({strCurrentUser}) Attempting to delete file {FullPath} resulted in error {ex.GetBaseException().ToString()}.");
                    }
                }
                else
                {
                    Log.InsertSystemLog(DefaultConnection,
                        Constants.FileReadError,
                        strCurrentUser,
                        $"({strCurrentUser}) Attempting to delete file {FullPath} resulted in 'file not found' error.");
                }
            }
        }
        #endregion

        #region private static void SetEmailContents(string FileName, int intAttachmentId, string strFullFilePath, string ConnectionString, ref DTOTaskDetail objDTOTaskDetail)
        private static void SetEmailContents(string FileName, int intAttachmentId, string strFullFilePath, string ConnectionString, ref DTOTaskDetail objDTOTaskDetail)
        {
            GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);

            if (objGeneralSettings.StorageFileType == "AzureStorage")
            {
                CloudStorageAccount storageAccount = null;
                CloudBlobContainer cloudBlobContainer = null;

                // Retrieve the connection string for use with the application. 
                string storageConnectionString = objGeneralSettings.AzureStorageConnection;

                // Check whether the connection string can be parsed.
                if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
                {
                    // Ensure there is a AdefHelpDesk Container
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                    cloudBlobContainer = cloudBlobClient.GetContainerReference("adefhelpdesk-files");
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(FileName);

                    // Download
                    cloudBlockBlob.FetchAttributesAsync().Wait();
                    long fileByteLength = cloudBlockBlob.Properties.Length;
                    byte[] fileContent = new byte[fileByteLength];
                    for (int i = 0; i < fileByteLength; i++)
                    {
                        fileContent[i] = 0x20;
                    }

                    cloudBlockBlob.DownloadToByteArrayAsync(fileContent, 0).Wait();

                    using (MemoryStream memstream = new MemoryStream(fileContent))
                    {
                        var message = MimeMessage.Load(memstream);

                        var visitor = new HtmlPreviewVisitor();
                        message.Accept(visitor);
                        objDTOTaskDetail.emailDescription = Utility.CleanOutlookFontDefinitions(visitor.HtmlBody);

                        // Add attachments (if any)
                        objDTOTaskDetail.colDTOAttachment = new List<DTOAttachment>();

                        foreach (var item in visitor.Attachments)
                        {
                            if (item.ContentDisposition.FileName != null)
                            {
                                DTOAttachment objDTOAttachment = new DTOAttachment();

                                objDTOAttachment.attachmentID = intAttachmentId;
                                objDTOAttachment.attachmentPath = "EML";
                                objDTOAttachment.userId = "-1";
                                objDTOAttachment.fileName = item.ContentDisposition.FileName;
                                objDTOAttachment.originalFileName = strFullFilePath;

                                objDTOTaskDetail.colDTOAttachment.Add(objDTOAttachment);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("AzureStorage configured but AzureStorageConnection value cannot connect.");
                }
            }
            else
            {
                using (var fileContents = System.IO.File.OpenRead(strFullFilePath))
                {
                    var message = MimeMessage.Load(fileContents);

                    var visitor = new HtmlPreviewVisitor();
                    message.Accept(visitor);
                    objDTOTaskDetail.emailDescription = Utility.CleanOutlookFontDefinitions(visitor.HtmlBody);

                    // Add attachments (if any)
                    objDTOTaskDetail.colDTOAttachment = new List<DTOAttachment>();

                    foreach (var item in visitor.Attachments)
                    {
                        if (item.ContentDisposition.FileName != null)
                        {
                            DTOAttachment objDTOAttachment = new DTOAttachment();

                            objDTOAttachment.attachmentID = intAttachmentId;
                            objDTOAttachment.attachmentPath = "EML";
                            objDTOAttachment.userId = "-1";
                            objDTOAttachment.fileName = item.ContentDisposition.FileName;
                            objDTOAttachment.originalFileName = strFullFilePath;

                            objDTOTaskDetail.colDTOAttachment.Add(objDTOAttachment);
                        }
                    }
                }
            }
        }
        #endregion
    }
}