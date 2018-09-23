import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { IRole } from '../../classes/role';
import { IStatus } from '../../classes/status';

@Injectable()
export class RoleService {

    // This is the URL to the end points
    private _BaseUrl = 'api/Role/';

    constructor(private _httpClient: HttpClient) { }

    // ** Get **
    getRoles(): Observable<IRole[]> {
        var _Url = 'api/Role/GetRoles';

        return this._httpClient.get<IRole[]>(_Url)
            .catch(this.handleError);
    }

    // ** Create **
    createRole(paramtreenode: IRole): Observable<IRole> {

        // Make the Post
        return this._httpClient.post<IRole>(this._BaseUrl, paramtreenode)
            .catch(this.handleError);
    }

    // ** Update **
    updateRole(param: IRole): Observable<IStatus> {

        // Make the Put
        return this._httpClient.put<IStatus>(
            this._BaseUrl + param.iD,param)
            .catch(this.handleError);
    }

    // ** Delete **
    deleteRole(id: number): Observable<IStatus> {
        // A Delete does not return anything
        return this._httpClient.delete<IStatus>(this._BaseUrl + id)
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