import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

// See: BehaviorSubject vs Observable?
// https://stackoverflow.com/questions/39494058/behaviorsubject-vs-observable
 
@Injectable()
export class ProfileService {
    private _showProfile = new BehaviorSubject<boolean>(false);
 
    setVisibility(paramCount: boolean) {
        this._showProfile.next(paramCount);
    }
 
    getVisbility(): Observable<boolean> {
        return this._showProfile.asObservable();
    }
}