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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.Extensions.Options;
using AdefHelpDeskBase.Models;
using ADefHelpDeskApp.Models;
using ADefHelpDeskApp.Classes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using AdefHelpDeskBase.Models.DataContext;

namespace ADefHelpDeskApp.Controllers.InternalApi
{
    public class CategoryController
    {
        private IConfiguration _config { get; set; }
        private IMemoryCache _cache;

        public CategoryController(
            IConfiguration config,
            IMemoryCache memoryCache)
        {
            _config = config;
            _cache = memoryCache;
        }

        #region public IActionResult Put(int id, CategoryNode categoryNode)
        public IActionResult Put(int id, CategoryNode categoryNode)
        {
            // Must be a Super Administrator to call this Method
            return (IActionResult)UpdateCategory(id, categoryNode, GetConnectionString());
        }
        #endregion

        #region public IActionResult Post(CategoryNode categoryNode)
        public IActionResult Post(CategoryNode categoryNode)
        {
            // Must be a Super Administrator to call this Method
            return (IActionResult)CreateCategory(categoryNode, GetConnectionString());
        }
        #endregion

        #region public IActionResult Delete(int id)
        public IActionResult Delete(int id)
        {
            // Must be a Super Administrator to call this Method
            return (IActionResult)DeleteCategory(id, GetConnectionString());
        }
        #endregion

        // Methods

        #region public static DTOStatus UpdateCategory(int id, CategoryNode categoryNode, string ConnectionString)
        public static DTOStatus UpdateCategory(int id, CategoryNode categoryNode, string ConnectionString)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var existingCategoryNode = context.AdefHelpDeskCategories.SingleOrDefault(x => x.CategoryId == id);
                if (existingCategoryNode == null)
                {
                    objDTOStatus.StatusMessage = $"id #{id} Not Found";
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }

                // Update the Node 
                existingCategoryNode.CategoryName = categoryNode.NodeName;
                if (categoryNode.ParentId > 0)
                {
                    existingCategoryNode.ParentCategoryId = categoryNode.ParentId;
                }
                else
                {
                    existingCategoryNode.ParentCategoryId = null;
                }

                existingCategoryNode.Selectable = categoryNode.Selectable;
                existingCategoryNode.RequestorVisible = categoryNode.RequestorVisible;

                context.Entry(existingCategoryNode).State = EntityState.Modified;

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    objDTOStatus.StatusMessage = ex.GetBaseException().Message;
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }
            }

            return objDTOStatus;
        }
        #endregion

        #region public static CategoryNode CreateCategory(CategoryNode categoryNode, string ConnectionString)
        public static CategoryNode CreateCategory(CategoryNode categoryNode, string ConnectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var newCategoryNode = new AdefHelpDeskCategories();

                newCategoryNode.CategoryName = categoryNode.NodeName;

                if (categoryNode.ParentId > 0)
                {
                    newCategoryNode.ParentCategoryId = categoryNode.ParentId;
                }
                else
                {
                    newCategoryNode.ParentCategoryId = null;
                }

                newCategoryNode.Selectable = categoryNode.Selectable;
                newCategoryNode.RequestorVisible = categoryNode.RequestorVisible;

                context.AdefHelpDeskCategories.Add(newCategoryNode);
                context.SaveChanges();

                categoryNode.Id = newCategoryNode.CategoryId;
            }

            return categoryNode;
        }
        #endregion

        #region public static DTOStatus DeleteCategory(int id, string ConnectionString)
        public static DTOStatus DeleteCategory(int id, string ConnectionString)
        {
            // Status to return
            DTOStatus objDTOStatus = new DTOStatus();
            objDTOStatus.Success = true;

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                var categoryNode = context.AdefHelpDeskCategories.SingleOrDefaultAsync(x => x.CategoryId == id).Result;

                if (categoryNode == null)
                {
                    objDTOStatus.StatusMessage = $"id #{id} Not Found";
                    objDTOStatus.Success = false;
                    return objDTOStatus;
                }

                try
                {
                    // Get all Task Categories of the current item
                    var ColExistingTaskCategories = from objTaskCategory in context.AdefHelpDeskTaskCategories
                                                    where objTaskCategory.CategoryId == categoryNode.CategoryId
                                                    select objTaskCategory;

                    if (ColExistingTaskCategories.Count() > 0)
                    {
                        context.AdefHelpDeskTaskCategories.RemoveRange(ColExistingTaskCategories);
                        context.SaveChanges();
                    }

                    int? ParentNodeID = null;

                    // Possibly update Child Nodes
                    if (categoryNode.ParentCategoryId.HasValue)
                    {
                        // Get the Parent Node of the ExistingNode
                        ParentNodeID = categoryNode.ParentCategoryId.Value;
                    }

                    // Get the children of the current item
                    var ChildResults = from objNode in context.AdefHelpDeskCategories
                                       where objNode.ParentCategoryId.Value == categoryNode.CategoryId
                                       where objNode.ParentCategoryId.HasValue == true
                                       select objNode;

                    // Loop thru each Child of the current Node
                    foreach (var objChild in ChildResults)
                    {
                        // Update the Parent Node
                        // for the Child Node
                        objChild.ParentCategoryId = ParentNodeID;
                    }

                    context.AdefHelpDeskCategories.Remove(categoryNode);
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
