/* Defines the ApplicationSettings entity */
export interface IApplicationSettings {
    applicationName: string;
    applicationGUID: string;
    fileUploadPath: string;
    storagefiletype: string;
    azurestorageconnection: string;
    uploadPermission: string;
    allowRegistration: boolean;
    verifiedRegistration: boolean;
    valid: boolean;
    status: string;
    termsOfUse: string;
    privacyStatement: string;
    swaggerWebAddress: string;
}
