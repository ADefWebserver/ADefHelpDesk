import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { LazyLoadEvent } from 'primeng/components/common/api';
import {
    InputTextModule,
    DropdownModule,
    ButtonModule,
    SelectItem
} from 'primeng/primeng';

import { DialogService } from '../services/internal/dialog.service';
import { IApplicationSettings } from '../classes/applicationSettings';
import { SettingsService } from '../services/web/settings.service';
import { ApiSecurityService } from '../services/web/apiSecurity.service';
import { IApiSecurityDTO } from '../classes/apiSecurityDTO';
import { IStatus } from '../classes/status';

@Component({
    selector: 'api-security',
    templateUrl: './api-security.component.html'
})
export class ApiSecurityComponent implements OnInit {
    public errorMessage: string;
    showWaitGraphic: boolean = false;

    public SearchResults: IApiSecurityDTO[];
    public displayDialog: boolean;
    public displayRoleDialog: boolean;
    public user: IApiSecurityDTO = new User(-1, "", "", "", "", "", "", "", false);
    public newUser: boolean;
    public EditUserIndex: number = 0;
    public applicationGUID: string;
    public swaggerWebAddress: string = "";

    constructor(
        private _ApiSecurityService: ApiSecurityService,
        private _settingsService: SettingsService,
        private _router: Router,
        private _dialogService: DialogService) { }

    ngOnInit() {
        this.populateUserList();
        this.GetSettings();
    }

    GetSettings() {

        this._settingsService.getApplicationSettings().subscribe(
            settings => {
              this.applicationGUID = settings.applicationGUID;
              this.swaggerWebAddress = settings.swaggerWebAddress;            
            });
    }

    loadUsersLazy(event: LazyLoadEvent) {
        this.populateUserList();
    }

    populateUserList() {

        this._ApiSecurityService.getApiSecuritys().subscribe(
            (usersResults: IApiSecurityDTO[]) => {
                this.SearchResults = usersResults;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    editUserTabChange(e) {
        // When switching to the connection Tab - save first
        if (this.newUser) {

            // Call the service 
            this.showWaitGraphic = true;
            this._ApiSecurityService.createApiSecurity(this.user).subscribe(status => {
                this.showWaitGraphic = false;
                if (status.success) {
                    this.EditUserIndex = e.index;
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
            this._ApiSecurityService.updateApiSecurity(this.user).subscribe(status => {
                if (status.success) {
                    this.EditUserIndex = e.index;
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

    onUserSelect(event: any) {
        this.newUser = false;

        this.user = event.data;
        this.displayDialog = true;
        this.EditUserIndex = 0;
    }

    showDialogToAdd() {
        this.newUser = true;
      this.user = new User(-1, "", "", "", "", "", "", "", false);
        this.displayDialog = true;
        this.EditUserIndex = 0;
    }

    save() {
        if (this.newUser) {

            // Call the service 
            this.showWaitGraphic = true;
            this._ApiSecurityService.createApiSecurity(this.user).subscribe(status => {
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
            this._ApiSecurityService.updateApiSecurity(this.user).subscribe(status => {
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
        this._ApiSecurityService.deleteApiSecurity(this.user).subscribe(() => {
            this.displayDialog = false;
            this.populateUserList()
        },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

}

// Classes

class User implements IApiSecurityDTO {
    constructor(
      public id,
      public username,
      public password,
      public contactName,
      public contactCompany,
      public contactWebsite,
      public contactEmail,
      public contactPhone,
      public isActive
    ) { }
}
