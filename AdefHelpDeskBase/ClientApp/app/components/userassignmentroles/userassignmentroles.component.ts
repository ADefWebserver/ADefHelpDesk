import { Component, OnInit, Input } from '@angular/core';

import { RoleService } from '../services/web/role.service';
import { IRole } from '../classes/role';

@Component({
    selector: 'userassignmentroles',
    templateUrl: './userassignmentroles.component.html'
})
export class UserassignmentrolesComponent implements OnInit {
    public errorMessage: string;
    public roles: IRole[] = [];
    public displayDialog: boolean;
    public role: IRole = new UserRole(-1, -1, "");
    public selectedRole: IRole;
    public newRole: boolean;

    constructor(private _RoleService: RoleService) { }

    ngOnInit() {
        this.populateRoles();
    }

    populateRoles() {
        this.roles = [];

        // Call the service 
        this._RoleService.getRoles().subscribe(roles => {
            this.roles = roles;
        },
        error => {
            this.errorMessage = <any>error;
            alert(this.errorMessage);
        });  
    }

    showDialogToAdd() {
        this.newRole = true;
        this.role = new UserRole(-1, -1, "");
        this.displayDialog = true;
    }

    save() {
        if (this.newRole) {

            // Call the service 
            this._RoleService.createRole(this.role).subscribe(role => {
                this.role = null;
                this.displayDialog = false;
                this.populateRoles()
            },
            error => {
                this.errorMessage = <any>error;
                alert(this.errorMessage);
            });  
        }
        else {

            // Call the service 
            this._RoleService.updateRole(this.role).subscribe(role => {
                this.role = null;
                this.displayDialog = false;
                this.populateRoles()
            },
            error => {
                this.errorMessage = <any>error;
                alert(this.errorMessage);
            });  
        }
    }

    delete() {
        // Call the service 
        this._RoleService.deleteRole(this.selectedRole.iD).subscribe(status => {
            if (status.success) {
                this.displayDialog = false;
                this.populateRoles()
            } else {
                alert(status.statusMessage);
            }
        },
        error => {
            this.errorMessage = <any>error;
            alert(this.errorMessage);
        });        
    }

    onRowSelect(event) {
        this.newRole = false;
        this.role = this.cloneRole(event.data);
        this.displayDialog = true;
    }

    cloneRole(r: IRole): IRole {
        let role = new UserRole(-1,-1,"");
        for (let prop in r) {
            role[prop] = r[prop];
        }
        return role;
    }
}

class UserRole implements IRole {
    constructor(public iD, public portalID, public roleName) { }
}