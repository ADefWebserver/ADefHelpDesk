export interface ISystemLog {
    logID: number;
    logType: string;
    logMessage: string;
    userName?: string;
    createdDate: string;
}