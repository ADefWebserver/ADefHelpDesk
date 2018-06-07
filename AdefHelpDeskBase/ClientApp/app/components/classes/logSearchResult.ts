import { ILog } from "./log";

export interface ILogSearchResult {
    logList: ILog[]
    totalRows: number;
    errorMessage: string;
}