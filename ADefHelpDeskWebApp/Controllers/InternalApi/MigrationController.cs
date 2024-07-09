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
using ADefHelpDeskWebApp.Classes;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;

namespace AdefHelpDeskBase.Controllers
{
    public class MigrationController
    {        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IConfiguration _config { get; set; }

        public MigrationController(
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ********************************************************
        // Migrate

        #region public LoginStatus Index(DTOMigration Migration)
        public LoginStatus Index(DTOMigration Migration)
        {
            // LoginStatus to return
            LoginStatus objLoginStatus = new LoginStatus();
            objLoginStatus.isLoggedIn = false;

            if ((Migration.userName != null) && (Migration.password != null) && (Migration.passwordNew != null))
            {
                // Get values passed
                var paramUserName = Migration.userName;
                var paramPassword = ComputeHash.GetSwcMD5(paramUserName.Trim().ToLower() + Migration.password.Trim());
                var paramPasswordNew = Migration.passwordNew;

                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(GetConnectionString());

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Must be in legacy User table
                    var objAdefHelpDeskUser = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                               where AdefHelpDeskUsers.Username == paramUserName
                                               where AdefHelpDeskUsers.Password == paramPassword
                                               select AdefHelpDeskUsers).FirstOrDefault();

                    if (objAdefHelpDeskUser != null)
                    {
                        // Email Validation ****************************

                        if (objAdefHelpDeskUser.Email == null)
                        {
                            objLoginStatus.status = "The Email for this account is not valid. It cannot be migrated.";
                            objLoginStatus.isLoggedIn = false;
                            return objLoginStatus;
                        }

                        EmailValidation objEmailValidation = new EmailValidation();
                        if (!objEmailValidation.IsValidEmail(objAdefHelpDeskUser.Email))
                        {
                            objLoginStatus.status = "The Email for this account is not valid. It cannot be migrated.";
                            objLoginStatus.isLoggedIn = false;
                            return objLoginStatus;
                        }

                        // Migrate Account

                        var user = new ApplicationUser { UserName = paramUserName, Email = objAdefHelpDeskUser.Email };
                        var result = _userManager.CreateAsync(user, paramPasswordNew).Result;

                        if (result.Succeeded)
                        {
                            // Sign the User in
                            var SignInResult = _signInManager.PasswordSignInAsync(
                                paramUserName, paramPasswordNew, false, lockoutOnFailure: false).Result;

                            if (!SignInResult.Succeeded)
                            {
                                // Return the error
                                objLoginStatus.status = $"Could not sign user {paramUserName} in.";
                                objLoginStatus.isLoggedIn = false;
                                return objLoginStatus;
                            }                           
                            else
                            {
                                try
                                {
                                    // Everything worked
                                    // Update the users password in the legacy table
                                    objAdefHelpDeskUser.Password = ComputeHash.GetSwcMD5(paramUserName.Trim().ToLower() + paramPasswordNew.Trim());
                                    context.SaveChanges();
                                }
                                catch
                                {
                                    // Do nothing if this does not work
                                    // This password is only needed if connecting from the older
                                    // Non Angular version of ADefHelpDesk
                                }

                                // Success 
                                objLoginStatus.status = $"Logged {paramUserName} in.";
                                objLoginStatus.isLoggedIn = true;
                                return objLoginStatus;
                            }
                        }
                        else
                        {
                            // Return the errors from the Memberhip API Creation
                            string strErrors = "";
                            foreach (var Error in result.Errors)
                            {
                                strErrors = strErrors + "\n" + Error.Description;
                            }

                            // Return the error
                            objLoginStatus.status = strErrors;
                            objLoginStatus.isLoggedIn = false;
                            return objLoginStatus;
                        }
                    }
                    else
                    {
                        objLoginStatus.status = "Orginal password does not match.";
                        return objLoginStatus;
                    }
                }
            }

            objLoginStatus.status = "Authentication Failure";

            return objLoginStatus;
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
