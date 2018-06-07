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
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdefHelpDeskBase.Classes
{
    public class UpdateApplication
    {
        private string _UpgradeProcessDirectory;
        private readonly IHostingEnvironment _hostEnvironment;
        private string _SystemFiles;

        public UpdateApplication(IHostingEnvironment hostEnvironment)
        {
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

            // Create wwwroot\Upgrade\UpgradeProcess directory if needed
            if (!Directory.Exists(_UpgradeProcessDirectory))
            {
                DirectoryInfo di =
                    Directory.CreateDirectory(_UpgradeProcessDirectory);
            }
        }

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        #region public void ProcessDirectory(string targetDirectory)
        public void ProcessDirectory(string targetDirectory)
        {
            if(targetDirectory == "")
            {
                targetDirectory = _UpgradeProcessDirectory;
            }

            // Determine and set the current sub path 
            string CurrentSubPath = targetDirectory.Replace(_UpgradeProcessDirectory, "");

            // Create the current sub path if needed
            if (!System.IO.Directory.Exists(_hostEnvironment.ContentRootPath + CurrentSubPath) && (CurrentSubPath.Length > 0))
            {
                System.IO.Directory.CreateDirectory(_hostEnvironment.ContentRootPath + CurrentSubPath);
            }

            // Process the list of files found in the directory.
            DirectoryInfo CurrentDirectory = new DirectoryInfo(targetDirectory);
            foreach (var file in CurrentDirectory.GetFiles())
            {
                // If the file is an assembly try to delete it first
                if (file.Extension == ".dll")
                {
                    if (System.IO.File.Exists(_hostEnvironment.ContentRootPath + $@"{CurrentSubPath}\{file.Name}"))
                    {
                        System.IO.File.Delete(_hostEnvironment.ContentRootPath + $@"{CurrentSubPath}\{file.Name}");
                    }
                }

                // Copy file to final location
                file.CopyTo(_hostEnvironment.ContentRootPath + $@"{CurrentSubPath}\{file.Name}", true);
            }

            // Recurse into subdirectories of this directory.
            foreach (var subdirectory in CurrentDirectory.GetDirectories())
            {
                ProcessDirectory(subdirectory.FullName);
            }
        }
        #endregion

        #region public void DeleteProcessDirectory()
        public void DeleteProcessDirectory()
        {
            DirectoryInfo CurrentDirectory = new DirectoryInfo(_UpgradeProcessDirectory);
            CurrentDirectory.Delete(true);
        } 
        #endregion
    }
}
