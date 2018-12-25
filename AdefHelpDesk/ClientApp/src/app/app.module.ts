// Angular Core

import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { DatePipe } from '@angular/common'

// Components

import { AppComponent } from './components/app/app.component'
import { HomeComponent } from './components/home/home.component';
import { TicketsComponent } from './components/tickets/tickets.component';
import { TicketEditComponent } from './components/tickets/ticketEdit.component';
import { UserComponent } from './components/user/user.component';
import {
  AccordionModule,
  AutoCompleteModule,
  BreadcrumbModule,
  ButtonModule,
  CalendarModule,
  CarouselModule,
  ChartModule,
  CheckboxModule,
  ChipsModule,
  CodeHighlighterModule,
  ConfirmDialogModule,
  SharedModule,
  ContextMenuModule,
  DataGridModule,
  DataListModule,
  DataScrollerModule,
  DataTableModule,
  DialogModule,
  DragDropModule,
  DropdownModule,
  EditorModule,
  FieldsetModule,
  FileUploadModule,
  GalleriaModule,
  GMapModule,
  GrowlModule,
  InputMaskModule,
  InputSwitchModule,
  InputTextModule,
  InputTextareaModule,
  LightboxModule,
  ListboxModule,
  MegaMenuModule,
  MenuModule,
  MenubarModule,
  MessagesModule,
  MultiSelectModule,
  OrderListModule,
  OverlayPanelModule,
  PaginatorModule,
  PanelModule,
  PanelMenuModule,
  PasswordModule,
  PickListModule,
  ProgressBarModule,
  RadioButtonModule,
  RatingModule,
  ScheduleModule,
  SelectButtonModule,
  SlideMenuModule,
  SliderModule,
  SpinnerModule,
  SplitButtonModule,
  StepsModule,
  TabMenuModule,
  TabViewModule,
  TerminalModule,
  TieredMenuModule,
  ToggleButtonModule,
  ToolbarModule,
  TooltipModule,
  TreeModule,
  TreeTableModule
} from 'primeng/primeng';

import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { ProfileComponent } from './components/profile/profile.component';
import { ReportsComponent } from './components/reports/reports.component';
import { MenuComponent } from './components/app-menu/menu.component';
import { TopMenuComponent } from './components/app-menu/top-menu.component';
import { SideMenuComponent } from './components/app-menu/side-menu.component';
import { StatisticComponent } from './components/statistic/statistic.component';
import { InstallWizardComponent } from './components/installWizard/installWizard.component';
import { UsermanagerComponent } from './components/usermanager/usermanager.component';
import { UserassignmentrolesComponent } from './components/userassignmentroles/userassignmentroles.component';
import { TagsadminComponent } from './components/tagsadmin/tagsadmin.component';
import { EmailadminComponent } from './components/emailadmin/emailadmin.component';
import { ApplicationsettingsComponent } from './components/applicationsettings/applicationsettings.component';
import { SystemLogComponent } from './components/systemLogs/systemLogs.component';
import { FilesComponent } from './components/filemanager/files.component';
import { UpgradeComponent } from './components/upgrade/upgrade.component';
import { ApiSecurityComponent } from './components/api-security/api-security.component';

// Services

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
  declarations: [
    AppComponent,
    HomeComponent,
    TicketsComponent,
    TicketEditComponent,
    MenuComponent,
    TopMenuComponent,
    SideMenuComponent,
    StatisticComponent,
    UserComponent,
    LoginComponent,
    RegisterComponent,
    ProfileComponent,
    ReportsComponent,
    InstallWizardComponent,
    UsermanagerComponent,
    UserassignmentrolesComponent,
    TagsadminComponent,
    EmailadminComponent,
    ApplicationsettingsComponent,
    SystemLogComponent,
    FilesComponent,
    UpgradeComponent,
    ApiSecurityComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(
      [
      { path: '', redirectTo: 'installwizard', pathMatch: 'full' },
      { path: 'home', component: HomeComponent },
      { path: 'tickets', component: TicketsComponent },
      { path: 'reports', component: ReportsComponent },
      { path: 'installwizard', component: InstallWizardComponent },
      { path: 'usermanager', component: UsermanagerComponent },
      { path: 'userassignmentroles', component: UserassignmentrolesComponent },
      { path: 'tagsadmin', component: TagsadminComponent },
      { path: 'emailadmin', component: EmailadminComponent },
      { path: 'applicationsettings', component: ApplicationsettingsComponent },
      { path: 'filemanager', component: FilesComponent },
      { path: 'upgrade', component: UpgradeComponent },
      { path: 'systemlogs', component: SystemLogComponent },
      { path: 'apisecurity', component: ApiSecurityComponent },
      { path: '**', redirectTo: 'home' }
      ],
      { useHash: true }
    ),
    BrowserAnimationsModule,
    ChartModule,
    FormsModule,
    ReactiveFormsModule,
    AccordionModule,
    AutoCompleteModule,
    BreadcrumbModule,
    ButtonModule,
    CalendarModule,
    CarouselModule,
    ChartModule,
    CheckboxModule,
    ChipsModule,
    CodeHighlighterModule,
    ConfirmDialogModule,
    SharedModule,
    ContextMenuModule,
    DataGridModule,
    DataListModule,
    DataScrollerModule,
    DataTableModule,
    DialogModule,
    DragDropModule,
    DropdownModule,
    EditorModule,
    FieldsetModule,
    FileUploadModule,
    GalleriaModule,
    GMapModule,
    GrowlModule,
    InputMaskModule,
    InputSwitchModule,
    InputTextModule,
    InputTextareaModule,
    LightboxModule,
    ListboxModule,
    MegaMenuModule,
    MenuModule,
    MenubarModule,
    MessagesModule,
    MultiSelectModule,
    OrderListModule,
    OverlayPanelModule,
    PaginatorModule,
    PanelModule,
    PanelMenuModule,
    PasswordModule,
    PickListModule,
    ProgressBarModule,
    RadioButtonModule,
    RatingModule,
    ScheduleModule,
    SelectButtonModule,
    SlideMenuModule,
    SliderModule,
    SpinnerModule,
    SplitButtonModule,
    StepsModule,
    TabMenuModule,
    TabViewModule,
    TerminalModule,
    TieredMenuModule,
    ToggleButtonModule,
    ToolbarModule,
    TooltipModule,
    TreeModule,
    TreeTableModule
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
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
