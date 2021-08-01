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
using System.IO;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;
using Microsoft.AspNetCore.Http;

namespace ADefHelpDeskApp.Controllers
{
    public class DashboardController
    {
        private IConfiguration _config { get; set; }

        public DashboardController(IConfiguration config)
        {
            _config = config;
        }

        #region public DTODashboard DashboardValues(string CurrentUserName)
        public DTODashboard DashboardValues(string CurrentUserName)
        {
            // Create DTODashboard
            DTODashboard objDTODashboard = new DTODashboard();
            string strConnectionString = GetConnectionString();

            // Must be a Administrator to call this Method
            if (!UtilitySecurity.IsAdministrator(CurrentUserName, GetConnectionString()))
            {
                return objDTODashboard;
            }

            return ShowDashboard(strConnectionString);
        }
        #endregion

        // Methods

        #region public static DTODashboard ShowDashboard(string ConnectionString)
        public static DTODashboard ShowDashboard(string ConnectionString)
        {
            // Create DTODashboard
            DTODashboard objDTODashboard = new DTODashboard();

            List<DTOTicketStatus> colDTOTicketStatus = new List<DTOTicketStatus>();
            List<DTORoleAssignments> colDTORoleAssignments = new List<DTORoleAssignments>();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // Get all Roles
                var AllRoles = context.AdefHelpDeskRoles.ToList();

                objDTODashboard.tickets = context.AdefHelpDeskTasks.Count();
                objDTODashboard.roles = context.AdefHelpDeskRoles.Count();
                objDTODashboard.tags = context.AdefHelpDeskCategories.Where(x => x.Selectable == true).Count();
                objDTODashboard.users = context.AdefHelpDeskUsers.Count();

                colDTOTicketStatus.Add(new DTOTicketStatus { id = 1, name = "New", ticketCount = context.AdefHelpDeskTasks.Where(x => x.Status == "New").Count() });
                colDTOTicketStatus.Add(new DTOTicketStatus { id = 2, name = "Active", ticketCount = context.AdefHelpDeskTasks.Where(x => x.Status == "Active").Count() });
                colDTOTicketStatus.Add(new DTOTicketStatus { id = 3, name = "Cancelled", ticketCount = context.AdefHelpDeskTasks.Where(x => x.Status == "Cancelled").Count() });
                colDTOTicketStatus.Add(new DTOTicketStatus { id = 4, name = "On Hold", ticketCount = context.AdefHelpDeskTasks.Where(x => x.Status == "On Hold").Count() });
                colDTOTicketStatus.Add(new DTOTicketStatus { id = 5, name = "Resolved", ticketCount = context.AdefHelpDeskTasks.Where(x => x.Status == "Resolved").Count() });

                var RoleAssignments = context.AdefHelpDeskTasks.Select(x => x.AssignedRoleId).Distinct().ToList();

                foreach (var RoleId in RoleAssignments)
                {
                    var RoleAssignment = AllRoles.Where(x => x.Id == RoleId).FirstOrDefault();

                    if (RoleAssignment != null)
                    {
                        var roleAssignmentCount = context.AdefHelpDeskTasks.Where(x => x.AssignedRoleId == RoleId).Count();
                        colDTORoleAssignments.Add(new DTORoleAssignments { id = 1, name = RoleAssignment.RoleName, roleAssignments = roleAssignmentCount });
                    }
                }

                // Unassigned roles
                var roleAssignmentCountUnassigned = context.AdefHelpDeskTasks.Where(x => x.AssignedRoleId == -1).Count();
                colDTORoleAssignments.Add(new DTORoleAssignments { id = 1, name = "Unassigned", roleAssignments = roleAssignmentCountUnassigned });

                objDTODashboard.colTicketStatus = colDTOTicketStatus;
                objDTODashboard.colRoleAssignments = colDTORoleAssignments;
            }

            return objDTODashboard;
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