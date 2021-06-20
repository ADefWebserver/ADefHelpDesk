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
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;

namespace AdefHelpDeskBase.Controllers
{
    //api/Login
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class LoginController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHostingEnvironment _hostEnvironment;
        private IConfigurationRoot _configRoot { get; set; }

        public LoginController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHostingEnvironment hostEnvironment,
            IConfigurationRoot configRoot)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _hostEnvironment = hostEnvironment;
            _configRoot = configRoot;
        }

        // ********************************************************
        // Login

        // api/Login
        #region public IActionResult CurrentUser()
        [HttpGet]
        [AllowAnonymous]
        public IActionResult CurrentUser()
        {
            // User to return
            User objUser = new User();

            // See if the user is logged in
            if (this.User.Identity.IsAuthenticated)
            {
                // They are logged in
                objUser.userName = this.User.Identity.Name;
                objUser.isLoggedIn = true;

                // Get the Roles
                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Get all possible roles to reduce database calls later
                    var AllRoles = (from role in context.AdefHelpDeskRoles
                                    select role).ToList();

                    var UserAndRoles = context.AdefHelpDeskUsers
                        .Where(x => x.Username == objUser.userName)
                        .Include(role => role.AdefHelpDeskUserRoles)
                        .FirstOrDefault();

                    objUser.userId = UserAndRoles.UserId;
                    objUser.firstName = UserAndRoles.FirstName;
                    objUser.lastName = UserAndRoles.LastName;
                    objUser.email = UserAndRoles.Email;
                    objUser.isSuperUser = UserAndRoles.IsSuperUser;

                    objUser.userRoles = new List<RoleDTO>();

                    foreach (var Role in UserAndRoles.AdefHelpDeskUserRoles)
                    {
                        var objUserRole = AllRoles.Where(x => x.Id == Role.RoleId).FirstOrDefault();

                        RoleDTO objRole = new RoleDTO();

                        objRole.iD = objUserRole.Id;
                        objRole.roleName = objUserRole.RoleName;
                        objRole.portalID = objUserRole.PortalId;
                        objUser.userRoles.Add(objRole);
                    }
                }
            }
            else
            {
                // They are not logged in
                objUser.userId = -1;
                objUser.userName = "";
                objUser.isLoggedIn = false;
                objUser.firstName = "";
                objUser.lastName = "";
                objUser.email = "";
                objUser.isSuperUser = false;
                objUser.userRoles = new List<RoleDTO>();
            }

            // Return the result
            return Ok(objUser);
        }
        #endregion

        // (POST) api/Login 
        #region public IActionResult Index([FromBody]DTOAuthentication Authentication)
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Index([FromBody]DTOAuthentication Authentication)
        {
            // LoginStatus to return
            LoginStatus objLoginStatus = new LoginStatus();
            objLoginStatus.isLoggedIn = false;

            // Get values passed
            var paramUserName = Authentication.userName;
            var paramPassword = Authentication.password;

            if ((paramUserName != null) && (paramPassword != null))
            {
                // First log the user out
                if (this.User.Identity.IsAuthenticated)
                {
                    // Log user out
                    _signInManager.SignOutAsync().Wait();
                }

                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                try
                {
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
                                    return Ok(objLoginStatus);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // There may have been an error because this is an upgrade from a version
                    // of Adefhelpdesk before the AspNetUsers tables existed
                    using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                    {
                        // Check the legacy User table
                        var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                                   where AdefHelpDeskUsers.Username == paramUserName
                                                   where AdefHelpDeskUsers.Password != ""
                                                   select AdefHelpDeskUsers).FirstOrDefault();

                        if (objAdefHelpDeskUser != null)
                        {
                            // User is in the Legacy table and the password is not null
                            // Check their password 
                            if (objAdefHelpDeskUser.Password ==
                                ComputeHash.GetSwcMD5(paramUserName.Trim().ToLower() + paramPassword.Trim()))
                            {
                                // This database must be upgraded to ass the AspNetUseers table (for anything else to work)
                                //InstallWizardController.RunUpdateScripts("00.00.00", _hostEnvironment, GetConnectionString());

                                // Return that this account can be migrated
                                objLoginStatus.status = "Migrate";
                                return Ok(objLoginStatus);
                            }
                            else
                            {
                                objLoginStatus.status = "Error: Account needs to be migrated, but account cannot be migrated because the password is incorrect";
                                return Ok(objLoginStatus);
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
                            return Ok(objLoginStatus);
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
                    return Ok(objLoginStatus);
                }
                if (result.RequiresTwoFactor)
                {
                    objLoginStatus.status = "RequiresVerification";
                    return Ok(objLoginStatus);
                }
                if (result.IsLockedOut)
                {
                    objLoginStatus.status = "IsLockedOut";
                    return Ok(objLoginStatus);
                }
            }

            objLoginStatus.status = "Authentication Failure";

            return Ok(objLoginStatus);
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
