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
using AdefHelpDeskBase.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AdefHelpDeskBase.Models.DataContext;

namespace ADefHelpDeskApp.Classes
{
    public static class UtilitySecurity
    {
        #region public static int UserIdFromUserName(string paramUser, string DefaultConnection)
        public static int UserIdFromUserName(string paramUser, string DefaultConnection)
        {
            int response = -1;

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get the user
                var objUser = (from user in context.AdefHelpDeskUsers
                               where user.Username == paramUser
                               select user).FirstOrDefault();

                if (objUser != null)
                {
                    return response = objUser.UserId;
                }
                else
                {
                    response = -1;
                }
            }

            return response;
        }
        #endregion

        #region public static DTOUser UserFromUserName(string paramUser, string DefaultConnection)
        public static DTOUser UserFromUserName(string paramUser, string DefaultConnection)
        {
            DTOUser response = new DTOUser();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get the user
                var objUser = (from user in context.AdefHelpDeskUsers
                               where user.Username == paramUser
                               select user).FirstOrDefault();

                if (objUser != null)
                {
                    response.userId = objUser.UserId;
                    response.userName = objUser.Username;
                    response.email = objUser.Email;
                    response.firstName = objUser.FirstName;
                    response.lastName = objUser.LastName;
                    response.isSuperUser = objUser.IsSuperUser;
                    response.riapassword = objUser.Riapassword;
                }
            }

            return response;
        }
        #endregion

        #region public static DTOUser UserFromUserId(int paramUser, string DefaultConnection)
        public static DTOUser UserFromUserId(int paramUser, string DefaultConnection)
        {
            DTOUser response = new DTOUser();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get the user
                var objUser = (from user in context.AdefHelpDeskUsers
                               where user.UserId == paramUser
                               select user).FirstOrDefault();

                if (objUser != null)
                {
                    response.userId = objUser.UserId;
                    response.userName = objUser.Username;
                    response.email = objUser.Email;
                    response.firstName = objUser.FirstName;
                    response.lastName = objUser.LastName;
                    response.isSuperUser = objUser.IsSuperUser;
                    response.riapassword = objUser.Riapassword;
                }
            }

            return response;
        }
        #endregion

        #region public static List<int> UserRoleIdsForUser(string paramUser, string DefaultConnection)
        public static List<int> UserRoleIdsForUser(string paramUser, string DefaultConnection)
        {
            List<int> response = new List<int>();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get User roles for user
                var colUserRoles = (from user in context.AdefHelpDeskUsers
                                    from UserRoles in context.AdefHelpDeskUserRoles
                                    where UserRoles.UserId == user.UserId
                                    where user.Username == paramUser
                                    select UserRoles);

                foreach (var item in colUserRoles)
                {
                    response.Add(item.RoleId);
                }
            }

            return response;
        }
        #endregion

        #region public static string RoleNameForRoleId(int paramRoleId, string DefaultConnection)
        public static string RoleNameForRoleId(int paramRoleId, string DefaultConnection)
        {
            string response = "";

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get Role
                var result = (from role in context.AdefHelpDeskRoles
                              where role.Id == paramRoleId
                              select role).FirstOrDefault();

                if (result != null)
                {
                    response = result.RoleName;
                }
            }

            return response;
        }
        #endregion

        #region public static List<DTOUser> UsersForRoleId(int paramRoleID, string DefaultConnection)
        public static List<DTOUser> UsersForRoleId(int paramRoleID, string DefaultConnection)
        {
            List<DTOUser> response = new List<DTOUser>();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get Users for Role
                var colUsers = (from user in context.AdefHelpDeskUsers
                                from UserRoles in context.AdefHelpDeskUserRoles
                                where UserRoles.User == user
                                where UserRoles.RoleId == paramRoleID
                                select user);

                foreach (var item in colUsers)
                {
                    DTOUser objDTOUser = new DTOUser();

                    objDTOUser.userId = item.UserId;
                    objDTOUser.userName = item.Username;
                    objDTOUser.firstName = item.FirstName;
                    objDTOUser.lastName = item.LastName;
                    objDTOUser.email = item.Email;
                    objDTOUser.isSuperUser = item.IsSuperUser;

                    response.Add(objDTOUser);
                }
            }

            return response;
        }
        #endregion

        #region public static List<DTOUser> SuperUsers(string DefaultConnection)
        public static List<DTOUser> SuperUsers(string DefaultConnection)
        {
            List<DTOUser> response = new List<DTOUser>();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get the SuperUsers
                var colUsers = (from user in context.AdefHelpDeskUsers
                                where user.IsSuperUser == true
                                select user);

                foreach (var item in colUsers)
                {
                    DTOUser objDTOUser = new DTOUser();

                    objDTOUser.userId = item.UserId;
                    objDTOUser.userName = item.Username;
                    objDTOUser.firstName = item.FirstName;
                    objDTOUser.lastName = item.LastName;
                    objDTOUser.email = item.Email;
                    objDTOUser.isSuperUser = item.IsSuperUser;

                    response.Add(objDTOUser);
                }
            }

            return response;
        }
        #endregion

        #region public static bool IsSuperUser(string CurrentUser, string DefaultConnection)
        public static bool IsSuperUser(string CurrentUser, string DefaultConnection)
        {
            bool response = false;

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(DefaultConnection);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get the user
                var objUser = (from user in context.AdefHelpDeskUsers
                               where user.Username == CurrentUser
                               select user).FirstOrDefault();

                if (objUser != null)
                {
                    return response = (objUser.IsSuperUser);
                }
                else
                {
                    response = false;
                }
            }

            return response;
        }
        #endregion

        #region public static bool IsAdministrator(string CurrentUser, string DefaultConnection)
        public static bool IsAdministrator(string CurrentUser, string DefaultConnection)
        {
            bool response = false;

            // Fist see if his is a Super User
            if (IsSuperUser(CurrentUser, DefaultConnection))
            {
                return response = true;
            }
            else
            {
                var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
                optionsBuilder.UseSqlServer(DefaultConnection);

                using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
                {
                    // Try to get at least one UserRole for the user
                    var objUserRole = (from user in context.AdefHelpDeskUsers
                                       from UserRoles in context.AdefHelpDeskUserRoles
                                       where UserRoles.UserId == user.UserId
                                       where user.Username == CurrentUser
                                       select UserRoles).FirstOrDefault();

                    if (objUserRole != null)
                    {
                        return response = true;
                    }
                    else
                    {
                        response = false;
                    }
                }
            }

            return response;
        }
        #endregion
    }
}
