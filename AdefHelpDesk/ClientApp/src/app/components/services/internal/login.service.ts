import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { ILoginParameter } from '../../classes/loginParameter';

// See: BehaviorSubject vs Observable?
// https://stackoverflow.com/questions/39494058/behaviorsubject-vs-observable
 
@Injectable()
export class LoginService {
    private _loginParamater = new BehaviorSubject<ILoginParameter>({ showLogin: false, taskId: -1 });
 
    setLogin(loginParameter: ILoginParameter) {
        this._loginParamater.next(loginParameter);
    }
 
    getLogin(): Observable<ILoginParameter> {
        return this._loginParamater.asObservable();
    }
}