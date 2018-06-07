import { Component, OnInit, ViewChild } from '@angular/core';
import { UIChart } from "primeng/primeng";
import { Observable } from "rxjs/Observable";

const DEFAULT_COLORS = ['#3366CC', '#DC3912', '#FF9900', '#109618', '#990099',
    '#3B3EAC', '#0099C6', '#DD4477', '#66AA00', '#B82E2E',
    '#316395', '#994499', '#22AA99', '#AAAA11', '#6633CC',
    '#E67300', '#8B0707', '#329262', '#5574A6', '#3B3EAC']

import { DashboardService } from '../services/web/dashboard.service';
import { IDashboard } from '../classes/dashboard';
import { IDTOTicketStatus } from '../classes/dashboard';
import { IDTORoleAssignments } from '../classes/dashboard';

@Component({
    selector: 'reports',
    templateUrl: './reports.component.html'
})
export class ReportsComponent {

    public Tickets: number = 0;
    public Users: number = 0;
    public Tags: number = 0;
    public Roles: number = 0;
    public ticketStatus: IDTOTicketStatus[] = [];
    public rolesAssigned: IDTORoleAssignments[] = [];

    // Tickets

    public chartOptionsTicketStatus = {
        title: {
            display: false,
            text: ''
        },
        legend: {
            position: 'bottom'
        },
    };

    public ticketStatusPieLabels: string[] = [];
    public ticketStatusPieData: number[] = [];
    public ticketStatusPieColors: string[] = [];

    public ticketStatusChartData = {
        labels: this.ticketStatusPieLabels,
        datasets: [
            {
                data: this.ticketStatusPieData,
                backgroundColor: this.ticketStatusPieColors
            }
        ]
    }

    // Role Assignments

    public chartOptionsRoleAssigned = {
        title: {
            display: false,
            text: ''
        },
        legend: {
            position: 'bottom'
        },
    };

    public roleAssignmentsPieLabels: string[] = [];
    public roleAssignmentsPieData: number[] = [];
    public roleAssignmentsPieColors: string[] = [];

    public roleAssignmentsChartData = {
        labels: this.roleAssignmentsPieLabels,
        datasets: [
            {
                data: this.roleAssignmentsPieData,
                backgroundColor: this.roleAssignmentsPieColors
            }
        ]
    }

    // Contructor is called when the class is created
    constructor(private _DashboardService: DashboardService) { }

    ngOnInit() {
        // Call the service
        this._DashboardService.getDashboard().subscribe((dashboard: IDashboard) => {

            this.Tickets = dashboard.tickets;
            this.Users = dashboard.users;
            this.Tags = dashboard.tags;
            this.Roles = dashboard.roles;

            this.ticketStatus = dashboard.colTicketStatus;
            this.rolesAssigned = dashboard.colRoleAssignments;

            // Map ticketStatus Data
            this.ticketStatusPieLabels = this.ticketStatus.map((proj) => proj.name);
            this.ticketStatusPieData = this.ticketStatus.map((proj) => proj.ticketCount);
            this.ticketStatusPieColors = this.configureDefaultColours(this.ticketStatusPieData);

            this.ticketStatusChartData = {
                labels: this.ticketStatusPieLabels,
                datasets: [
                    {
                        data: this.ticketStatusPieData,
                        backgroundColor: this.ticketStatusPieColors
                    }
                ]
            }

            // Map rolesAssigned Data
            this.roleAssignmentsPieLabels = this.rolesAssigned.map((proj) => proj.name);
            this.roleAssignmentsPieData = this.rolesAssigned.map((proj) => proj.roleAssignments);
            this.roleAssignmentsPieColors = this.configureDefaultColours(this.roleAssignmentsPieData);

            this.roleAssignmentsChartData = {
                labels: this.roleAssignmentsPieLabels,
                datasets: [
                    {
                        data: this.roleAssignmentsPieData,
                        backgroundColor: this.roleAssignmentsPieColors
                    }
                ]
            }
        });
    }

    // Utility

    private configureDefaultColours(data: number[]): string[] {
        let customColours = []
        if (data.length) {

            customColours = data.map((element, idx) => {
                return DEFAULT_COLORS[idx % DEFAULT_COLORS.length];
            });
        }

        return customColours;
    }
}