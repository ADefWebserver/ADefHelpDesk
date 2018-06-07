//
// ADefHelpDesk.com
// Copyright (c) 2017
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
using ADefHelpDeskApp.Models;
using AdefHelpDeskBase.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using AdefHelpDeskBase.Models.DataContext;

namespace ADefHelpDeskApp.Controllers.WebApi
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class CategoryNodesController : Controller
    {        
        private IConfigurationRoot _configRoot { get; set; }

        public CategoryNodesController(IConfigurationRoot configRoot)
        {
            _configRoot = configRoot;
        }

        // GET: api/CategoryNodes/GetCategoryNodes
        [AllowAnonymous]
        [HttpGet("[action]")]
        #region public List<CategoryDTO> GetCategoryNodes()
        public List<CategoryDTO> GetCategoryNodes()
        {
            return NewMethod();
        }

        private List<CategoryDTO> NewMethod()
        {
            // Collection to hold final TreeNodes
            List<CategoryDTO> colTreeNodes = new List<CategoryDTO>();

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // This returns all Nodes in the database 
                var colNodes = (from objNode in context.AdefHelpDeskCategories
                                select new CategoryNode
                                {
                                    Id = objNode.CategoryId,
                                    NodeName = objNode.CategoryName,
                                    ParentId = objNode.ParentCategoryId
                                }).OrderBy(x => x.ParentId).ThenBy(y => y.NodeName).ToList();

                // Create a '[None]' Node
                CategoryDTO objNoneNode = new CategoryDTO();

                objNoneNode.label = "[None]";
                objNoneNode.parentId = 0;
                colTreeNodes.Add(objNoneNode);

                // Loop through Parent 'root' nodes
                // (meaning the NodeParentData is blank)
                foreach (CategoryNode objNode in colNodes
                    .Where(x => x.ParentId == null))
                {
                    // Create a new Node
                    CategoryDTO objNewNode = new CategoryDTO();

                    NodeDetailDTO objNewNodeDetail = new NodeDetailDTO();
                    objNewNodeDetail.categoryId = objNode.Id.ToString();
                    objNewNodeDetail.CheckboxChecked = false;
                    objNewNodeDetail.selectable = objNode.Selectable;
                    objNewNodeDetail.requestorVisible = objNode.RequestorVisible;
                    objNewNode.data = objNewNodeDetail;

                    objNewNode.categoryId = objNode.Id.ToString();
                    objNewNode.label = objNode.NodeName;
                    objNewNode.selectable = objNode.Selectable;

                    if (objNode.ParentId.ToString() != "")
                    {
                        objNewNode.parentId = Convert.ToInt32(objNode.ParentId);
                    }

                    colTreeNodes.Add(objNewNode);

                    // Add Nodes
                    AddNodes(colNodes, colTreeNodes, objNewNode);
                }
            }

            // This is not Queryable because we need the list
            // to stay in the correct order (by NodeParentData)
            return colTreeNodes;
        }
        #endregion

        // Utility

        #region AddNodes
        private void AddNodes(
            List<CategoryNode> colNodeItemCollection,
            List<CategoryDTO> colTreeNodeCollection,
            CategoryDTO paramTreeNode)
        {
            // Get the children of the current item
            // This method may be called from the top level 
            // or recuresively by one of the child items
            var ChildResults = from objNode in colNodeItemCollection
                               where objNode.ParentId == Convert.ToInt32(paramTreeNode.categoryId)
                               select objNode;

            // Loop thru each Child of the current Node
            foreach (var objChild in ChildResults)
            {
                // Create a new Node
                var objNewNode = new CategoryDTO();

                NodeDetailDTO objNewNodeDetail = new NodeDetailDTO();
                objNewNodeDetail.categoryId = objChild.Id.ToString();
                objNewNodeDetail.CheckboxChecked = false;
                objNewNodeDetail.selectable = objChild.Selectable;
                objNewNodeDetail.requestorVisible = objChild.RequestorVisible;
                objNewNode.data = objNewNodeDetail;

                objNewNode.categoryId = objChild.Id.ToString();
                objNewNode.label = objChild.NodeName;
                objNewNode.selectable = objChild.Selectable;

                // See if there is a Parent
                if (objChild.ParentId != null)
                {
                    // Set the Parent
                    objNewNode.parentId = Convert.ToInt32(objChild.ParentId);

                    // Get the Parent
                    CategoryDTO objParent =
                        colTreeNodeCollection.Where(x => x.categoryId == objChild.ParentId.ToString()).FirstOrDefault();

                    // See how many dots the Parent has
                    int CountOfParentDots = objParent.label.Count(x => x == '.');

                    // Update the label to add dots in front of the name
                    objNewNode.label = $"{AddDots(CountOfParentDots + 1)}{objChild.NodeName}";
                }
                else
                {
                    // There was no parent so don't add any dots
                    objNewNode.label = objChild.NodeName;
                }

                colTreeNodeCollection.Add(objNewNode);

                //Recursively call the AddChildren method adding all children                
                AddNodes(colNodeItemCollection, colTreeNodeCollection, objNewNode);
            }
        }
        #endregion

        #region AddDots
        private static string AddDots(int intDots)
        {
            String strDots = "";

            for (int i = 0; i < intDots; i++)
            {
                strDots += ". ";
            }

            return strDots;
        }
        #endregion

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
