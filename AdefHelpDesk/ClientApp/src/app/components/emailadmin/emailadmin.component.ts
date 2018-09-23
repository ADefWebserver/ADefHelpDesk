import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, Validators } from '@angular/forms';

import { Subscription } from 'rxjs/Subscription';

import {
    DialogModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    RadioButtonModule
} from 'primeng/primeng';

import { ISMTPSettings } from '../classes/smtpSettings';
import { SettingsService } from '../services/web/settings.service';
import { DialogService } from '../services/internal/dialog.service';

@Component({
    selector: 'emailadmin',
    templateUrl: './emailadmin.component.html',
    styleUrls: ['./emailadmin.component.css']
})
export class EmailadminComponent implements OnInit {
    showWaitGraphic: boolean = false;
    errorMessage: string = "";
    updateType: string = "Save";

    public emailAdminForm = this.fb.group({
        smtpServer: ["", Validators.required],
        smtpAuthentication: ["", Validators.required],
        smtpSecure: [""],
        smtpUserName: [""],
        smtpPassword: [""],
        smtpFromEmail: ["", Validators.required]
    });

    constructor(
        public fb: FormBuilder,
        private _settingsService: SettingsService,
        private _dialogService: DialogService) { }

    ngOnInit() {

        // Get current settings
        this._settingsService.getSMTPSettings().subscribe(
            settings => {

                // Set the values on the form
                this.emailAdminForm.setValue({
                    smtpServer: settings.smtpServer,
                    smtpAuthentication: settings.smtpAuthentication,
                    smtpSecure: settings.smtpSecure,
                    smtpUserName: settings.smtpUserName,                    
                    smtpFromEmail: settings.smtpFromEmail,
                    smtpPassword: settings.smtpPassword
                });

            });
    }

    TestSMTP() {
        this.updateType = "Test";
        this.Update();
    }

    SaveSMTP() {
        this.updateType = "Save";
        this.Update();
    }

    Update() {
        // Get the form values
        let formData = this.emailAdminForm.value;
        let smtpServer = formData.smtpServer;
        let smtpAuthentication = formData.smtpAuthentication;
        let smtpSecure = formData.smtpSecure;
        let smtpUserName = formData.smtpUserName;
        let smtpPassword = formData.smtpPassword;
        let smtpFromEmail = formData.smtpFromEmail;

        let SMTPSettings: ISMTPSettings = {
            smtpServer: smtpServer,
            smtpAuthentication: smtpAuthentication,
            smtpSecure: smtpSecure,
            smtpUserName: smtpUserName,
            smtpPassword: smtpPassword,
            smtpFromEmail: smtpFromEmail,
            smtpStatus: '',
            updateType: this.updateType,
            smtpValid: true
        };

        // Call the service
        this.showWaitGraphic = true;
        this._settingsService.updateSMTPSettings(SMTPSettings).subscribe(
            objISMTPSettings => {
                this.showWaitGraphic = false;
                this._dialogService.setMessage(objISMTPSettings.smtpStatus); 
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }
}