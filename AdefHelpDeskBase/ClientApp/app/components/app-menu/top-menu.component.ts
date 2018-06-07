import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';

import { MenubarModule, TabMenuModule, MenuItem } from 'primeng/primeng';

import { LoginService } from '../services/internal/login.service';
import { RegisterService } from '../services/internal/register.service';
import { MenuService } from '../services/internal/menu.service';
import { IUser } from '../classes/user';
import { UserService } from '../services/web/user.service';
import { DialogService } from '../services/internal/dialog.service';
import { ILoginParameter } from '../classes/loginParameter';

@Component({
    selector: 'top-menu',
    template: `
        <p-menubar [model]="MenuItems">        
        </p-menubar>
    `
})
export class TopMenuComponent implements OnInit, OnDestroy {
    LoginSubscription: Subscription;
    RegisterSubscription: Subscription;
    MenuItems: MenuItem[] = [];
    user: IUser;
    errorMessage: string;

    constructor(
        private _userService: UserService,
        private _MenuService: MenuService,
        private _loginService: LoginService,
        private _dialogService: DialogService,
        private _registerService: RegisterService) { }

    ngOnInit() {
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
    }

    getCurrentUser() {
        // Call the service
        this._userService.getCurrentUser().subscribe(
            user => {
                this.user = user;

                // Set Menu
                this.MenuItems = this._MenuService.getMenu(user);

            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    ngOnDestroy(): void {
        // Important - Unsubscribe from any subscriptions
        this.LoginSubscription.unsubscribe();
        this.RegisterSubscription.unsubscribe();
    }
}