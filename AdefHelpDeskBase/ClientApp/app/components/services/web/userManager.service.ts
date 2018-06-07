import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { IUser } from '../../classes/user';
import { ISearchParameters } from '../../classes/searchParameters';
import { IUserSearchResult } from '../../classes/userSearchResult';
import { IStatus } from '../../classes/status';

@Injectable()
export class UserManagerService {

    // This is the URL to the end points
    private _BaseUrl = 'api/UserManager/';

    constructor(private _httpClient: HttpClient) { }

    // ** Get **
    getUser(param: IUser): Observable<IUser> {

        // Make the Get
        return this._httpClient.get<IUser>(
            this._BaseUrl + param.userId)
            .catch(this.handleError);
    }

    // ** Search **
    searchUsers(SearchParameters: ISearchParameters) {
        let _Url = 'api/UserManager/SearchUsers';

        return this._httpClient.post<IUserSearchResult[]>(_Url, SearchParameters)
            .catch(this.handleError);
    }

    // ** Create **
    createUser(param: IUser): Observable<IStatus> {
        let _Url = 'api/UserManager/CreateUser';

        // Make the Post
        return this._httpClient.post<IStatus>(_Url, param)
            .catch(this.handleError);
    }

    // ** Update **
    updateUser(param: IUser): Observable<IStatus> {

        // Make the Put
        return this._httpClient.put<IStatus>(
            this._BaseUrl + param.userId,param)
            .catch(this.handleError);
    }

    // ** Delete **
    deleteUser(param: IUser): Observable<void> {
        // A Delete does not return anything
        return this._httpClient.delete(this._BaseUrl + param.userId)
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