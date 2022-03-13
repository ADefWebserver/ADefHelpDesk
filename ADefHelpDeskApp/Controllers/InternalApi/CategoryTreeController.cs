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
using Microsoft.EntityFrameworkCore;
using ADefHelpDeskApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using AdefHelpDeskBase.Models.DataContext;

namespace ADefHelpDeskApp.Controllers.InternalApi
{
    public class CategoryTreeController
    {
        private IConfiguration _config { get; set; }
        private IMemoryCache _cache;

        public CategoryTreeController(
            IConfiguration config,
            IMemoryCache memoryCache)
        {
            _config = config;
            _cache = memoryCache;
        }

        #region public List<CategoryDTO> GetCategoryTree(bool RequestorVisible, bool UseCache)
        public List<CategoryDTO> GetCategoryTree(bool RequestorVisibleOnly, bool UseCache, List<int> SelectedNodes)
        {
            return GetNodesMethod(RequestorVisibleOnly, UseCache, _cache, SelectedNodes, GetConnectionString());
        }
        #endregion

        // Methods

        #region public List<CategoryDTO> GetNodesMethod(bool RequestorVisibleOnly, bool UseCache, IMemoryCache _cache, List<int> SelectedNodes, string strConectionString)
        public List<CategoryDTO> GetNodesMethod(bool RequestorVisibleOnly, bool UseCache, IMemoryCache _cache, List<int> SelectedNodes, string strConectionString)
        {
            // Collection to hold final TreeNodes
            List<CategoryDTO> colTreeNodes = new List<CategoryDTO>();

            if (Convert.ToBoolean(UseCache))
            {
                // Look for tree in cache
                if (RequestorVisibleOnly)
                {
                    if (_cache.TryGetValue("TreeNodesRequestorVisibleOnly", out colTreeNodes))
                    {
                        return colTreeNodes;
                    }
                }
                else
                {
                    if (_cache.TryGetValue("TreeNodesAll", out colTreeNodes))
                    {
                        return colTreeNodes;
                    }
                }
            }

            var optionsBuilder = new DbContextOptionsBuilder<ADefHelpDeskContext>();
            optionsBuilder.UseSqlServer(strConectionString);

            using (var context = new ADefHelpDeskContext(optionsBuilder.Options))
            {
                // This returns all Nodes in the database 
                List<CategoryNode> colNodes = new List<CategoryNode>();

                if (RequestorVisibleOnly)
                {
                    colNodes = (from objNode in context.AdefHelpDeskCategories
                                where objNode.RequestorVisible == true
                                select new CategoryNode
                                {
                                    Id = objNode.CategoryId,
                                    NodeName = objNode.CategoryName,
                                    ParentId = objNode.ParentCategoryId,
                                    Selectable = objNode.Selectable,
                                    RequestorVisible = objNode.RequestorVisible
                                }).OrderBy(x => x.ParentId).ThenBy(y => y.NodeName).ToList();
                }
                else
                {
                    colNodes = (from objNode in context.AdefHelpDeskCategories
                                select new CategoryNode
                                {
                                    Id = objNode.CategoryId,
                                    NodeName = objNode.CategoryName,
                                    ParentId = objNode.ParentCategoryId,
                                    Selectable = objNode.Selectable,
                                    RequestorVisible = objNode.RequestorVisible
                                }).OrderBy(x => x.ParentId).ThenBy(y => y.NodeName).ToList();
                }

                // Loop through Parent 'root' nodes
                // (meaning the NodeParentData is blank)
                colTreeNodes = new List<CategoryDTO>();
                foreach (CategoryNode objNode in colNodes
                    .Where(x => x.ParentId == null))
                {
                    // Create a new Node
                    CategoryDTO objNewNode = new CategoryDTO();

                    NodeDetailDTO objNewNodeDetail = new NodeDetailDTO();
                    objNewNodeDetail.categoryId = objNode.Id.ToString();
                    objNewNodeDetail.CheckboxChecked = (SelectedNodes.Contains(objNode.Id));
                    objNewNodeDetail.selectable = objNode.Selectable;
                    objNewNodeDetail.requestorVisible = objNode.RequestorVisible;
                    objNewNode.data = objNewNodeDetail;

                    objNewNode.categoryId = objNode.Id.ToString();
                    objNewNode.label = objNode.NodeName;
                    objNewNode.parentId = 0;
                    objNewNode.children = new List<CategoryDTO>();
                    objNewNode.selectable = objNode.Selectable;

                    if (objNode.Selectable == true)
                    {
                        if (objNode.RequestorVisible == true)
                        {
                            objNewNode.expandedIcon = "check_box";
                            objNewNode.collapsedIcon = "check_box_outline_blank";
                            objNewNode.type = "ShowCheckBox";
                        }
                        else
                        {
                            objNewNode.expandedIcon = "radio_button_checked";
                            objNewNode.collapsedIcon = "radio_button_unchecked";
                            objNewNode.type = "ShowCheckBox";
                        }
                    }

                    if (objNode.Selectable == false)
                    {
                        if (objNode.RequestorVisible == true)
                        {
                            objNewNode.expandedIcon = "apps";
                            objNewNode.collapsedIcon = "apps";
                            objNewNode.type = "HideCheckBox";
                        }
                        else
                        {
                            objNewNode.expandedIcon = "app_registration";
                            objNewNode.collapsedIcon = "app_registration";
                            objNewNode.type = "HideCheckBox";
                        }
                    }

                    colTreeNodes.Add(objNewNode);

                    // Add Child Nodes
                    AddChildren(colNodes, colTreeNodes, objNewNode, SelectedNodes);
                }
            }

            // Only save to Cache is UseCache is true
            if (UseCache)
            {
                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.MaxValue);

                // Save data in cache.
                if (RequestorVisibleOnly)
                {
                    _cache.Set("TreeNodesRequestorVisibleOnly", colTreeNodes, cacheEntryOptions);
                }
                else
                {
                    _cache.Set("TreeNodesAll", colTreeNodes, cacheEntryOptions);
                }
            }

