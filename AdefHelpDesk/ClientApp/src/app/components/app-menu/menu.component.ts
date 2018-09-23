import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, Route, ActivatedRouteSnapshot, UrlSegment, Params, Data } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';

import { IUser } from '../classes/user';
import { ITask } from '../classes/task';
import { UserService } from '../services/web/user.service';
import { LoginService } from '../services/internal/login.service';
import { RegisterService } from '../services/internal/register.service';
import { ProfileService } from '../services/internal/profile.service';
import { DialogService } from '../services/internal/dialog.service';
import { HTMLDialogService } from '../services/internal/htmldialog.service';
import { TaskVisibilityService } from '../services/internal/taskVisibility.service';
import { TaskService } from '../services/web/task.service';

import { IStatus } from '../classes/status';
import { IQueryStringParameter } from '../classes/querystringParameters';
import { InstallWizardService } from '../services/web/installWizard.service';
import { SetupService } from '../services/internal/setup.service';
import { QueryStringService } from '../services/internal/queryString.service';
import { SettingsService } from '../services/web/settings.service';
import { ILoginParameter } from '../classes/loginParameter';

@Component({
    selector: 'menu-component',
    templateUrl: './menu.component.html',
    styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit, AfterViewInit, OnDestroy {
    UserSubscription: Subscription;
    LoginSubscription: Subscription;
    RegisterSubscription: Subscription;
    ProfileSubscription: Subscription;
    DialogSubscription: Subscription;
    HTMLDialogSubscription: Subscription;
    SetUpSubscription: Subscription;
    TaskVisibilityService: Subscription;
    TaskDialogVisibilityService: Subscription;

    setUpMode: boolean = false;
    LoginDialogVisibility: boolean = false;
    RegisterDialogVisibility: boolean = false;
    ProfileDialogVisibility: boolean = false;
    DialogVisibility: boolean = false;
    HTMLDialogVisibility: boolean = false;
    displayEditTicketDialog: boolean = false;
    DialogMessage: string = '';
    TicketHeader: string = "";
    HTMLDialogMessage: string = '';
    user: IUser;
    errorMessage: string;

    // Register the services
    constructor(
        private _activatedRoute: ActivatedRoute,
        private _userService: UserService,
        private _loginService: LoginService,
        private _registerService: RegisterService,
        private _profileService: ProfileService,
        private _dialogService: DialogService,
        private _htmldialogService: HTMLDialogService,
        private _taskVisibilityService: TaskVisibilityService,
        private _taskService: TaskService,
        private _installWizardService: InstallWizardService,
        private _setupService: SetupService,
        private _queryStringService: QueryStringService,
        private _settingsService: SettingsService,
        private _router: Router
    ) {

    }

    ngOnInit() {

        //  ** Parameters

        // Put any Parameters in the service so that other components
        // will have access to the values
        this._activatedRoute.queryParams.subscribe((params: Params) => {
            let paramUsername = params['username'];
            let paramTicketNumber = params['ticketnumber'];
            let paramCode = params['code'];

            let paramIQueryStringParameter: IQueryStringParameter = {
                userName: paramUsername,
                ticketNumber: paramTicketNumber,
                code: paramCode
            }

            // Only set if we have a value
            if ((paramUsername != undefined) || (paramTicketNumber != undefined) || (paramCode != undefined)) {
                this._queryStringService.setQueryString(paramIQueryStringParameter);
            }
        });

        // ** Subscriptions

        // Subscribe to the UserSubscription Service
        this.UserSubscription = this._userService.getCurrentUser().subscribe(
            (user: IUser) => {
                this.user = user;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });

        // Subscribe to the LoginSubscription Service
        this.LoginSubscription = this._loginService.getLogin().subscribe(
            (loginParameter: ILoginParameter) => {

                this.LoginDialogVisibility = loginParameter.showLogin;

                if (this.LoginDialogVisibility == false) {
                    // If the Login Dialog is closed
                    // the user may be logged in
                    this.getCurrentUser();
                }
            });

        // Subscribe to the RegisterSubscription Service
        this.RegisterSubscription = this._registerService.getVisbility().subscribe(
            (visibility: boolean) => {
                this.RegisterDialogVisibility = visibility;

                if (this.RegisterDialogVisibility == false) {
                    // If the Register Dialog is closed
                    // the user may be logged in
                    this.getCurrentUser();
                }
            });

        // Subscribe to the ProfileSubscription Service
        this.ProfileSubscription = this._profileService.getVisbility().subscribe(
            (visibility: boolean) => {
                this.ProfileDialogVisibility = visibility;

                if (this.ProfileDialogVisibility == false) {
                    // If the Profile Dialog is closed
                    // update the user
                    this.getCurrentUser();
                }
            });

        // Subscribe to the DialogSubscription Service
        this.DialogSubscription = this._dialogService.getMessage().subscribe(
            (message: string) => {
                this.DialogMessage = message;
                if (this.DialogMessage !== '') {
                    this.DialogVisibility = true;
                }
            });

        // Subscribe to the HTMLDialogSubscription Service
        this.HTMLDialogSubscription = this._htmldialogService.getMessage().subscribe(
            (message: string) => {
                this.HTMLDialogMessage = message;
                if (this.HTMLDialogMessage !== '') {
                    this.HTMLDialogVisibility = true;
                }
            });

        // Subscribe to the SetUpSubscription Service
        this.SetUpSubscription = this._setupService.isSetupMode().subscribe(
            (isSetupMode: boolean) => {
                // SetUp mode causes the menu bar to dissapear
                // when true and reappear when false
                this.setUpMode = isSetupMode;
            });

        // ** Services 

        // Call the service to get the current database status
        this._installWizardService.getCurrentVersion().subscribe(
            version => {

                if (version.isUpToDate) {
                    // Everything is set up -- Show the application
                    this._router.navigateByUrl('/home');
                } else {
                    // Put the application in Setup mode
                    this._setupService.setSetupMode(true);

                    // Show the Install Wizard
                    this._router.navigateByUrl('/installwizard');
                }
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });

        // Call the service to determine if the Register button should show
        this._settingsService.getApplicationSettings().subscribe(
            settings => {

                // Set if Registration button should show
                this._registerService.setRegistrationEnabled(settings.allowRegistration);
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    ngAfterViewInit() {
        // Subscribe to the TaskVisibility Service
        this.TaskVisibilityService = this._taskVisibilityService.getTaskVisibility().subscribe(
            (paramTaskParameter: ITask) => {
                if (paramTaskParameter.taskId != undefined) {
                    // Open the Task Edit Dialog
                    this.displayEditTicketDialog = true;

                    this._taskService.getTaskByID(paramTaskParameter).subscribe(task => {
                        // Set TicketHeader
                        this.TicketHeader = "Edit Ticket #" + task.taskId.toString();
                    });
                }
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });

        this.TaskDialogVisibilityService = this._taskVisibilityService.getTaskDialogVisibility().subscribe(
            (paramTaskParameter: ITask) => {
                if (paramTaskParameter.taskId != undefined) {
                    // Close the Task Edit Dialog
                    this.displayEditTicketDialog = false;
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
        this.DialogSubscription.unsubscribe();
        this.HTMLDialogSubscription.unsubscribe();
        this.SetUpSubscription.unsubscribe();
        this.TaskVisibilityService.unsubscribe();
        this.TaskDialogVisibilityService.unsubscribe();
    }
}