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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using AdefHelpDeskBase.Models;
using ADefHelpDeskApp.Classes;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;

namespace AdefHelpDeskBase.Controllers
{
    //api/UploadSystemFiles
    [Authorize]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class UploadSystemFilesController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;        
        private string _SystemFiles;
        private IConfigurationRoot _configRoot { get; set; }

        public UploadSystemFilesController(
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
        }

        // api/UploadSystemFiles
        [HttpPost]
        #region public IActionResult Index([FromForm] ICollection<IFormFile> files)
        public IActionResult Index([FromForm] ICollection<IFormFile> files)
        {
            // Must be a Super User to proceed
            if (!UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            {
                return Ok();
            }

            if (!Request.HasFormContentType)
            {
                return BadRequest();
            }

            // Retrieve data from Form
            var form = Request.Form;

            // Retrieve SelectedFolder
            string SelectedFolder = form["selectedFolder"].First();

            // Process all Files
            foreach (var file in form.Files)
            {
                // Process file
                using (var readStream = file.OpenReadStream())
                {
                    var filename = file.FileName.Replace("\"","").ToString();

                    filename = _SystemFiles + $@"\{SelectedFolder}\{filename}";

                    //Save file to harddrive
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                }
            }

            return Ok();
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
    }
}
