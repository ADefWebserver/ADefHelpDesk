//
// ADefHelpDesk.com
// Copyright (c) 2022
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.Extensions.Options;
using AdefHelpDeskBase.Models;
using ADefHelpDeskWebApp.Models;
using ADefHelpDeskWebApp.Classes;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;

namespace ADefHelpDeskWebApp.Controllers.InternalApi
{
    public class RoleController
    {        
        private IConfiguration _config { get; set; }

        public RoleController(IConfiguration config)
        {
            _config = config;
        }

        #region public List<RoleDTO> GetRoles()
        public List<RoleDTO> GetRoles()
        {
            return GetRolesMethod(GetConnectionString());
        }
        #endregion

        #region public DTOStatus Put(int id, RoleDTO RoleDTO)
        public DTOStatus Put(int id, RoleDTO RoleDTO)
        {
            // Must be a Super Administrator to call this Method
            return UpdateRole(id, RoleDTO, GetConnectionString());
        }
        #endregion

        #region public RoleDTO Post(RoleDTO RoleDTO)
        public RoleDTO Post(RoleDTO RoleDTO)
        {
            // Must be a Super Administrator to call this Method
            return CreateRole(RoleDTO, GetConnectionString());
        }
        #endregion

        #region public DTOStatus Delete(int id)
        public DTOStatus Delete(int id)
        {
            // Must be a Super Administrator to call this Method
            return DeleteRole(id, GetConnectionString());
        }
        #endregion

        // Methods 

        #region public static List<RoleDTO> GetRolesMethod(string ConnectionString)
        public static List<RoleDTO> GetRolesMethod(string ConnectionString)
        {
            // Collection to hold Roles
            List<RoleDTO> colRoleDTOs = new List<RoleDTO>();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // This returns all Roles in the database 
                colRoleDTOs = (from objRole in context.AdefHelpDeskRoles
                               select new RoleDTO
                               {
                                   iD = objRole.Id,
                                   portalID = objRole.PortalId,
                                   roleName = objRole.RoleName,
                               }).OrderBy(x => x.roleName).ToList();
            }

            return colRoleDTOs;
        }
        #endregion

        #region public static DTOStatus UpdateRole(int id, RoleDTO RoleDTO, string ConnectionString)
        public static DTOStatus UpdateRole(int id, RoleDTO RoleDTO, string ConnectionString)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var existingRole = context.AdefHelpDeskRoles.SingleOrDefaultAsync(x => x.Id == id).Result;
                if (existingRole == null)
                {
                    objDTOStatus.StatusMessage = $"id #{id} Not Found";
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }

                // Update the Role 
                existingRole.RoleName = RoleDTO.roleName;
                context.Entry(existingRole).State = EntityState.Modified;

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    objDTOStatus.Success = false;
                }
            }

            return objDTOStatus;
        }
        #endregion

        #region public static RoleDTO CreateRole(RoleDTO RoleDTO, string ConnectionString)
        public static RoleDTO CreateRole(RoleDTO RoleDTO, string ConnectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var newRoleDTO = new AdefHelpDeskRoles();

                newRoleDTO.RoleName = RoleDTO.roleName;
                newRoleDTO.PortalId = -1;

                context.AdefHelpDeskRoles.Add(newRoleDTO);
                context.SaveChanges();

                RoleDTO.iD = newRoleDTO.Id;
            }

            return RoleDTO;
        }
        #endregion

        #region public static DTOStatus DeleteRole(int id, string ConnectionString)
        public static DTOStatus DeleteRole(int id, string ConnectionString)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                try
                {
                    var objRole = context.AdefHelpDeskRoles.SingleOrDefaultAsync(x => x.Id == id).Result;

                    if (objRole == null)
                    {
                        objDTOStatus.StatusMessage = $"id #{id} Not Found";
                        objDTOStatus.Success = false;
                    }

                    context.AdefHelpDeskRoles.Remove(objRole);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    objDTOStatus.Success = false;
                }
            }

            return objDTOStatus;
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
