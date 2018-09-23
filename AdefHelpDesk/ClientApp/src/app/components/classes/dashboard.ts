/* Defines the Dashboard entity */
export interface IDashboard {
    tickets: number;
    users: number;
    tags: number;
    roles: number;
    colTicketStatus: IDTOTicketStatus[];
    colRoleAssignments: IDTORoleAssignments[];
}

export interface IDTOTicketStatus {
    id: number;
    name: string;
    ticketCount: number;
}

export interface IDTORoleAssignments {
    id: number;
    name: string;
    roleAssignments: number;
}