            return colTreeNodes;
        }
        #endregion

        #region AddChildren
        private static void AddChildren(
            List<CategoryNode> colNodeItemCollection,
            List<CategoryDTO> colTreeNodeCollection,
            CategoryDTO paramTreeNode,
            List<int> SelectedNodes)
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

                objNewNode.categoryId = objChild.Id.ToString();
                objNewNode.label = objChild.NodeName;
                objNewNode.parentId = Convert.ToInt32(paramTreeNode.categoryId);
                objNewNode.children = new List<CategoryDTO>();
                objNewNode.selectable = objChild.Selectable;

                if (objChild.Selectable == true)
                {
                    if (objChild.RequestorVisible == true)
                    {
                        objNewNode.expandedIcon = "check_box";
                        objNewNode.collapsedIcon = "check_box_outline_blank";
                        objNewNode.type = "ShowCheckBox";
                    }
                    else
                    {
                        objNewNode.expandedIcon = "radio_button_checked";
                        objNewNode.collapsedIcon = "radio_button_unchecked";
                        objNewNode.type = "ShowCheckBox";
                    }
                }

                if (objChild.Selectable == false)
                {
                    if (objChild.RequestorVisible == true)
                    {
                        objNewNode.expandedIcon = "apps";
                        objNewNode.collapsedIcon = "apps";
                        objNewNode.type = "HideCheckBox";
                    }
                    else
                    {
                        objNewNode.expandedIcon = "app_registration";
                        objNewNode.collapsedIcon = "app_registration";
                        objNewNode.type = "HideCheckBox";
                    }
                }

                // Search for the Node in colTreeNodeCollection
                // By looping through each 'root' Node
                // (meaning the NodeParentData is blank)
                foreach (CategoryNode objNode in colNodeItemCollection
                    .Where(x => x.ParentId == null))
                {
                    // See if Parent is in the colTreeNodeCollection
                    CategoryDTO objParent =
                        colTreeNodeCollection.Where(x => x.categoryId == objNode.Id.ToString()).FirstOrDefault();

                    if (objParent != null) // Parent exists in the colTreeNodeCollection
                    {
                        // Get the Parent Node for the current Child Node
                        CategoryDTO objParentTreeNode = objParent.Descendants()
                            .Where(x => x.categoryId == paramTreeNode.categoryId).FirstOrDefault();

                        if (objParentTreeNode != null)
                        {
                            // Add the Child node to the Parent
                            NodeDetailDTO objNewNodeDetail = new NodeDetailDTO();
                            objNewNodeDetail.categoryId = objChild.Id.ToString();
                            objNewNodeDetail.CheckboxChecked = (SelectedNodes.Contains(objChild.Id));
                            objNewNodeDetail.selectable = objChild.Selectable;
                            objNewNodeDetail.requestorVisible = objChild.RequestorVisible;
                            objNewNode.data = objNewNodeDetail;

                            objParentTreeNode.children.Add(objNewNode);
                        }
                    }
                }

                //Recursively call the AddChildren method adding all children
                AddChildren(colNodeItemCollection, colTreeNodeCollection, objNewNode, SelectedNodes);
            }
        }
        #endregion

        // Utility

        #region public List<int> GetSelected(List<CategoryDTO> TreeNodes)
        public List<int> GetSelected(List<CategoryDTO> TreeNodes)
        {
            List<int> ColSelectedTreeNodes = new List<int>();
            // Get all the top level nodes
            foreach (var node in TreeNodes)
            {
                if (node.data.CheckboxChecked)
                {
                    ColSelectedTreeNodes.Add(Convert.ToInt32(node.categoryId));
                }

                // Recursively call the AddChildren method adding all children
                AddSelectedChildren(TreeNodes, ColSelectedTreeNodes, node);
            }
            return ColSelectedTreeNodes;
        }
        #endregion

        #region private void AddSelectedChildren(List<CategoryDTO> colNodeItemCollection, List<int> colTreeNodeCollection, CategoryDTO paramTreeNode)
        private void AddSelectedChildren(
            List<CategoryDTO> colNodeItemCollection,
            List<int> colTreeNodeCollection,
            CategoryDTO paramTreeNode)
        {
            // Get the children of the current item
            // This method may be called from the top level
            // or recursively by one of the child items
            // Loop thru each Child of the current Node
            foreach (var objChild in paramTreeNode.children)
            {
                if (objChild.data.CheckboxChecked)
                {
                    colTreeNodeCollection.Add(Convert.ToInt32(objChild.categoryId));
                }
                // Recursively call the AddChildren method adding all children
                AddSelectedChildren(colNodeItemCollection, colTreeNodeCollection, objChild);
            }
        }
        #endregion

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
