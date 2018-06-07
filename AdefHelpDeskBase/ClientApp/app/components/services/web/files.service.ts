import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { IDTONode } from '../../classes/DTONode';
import { IDTOResponse } from '../../classes/dtoResponse';
import { IDTOFileParameter } from "../../classes/DTOFileParameter";

@Injectable()
export class FilesService {

    constructor(private _httpClient: HttpClient) { }

    // ** Get files **
    getFiles(): Observable<IDTONode> {
        var _Url = 'api/Files/SystemFiles';

        // Call the client side code
        return this._httpClient.get<IDTONode>(_Url)
            .catch(this.handleError);
    }

    // ** Get file **
    getFile(paramNode: IDTONode): Observable<any> {
        var _Url = 'api/Files/ReturnContent';

        return this._httpClient.post(_Url, paramNode)
            .catch(this.handleError);
    }

    // ** Return file **
    returnFile(paramFileParameter: IDTOFileParameter): Observable<any> {
        var _Url = 'api/Files/ReturnFile';

        return this._httpClient.post(_Url, paramFileParameter, { responseType: "blob" })
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