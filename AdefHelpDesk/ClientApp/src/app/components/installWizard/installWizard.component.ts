import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';

import {
    TabViewModule,
    PanelModule,
    InputTextModule,
    InputSwitchModule,
    ButtonModule,
    PasswordModule
}
    from 'primeng/primeng';

import { IVersion } from '../classes/version';
import { IStatus } from '../classes/status';
import { InstallWizardService } from '../services/web/installWizard.service';
import { panelType } from './panelType';
import { IConnectionSetting } from '../classes/connectionSetting';
import { IAuthentication } from '../classes/authentication';

import { IUser } from '../classes/user';
import { IRegister } from '../classes/register';
import { IRegisterStatus } from '../classes/registerStatus';
import { LoginService } from '../services/internal/login.service';
import { LoginWizardService } from '../services/internal/loginWizard.service';
import { UserService } from '../services/web/user.service';
import { SetupService } from '../services/internal/setup.service';
import { DialogService } from '../services/internal/dialog.service';

@Component({
    selector: 'version-detail',
    templateUrl: './installWizard.component.html'
})
export class InstallWizardComponent implements OnInit {
    pageTitle: string = 'Current User';
    showWaitGraphic: boolean = false;

    LoginWizardSubscription: Subscription;
    SetUpSubscription: Subscription;

    version: IVersion;
    errorMessage: string;

    user: IUser;
    setUpMode: boolean = false;

    DatabaseName: string = "ADefHelpDesk";
    ServerName: string = "(local)";
    IntegratedSecurity: boolean = false;
    Username: string = "";
    Password: string = "";

    CreateAdminUsername: string = "";
    CreateAdminFirstName: string = "";
    CreateAdminLastName: string = "";
    CreateAdminEmail: string = "";
    CreateAdminPassword: string = "";

    DatabaseConnectionMessage: string = "";
    InstallStatus: string = "Installing Scripts...";

    DatabaseConfigurationPanel_Disabled: boolean = false;
    DatabaseConfigurationPanel_Selected: boolean = true;
    AdministratorCreationPanel_Selected: boolean = true;
    AdministratorCreationPanel_Disabled: boolean = false;
    AdministratorLoginPanel_Selected: boolean = true;
    AdministratorLoginPanel_Disabled: boolean = false;
    InstallScriptsPanel_Disabled: boolean = true;
    InstallScriptsPanel_Selected: boolean = false;

    // Register the service
    constructor(
        private _installWizardService: InstallWizardService,
        private _loginService: LoginService,
        private _loginWizardService: LoginWizardService,
        private _userService: UserService,
        private _setupService: SetupService,
        private _dialogService: DialogService,
        private _router: Router) {
    }

    ngOnInit(): void {
        this.showWaitGraphic = false;

        // Subscribe to the SetUpSubscription Service
        this.SetUpSubscription = this._setupService.isSetupMode().subscribe(
            (isSetupMode: boolean) => {
                // SetUp mode causes the menu bar to dissapear
                // when true and reappear when false
                this.setUpMode = isSetupMode;
            });

        // Subscribe to the LoginSubscription Service
        this.LoginWizardSubscription = this._loginWizardService.getLoggedIn().subscribe(
            (loggedIn: boolean) => {
                if (loggedIn) {
                    // The user may be logged in
                    this.getCurrentUser();
                }
            });

        // Call the method that gets database status
        // and set the active panel
        this.getDatabaseStatus();
    }

