import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { IQueryStringParameter } from '../../classes/querystringParameters';

// See: BehaviorSubject vs Observable?
// https://stackoverflow.com/questions/39494058/behaviorsubject-vs-observable
 
@Injectable()
export class QueryStringService {
    private _querystringParameter = new BehaviorSubject<IQueryStringParameter>({userName: undefined, ticketNumber: undefined, code: undefined});
 
    setQueryString(param: IQueryStringParameter) {
        this._querystringParameter.next(param);
    }
 
    getQueryString(): Observable<IQueryStringParameter> {
        return this._querystringParameter.asObservable();
    }
}