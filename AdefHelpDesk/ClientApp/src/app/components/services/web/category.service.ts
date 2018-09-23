import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; 
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { ICategory } from '../../classes/category';
import { ICategoryNode } from '../../classes/categoryNode';
import { TreeNode } from 'primeng/primeng';
@Injectable()
export class CategoryService {

    // This is the URL to the end points
    private _updateTreeNodeUrl = 'api/Category/';

    constructor(private _httpClient: HttpClient) { }

    // ** Build Tree Category **
    getTreeCategorys(paramUseCache: boolean): Observable<ICategory[]> {
        var _Url = 'api/CategoryTree/';

        return this._httpClient.get<ICategory[]>(_Url + paramUseCache)
            .catch(this.handleError);
    }

    // ** Build Category Dropdown **
    getCategorys(): Observable<ICategory[]> {
        var _Url = 'api/CategoryNodes/GetCategoryNodes';

        return this._httpClient.get<ICategory[]>(_Url)
            .catch(this.handleError);
    }

    // ** Create a Category Node **
    createCategoryNode(paramtreenode: ICategoryNode): Observable<ICategoryNode> {

        // Make the Post
        return this._httpClient.post<ICategoryNode>(this._updateTreeNodeUrl, paramtreenode)
            .catch(this.handleError);
    }

    // ** Update a Category Node **
    updateCategoryNode(paramtreenode: ICategoryNode): Observable<void> {

        // Make the Put
        return this._httpClient.put(
            this._updateTreeNodeUrl + paramtreenode.Id,paramtreenode)
            .catch(this.handleError);
    }

    // ** Delete a Tree Node **
    deleteTreeNode(id: number): Observable<void> {
        // A Delete does not return anything
        return this._httpClient.delete(this._updateTreeNodeUrl + id)
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