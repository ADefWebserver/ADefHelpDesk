import { Component, OnInit } from '@angular/core';
import { Http } from '@angular/http';
import {
    InputTextModule,
    DropdownModule,
    ButtonModule,
    GrowlModule,
    Message,
    MessagesModule,
    FieldsetModule,
    TreeModule,
    TreeNode,
    SelectItem
} from 'primeng/primeng';

import { CategoryService } from '../services/web/category.service';
import { ICategory } from '../classes/category';
import { INodeDetail } from '../classes/category';
import { ICategoryNode } from '../classes/categoryNode';

@Component({
    selector: 'tagsadmin',
    templateUrl: './tagsadmin.component.html'
})
export class TagsadminComponent implements OnInit {

    public msgs: Message[] = [];
    public applicationsList: SelectItem[] = [];
    public categories: ICategory[] = [];

    public bShowCategory: boolean = false;
    public treeNodes: TreeNode[];
    public EditModeLabel: string = "Edit Node";
    // Any error messages
    public errorMessage: string;
    public selectedNode: TreeNode;
    public editNodeName: string;
    public nodeParents: TreeNode[];
    public selectedNodeParent: SelectItem;
    public nodeParentsDropdown: SelectItem[] = [];

    // Contructor is called when the class is created
    constructor(private _CategoryService: CategoryService) { }

    ngOnInit() {
        // Create and set default node
        this.selectedNode = this.createNewCategory();

        // Populate Screen
        this.populateTree();
        this.populateDropdown();

        // Start in New Node Mode
        this.NewNode();
    }

    populateTree() {
        this.treeNodes = [];

        // Call the service to get the Tree
        this._CategoryService.getTreeCategorys(false).subscribe((categories: ICategory[]) => {

            this.treeNodes = categories;

            // Expand all the Tree nodes
            this.expandAll();
        });
    }

    populateDropdown() {
        // Call the service
        this._CategoryService.getCategorys().subscribe((nodes: ICategory[]) => {

            // Clear the list
            this.nodeParentsDropdown = [];

            // Loop through the returned Nodes
            for (let node of nodes) {

                // Create a new SelectedItem
                let newSelectedItem: SelectItem = {
                    label: node.label,
                    value: node.categoryId
                }

                // Add Selected Item to the DropDown
                this.nodeParentsDropdown.push(newSelectedItem);
            }

            // Set the selected option to the first option
            this.selectedNodeParent = this.nodeParentsDropdown[0];
        },
            error => this.errorMessage = <any>error);
    }

    nodeSelect() {
        // We are editing
        this.EditModeLabel = "Edit Node";

        // Set the edit node label
        this.editNodeName = this.selectedNode.label

        // Set the Edit Node parent
        var selectedTreeNode: SelectItem;
        if (this.selectedNode.parent !== undefined) {
            selectedTreeNode = this.nodeParentsDropdown.find(x => x.value == this.selectedNode.parent.data.categoryId);
        } else {
            // Select the '[None]' node
            // It is always the first option in the list
            selectedTreeNode = this.nodeParentsDropdown[0];
        }

        // Set the selected option to update the DropDown
        this.selectedNodeParent = selectedTreeNode.value;
    }

    NewNode() {
        // We are creating a new node
        this.EditModeLabel = "New Node";

        // Set the Edit Node parent
        var selectedTreeNode: SelectItem;
        if (this.selectedNode !== undefined) {
            selectedTreeNode = this.nodeParentsDropdown.find(x => x.value == this.selectedNode.data.categoryId);
            if (selectedTreeNode !== undefined) {
                // Set the selected option to update the DropDown
                this.selectedNodeParent = selectedTreeNode.value;
            }
        }

        // Set selectedNode to a new Category
        this.selectedNode = this.createNewCategory();
    }

    createNewCategory() {

        // Create a new Category
        let newNodeDetail: INodeDetail = {
            categoryId: "-1",
            checkboxChecked: false,
            selectable: true,
            requestorVisible: true
        }

        let newCategory: ICategory = {
            label: "",
            categoryId: -1, // So we know it is a new Node
            parentId: 0,
            selectable: true,            
            type: "",
            data: newNodeDetail
        }

        return newCategory;
    }

    Save() {
        this.errorMessage = "";

        // Validate Inputs
        if (this.selectedNode.label == "") {
            this.errorMessage = "Node Name cannot be blank";
            return;
        }

        // Create an CategoryNode
        // This will be used to update the database
        let objCategoryNode: ICategoryNode = {
            Id: this.selectedNode.data.categoryId,
            NodeName: this.selectedNode.label,
            ParentId: 0,
            Selectable: this.selectedNode.data.selectable,
            RequestorVisible: this.selectedNode.data.requestorVisible
        }

        // Is this a new CategoryNode?
        if (objCategoryNode.Id == -1) {

            if (this.selectedNodeParent !== null) {
                // Set the ParentId
                objCategoryNode.ParentId =
                    Number(this.selectedNodeParent.value
                        ? this.selectedNodeParent.value
                        : this.selectedNodeParent);
            } else {
                objCategoryNode.ParentId = 0;
            }

            // Call the service to Insert the CategoryNode
            this._CategoryService.createCategoryNode(objCategoryNode)
                .subscribe(() => {

                    // Create a new Category Node
                    this.selectedNode = this.createNewCategory();
                    // Refresh 
                    this.populateTree();
                    this.populateDropdown();
                    // Set NewNode Mode
                    this.NewNode();
                },
                error => this.errorMessage = <any>error);
        } else {
            // A Node cannot be set as a parent to itself
            if (this.selectedNodeParent !== this.selectedNode.data.categoryId) {

                // Set the ParentId
                objCategoryNode.ParentId = Number(this.selectedNodeParent);

                // Call the service to update the Category
                this._CategoryService.updateCategoryNode(objCategoryNode)
                    .subscribe(() => {

                        // Create a new Category Node
                        this.selectedNode = this.createNewCategory();
                        // Refresh 
                        this.populateTree();
                        this.populateDropdown();
                        // Set NewNode Mode
                        this.NewNode();
                    },
                    error => this.errorMessage = <any>error);
            }
        }
    }

    DeleteNode() {
        this.errorMessage = "";
        // Get NodeId
        var NodeId: number = Number(this.selectedNode.data.categoryId);

        // Only a NodeId other than -1 can be deleted
        if (NodeId > -1) {
            // Call the service to delete the Node
            this._CategoryService.deleteTreeNode(NodeId)
                .subscribe(() => {
                    // Refresh 
                    this.populateTree();
                    this.populateDropdown();
                    // Set NewNode Mode
                    this.NewNode();
                },
                error => this.errorMessage = <any>error);
        }
    }

    // Utility

    expandAll() {
        this.treeNodes.forEach(node => {
            this.expandRecursive(node, true);
        });
    }

    private expandRecursive(node: TreeNode, isExpand: boolean) {
        node.expanded = isExpand;
        if (node.children) {
            node.children.forEach(childNode => {
                this.expandRecursive(childNode, isExpand);
            });
        }
    }
}