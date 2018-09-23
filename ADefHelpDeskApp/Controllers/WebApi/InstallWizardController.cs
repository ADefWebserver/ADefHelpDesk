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

namespace AdefHelpDeskBase.Controllers
{
    //api/InstallWizard
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class InstallWizardController : Controller
    {
        // NewDatabaseVersion should always be "00.00.00"
        string NewDatabaseVersion = "00.00.00";

        // TargetDatabaseVersion should be changed to 
        // the version that the current code requires
        // ** Each upgrade will set this value and it will be
        // ** read when the appication starts up each time
        string TargetDatabaseVersion = "04.00.00";

        private string _DefaultConnection;
        private string _DefaultFilesPath;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWritableOptions<ConnectionStrings> _connectionString;
        private IConfigurationRoot _configRoot { get; set; }
        private readonly IHostingEnvironment _hostEnvironment;

        public InstallWizardController(
            IOptions<ConnectionStrings> ConnectionStrings,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWritableOptions<ConnectionStrings> connectionString,
            IConfigurationRoot configRoot,
            IHostingEnvironment hostEnvironment)
        {
            _DefaultConnection = ConnectionStrings.Value.DefaultConnection;
            _userManager = userManager;
            _signInManager = signInManager;
            _connectionString = connectionString;
            _configRoot = configRoot;
            _hostEnvironment = hostEnvironment;

            // Set WebRootPath to wwwroot\Files directory
            _hostEnvironment.WebRootPath =
                System.IO.Path.Combine(
                    Directory.GetCurrentDirectory(),
                    @"wwwroot");

            // We need to create a Files directory if none exists
            // This will be used if the Administrator does not set a Files directory
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

        // ********************************************************
        // Setupstatus

        // api/InstallWizard/CurrentVersion
        [HttpGet("[action]")]
        [AllowAnonymous]
        #region public DTOVersion CurrentVersion()
        public DTOVersion CurrentVersion()
        {
            // Version object to return
            DTOVersion objVersion = GetDatabaseVersion(NewDatabaseVersion, GetConnectionString());
            objVersion.isNewDatabase =
                (objVersion.VersionNumber == NewDatabaseVersion);
            objVersion.isUpToDate =
                (objVersion.VersionNumber == TargetDatabaseVersion);

            // Return the result
            return objVersion;
        }
        #endregion

        // POST: /api/InstallWizard/ConnectionSetting
        [HttpPost("[action]")]
        #region public DTOStatus ConnectionSetting([FromBody]DTOConnectionSetting objConnectionSetting)
        public DTOStatus ConnectionSetting([FromBody]DTOConnectionSetting objConnectionSetting)
        {
            // The return message
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;

            // Do not run if we can connect to the current database
            if (CurrentVersion().isNewDatabase == false)
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = "Database already set-up";
            }
            else
            {
                // Create a Database connection string
                string strConnectionString = CreateDatabaseConnectionString(objConnectionSetting);

                // Test the database connection string
                if (DatabaseConnectionValid(strConnectionString))
                {
                    try
                    {
                        // Update the appsettings.json file
                        UpdateDatabaseConnectionString(strConnectionString);

                        // Update the in-memory connection string
                        _DefaultConnection = strConnectionString;
                    }
                    catch (Exception ex)
                    {
                        // appsettings.json file update error
                        objDTOStatus.Success = false;
                        objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    }
                }
                else
                {
                    // Bad connection setting
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = "Connection settings are not valid";
                }
            }

            // Return the result
            return objDTOStatus;
        }
        #endregion

        // api/InstallWizard/UpdateDatabase
        [HttpGet("[action]")]
        [Authorize]
        #region public DTOStatus UpdateDatabase()
        public DTOStatus UpdateDatabase()
        {
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;

            // Must be a Super User to proceed
            if (UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                // Run update scripts
                objDTOStatus = RunUpdateScripts(NewDatabaseVersion, _hostEnvironment, GetConnectionString());
            }
            else
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = "Must be a Super User to proceed";
            }

            // Return the result
            return objDTOStatus;
        }
        #endregion

