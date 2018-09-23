import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { IDashboard } from '../../classes/dashboard';

@Injectable()
export class DashboardService {

    constructor(private _httpClient: HttpClient) { }

    getDashboard(): Observable<IDashboard> {
        var _Url = 'api/Dashboard/DashboardValues';

        return this._httpClient.get<IDashboard>(_Url)
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