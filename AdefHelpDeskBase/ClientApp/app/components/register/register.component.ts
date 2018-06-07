import { Component, OnInit, OnDestroy, Input } from '@angular/core';
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
import { IRegister } from '../classes/register';
import { ILoginStatus } from '../classes/loginStatus';
import { IAuthentication } from '../classes/authentication';
import { UserService } from '../services/web/user.service';
import { RegisterService } from '../services/internal/register.service';
import { IRegisterStatus } from '../classes/registerStatus';
import { DialogService } from '../services/internal/dialog.service';

@Component({
    selector: 'register-dialog',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css']
})

export class RegisterComponent implements OnInit, OnDestroy {
    UserSubscription: Subscription;
    showWaitGraphic: boolean = false;
    user: IUser;
    register: IRegister;
    errorMessage: string;

    public registerForm = this.fb.group({
        username: ["", Validators.required],
        firstname: ["", Validators.required],
        lastname: ["", Validators.required],
        email: ["", Validators.required],
        password: ["", Validators.required]
    });

    // Register the service
    constructor(
        private _userService: UserService,
        private _registerService: RegisterService,
        private _dialogService: DialogService,
        public fb: FormBuilder) {
    }

    ngOnInit(): void {
        // Subscribe to the User service
        this.UserSubscription = this._userService.getCurrentUser().subscribe(
            user => {
                this.user = user;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });

        this.showWaitGraphic = false;
    }

    Register() {
        // Get the form values
        let formData = this.registerForm.value;
        let username = formData.username;
        let email = formData.email;
        let password = formData.password;
        let firstName = formData.firstname;
        let lastName = formData.lastname;

        let Register: IRegister = {
            userName: username,
            email: email,
            password: password,
            firstName: firstName,
            lastName: lastName
        };

        // Call the service
        this.showWaitGraphic = true;
        this._userService.registerUser(Register).subscribe(
            RegisterStatus => {
                this.showWaitGraphic = false;

                // Was Register successful?
                if (!RegisterStatus.isSuccessful) {
                    this._dialogService.setMessage(RegisterStatus.status);
                } else {
                    if (RegisterStatus.requiresVerification) {
                        // Display Verification message 
                        this._dialogService.setMessage(RegisterStatus.status);
                    }
                    // Close the Register Dialog
                    this._registerService.setVisibility(false);
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
    }
}