/* Defines the ApiSecurityDTO entity */
export interface IApiSecurityDTO {
    id: number;
    username: string;
    password: string;
    contactName: string;
    contactCompany: string;
    contactWebsite: string;
    contactEmail: string;
    contactPhone: string;
    isActive: boolean;
}
