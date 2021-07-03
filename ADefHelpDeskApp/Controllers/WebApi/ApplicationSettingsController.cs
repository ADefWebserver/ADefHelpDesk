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
using AdefHelpDeskBase.Models.DataContext;

namespace ADefHelpDeskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class ApplicationSettingsController : Controller
    {        
        private string _DefaultFilesPath;
        private readonly IWebHostEnvironment _hostEnvironment;
        private IConfigurationRoot _configRoot { get; set; }

        public ApplicationSettingsController(
            IConfigurationRoot configRoot,
            IWebHostEnvironment hostEnvironment)
        {
            _configRoot = configRoot;

            // We need to create a Files directory if none exists
            // This will be used if the Administrator does not set a Files directory
            _hostEnvironment = hostEnvironment;

            // Set WebRootPath to wwwroot\Files directory
            _DefaultFilesPath =
                System.IO.Path.Combine(
                    Directory.GetCurrentDirectory(),
                    @"wwwroot\Files");

            // Create wwwroot\Files directory if needed
            if (!Directory.Exists(_DefaultFilesPath))
            {
                DirectoryInfo di =
                    Directory.CreateDirectory(_DefaultFilesPath);
            }
        }

        // api/ApplicationSettings/GetTermsOfUse
        [HttpGet("[action]")]
        #region public DTOApplicationSetting GetTermsOfUse()
        public DTOApplicationSetting GetTermsOfUse()
        {
            // Create DTOApplicationSetting
            DTOApplicationSetting objDTOApplicationSetting = new DTOApplicationSetting();
            objDTOApplicationSetting.valid = true;
            objDTOApplicationSetting.status = "";

            try
            {
                // Get Settings
                GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());

                // Get file and make replacements
                objDTOApplicationSetting.termsOfUse = System.IO.File.ReadAllText(_hostEnvironment.ContentRootPath + $@"\SystemFiles\TermsOfUse.txt");
                objDTOApplicationSetting.termsOfUse = objDTOApplicationSetting.termsOfUse.Replace("[SITE NAME]", objGeneralSettings.ApplicationName);
                objDTOApplicationSetting.termsOfUse = objDTOApplicationSetting.termsOfUse.Replace("[EMAIL ADDRESS]", objGeneralSettings.SMTPFromEmail);
                objDTOApplicationSetting.termsOfUse = objDTOApplicationSetting.termsOfUse.Replace("[YEAR]", DateTime.Now.Year.ToString());
            }
            catch (Exception ex)
            {
                objDTOApplicationSetting.valid = false;
                objDTOApplicationSetting.status = ex.GetBaseException().Message;
                return objDTOApplicationSetting;
            }

            return objDTOApplicationSetting;
        }
        #endregion

        // api/ApplicationSettings/GetPrivacyStatement
        [HttpGet("[action]")]
        #region public DTOApplicationSetting GetPrivacyStatement()
        public DTOApplicationSetting GetPrivacyStatement()
        {
            // Create DTOApplicationSetting
            DTOApplicationSetting objDTOApplicationSetting = new DTOApplicationSetting();
            objDTOApplicationSetting.valid = true;
            objDTOApplicationSetting.status = "";

            try
            {
                // Get Settings
                GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());

                // Get file and make replacements
                objDTOApplicationSetting.privacyStatement = System.IO.File.ReadAllText(_hostEnvironment.ContentRootPath + $@"\SystemFiles\PrivacyStatement.txt");
                objDTOApplicationSetting.privacyStatement = objDTOApplicationSetting.privacyStatement.Replace("[SITE NAME]", objGeneralSettings.ApplicationName);
                objDTOApplicationSetting.privacyStatement = objDTOApplicationSetting.privacyStatement.Replace("[EMAIL ADDRESS]", objGeneralSettings.SMTPFromEmail);
                objDTOApplicationSetting.privacyStatement = objDTOApplicationSetting.privacyStatement.Replace("[YEAR]", DateTime.Now.Year.ToString());
            }
            catch (Exception ex)
            {
                objDTOApplicationSetting.valid = false;
                objDTOApplicationSetting.status = ex.GetBaseException().Message;
                return objDTOApplicationSetting;
            }

            return objDTOApplicationSetting;
        }
        #endregion

        // api/ApplicationSettings/GetApplicationName
        [HttpGet("[action]")]
        #region public DTOApplicationSetting GetApplicationName()
        public DTOApplicationSetting GetApplicationName()
        {
            // Create DTOApplicationSetting
            DTOApplicationSetting objDTOApplicationSetting = new DTOApplicationSetting();
            objDTOApplicationSetting.valid = true;
            objDTOApplicationSetting.status = "";

            try
            {
                // Get Settings
                GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());

                // Set Setting 
                objDTOApplicationSetting.applicationName = objGeneralSettings.ApplicationName;
            }
            catch (Exception ex)
            {
                objDTOApplicationSetting.valid = false;
                objDTOApplicationSetting.status = ex.GetBaseException().Message;
                return objDTOApplicationSetting;
            }

            return objDTOApplicationSetting;
        }
        #endregion

        // api/ApplicationSettings/GetSettings
        [HttpGet("[action]")]
        #region public DTOApplicationSetting GetSettings()
        public DTOApplicationSetting GetSettings()
        {
            // Create DTOApplicationSetting
            DTOApplicationSetting objDTOApplicationSetting = new DTOApplicationSetting();
            objDTOApplicationSetting.valid = true;
            objDTOApplicationSetting.status = "";

            try
            {
                // Get Settings
                GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());

                // Set Setting 
                objDTOApplicationSetting.applicationName = objGeneralSettings.ApplicationName;
                objDTOApplicationSetting.uploadPermission = objGeneralSettings.UploadPermission;
                objDTOApplicationSetting.allowRegistration = objGeneralSettings.AllowRegistration;
                objDTOApplicationSetting.swaggerWebAddress = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/swagger";

                // Only return the following if a SuperUser is calling this method
                if (UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
                {
                    objDTOApplicationSetting.verifiedRegistration = objGeneralSettings.VerifiedRegistration;
                    objDTOApplicationSetting.applicationGUID = objGeneralSettings.ApplicationGUID;
                                        
                    objDTOApplicationSetting.storagefiletype = objGeneralSettings.StorageFileType;
                    objDTOApplicationSetting.azurestorageconnection = objGeneralSettings.AzureStorageConnection;

                    // For fileUploadPath we use special code to potentially get the default file path
                    objDTOApplicationSetting.fileUploadPath = GetFilesPath(_DefaultFilesPath, GetConnectionString());
                }
            }
            catch (Exception ex)
            {
                objDTOApplicationSetting.valid = false;
                objDTOApplicationSetting.status = ex.GetBaseException().Message;
                return objDTOApplicationSetting;
            }

            return objDTOApplicationSetting;
        }
        #endregion

        // api/ApplicationSettings/SetSettings        
        [HttpPut("[action]")]
        [Authorize]
        #region public DTOApplicationSetting SetSettings([FromBody]DTOFileUploadSetting FileUploadSetting)
        public DTOApplicationSetting SetSettings([FromBody]DTOApplicationSetting FileUploadSetting)
        {
            DTOApplicationSetting objDTOApplicationSetting = new DTOApplicationSetting();
            objDTOApplicationSetting.valid = true;
            objDTOApplicationSetting.status = "";

            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                objDTOApplicationSetting.valid = false;
                objDTOApplicationSetting.status = "Must be a Super Administrator to call this Method";
                return objDTOApplicationSetting;
            }

            // Get GeneralSettings
            GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());

            if(FileUploadSetting.uploadPermission == null)
            {
                FileUploadSetting.uploadPermission = "False";
            }

            // Update ****************************       

            try
            {
                objGeneralSettings.UpdateApplicationName(GetConnectionString(), FileUploadSetting.applicationName);
                objGeneralSettings.UpdateFileUploadPath(GetConnectionString(), FileUploadSetting.fileUploadPath);
                objGeneralSettings.UpdateStorageFileType(GetConnectionString(), FileUploadSetting.storagefiletype);
                if (FileUploadSetting.storagefiletype == "AzureStorage")
                {
                    objGeneralSettings.UpdateAzureStorageConnection(GetConnectionString(), FileUploadSetting.azurestorageconnection);
                }
                objGeneralSettings.UpdateUploadPermission(GetConnectionString(), FileUploadSetting.uploadPermission);
                objGeneralSettings.UpdateAllowRegistration(GetConnectionString(), FileUploadSetting.allowRegistration);
                objGeneralSettings.UpdateVerifiedRegistration(GetConnectionString(), FileUploadSetting.verifiedRegistration);
            }
            catch (Exception ex)
            {
                objDTOApplicationSetting.valid = false;
                objDTOApplicationSetting.status = ex.GetBaseException().Message;
                return objDTOApplicationSetting;
            }

            return objDTOApplicationSetting;
        }
        #endregion

        // Utility

        #region public static string GetFilesPath(string DefaultFilesPath, string ConnectionString)
        public static string GetFilesPath(string DefaultFilesPath, string ConnectionString)
        {
            string strFilesPath = "";
            try
            {
                // Get Settings
                GeneralSettings objGeneralSettings = new GeneralSettings(ConnectionString);

                // Set Files Path
                if (objGeneralSettings.FileUploadPath.Trim().Length > 1)
                {
                    // Use database setting
                    strFilesPath = objGeneralSettings.FileUploadPath;
                }
                else
                {
                    // Use default setting
                    strFilesPath = DefaultFilesPath;
                }
            }
            catch (Exception)
            {
                // Just return the default Files Path
                strFilesPath = DefaultFilesPath;
            }

            return strFilesPath;
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
