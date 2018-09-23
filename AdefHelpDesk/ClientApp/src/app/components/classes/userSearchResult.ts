import { IUser } from './user';

export interface IUserSearchResult {
    userList: IUser[];
    totalRows: number;
    errorMessage: string;
}