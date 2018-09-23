// ** Steps to create new Deploy package **
//
// Add new script to AdefHelpDesk\AdefHelpDesk\SQLScripts\
// 	/** Update Version **/
// 	DELETE FROM ADefHelpDesk_Version
// 	INSERT INTO ADefHelpDesk_Version(VersionNumber) VALUES (N'04.00.00')
//
// In ADefHelpDeskApp\Controllers\WebApi\InstallWizardController.cs:
// 	* Update string TargetDatabaseVersion = "04.00.00";
// 	* Add a entry for the new script to UpdateScripts()
//
// Publish AdefHelpDeskBase
//
// Put the results in its own folder (like C:\Temp\ADefHelpDesk_04.00.00)
//
// Open appsettings.json and set:
// 	* "DefaultConnection": "data source=(local);initial catalog=ADefHelpDesk;uid=DatabaseUser;pwd=password"
//
// Rename Pages\IndexModelRuntime.cshtml to Pages\Index.cshtml
// Copy contents of ClientApp\dist\index.html to Pages\Index.cshtml
//
// Remove the ClientApp\dist\index.html page
//
// Zip up and label it the *Install Package*
//
// Remove all the other files and directories so you only have the contents of the following directories:
//	   ADefHelpDeskApp
//	   ClientApp
//	   Pages
//	   SQLScripts
//	   SystemFiles
//
// Add a manifest.json file to the root directory
//     {
//     ManifestLowestVersion:"04.00.00",
//     ManifestHighestVersion:"04.00.00",
//     ManifestSuccess:"05.00.00 version loaded. Re-navigate to the website in your web browser to continue upgrade.",
//     ManifestFailure:"04.00.00 is the highest version that can be upgraded with this package. This install is being cancelled."
//     }
//
// Zip up and label it the *Upgrade Package*

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdefHelpDeskBase.Pages
{
    public class IndexModelRuntime : PageModel
    {
        public string PathOfScheme { get; set; }
        public string PathOfHost { get; set; }
        public string PathOfVirtualDirectory { get; set; }
        public string PathOfCurrentHostLocation { get; set; }

        public void OnGet()
        {
            PathOfScheme = this.Request.Scheme.ToString();
            PathOfHost = this.Request.Host.ToString();
            PathOfVirtualDirectory = this.Request.PathBase.ToString();
            PathOfCurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
        }
    }
}