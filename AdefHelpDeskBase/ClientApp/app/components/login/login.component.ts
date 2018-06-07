import { Component, OnInit, AfterViewInit, OnDestroy, Input } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, Validators } from '@angular/forms';

import { Subscription } from 'rxjs/Subscription';

import {
    DialogModule,
    InputTextModule,
    PasswordModule,
    ButtonModule
} from 'primeng/primeng';

import { IUser } from '../classes/user';
import { ITask } from '../classes/task'; 
import { IQueryStringParameter } from '../classes/querystringParameters';
import { IMigration } from '../classes/migration';
import { IVerification } from '../classes/verification';
import { ILoginStatus } from '../classes/loginStatus';
import { IAuthentication } from '../classes/authentication';

import { TaskVisibilityService } from '../services/internal/taskVisibility.service';
import { UserService } from '../services/web/user.service';
import { LoginService } from '../services/internal/login.service';
import { LoginWizardService } from '../services/internal/loginWizard.service';
import { QueryStringService } from '../services/internal/queryString.service';
import { DialogService } from '../services/internal/dialog.service';
import { ILoginParameter } from '../classes/loginParameter';

@Component({
    selector: 'login-dialog',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
})

export class LoginComponent implements OnInit, AfterViewInit, OnDestroy {
    UserSubscription: Subscription;
    LoginSubscription: Subscription;
    QueryStringSubscription: Subscription;

    showWaitGraphic: boolean = false;
    user: IUser;
    TaskId: number = -1;
    isMigrateMode: boolean = false;
    isVerificationMode: boolean = false;
    username: string = "";
    password: string = "";
    passwordNew: string = "";
    errorMessage: string;
    verificationCode: string;

    public loginForm = this.fbLogin.group({
        username: ["", Validators.required],
        password: ["", Validators.required]
    });

    public MigrateForm = this.fbMigrate.group({
        passwordNew: ["", Validators.required],
        password: ["", Validators.required]
    });

    public VerificationForm = this.fbVerification.group({
        verificationCode: ["", Validators.required]
    });

    // Register the service
    constructor(
        private _userService: UserService,
        private _loginService: LoginService,
        private _loginWizardService: LoginWizardService,
        private _queryStringService: QueryStringService,
        private _taskVisibilityService: TaskVisibilityService,
        private _router: Router,
        public fbLogin: FormBuilder,
        private _dialogService: DialogService,
        public fbMigrate: FormBuilder,
        public fbVerification: FormBuilder) {
    }

    ngOnInit(): void {
        // Subscribe to the User service
        this.UserSubscription = this._userService.getCurrentUser().subscribe(
            (user: IUser) => {
                this.user = user;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });

        this.showWaitGraphic = false;

        // Subscribe to the LoginSubscription Service
        this.LoginSubscription = this._loginService.getLogin().subscribe(
            (loginParameter: ILoginParameter) => {

                if (loginParameter.showLogin == true) {
                    // If the Login Dialog is true
                    // it means the Login button was pressed
                    // Set this form to the Login form
                    this.isMigrateMode = false;
                    this.isVerificationMode = false;
                }

                if (loginParameter.taskId != -1) {
                    // A TaskID was passed -- save it for post Login
                    this.TaskId = loginParameter.taskId;
                }
            });
    }

    ngAfterViewInit() {

        // Subscribe to the QueryStringSubscription Service
        this.QueryStringSubscription = this._queryStringService.getQueryString().subscribe(
            (paramQueryStringParameter: IQueryStringParameter) => {
                if (paramQueryStringParameter.userName != undefined) {
                    // Set the username
                    this.loginForm.setValue({ username: paramQueryStringParameter.userName, password: "" });
                }
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    logIn() {
        // Get the form values
        let formData = this.loginForm.value;
        this.username = formData.username;
        this.password = formData.password;

        let Authentication: IAuthentication = {
            userName: this.username,
            password: this.password
        };

        // Call the service
        this.showWaitGraphic = true;
        this._userService.loginUser(Authentication).subscribe(
            LoginStatus => {
                this.showWaitGraphic = false;

                // Was Login successful?
                if (!LoginStatus.isLoggedIn) {

                    switch (LoginStatus.status) {
                        case "Migrate": {
                            this.MigrateMode();
                            break;
                        }
                        case "Verify": {
                            this.VerificationMode();
                            break;
                        }
                        default: {
                            alert(LoginStatus.status);
                            break;
                        }
                    }

                } else {
                    // Close the Login Dialog
                    this._loginService.setLogin({ showLogin: false, taskId: -1 });
                    // Notifty Install Wizard that login was performed
                    this._loginWizardService.setLoggedIn(true);

                    if (this.TaskId > -1) {
                        // If there is a TaskId display it

                        let selectedTask: ITask = {
                            taskId: this.TaskId
                        }

                        this._taskVisibilityService.setTaskVisibility(selectedTask)
                    }
                }

            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    // Migration

    MigrateMode() {
        // Switch to Migrate Mode
        this.isMigrateMode = true;
    }

    MigratelogIn() {
        // Get the form values
        let formData = this.MigrateForm.value;
        this.password = formData.password;
        this.passwordNew = formData.passwordNew;

        let Migration: IMigration = {
            userName: this.username,
            password: this.password,
            passwordNew: this.passwordNew
        };

        // Call the service
        this.showWaitGraphic = true;
        this._userService.migrateUser(Migration).subscribe(
            LoginStatus => {
                this.showWaitGraphic = false;

                // Was Login successful?
                if (!LoginStatus.isLoggedIn) {
                    alert(LoginStatus.status);
                } else {
                    // Close the Login Dialog 
                    this._loginService.setLogin({ showLogin: false, taskId: -1 });
                    // Notify Install Wizard (if running)
                    this._loginWizardService.setLoggedIn(true);
                }

            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    // Verification

    VerificationMode() {
        // Switch to Verification Mode
        this.isVerificationMode = true;
    }

    VerificationlogIn() {
        // Get the form values
        let formData = this.VerificationForm.value;
        this.verificationCode = formData.verificationCode;

        let Verification: IVerification = {
            userName: this.username,
            password: this.password,
            verificationCode: this.verificationCode
        };

        // Call the service
        this.showWaitGraphic = true;
        this._userService.verifyUser(Verification).subscribe(
            LoginStatus => {
                this.showWaitGraphic = false;

                // Was Login successful?
                if (!LoginStatus.isLoggedIn) {
                    this._dialogService.setMessage(LoginStatus.status);
                } else {
                    // Close the Login Dialog
                    this._loginService.setLogin({ showLogin: false, taskId: -1 });
                }

            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    ngOnDestroy(): void {
        // Important - Unsubscribe from any subscriptions
        this.UserSubscription.unsubscribe();
        this.LoginSubscription.unsubscribe();
        this.QueryStringSubscription.unsubscribe();
    }
}