/* Defines the connectionSetting entity */
export interface IConnectionSetting {
    DatabaseName: string;
    ServerName: string;
    IntegratedSecurity: boolean;
    Username: string;
    Password: string;
}