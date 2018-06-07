import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { ISystemLog } from '../../classes/systemLog';
import { ISearchParameters } from '../../classes/searchParameters';
import { ISystemLogSearchResult } from '../../classes/systemLogSearchResult';

@Injectable()
export class SystemLogService {
    constructor(private _httpClient: HttpClient) { }

    SearchSystemLogs(SearchParameters: ISearchParameters) {
        let _Url = 'api/SystemLog/SystemLogs';

        return this._httpClient.post<ISystemLogSearchResult[]>(_Url, SearchParameters)
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