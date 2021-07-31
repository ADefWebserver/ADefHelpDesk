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
    //api/Verification
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class VerificationController : Controller
    {        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IConfiguration _config { get; set; }

        public VerificationController(
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ********************************************************
        // Verification

        // (POST) api/Verification
        #region public IActionResult Index([FromBody]DTOVerification Verification)
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Index([FromBody]DTOVerification Verification)
        {
            // LoginStatus to return
            LoginStatus objLoginStatus = new LoginStatus();

            if ((Verification.userName != null) && (Verification.password != null) && (Verification.verificationCode != null))
            {
                // Get values passed
                var paramUserName = Verification.userName.Trim();
                var paramPassword = Verification.password.Trim();
                var paramVerificationCode = Verification.verificationCode;

                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Test the Verification Code
                    var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                               where AdefHelpDeskUsers.Username == paramUserName
                                               where AdefHelpDeskUsers.VerificationCode == paramVerificationCode
                                               select AdefHelpDeskUsers).FirstOrDefault();

                    if (objAdefHelpDeskUser == null)
                    {
                        // Bad verification code
                        objLoginStatus.isLoggedIn = false;
                        objLoginStatus.status = "Incorrrect Verification Code.";
                        return Ok(objLoginStatus);
                    }

                    // Sign the User in
                    var SignInResult = _signInManager.PasswordSignInAsync(
                        paramUserName, paramPassword, false, lockoutOnFailure: false).Result;

                    if (!SignInResult.Succeeded)
                    {
                        // Return the error
                        objLoginStatus.status = $"Could not sign user {paramUserName} in.";
                        objLoginStatus.isLoggedIn = false;
                        return Ok(objLoginStatus);
                    }
                    else
                    {
                        // Clear the verification code
                        objAdefHelpDeskUser.VerificationCode = null;
                        context.SaveChanges();

                        // Return Success
                        objLoginStatus.status = $"User {paramUserName} signed in.";
                        objLoginStatus.isLoggedIn = true;
                        return Ok(objLoginStatus);
                    }
                }
            }

            objLoginStatus.isLoggedIn = false;
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
