import { Component, ViewChild, OnInit, AfterViewInit, OnDestroy, Input } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, Validators } from '@angular/forms';

import { Subscription } from 'rxjs/Subscription';

import { Http } from '@angular/http';
import {
    DialogModule,
    PasswordModule,
    InputTextModule,
    RadioButtonModule,
    DropdownModule,
    ButtonModule,
    GrowlModule,
    Message,
    MessagesModule,
    FieldsetModule,
    TreeModule,
    TreeNode,
    FileUpload,
    SelectItem
} from 'primeng/primeng';
import { LazyLoadEvent } from 'primeng/components/common/api';

import { CategoryService } from '../services/web/category.service';
import { ICategory } from '../classes/category';
import { INodeDetail } from '../classes/category';
import { ICategoryNode } from '../classes/categoryNode';

import { IQueryStringParameter } from '../classes/querystringParameters';
import { ITask } from '../classes/task';
import { ITaskDetail } from '../classes/task';
import { IStatus } from '../classes/status';

import { UserManagerService } from '../services/web/userManager.service';
import { ISearchParameters } from '../classes/searchParameters';
import { IUserSearchResult } from '../classes/userSearchResult';
import { QueryStringService } from '../services/internal/queryString.service';
import { TaskVisibilityService } from '../services/internal/taskVisibility.service';
import { DialogService } from '../services/internal/dialog.service';
import { LoginService } from '../services/internal/login.service';
import { RegisterService } from '../services/internal/register.service';
import { UserService } from '../services/web/user.service';
import { SettingsService } from '../services/web/settings.service';
import { IUser } from '../classes/user';
import { RoleService } from '../services/web/role.service';
import { IRole } from '../classes/role';
import { ILoginParameter } from '../classes/loginParameter';

