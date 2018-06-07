import { ISystemLog } from "./systemLog";

export interface ISystemLogSearchResult {
    systemLogList: ISystemLog[]
    totalRows: number;
    errorMessage: string;
}