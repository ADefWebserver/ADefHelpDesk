import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { ILog } from '../../classes/log';
import { ISearchParameters } from '../../classes/searchParameters';
import { ILogSearchResult } from '../../classes/logSearchResult';

@Injectable()
export class LogService {
    constructor(private _httpClient: HttpClient) { }

    SearchLogs(SearchParameters: ISearchParameters) {
        let _Url = 'api/Log/Logs';

        return this._httpClient.post<ILogSearchResult[]>(_Url, SearchParameters)
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