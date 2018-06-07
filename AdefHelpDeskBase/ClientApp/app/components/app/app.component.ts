import { Component, OnInit } from '@angular/core';

import { Observable } from 'rxjs/Rx';
import { Subscription } from 'rxjs/Subscription';
import { MenuItem } from 'primeng/primeng';

import { HTMLDialogService } from '../services/internal/htmldialog.service';
import { UserService } from '../services/web/user.service';
import { SettingsService } from '../services/web/settings.service';
import { IUser } from '../classes/user';

@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
    public ApplicationName: string = "";
    public FooterMessage: string;
    errorMessage: string = "";
    user: IUser;

    constructor(
        private _userService: UserService,
        private _settingsService: SettingsService,
        private _htmldialogService: HTMLDialogService) { }

    ngOnInit() {
        // Call this method every 15 minutes to keep the user logged in
        Observable.interval((1000 * 60) * 15).subscribe(x => {
            this._userService.getCurrentUser().subscribe(
                user => {
                    // Do nothing this is to keep the user is logged in
                    this.user = user;
                });
        });

        // Call the service to get the application name
        this._settingsService.getApplicationName().subscribe(
            objSettings => {

                this.ApplicationName = objSettings.applicationName;
            },
            error => {
                this.errorMessage = <any>error;
            });

        // Set FooterMessage
        var year = new Date();
        this.FooterMessage = 'Copyright ' + year.getUTCFullYear();
    }

    showPrivacyStatement() {
        this._settingsService.getPrivacyStatement().subscribe(
            objApplicationSettings => {

                this._htmldialogService.setMessage(objApplicationSettings.privacyStatement);

            },
            error => {
                this.errorMessage = <any>error;
                this._htmldialogService.setMessage(this.errorMessage);
            });
    }

    showTermsOfUse() {
        this._settingsService.getTermsOfUse().subscribe(
            objApplicationSettings => {

                this._htmldialogService.setMessage(objApplicationSettings.termsOfUse);

            },
            error => {
                this.errorMessage = <any>error;
                this._htmldialogService.setMessage(this.errorMessage);
            });
    }
}