        // POST: /api/InstallWizard/CreateAdminLogin
        [HttpPost("[action]")]
        [AllowAnonymous]
        #region public DTOStatus CreateAdminLogin([FromBody]RegisterDTO Register)
        public RegisterStatus CreateAdminLogin([FromBody]RegisterDTO objRegister)
        {
            // RegisterStatus to return
            RegisterStatus objRegisterStatus = new RegisterStatus();
            objRegisterStatus.status = "Registration Failure";
            objRegisterStatus.isSuccessful = false;

            // Do not run if we can connect to the current database
            if (CurrentVersion().isNewDatabase == false)
            {
                objRegisterStatus.isSuccessful = false;
                objRegisterStatus.status = "Cannot create the Admin account because the database is already set-up. Reload your web browser to upgrade using the updated database connection.";
            }
            else
            {
                // Run the scripts to set-up the database
                DTOStatus objDTOStatus = RunUpdateScripts(NewDatabaseVersion, _hostEnvironment, GetConnectionString());

                if (!objDTOStatus.Success)
                {
                    // If scripts have an error return it
                    objRegisterStatus.isSuccessful = false;
                    objRegisterStatus.status = objDTOStatus.StatusMessage;
                }
                else
                {
                    // Create the Administrator
                    string strCurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                    objRegisterStatus = RegisterController.RegisterUser(
                        objRegister, GetConnectionString(), _hostEnvironment, _userManager, _signInManager, strCurrentHostLocation, true, true);

                    // There was an error creating the Administrator
                    if (!objRegisterStatus.isSuccessful)
                    {
                        // Delete the record in the version table 
                        // So the install can be run again
                        objDTOStatus = ResetVersionTable();

                        if (!objDTOStatus.Success)
                        {
                            // If there is an error return it
                            objRegisterStatus.isSuccessful = false;
                            objRegisterStatus.status = objDTOStatus.StatusMessage;
                        }
                        else
                        {
                            //  Delete the user in case they were partially created
                            objDTOStatus = DeleteAllUsers();

                            if (!objDTOStatus.Success)
                            {
                                // If there is an error return it
                                objRegisterStatus.isSuccessful = false;
                                objRegisterStatus.status = objDTOStatus.StatusMessage;
                            }
                        }
                    }
                    else
                    {
                        // Update the created user to be a SuperUser
                        objDTOStatus = MakeUserASuperUser(objRegister.userName);

                        #region Set the upload file path
                        try
                        {
                            string strDefaultFilesPath = ADefHelpDeskApp.Controllers.ApplicationSettingsController.GetFilesPath(_DefaultFilesPath, GetConnectionString());

                            // Get GeneralSettings
                            GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());
                            objGeneralSettings.UpdateFileUploadPath(GetConnectionString(), strDefaultFilesPath);
                        }
                        catch
                        {
                            // Do nothing if this fails
                            // Admin can set the file path manually
                        } 
                        #endregion

                        if (!objDTOStatus.Success)
                        {
                            // If there is an error return it
                            objRegisterStatus.isSuccessful = false;
                            objRegisterStatus.status = objDTOStatus.StatusMessage;
                        }
                    }
                }
            }

