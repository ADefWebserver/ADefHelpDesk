import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { IVersion } from '../../classes/version';
import { IStatus } from '../../classes/status';
import { IRegister } from '../../classes/register';
import { IRegisterStatus } from '../../classes/registerStatus';
import { IConnectionSetting } from '../../classes/connectionSetting';
import { IAuthentication } from '../../classes/authentication';

@Injectable()
export class InstallWizardService {

    constructor(private _httpClient: HttpClient) { }

    getCurrentVersion(): Observable<IVersion> {
        var _Url = 'api/InstallWizard/CurrentVersion';

        return this._httpClient.get<IVersion>(_Url)
            .catch(this.handleError);
    }

    updateDatabase(): Observable<IStatus> {
        var _Url = 'api/InstallWizard/UpdateDatabase';

        return this._httpClient.get<IStatus[]>(_Url)
            .catch(this.handleError);
    }

    setConnection(ConnectionSetting: IConnectionSetting): Observable<IStatus> {
        var _Url = 'api/InstallWizard/ConnectionSetting';

        // Make the Angular Post
        // passing the objConnectionSetting
        return this._httpClient.post(_Url, ConnectionSetting)
            .catch(this.handleError);
    }

    createAdmin(Register: IRegister): Observable<IRegisterStatus> {
        var _Url = 'api/InstallWizard/CreateAdminLogin';

        // Make the Angular Post
        return this._httpClient.post(_Url, Register)
            .catch(this.handleError);
    }

    // Utility
    private handleError(error: HttpErrorResponse) {
        console.error(error);
        let customError: string = null;
        if (error.error) {
            customError = error.status === 400 ? error.error : error.statusText
        }
        return Observable.throw(customError || 'Server error');
    }
}