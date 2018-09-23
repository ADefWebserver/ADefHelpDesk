import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { ITask } from '../../classes/task'; 

// See: BehaviorSubject vs Observable?
// https://stackoverflow.com/questions/39494058/behaviorsubject-vs-observable
 
@Injectable()
export class TaskVisibilityService {
    private _taskParameter = new BehaviorSubject<ITask>({});
    private _taskDialogParameter = new BehaviorSubject<ITask>({});
 
    setTaskVisibility(param: ITask) {
        this._taskParameter.next(param);
    }
 
    getTaskVisibility(): Observable<ITask> {
        return this._taskParameter.asObservable();
    }

    setTaskDialogVisibility(param: ITask) {
        this._taskDialogParameter.next(param);
    }

    getTaskDialogVisibility(): Observable<ITask> {
        return this._taskDialogParameter.asObservable();
    }
}