import { NgModule } from '@angular/core';
import { DatePipe } from '@angular/common'
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { sharedConfig } from './app.module.shared';
import { HttpClientModule } from '@angular/common/http'

import { LoginService } from './components/services/internal/login.service';
import { LoginWizardService } from './components/services/internal/loginWizard.service';
import { RegisterService } from './components/services/internal/register.service';
import { ProfileService } from './components/services/internal/profile.service';
import { DialogService } from './components/services/internal/dialog.service';
import { HTMLDialogService } from './components/services/internal/htmldialog.service';
import { MenuService } from './components/services/internal/menu.service';
import { SetupService } from './components/services/internal/setup.service';
import { QueryStringService } from './components/services/internal/queryString.service';

import { CategoryService } from './components/services/web/category.service';
import { DashboardService } from './components/services/web/dashboard.service';
import { InstallWizardService } from './components/services/web/installWizard.service';
import { RoleService } from './components/services/web/role.service';
import { SettingsService } from './components/services/web/settings.service';
import { SystemLogService } from './components/services/web/systemLog.service';
import { LogService } from './components/services/web/log.service';
import { UserService } from './components/services/web/user.service';
import { UserManagerService } from './components/services/web/userManager.service';
import { ApiSecurityService } from './components/services/web/apiSecurity.service';
import { FilesService } from './components/services/web/files.service';
import { TaskService } from './components/services/web/task.service';
import { TaskVisibilityService } from './components/services/internal/taskVisibility.service';

@NgModule({
    bootstrap: sharedConfig.bootstrap,
    declarations: sharedConfig.declarations,
    imports: [
        BrowserModule,
        FormsModule,
        HttpClientModule,
        ...sharedConfig.imports
    ],
    providers: [
        { provide: 'ORIGIN_URL', useValue: location.origin },
        LoginService,
        LoginWizardService,
        RegisterService,
        ProfileService,
        DialogService,
        HTMLDialogService,
        UserService,
        MenuService,
        InstallWizardService,
        SetupService,
        QueryStringService,
        DashboardService,
        CategoryService,
        SystemLogService,
        LogService,
        SettingsService,
        RoleService,
        UserManagerService,
        ApiSecurityService,
        FilesService,
        TaskService,
        TaskVisibilityService,
        DatePipe
    ]
})
export class AppModule {
}
