import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

// See: BehaviorSubject vs Observable?
// https://stackoverflow.com/questions/39494058/behaviorsubject-vs-observable
 
@Injectable()
export class SetupService {
    private _setup = new BehaviorSubject<boolean>(false);
 
    setSetupMode(paramCount: boolean) {
        this._setup.next(paramCount);
    }
 
    isSetupMode(): Observable<boolean> {
        return this._setup.asObservable();
    }
}