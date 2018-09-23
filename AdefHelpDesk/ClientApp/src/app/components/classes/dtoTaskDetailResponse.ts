import { ITaskDetail } from "./task";
/* Defines the dtoTaskDetailResponse entity */
export interface IDTOTaskDetailResponse {
    isSuccess: boolean;
    message: string;
    taskDetail?: ITaskDetail; 
}