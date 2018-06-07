import { ITask } from './task';

export interface ITaskSearchResult {
    taskList: ITask[];
    totalRows: number;
    errorMessage: string;
}