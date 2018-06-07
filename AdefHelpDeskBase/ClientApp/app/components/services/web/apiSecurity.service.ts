import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { IApiSecurityDTO } from '../../classes/apiSecurityDTO';
import { IStatus } from '../../classes/status';

@Injectable()
export class ApiSecurityService {

    // This is the URL to the end points
    private _BaseUrl = 'api/ApiSecurity/';

    constructor(private _httpClient: HttpClient) { }

    // ** Get **
    getApiSecuritys(): Observable<IApiSecurityDTO[]> {
        let _Url = 'api/ApiSecurity/Get/';

        // Make the Get
        return this._httpClient.get<IApiSecurityDTO[]>(
            _Url)
            .catch(this.handleError);
    }

    // ** Create **
    createApiSecurity(param: IApiSecurityDTO): Observable<IStatus> {

        // Make the Post
        return this._httpClient.post<IStatus>(this._BaseUrl, param)
            .catch(this.handleError);
    }

    // ** Update **
    updateApiSecurity(param: IApiSecurityDTO): Observable<IStatus> {

        // Make the Put
        return this._httpClient.put<IStatus>(
            this._BaseUrl + param.id,param)
            .catch(this.handleError);
    }

    // ** Delete **
    deleteApiSecurity(param: IApiSecurityDTO): Observable<void> {
        // A Delete does not return anything
        return this._httpClient.delete(this._BaseUrl + param.id)
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