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
import { IProfile } from '../classes/profile';
import { ILoginStatus } from '../classes/loginStatus';
import { IAuthentication } from '../classes/authentication';
import { UserService } from '../services/web/user.service';
import { RegisterService } from '../services/internal/register.service';
import { ProfileService } from '../services/internal/profile.service';
import { IProfileStatus } from '../classes/profileStatus';
import { DialogService } from '../services/internal/dialog.service';

@Component({
    selector: 'profile-dialog',
    templateUrl: './profile.component.html',
    styleUrls: ['./profile.component.css']
})

export class ProfileComponent implements OnInit, OnDestroy {
    ProfileSubscription: Subscription;
    showWaitGraphic: boolean = false;
    user: IUser;
    profile: IProfile;
    errorMessage: string;

    public profileForm = this.fb.group({
        firstname: ["", Validators.required],
        lastname: ["", Validators.required],
        email: ["", Validators.required],
        orginalpassword: [""],
        password: [""]
    });

    // Register the service
    constructor(
        private _userService: UserService,
        private _profileService: ProfileService,
        private _dialogService: DialogService,
        public fb: FormBuilder) {
    }

    ngOnInit(): void {

        // Subscribe to the ProfileSubscription Service
        this.ProfileSubscription = this._profileService.getVisbility().subscribe(
            (visibility: boolean) => {

                if (visibility == true) {
                    this.showWaitGraphic = true;
                    this._userService.getCurrentUser().subscribe(
                        user => {
                            this.user = user;
                       
                            this.profileForm.setValue({
                                firstname: user.firstName,
                                lastname: user.lastName,
                                email: user.email,
                                orginalpassword: '',
                                password: ''
                            });

                            this.showWaitGraphic = false;
                        },
                        error => {
                            this.errorMessage = <any>error;
                            this._dialogService.setMessage(this.errorMessage);
                        });
                }
            });
    }

    Update() {
        // Get the form values
        let formData = this.profileForm.value;
        let email = formData.email;
        let password = formData.password;
        let firstName = formData.firstname;
        let lastName = formData.lastname;
        let orginalpassword = formData.orginalpassword;

        let Profile: IProfile = {
            email: email,
            orginalpassword: orginalpassword,
            password: password,
            firstName: firstName,
            lastName: lastName
        };

        // Call the service
        this.showWaitGraphic = true;
        this._userService.updateUser(Profile).subscribe(
            ProfileStatus => {
                this.showWaitGraphic = false;

                // Was Profile update successful?
                if (!ProfileStatus.isSuccessful) {
                    this._dialogService.setMessage(ProfileStatus.status);
                } else {
                    // Close the Profile Dialog
                    this._profileService.setVisibility(false);
                }

            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    ngOnDestroy(): void {
        // Important - Unsubscribe from any subscriptions
        this.ProfileSubscription.unsubscribe();
    }
}