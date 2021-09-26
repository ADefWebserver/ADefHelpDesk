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
using ADefHelpDeskApp.Classes;
using AdefHelpDeskBase.Models;
using AdefHelpDeskBase.Models.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdefHelpDeskBase.Controllers
{
    public class UserManagerController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IConfiguration _configuration { get; set; }
        private readonly IWebHostEnvironment _hostEnvironment;

        public UserManagerController(
            IConfiguration configuration,
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region public DTOUser GetUser(int id)
        public DTOUser GetUser(int id)
        {
            DTOUser objDTOUser = new DTOUser();

            // Must be a Administrator to call this Method

            return GetUserMethod(id, GetConnectionString());
        }
        #endregion

        #region public UserSearchResult SearchUsers(SearchParameters searchData)
        public UserSearchResult SearchUsers(SearchParameters searchData)
        {
            UserSearchResult objUserSearchResult = new UserSearchResult();

            // Must be a Administrator to call this Method

            return SearchUsersMethod(searchData, GetConnectionString());
        }
        #endregion

        #region public DTOStatus Put(int id, DTOUser DTOUser,string CurrentUserName)
        public DTOStatus Put(int id, DTOUser DTOUser, string CurrentUserName)
        {
            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(CurrentUserName, GetConnectionString()))
            {
                return new DTOStatus();
            }

            return UpdateUser(id, DTOUser, _userManager, GetConnectionString(), CurrentUserName);
        }
        #endregion

        #region public Task<DTOStatus> CreateUser(DTOUser DTOUser, string BaseWebAddress)
        public Task<DTOStatus> CreateUser(DTOUser DTOUser, string CurrentUserName, string BaseWebAddress)
        {
            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(CurrentUserName, GetConnectionString()))
            {
                var objDTOStatus = new DTOStatus();
                return Task.FromResult(objDTOStatus);
            }

            return CreateUserMethod(DTOUser, _hostEnvironment, _userManager, _signInManager, GetConnectionString(), BaseWebAddress);
        }
        #endregion

        #region public Task<DTOStatus> Delete(int id, string CurrentUserName)
        public Task<DTOStatus> Delete(int id, string CurrentUserName)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;

            // Must be a Super Administrator to call this Method
            if (!UtilitySecurity.IsSuperUser(CurrentUserName, GetConnectionString()))
            {
                return Task.FromResult(objDTOStatus);
            }

            var result = DeleteUser(id, _userManager, GetConnectionString(), CurrentUserName);

            if (result != "")
            {
                objDTOStatus.Success = false;
                objDTOStatus.StatusMessage = result;
                return Task.FromResult(objDTOStatus);
            }
            else
            {
                objDTOStatus.Success = true;
                objDTOStatus.StatusMessage = "";
                return Task.FromResult(objDTOStatus);
            }
        }
        #endregion

        // Methods

        #region public static DTOUser GetUserMethod(int id, string ConnectionString)
        public static DTOUser GetUserMethod(int id, string ConnectionString)
        {
            DTOUser objDTOUser = new DTOUser();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get all possible roles to reduce database calls later
                var AllRoles = (from role in context.AdefHelpDeskRoles
                                select role).ToList();

                // Perform Search
                var Result = (from user in context.AdefHelpDeskUsers
                              .Include(roles => roles.AdefHelpDeskUserRoles)
                              where user.UserId == id
                              select user).FirstOrDefault();

                objDTOUser.userId = Result.UserId;
                objDTOUser.userName = Result.Username;
                objDTOUser.firstName = Result.FirstName;
                objDTOUser.lastName = Result.LastName;
                objDTOUser.isSuperUser = Result.IsSuperUser;
                objDTOUser.email = Result.Email;
                objDTOUser.password = ""; // Never send to UI - also no longer used
                objDTOUser.riapassword = ""; // Never send to UI 
                objDTOUser.verificationCode = Result.VerificationCode;

                objDTOUser.userRoles = new List<RoleDTO>();

                foreach (var itemRole in Result.AdefHelpDeskUserRoles)
                {
                    var objUserRole = AllRoles.Where(x => x.Id == itemRole.RoleId).FirstOrDefault();

                    RoleDTO objRoleDTO = new RoleDTO();

                    objRoleDTO.iD = objUserRole.Id;
                    objRoleDTO.portalID = objUserRole.PortalId;
                    objRoleDTO.roleName = objUserRole.RoleName;

                    objDTOUser.userRoles.Add(objRoleDTO);
                }
            }

            return objDTOUser;
        }
        #endregion

        #region public static UserSearchResult SearchUsersMethod(SearchParameters searchData, string ConnectionString)
        public static UserSearchResult SearchUsersMethod(SearchParameters searchData, string ConnectionString)
        {
            UserSearchResult objUserSearchResult = new UserSearchResult();
            objUserSearchResult.userList = new List<DTOUser>();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get all possible roles to reduce database calls later
                var AllRoles = (from role in context.AdefHelpDeskRoles
                                select role).ToList();

                // Perform Search
                IQueryable<AdefHelpDeskUsers> Result;

                if (searchData.searchString.Trim() != "")
                {
                    // A search string was passed
                    // Filter results
                    Result = (from user in context.AdefHelpDeskUsers
                              .Include(roles => roles.AdefHelpDeskUserRoles)
                              where
                              user.FirstName.Contains(searchData.searchString)
                              || user.LastName.Contains(searchData.searchString)
                              || user.Username.Contains(searchData.searchString)
                              || user.Email.Contains(searchData.searchString)
                              select user);
                }
                else
                {
                    // No search string was passed
                    Result = (from user in context.AdefHelpDeskUsers
                              .Include(role => role.AdefHelpDeskUserRoles)
                              select user);
                }

                // Paginate results
                var QueryResult = (from user in Result
                                   select user).OrderByDescending(x => x.LastName)
                                   .Skip(searchData.rowsPerPage * (searchData.pageNumber))
                                   .Take(searchData.rowsPerPage).ToList();

                List<DTOUser> colDTOUser = new List<DTOUser>();

                foreach (var item in QueryResult)
                {
                    DTOUser objDTOUser = new DTOUser();

                    objDTOUser.userId = item.UserId;
                    objDTOUser.userName = item.Username;
                    objDTOUser.firstName = item.FirstName;
                    objDTOUser.lastName = item.LastName;
                    objDTOUser.isSuperUser = item.IsSuperUser;
                    objDTOUser.email = item.Email;
                    objDTOUser.password = item.Password;
                    objDTOUser.riapassword = item.Riapassword;
                    objDTOUser.verificationCode = item.VerificationCode;

                    objDTOUser.userRoles = new List<RoleDTO>();

                    foreach (var itemRole in item.AdefHelpDeskUserRoles)
                    {
                        var objUserRole = AllRoles.Where(x => x.Id == itemRole.RoleId).FirstOrDefault();

                        RoleDTO objRoleDTO = new RoleDTO();

                        objRoleDTO.iD = objUserRole.Id;
                        objRoleDTO.portalID = objUserRole.PortalId;
                        objRoleDTO.roleName = objUserRole.RoleName;

                        objDTOUser.userRoles.Add(objRoleDTO);
                    }

                    colDTOUser.Add(objDTOUser);
                }

                objUserSearchResult.userList = colDTOUser;
                objUserSearchResult.totalRows = Result.Count();
                objUserSearchResult.errorMessage = string.Empty;
            }

            return objUserSearchResult;
        }
        #endregion

        #region public static DTOStatus UpdateUser(int id, DTOUser DTOUser, UserManager<ApplicationUser> _userManager, string ConnectionString, string strCurrentUser)
        public static DTOStatus UpdateUser(int id, DTOUser DTOUser, UserManager<ApplicationUser> _userManager, string ConnectionString, string strCurrentUser)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;

            #region Validation ****************************
            EmailValidation objEmailValidation = new EmailValidation();
            if (!objEmailValidation.IsValidEmail(DTOUser.email))
            {
                objDTOStatus.StatusMessage = "This Email is not valid.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            if ((DTOUser.firstName == null) || (DTOUser.firstName.Length < 1))
            {
                objDTOStatus.StatusMessage = "This First Name is not long enough.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            if ((DTOUser.lastName == null) || (DTOUser.lastName.Length < 1))
            {
                objDTOStatus.StatusMessage = "This Last Name is not long enough.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }
            #endregion

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get User  
                var objDTOUser = (from objuser in context.AdefHelpDeskUsers
                                   .Include(role => role.AdefHelpDeskUserRoles)
                                  where objuser.UserId == id
                                  select objuser).FirstOrDefault();

                if (objDTOUser == null)
                {
                    objDTOStatus.StatusMessage = "Not Found";
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }

                // Check the Email
                var objAdefHelpDeskEmail = (from AdefHelpDeskUsers in context.AdefHelpDeskUsers
                                            where AdefHelpDeskUsers.Email.ToLower() == DTOUser.email.ToLower()
                                            where AdefHelpDeskUsers.Username != DTOUser.userName
                                            select AdefHelpDeskUsers).FirstOrDefault();

                if (objAdefHelpDeskEmail != null)
                {
                    // User is already taken
                    objDTOStatus.StatusMessage = "This Email address is already taken.";
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }

                try
                {
                    // Update the user
                    objDTOUser.FirstName = DTOUser.firstName;
                    objDTOUser.LastName = DTOUser.lastName;
                    objDTOUser.Email = DTOUser.email;
                    objDTOUser.VerificationCode = null; // Admin updating user always clears verification code

                    // Cannot change your own IsSuperUser status
                    if (objDTOUser.Username != strCurrentUser)
                    {
                        objDTOUser.IsSuperUser = DTOUser.isSuperUser;
                    }

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    return objDTOStatus;
                }

                // Delete all roles -- so we can add the new ones
                foreach (var itemRole in objDTOUser.AdefHelpDeskUserRoles)
                {
                    var objUserRole = context.AdefHelpDeskUserRoles.SingleOrDefault(x => x.UserRoleId == itemRole.UserRoleId);
                    context.AdefHelpDeskUserRoles.Remove(objUserRole);
                }

                context.SaveChanges();

                // Add the Roles for the user
                foreach (var itemRole in DTOUser.userRoles)
                {
                    AdefHelpDeskUserRoles objRoleDTO = new AdefHelpDeskUserRoles();

                    objRoleDTO.RoleId = itemRole.iD;
                    objRoleDTO.UserId = DTOUser.userId;

                    context.AdefHelpDeskUserRoles.Add(objRoleDTO);
                }

                context.SaveChanges();
            }

            #region Migrate User (if needed)
            // Get user in UserManager
            var user = Task.Run(() => _userManager.FindByNameAsync(DTOUser.userName)).Result;

            if (user == null)
            {
                // The user is in the old memebership API
                // Migrate them

                if ((DTOUser.password != null) && (DTOUser.password.Trim().Length < 1))
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = "Account must be migrated to the new membership system -- Must supply a new password";
                    return objDTOStatus;
                }

                RegisterDTO objRegisterDTO = new RegisterDTO();

                objRegisterDTO.email = DTOUser.email;
                objRegisterDTO.firstName = DTOUser.firstName;
                objRegisterDTO.lastName = DTOUser.lastName;
                objRegisterDTO.password = DTOUser.password;
                objRegisterDTO.userName = DTOUser.userName;

                try
                {
                    // Membership API

                    user = new ApplicationUser { UserName = DTOUser.userName, Email = DTOUser.email };

                    var RegisterStatus = Task.Run(() => _userManager.CreateAsync(user, DTOUser.password)).Result;

                    if (!RegisterStatus.Succeeded)
                    {
                        // Registration was not successful
                        if (RegisterStatus.Errors.FirstOrDefault() != null)
                        {
                            objDTOStatus.StatusMessage = RegisterStatus.Errors.FirstOrDefault().Description;
                        }
                        else
                        {
                            objDTOStatus.StatusMessage = "Registration error";
                        }

                        objDTOStatus.Success = false;
                        return objDTOStatus;
                    }
                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.Message;
                    return objDTOStatus;
                }
            }
            #endregion

            // Update Email
            var result = Task.Run(() => _userManager.SetEmailAsync(user, DTOUser.email)).Result;

            // Only update password if it is passed
            if ((DTOUser.password != null) && (DTOUser.password.Trim().Length > 1))
            {
                try
                {
                    var resetToken = Task.Run(() => _userManager.GeneratePasswordResetTokenAsync(user)).Result;
                    var passwordResult = Task.Run(() => _userManager.ResetPasswordAsync(user, resetToken, DTOUser.password)).Result;

                    if (!passwordResult.Succeeded)
                    {
                        if (passwordResult.Errors.FirstOrDefault() != null)
                        {
                            objDTOStatus.StatusMessage = passwordResult.Errors.FirstOrDefault().Description;
                        }
                        else
                        {
                            objDTOStatus.StatusMessage = "Pasword error";
                        }

                        objDTOStatus.Success = false;
                        return objDTOStatus;
                    }
                }
                catch (Exception ex)
                {
                    objDTOStatus.Success = false;
                    objDTOStatus.StatusMessage = ex.Message;
                    return objDTOStatus;
                }
            }

            objDTOStatus.StatusMessage = "";
            objDTOStatus.Success = true;

            return objDTOStatus;
        }
        #endregion

        #region public static DTOStatus CreateUserMethod(DTOUser DTOUser, IHostingEnvironment _hostEnvironment, UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, string ConnectionString, string CurrentHostLocation)
        public async Task<DTOStatus> CreateUserMethod(DTOUser DTOUser, IWebHostEnvironment _hostEnvironment, UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, string ConnectionString, string CurrentHostLocation)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.StatusMessage = "Failure";
            objDTOStatus.Success = false;

            #region Validation ****************************
            EmailValidation objEmailValidation = new EmailValidation();
            if (!objEmailValidation.IsValidEmail(DTOUser.email))
            {
                objDTOStatus.StatusMessage = "This Email is not valid.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            if ((DTOUser.firstName == null) || (DTOUser.firstName.Length < 1))
            {
                objDTOStatus.StatusMessage = "This First Name is not long enough.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            if ((DTOUser.lastName == null) || (DTOUser.lastName.Length < 1))
            {
                objDTOStatus.StatusMessage = "This Last Name is not long enough.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            if ((DTOUser.userName == null) || (DTOUser.userName.Length < 1))
            {
                objDTOStatus.StatusMessage = "This User Name is not long enough.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            if ((DTOUser.password == null) || (DTOUser.password.Length < 3))
            {
                objDTOStatus.StatusMessage = "This Password is not long enough.";
                objDTOStatus.Success = false;
                return objDTOStatus;
            }
            #endregion

            try
            {
                RegisterDTO objRegisterDTO = new RegisterDTO();

                objRegisterDTO.userName = DTOUser.userName;
                objRegisterDTO.email = DTOUser.email;
                objRegisterDTO.firstName = DTOUser.firstName;
                objRegisterDTO.lastName = DTOUser.lastName;
                objRegisterDTO.password = DTOUser.password;

                RegisterController objRegisterController =
                    new RegisterController(_configuration, _hostEnvironment, _userManager, _signInManager);

                var objRegisterStatus = await objRegisterController.RegisterUser(objRegisterDTO,
                    ConnectionString, _hostEnvironment, _userManager, _signInManager, true, CurrentHostLocation);

                if (!objRegisterStatus.isSuccessful)
                {
                    // Registration was not successful
                    objDTOStatus.StatusMessage = objRegisterStatus.status;
                    return objDTOStatus;
                }

                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(ConnectionString);

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Get User  
                    var objDTOUser = (from objuser in context.AdefHelpDeskUsers
                                      where objuser.Username == DTOUser.userName
                                      select objuser).FirstOrDefault();

                    if (objDTOUser != null)
                    {
                        // Update remaining fields
                        objDTOUser.IsSuperUser = DTOUser.isSuperUser;
                    }

                    // Add the Roles for the user
                    int UserId = objDTOUser.UserId;
                    foreach (var itemRole in DTOUser.userRoles)
                    {
                        AdefHelpDeskUserRoles objRoleDTO = new AdefHelpDeskUserRoles();

                        objRoleDTO.RoleId = itemRole.iD;
                        objRoleDTO.UserId = UserId;

                        context.AdefHelpDeskUserRoles.Add(objRoleDTO);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                objDTOStatus.Success = false;
                return objDTOStatus;
            }

            objDTOStatus.StatusMessage = "";
            objDTOStatus.Success = true;
            return objDTOStatus;
        }
        #endregion

        #region public static string DeleteUser(int id, UserManager<ApplicationUser> _userManager, string ConnectionString, string strCurrentUser)
        public static string DeleteUser(int id, UserManager<ApplicationUser> _userManager, string ConnectionString, string strCurrentUser)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get User  
                var objDTOUser = (from objuser in context.AdefHelpDeskUsers
                                   .Include(role => role.AdefHelpDeskUserRoles)
                                  where objuser.UserId == id
                                  select objuser).FirstOrDefault();

                if (objDTOUser == null)
                {
                    return "NotFound";
                }

                // Cannot delete yourself
                if (objDTOUser.Username == strCurrentUser)
                {
                    return "You cannot delete your own account";
                }

                // Get user in UserManager
                var objUser = Task.Run(() => _userManager.FindByNameAsync(objDTOUser.Username)).Result;

                // Delete all roles
                foreach (var itemRole in objDTOUser.AdefHelpDeskUserRoles)
                {
                    var objUserRole = Task.Run(() => context.AdefHelpDeskUserRoles.SingleOrDefaultAsync(x => x.UserRoleId == itemRole.UserRoleId)).Result;
                    context.AdefHelpDeskUserRoles.Remove(objUserRole);
                }

                context.SaveChanges();

                // Delete User in AdefHelpDeskUsers
                context.AdefHelpDeskUsers.Remove(objDTOUser);
                context.SaveChanges();

                // Delete the User in UserManager              
                _userManager.DeleteAsync(objUser);
            }

            return "";
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
                strConnectionString = _configuration.GetConnectionString("DefaultConnection");
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