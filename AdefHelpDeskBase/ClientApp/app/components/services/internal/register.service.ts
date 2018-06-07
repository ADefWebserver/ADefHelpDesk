import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

// See: BehaviorSubject vs Observable?
// https://stackoverflow.com/questions/39494058/behaviorsubject-vs-observable
 
@Injectable()
export class RegisterService {
    private _showRegister = new BehaviorSubject<boolean>(false);
    private _registrationEnabled = new BehaviorSubject<boolean>(false);

    setVisibility(paramCount: boolean) {
        this._showRegister.next(paramCount);
    }

    setRegistrationEnabled(paramCount: boolean) {
        this._registrationEnabled.next(paramCount);
    }
 
    getVisbility(): Observable<boolean> {
        return this._showRegister.asObservable();
    }

    getRegistrationEnabled(): Observable<boolean> {
        return this._registrationEnabled.asObservable();
    }
}