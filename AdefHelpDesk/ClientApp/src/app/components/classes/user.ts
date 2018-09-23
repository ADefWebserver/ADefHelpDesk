import { IRole } from './role';

/* Defines the user entity */
export interface IUser {
    userId: number;
    userName: string;
    firstName: string;
    lastName: string;
    email: string;
    isLoggedIn: boolean;
    isSuperUser: boolean;
    password: string;
    riapassword: string;
    verificationCode: string;
    userRoles: IRole[];
}