    getDatabaseStatus() {
        // Call the service to get the current database status
        // if isNewDatabase is returned it means version table does not exist
        this._installWizardService.getCurrentVersion().subscribe(
            version => {
                // Set the current application version
                this.version = version;

                if (this.version.isNewDatabase) {
                    // The database is not set up
                    this.setActvePanel(panelType.DatabaseConfiguration);
                }
                else {
                    // The database is set up
                    if (this.version.isUpToDate) {

                        // Everything is set up                        
                        this._setupService.setSetupMode(false);

                        // Show the application
                        this._router.navigateByUrl('/home');
                    }
                    else {
                        // Administrator must log in to continue
                        this.setActvePanel(panelType.AdministratorLogin);
                    }
                }
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    getCurrentUser() {
        // Call the service
        this._userService.getCurrentUser().subscribe(
            (user: IUser) => {
                this.user = user;

                // We need to check the user if on the Administrator login screen
                if (this.AdministratorLoginPanel_Selected == true) {
                    // Only allow an Administrator to proceed
                    if (this.user.isSuperUser) {
                        // Move to the next step
                        this.setActvePanel(panelType.InstallScripts);
                        // Set InstallStatus
                        this.InstallStatus = "Installing...";

                        // Call the update database method
                        this._installWizardService.updateDatabase().subscribe(
                            response => {
                                if (response.success) {
                                    this.InstallStatus = "Complete!";
                                }
                                else {
                                    this._dialogService.setMessage(response.statusMessage);
                                }
                            },
                            error => this.errorMessage = <any>error);
                    }
                    else {
                        this._dialogService.setMessage("You must be an administrator to continue");
                    }
                }
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    // Events

    setConnection() {
        this.errorMessage = "";
        // Create a ConnectionSettings object
        let ConnectionSetting: IConnectionSetting = {
            DatabaseName: this.DatabaseName,
            ServerName: this.ServerName,
            IntegratedSecurity: this.IntegratedSecurity,
            Username: this.Username,
            Password: this.Password
        }

        // Call the service 
        this.showWaitGraphic = true;
        this._installWizardService.setConnection(ConnectionSetting).subscribe(
            (connectionResponse: IStatus) => {
                this.showWaitGraphic = false;
                this.DatabaseConnectionMessage = connectionResponse.statusMessage;
                
                if (connectionResponse.success) {
                    // Move to the next step
                    this.setActvePanel(panelType.AdministratorCreation);
                } else {
                    alert(this.DatabaseConnectionMessage);
                }
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    createAdministrator() {
        this.errorMessage = "";
        let userName = this.CreateAdminUsername;
        let firstName = this.CreateAdminFirstName;
        let lastName = this.CreateAdminLastName
        let email = this.CreateAdminEmail;
        let password = this.CreateAdminPassword;

        if (this.validatePassword(password) &&
            this.validateEmail(email)) {

            let Register: IRegister = {
                userName: userName,
                firstName: firstName,
                lastName: lastName,
                email: email,
                password: password
            };

            // Call the service 
            this.showWaitGraphic = true;

            this._installWizardService.createAdmin(Register).subscribe(
                (RegisterStatus: IRegisterStatus) => {
                    this.showWaitGraphic = false;

                    // Was Register successful?
                    if (!RegisterStatus.isSuccessful) {
                        alert(RegisterStatus.status);
                    } else {
                        // Everything is set-up
                        this.CompleteWizard();
                    }

                },
                error => {
                    this.errorMessage = <any>error;
                    this.showWaitGraphic = false;
                    this._dialogService.setMessage(this.errorMessage);
                });
        }
        else {
            alert('password [ ' + password + ' ] is not strong enough');
        }
    }

    CompleteWizard() {
        // Call getDatabaseStatus
        // This should trigger navigation to
        // the application
        this.getDatabaseStatus();
    }

    // Utility

    setActvePanel(panel: panelType) {
        this.errorMessage = "";
        // First set everything to false
        this.DatabaseConfigurationPanel_Disabled = false;
        this.DatabaseConfigurationPanel_Selected = false;
        this.AdministratorCreationPanel_Disabled = false;
        this.AdministratorCreationPanel_Selected = false;
        this.AdministratorLoginPanel_Disabled = false;
        this.AdministratorLoginPanel_Selected = false;
        this.InstallScriptsPanel_Disabled = false;
        this.InstallScriptsPanel_Selected = false;

        switch (panel) {
            case (panel = panelType.DatabaseConfiguration):
                this.DatabaseConfigurationPanel_Selected = true;
                this.AdministratorCreationPanel_Disabled = true;
                this.AdministratorLoginPanel_Disabled = true;
                this.InstallScriptsPanel_Disabled = true;
                break;
            case (panel = panelType.AdministratorCreation):
                this.AdministratorCreationPanel_Selected = true;
                this.DatabaseConfigurationPanel_Disabled = true;
                this.AdministratorLoginPanel_Disabled = true;
                this.InstallScriptsPanel_Disabled = true;
                break;
            case (panel = panelType.AdministratorLogin):
                this.AdministratorLoginPanel_Selected = true;
                this.DatabaseConfigurationPanel_Disabled = true;
                this.AdministratorCreationPanel_Disabled = true;
                this.InstallScriptsPanel_Disabled = true;
                break;
            case (panel = panelType.InstallScripts):
                this.InstallScriptsPanel_Selected = true;
                this.DatabaseConfigurationPanel_Disabled = true;
                this.AdministratorCreationPanel_Disabled = true;
                this.AdministratorLoginPanel_Disabled = true;
                break;
            default:
                this.DatabaseConfigurationPanel_Selected = true;
                this.InstallScriptsPanel_Disabled = true;
        }
    }

    validatePassword(paramPassword: string): boolean {
        // Validate that one of each required 
        // character is in the password
        var boolContainsNumber: boolean = false;
        var boolContainsNonLetter: boolean = false;
        var boolContainsUppercase: boolean = false;
        var boolIsEightChracters: boolean = false;

        let listOfNumbers = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
        let listOfNonLetters = ['~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '-', '+', '?', '<', '>', '[', ']', '{', '}', '|', ';'];
        let listOfUppercase = ['Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M'];

        for (let number of listOfNumbers) {
            if (paramPassword.indexOf(number.toString()) > -1) {
                boolContainsNumber = true;
            }
        }

        for (let nonLetter of listOfNonLetters) {
            if (paramPassword.indexOf(nonLetter) > -1) {
                boolContainsNonLetter = true;
            }
        }

        for (let upperCase of listOfUppercase) {
            if (paramPassword.indexOf(upperCase) > -1) {
                boolContainsUppercase = true;
            }
        }

        boolIsEightChracters = (paramPassword.length > 7);

        return (boolContainsNumber && boolContainsNonLetter && boolContainsUppercase && boolIsEightChracters);
    }

    validateEmail(paramEmail: string): boolean {
        // Validate email

        let boolIsFiveChracters: boolean = (paramEmail.length > 4);
        let boolContainsAmpersand: boolean = (paramEmail.indexOf("@") > -1);
        let boolContainsPeriod: boolean = (paramEmail.indexOf(".") > -1);

        return (boolIsFiveChracters && boolContainsAmpersand && boolContainsPeriod);
    }

    // Final

    ngOnDestroy(): void {
        // Important - Unsubscribe from any subscriptions
        if (this.LoginWizardSubscription !== undefined) {
            this.LoginWizardSubscription.unsubscribe();
        }

        if (this.SetUpSubscription !== undefined) {
            this.SetUpSubscription.unsubscribe();
        }
    }
}
