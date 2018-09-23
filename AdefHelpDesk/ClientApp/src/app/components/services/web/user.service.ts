import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { IUser } from '../../classes/user';
import { IRegister } from '../../classes/register';
import { IProfile } from '../../classes/profile';
import { ILoginStatus } from '../../classes/loginStatus';
import { IRegisterStatus } from '../../classes/registerStatus';
import { IProfileStatus } from '../../classes/profileStatus';
import { IAuthentication } from '../../classes/authentication';
import { IMigration } from '../../classes/migration';
import { IVerification } from '../../classes/verification';

@Injectable()
export class UserService {

    constructor(private _httpClient: HttpClient) { }

    getCurrentUser(): Observable<IUser> {
        var _Url = 'api/Login';

        return this._httpClient.get<IUser>(_Url)
            .catch(this.handleError);
    }

    logOutUser(): Observable<IUser> {
        var _Url = 'api/LogOut';

        return this._httpClient.get<IUser>(_Url)
            .catch(this.handleError);
    }

    loginUser(Authentication: IAuthentication): Observable<ILoginStatus> {
        var _Url = 'api/Login';

        // Make the Angular Post
        return this._httpClient.post<ILoginStatus>(_Url, Authentication)
            .catch(this.handleError);
    }

    registerUser(Register: IRegister): Observable<IRegisterStatus> {
        var _Url = 'api/Register';

        // Make the Angular Post
        return this._httpClient.post<IRegisterStatus>(_Url, Register)
            .catch(this.handleError);
    }

    updateUser(Profile: IProfile): Observable<IProfileStatus> {
        var _Url = 'api/Profile';

        // Make the Angular Post
        return this._httpClient.post<IProfileStatus>(_Url, Profile)
            .catch(this.handleError);
    }

    migrateUser(Migration: IMigration): Observable<ILoginStatus> {
        var _Url = 'api/Migration';

        // Make the Angular Post
        return this._httpClient.post<ILoginStatus>(_Url, Migration)
            .catch(this.handleError);
    }

    verifyUser(Verification: IVerification): Observable<ILoginStatus> {
        var _Url = 'api/Verification';

        // Make the Angular Post
        return this._httpClient.post<ILoginStatus>(_Url, Verification)
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