@Component({
    selector: 'home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
    @ViewChild('fileInput') fileInput: FileUpload;
    QueryStringSubscription: Subscription;
    LoginSubscription: Subscription;
    RegisterSubscription: Subscription;
    public errorMessage: string;

    public newtask: ITask;
    public user: IUser;
    public UploadResponse: IStatus;
    public assignedRole: IRole;

    public showUpload: boolean = false;
    public showManualUser: boolean = true;
    public bShowCategory: boolean = false;
    public isAdmin: boolean = false;
    public showWaitGraphic: boolean = false;
    public displayDialog: boolean = false;

    searchString: string = "";
    totalUsers: number = 0;
    rowsPerPage: number = 0;
    pageNumber: number;
    first: number;
    SearchResults: IUser[];
    searchError: string;

    public NewTaskIndex: number = 0;
    public assignedRoleId: string = "-1";
    public requesterUserId: string = "-1";
    public status: string = "New";
    public estimatedStart: string = "";
    public estimatedCompletion: string = "";
    public estimatedHours: string = "";
    public startTime: string = "";
    public stopTime: string = "";

    public treeNodes: TreeNode[];
    public selectedTreeNodes: TreeNode[] = [];
    public priorityDropdown: SelectItem[] = [];
    public rolesDropdown: SelectItem[] = [];
    public statusDropdown: SelectItem[] = [];

    public newtaskForm = this.fb.group({
        name: [""],
        email: [""],
        phone: [""],
        description: [""],
        detail: [""],
        dueDate: [""],
        priority: ["Normal"]
    });

    constructor(
        public fb: FormBuilder,
        private _router: Router,
        private _queryStringService: QueryStringService,
        private _settingsService: SettingsService,
        private _taskVisibilityService: TaskVisibilityService,
        private _userService: UserService,
        private _registerService: RegisterService,
        private _CategoryService: CategoryService,
        private _dialogService: DialogService,
        private _loginService: LoginService,
        private _RoleService: RoleService,
        private _UserManagerService: UserManagerService) { }

    ngOnInit() {
        // Add Items to the Priority DropDown
        this.priorityDropdown.push({ label: "Normal", value: 'Normal' });
        this.priorityDropdown.push({ label: "High", value: 'High' });
        this.priorityDropdown.push({ label: "Low", value: 'Low' });

        this.rowsPerPage = 10;
        this.pageNumber = 1;
        this.first = 0;
    }

    ngAfterViewInit() {

        // Subscribe to the QueryStringSubscription Service
        this.QueryStringSubscription = this._queryStringService.getQueryString().subscribe(
            (paramQueryStringParameter: IQueryStringParameter) => {

                // ** Login
                if (paramQueryStringParameter.userName != undefined) {
                    // Cause the Login dialog to show
                    this._loginService.setLogin({ showLogin: true, taskId: -1 });

                    // Clear the parameters
                    let paramIQueryStringParameter: IQueryStringParameter = {
                        userName: undefined,
                        ticketNumber: undefined,
                        code: undefined
                    }

                    this._queryStringService.setQueryString(paramIQueryStringParameter);
                }

                // ** Task (with a code)
                if (paramQueryStringParameter.code != undefined) {
                    // If there is a code always open ticket as requester

                    let selectedTask: ITask = {
                        taskId: Number(paramQueryStringParameter.ticketNumber),
                        ticketPassword: paramQueryStringParameter.code
                    }

                    this._taskVisibilityService.setTaskVisibility(selectedTask)
                }

                // Task (without a code -- just a number)
                if ((paramQueryStringParameter.ticketNumber != undefined) && (paramQueryStringParameter.code == undefined)) {
                    // Does user need to log in?
                    this._userService.getCurrentUser().subscribe(
                        (user: IUser) => {
                            if (user.userId > 0) {
                                // The user is logged in and only a ticket number is being passed
                                // Open the ticket

                                let selectedTask: ITask = {
                                    taskId: Number(paramQueryStringParameter.ticketNumber)
                                }

                                this._taskVisibilityService.setTaskVisibility(selectedTask)
                                // Navigate to Ticket page
                                this._router.navigateByUrl('/tickets');
                            } else {
                                // Cause the Login dialog to show
                                this._loginService.setLogin({ showLogin: true, taskId: Number(paramQueryStringParameter.ticketNumber) });
                            }
                        });
                }
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });

        // Subscribe to the LoginSubscription Service
        this.LoginSubscription = this._loginService.getLogin().subscribe(
            (loginParameter: ILoginParameter) => {

                if (loginParameter.showLogin == false) {
                    // If the Login Dialog is closed
                    // Trigger UI code
                    this.updateUI();
                }
            });

        // Subscribe to the RegisterSubscription Service
        this.RegisterSubscription = this._registerService.getVisbility().subscribe(
            (visibility: boolean) => {

                if (visibility == false) {
                    // If the Register Dialog is closed
                    // Trigger UI code
                    this.updateUI();
                }
            });

    }

    populateStatusDropdown() {

        // Clear the list
        this.statusDropdown = [];

        // Add Items to the DropDown
        this.statusDropdown.push({ label: "New", value: 'New' });
        this.statusDropdown.push({ label: "Active", value: 'Active' });
        this.statusDropdown.push({ label: "Cancelled", value: 'Cancelled' });
        this.statusDropdown.push({ label: "On Hold", value: 'On Hold' });
        this.statusDropdown.push({ label: "Resolved", value: 'Resolved' });
    }

    populateRolesDropdown() {
        // Call the service
        this._RoleService.getRoles().subscribe((roles: IRole[]) => {

            // Clear the list
            this.rolesDropdown = [];

            // Set Default
            let defaultRole: IRole = {
                iD: -1,
                portalID: 0,
                roleName: '[Unassigned]'
            }

            let newSelectedItem: SelectItem = {
                label: "[Unassigned]",
                value: defaultRole
            }

            this.assignedRole = defaultRole;

            // Add Selected Item to the DropDown
            this.rolesDropdown.push(newSelectedItem);

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
        },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    updateUI() {
        // Determine if upload box should show
        this._userService.getCurrentUser().subscribe(
            (user: IUser) => {

                // Set the current user
                this.user = user;

                // Calculate isAdmin ***************
                this.isAdmin = ((this.user.userRoles.length > 0) || (this.user.isSuperUser));

                this.treeNodes = [];

                // Only populate if an Admin (and the Ticket Details tab is showing)
                if (this.isAdmin) {
                    this.populateRolesDropdown();
                    this.populateStatusDropdown();
                    this.populateUserList();                    

                    // Call the service to get the FULL Tree
                    this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                        this.treeNodes = categories;
                    });
                } else {
                    // Call the service to get the Requestor Visible Tree
                    this._CategoryService.getTreeCategorys(true).subscribe((categories: ICategory[]) => {
                        this.treeNodes = categories.filter(x => x.data.requestorVisible == true);
                    });
                }

                // Calculate showManualUser
                if (this.user.isLoggedIn) {
                    this.showManualUser = false;
                    this.requesterUserId = this.user.userId.toString();
                } else {
                    this.showManualUser = true;
                }

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
                    });
            });
    }

    // Set User

    ClearUser() {
        this.showManualUser = true;
        this.requesterUserId = "-1";
    }

    SetUser() {
        this.displayDialog = true;
        this.showManualUser = false;
        this.requesterUserId = this.user.userId.toString();
        this.newtaskForm.controls['name'].setValue('');
        this.newtaskForm.controls['email'].setValue('');
    }

    onUserSelect(event: any) {
        this._UserManagerService.getUser(event.data).subscribe(
            (userResult: IUser) => {
                this.user = userResult;
                this.requesterUserId = this.user.userId.toString();
                this.displayDialog = false;
                this.showManualUser = false;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            }); 
    }

    onSearchParameterChange() {
        this.pageNumber = 1;
        this.populateUserList();
    }

    loadUsersLazy(event: LazyLoadEvent) {
        this.rowsPerPage = event.rows;
        this.pageNumber = Math.floor(event.first / event.rows) + 1;

        if (this.user != undefined) {
            // Calculate isAdmin ***************
            this.isAdmin = ((this.user.userRoles.length > 0) || (this.user.isSuperUser));

            // Only populate if an Admin (and the Ticket Details tab is showing)
            if (this.isAdmin) {
                this.populateUserList();
            }
        }
    }

    populateUserList() {
        let parameters: ISearchParameters = {
            rowsPerPage: this.rowsPerPage,
            pageNumber: this.pageNumber,
            searchString: this.searchString
        }

        this._UserManagerService.searchUsers(parameters).subscribe(
            (usersResults: IUserSearchResult) => {
                if (usersResults.errorMessage.length < 1) {
                    this.SearchResults = usersResults.userList;
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

    // SAVE

    Save() {

        // Get the form values
        let formData = this.newtaskForm.value;
        let nameValue = formData.name;
        let emailValue = formData.email;
        let phoneValue = formData.phone;
        let descriptionValue = formData.description;
        let detailValue = formData.detail;
        let dueDateValue = formData.dueDate;
        let priorityValue = formData.priority;

        let Newtask: ITask = {
            requesterName: nameValue,
            requesterEmail: emailValue,
            requesterPhone: phoneValue,
            description: descriptionValue,
            dueDate: dueDateValue,
            priority: priorityValue,
            status: this.status,
            estimatedStart: this.estimatedStart,
            estimatedCompletion: this.estimatedCompletion,
            estimatedHours: this.estimatedHours,
            sendEmails: true,
            colDTOTaskDetail: []
        };

        let NewtaskDetail: ITaskDetail = {
            description: detailValue,
            detailType: 'Comment - Visible',
            startTime: this.startTime,
            stopTime: this.stopTime
        };

        // **** VALIDATION
        if (this.requesterUserId == "-1") { // Only if user is not set
            if (Newtask.requesterName == undefined || Newtask.requesterName.length == 0) {
                this._dialogService.setMessage("Requester Name is required");
                return;
            }

            if (Newtask.requesterEmail == undefined || Newtask.requesterEmail.length == 0) {
                this._dialogService.setMessage("Email is required");
                return;
            }
        }

        if (Newtask.description == undefined || Newtask.description.length == 0) {
            this._dialogService.setMessage("Description is required");
            return;
        }

        // **** ADMIN SETTINGS
        // If isAdmin the Ticket Details is showing - set the values
        if (this.isAdmin) {
            // Set assignedRoleId 
            // This is so an assignedRole value will still be available 
            // to be set for the Task (later in this method) if the
            // assignedRole dropdown is hidden because the current user
            // is not an Admin
            if (this.assignedRole !== undefined) {
                this.assignedRoleId = this.assignedRole.iD.toString();
            }
        }

        // **** SET TASK
        this.newtask = Newtask;

        // Set assignedRoleId
        if (this.assignedRoleId !== "-1") {
            this.newtask.assignedRoleId = this.assignedRoleId;
        } else {
            this.newtask.assignedRoleId = "-1";
        }

        // Set requesterUserId
        if (this.requesterUserId !== "-1") {
            this.newtask.requesterUserId = this.requesterUserId;
        } else {
            this.newtask.requesterUserId = "-1";
        }

        // Add the task Detail record
        this.newtask.colDTOTaskDetail.push(NewtaskDetail);

        // Set the selected tree nodes for the task (if any)
        this.newtask.selectedTreeNodes = this.selectedTreeNodes
            .map(x => x.data.categoryId).map(Number).filter(Boolean);

        // Put all form values and tree values in an object that will be sent when 
        // onBeforeUpload(event) is called (below)
        // This is triggered when we call this.fileInput.upload() (below)

        // Start file upload
        this.fileInput.upload();
    }

    // Adds/Removes tree nodes to this.selectedTreeNodes
    // On change event for the Checkbox Checked/Unchecked Event
    public OnchangeNode(node: TreeNode) {

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

    // Upload Code *********************
    // (this posts to: "api/UploadTask")

    public onBeforeUpload(event) {
        // Pass the Task information and selected tree nodes (if any)
        // with the file (if any)
        event.formData.append("task", JSON.stringify(this.newtask));
    }

    public onUploadHandler(event) {
        this.showWaitGraphic = false;
        // The .Net controller will return the results
        // as xhr.responseText
        this.UploadResponse = JSON.parse(event.xhr.responseText);

        // There is an error
        if (!this.UploadResponse.success) {
            this._dialogService.setMessage(this.UploadResponse.statusMessage);
        } else {
            // Clear Form
            this.setNewTaskForm();

            // Clear Tree Nodes
            this.selectedTreeNodes = [];
            this.clearAllNodes();

            // Indicate success
            this._dialogService.setMessage("Ticket saved successfully");
        }

    }

    public onError(event) {
        this.showWaitGraphic = false;
        // Display Error
        this._dialogService.setMessage(event);
    }

    newTaskTabChange(e) {
        this.NewTaskIndex = e.index;
    }

    // Utility

    setNewTaskForm() {
        this.newtaskForm = this.fb.group({
            name: [""],
            email: [""],
            phone: [""],
            description: [""],
            detail: [""],
            dueDate: [""],
            priority: ["Normal"]
        });
    }

    clearAllNodes() {
        this.treeNodes.forEach(node => {
            this.clearNodeRecursive(node, true);
        });
    }

    private clearNodeRecursive(node: TreeNode, isExpand: boolean) {
        node.data.checkboxChecked = false;
        if (node.children) {
            node.children.forEach(childNode => {
                this.clearNodeRecursive(childNode, isExpand);
            });
        }
    }

    ngOnDestroy(): void {
        // Important - Unsubscribe from any subscriptions
        this.QueryStringSubscription.unsubscribe();
        this.LoginSubscription.unsubscribe();
        this.RegisterSubscription.unsubscribe();
    }
}
