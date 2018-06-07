import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { ITask } from '../../classes/task';
import { IStatus } from '../../classes/status';
import { ISearchTaskParameters } from '../../classes/searchTaskParameters';
import { ITaskSearchResult } from '../../classes/taskSearchResult';

@Injectable()
export class TaskService {

    constructor(private _httpClient: HttpClient) { }

    // ** Get **
    getTaskByID(paramTask: ITask): Observable<ITask> {
        let _Url = 'api/Task/RetrieveTask';

        return this._httpClient.post<ITask>(_Url, paramTask)
            .catch(this.handleError);
    }

    // ** Update **
    updateTask(paramTask: ITask) {
        let _Url = 'api/UploadTask/UpdateTask';

        return this._httpClient.post<IStatus>(_Url, paramTask)
            .catch(this.handleError);
    }

    // ** Delete Task **
    deleteTask(id: number): Observable<void> {
        // A Delete does not return anything
        let _Url = 'api/Task/Delete/';
        return this._httpClient.delete(_Url + id)
            .catch(this.handleError);
    }

    // ** Delete Task Detail **
    deleteTaskDetail(id: number): Observable<void> {
        // A Delete does not return anything
        let _Url = 'api/Task/DeleteTaskDetail/';
        return this._httpClient.delete(_Url + id)
            .catch(this.handleError);
    }

    // ** Search **
    searchTasks(SearchParameters: ISearchTaskParameters) {
        let _Url = 'api/Task/SearchTasks';

        return this._httpClient.post<ITaskSearchResult[]>(_Url, SearchParameters)
            .catch(this.handleError);
    }

    // ** Search Parameters **
    getSearchParameters() {
        let _Url = 'api/SearchParameters';

        return this._httpClient.get<ISearchTaskParameters>(_Url)
            .catch(this.handleError);
    }

    saveSearchParameters(SearchParameters: ISearchTaskParameters): Observable<void> {
        let _Url = 'api/SearchParameters/SaveSearchParameters';

        return this._httpClient.post<void>(_Url, SearchParameters)
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