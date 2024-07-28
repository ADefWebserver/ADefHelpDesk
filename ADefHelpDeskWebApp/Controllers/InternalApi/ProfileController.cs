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
using Microsoft.AspNetCore.Http;

namespace AdefHelpDeskBase.Controllers
{
    public class ProfileController
    {        
        private readonly UserManager<ApplicationUser> _userManager;
        private IConfiguration _config { get; set; }

        public ProfileController(
            IConfiguration config,
            UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        // ********************************************************
        // Profile

        #region public async Task<ProfileStatus> UpdateUser(ProfileDTO Profile, string CurrentUserName)
        public async Task<ProfileStatus> UpdateUser(ProfileDTO Profile, string CurrentUserName)
        {
            ProfileStatus objProfileStatus = new ProfileStatus();
            objProfileStatus.isSuccessful = true;
            objProfileStatus.status = "";

            #region Validation ****************************
            EmailValidation objEmailValidation = new EmailValidation();
            if (!objEmailValidation.IsValidEmail(Profile.email))
            {
                objProfileStatus.status = "This Email is not valid.";
                objProfileStatus.isSuccessful = false;
                return objProfileStatus;
            }

            if ((Profile.firstName == null) || (Profile.firstName.Length < 1))
            {
                objProfileStatus.status = "This First Name is not long enough.";
                objProfileStatus.isSuccessful = false;
                return objProfileStatus;
            }

            if ((Profile.lastName == null) || (Profile.lastName.Length < 1))
            {
                objProfileStatus.status = "This Last Name is not long enough.";
                objProfileStatus.isSuccessful = false;
                return objProfileStatus;
            } 
            #endregion

            // Update User ****************************

            string CurrentUser = CurrentUserName;

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                try
                {
                    // Check the Email
                    string strEmailToCheck = Profile.email.Trim().ToLower();
                    var objAdefHelpDeskEmail = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                                where AdefHelpDeskUsers.Email.ToLower() == strEmailToCheck
                                                where AdefHelpDeskUsers.Username != CurrentUser
                                                select AdefHelpDeskUsers).FirstOrDefault();

                    if (objAdefHelpDeskEmail != null)
                    {
                        // User is already taken
                        objProfileStatus.status = "This Email address is already taken.";
                        objProfileStatus.isSuccessful = false;
                        return objProfileStatus;
                    }

                    // Get the user
                    var objUser = (from user in context.AdefHelpDeskUsers
                                   where user.Username == CurrentUser
                                   select user).FirstOrDefault();

                    if (objUser != null)
                    {
                        // Update them
                        objUser.FirstName = Profile.firstName.Trim();
                        objUser.LastName = Profile.lastName.Trim();
                        objUser.Email = Profile.email.Trim();

                        #region See if the password will be updated
                        if (
                            (Profile.password != null) &&
                            (Profile.password.Trim().Length > 0)
                            )
                        {
                            // The original password must be correct
                            var user = _userManager.Users.Where(x => x.UserName == CurrentUser).FirstOrDefault();
                            var SignInResult = _userManager.CheckPasswordAsync(user, Profile.orginalpassword.Trim()).Result;

                            if (!SignInResult)
                            {
                                objProfileStatus.status =
                                    "The original password must be correct to set the new password.";
                                objProfileStatus.isSuccessful = false;
                                return objProfileStatus;
                            }

                            // First try to update the password in the ASP.NET Membership provider
                            var result = await _userManager.ChangePasswordAsync(
                                user, Profile.orginalpassword.Trim(), Profile.password.Trim());

                            if (!result.Succeeded)
                            {
                                // Return the errors
                                string strErrors = "";
                                foreach (var Error in result.Errors)
                                {
                                    strErrors = strErrors + "\n" + Error.Description;
                                }

                                objProfileStatus.status = strErrors;
                                objProfileStatus.isSuccessful = false;
                                return objProfileStatus;
                            }
                        } 
                        #endregion

                        // Save changes
                        context.SaveChanges();
                    }
                    else
                    {
                        objProfileStatus.isSuccessful = false;
                        objProfileStatus.status = $"Could not find {CurrentUser} in database";
                    }
                }
                catch (Exception ex)
                {
                    objProfileStatus.isSuccessful = false;
                    objProfileStatus.status = ex.GetBaseException().Message;
                }
            }

            return objProfileStatus;
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
