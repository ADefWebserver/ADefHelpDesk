import { Component, OnInit, OnDestroy } from '@angular/core';
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
import { ILoginStatus } from '../classes/loginStatus';
import { IAuthentication } from '../classes/authentication';
import { UserService } from '../services/web/user.service';
import { LoginService } from '../services/internal/login.service';
import { RegisterService } from '../services/internal/register.service';
import { QueryStringService } from '../services/internal/queryString.service';
import { ProfileService } from '../services/internal/profile.service';
import { SetupService } from '../services/internal/setup.service';
import { TaskVisibilityService } from '../services/internal/taskVisibility.service';
import { DialogService } from '../services/internal/dialog.service';
import { ILoginParameter } from '../classes/loginParameter';
import { IQueryStringParameter } from '../classes/querystringParameters';

@Component({
    selector: 'user-detail',
    templateUrl: './user.component.html',
    styleUrls: ['./user.component.css']
})

export class UserComponent implements OnInit, OnDestroy {
    pageTitle: string = 'Current User';

    UserSubscription: Subscription;
    LoginSubscription: Subscription;
    RegisterSubscription: Subscription;
    ProfileSubscription: Subscription;
    SetUpSubscription: Subscription;
    RegisterButtonVisibilitySubscription: Subscription;

    user: IUser;
    errorMessage: string;
    setUpMode: boolean = false;
    registerButtonVisibility: boolean = false;

    // Register the services
    constructor(
        private _userService: UserService,
        private _loginService: LoginService,
        private _registerService: RegisterService,
        private _profileService: ProfileService,
        private _setupService: SetupService,
        private _taskVisibilityService: TaskVisibilityService,
        private _queryStringService: QueryStringService,
        private _dialogService: DialogService,
        private _router: Router,
        public fb: FormBuilder) {
    }

    ngOnInit(): void {
        this.user = {
            userId: -1,
            userName: "[Not Logged In]",
            isLoggedIn: false,
            firstName: '',
            lastName: '',
            email: '',
            isSuperUser: false,
            password: "",
            riapassword: "",
            verificationCode: "",
            userRoles: []
        }

        // Subscribe to the UserSubscription Service
        this.UserSubscription = this._userService.getCurrentUser().subscribe(
            user => {
                this.user = user;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });

        // Subscribe to the LoginSubscription Service
        this.LoginSubscription = this._loginService.getLogin().subscribe(
            (loginParameter: ILoginParameter) => {
                if (loginParameter.showLogin == false) {
                    // If the Login Dialog is closed
                    // the user may be logged in
                    this.getCurrentUser();
                }
            });

        // Subscribe to the RegisterSubscription Service
        this.RegisterSubscription = this._registerService.getVisbility().subscribe(
            (visibility: boolean) => {
                if (visibility == false) {
                    // If the Register Dialog is closed
                    // the user may be logged in
                    this.getCurrentUser();
                }
            });

        // Subscribe to the ProfileSubscription Service
        this.ProfileSubscription = this._profileService.getVisbility().subscribe(
            (visibility: boolean) => {
                if (visibility == false) {
                    // If the Profile Dialog is closed
                    // update the user
                    this.getCurrentUser();
                }
            });

        // Subscribe to the SetUpSubscription Service
        this.SetUpSubscription = this._setupService.isSetupMode().subscribe(
            (isSetupMode: boolean) => {
                // SetUp mode causes the menu bar to dissapear
                // when true and reappear when false
                this.setUpMode = isSetupMode;

                // update the user
                this.getCurrentUser();
            });

        // Subscribe to the RegistrationEnabled Service
        this.RegisterButtonVisibilitySubscription = this._registerService.getRegistrationEnabled().subscribe(
            (enabled: boolean) => {
                // Show the Registration button
                this.registerButtonVisibility = enabled;
            });
    }

    getCurrentUser() {
        // Call the service
        this._userService.getCurrentUser().subscribe(
            user => {
                this.user = user;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    showLogIn() {
        // Cause the Login dialog to show
        this._loginService.setLogin({ showLogin: true, taskId: -1 });
    }

    showRegister() {
        // Cause the Login dialog to show
        this._registerService.setVisibility(true);
    }

    showProfile() {
        // Cause the Profile dialog to show
        this._profileService.setVisibility(true);
    }

    LogOut() {
        // Call the service
        this._userService.logOutUser().subscribe(
            user => {
                this.user = user;
                // Call the method to see who 
                // the server-side 
                // thinks is logged in
                this.getCurrentUser();

                // Cause any subscriptions to the Login dialog to trigger
                // This will cause the menu to reset
                this._loginService.setLogin({ showLogin: false, taskId: -1 });

                // Reset any QueryString value so it wont load
                // the last ticket on navigation to Home
                this._queryStringService.setQueryString({ userName: undefined, ticketNumber: undefined, code: undefined });

                // Navigate to main page
                this._router.navigateByUrl('/home');
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
        this.RegisterSubscription.unsubscribe();
        this.ProfileSubscription.unsubscribe();
        this.SetUpSubscription.unsubscribe();
        this.RegisterButtonVisibilitySubscription.unsubscribe();
    }
}