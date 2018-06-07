import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, Validators } from '@angular/forms';

import { Subscription } from 'rxjs/Subscription';

import {
    DialogModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    RadioButtonModule,
    SelectItem
} from 'primeng/primeng';

import { IApplicationSettings } from '../classes/applicationSettings';
import { SettingsService } from '../services/web/settings.service';
import { DialogService } from '../services/internal/dialog.service';

@Component({
    selector: 'applicationsettings',
    templateUrl: './applicationsettings.component.html'
})
export class ApplicationsettingsComponent implements OnInit {
    public showWaitGraphic: boolean = false;
    public errorMessage: string = "";
    public applicationName: string = "";
    public fileUploadPath: string = "";
    public storagefiletype: string = "";
    public azurestorageconnection: string = "";
    public uploadPermission: string = "";
    public selectedPermission: string;
    public applicationGUID: string;
    public storageFileTypeDropdown: SelectItem[] = [];
    public uploadPermissionDropdown: SelectItem[] = [];
    public selectedAllowUserRegistration: boolean;
    public selectedVerifiedRegistration: boolean;
    public allowUserRegistrationDropdown: SelectItem[] = [];
    public verifiedRegistrationDropdown: SelectItem[] = [];

    constructor(
        private _settingsService: SettingsService,
        private _dialogService: DialogService) { }

    ngOnInit() {
        this.populateDropdowns();
    }

    populateDropdowns() {

        // ** Upload Permission

        // Create a new SelectedItem
        let newSelectedItem1: SelectItem = {
            label: "All",
            value: "All"
        }

        // Add Selected Item to the DropDown
        this.uploadPermissionDropdown.push(newSelectedItem1);

        // Create a new SelectedItem
        let newSelectedItem2: SelectItem = {
            label: "Administrator",
            value: "Administrator"
        }

        // Add Selected Item to the DropDown
        this.uploadPermissionDropdown.push(newSelectedItem2);

        // Create a new SelectedItem
        let newSelectedItem3: SelectItem = {
            label: "Administrator/Registered Users",
            value: "Administrator/Registered Users"
        }

        // ** Storage File Type

        // Create a new SelectedItem
        let newSelectedItem4: SelectItem = {
            label: "File System",
            value: "FileSystem"
        }

        // Add Selected Item to the DropDown
        this.storageFileTypeDropdown.push(newSelectedItem4);

        // Create a new SelectedItem
        let newSelectedItem5: SelectItem = {
            label: "Azure Storage",
            value: "AzureStorage"
        }

        // Add Selected Item to the DropDown
        this.storageFileTypeDropdown.push(newSelectedItem5);

        // ** Allow User Registration

        // Create SelectedItems
        let TrueItem: SelectItem = {
            label: "True",
            value: true
        }

        let FalseItem: SelectItem = {
            label: "False",
            value: false
        }

        // Add Items to the DropDowns
        this.allowUserRegistrationDropdown.push(TrueItem);
        this.allowUserRegistrationDropdown.push(FalseItem);

        this.verifiedRegistrationDropdown.push(TrueItem);
        this.verifiedRegistrationDropdown.push(FalseItem);

        // Get current settings
        this.GetCurrentSettings();
    }

    GetCurrentSettings() {

        this._settingsService.getApplicationSettings().subscribe(
            settings => {

                // Set the values 
                this.applicationName = settings.applicationName;
                this.fileUploadPath = settings.fileUploadPath;
                this.storagefiletype = settings.storagefiletype;
                this.azurestorageconnection = settings.azurestorageconnection;
                this.selectedPermission = settings.uploadPermission;
                this.selectedAllowUserRegistration = settings.allowRegistration;
                this.selectedVerifiedRegistration = settings.verifiedRegistration;
                this.applicationGUID = settings.applicationGUID;
            });
    }

    Save() {
        // Get the form values
        let ApplicationSettings: IApplicationSettings = {
            applicationName: this.applicationName,
            applicationGUID: this.applicationGUID,
            fileUploadPath: this.fileUploadPath,
            storagefiletype: this.storagefiletype,
            azurestorageconnection: this.azurestorageconnection,
            uploadPermission: this.selectedPermission,
            allowRegistration: this.selectedAllowUserRegistration,
            verifiedRegistration: this.selectedVerifiedRegistration,
            status: "",
            valid: true,
            termsOfUse: "",
            privacyStatement: ""
        };

        // Call the service
        this.showWaitGraphic = true;
        this._settingsService.updateApplicationSettings(ApplicationSettings).subscribe(
            objApplicationSettings => {
                this.showWaitGraphic = false;

                // Was update successful?
                if (!objApplicationSettings.valid) {
                    this._dialogService.setMessage(objApplicationSettings.status);
                } else {
                    this._dialogService.setMessage("Settings Updated!");
                }

            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }
}