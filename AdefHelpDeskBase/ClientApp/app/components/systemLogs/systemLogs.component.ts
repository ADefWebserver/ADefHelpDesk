import { Component, OnInit } from '@angular/core';

import { SystemLogService } from '../services/web/systemLog.service';

import { ISystemLog } from '../classes/systemLog';
import { ISearchParameters } from '../classes/searchParameters';
import { ISystemLogSearchResult } from '../classes/systemLogSearchResult';
import { LazyLoadEvent } from "primeng/components/common/api";

@Component({
    selector: 'systemLogs',
    templateUrl: './systemLogs.component.html'
})
export class SystemLogComponent {
    rowsPerPage: number = 10;
    pageNumber: number = 1;
    totalLogs: number;

    SearchResults: ISystemLog[];
    searchError: string;

    constructor(private _SystemLogService: SystemLogService) { }

    ngOnInit() {

    }

    loadLogsLazy(event: LazyLoadEvent) {
        this.searchError = "";
        this.rowsPerPage = event.rows;
        this.pageNumber = Math.floor(event.first / event.rows) + 1;
        this.populate();
    }

    populate() {

        let SearchParameters: ISearchParameters = {
            rowsPerPage: this.rowsPerPage,
            pageNumber: this.pageNumber
        }

        this._SystemLogService.SearchSystemLogs(SearchParameters).subscribe(
            (logResults: ISystemLogSearchResult) => {
                if (logResults.errorMessage.length < 1) {
                    this.SearchResults = logResults.systemLogList;
                    this.totalLogs = logResults.totalRows;
                }
                else {
                    this.searchError = logResults.errorMessage;
                }
            },
            error => this.searchError = <any>error
        )

    }
}