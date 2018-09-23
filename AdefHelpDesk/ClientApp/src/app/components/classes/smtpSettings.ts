/* Defines the SMTPSettings entity */
export interface ISMTPSettings {
    smtpServer: string;
    smtpAuthentication: string;
    smtpSecure: string;
    smtpUserName: string;
    smtpPassword: string;
    smtpFromEmail: string;
    smtpStatus: string;
    updateType: string;
    smtpValid: boolean;
}