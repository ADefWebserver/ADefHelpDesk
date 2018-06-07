import { Component, ViewChild, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { DatePipe } from '@angular/common'
import { FormBuilder, Validators } from '@angular/forms';
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
import { ITaskDetail, IAttachment } from '../classes/task';

import { DialogService } from '../services/internal/dialog.service';
import { LoginService } from '../services/internal/login.service';
import { TaskVisibilityService } from '../services/internal/taskVisibility.service';
import { QueryStringService } from '../services/internal/queryString.service';
import { UserService } from '../services/web/user.service';
import { UserManagerService } from '../services/web/userManager.service';
import { SettingsService } from '../services/web/settings.service';
import { RoleService } from '../services/web/role.service';
import { CategoryService } from '../services/web/category.service';
import { LogService } from '../services/web/log.service';
import { FilesService } from "../services/web/files.service";

import { IUserSearchResult } from '../classes/userSearchResult';
import { IUser } from '../classes/user';
import { IStatus } from '../classes/status';
import { IRole } from '../classes/role';

import { ICategory } from '../classes/category';
import { INodeDetail } from '../classes/category';
import { ICategoryNode } from '../classes/categoryNode';

import { ILog } from '../classes/log';
import { ISearchParameters } from '../classes/searchParameters';
import { IQueryStringParameter } from '../classes/querystringParameters';
import { ILogSearchResult } from '../classes/logSearchResult';
import { IDTOTaskDetailResponse } from "../classes/dtoTaskDetailResponse";
import { IDTOFileParameter } from '../classes/DTOFileParameter';
import { ITaskParameter } from '../classes/taskParameter';

@Component({
    selector: 'ticketEdit',
    templateUrl: './ticketEdit.component.html',
    styleUrls: ['./ticketEdit.component.css']
})
export class TicketEditComponent implements OnInit, AfterViewInit, OnDestroy {
    @ViewChild('tree') treeModule: TreeModule;
    @ViewChild('fileInput') fileInput: FileUpload;

    QueryStringSubscription: Subscription;
    TaskVisibilityService: Subscription;

    public UploadResponse: IDTOTaskDetailResponse;
    public errorMessage: string;
    public isAdmin: boolean = false;
    public user: IUser;
    public manualUser: IUser;
    public showManualUser: boolean;
    public UpdateResponse: IStatus;
    showWaitGraphic: boolean = false;
    boolSendEmail: boolean = false;
    searchButtonItems: MenuItem[];
    isPageNew = true;

    searchString: string = "";
    searchStringUser: string = "";
    totalTasks: number = 0;
    rowsPerPage: number = 0;
    pageNumber: number;
    pageNumberUser: number;
    first: number;
    SearchResultsUsers: IUser[];
    searchError: string;

    rowsPerPageLog: number = 5;
    rowsPerPageManual: number = 10;
    pageNumberLog: number = 1;
    totalLogs: number;
    totalUsers: number = 0;
    SearchResultsLog: ILog[];
    taskDetailsComments: ITaskDetail[];
    taskDetailsWork: ITaskDetail[];
    searchErrorLog: string = "";
    TicketHeader: string = "";

    public SelectedTask: ITask;
    public SelectedTaskDetail: ITaskDetail;
    public SelectedTaskDetailOriginal: ITaskDetail;
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
    public rolesDropdownEdit: SelectItem[] = [];

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

    // Comments Dialog Edit
    public boolVisibleToRequester: boolean = true;
    public boolHideForNonAdmin: boolean = false;
    public showUpload: boolean = false;
    public hasFile: boolean = false;

    public displaySelectUserDialog: boolean = false;
    public displayEditCommentDialog: boolean = false;
    public displayEditWorkDialog: boolean = false;
    public displayConfirmDeleteDialog: boolean = false;
    public displayConfirmDeleteTaskDialog: boolean = false;

    public taskForm = this.fb.group({
        name: [""],
        email: [""],
        phone: [""],
        description: [""],
        detail: [""],
        dueDate: [""],
        priority: ["Normal"],
        status: [""],
        assignedRole: [""],
        estimatedStart: [""],
        estimatedCompletion: [""],
        estimatedHours: [""]
    });

    constructor(
        public fb: FormBuilder,
        private _taskService: TaskService,
        private _taskVisibilityService: TaskVisibilityService,
        private _queryStringService: QueryStringService,
        private _settingsService: SettingsService,
        private _dialogService: DialogService,
        private _CategoryService: CategoryService,
        private _RoleService: RoleService,
        private _userService: UserService,
        private _UserManagerService: UserManagerService,
        private _loginService: LoginService,
        private _FilesService: FilesService,
        private _LogService: LogService,
        private _datepipe: DatePipe) { }

    ngOnInit() {
        this.rowsPerPage = 10;
        this.pageNumber = 1;
        this.first = 0;

        this.SelectedTaskDetail = {
            detailId: -1,
            description: '',
            userId: '-1',
            userName: 'current user',
            insertDate: 'current date',
            startTime: '',
            stopTime: '',
            colDTOAttachment: []
        }

        let fileAttachment: IAttachment = {
            attachmentID: -1,
            attachmentPath: '',
            fileName: '',
            originalFileName: '',
            userId: ''
        }

        this.SelectedTaskDetail.colDTOAttachment.push(fileAttachment);

        this.populatePriorityDropdown();
        this.populateStatusDropdown();
    }

    ngAfterViewInit() {

        // Subscribe to the TaskVisibility Service
        this.TaskVisibilityService = this._taskVisibilityService.getTaskVisibility().subscribe(
            (paramTaskParameter: ITask) => {
                if (paramTaskParameter.taskId != undefined) {
                    this.populateRolesDropdown();

                    // Get the code if we dont have it
                    if (paramTaskParameter.ticketPassword == undefined) {

                        let parameters: ISearchTaskParameters = {
                            rowsPerPage: 1,
                            pageNumber: 1,
                            selectedTreeNodes: [],
                            searchText: paramTaskParameter.taskId.toString()
                        }

                        this._taskService.searchTasks(parameters).subscribe(
                            (tasksResults: ITaskSearchResult) => {
                                if ((tasksResults.errorMessage.length < 1) && (tasksResults.taskList[0] != undefined)) {

                                    // Open the Ticket
                                    let selectedTask: ITask = {
                                        taskId: paramTaskParameter.taskId,
                                        ticketPassword: tasksResults.taskList[0].ticketPassword
                                    }

                                    this.selectTicket(selectedTask);

                                } else {
                                    this.searchError = tasksResults.errorMessage;
                                    this._dialogService.setMessage(this.searchError);
                                }
                            },
                            error => {
                                this.errorMessage = <any>error;
                                this._dialogService.setMessage(this.errorMessage);
                            });
                    } else {

                        // Open the Ticket
                        let selectedTask: ITask = {
                            taskId: paramTaskParameter.taskId,
                            ticketPassword: paramTaskParameter.ticketPassword
                        }

                        this.selectTicket(selectedTask);
                    }

                }
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
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
    }

    populateRolesDropdown() {
        // Call the service
        this._RoleService.getRoles().subscribe((roles: IRole[]) => {

            // Clear the list
            this.rolesDropdown = [];
            this.rolesDropdownEdit = [];

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
            this.rolesDropdownEdit.push(newUnassignedItem);

            // Loop through the returned Roles
            for (let role of roles) {

                // Create a new SelectedItem
                let newSelectedItem: SelectItem = {
                    label: role.roleName,
                    value: role
                }

                // Add Selected Item to the DropDown
                this.rolesDropdown.push(newSelectedItem);
                this.rolesDropdownEdit.push(newSelectedItem);
            }

            this.boolRolesDropdownLoaded = true;
        },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    // ** Ticket **

    private selectTicket(selectedTask: ITask) {

        this._userService.getCurrentUser().subscribe(
            (user: IUser) => {
                // Set the current user
                this.user = user;

                // Calculate isAdmin ***************
                this.isAdmin = ((this.user.userRoles.length > 0) || (this.user.isSuperUser));

                // Calculate showUpload
                // Call the service to get the upload permission
                this._settingsService.getApplicationSettings().subscribe(
                    objSettings => {

                        var uploadPermission = objSettings.uploadPermission;

                        if (
                            (uploadPermission == 'All') ||
                            ((uploadPermission == 'Administrator/Registered Users') && (this.user.isLoggedIn)) ||
                            ((uploadPermission == 'Administrator') && (this.user.isSuperUser))
                        ) {
                            this.showUpload = true;
                        } else {
                            this.showUpload = false;
                        }

                    },
                    error => {
                        this.errorMessage = <any>error;
                        this._dialogService.setMessage(this.errorMessage);
                    });

                // Disable Info form if not an admin
                if (!this.isAdmin) {
                    this.taskForm.disable();
                }

                // Call the service 
                this.showWaitGraphic = true;
                this._taskService.getTaskByID(selectedTask).subscribe(task => {
                    this.showWaitGraphic = false;
                    this.SelectedTask = task;

                    // Set TicketHeader
                    this.TicketHeader = "Edit Ticket #" + task.taskId.toString();

                    // Get AssignedRole
                    let selectedRole: SelectItem;
                    if (task.assignedRoleId !== undefined && task.assignedRoleId !== "") {
                        let intSelectedRoleId: number = Number(task.assignedRoleId);
                        // try to find it in the dropdown
                        selectedRole = this.rolesDropdownEdit.find(x => x.value.iD == intSelectedRoleId);
                    }

                    if (task !== undefined) {
                        this.taskForm = this.fb.group({
                            name: [task.requesterName],
                            email: [task.requesterEmail],
                            phone: [task.requesterPhone],
                            description: [task.description],
                            detail: [""],
                            dueDate: [task.dueDate],
                            priority: [task.priority],
                            status: [task.status],
                            assignedRole: [selectedRole.value],
                            estimatedStart: [task.estimatedStart],
                            estimatedCompletion: [task.estimatedCompletion],
                            estimatedHours: [task.estimatedHours]
                        });

                        // Set showManualUser
                        this.showManualUser = ((task.requesterUserId == "0") || (task.requesterUserId == "-1"));

                        // Call the service to get the Tree(s)
                        this.treeNodes = [];
                        if (this.isAdmin) {
                            this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                                this.treeNodes = categories;
                                this.boolTreeNodesLoaded = true;

                                this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                                    this.treeNodesEdit = categories;

                                    // Expand and clear the tree nodes
                                    this.treeNodesEdit.forEach(node => {
                                        this.expandAndSetRecursive(node);
                                    });
                                });
                            });                            
                        } else {
                            this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                                this.treeNodes = categories.filter(x => x.data.requestorVisible == true);
                                this.boolTreeNodesLoaded = true;

                                this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                                    this.treeNodesEdit = categories.filter(x => x.data.requestorVisible == true);

                                    // Expand and clear the tree nodes
                                    this.treeNodesEdit.forEach(node => {
                                        this.expandAndSetRecursive(node);
                                    });
                                });
                            });                            
                        }

                        // Set Details
                        this.taskDetailsComments = this.SelectedTask.colDTOTaskDetail.filter(x => x.detailType !== 'Work');
                        this.taskDetailsWork = this.SelectedTask.colDTOTaskDetail.filter(x => x.detailType == 'Work');

                        // Reset any QueryString value so it wont load
                        // the last ticket on navigation to Home
                        this._queryStringService.setQueryString({ userName: undefined, ticketNumber: undefined, code: undefined });
                    }
                },
                    error => {
                        this.errorMessage = <any>error;
                        this.showWaitGraphic = false;
                        this._dialogService.setMessage(this.errorMessage);
                    });
            });

    }

    private setSelectedChildCategoryEdit(category: number, childTreeNodes: TreeNode[]) {

        for (let childNode of childTreeNodes) {
            if (childNode.data.categoryId == category) {
                childNode.data.checkboxChecked = true;
                this.OnchangeNodeEdit(childNode);
            }

            if (childNode.children != null)
                this.setSelectedChildCategoryEdit(category, childNode.children);
        }
    }

    editTabChange(e) {
        this.EditIndex = e.index;
    }

    save() {
        // Get the form values
        let formData = this.taskForm.value;

        this.SelectedTask.colDTOTaskDetail = [];
        this.SelectedTask.requesterName = formData.name;
        this.SelectedTask.requesterEmail = formData.email;
        this.SelectedTask.requesterPhone = formData.phone;
        this.SelectedTask.description = formData.description;
        this.SelectedTask.dueDate = formData.dueDate;
        this.SelectedTask.priority = formData.priority;
        this.SelectedTask.status = formData.status;
        this.SelectedTask.estimatedStart = formData.estimatedStart;
        this.SelectedTask.estimatedCompletion = formData.estimatedCompletion;
        this.SelectedTask.estimatedHours = formData.estimatedHours;
        this.SelectedTask.sendEmails = true;

        if (formData.assignedRole !== undefined) {
            this.SelectedTask.assignedRoleId = formData.assignedRole.iD.toString();
        }

        // Set the selected tree nodes for the task (if any)
        this.SelectedTask.selectedTreeNodes = this.selectedTreeNodesEdit
            .map(x => x.data.categoryId).map(Number).filter(Boolean);

        // **** VALIDATION
        if (this.SelectedTask.requesterUserId == "-1") { // Only if user is not set
            if (this.SelectedTask.requesterName == undefined || this.SelectedTask.requesterName.length == 0) {
                this._dialogService.setMessage("Requester Name is required");
                return;
            }

            if (this.SelectedTask.requesterEmail == undefined || this.SelectedTask.requesterEmail.length == 0) {
                this._dialogService.setMessage("Email is required");
                return;
            }
        }

        if (this.SelectedTask.description == undefined || this.SelectedTask.description.length == 0) {
            this._dialogService.setMessage("Description is required");
            return;
        }

        this._taskService.updateTask(this.SelectedTask).subscribe(
            (UpdateResponse: IStatus) => {
                if (UpdateResponse.success) {

                    // Close the dialog
                    // Call the TaskVisibility service so that the task list will be updated
                    this._taskVisibilityService.setTaskDialogVisibility(this.SelectedTask);

                } else {
                    this._dialogService.setMessage(UpdateResponse.statusMessage);
                }
            });
    }

    // Set User

    ClearUser() {
        this.showManualUser = true;
        this.SelectedTask.requesterUserId = "-1";
    }

    SetUser() {
        this.searchStringUser = "";
        this.SelectedTask.requesterEmail
        this.SelectedTask.requesterPhone

        this.displaySelectUserDialog = true;
        this.showManualUser = false;
        this.taskForm.controls['name'].setValue('');
        this.taskForm.controls['email'].setValue('');
        this.onSearchParameterChange();
    }

    onUserSelect(event: any) {
        this._UserManagerService.getUser(event.data).subscribe(
            (userResult: IUser) => {
                this.manualUser = userResult;
                this.SelectedTask.requesterUserId = userResult.userId.toString();
                this.SelectedTask.requesterName = userResult.firstName + ' ' + userResult.lastName;
                this.displaySelectUserDialog = false;
                this.showManualUser = false;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    onSearchParameterChange() {
        this.pageNumberUser = 1;
        this.populateUserList();
    }

    loadUsersLazy(event: LazyLoadEvent) {
        this.rowsPerPage = event.rows;
        this.pageNumber = Math.floor(event.first / event.rows) + 1;

        if (this.user != undefined) {
            // Calculate isAdmin ***************
            this.isAdmin = ((this.user.userRoles.length > 0) || (this.user.isSuperUser));

            // Only populate if an Admin
            if (this.isAdmin) {
                this.populateUserList();
            }
        }
    }

    populateUserList() {
        if (this.pageNumberUser === undefined) {
            this.pageNumberUser = 1;
        }

        let parameters: ISearchParameters = {
            rowsPerPage: this.rowsPerPageManual,
            pageNumber: this.pageNumberUser,
            searchString: this.searchStringUser
        }

        this._UserManagerService.searchUsers(parameters).subscribe(
            (usersResults: IUserSearchResult) => {
                if (usersResults.errorMessage.length < 1) {
                    this.SearchResultsUsers = usersResults.userList;
                    this.totalUsers = usersResults.totalRows;
                }
                else {
                    this.searchError = usersResults.errorMessage;
                    this._dialogService.setMessage(this.searchError);
                }
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    // EDIT Detail

    editComment(item: ITaskDetail) {
        this.SelectedTaskDetailOriginal = item; // Save to be updated later
        this.newSelectedTaskDetail();
        this.SelectedTaskDetail.contentType = item.contentType;
        this.SelectedTaskDetail.detailId = item.detailId;
        this.SelectedTaskDetail.description = item.description;
        this.SelectedTaskDetail.emailDescription = item.emailDescription;
        this.SelectedTaskDetail.detailType = item.detailType;
        this.SelectedTaskDetail.insertDate = item.insertDate;
        this.SelectedTaskDetail.userId = item.userId;
        this.SelectedTaskDetail.userName = item.userName;
        this.boolVisibleToRequester = (item.detailType == "Comment - Visible");
        this.boolHideForNonAdmin = ((!this.isAdmin) && (this.user.userId.toString() != item.userId.toString()));
        this.hasFile = (item.colDTOAttachment.length > 0);
        if (this.hasFile) {
            this.SelectedTaskDetail.colDTOAttachment = item.colDTOAttachment;
        }
        this.displayEditCommentDialog = true;
    }

    editWork(item: ITaskDetail) {
        this.SelectedTaskDetailOriginal = item; // Save to be updated later
        this.newSelectedTaskDetail();
        this.SelectedTaskDetail.detailId = item.detailId;
        this.SelectedTaskDetail.description = item.description;
        this.SelectedTaskDetail.detailType = item.detailType;
        this.SelectedTaskDetail.insertDate = item.insertDate;
        this.SelectedTaskDetail.userId = item.userId;
        this.SelectedTaskDetail.userName = item.userName;
        if (item.startTime !== '') {
            this.SelectedTaskDetail.startTime = item.startTime;
        }
        if (item.stopTime !== '') {
            this.SelectedTaskDetail.stopTime = item.stopTime;
        }
        this.displayEditWorkDialog = true;
    }

    addNewCommentsDetail() {
        this.newSelectedTaskDetailOriginal();
        this.newSelectedTaskDetail();
        this.displayEditCommentDialog = true;
    }

    addNewWorkDetail() {
        this.newSelectedTaskDetailOriginal();
        this.newSelectedTaskDetail();
        this.SelectedTaskDetail.detailType = "Work";
        this.displayEditWorkDialog = true;
    }

    viewfile() {

        let FileParameter: IDTOFileParameter = {
            taskId: this.SelectedTask.taskId,
            portalId: -1,
            ticketPassword: this.SelectedTask.ticketPassword,
            detailId: this.SelectedTaskDetail.detailId,
            attachmentID: this.SelectedTaskDetail.colDTOAttachment[0].attachmentID
        }

        // Call the service
        this._FilesService.returnFile(FileParameter)
            .subscribe(response => {
                this.downloadFile(response, this.SelectedTaskDetail.colDTOAttachment[0].originalFileName);
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    viewEmailFile(attachmentFileName: string) {

        let FileParameter: IDTOFileParameter = {
            taskId: this.SelectedTask.taskId,
            portalId: -1,
            ticketPassword: this.SelectedTask.ticketPassword,
            detailId: this.SelectedTaskDetail.detailId,
            attachmentID: this.SelectedTaskDetail.colDTOAttachment[0].attachmentID,
            emailFileName: attachmentFileName
        }

        // Call the service
        this._FilesService.returnFile(FileParameter)
            .subscribe(response => {
                this.downloadFile(response, attachmentFileName);
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    deletefile() {

        // Change hasFile
        this.hasFile = false;

        // Close dialog
        this.displayConfirmDeleteDialog = false;
    }

    // *********************************
    // SAVE CODE

    saveComment() {

        // Set detailType
        if (this.boolVisibleToRequester) {
            this.SelectedTaskDetail.detailType = "Comment - Visible";
        } else {
            this.SelectedTaskDetail.detailType = "Comment";
        }

        this.boolSendEmail = false;
        this.performSave();
    }

    saveCommentEmail() {

        // Set detailType
        if (this.boolVisibleToRequester) {
            this.SelectedTaskDetail.detailType = "Comment - Visible";
        } else {
            this.SelectedTaskDetail.detailType = "Comment";
        }

        this.boolSendEmail = true;
        this.performSave();
    }

    saveWork() {
        this.performSave();
    }

    performSave() {
        // We have put all form values in an object that will be sent when 
        // onBeforeUpload(event) is called 
        // This is triggered when we call this.fileInput.upload() (below)

        // Start file upload
        this.fileInput.upload();
    }

    delete() {
        // Call the service
        this._taskService.deleteTask(this.SelectedTask.taskId)
            .subscribe(response => {
                // Mark Task as Deleted
                this.SelectedTask.status = "Deleted";

                // Call the TaskVisibility service so that the task list will be deleted
                this._taskVisibilityService.setTaskDialogVisibility(this.SelectedTask);

                // Close dialog
                this.displayConfirmDeleteTaskDialog = false;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);

                // Close dialog
                this.displayConfirmDeleteTaskDialog = false;
            });
    }

    deleteComment() {
        // Call the service
        this._taskService.deleteTaskDetail(this.SelectedTaskDetail.detailId)
            .subscribe(response => {

                // Find TaskDetail 
                let objTaskDetail: ITaskDetail = this.taskDetailsComments.find(x => x.detailId == this.SelectedTaskDetail.detailId);
                if (objTaskDetail !== null) {
                    // Create array that has the Task Detail Comment removed
                    var colTaskDetailsComments = this.taskDetailsComments.filter(obj => obj !== objTaskDetail);
                    // Bind to the Task Detail Comments to remove the Comment
                    this.taskDetailsComments = colTaskDetailsComments;
                }

                // Close open form
                this.displayEditCommentDialog = false;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    deleteWork() {
        // Call the service
        this._taskService.deleteTaskDetail(this.SelectedTaskDetail.detailId)
            .subscribe(response => {

                // Find TaskDetail 
                let objTaskDetail: ITaskDetail = this.taskDetailsWork.find(x => x.detailId == this.SelectedTaskDetail.detailId);
                if (objTaskDetail !== null) {
                    // Create array that has the Task Detail Work Comment removed
                    var colTaskDetailsWorkComments = this.taskDetailsWork.filter(obj => obj !== objTaskDetail);
                    // Bind to the Task Detail Work Comments to remove the Comment
                    this.taskDetailsWork = colTaskDetailsWorkComments;
                }

                // Close open form
                this.displayEditWorkDialog = false;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });

    }
    // taskDetailsWork

    // Upload Code *********************
    // Posts to: api/UploadTask/InsertUpdateTaskDetail

    public onBeforeUpload(event) {
        // Pass the Task information with the file (if any)

        let newTask: ITask = {
            taskId: this.SelectedTask.taskId,
            ticketPassword: this.SelectedTask.ticketPassword,
            sendEmails: true,
            colDTOTaskDetail: []
        }

        let newTaskDetail: ITaskDetail = {
            description: this.SelectedTaskDetail.description,
            detailId: this.SelectedTaskDetail.detailId,
            detailType: this.SelectedTaskDetail.detailType,
            insertDate: this.SelectedTaskDetail.insertDate,
            startTime: this.SelectedTaskDetail.startTime,
            stopTime: this.SelectedTaskDetail.stopTime,
            userId: this.SelectedTaskDetail.userId,
            userName: this.SelectedTaskDetail.userName,
            sendEmails: this.boolSendEmail,
            colDTOAttachment: []
        }

        if (this.hasFile) {
            let newTaskDetailAttachment: IAttachment = {
                attachmentID: this.SelectedTaskDetail.colDTOAttachment[0].attachmentID,
                fileName: this.SelectedTaskDetail.colDTOAttachment[0].fileName,
                originalFileName: this.SelectedTaskDetail.colDTOAttachment[0].userId
            }

            newTaskDetail.colDTOAttachment.push(newTaskDetailAttachment);
        }

        // Add TaskDetail to Task object
        newTask.colDTOTaskDetail.push(newTaskDetail);

        event.formData.append("task", JSON.stringify(newTask));
    }

    public onUploadHandler(event) {
        this.showWaitGraphic = false;
        // The .Net controller will return the results
        // as xhr.responseText
        this.UploadResponse = JSON.parse(event.xhr.responseText);

        // There is an error
        if (!this.UploadResponse.isSuccess) {
            this._dialogService.setMessage(this.UploadResponse.message);
        } else {

            if (this.UploadResponse.taskDetail !== null) {
                // Search for the item
                let objTaskDetail = this.SelectedTask.colDTOTaskDetail.find(x => x.detailId == this.SelectedTaskDetailOriginal.detailId);

                // Did we find the item?
                if (objTaskDetail == null) {
                    // The item is not in the collection (because it is new) add it
                    this.SelectedTask.colDTOTaskDetail.unshift(this.UploadResponse.taskDetail);

                    // Update Details collections
                    this.taskDetailsComments = this.SelectedTask.colDTOTaskDetail.filter(x => x.detailType !== 'Work');
                    this.taskDetailsWork = this.SelectedTask.colDTOTaskDetail.filter(x => x.detailType == 'Work');
                } else {
                    // The item exists
                    // Update the fields in grid
                    this.SelectedTaskDetailOriginal.description = this.UploadResponse.taskDetail.description;
                    this.SelectedTaskDetailOriginal.detailId = this.UploadResponse.taskDetail.detailId;
                    this.SelectedTaskDetailOriginal.detailType = this.UploadResponse.taskDetail.detailType;
                    this.SelectedTaskDetailOriginal.insertDate = this.UploadResponse.taskDetail.insertDate;
                    this.SelectedTaskDetailOriginal.startTime = this.UploadResponse.taskDetail.startTime;
                    this.SelectedTaskDetailOriginal.stopTime = this.UploadResponse.taskDetail.stopTime;
                    this.SelectedTaskDetailOriginal.userId = this.UploadResponse.taskDetail.userId;
                    this.SelectedTaskDetailOriginal.userName = this.UploadResponse.taskDetail.userName;

                    // Was a file attached
                    if (this.UploadResponse.taskDetail.colDTOAttachment.length > 0) {
                        // Clear the last one and add it
                        this.SelectedTaskDetailOriginal.colDTOAttachment = [];
                        this.SelectedTaskDetailOriginal.colDTOAttachment.push(this.UploadResponse.taskDetail.colDTOAttachment[0]);
                    } else {
                        // Clear any file that may have been there
                        this.SelectedTaskDetailOriginal.colDTOAttachment = [];
                    }
                }

                // If the file type is EML we need to re-load the entire Ticket
                if (this.UploadResponse.taskDetail.contentType == "EML") {
                    this._taskVisibilityService.setTaskVisibility(this.SelectedTask);
                }
            }

            // Close any open form
            this.displayEditCommentDialog = false;
            this.displayEditWorkDialog = false;
        }
    }

    public onError(event) {
        this.showWaitGraphic = false;
        // Display Error
        this._dialogService.setMessage(event);
    }

    // Logs

    loadLogsLazy(event: LazyLoadEvent) {
        if (this.SelectedTask !== undefined) {
            this.searchErrorLog = "";
            this.rowsPerPageLog = event.rows;
            this.pageNumberLog = Math.floor(event.first / event.rows) + 1;
            this.populateLog();
        }
    }

    populateLog() {

        let SearchParameters: ISearchParameters = {
            searchString: this.SelectedTask.taskId.toString(),
            rowsPerPage: this.rowsPerPageLog,
            pageNumber: this.pageNumberLog
        }

        this._LogService.SearchLogs(SearchParameters).subscribe(
            (logResults: ILogSearchResult) => {
                if (logResults.errorMessage.length < 1) {
                    this.SearchResultsLog = logResults.logList;
                    this.totalLogs = logResults.totalRows;
                }
                else {
                    this.searchErrorLog = logResults.errorMessage;
                }
            },
            error => this.searchErrorLog = <any>error
        )

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

    convertDateToString(paramDate: Date) {
        //return string
        var returnDate = "";

        // If date is not in the long format -- return it
        if (paramDate.toString().length < 11) {
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

    newSelectedTaskDetail() {
        this.SelectedTaskDetail.detailId = -1;
        this.SelectedTaskDetail.contentType = "TXT";
        this.SelectedTaskDetail.description = "";
        this.SelectedTaskDetail.startTime = "";
        this.SelectedTaskDetail.stopTime = "";
        this.SelectedTaskDetail.colDTOAttachment = [];
        this.SelectedTaskDetail.insertDate = this.convertDateToString(new Date());
        this.SelectedTaskDetail.userId = this.user.userId.toString();
        this.SelectedTaskDetail.userName = this.user.userName;
        this.hasFile = false;
        // Everyone can save a new comment
        this.boolHideForNonAdmin = false;

        // Create a blank file attachment so that client side
        // binding does not throw errors
        let fileAttachment: IAttachment = {
            attachmentID: -1,
            attachmentPath: '',
            fileName: '',
            originalFileName: '',
            userId: ''
        }

        this.SelectedTaskDetail.colDTOAttachment.push(fileAttachment);
    }

    newSelectedTaskDetailOriginal() {
        let objITaskDetail: ITaskDetail = {
            detailId: -1 // On the round-trip we will know the orginal record was a new record
        }
        this.SelectedTaskDetailOriginal = objITaskDetail;
    }

    downloadFile(blob: any, filename: string): string {

        if ('msSaveOrOpenBlob' in navigator) {
            navigator.msSaveOrOpenBlob(blob, filename);
        } else {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.setAttribute('style', 'display:none;');
            document.body.appendChild(a);

            a.href = url;
            a.download = filename;
            a.click();
            return url;
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

    private expandAndSetRecursive(node: TreeNode) {
        // Expand the node and clear it
        node.expanded = true;
        node.data.checkboxChecked = false;

        // Set the node
        if (this.SelectedTask.selectedTreeNodes !== undefined) {
            var categoryId = Number(node.data.categoryId);
            if (this.SelectedTask.selectedTreeNodes.indexOf(categoryId) > -1) {
                node.data.checkboxChecked = true;
            }
        }

        // Handle any children
        if (node.children) {
            node.children.forEach(childNode => {
                this.expandAndSetRecursive(childNode);
            });
        }
    }

    shortenName(fileName: string) {
        var intPeriod = fileName.lastIndexOf('.');
        var extension = fileName.substring(intPeriod);

        var nameOfFile = fileName.substring(0, intPeriod);
        if (nameOfFile.length > 50) {
            nameOfFile = nameOfFile.substring(0, 50) + '... ' + extension;
        } else {
            nameOfFile = nameOfFile + extension;
        }
        return nameOfFile;
    }

    ngOnDestroy(): void {
        // Important - Unsubscribe from any subscriptions
        if (this.QueryStringSubscription !== undefined) {
            this.QueryStringSubscription.unsubscribe();
        }

        if (this.TaskVisibilityService !== undefined) {
            this.TaskVisibilityService.unsubscribe();
        }
    }
}