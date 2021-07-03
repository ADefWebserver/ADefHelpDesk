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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ADefHelpDeskApp.Classes;
using Microsoft.AspNetCore.Authorization;
using AdefHelpDeskBase.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AdefHelpDeskBase.Controllers
{
    //api/UploadUpgrader
    [Authorize]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class UploadUpgraderController : Controller
    {
        private string _UpgradeProcessDirectory;
        private readonly IWebHostEnvironment _hostEnvironment;        
        private string _SystemFiles;
        private IConfigurationRoot _configRoot { get; set; }

        public UploadUpgraderController(
            IWebHostEnvironment hostEnvironment,
            IConfigurationRoot configRoot)
        {
            _configRoot = configRoot;
            _hostEnvironment = hostEnvironment;

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

            // Set _UpgradeProcessDirectory
            _UpgradeProcessDirectory = _SystemFiles + $@"\UpgradeProcess";
        }

        // api/Upload
        [HttpPost]
        [DisableFormValueModelBinding]
        #region public IActionResult Index()
        public IActionResult Index()
        {
            // Must be a Super User to proceed
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                return Ok();
            }

            string FileNameAndPath = _SystemFiles + $@"\UpgradePackage.zip";
            string WebConfigOrginalFileNameAndPath = _hostEnvironment.ContentRootPath + @"\Web.config";
            string WebConfigTempFileNameAndPath = _hostEnvironment.ContentRootPath + @"\Web.config.txt";

            try
            {
                // Delete file if it exists
                if (System.IO.File.Exists(FileNameAndPath))
                {
                    System.IO.File.Delete(FileNameAndPath);
                }

                // Delete UpgradeProcess directory            
                DirectoryInfo UpgradeDirectory = new DirectoryInfo(_UpgradeProcessDirectory);
                if (System.IO.Directory.Exists(_UpgradeProcessDirectory))
                {
                    UpgradeDirectory.Delete(true);
                }

                // Save file
                FormValueProvider formModel;
                using (var stream = System.IO.File.Create(FileNameAndPath))
                {
                    formModel = Request.StreamFile(stream).Result;
                }

                // Unzip files to ProcessDirectory
                ZipFile.ExtractToDirectory(FileNameAndPath, _UpgradeProcessDirectory);

                // *** Check upgrade
                // Get current version
                DTOVersion objVersion = GetDatabaseVersion();

                // Examine the manifest file
                objVersion = ReadManifest(objVersion);

                try
                {
                    if (objVersion.ManifestLowestVersion == "")
                    {
                        // Delete the files
                        if (System.IO.Directory.Exists(_UpgradeProcessDirectory))
                        {
                            UpgradeDirectory.Delete(true);
                        }
                        return Ok("Error: Cound not find manifest");
                    }
                }
                catch (Exception ex)
                {
                    // Delete the files
                    if (System.IO.Directory.Exists(_UpgradeProcessDirectory))
                    {
                        UpgradeDirectory.Delete(true);
                    }
                    return Ok(ex.ToString());
                }

                // Show error if needed and delete upgrade files 
                if
                    (
                    (ConvertToInteger(objVersion.VersionNumber) > ConvertToInteger(objVersion.ManifestHighestVersion)) ||
                    (ConvertToInteger(objVersion.VersionNumber) < ConvertToInteger(objVersion.ManifestLowestVersion))
                    )
                {
                    // Delete the files
                    if (System.IO.Directory.Exists(_UpgradeProcessDirectory))
                    {
                        UpgradeDirectory.Delete(true);
                    }

                    // Return the error response
                    return Ok(objVersion.ManifestFailure);
                }

                // Temporarily rename the web.config file
                // to release the locks on any assemblies
                System.IO.File.Copy(WebConfigOrginalFileNameAndPath, WebConfigTempFileNameAndPath);
                System.IO.File.Delete(WebConfigOrginalFileNameAndPath);

                // Give the site time to release locks on the assemblies
                Task.Delay(2000).Wait(); // Wait 2 seconds with blocking

                // Rename the temp web.config file back to web.config
                // so the site will be active again
                System.IO.File.Copy(WebConfigTempFileNameAndPath, WebConfigOrginalFileNameAndPath);
                System.IO.File.Delete(WebConfigTempFileNameAndPath);

                return Ok(objVersion.ManifestSuccess);
            }
            catch (Exception ex1)
            {
                try
                {
                    // Rename the temp web.config file back to web.config
                    // so the site will be active again
                    System.IO.File.Copy(WebConfigTempFileNameAndPath, WebConfigOrginalFileNameAndPath);
                    System.IO.File.Delete(WebConfigTempFileNameAndPath);
                }
                catch (Exception ex2)
                {
                    return Ok(ex2.Message);
                }

                return Ok(ex1.Message);
            }
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
                strConnectionString = _configRoot.GetConnectionString("DefaultConnection");
            }
            catch
            {
                // Do nothing
            }

            return strConnectionString;
        }
        #endregion

        #region public DTOVersion GetDatabaseVersion()
        public DTOVersion GetDatabaseVersion()
        {
            // Version object to return
            DTOVersion objVersion = new DTOVersion();

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
                        objVersion.VersionNumber = result.VersionNumber;
                    }
                }
                catch
                {
                    objVersion.VersionNumber = "00.00.00";
                }
            }

            return objVersion;
        }
        #endregion

        #region private String ReadManifest(DTOVersion objVersion)
        private DTOVersion ReadManifest(DTOVersion objVersion)
        {
            string strManifest;
            string strFilePath = _UpgradeProcessDirectory + $@"\Manifest.json";
            using (StreamReader reader = new StreamReader(strFilePath))
            {
                strManifest = reader.ReadToEnd();
                reader.Close();
            }

            dynamic objManifest = JsonConvert.DeserializeObject(strManifest);

            objVersion.ManifestHighestVersion = objManifest.ManifestHighestVersion;
            objVersion.ManifestLowestVersion = objManifest.ManifestLowestVersion;
            objVersion.ManifestSuccess = objManifest.ManifestSuccess;
            objVersion.ManifestFailure = objManifest.ManifestFailure;

            return objVersion;
        }
        #endregion

        #region private int ConvertToInteger(string strParamVersion)
        private int ConvertToInteger(string strParamVersion)
        {
            int intVersionNumber = 0;
            string strVersion = strParamVersion;

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
    }
}
