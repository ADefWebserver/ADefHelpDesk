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
using Microsoft.AspNetCore.Http;

namespace AdefHelpDeskBase.Controllers
{
    public class LoginController
    {
        private IConfiguration _config { get; set; }

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        // ********************************************************
        // Login

        #region public User CurrentUser(string CurrentUserName)
        public User CurrentUser(string CurrentUserName)
        {
            // User to return
            User objUser = new User();

            objUser.userName = CurrentUserName;
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

            // Return the result
            return objUser;
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
