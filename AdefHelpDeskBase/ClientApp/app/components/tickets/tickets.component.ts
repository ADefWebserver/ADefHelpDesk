import { Component, ViewChild, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { DatePipe } from '@angular/common'
import { Subscription } from 'rxjs/Subscription';
import { LazyLoadEvent } from 'primeng/components/common/api';
import {
    InputTextModule,
    DropdownModule,
    ButtonModule,
    TreeModule,
    TreeNode,
    InputMaskModule,
    SelectItem,
    MenuItem,
    DataTable,
    FileUpload,
    DataListModule
} from 'primeng/primeng';

import { ISearchTaskParameters } from '../classes/searchTaskParameters';
import { ITaskSearchResult } from '../classes/taskSearchResult';
import { TaskService } from '../services/web/task.service';
import { ITask } from '../classes/task';

import { DialogService } from '../services/internal/dialog.service';
import { LoginService } from '../services/internal/login.service';
import { UserService } from '../services/web/user.service';
import { SettingsService } from '../services/web/settings.service';
import { RoleService } from '../services/web/role.service';
import { CategoryService } from '../services/web/category.service';
import { TaskVisibilityService } from '../services/internal/taskVisibility.service';

import { IUserSearchResult } from '../classes/userSearchResult';
import { IUser } from '../classes/user';
import { IStatus } from '../classes/status';
import { IRole } from '../classes/role';

import { ICategory } from '../classes/category';
import { INodeDetail } from '../classes/category';
import { ICategoryNode } from '../classes/categoryNode';

import { ISearchParameters } from '../classes/searchParameters';
import { ILogSearchResult } from '../classes/logSearchResult';
import { IDTOTaskDetailResponse } from "../classes/dtoTaskDetailResponse";

@Component({
    selector: 'tickets',
    templateUrl: './tickets.component.html',
    styleUrls: ['./tickets.component.css']
})
export class TicketsComponent implements OnInit, AfterViewInit {
    @ViewChild('datatable') dataTable: DataTable;
    TaskVisibilityService: Subscription;

    public errorMessage: string;
    public isAdmin: boolean = false;
    public user: IUser;
    public manualUser: IUser;
    public UpdateResponse: IStatus;
    showWaitGraphic: boolean = false;
    searchButtonItems: MenuItem[];
    isPageNew = true;

    searchString: string = "";
    searchStringUser: string = "";
    totalTasks: number = 0;
    rowsPerPage: number = 0;
    pageNumber: number;
    pageNumberUser: number;
    first: number;
    SearchResults: ITask[];
    SearchResultsUsers: IUser[];
    searchError: string;

    rowsPerPageLog: number = 5;
    rowsPerPageManual: number = 10;
    pageNumberLog: number = 1;
    totalLogs: number;
    totalUsers: number = 0;
    searchErrorLog: string = "";
    TicketHeader: string = "";

    public SelectedTask: ITask;
    public EditIndex: number = 0;
    public treeNodes: TreeNode[];
    public treeNodesEdit: TreeNode[];
    public selectedTreeNodes: TreeNode[] = [];
    public selectedTreeNodesEdit: TreeNode[] = [];
    public statusDropdown: SelectItem[] = [];
    public statusDropdownEdit: SelectItem[] = [];
    public priorityDropdown: SelectItem[] = [];
    public priorityDropdownEdit: SelectItem[] = [];
    public rolesDropdown: SelectItem[] = [];

    boolTreeNodesLoaded = false;
    boolStatusDropdownLoaded = false;
    boolPriorityDropdownLoaded = false;
    boolRolesDropdownLoaded = false;

    public paramId: string = "";
    public paramStatus: string = "All";
    public paramPriority: string = "All";
    public paramDueDate: string;
    public paramCreated: string;
    public paramAssignedRole: IRole;
    public paramSortField: string;
    public paramSortOrder: string;
    public paramSelectedTreeNodes: number[] = []
    public searchParameters: ISearchTaskParameters;

     constructor(
         private _taskService: TaskService,
         private _taskVisibilityService: TaskVisibilityService,
         private _settingsService: SettingsService,
         private _dialogService: DialogService,
         private _CategoryService: CategoryService,
         private _RoleService: RoleService,
         private _userService: UserService,
         private _loginService: LoginService,
         private _datepipe: DatePipe
     ) { }

    ngOnInit() {
        this.rowsPerPage = 10;
        this.pageNumber = 1;
        this.first = 0;

        this.populatePriorityDropdown();
        this.populateRolesDropdown();
        this.populateStatusDropdown();
        this.populateSearchButtonMenu();
    }

    ngAfterViewInit() {

        this._userService.getCurrentUser().subscribe(
            (user: IUser) => {
                // Set the current user
                this.user = user;
                
                // Calculate isAdmin ***************
                this.isAdmin = ((this.user.userRoles.length > 0) || (this.user.isSuperUser));

                // Call the service to get the Tree(s)
                this.treeNodes = [];
                if (this.isAdmin) {
                    this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                        this.treeNodes = categories;
                        this.boolTreeNodesLoaded = true;
                        this.loadLastSearchParameters();
                    });
                    this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                        this.treeNodesEdit = categories;
                    });
                } else {
                    this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                        this.treeNodes = categories.filter(x => x.data.requestorVisible == true);
                        this.boolTreeNodesLoaded = true;
                        this.loadLastSearchParameters();
                    });
                    this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                        this.treeNodesEdit = categories.filter(x => x.data.requestorVisible == true);
                    });
                }

            });

        // Subscribe to the TaskVisibility Service
        this.TaskVisibilityService = this._taskVisibilityService.getTaskDialogVisibility().subscribe(
            (paramTaskParameter: ITask) => {
                // Update the selected task in the task list

                if (this.SearchResults !== undefined) {
                    // If the Task is viewable in the Search Results - update it
                    for (let objTask of this.SearchResults) {
                        if (objTask.taskId == paramTaskParameter.taskId) {

                            if (paramTaskParameter.status == "Deleted") {
                                // Remove the task from the list
                                // Create array that has the task removed
                                var colTasks = this.SearchResults.filter(task => task.taskId !== objTask.taskId);
                                this.SearchResults = colTasks;
                                return;
                            }

                            // Update the visible Task fields
                            objTask.status = paramTaskParameter.status;
                            objTask.priority = paramTaskParameter.priority;
                            objTask.requesterName = paramTaskParameter.requesterName;
                            objTask.description = paramTaskParameter.description;

                            // Set objTask.dueDate
                            if (paramTaskParameter.dueDate !== null && paramTaskParameter.dueDate !== "") {
                                objTask.dueDate = paramTaskParameter.dueDate;
                            } else {
                                objTask.dueDate = "";
                            }

                            // Get AssignedRole
                            let selectedRole: SelectItem;
                            if (paramTaskParameter.assignedRoleId !== undefined && paramTaskParameter.assignedRoleId !== "") {
                                let intSelectedRoleId: number = Number(paramTaskParameter.assignedRoleId);
                                // try to find it in the dropdown
                                selectedRole = this.rolesDropdown.find(x => x.value.iD == intSelectedRoleId);
                                objTask.assignedRoleName = selectedRole.label;
                            }
                        }
                    }
                }

            });
    }

    populateSearchButtonMenu() {
        this.searchButtonItems = [
            {
                label: 'Clear Search', icon: 'fa-refresh', command: () => {
                    this.clearSearch();
                }
            },          
        ];
    }

    clearSearch() {
        this.searchString = "";
        this.paramCreated = "";
        this.paramDueDate = "";
        this.paramPriority = "";
        this.rowsPerPage = 10;
        this.paramStatus = "";

        let selectedRole = this.rolesDropdown.find(x => x.value.iD == 0);
        if (selectedRole !== undefined) {
            this.paramAssignedRole = selectedRole.value;
        }

        // Clear Tree nodes
        this.selectedTreeNodes = [];
        for (let treeNode of this.treeNodes) {
            treeNode.data.checkboxChecked = false;

            if (treeNode.children != null) {
                this.clearChildCategory(treeNode.children);
            }
        }

        this.searchTasksList();
    }

    private clearChildCategory(childTreeNodes: TreeNode[]) {

        for (let childNode of childTreeNodes) {
            childNode.data.checkboxChecked = false;

            if (childNode.children != null) {
                this.clearChildCategory(childNode.children);
            }
        }
    }

    populatePriorityDropdown() {

        // Clear the list
        this.priorityDropdownEdit = [];

        // Add Items to the Priority DropDown Edit
        this.priorityDropdownEdit.push({ label: "Normal", value: 'Normal' });
        this.priorityDropdownEdit.push({ label: "High", value: 'High' });
        this.priorityDropdownEdit.push({ label: "Low", value: 'Low' });

        // Clear the list
        this.priorityDropdown = [];

        // Add Items to the Priority DropDown
        this.priorityDropdown.push({ label: "*All*", value: 'All' });
        this.priorityDropdown.push({ label: "Normal", value: 'Normal' });
        this.priorityDropdown.push({ label: "High", value: 'High' });
        this.priorityDropdown.push({ label: "Low", value: 'Low' });

        this.boolPriorityDropdownLoaded = true;
        this.loadLastSearchParameters();
    }

    populateStatusDropdown() {

        // Clear the list
        this.statusDropdownEdit = [];

        // Add Items to the DropDown Edit
        this.statusDropdownEdit.push({ label: "New", value: 'New' });
        this.statusDropdownEdit.push({ label: "Active", value: 'Active' });
        this.statusDropdownEdit.push({ label: "Cancelled", value: 'Cancelled' });
        this.statusDropdownEdit.push({ label: "On Hold", value: 'On Hold' });
        this.statusDropdownEdit.push({ label: "Resolved", value: 'Resolved' });

        // Clear the list
        this.statusDropdown = [];

        // Add Items to the DropDown
        this.statusDropdown.push({ label: "*All*", value: 'All' });
        this.statusDropdown.push({ label: "New", value: 'New' });
        this.statusDropdown.push({ label: "Active", value: 'Active' });
        this.statusDropdown.push({ label: "Cancelled", value: 'Cancelled' });
        this.statusDropdown.push({ label: "On Hold", value: 'On Hold' });
        this.statusDropdown.push({ label: "Resolved", value: 'Resolved' });

        this.boolStatusDropdownLoaded = true;
        this.loadLastSearchParameters();
    }

    populateRolesDropdown() {
        // Call the service
        this._RoleService.getRoles().subscribe((roles: IRole[]) => {

            // Clear the list
            this.rolesDropdown = [];

            // Create All Role
            let allRole: IRole = {
                iD: 0,
                portalID: 0,
                roleName: '*All*'
            }

            let newSelectedItem: SelectItem = {
                label: "*All*",
                value: allRole
            }

            // Add Item to the DropDown
            this.rolesDropdown.push(newSelectedItem);

            // Create Unassigned Role
            let unassignedRole: IRole = {
                iD: -1,
                portalID: 0,
                roleName: '[Unassigned]'
            }

            let newUnassignedItem: SelectItem = {
                label: "[Unassigned]",
                value: unassignedRole
            }

            // Add Item to the DropDown
            this.rolesDropdown.push(newUnassignedItem);

            // Loop through the returned Roles
            for (let role of roles) {

                // Create a new SelectedItem
                let newSelectedItem: SelectItem = {
                    label: role.roleName,
                    value: role
                }

                // Add Selected Item to the DropDown
                this.rolesDropdown.push(newSelectedItem);
            }

            this.boolRolesDropdownLoaded = true;
            this.loadLastSearchParameters();
        },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    // Tasks

    loadLastSearchParameters() {

        // Only load if all dropdowns have been loaded
        if ((this.boolTreeNodesLoaded == true) &&
            (this.boolStatusDropdownLoaded == true) &&
            (this.boolPriorityDropdownLoaded == true) &&
            (this.boolRolesDropdownLoaded == true)) {

            this._taskService.getSearchParameters()
                .subscribe((SearchTaskParameters: ISearchTaskParameters) => {
                    this.searchParameters = SearchTaskParameters;

                    this.searchString = this.searchParameters.searchText;                    
                    this.paramCreated = this._datepipe.transform(this.searchParameters.createdDate, 'MM/dd/yyyy');
                    this.paramDueDate = this._datepipe.transform(this.searchParameters.dueDate, 'MM/dd/yyyy');
                    
                    this.paramPriority = this.searchParameters.priority;
                    this.rowsPerPage = this.searchParameters.rowsPerPage; 
                    this.dataTable.rows = this.searchParameters.rowsPerPage; 
                    this.paramSortField = this.searchParameters.sortField;
                    this.paramSortOrder = this.searchParameters.sortOrder;
                    this.paramStatus = this.searchParameters.status;

                    // Set AssignedRole
                    if (this.searchParameters.assignedRoleId !== undefined && this.searchParameters.assignedRoleId !== "") {
                        let intSelectedRoleId: number = Number(this.searchParameters.assignedRoleId);
                        // try to find it in the dropdown
                        let selectedRole = this.rolesDropdown.find(x => x.value.iD == intSelectedRoleId);
  
                        if (selectedRole !== undefined) {
                            this.paramAssignedRole = selectedRole.value;
                        }
                    }

                    this.setToLastSavedPage();

                    if (SearchTaskParameters.selectedTreeNodes !== null) {
                        // Set selected tree nodes
                        for (let category of SearchTaskParameters.selectedTreeNodes) {

                            for (let treeNode of this.treeNodes) {
                                if (category == treeNode.data.categoryId) {
                                    treeNode.data.checkboxChecked = true;
                                    this.OnchangeNode(treeNode);
                                }

                                if (treeNode.children != null) {
                                    this.setSelectedChildCategory(category, treeNode.children);
                                }
                            }
                        }
                    }

                    // Expand all the tree nodes
                    this.treeNodes.forEach(node => {
                        this.expandRecursive(node, true);
                    });

                    this.populateTasksList();
            });
        }
    }

    private setSelectedChildCategory(category: number, childTreeNodes: TreeNode[]) {

        for (let childNode of childTreeNodes) {
            if (childNode.data.categoryId == category) {
                childNode.data.checkboxChecked = true;
                this.OnchangeNode(childNode);
            }

            if (childNode.children != null)
                this.setSelectedChildCategory(category, childNode.children);
        }
    }

    loadTasksLazy(event: LazyLoadEvent) {
        this.rowsPerPage = event.rows;
        this.pageNumber = Math.floor(event.first / event.rows) + 1;
        // Do not load tasks on first page load
        if (!this.isPageNew) {
            this.populateTasksList();
            this.saveSearchParameters();
        }
    }

    searchTasksList() {
        // On a search always set to the first page
        this.pageNumber = 1;

        this.saveSearchParameters();
        this.populateTasksList();
        this.setToLastSavedPage();
    }

    saveSearchParameters() {
        this.getSearchParameters();
        this._taskService.saveSearchParameters(this.searchParameters).subscribe(
            () => { },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });      
    }

    setToLastSavedPage() {
        // Try to set page if a value is saved
        if ((this.searchParameters.pageNumber !== undefined)
            && (this.searchParameters.rowsPerPage !== undefined)) {

            // See if there are enough records to set the page
            let intStartingRecord = ((this.searchParameters.pageNumber - 1) * this.searchParameters.rowsPerPage);

            // Set the page
            this.dataTable.first = intStartingRecord;
        }
    }

    populateTasksList() {
        this.isPageNew = false;
        this.getSearchParameters();        

        this._taskService.searchTasks(this.searchParameters).subscribe(
            (tasksResults: ITaskSearchResult) => {
                if (tasksResults.errorMessage.length < 1) {
                    this.SearchResults = tasksResults.taskList;
                    this.totalTasks = tasksResults.totalRows;
                } else {
                    this.searchError = tasksResults.errorMessage;
                    this._dialogService.setMessage(this.searchError);
                }               
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    public changeSort(event) {
        // Set Sort parameters
        this.paramSortOrder = (event.order == 1) ? 'asc' : 'desc';
        this.paramSortField = event.field;
    }

    public OnchangeNode(node: TreeNode) {
        // Adds/Removes tree nodes to this.selectedTreeNodes
        // On change event for the Checkbox Checked/Unchecked Event
        let newSelectedNode: TreeNode = node;

        // If check box is checked Add
        if (node.data.checkboxChecked === true) {
            // Add the Node
            this.selectedTreeNodes = [...this.selectedTreeNodes, newSelectedNode];
        }
        else // Check box is unchecked so remove 
        {
            // Create array that has the Node removed
            var colNodes = this.selectedTreeNodes.filter(obj => obj.data.categoryId !== newSelectedNode.data.categoryId);

            // Clear 
            this.selectedTreeNodes = [];

            //Re-add
            for (let node of colNodes) {
                this.selectedTreeNodes.push(node);
            }
        }
    }

    public OnchangeNodeEdit(node: TreeNode) {
        // Adds/Removes tree nodes to this.selectedTreeNodes
        // On change event for the Checkbox Checked/Unchecked Event
        let newSelectedNode: TreeNode = node;

        // If check box is checked Add
        if (node.data.checkboxChecked === true) {
            // Add the Node
            this.selectedTreeNodesEdit = [...this.selectedTreeNodesEdit, newSelectedNode];
        }
        else // Check box is unchecked so remove 
        {
            // Create array that has the Node removed
            var colNodes = this.selectedTreeNodesEdit.filter(obj => obj.data.categoryId !== newSelectedNode.data.categoryId);

            // Clear 
            this.selectedTreeNodesEdit = [];

            //Re-add
            for (let node of colNodes) {
                this.selectedTreeNodesEdit.push(node);
            }
        }
    }

    private expandRecursive(node: TreeNode, isExpand: boolean) {
        node.expanded = isExpand;
        if (node.children) {
            node.children.forEach(childNode => {
                this.expandRecursive(childNode, isExpand);
            });
        }
    }

    // ** Ticket **

    onTicketSelect(event) {

        let selectedTask: ITask = {
            taskId: event.data.taskId,
            ticketPassword: event.data.ticketPassword
        }

        this._taskVisibilityService.setTaskVisibility(selectedTask)
    }

    // Utility

    getSearchParameters() {
        this.searchParameters = {
            rowsPerPage: this.rowsPerPage,
            pageNumber: this.pageNumber,
            searchText: this.searchString,
            status: this.paramStatus,
            priority: this.paramPriority,
            createdDate: this.paramCreated,
            dueDate: this.paramDueDate,
            sortOrder: this.paramSortOrder,
            sortField: this.paramSortField
        }

        // Set AssignedRole
        if (this.paramAssignedRole != undefined) {
            this.searchParameters.assignedRoleId = this.paramAssignedRole.iD.toString();
        } else {
            this.searchParameters.assignedRoleId = "-1";
        }

        // Set selectedTreeNodes
        this.paramSelectedTreeNodes = [];
        for (let node of this.selectedTreeNodes) {
            this.paramSelectedTreeNodes.push(node.data.categoryId);
        }
        this.searchParameters.selectedTreeNodes = this.paramSelectedTreeNodes;
    }

    convertDateToString(paramDate:Date) {
        //return string
        var returnDate = "";

        // If date is not in the long format -- return it
        if (paramDate.toString().length < 11){
            return paramDate.toString();
        }

        //split
        var dd = paramDate.getDate();
        var mm = paramDate.getMonth() + 1; //because January is 0! 
        var yyyy = paramDate.getFullYear();

        //Interpolation date

        if (mm < 10) {
            returnDate += `0${mm}/`;
        } else {
            returnDate += `${mm}/`;
        }

        if (dd < 10) {
            returnDate += `0${dd}/`;
        } else {
            returnDate += `${dd}/`;
        }

        returnDate += yyyy;
        return returnDate;
    }

    ngOnDestroy(): void {
        // Important - Unsubscribe from any subscriptions
        if (this.TaskVisibilityService !== undefined) {
            this.TaskVisibilityService.unsubscribe();
        }
    }
}