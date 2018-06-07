import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

// See: BehaviorSubject vs Observable?
// https://stackoverflow.com/questions/39494058/behaviorsubject-vs-observable

@Injectable()
export class LoginWizardService {
    private _loggedIn = new BehaviorSubject<boolean>(false);

    setLoggedIn(paramCount: boolean) {
        this._loggedIn.next(paramCount);
    }

    getLoggedIn(): Observable<boolean> {
        return this._loggedIn.asObservable();
    }
}