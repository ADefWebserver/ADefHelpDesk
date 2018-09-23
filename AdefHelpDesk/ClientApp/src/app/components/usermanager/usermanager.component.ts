import { Component, OnInit, Input } from '@angular/core';
import { LazyLoadEvent } from 'primeng/components/common/api';
import {
    InputTextModule,
    DropdownModule,
    ButtonModule,
    SelectItem
} from 'primeng/primeng';

import { DialogService } from '../services/internal/dialog.service';

import { ISearchParameters } from '../classes/searchParameters';
import { IUserSearchResult } from '../classes/userSearchResult';

import { UserManagerService } from '../services/web/userManager.service';
import { IUser } from '../classes/user';

import { RoleService } from '../services/web/role.service';
import { IRole } from '../classes/role';

@Component({
    selector: 'usermanager',
    templateUrl: './usermanager.component.html'
})
export class UsermanagerComponent implements OnInit {
    public errorMessage: string;
    searchString: string = "";
    totalUsers: number = 0;
    rowsPerPage: number = 0;
    pageNumber: number;
    first: number;
    SearchResults: IUser[];
    searchError: string;
    showWaitGraphic: boolean = false;

    public displayDialog: boolean;
    public displayRoleDialog: boolean;
    public user: IUser = new User(-1, "", "", "", "", false, false, "", "", "", []);
    public newUser: boolean;
    public EditUserIndex: number = 0;

    public selectedRole: IRole;
    public rolesDropdown: SelectItem[] = [];

    constructor(
        private _UserManagerService: UserManagerService,
        private _dialogService: DialogService,
        private _RoleService: RoleService) { }

    ngOnInit() {
        this.rowsPerPage = 10;
        this.pageNumber = 1;
        this.first = 0;

        this.populateUserList();
    }

    loadUsersLazy(event: LazyLoadEvent) {
        this.rowsPerPage = event.rows;
        this.pageNumber = Math.floor(event.first / event.rows) + 1;
        this.populateUserList();
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

    populateRolesDropdown() {
        // Call the service
        this._RoleService.getRoles().subscribe((roles: IRole[]) => {

            // Clear the list
            this.rolesDropdown = [];

            // Loop through the returned Roles
            for (let role of roles) {

                // Only add the Role if the user does not 
                // already hase it
                if (this.user.userRoles.find(x => x.iD == role.iD) == undefined) {

                    // Create a new SelectedItem
                    let newSelectedItem: SelectItem = {
                        label: role.roleName,
                        value: role
                    }

                    // Add Selected Item to the DropDown
                    this.rolesDropdown.push(newSelectedItem);
                }

            }

            // Set the selected option to the first option
            if (this.rolesDropdown[0].value != undefined) {
                this.selectedRole = this.rolesDropdown[0].value;
            }
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

    editUserTabChange(e) {
        this.EditUserIndex = e.index;
    }

    onUserSelect(event: any) {
        this.newUser = false;

        this._UserManagerService.getUser(event.data).subscribe(
            (userResult: IUser) => {
                this.user = userResult;
                this.displayDialog = true;
                this.populateRolesDropdown();
                this.EditUserIndex = 0;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    showDialogToAdd() {
        this.newUser = true;
        this.user = new User(-1, "", "", "", "", false, false, "", "", "", []);
        this.displayDialog = true;
        this.populateRolesDropdown();
        this.EditUserIndex = 0;
    }

    save() {
        if (this.newUser) {

            // Call the service 
            this.showWaitGraphic = true;
            this._UserManagerService.createUser(this.user).subscribe(status => {
                this.showWaitGraphic = false;
                if (status.success) {
                    this.user = null;
                    this.displayDialog = false;
                    this.populateUserList()
                } else {
                    this._dialogService.setMessage(status.statusMessage);
                }
            },
                error => {
                    this.errorMessage = <any>error;
                    this.showWaitGraphic = false;
                    this._dialogService.setMessage(this.errorMessage);
                });
        }
        else {

            // Call the service 
            this._UserManagerService.updateUser(this.user).subscribe(status => {
                if (status.success) {
                    this.user = null;
                    this.displayDialog = false;
                    this.populateUserList()
                } else {
                    this._dialogService.setMessage(status.statusMessage);
                }
            },
                error => {
                    this.errorMessage = <any>error;
                    this._dialogService.setMessage(this.errorMessage);
                });
        }
    }

    delete() {
        // Call the service 
        this._UserManagerService.deleteUser(this.user).subscribe(() => {
            this.displayDialog = false;
            this.populateUserList()
        },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    // Roles

    addRole() {
        if (this.selectedRole !== undefined) {
            // Add the role to the user (and the DataTable)
            this.user.userRoles = [...this.user.userRoles, this.selectedRole];

            // Remove the role from the dropdown
            // Create array that has the Role removed
            var colRoles = this.rolesDropdown.filter(obj => obj.value !== this.selectedRole);
            // Bind to the userRoles to remove the role from the user (and the DataTable)
            this.rolesDropdown = colRoles;

            // Clear selectedRole
            this.selectedRole = undefined;

            // Set the selected option to the first option
            if (this.rolesDropdown[0].value != undefined) {
                this.selectedRole = this.rolesDropdown[0].value;
            }
        }
    }

    deleteRole(role: IRole) {
        // Create array that has the Role removed
        var colRoles = this.user.userRoles.filter(obj => obj !== role);
        // Bind to the userRoles to remove the role from the user (and the DataTable)
        this.user.userRoles = colRoles;

        // Add the role back in the dropdown
        let newSelectItem: SelectItem = {
            label: role.roleName,
            value: role
        }

        this.rolesDropdown = [...this.rolesDropdown, newSelectItem];
    }
}

// Classes

class User implements IUser {
    constructor(
        public userId,
        public userName,
        public firstName,
        public lastName,
        public email,
        public isLoggedIn,
        public isSuperUser,
        public password,
        public riapassword,
        public verificationCode,
        public userRoles
    ) { }
}

class UserRole implements IRole {
    constructor(
        public iD,
        public portalID,
        public roleName
    ) { }
}