import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

// See: BehaviorSubject vs Observable?
// https://stackoverflow.com/questions/39494058/behaviorsubject-vs-observable

@Injectable()
export class DialogService {
    private _showMessage = new BehaviorSubject<string>('');

    setMessage(paramMessage: string) {
        this._showMessage.next(paramMessage);
    }

    getMessage(): Observable<string> {
        return this._showMessage.asObservable();
    }
}