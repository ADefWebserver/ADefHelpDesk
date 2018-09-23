import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { ISMTPSettings } from '../../classes/smtpSettings';
import { IApplicationSettings } from '../../classes/applicationSettings';

@Injectable()
export class SettingsService {

    constructor(private _httpClient: HttpClient) { }

    getSMTPSettings(): Observable<ISMTPSettings> {
        var _Url = 'api/EmailAdmin/SMTPSettings';

        return this._httpClient.get<ISMTPSettings>(_Url)
            .catch(this.handleError);
    }

    getApplicationSettings(): Observable<IApplicationSettings> {
        var _Url = 'api/ApplicationSettings/GetSettings';

        return this._httpClient.get<IApplicationSettings>(_Url)
            .catch(this.handleError);
    }

    getApplicationName(): Observable<IApplicationSettings> {
        var _Url = 'api/ApplicationSettings/GetApplicationName';

        return this._httpClient.get<IApplicationSettings>(_Url)
            .catch(this.handleError);
    }

    getTermsOfUse(): Observable<IApplicationSettings> {
        var _Url = 'api/ApplicationSettings/GetTermsOfUse';

        return this._httpClient.get<IApplicationSettings>(_Url)
            .catch(this.handleError);
    }

    getPrivacyStatement(): Observable<IApplicationSettings> {
        var _Url = 'api/ApplicationSettings/GetPrivacyStatement';

        return this._httpClient.get<IApplicationSettings>(_Url)
            .catch(this.handleError);
    }

    updateSMTPSettings(param: ISMTPSettings): Observable<ISMTPSettings> {
        var _Url = 'api/EmailAdmin/SMTPSetting';

        // Make the Put
        return this._httpClient.put<ISMTPSettings>(_Url,param)
            .catch(this.handleError);
    }

    updateApplicationSettings(param: IApplicationSettings): Observable<IApplicationSettings> {
        var _Url = 'api/ApplicationSettings/SetSettings';

        // Make the Put
        return this._httpClient.put<IApplicationSettings>(_Url,param)
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