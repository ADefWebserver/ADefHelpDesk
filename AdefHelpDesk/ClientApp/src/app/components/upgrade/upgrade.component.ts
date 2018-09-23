import {
    Component, OnInit, OnDestroy, Input, Output,
    ViewContainerRef, EventEmitter, ViewChild, trigger
} from '@angular/core';
import {
    Router, ActivatedRoute
} from '@angular/router';
import { Subscription } from 'rxjs/Subscription';

import { IVersion } from '../classes/version';
import { IDTONode } from '../classes/DTONode';
import { InstallWizardService } from '../services/web/installWizard.service';
import { DialogService } from '../services/internal/dialog.service';

@Component({
    selector: 'upgrade',
    templateUrl: './upgrade.component.html'
})
export class UpgradeComponent implements OnInit {
    errorMessage: string;
    version: IVersion;
    versionNumber: string = "";

    // Register the service
    constructor(
        private _installWizardService: InstallWizardService,
        private _dialogService: DialogService) { }

    ngOnInit(): void {
        this._installWizardService.getCurrentVersion().subscribe(
            (version :IVersion) => {
                // Set the current application version
                this.version = version;
                this.versionNumber = version.versionNumber;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    public onUploadHandler(event) {
        // Called after the file is uploaded
        alert(event.xhr.responseText);
    }
}