            return objRegisterStatus;
        }
        #endregion

        // Helpers

        #region public static DTOVersion GetDatabaseVersion(string NewDatabaseVersion, string ConnectionString)
        public static DTOVersion GetDatabaseVersion(string NewDatabaseVersion, string ConnectionString)
        {
            // Version object to return
            DTOVersion objVersion = new DTOVersion();

            // If Version returned is NewDatabaseVersion 
            // we will assume the Version table does not exist
            objVersion.VersionNumber = NewDatabaseVersion;

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

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
                        objVersion.VersionNumber = result.VersionNumber;
                    }
                }
                catch
                {
                    // Do nothing if we cannot connect
                    // the method will return NewDatabaseVersion
                }
            }

            return objVersion;
        }
        #endregion

        #region private string CreateDatabaseConnectionString(DTOConnectionSetting objConnectionSetting)
        private string CreateDatabaseConnectionString(DTOConnectionSetting objConnectionSetting)
        {
            StringBuilder SB = new StringBuilder();
            string strConnectionString = "";

            string strUserInfo = (!objConnectionSetting.IntegratedSecurity) ?
                String.Format("uid={0};pwd={1}",
                objConnectionSetting.Username,
                objConnectionSetting.Password) :
                "integrated security=True";

            strConnectionString = String.Format("{0}data source={1};initial catalog={2};{3}",
                SB.ToString(),
                objConnectionSetting.ServerName,
                objConnectionSetting.DatabaseName,
                strUserInfo);

            return strConnectionString;
        }
        #endregion

        #region private bool DatabaseConnectionValid(string strConnectionString)
        private bool DatabaseConnectionValid(string strConnectionString)
        {
            bool boolDatabaseConnectionValid = true;

            try
            {
                // Try to connect to the database
                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(strConnectionString);

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    context.Database.OpenConnection();
                }
            }
            catch
            {
                // Could not connect
                boolDatabaseConnectionValid = false;
            }

            return boolDatabaseConnectionValid;
        }
        #endregion

        #region private string UpdateDatabaseConnectionString(string strConnectionString)
        private string UpdateDatabaseConnectionString(string strConnectionString)
        {
            // Update DefaultConnection in the appsettings.json file            
            _connectionString.Update(opt =>
            {
                opt.DefaultConnection = strConnectionString;
            });

            // *********************************
            // Reload configuration
            ReloadConfiguration();

            return strConnectionString;
        }
        #endregion

        #region public static DTOStatus RunUpdateScripts(string _NewDatabaseVersion, IHostingEnvironment _hostEnvironment, string ConnectionString)
        public static DTOStatus RunUpdateScripts(string _NewDatabaseVersion, IHostingEnvironment _hostEnvironment, string ConnectionString)
        {
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;

            // Get the update scripts
            Dictionary<int, string> ColScripts = UpdateScripts();
            foreach (var sqlScript in ColScripts)
            {
                try
                {
                    // Run the script
                    DTOVersion objVersion = GetDatabaseVersion(_NewDatabaseVersion, ConnectionString);
                    int intCurrentDatabaseVersion = ConvertVersionToInteger(objVersion.VersionNumber);

                    // Only run the script if it is higher than the 
                    // current database version
                    if (sqlScript.Key > intCurrentDatabaseVersion)
                    {
                        var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                        optionsBuilder.UseSqlServer(ConnectionString);

                        using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                        {
                            context.Database.ExecuteSqlCommand(GetSQLScript(sqlScript.Value, _hostEnvironment));
                        }
                    }
                }
                catch (Exception ex)
                {
                    objDTOStatus.StatusMessage = ex.Message;
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }
            }

            return objDTOStatus;
        }
        #endregion

        #region ResetVersionTable()
        private DTOStatus ResetVersionTable()
        {
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                try
                {
                    // Get all version records
                    var versions = (from version in context.AdefHelpDeskVersion
                                    select version).ToList();

                    // Delete them
                    foreach (var version in versions)
                    {
                        context.AdefHelpDeskVersion.Remove(version);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                }
            }

            return objDTOStatus;
        }
        #endregion

        #region private DTOStatus MakeUserASuperUser(string UserName)
        private DTOStatus MakeUserASuperUser(string UserName)
        {
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                try
                {
                    // Get the user
                    var objUser = (from user in context.AdefHelpDeskUsers
                                   where user.Username == UserName
                                   select user).FirstOrDefault();

                    if (objUser != null)
                    {
                        // Update them
                        objUser.IsSuperUser = true;
                        context.SaveChanges();
                    }
                    else
                    {
                        objDTOStatus.Success = false;
                        objDTOStatus.StatusMessage = $"Cound not find {UserName} in database";
                    }
                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                }
            }

            return objDTOStatus;
        }
        #endregion

        #region private DTOStatus DeleteAllUsers()
        private DTOStatus DeleteAllUsers()
        {
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;
            objDTOStatus.StatusMessage = "";

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                try
                {
                    context.Database.ExecuteSqlCommand("delete from ADefHelpDesk_Users");
                    context.Database.ExecuteSqlCommand("delete from AspNetUsers");
                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                }
            }

            return objDTOStatus;
        }
        #endregion

        // Utility

        #region private static Dictionary<int, string> UpdateScripts()
        private static Dictionary<int, string> UpdateScripts()
        {
            Dictionary<int, string> ColScripts = new Dictionary<int, string>();

            ColScripts.Add(ConvertVersionToInteger("01.00.00.sql"), "01.00.00.sql");
            ColScripts.Add(ConvertVersionToInteger("02.10.00.sql"), "02.10.00.sql");
            ColScripts.Add(ConvertVersionToInteger("03.10.00.sql"), "03.10.00.sql");
            ColScripts.Add(ConvertVersionToInteger("03.20.00.sql"), "03.20.00.sql");
            ColScripts.Add(ConvertVersionToInteger("03.30.00.sql"), "03.30.00.sql");
            ColScripts.Add(ConvertVersionToInteger("04.00.00.sql"), "04.00.00.sql");

            return ColScripts;
        }
        #endregion

        #region private static String GetSQLScript(string SQLScript, IHostingEnvironment _hostEnvironment)
        private static String GetSQLScript(string SQLScript, IHostingEnvironment _hostEnvironment)
        {
            string strSQLScript;
            string strFilePath = _hostEnvironment.ContentRootPath + $@"\SQLScripts\{SQLScript}";
            using (StreamReader reader = new StreamReader(strFilePath))
            {
                strSQLScript = reader.ReadToEnd();
                reader.Close();
            }
            return strSQLScript;
        }
        #endregion

        #region private static int ConvertVersionToInteger(string strParamVersion)
        private static int ConvertVersionToInteger(string strParamVersion)
        {
            int intVersionNumber = 0;

            // Strip out possible .sql in string
            string strVersion = strParamVersion.Replace(".sql", "");

            // Split into parts seperated by periods
            char[] splitchar = { '.' };
            var strSegments = strVersion.Split(splitchar);

            // Process the segments
            int i = 0;
            List<int> colMultiplyers = new List<int> { 10000, 100, 1 };
            foreach (var strSegment in strSegments)
            {
                int intSegmentNumber = Convert.ToInt32(strSegment);
                intVersionNumber = intVersionNumber + (intSegmentNumber * colMultiplyers[i]);
                i++;
            }

            return intVersionNumber;
        }
        #endregion

        #region private void ReloadConfiguration()
        private void ReloadConfiguration()
        {
            // Temporarily rename the web.config file
            // to release the locks on any assemblies
            string WebConfigOrginalFileNameAndPath =
                _hostEnvironment.ContentRootPath + @"\Web.config";
            string WebConfigTempFileNameAndPath =
                _hostEnvironment.ContentRootPath + @"\Web.config.txt";

            System.IO.File.Copy(WebConfigOrginalFileNameAndPath,
                WebConfigTempFileNameAndPath);
            System.IO.File.Delete(WebConfigOrginalFileNameAndPath);
            // Give the site time to release locks on the assemblies
            Task.Delay(2000).Wait(); // Wait 2 seconds with blocking
                                     // Rename the temp web.config file back to web.config
                                     // so the site will be active again
            System.IO.File.Copy(WebConfigTempFileNameAndPath,
                WebConfigOrginalFileNameAndPath);
            System.IO.File.Delete(WebConfigTempFileNameAndPath);

            // Finally a Config Reload
            _configRoot.Reload();
        }
        #endregion

        #region private string GetConnectionString()
        private string GetConnectionString()
        {
            // Use this method because the Instal Wizard updated the
            // connection string and we want to make sure we get
            // the latest one
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
