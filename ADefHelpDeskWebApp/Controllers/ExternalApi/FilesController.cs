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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using AdefHelpDeskBase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ADefHelpDeskWebApp.Classes;
using Microsoft.Extensions.Configuration;
using ADefHelpDeskWebApp.Models;
using Microsoft.EntityFrameworkCore;
using AdefHelpDeskBase.Models.DataContext;
using MimeKit;
using MessageReader;
using System.Threading;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs.Models;

namespace ADefHelpDeskWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class FilesController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private string _SystemFiles;
        private IConfiguration _config { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FilesController(
            IWebHostEnvironment hostEnvironment,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _hostEnvironment = hostEnvironment;
            _httpContextAccessor = httpContextAccessor;

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

        #region public DTONode SystemFiles(string CurrentUserName)
        public DTONode SystemFiles(string CurrentUserName)
        {
            // Must be a Super User to proceed
            if (!UtilitySecurity.IsSuperUser(CurrentUserName, GetConnectionString()))
            {
                return new DTONode();
            }

            return SystemFilesMethod(_hostEnvironment, _SystemFiles);
        }
        #endregion

        #region public DTOResponse ReturnContent(DTONode paramDTONode, string CurrentUserName)
        public DTOResponse ReturnContent(DTONode paramDTONode, string CurrentUserName)
        {
            DTOResponse objDTOResponse = new DTOResponse();

            // Must be a Super User to proceed
            if (!UtilitySecurity.IsSuperUser(CurrentUserName, GetConnectionString()))
            {
                objDTOResponse.isSuccess = false;
                objDTOResponse.message = "Must be a Super User to proceed";
                return objDTOResponse;
            }

            return GetFileContentMethod(paramDTONode, _SystemFiles);
        }
        #endregion

        #region public FileContentResult ReturnFile([FromBody]DTOFileParameter paramDTOFileParameter)
        [HttpPost("ReturnFile")]
        public FileContentResult ReturnFile([FromBody] DTOFileParameter paramDTOFileParameter)
        {
            var fileResult = ReturnFileMethod(paramDTOFileParameter, _SystemFiles, GetConnectionString());
            return File(fileResult.Buffer, "application/octet-stream", fileResult.FileName);
        }
        #endregion

        // Methods

        #region public static DTONode SystemFilesMethod(IHostingEnvironment _hostEnvironment, string _SystemFiles)
        public static DTONode SystemFilesMethod(IWebHostEnvironment _hostEnvironment, string _SystemFiles)
        {
            // Create Root Node
            DTONode objDTONode = new DTONode();

            if (Directory.Exists(_hostEnvironment.WebRootPath))
            {
                objDTONode.label = "Root";
                objDTONode.data = "Root";
                objDTONode.expandedIcon = "description";
                objDTONode.collapsedIcon = "description";
                objDTONode.children = new List<DTONode>();

                // Get Files
                ProcessDirectory(_SystemFiles, _SystemFiles, ref objDTONode);
            }

            return objDTONode;
        }
        #endregion

        #region public static DTOResponse GetFileContentMethod(DTONode paramDTONode, string _SystemFiles)
        public static DTOResponse GetFileContentMethod(DTONode paramDTONode, string _SystemFiles)
        {
            DTOResponse objDTOResponse = new DTOResponse();

            try
            {
                // Construct path
                string FullPath = Path.Combine(_SystemFiles, paramDTONode.data);

                // Get file
                if (System.IO.File.Exists(FullPath))
                {
                    objDTOResponse.isSuccess = true;
                    objDTOResponse.message = System.IO.File.ReadAllText(FullPath);
                }
            }
            catch (Exception ex)
            {
                objDTOResponse.isSuccess = false;
                objDTOResponse.message = ex.Message;
                return objDTOResponse;
            }

            return objDTOResponse;
        }
        #endregion

        #region public static DTOFile ReturnFileMethod(DTOFileParameter paramDTOFileParameter, string _SystemFiles, string ConnectionString)
        public static DTOFile ReturnFileMethod(DTOFileParameter paramDTOFileParameter, string _SystemFiles, string ConnectionString)
        {
            GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);
            DTOFile objDTOFile = new DTOFile();

            // Find record
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var objTask = (from task in context.AdefHelpDeskTasks
                               .Include(details => details.AdefHelpDeskTaskDetails)
                               where task.TaskId == paramDTOFileParameter.taskId
                               where task.TicketPassword == paramDTOFileParameter.ticketPassword
                               select task).FirstOrDefault();

                if (objTask != null)
                {
                    var objTaskDetail = (from taskDetail in context.AdefHelpDeskTaskDetails
                                         .Include(attachment => attachment.AdefHelpDeskAttachments)
                                         where taskDetail.DetailId == paramDTOFileParameter.detailId
                                         select taskDetail).FirstOrDefault();

                    if (objTaskDetail != null)
                    {
                        var objAttachment = (from Attachments in context.AdefHelpDeskAttachments
                                             where Attachments.AttachmentId == paramDTOFileParameter.attachmentID
                                             select Attachments).FirstOrDefault();

                        if (objAttachment != null)
                        {
                            // Construct path
                            string FullPath = "";
                            if (objGeneralSettings.StorageFileType == "AzureStorage")
                            {
                                FullPath = objAttachment.FileName;
                            }
                            else
                            {
                                FullPath = Path.Combine(objAttachment.AttachmentPath, objAttachment.FileName);
                            }

                            // See if this is a file in an .Eml file
                            if (paramDTOFileParameter.emailFileName != null)
                            {
                                // ****************************************
                                // This is a file contained in an .eml file
                                // ****************************************
                                MimeEntity emailFile = GetEmailFile(FullPath, paramDTOFileParameter.emailFileName, ConnectionString);

                                if (emailFile != null)
                                {
                                    // Get file contents
                                    using (var memory = new MemoryStream())
                                    {
                                        if (emailFile is MessagePart)
                                        {
                                            var rfc822 = (MessagePart)emailFile;
                                            rfc822.Message.WriteTo(memory);
                                        }
                                        else
                                        {
                                            var part = (MimePart)emailFile;
                                            part.Content.DecodeTo(memory);
                                        }

                                        objDTOFile.Buffer = memory.ToArray();
                                        objDTOFile.FileName = paramDTOFileParameter.emailFileName;
                                        return objDTOFile;
                                    }
                                }
                                else
                                {
                                    objDTOFile.Buffer = null;
                                    objDTOFile.FileName = "FileNotFound.txt";
                                    return objDTOFile;
                                }
                            }
                            else
                            {
                                if (objGeneralSettings.StorageFileType == "AzureStorage")
                                {
                                    string storageConnectionString = objGeneralSettings.AzureStorageConnection;

                                    try
                                    {
                                        // Create a BlobServiceClient object which will be used to create a container client.
                                        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

                                        // Get a reference to a container named "adefhelpdesk-files".
                                        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("adefhelpdesk-files");

                                        // Ensure the container exists.
                                        blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);

                                        // Get a reference to the blob.
                                        BlobClient blobClient = blobContainerClient.GetBlobClient(objAttachment.FileName);

                                        // Fetch attributes to get the blob's properties.
                                        BlobProperties properties = blobClient.GetProperties().Value;
                                        long fileByteLength = properties.ContentLength;

                                        // Download the blob's contents.
                                        byte[] fileContent = new byte[fileByteLength];
                                        using (var memoryStream = new MemoryStream(fileContent))
                                        {
                                            blobClient.DownloadTo(memoryStream);
                                        }

                                        objDTOFile.Buffer = fileContent;
                                        objDTOFile.FileName = objAttachment.OriginalFileName;
                                        return objDTOFile;
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("AzureStorage configured but AzureStorageConnection value cannot connect.", ex);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        // Get file contents
                                        var fileContents = System.IO.File.ReadAllBytes(FullPath);

                                        objDTOFile.Buffer = fileContents;
                                        objDTOFile.FileName = objAttachment.OriginalFileName;
                                        return objDTOFile;
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Get file contents:", ex);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Return missing file
            string strPath = Path.Combine(_SystemFiles, "MissingFile.html");
            var missingFileContents = System.IO.File.ReadAllBytes(strPath);

            objDTOFile.Buffer = missingFileContents;
            objDTOFile.FileName = "MissingFile.html";
            return objDTOFile;
        }
        #endregion

        // Utility

        #region public static void ProcessDirectory(string _SystemFiles, string targetDirectory, ref DTONode paramDTONode)
        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string _SystemFiles, string targetDirectory, ref DTONode paramDTONode)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                ProcessFile(_SystemFiles, fileName, ref paramDTONode);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                string WebRootPath = _SystemFiles + @"\";

                // The directory label should only contain the name of the directory
                string subdirectoryLabel = FixDirectoryName(_SystemFiles, subdirectory);

                DTONode objDTONode = new DTONode();
                objDTONode.label = subdirectoryLabel;
                objDTONode.data = subdirectory.Replace(WebRootPath, "");
                objDTONode.expandedIcon = "description";
                objDTONode.collapsedIcon = "description";
                objDTONode.children = new List<DTONode>();
                objDTONode.type = "folder";

                paramDTONode.children.Add(objDTONode);

                ProcessDirectory(_SystemFiles, subdirectory, ref objDTONode);
            }
        }
        #endregion

        #region public static void ProcessFile(string _SystemFiles, string path, ref DTONode paramDTONode)
        // Insert logic for processing found files here.
        public static void ProcessFile(string _SystemFiles, string path, ref DTONode paramDTONode)
        {
            string WebRootPath = _SystemFiles + @"\";
            string FileName = Path.GetFileName(path);
            string FilePath = path;

            DTONode objDTONode = new DTONode();
            objDTONode.label = FileName;
            objDTONode.data = FilePath.Replace(WebRootPath, "");
            objDTONode.expandedIcon = "description";
            objDTONode.collapsedIcon = "description";
            objDTONode.type = "file";

            paramDTONode.children.Add(objDTONode);
        }
        #endregion

        #region public static string FixDirectoryName(string _SystemFiles, string subdirectory)
        public static string FixDirectoryName(string _SystemFiles, string subdirectory)
        {
            string subdirectoryLabel = subdirectory;

            // Create a subdirectory label that does not include the path Root
            int intRootPosition = _SystemFiles.Count() + 1;
            subdirectoryLabel =
                subdirectory.Substring(intRootPosition,
                (subdirectory.Length - intRootPosition));

            // Create a subdirectory label that does not include the parent
            int intParentPosition = subdirectoryLabel.LastIndexOf(@"\");

            if (intParentPosition > 0)
            {
                intParentPosition++;
                subdirectoryLabel =
                    subdirectoryLabel.Substring(intParentPosition,
                    (subdirectoryLabel.Length - intParentPosition));
            }

            return subdirectoryLabel;
        }
        #endregion

        #region private static MimeEntity GetEmailFile(string strFullFilePath, string strEmailFileName, string ConnectionString)
        private static MimeEntity GetEmailFile(string strFullFilePath, string strEmailFileName, string ConnectionString)
        {
            GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);
            MimeEntity objMimeEntity = null;

            if (objGeneralSettings.StorageFileType == "AzureStorage")
            {
                string storageConnectionString = objGeneralSettings.AzureStorageConnection;

                try
                {
                    // Create a BlobServiceClient object which will be used to create a container client.
                    BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

                    // Get a reference to a container named "adefhelpdesk-files".
                    BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("adefhelpdesk-files");

                    // Ensure the container exists.
                    blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);

                    // Get a reference to the blob.
                    BlobClient blobClient = blobContainerClient.GetBlobClient(strFullFilePath);

                    // Fetch attributes to get the blob's properties.
                    BlobProperties properties = blobClient.GetProperties().Value;
                    long fileByteLength = properties.ContentLength;

                    // Download the blob's contents.
                    byte[] fileContent = new byte[fileByteLength];
                    using (var memoryStream = new MemoryStream(fileContent))
                    {
                        blobClient.DownloadTo(memoryStream);
                    }

                    // Process the downloaded content.
                    using (MemoryStream memstream = new MemoryStream(fileContent))
                    {
                        var message = MimeMessage.Load(memstream);

                        var visitor = new HtmlPreviewVisitor();
                        message.Accept(visitor);

                        // Loop through attachments
                        foreach (var item in visitor.Attachments)
                        {
                            if (item.ContentDisposition?.FileName != null)
                            {
                                if (item.ContentDisposition.FileName == strEmailFileName)
                                {
                                    return item;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("AzureStorage configured but AzureStorageConnection value cannot connect.", ex);
                }
            }
            else
            {
                using (var fileContents = System.IO.File.OpenRead(strFullFilePath))
                {
                    var message = MimeMessage.Load(fileContents);

                    var visitor = new HtmlPreviewVisitor();
                    message.Accept(visitor);

                    // Loop through attachments
                    foreach (var item in visitor.Attachments)
                    {
                        if (item.ContentDisposition.FileName != null)
                        {
                            if (item.ContentDisposition.FileName == strEmailFileName)
                            {
                                return item;
                            }
                        }
                    }
                }
            }

            return objMimeEntity;
        }
        #endregion

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