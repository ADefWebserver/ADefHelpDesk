import { Component, Input, OnInit, OnDestroy} from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';

import { ToolbarModule, TieredMenuModule, MenuItem } from 'primeng/primeng';
import { IUser } from '../classes/user';
import { UserService } from '../services/web/user.service';
import { LoginService } from '../services/internal/login.service';
import { RegisterService } from '../services/internal/register.service';
import { ProfileService } from '../services/internal/profile.service';
import { MenuService } from '../services/internal/menu.service';
import { DialogService } from '../services/internal/dialog.service';
import { ILoginParameter } from '../classes/loginParameter';

@Component({
    selector: 'side-menu',
    template: `
        <p-toolbar> 
        <div class="ui-toolbar-group-left">
        <button #btn type="button" pButton label="Menu" icon="fa fa-bars" 
         class="ui-button-secondary" (click)="openMenu(menu, $event)"></button>
        <p-tieredMenu #menu [model]="MenuItems" [popup]="true">
        </p-tieredMenu>  
        </div>
        </p-toolbar>
    `
})
export class SideMenuComponent implements OnInit, OnDestroy {
    LoginSubscription: Subscription;
    RegisterSubscription: Subscription;
    RegisterButtonVisibilitySubscription: Subscription;
    MenuItems: MenuItem[] = [];
    user: IUser;
    errorMessage: string;
    RegisterButtonVisibility: boolean = false;

    // Register the service
    constructor(
        private _userService: UserService,
        private _loginService: LoginService,
        private _registerService: RegisterService,
        private _profileService: ProfileService,
        private _dialogService: DialogService,
        private _router: Router,
        private _MenuService: MenuService
    ) { }

    ngOnInit(): void {

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

        // Subscribe to the RegistrationEnabled Service
        this.RegisterButtonVisibilitySubscription = this._registerService.getRegistrationEnabled().subscribe(
            (enabled: boolean) => {
                // Set the Registration button visibility on the menu
                this.RegisterButtonVisibility = enabled;
                // Get the current user and redraw the menu
                this.getCurrentUser();
            });
    }

    public openMenu(menu, event) {
        // Toggle the menu
        menu.toggle(event);
        // Get status of user to determine if the Login button
        // needs to be shown and what menu to show
        this.getCurrentUser();
    }

    getCurrentUser() {
        // Call the service
        this._userService.getCurrentUser().subscribe(
            user => {
                this.user = user;

                // Clear Menu
                this.MenuItems = [];
                let newMenu: MenuItem[] = [];

                // Build new Menu
                if (!this.user.isLoggedIn) {    
                    if (!this.RegisterButtonVisibility) {
                        // Do not show the Reistration button
                        newMenu = newMenu.concat(this.getLoginButton(), this._MenuService.getMenu(user));
                    } else {
                        // Show the Registration button
                        newMenu = newMenu.concat(this.getLoginButton(), this.getRegisterButton(), this._MenuService.getMenu(user));
                    }
                } else {
                    newMenu = newMenu.concat(this._MenuService.getMenu(user), this.getLoggedInMenu());   
                }          

                // Set Menu
                this.MenuItems = newMenu;

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
        // Cause the Register dialog to show
        this._registerService.setVisibility(true);
    }

    ngOnDestroy(): void {
        // Important - Unsubscribe from any subscriptions
        this.LoginSubscription.unsubscribe();
        this.RegisterSubscription.unsubscribe();
        this.RegisterButtonVisibilitySubscription.unsubscribe();
    }

    // Custom Menus

    getLoggedInMenu(): MenuItem[] {
        return [
            {
                label: 'Edit Profile',
                icon: 'fa fa-fw fa-id-card-o', command: (event) => {
                    // Cause the Profile dialog to show
                    this._profileService.setVisibility(true);
                }
            },
            {
                label: 'Logout',
                icon: 'fa fa-fw fa-sign-out', command: (event) => {
                    // Call the service
                    this._userService.logOutUser().subscribe(
                        user => {
                            this.user = user;
                            // Call the method to see who 
                            // the server-side 
                            // thinks is logged in
                            this.getCurrentUser();

                            // Cause any subscriptions to the Login dialog to trigger
                            // This will cause the User control for the full width
                            // mode to update to logoff mode
                            this._loginService.setLogin({ showLogin: false, taskId: -1 });

                            // Navigate to main page
                            this._router.navigateByUrl('/home');
                        },
                        error => {
                            this.errorMessage = <any>error;
                            this._dialogService.setMessage(this.errorMessage);
                        });
                }
            }
        ];
    }

    getLoginButton(): MenuItem[] {
        return [
            {
                label: 'Login',
                icon: 'fa fa-fw fa-sign-in', command: (event) => { 
                    this.showLogIn();
                }
            }
        ];
    }

    getRegisterButton(): MenuItem[] {
        return [
            {
                label: 'Register',
                icon: 'fa fa-fw fa-user-plus', command: (event) => {
                    this.showRegister();
                }
            }
        ];
    }
}